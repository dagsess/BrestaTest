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

        private int m_space = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            m_scales = GetConfigs(Directory.GetFiles(".\\scales", "*.cfg", SearchOption.AllDirectories));
            m_boards = GetConfigs(Directory.GetFiles(".\\boards", "*.cfg", SearchOption.AllDirectories));
            Repaint();

            var orderedScales = m_scales.OrderBy(c => c.Objects.Select(o => o.VisualObject.Left).Min()).ToArray();

            listScales.Items.Clear();

            if(orderedScales.Length > 1)
            {
                m_space = orderedScales[1].Left - orderedScales[0].Right;
            }

            foreach (var scale in orderedScales)
            {
                listScales.Items.Add(scale);
            }

            foreach (var board in m_boards)
            {
                listBoards.Items.Add(board);
            }
        }

        private void Repaint()
        {
            canvas.Children.Clear();

            foreach (var vo in m_scales.SelectMany(c=>c.Objects.Select(o=>o.VisualObject)))
            {
                DrawVisualObject(vo);
            }

            foreach (var vo in m_boards.SelectMany(c => c.Objects.Select(o => o.VisualObject)))
            {
                DrawVisualObject(vo);
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

            TextBlock text = new TextBlock
            {
                Text = visualObject.Name,
                Foreground = new SolidColorBrush(Colors.Black),
                Width = visualObject.Width,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(3)
            };

            /*canvas.Children.Add(text);
            Canvas.SetLeft(text, visualObject.Left);
            Canvas.SetTop(text, visualObject.Top);*/


        }
    }
}
