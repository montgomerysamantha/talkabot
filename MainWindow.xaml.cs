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
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net.Sockets;

namespace Twitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WebChat chat = new WebChat();
        private int lines = 0;
        private int commandHeight = 110;
        private int editedCommandHeight = 0;
        private bool color = false;
        private bool fillPurple = false;
        private bool selectedRecFill = false;
        private Rectangle selectedRec;
        private Canvas lastButtonClicked;
        private Dictionary<Canvas, Canvas> canvasLink;
        private Dictionary<Command, Canvas> visibleCommands;
        private LinearGradientBrush gray;
        private LinearGradientBrush purple;
        public TwitchIRC connection;
        public MainWindow()
        {
            InitializeComponent();
            chat.Run();
            TChatTextBox.Document.Blocks.Clear();
            TChatTextBox.IsReadOnly = true;
            lastButtonClicked = commandCanvas;
            makeBrushes(); // set up the gradient brushes for future use
            visibleCommands = new Dictionary<Command, Canvas>();

            canvasLink = new Dictionary<Canvas, Canvas>();
            canvasLink.Add(commandCanvas, commandCanvasUI);
            canvasLink.Add(timerCanvas, timerCanvasUI);
            canvasLink.Add(logoutCanvas, logoutCanvasUI);
        }

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
                tr.Text = data.username + ": ";
                SolidColorBrush s = (SolidColorBrush)(new BrushConverter().ConvertFrom(data.tags.color));
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, s);

                TextRange selection = new TextRange(TChatTextBox.Document.ContentEnd, TChatTextBox.Document.ContentEnd);
                selection.Text = data.message + Environment.NewLine;
                selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }
            else
            {
                // no color selected, so use default powder blue color 
                TextRange tr = new TextRange(TChatTextBox.Document.ContentEnd, TChatTextBox.Document.ContentEnd);
                tr.Text = data.username + ": ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.PowderBlue);

                TextRange selection = new TextRange(TChatTextBox.Document.ContentEnd, TChatTextBox.Document.ContentEnd);
                selection.Text = data.message + Environment.NewLine;
                selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }


            TChatTextBox.ScrollToEnd();

            lines++;

            return;
        }

        private void Exit_Form(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Move(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Activate_Form(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Deactivate_Form(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Released)
                this.DragMove();
        }

        private void Main(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("pack://application:,,,/Resources/background.png");
            bitmap.EndInit();
            backlogin.Source = bitmap;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void commandCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            commandCanvas.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF462D73"));
        }

        private void timerCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timerCanvas.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF462D73"));
        }

        private void logoutCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            logoutCanvas.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF462D73"));
        }

        private void clickColorEvent(object sender, MouseButtonEventArgs e)
        {
            Canvas c = sender as Canvas;
            if (c != lastButtonClicked)
            {
                Canvas selected = canvasLink[c];
                Canvas hide = canvasLink[lastButtonClicked];

                lastButtonClicked.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF25292E"));
                c.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF462D73"));
                lastButtonClicked = c;


                hide.Visibility = Visibility.Collapsed;
                selected.Visibility = Visibility.Visible;
            }
        }

        private void makeBrushes()
        {
            Point pend = new Point(0.5, 1);
            Point pstar = new Point(0.5, 0);
            GradientStopCollection gradientStopsGray = new GradientStopCollection();
            GradientStopCollection gradientStops1Purple = new GradientStopCollection();


            //alternate between gray and purple color in UI
            Color col1 = Color.FromRgb(0, 0, 0);
            Color col2;
            Color col3;

            //gray color
            col2 = Color.FromRgb(103, 116, 157);
            //purple color
            col3 = Color.FromArgb(75, 115, 75, 191);


            GradientStop g1 = new GradientStop(col1, 0);
            gradientStopsGray.Add(g1);
            GradientStop g2 = new GradientStop(col2, 1);
            gradientStopsGray.Add(g2);
            gray = new LinearGradientBrush(gradientStopsGray, pend, pstar);
            GradientStop g3 = new GradientStop(col3, 1);
            gradientStops1Purple.Add(g3);
            gradientStops1Purple.Add(g1);
            purple = new LinearGradientBrush(gradientStops1Purple, pend, pstar);
        }

        public void addCommandToUI(Command addme)
        {
            if (visibleCommands.ContainsKey(addme))
            {
                Canvas c = visibleCommands[addme];
                if (editedCommandHeight != 0)
                {
                    Canvas.SetTop(c, editedCommandHeight);
                }
                else
                {
                    Canvas.SetTop(c, commandHeight);
                }
                //commandHeight = commandHeight + 50;

                Rectangle r = c.Children.OfType<Rectangle>().FirstOrDefault();
                //alternate between gray and purple color in UI
                if (!color)
                {
                    //gray color
                    r.Fill = gray;
                    color = true;
                }
                else
                {
                    //purple color
                    r.Fill = purple;
                    color = false;
                }

                Render(r);
                Render(c);
            }
            else
            {
                Canvas c = new Canvas();
                Rectangle r = new Rectangle();
                Canvas.SetLeft(c, 0);
                Canvas.SetTop(c, commandHeight);
                commandHeight = commandHeight + 50;
                c.Visibility = Visibility.Visible;
                r.Width = 900;
                r.Height = 50;
                r.MouseEnter += mouseEnterRec;
                r.MouseLeave += mouseLeaveRec;
                r.MouseDown += mouseDownRec;
                visibleCommands.Add(addme, c);

                //alternate between gray and purple color in UI
                if (!color)
                {
                    //gray color
                    r.Fill = gray;
                    color = true;
                }
                else
                {
                    //purple color
                    r.Fill = purple;
                    color = false;
                }

                c.Children.Add(r);

                TextBlock commText = new TextBlock();
                commText.FontSize = 22;
                commText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
                Thickness thicc = new Thickness(30, 8, 60, 0);
                commText.Margin = thicc;
                commText.Text = addme.title;

                TextBlock permissText = new TextBlock();
                permissText.FontSize = 22;
                permissText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
                thicc.Left = thicc.Left + 150;
                permissText.Margin = thicc;
                permissText.Text = addme.permission;

                TextBlock cdText = new TextBlock();
                cdText.FontSize = 22;
                cdText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
                thicc.Left = thicc.Left + 150;
                cdText.Margin = thicc;
                cdText.Text = addme.cooldown.TotalSeconds.ToString();

                TextBlock outputText = new TextBlock();
                outputText.FontSize = 22;
                outputText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
                thicc.Left = thicc.Left + 150;
                outputText.Margin = thicc;
                outputText.Text = addme.output;

                c.Children.Add(commText);
                c.Children.Add(permissText);
                c.Children.Add(cdText);
                c.Children.Add(outputText);
                //CommandBar.Children.Add(c);
            }
            
        }

        public void deleteCommandFromUI(Command todelete)
        {
            if (visibleCommands.ContainsKey(todelete))
            {
                Canvas c = visibleCommands[todelete];
                visibleCommands.Remove(todelete);
                commandCanvasUI.Children.Remove(c as UIElement);
                commandHeight = 110;

                foreach (KeyValuePair<Command, Canvas> kvp in visibleCommands)
                {
                    addCommandToUI(kvp.Key);
                }
            }
            else
            {

            }

            return;
        }

        private void Render(UIElement element)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                element.InvalidateVisual();
            }));
        }

        private void mouseEnterRec(object sender, RoutedEventArgs e)
        {
            Rectangle r = sender as Rectangle;
            if (r == selectedRec) return; // don't change color of selected rectangle

            Brush b = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737373"));
            if (r.Fill == purple)
            {
                fillPurple = true;
            }
            r.Fill = b;
            Render(r);
        }

        private void mouseLeaveRec(object sender, RoutedEventArgs e)
        {
            Rectangle r = sender as Rectangle;

            if (r == selectedRec) return; // don't change color of selected rectangle

            if (fillPurple)
            {
                r.Fill = purple;
                fillPurple = false;
            }
            else
            {
                r.Fill = gray;
            }
            Render(r);
        }

        private void mouseDownRec(object sender, RoutedEventArgs e)
        {
            Rectangle r = sender as Rectangle;
            Brush b = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737373"));

            if (selectedRecFill && selectedRec != null)
            {
                selectedRec.Fill = purple;
            }
            else if (selectedRec != null)
            {
                selectedRec.Fill = gray;
            }

            if (r.Fill == purple)
            {
                selectedRecFill = true;
            }
            else
            {
                selectedRecFill = false;
            }
            selectedRec = r;
            Render(r);
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            //AddCommand add = new AddCommand(this);
            //add.Show();
        }

        public void RecieveNewCommand(Command c)
        {
            
            return;
        }

        private void EditCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRec == null) return;
            DependencyObject dep = VisualTreeHelper.GetParent(selectedRec);
            Canvas c = dep as Canvas;
            editedCommandHeight = (int)c.GetValue(Canvas.TopProperty);
            TextBlock t = c.Children.OfType<TextBlock>().FirstOrDefault();
            string title = t.Text;
            Command comm = visibleCommands.FirstOrDefault(x => x.Value == c).Key;

            //EditCommand edit = new EditCommand(this);
            //edit.LoadCommand(comm);
            //edit.Show();
        }

        public void RecieveEditedCommand(Command original, Command c)
        {
            if (visibleCommands.ContainsKey(c))
            {
                // command has not changed, do not update
                return;
            }
            else
            {
                // need to update key in original list
                if (visibleCommands.ContainsKey(original))
                {

                    return;
                }
                else
                {
                    // original does not exist, so do not try to modify / delete
                    return;
                }
            }
        }

        private void DeleteCommandButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            connection.SaveConfigRequest();
        }
    }
}
