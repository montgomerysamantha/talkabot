using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Navigation;

namespace Twitch
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        TwitchIRC twitch;
        TalkaBot talk = null;
        ConfigObject Cache = null;
        ConfigManager smg = new ConfigManager();
        public Login()
        {
            InitializeComponent();

            tokenTextBox.PasswordChar = '*';

            if (File.Exists("Config.xml"))
            {
                Cache = smg.LoadConfig();

                UsernameTextBox.Text = Cache.username;
                tokenTextBox.Password = Cache.oauth;
                ChannelTextBox.Text = Cache.channel;
            }
        }

        public void Logout()
        {
            this.Show();
            talk.Hide();
            twitch.exited = true;
            twitch.tokenSource.Cancel();
            talk = null;
        }

        /// <summary>
        /// Login and connect to Twitch IRC channel with the given bot account.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("pack://application:,,,/Resources/background.png");
            bitmap.EndInit();
            backlogin.Source = bitmap;
        }

        private void FindOauth_Navigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void OnReceiveMsg(object sender, TwitchData eData)
        {
            //MessageBox.Show("I have received the event!");
            //MessageBox.Show("Message: " + eData.message, eData.username);
        }

        private void Move_Form(object sender, MouseButtonEventArgs e)
        {
            if(e.RightButton == MouseButtonState.Released)
                this.DragMove();
        }

        private void Log_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("microsoft-edge:https://twitchapps.com/tmi/");
        }

        private void EnterTokenButton_Click(object sender, RoutedEventArgs e)
        {
            if(!(string.IsNullOrEmpty(tokenTextBox.Password) || string.IsNullOrEmpty(UsernameTextBox.Text) || string.IsNullOrEmpty(ChannelTextBox.Text)))
            {
                string token = tokenTextBox.Password;
                string username = UsernameTextBox.Text;
                string channel = ChannelTextBox.Text;

                twitch = new TwitchIRC(username, token, channel);
                if (!twitch.Connect())
                {
                    System.Windows.Forms.MessageBox.Show("An error occured while connecting to Twitch.");
                    tokenTextBox.Password = "";
                    UsernameTextBox.Text = "";
                    ChannelTextBox.Text = "";
                }
                else
                {
                    talk = new TalkaBot(ChannelTextBox.Text, this, twitch);
                    talk.Show();
                    this.Hide();
                    List<Command> commands = new List<Command>();
                    smg.SaveConfig(commands, username, token, channel);
                }
            }
            else
            {
                MessageBox.Show("Please fill the fields");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
