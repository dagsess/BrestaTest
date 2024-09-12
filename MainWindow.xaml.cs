using BrestaTest.Bresta;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;

namespace BrestaTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SCALESPAINT_NAME = "scalesPaint2.cfg";
        private Config[] m_scales;
        private Config[] m_boards;
        //private List<ScaleBoard> m_scaleBoards = new List<ScaleBoard>();

        private int m_space = 0;
        private int m_startX = 0;

        public ObservableCollection<ScaleBoard> ScaleBoards { get; set; } = new ObservableCollection<ScaleBoard> { };

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            buttonPanel.IsEnabled = false;
            buttonPanel2.IsEnabled = false;

            bool isSleep = (bool)isSlow.IsChecked;

            await Task.Factory.StartNew(() =>
            {
                m_scales = GetConfigs(Directory.GetFiles(".\\scales", "*.cfg", SearchOption.AllDirectories));
                m_boards = GetConfigs(Directory.GetFiles(".\\boards", "*.cfg", SearchOption.AllDirectories));

                List<ScaleBoard> tmpScaleBoards = new List<ScaleBoard>();

                foreach (var scale in m_scales)
                {
                    var board = m_boards.Where(c => c.ConfigName == scale.ConfigName).FirstOrDefault();

                    if (board != null)
                    {
                        tmpScaleBoards.Add(new ScaleBoard { Board = board, Scale = scale });
                    }
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ScaleBoards.Clear();
                }));

                foreach (var scale in tmpScaleBoards.OrderBy(c => c.Left))
                {
                    if(isSleep)
                    {
                        Thread.Sleep(500);
                    }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ScaleBoards.Add(scale);
                        Repaint();
                    }));
                 }

                if (ScaleBoards.Count > 1)
                {
                    m_space = ScaleBoards[1].Left - ScaleBoards[0].Right;
                    m_startX = ScaleBoards[0].Left;
                }

            });

            buttonPanel.IsEnabled = true;
            buttonPanel2.IsEnabled = true;


        }

        private void Repaint()
        {
            canvas.Children.Clear();

            foreach (var sb in ScaleBoards)
            {
                foreach (var vo in sb.Scale.Objects.Select(o => o.VisualObject))
                {
                    DrawVisualObject(vo);
                }
                foreach (var vo in sb.Board.Objects.Select(o => o.VisualObject))
                {
                    DrawVisualObject(vo);
                }
            }

        }

        private Config[] GetConfigs(string[] fileNames)
        {
            List<Config> configs = new List<Config>();

            foreach (var file in fileNames)
            {
                if (Config.TryLoadFile(file, out Config config))
                {
                    configs.Add(config);
                }
            }

            return configs.ToArray();
        }

        private void DrawVisualObject(VisualObject visualObject)
        {
            if (visualObject.VisualType == TypeVisualObject.Path) return;

            FrameworkElement element = new Rectangle();
            
            if(visualObject.VisualType == TypeVisualObject.Button)
            {
                element = new Button();
            }

            if (visualObject.VisualType == TypeVisualObject.Form)
            {
                element = new Rectangle();
                (element as Rectangle).Stroke = new SolidColorBrush(visualObject.Stroke);
                (element as Rectangle).Fill = new LinearGradientBrush(visualObject.Color1, visualObject.Color2, 0);

            }


            element.Width = visualObject.Width;
            element.Height = visualObject.Height;

            canvas.Children.Add(element);
            Canvas.SetLeft(element, visualObject.Left);
            Canvas.SetTop(element, visualObject.Top);

            if (visualObject.VisualType == TypeVisualObject.Form)
            {
                TextBlock text = new TextBlock
                {
                    Text = visualObject.Name,
                    Foreground = new SolidColorBrush(Colors.Black),
                    Width = visualObject.Width,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(3)
                };

                canvas.Children.Add(text);
                Canvas.SetLeft(text, visualObject.Left);
                Canvas.SetTop(text, visualObject.Top);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedIndex = listScales.SelectedIndex;

            if (selectedIndex <= 0) return;


            var selected = (ScaleBoard)listScales.SelectedItem;

            ScaleBoards.Remove(selected);
            ScaleBoards.Insert(selectedIndex - 1, selected);
            listScales.SelectedIndex = selectedIndex - 1;

            UpdateCoordinates();
            Repaint();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var selectedIndex = listScales.SelectedIndex;

            if (selectedIndex < 0 || selectedIndex == listScales.Items.Count - 1) return;

            var selected = (ScaleBoard)listScales.SelectedItem;

            ScaleBoards.Remove(selected);
            ScaleBoards.Insert(selectedIndex + 1, selected);
            listScales.SelectedIndex = selectedIndex + 1;

            UpdateCoordinates();
            Repaint();

        }

        private void UpdateCoordinates()
        {
            for (int i = 0; i < listScales.Items.Count; i++)
            {
                var config = (listScales.Items[i] as ScaleBoard);
                int actualPosition = m_startX;

                for (int j = 0; j <= i - 1; j++)
                {
                    actualPosition += (listScales.Items[j] as ScaleBoard).Width + m_space;
                }

                int shift = actualPosition - config.Left;
                config.MoveHorizontal(shift);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if(ScaleBoards.Count == 0) return;

            var found = ScaleBoards.Where(s=>s.Scale.ConfigName == SCALESPAINT_NAME).Count();

            if (found > 0) return;

            Random rnd = new Random();

            int i = rnd.Next(0, ScaleBoards.Count);

            var sb = ScaleBoards[i];

            var clone = sb.Clone();
            clone.SetConfigName(SCALESPAINT_NAME);
            clone.Scale.SetName("Весы краски 2");
            clone.SetBodyColor(Colors.Crimson, Colors.HotPink);

            var last = ScaleBoards.Last();

            clone.MoveHorizontal(last.Right + m_space - clone.Left);

            ScaleBoards.Add(clone);

            //UpdateCoordinates();
            Repaint();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (ScaleBoards.Count == 0) return;

            foreach (var sb in ScaleBoards)
            {
                sb.Save();
            }
        }
    }
}
