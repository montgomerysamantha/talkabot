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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Twitch.CommandGrid
{
    /// <summary>
    /// Interaction logic for CmdGrid.xaml
    /// </summary>
    public partial class CmdGrid : UserControl
    {
        Action<Command, Command> OnModifyRow = null;
        private LinearGradientBrush purple;
        private StackPanel selectedRec;
        Dictionary<Command, KeyValuePair<StackPanel, CheckBox>> rows = new Dictionary<Command, KeyValuePair<StackPanel, CheckBox>>();
        TalkaBot talkref;

        /// <summary>
        /// Makes the brushes for the CmdGrid.
        /// </summary>
        public CmdGrid()
        {
            InitializeComponent();
            MakeBrush();
        }

        /// <summary>
        /// Gets set to ModifyEvent in Talkabot, what happens when a command is edited.
        /// </summary>
        /// <param name="callback">The action to do for the event</param>
        public void SetModifyEvent(Action<Command, Command> callback)
        {
            OnModifyRow = callback;
        }

        /// <summary>
        /// Fixes UI element rendering issue.
        /// </summary>
        /// <param name="element">Reload this element</param>
        private void Render(UIElement element)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                element.InvalidateVisual();
            }));
        }

        /// <summary>
        /// Finds a command in the UI.
        /// </summary>
        /// <param name="command">command title to find</param>
        /// <returns></returns>
        private Command FindCommand(string command)
        {
            foreach(var keyValue in rows)
            {
                if (keyValue.Key.title == command)
                    return keyValue.Key;
            }

            return null;
        }

        /// <summary>
        /// Just setting up the brushes for the gradients on the UI.
        /// </summary>
        private void MakeBrush()
        {
            Point pend = new Point(0.5, 1);
            Point pstar = new Point(0.5, 0);
            GradientStopCollection gradientStops1Purple = new GradientStopCollection();


            //alternate between gray and purple color in UI
            Color col1 = Color.FromRgb(0, 0, 0);
            Color col3;

            //purple color
            col3 = Color.FromArgb(75, 115, 75, 191);


            GradientStop g1 = new GradientStop(col1, 0);
            GradientStop g3 = new GradientStop(col3, 1);
            gradientStops1Purple.Add(g3);
            gradientStops1Purple.Add(g1);
            purple = new LinearGradientBrush(gradientStops1Purple, pend, pstar);
        }

        /// <summary>
        /// Highlights a rectangle gray if the mouse hovers over it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseEnterRec(object sender, RoutedEventArgs e)
        {
            StackPanel r = null;
            if (sender is TextBlock)
            {
                TextBlock t = sender as TextBlock;
                if (t.Parent is Canvas)
                {
                    Canvas c = t.Parent as Canvas;
                    r = c.Children.OfType<StackPanel>().FirstOrDefault();
                }
            }

            if (sender is StackPanel)
                r = sender as StackPanel;

            if (r == selectedRec) return; // don't change color of selected rectangle

            Brush b = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737373"));
            if(r != null)
            {
                r.Background = b;
                Render(r);
            }
        }

        /// <summary>
        /// Unhighlights the rectangle when the mouse leaves.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseLeaveRec(object sender, RoutedEventArgs e)
        {
            StackPanel r = sender as StackPanel;

            if (r == selectedRec) return; // don't change color of selected rectangle

            r.Background = purple;
            Render(r);
        }

        /// <summary>
        /// A more permanant gray highlight of the rectangle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseDownRec(object sender, RoutedEventArgs e)
        {
            StackPanel r = sender as StackPanel;
            Brush b = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737373"));

            if(selectedRec != null)
            {
                selectedRec.Background = purple;
            }

            selectedRec = r;
            Render(r);
        }

        /// <summary>
        /// When "enter" is recieved after edited a TextBox, update the command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCommand_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox)
                {
                    TextBox tb = sender as TextBox;
                    if (tb.Parent is StackPanel)
                    {
                        StackPanel c = tb.Parent as StackPanel;
                        var childs = c.Children.OfType<TextBlock>();
                        string commandtitle = null;
                        foreach (TextBlock t in childs)
                        {
                            if (t.Name == "command")
                            {
                                commandtitle = t.Text;
                                break;
                            }
                        }
                        string container = null;
                        Command cmd = null;
                        KeyValuePair<StackPanel, CheckBox> tempKeyValue = new KeyValuePair<StackPanel, CheckBox>();
                        Command originalCmd = null;

                        if (commandtitle != null)
                        {
                            cmd = FindCommand(commandtitle);
                            rows.TryGetValue(cmd, out tempKeyValue);
                            originalCmd = new Command(cmd.title, cmd.output, cmd.permission, cmd.cooldown);
                            if (cmd != null)
                            {
                                switch (tb.Name)
                                {
                                    case "command":
                                        {
                                            container = "command";
                                            int count = 0;
                                            tb.Text = tb.Text.Trim();
                                            tb.Text = tb.Text.Replace(" ", "");
                                            foreach (char ch in tb.Text)
                                            {
                                                if (ch == '!') count++;
                                            }

                                            if (tb.Text.StartsWith("!") && count == 1)
                                            {
                                                cmd.title = tb.Text;
                                            }
                                            else
                                            {
                                                tb.Text = tb.Text.Replace("!", "");
                                                if (tb.Text == "") tb.Text = "default";
                                                tb.Text = tb.Text.Insert(0, "!");
                                                cmd.title = tb.Text;
                                            }
                                            
                                            break;
                                        }
                                    case "permission":
                                        {
                                            container = "permission";
                                            tb.Text = tb.Text.Trim();
                                            tb.Text = tb.Text.Replace(" ", "");
                                            if (tb.Text != "everyone" && tb.Text != "mods+")
                                            {
                                                tb.Text = "everyone";
                                            }

                                            cmd.permission = tb.Text;
                                            break;
                                        }
                                    case "cooldown":
                                        {
                                            container = "cooldown";
                                            tb.Text = tb.Text.Trim();
                                            tb.Text = tb.Text.Replace(" ", "");
                                            int seconds = 0;
                                            if (int.TryParse(tb.Text, out seconds))
                                            {
                                                cmd.cooldown = new TimeSpan(0, 0, seconds);
                                            }
                                            else
                                            {
                                                tb.Text = "0";
                                                cmd.cooldown = new TimeSpan(0, 0, 0);
                                            }

                                            break;
                                        }
                                    case "output":
                                        {
                                            container = "output";
                                            tb.Text = tb.Text.Trim();
                                            cmd.output = tb.Text;
                                            break;
                                        }
                                }
                            }
                        }

                        foreach (TextBlock t in childs)
                        {
                            if (t.Name == container && cmd != null)
                            {
                                t.Text = tb.Text;
                                t.Visibility = Visibility.Visible;
                                tb.Visibility = Visibility.Collapsed;

                                rows.Remove(cmd);
                                rows.Add(cmd, tempKeyValue);

                                OnModifyRow?.Invoke(originalCmd, cmd);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts a command TextBlock to a TextBox so it can be edited and saved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditCommand_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TextBlock text = sender as TextBlock;
                TextBox tb = new TextBox();
                StackPanel c = text.Parent as StackPanel;

                tb.Width = 80;
                tb.Height = 30;
                tb.Text = text.Text;
                tb.Margin = text.Margin;
                text.Visibility = Visibility.Collapsed;
                tb.FontFamily = text.FontFamily;
                tb.FontSize = text.FontSize;
                tb.Foreground = text.Foreground;
                tb.Background = c.Background;
                Thickness thicc = new Thickness(0, 0, 0, 0);
                tb.BorderThickness = thicc;
                tb.KeyUp += SaveCommand_Enter;
                tb.Name = text.Name;

                tb.Focus();

                c.Children.Insert(c.Children.IndexOf(text), tb);
            }
        }

        /// <summary>
        /// Adds a command to the UI and into the Talkabot as well.
        /// The Talkabot then adds it to the commandsList in TwitchIRC.
        /// </summary>
        /// <param name="cmd"></param>
        public void AddCommand(Command cmd)
        {
            AddCommandTalkabot(cmd);
            StackPanel tempPanel = new StackPanel();
            tempPanel.Height = 50;
            tempPanel.Background = purple;
            tempPanel.Orientation = Orientation.Horizontal;
            tempPanel.MouseDown += mouseDownRec;
            tempPanel.MouseEnter += mouseEnterRec;
            tempPanel.MouseLeave += mouseLeaveRec;

            CheckBox check = new CheckBox();
            check.Style = (Style)FindResource("MyCheckBox");


            TextBlock commText = new TextBlock();
            commText.Name = "command";
            commText.FontSize = 22;
            commText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            Thickness thicc = new Thickness(18, 8, 60, 0);
            commText.Margin = thicc;
            commText.Width = 100;
            commText.Text = cmd.title;
            commText.MouseDown += EditCommand_Click;
            commText.MouseEnter += mouseEnterRec;

            TextBlock permissText = new TextBlock();
            permissText.Name = "permission";
            permissText.MouseDown += EditCommand_Click;
            permissText.MouseEnter += mouseEnterRec;
            permissText.FontSize = 22;
            permissText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            thicc.Left = thicc.Left - 30;
            permissText.Margin = thicc;
            permissText.Width = 100;
            permissText.Text = cmd.permission;

            TextBlock cdText = new TextBlock();
            cdText.Name = "cooldown";
            cdText.MouseDown += EditCommand_Click;
            cdText.MouseEnter += mouseEnterRec;
            cdText.FontSize = 22;
            cdText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            thicc.Left = thicc.Left + 10;
            cdText.Margin = thicc;
            cdText.Width = 75;
            cdText.Text = cmd.cooldown.TotalSeconds.ToString();

            TextBlock outputText = new TextBlock();
            outputText.Name = "output";
            outputText.MouseDown += EditCommand_Click;
            outputText.MouseEnter += mouseEnterRec;
            outputText.FontSize = 22;
            outputText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            thicc.Left = thicc.Left;
            outputText.Margin = thicc;
            outputText.Width = 300;
            outputText.Text = cmd.output;

            tempPanel.Children.Add(check);
            tempPanel.Children.Add(commText);
            tempPanel.Children.Add(permissText);
            tempPanel.Children.Add(cdText);
            tempPanel.Children.Add(outputText);

            GridMain.Children.Add(tempPanel);
            rows.Add(cmd, new KeyValuePair<StackPanel, CheckBox>(tempPanel, check));
            Render(GridMain);
        }

        /// <summary>
        /// Event for when the add button is clicked, sends different forms for timers
        /// and commands, respectively.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (this.Name == "timers")
            {
                AddTimer addt = new AddTimer(this);
                addt.Show();
            }
            else
            {
                AddCommand add = new AddCommand(this);
                add.Show();
            }
        }

        /// <summary>
        /// Deletes selected commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCommandButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var keyValue in rows)
            {
                if(keyValue.Value.Value.IsChecked == true)
                {
                    GridMain.Children.Remove(keyValue.Value.Key);
                    Command c = keyValue.Key; // get command to delete since it's checked
                    DeleteCommandTalkabot(c); // send it to the Talkabot
                }
            }
        }

        /// <summary>
        /// Adds a reference to the Talkabot window.
        /// </summary>
        /// <param name="t"></param>
        public void AddMainWindow(TalkaBot t)
        {
            talkref = t;
        }

        /// <summary>
        /// Sends a command to the Talkabot window, to add to the commandsList
        /// in the TwitchIRC connection.
        /// </summary>
        /// <param name="c"></param>
        private void AddCommandTalkabot(Command c)
        {
            talkref.RecieveNewCommand(c);
        }

        /// <summary>
        /// Sends a command to the Talkabot window, to delete from the commandsList
        /// in the TwitchIRC connection.
        /// </summary>
        /// <param name="c"></param>
        private void DeleteCommandTalkabot(Command c)
        {
            talkref.RecieveCommandToDelete(c);
        }
    }
}
