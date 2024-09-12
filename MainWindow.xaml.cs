using BrestaTest.Bresta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private Config[] m_scales;
        private Config[] m_boards;
        private List<ScaleBoard> m_scaleBoards = new List<ScaleBoard>();

        private int m_space = 0;
        private int m_startX = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            m_scales = GetConfigs(Directory.GetFiles(".\\scales", "*.cfg", SearchOption.AllDirectories));
            m_boards = GetConfigs(Directory.GetFiles(".\\boards", "*.cfg", SearchOption.AllDirectories));

            foreach (var scale in m_scales)
            {
                var board = m_boards.Where(c => c.ConfigName == scale.ConfigName).FirstOrDefault();

                if(board != null)
                {
                    m_scaleBoards.Add(new ScaleBoard { Board = board, Scale = scale });
                }
            }

            Repaint();

            var orderedScaleBoards = m_scaleBoards.OrderBy(c => c.Left).ToArray();


            if(orderedScaleBoards.Length > 1)
            {
                m_space = orderedScaleBoards[1].Left - orderedScaleBoards[0].Right;
                m_startX = orderedScaleBoards[0].Left;
            }

            listScales.Items.Clear();
            foreach (var sb in orderedScaleBoards)
            {
                listScales.Items.Add(sb);
            }

            listBoards.Items.Clear();
            foreach (var board in m_boards)
            {
                listBoards.Items.Add(board);
            }
        }

        private void Repaint()
        {
            canvas.Children.Clear();

            foreach (var sb in m_scaleBoards)
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

            listScales.Items.Remove(selected);
            listScales.Items.Insert(selectedIndex - 1, selected);
            listScales.SelectedIndex = selectedIndex - 1;


            UpdateCoordinates();
            Repaint();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var selectedIndex = listScales.SelectedIndex;

            if (selectedIndex < 0 || selectedIndex == listScales.Items.Count - 1) return;

            var selected = (ScaleBoard)listScales.SelectedItem;

            listScales.Items.Remove(selected);
            listScales.Items.Insert(selectedIndex + 1, selected);
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
            Random rnd = new Random();

            int i = rnd.Next(0, listScales.Items.Count);

            var sb = listScales.Items[i] as ScaleBoard;

            var clone = sb.Clone();
            clone.SetName("scalesPaint2");
            clone.SetBodyColor(Colors.Crimson, Colors.HotPink);

            var last = listScales.Items[i] as ScaleBoard;

            clone.MoveHorizontal(last.Right + m_space - clone.Left);

            listScales.Items.Add(clone);

            UpdateCoordinates();
            Repaint();
        }
    }
}
