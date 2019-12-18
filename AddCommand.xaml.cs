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
    /// Interaction logic for AddCommand.xaml
    /// </summary>
    public partial class AddCommand : Window
    {
        private CommandGrid.CmdGrid grid;
        public Command comm;

        /// <summary>
        /// The constructor for the Add Command form. 
        /// </summary>
        /// <param name="g">Reference to the main command grid</param>
        public AddCommand(CommandGrid.CmdGrid g)
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
        /// Sets the background color of the form.
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
        /// The event for when the Submit button is clicked. 
        /// Converts form submission to a command object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (titleTextBox.Text == "" || permissionComboBox.SelectedIndex == -1 || cooldownTextBox.Text == "" || outputTextBox.Text == "")
            {
                MessageBox.Show("Please enter some text!");
                return;
            }
            else
            {
                if (!titleTextBox.Text.StartsWith("!"))
                {
                    MessageBox.Show("Make sure the title of your command starts with a '!', like this: !example");
                    return;
                }

                int num;
                if (!int.TryParse(cooldownTextBox.Text, out num))
                {
                    MessageBox.Show("Cooldown has to use numbers");
                    return;
                }

                TimeSpan t = new TimeSpan(0, 0, num);

                comm = new Command(titleTextBox.Text, outputTextBox.Text, permissionComboBox.SelectedItem.ToString(), t);

                grid.AddCommand(comm);

                this.Close();
            }
        }
    }
}
