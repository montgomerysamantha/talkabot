using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Twitch
{
    public class TwitchIRC
    {
        private string username, oauth, channel;
        private TcpClient twitchAdapter;
        private StreamReader twitchReader;
        private Logger logger = new Logger();
        private StreamWriter twitchWriter;
        private TalkaBot MainWindowHandle; // reference to the MainWindow to update it

        //Event called everytime a message is received and treated!
        private EventHandler<TwitchData> OnReceiveMsg = null;

        Logger logs = new Logger();

        // stores commmands in key value pairs like so: commandsList["!command"] = Command object
        public Dictionary<string, Command> commandsList = new Dictionary<string, Command>();
        private string messageTemplate;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public bool exited = false;
        public CancellationTokenSource tokenSource;

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="twitchUsername">Username for the moderating account</param>
        /// <param name="oauthToken">Authentification token for the moderating account</param>
        /// <param name="twitchChannel">Name of the channel to be joined for the bot</param>
        public TwitchIRC(string twitchUsername, string oauthToken, string twitchChannel)
        {
            username = twitchUsername.ToLower();
            oauth = oauthToken;
            channel = twitchChannel.ToLower();
            messageTemplate = ":" + username + "!" + username + "@" + username + ".tmi.twitch.tv PRIVMSG #" + channel + " :";
        }
         
        /// <summary>
        /// Connects the bot to the selected Twitch IRC channel,
        /// then calls MessageHandler().
        /// </summary>
        /// <returns>true on successful connection, false if socket error happens</returns>
        public bool Connect()
        {
            try
            {
                //Connects to twitch and initialize a SteamWriter object to send messages to the irc server
                twitchAdapter = new TcpClient("irc.twitch.tv", 6667);
                twitchWriter = new StreamWriter(twitchAdapter.GetStream());


                //Authentificate the user to the irc server and joins the channel
                twitchWriter.WriteLine("PASS " + oauth + Environment.NewLine
                + "NICK " + username + Environment.NewLine
                + "USER " + username + " 8 * :" + username);
                twitchWriter.WriteLine("JOIN #" + channel);

                //Request aditional tags for moderation
                twitchWriter.WriteLine("CAP REQ :twitch.tv/membership");
                twitchWriter.WriteLine("CAP REQ :twitch.tv/tags");
                twitchWriter.Flush();

                //Checks the 2 first messages to see if twitch accepted the credentials
                for (int i = 0; i < 2; i++)
                {
                    twitchReader = new StreamReader(twitchAdapter.GetStream());
                    if (twitchAdapter.Available > 0 || twitchReader.Peek() >= 0)
                    {
                        string message = twitchReader.ReadLine();

                        if (message == ":tmi.twitch.tv NOTICE * :Login authentication failed")
                        {
                            return false;
                        }
                        else if (message == ":tmi.twitch.tv NOTICE * :Improperly formatted auth")
                        {
                            return false;
                        }
                    }
                }
                twitchReader = null;

                int num = 1;
                tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;

                Task.Run(() => MessageHandler(num, token), token);
            }
            catch (SocketException e)
            {
                twitchAdapter = null;
                twitchWriter = null;
                throw (e);
            }

            return true;
        }

        /// <summary>
        /// Takes the full message from Twitch and splits it using ';' as a delimiter.
        /// From there, it iterates through all the pieces and further splits them
        /// using '='. It then stores them in the filteredData dictionary.
        /// </summary>
        /// <param name="message">The full message from Twitch to split and put into the dictionary</param>
        /// <returns>A dictionary with key value pairs containing helpful information like whether a user
        /// is a subscriber, mod, Turbo user, etc.</returns>
        private Dictionary<string, string> FilterTags(string message)
        {
            Dictionary<string, string> tags = new Dictionary<string, string>();
            string[] tagsSplit = message.Split(';');

            int pos = message.IndexOf("user-type");
            string[] msg = message.Substring(pos, message.Length - pos).Split('=');
            tags.Add(msg[0], FilterMessage(msg[1]));

            foreach (string keyValue in tagsSplit)
            {
                string[] temp = keyValue.Split('=');

                if (temp[0] == "user-type")
                    break;
                if (temp.Length > 1)
                {
                    tags.Add(temp[0], temp[1]);
                }
            }
            return tags;
        }

        /// <summary>
        /// Splits up the full tagged message from Twitch to just get the message part.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string FilterMessage(string value)
        {
            char[] temp = value.ToCharArray();

            int count = 0;
            for(int i = 0; i < temp.Length; i++)
            {
                if(count == 2)
                {
                    count = i;
                    break;
                }
                if (temp[i] == ':')
                    count++;
            }

            return value.Substring(count, value.Length - count);
        }

        /// <summary>
        /// Breaks up the full message from Twitch and splits it into pieces (in FilterTags())
        /// and places each piece into its respective TwitchData properties
        /// </summary>
        /// <param name="message"></param>
        /// <returns>returns TwitchData object if successfully mapped, null otherewise</returns>
        private TwitchData MapMessageToObject(ref string message)
        {
            TwitchData twitchObj = new TwitchData();

            Dictionary<string, string> tags = FilterTags(message);

            try
            {
                twitchObj.message = FilterMessage(tags["user-type"]);

                twitchObj.messageId = tags["id"];
                twitchObj.username = tags["display-name"];
                twitchObj.tags.color = tags["color"];

                twitchObj.tags.isMod = (tags["mod"] == "1") ? true : false;
                string b = tags["badges"];
                if (b.Contains("broadcaster/1")) twitchObj.tags.isMod = true; // special case to make broadcasters count as moderators

                twitchObj.tags.isTurbo = (tags["turbo"] == "1") ? true : false;
                twitchObj.tags.isSubscriber = (tags["subscriber"] == "1") ? true : false;

                twitchObj.tags.userId = tags["user-id"];
            }
            catch(Exception e)
            {
                logger.PushError($"[{e.Message}] Could not map message to object :( {message}");
            }

            return twitchObj;
        }

        /// <summary>
        /// Invoker will format the message into a data struct object and will pass 
        /// it to the function that was set for the EventHandler OnReceiveMsg.
        /// </summary>
        /// <param name="objArray">The message to pass convert into a TwitchData message object</param>
        private void Invoker(object objArray)
        {
            object[] array = objArray as object[];
            CancellationToken token = (CancellationToken)array[1];

            if (token.IsCancellationRequested)
            {
                return;
            }

            string message = array[0].ToString();
            TwitchData data = MapMessageToObject(ref message);

            OnReceiveMsg?.Invoke(this, data);

            if (data != null && data.message != null)
            {
                DetectCommand(data);
            }
        }

        /// <summary>
        /// Recieves all messages for the given channel and responds to Twitch's PING/PONG
        /// requests when required. Verify that messages are available and if so then it passes
        /// the message to the invoker function
        /// </summary>
        /// <param name="taskNum">The number of the task</param>
        /// <param name="ct">A reference to the CancellationToken that we can check</param>
        private void MessageHandler(int taskNum, CancellationToken ct)
        {
            object[] objectArray = new object[2];

            while (true)
            {

                if (!twitchAdapter.Connected)
                    Connect();
                twitchReader = new StreamReader(twitchAdapter.GetStream());
                if (twitchAdapter.Available > 0 || twitchReader.Peek() >= 0)
                {
                    string message = twitchReader.ReadLine();
                    objectArray[0] = message;
                    if (!message.StartsWith("PING") && message.Length >= 130)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }
                        objectArray[1] = cts.Token;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(Invoker), objectArray);
                    }
                    else
                    {
                        twitchWriter.WriteLine("PONG");
                        twitchWriter.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Will call the function passed as the argument everytime a message is received and filtered
        /// </summary>
        /// <param name="function">Function to call when a message is recieved</param>
        public void SetMessageEvent(EventHandler<TwitchData> function)
        {
            OnReceiveMsg = function;
        }

        /// <summary>
        /// Replaces {user} with the user's name!
        /// </summary>
        /// <param name="output">string to modify</param>
        /// <param name="data">information about Twitch message and user</param>
        /// <returns></returns>
        private string SwapVariables(string output, TwitchData data)
        {
            output = output.Replace("{user}", data.username);

            return output;
        }

        /// <summary>
        /// Replaces {timer} with a nicely formatted date.
        /// </summary>
        /// <param name="output">string to modify</param>
        /// <param name="remain">the TimeSpan to convert into pretty format</param>
        /// <returns></returns>
        private string SwapTimer(string output, TimeSpan remain)
        {
            string time = remain.ToString("d'd 'h'h 'm'm 's's'");
            output = output.Replace("{timer}", time);

            return output;
        }

        /// <summary>
        /// This function looks at a message sent in chat to see if it is a command or not.
        /// </summary>
        /// <param name="data">The data to be analyzed</param>
        public void DetectCommand(TwitchData data)
        {
            string text = data.message;
            string command; // where the actual command is stored

            if (text.StartsWith("!"))
            {
                string[] pieces = text.Split(' '); // if message is like "!command extra text we don't need" then split it
                command = pieces[0]; // "!command" goes here
                CommandHandler(command, data); // call the command handler to do something
            }
        }

        /// <summary>
        /// This function will lookup a detected command to see if it exists in the commandsList
        /// </summary>
        /// <param name="command">The command to try and lookup</param>
        /// <param name="data">The full message from Twitch, with info about the user</param>
        public void CommandHandler(string command, TwitchData data)
        {
            if (commandsList.ContainsKey(command)) // check that this is an actual command
            {
                Command comm = commandsList[command]; // save the command object
                string output = SwapVariables(comm.output, data);


                DateTime curTime = DateTime.Now;
                TimeSpan elapsedTime = curTime.Subtract(comm.timeLastUsed); // time difference 
                if (elapsedTime > comm.cooldown) // if enough time has passed since the command was last used
                {
                    DateTime date = new DateTime(1979, 07, 28, 22, 35, 5); // the default date
                    if (comm.timerlength != TimeSpan.Zero && !comm.timerstart.Equals(date))
                    {
                        // this is a count down timer

                        TimeSpan elaspedTimeTimer = curTime.Subtract(comm.timerstart);

                        if (elaspedTimeTimer > comm.timerlength)
                        {
                            // timer has expired
                            TimeSpan expire = elaspedTimeTimer.Subtract(comm.timerlength);
                            output = SwapTimer("Timer expired {timer} ago", expire);
                        }
                        else
                        {
                            // timer still going
                            TimeSpan remain = comm.timerlength.Subtract(elaspedTimeTimer);
                            output = SwapTimer(comm.output, remain);
                        }
                    }
                    else if (!comm.timerstart.Equals(date))
                    {
                        // this is a count up timer

                        TimeSpan elaspedTimeTimer = curTime.Subtract(comm.timerstart);

                        output = SwapTimer(comm.output, elaspedTimeTimer);
                    }

                    switch (comm.permission)
                    {
                        case "mods+":
                            {
                                if (!data.tags.isMod)
                                {
                                    WriteBotMessage("Sorry, only moderators and above can use that command!");
                                }
                                else
                                {
                                    WriteBotMessage(output);
                                }

                                break;
                            }
                        case "everyone":
                            {
                                WriteBotMessage(output);
                                break;
                            }
                    }

                    comm.timeLastUsed = curTime; // update the timeLastUsed
                }
                else
                {
                    WriteBotMessage("That command is on cooldown :(");
                }
            }
            else
            {
                //WriteBotMessage("Sorry, command not found!");
            }
        }


        /// <summary>
        /// Deletes a command from the commandsList.
        /// </summary>
        /// <param name="command">This is the command to be deleted</param>
        public void DeleteCommandFromList(string command)
        {
            if (commandsList.ContainsKey(command))
            {
                Command del = commandsList[command];
                commandsList.Remove(command);
            }
            else
            {
                //MessageBox.Show("Command does not exist!");
            }
        }

        /// <summary>
        /// Modifys a command to the commandsList.
        /// </summary>
        /// <param name="command">This is the command to be modified</param>
        /// <param name="output">The output of the command to be modified</param>
        public void ModifyCommand(string command, string output)
        {
            if (commandsList.ContainsKey(command))
            {
                commandsList[command] = new Command(command, output, "everyone");
            }
            else
            {
                MessageBox.Show("Command does not exist! Cannot modify!");
            }
        }

        /// <summary>
        /// This writes a message to the selected Twitch channel using the bot using Twitch's message template.
        /// </summary>
        /// <param name="message">The message text to write</param>
        public void WriteBotMessage(string message)
        {
            twitchWriter.WriteLine(messageTemplate + message);
            twitchWriter.Flush();
            TwitchData td = new TwitchData(username, message, "#d703fc");
            OnReceiveMsg?.Invoke(this, td);
            //Application.Current.Dispatcher.BeginInvoke((Action<TwitchData>)MainWindowHandle.UpdateChat, td);
        }

        public void AddMainWindow(TalkaBot m)
        {
            MainWindowHandle = m;
        }

        public void SaveConfigRequest()
        {
            //config.SaveConfig(commandsList, username, oauth, channel);
            return;
        }

        public void Shutdown()
        {
            cts.Cancel();
            return;
        }

        public bool isCommand(Command c)
        {
            if (commandsList.ContainsKey(c.title))
            {
                return true;
            }
            return false;
        }
    }
}
