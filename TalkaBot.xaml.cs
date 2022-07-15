using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Twitch
{
    /// <summary>
    /// Interaction logic for TalkaBot.xaml
    /// </summary>
    public partial class TalkaBot : Window
    {
        Login parent;
        WebChat chat = new WebChat();
        private int lines = 0;
        //private Dictionary<Command, Canvas> visibleCommands;
        private Grid lastPanel = null;
        public TwitchIRC connection;

        /// <summary>
        /// Constructor for the Talkabot, aka the main form.
        /// </summary>
        /// <param name="channel">channel we are connected to</param>
        /// <param name="parents">the login info (user and oauth)</param>
        /// <param name="twitch">Twitch connection information / link</param>
        public TalkaBot(string channel, Login parents, TwitchIRC twitch)
        {
            InitializeComponent();
            //visibleCommands = new Dictionary<Command, Canvas>();
            connection = twitch;
            twitch.AddMainWindow(this);
            commands.AddMainWindow(this);
            timers.AddMainWindow(this);
            parent = parents;
            lastPanel = Commands;
            connection.SetMessageEvent(OnReceiveMsg);
            commands.SetModifyEvent(ModifyEvent);
            this.Title = "TalkABot Channel: " + channel;
            LoadCommands();
            timers.commandNameTextBlock.Text = "Timer";
        }

        /// <summary>
        /// Takes the original command out of the commandsList and adds the modified one.
        /// </summary>
        /// <param name="original">original command</param>
        /// <param name="cmd">modified/edited command</param>
        private void ModifyEvent(Command original, Command cmd)
        {
            if (connection.commandsList.ContainsKey(original.title))
            {
                connection.commandsList.Remove(original.title); // delete original
                connection.commandsList.Add(cmd.title, cmd); // add new command
            }
        }

        /// <summary>
        /// Loads the commands and timers onto the proper CommandGrid.
        /// </summary>
        private void LoadCommands()
        {
            ConfigObject obj = ConfigManager.LoadConfig();

            DateTime date = new DateTime(1979, 07, 28, 22, 35, 5); // the default date

            foreach (Command cmd in obj.commandList)
            {
                if (cmd.timerlength != TimeSpan.Zero || !cmd.timerstart.Equals(date))
                {
                    // this is a timer command
                    timers.AddCommand(cmd);
                }
                else
                {
                    // this is a command command
                    commands.AddCommand(cmd);
                }

            }

            ConfigManager.SaveConfig(obj.commandList, obj.username, obj.oauth, obj.channel);
        }

        /// <summary>
        /// This is called from the CommandGrid class to add a command to the list.
        /// </summary>
        /// <param name="c">Command to add</param>
        public void RecieveNewCommand(Command c)
        {
            if (!connection.commandsList.ContainsKey(c.title))
            {
                connection.commandsList.Add(c.title, c);
            }
        }

        /// <summary>
        /// This is called from the CommandGrid class to delete a command from the list.
        /// </summary>
        /// <param name="c">Command to delete from list</param>
        public void RecieveCommandToDelete(Command c)
        {
            if (connection.commandsList.ContainsKey(c.title))
            {
                connection.commandsList.Remove(c.title);
            }
        }

        /// <summary>
        /// Updates the RichTextBox (TChatTextBox) with a colorful message from someone on Twitch.
        /// </summary>
        /// <param name="data">The TwitchData to process and format</param>
        public void UpdateChat(TwitchData data)
        {
            chat.PushMessage(data.username, data.message);
            if (lines >= 100)
            {
                TChatTextBox.Document.Blocks.Clear();
                lines = 0;
            }

            if (data.tags.color != "")
            {
                // if the user has a color already selected
                TextRange tr = new TextRange(TChatTextBox.Document.ContentEnd, TChatTextBox.Document.ContentEnd);
                tr.Text = data.username;
                SolidColorBrush s = (SolidColorBrush)(new BrushConverter().ConvertFrom(data.tags.color));
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, s);
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                TextRange selection = new TextRange(TChatTextBox.Document.ContentEnd, TChatTextBox.Document.ContentEnd);
                selection.Text = ": "  + data.message + Environment.NewLine;
                selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGray);
            }
            else
            {
                // no color selected, so use default powder blue color 
                TextRange tr = new TextRange(TChatTextBox.Document.ContentEnd, TChatTextBox.Document.ContentEnd);
                tr.Text = data.username + ": ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.PowderBlue);
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                TextRange selection = new TextRange(TChatTextBox.Document.ContentEnd, TChatTextBox.Document.ContentEnd);
                selection.Text = ": " + data.message + Environment.NewLine;
                selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGray);
            }


            TChatTextBox.ScrollToEnd();

            lines++;

            return;
        }

        /// <summary>
        /// Sends a dispatcher to update the chat, fixing weird UI threading issues.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void OnReceiveMsg(object sender, TwitchData data)
        {
            Application.Current.Dispatcher.BeginInvoke((Action<TwitchData>)UpdateChat, data);
        }

        /// <summary>
        /// Saves the commands to a .xml file which is loaded automatically when the
        /// application next starts.
        /// </summary>
        private void SaveCommands() 
        {
            ConfigObject obj = ConfigManager.LoadConfig();
            List<Command> commands = new List<Command>();
            foreach (KeyValuePair<string, Command> i in connection.commandsList)
            {
                commands.Add(i.Value);
            }
            ConfigManager.SaveConfig(commands, obj.username, obj.oauth, obj.channel);
        }

        /// <summary>
        /// Saves the commands and then shuts down the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveCommands();
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// When the timers text is clicked, switch to the timer CommandGrid panel in UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Click(object sender, RoutedEventArgs e)
        {
            lastPanel.Visibility = Visibility.Collapsed;
            lastPanel = Timers;
            Timers.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// When the commands text is clicked, switch to the commands CommandGrid panel in UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Commands_Click(object sender, RoutedEventArgs e)
        {
            lastPanel.Visibility = Visibility.Collapsed;
            lastPanel = Commands;
            Commands.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Saves commands and logs out of the application, returns to login screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            SaveCommands();
            parent.Logout();
        }

        /// <summary>
        /// Runs the HTML & javascript websource for OBS studio.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunWeb_Click(object sender, RoutedEventArgs e)
        {
            chat.Run();
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + "WebChat\\ui.html");
        }
    }
}
