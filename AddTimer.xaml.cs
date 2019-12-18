using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Twitch
{
    /// <summary>
    /// Interaction logic for AddTimer.xaml
    /// </summary>
    public partial class AddTimer : Window
    {
        private CommandGrid.CmdGrid grid;
        public Command timercomm;

        /// <summary>
        /// The constructor for AddTimer form.
        /// </summary>
        /// <param name="g">the reference to the timers command grid</param>
        public AddTimer(CommandGrid.CmdGrid g)
        {
            InitializeComponent();
            permissionComboBox.Items.Add("everyone");
            permissionComboBox.Items.Add("mods+");
            grid = g;
        }

        /// <summary>
        /// Moves the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Move_Form(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Released)
                this.DragMove();
        }

        /// <summary>
        /// Sets the backgrounds color. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("pack://application:,,,/Resources/background.png");
            bitmap.EndInit();
            backlogin.Source = bitmap;
        }

        /// <summary>
        /// Event for when the submit button is clicked. 
        /// Converts timer form into a timer(command) object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (titleTextBox.Text == "" || permissionComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please enter some text!");
                return;
            }
            else
            {
                if ((hoursTextBox.Text == "" || minutesTextBox.Text == "" || secondsTextBox.Text == "") && !(countupCheckBox.IsChecked ?? false))
                {
                    MessageBox.Show("Please enter some text!");
                    return;
                }

                if (!titleTextBox.Text.StartsWith("!"))
                {
                    MessageBox.Show("Make sure the title of your command starts with a '!', like this: !example");
                    return;
                }
                titleTextBox.Text.Replace(" ", "");

                if (countupCheckBox.IsChecked ?? false)
                {
                    // this timer is simply counting up, like a stopwatch

                    timercomm = new Command(titleTextBox.Text,                          // title of timer 
                                            "Timer is at: {timer}",                     // output of timer
                                            permissionComboBox.SelectedItem.ToString(), // who can use the command
                                            new TimeSpan(0, 0, 0),                      // cooldown is 0
                                            DateTime.Now);                              // the start of the stopwatch timer
                }
                else
                {
                    // parsing for hours, minutes, seconds that timer has to last

                    int hours;
                    if (!int.TryParse(hoursTextBox.Text, out hours))
                    {
                        MessageBox.Show("Hours has to use numbers");
                        return;
                    }

                    int minutes;
                    if (!int.TryParse(minutesTextBox.Text, out minutes))
                    {
                        MessageBox.Show("Minutes has to use numbers");
                        return;
                    }

                    int seconds;
                    if (!int.TryParse(secondsTextBox.Text, out seconds))
                    {
                        MessageBox.Show("Seconds has to use numbers");
                        return;
                    }

                    timercomm = new Command(titleTextBox.Text,                           // title of timer 
                                            "Time left: {timer}",                        // output of timer
                                            permissionComboBox.SelectedItem.ToString(),  // who can use the command
                                            new TimeSpan(0,0,0),                         // cooldown is 0
                                            new TimeSpan(hours, minutes, seconds),       // how long the timer should last / countdown
                                            DateTime.Now);                               // the start of the timer
                    //MessageBox.Show(new TimeSpan(hours, minutes, seconds).ToString());
                }

                grid.AddCommand(timercomm);

                this.Close();
            }
        }

        /// <summary>
        /// Event for when the "countup/stopwatch" is checked. Sets textboxes to ReadOnly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void countupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            hoursTextBox.Text = "";
            hoursTextBox.IsReadOnly = true;

            minutesTextBox.Text = "";
            minutesTextBox.IsReadOnly = true;

            secondsTextBox.Text = "";
            secondsTextBox.IsReadOnly = true;
        }

        /// <summary>
        /// Event for when the "countup/stopwatch" is unchecked. 
        /// Sets textboxes ReadOnly property to false. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void countupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            hoursTextBox.IsReadOnly = false;

            minutesTextBox.IsReadOnly = false;

            secondsTextBox.IsReadOnly = false;
        }
    }
}
