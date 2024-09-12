using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BrestaTest.Bresta
{
    public class ScaleBoard
    {
        private Config m_scale = null;
        private Config m_board = null;

        public Config Scale { get => m_scale; 
            set 
            { 
                m_scale = value;
                Update();
            } 
        }
        public Config Board { get => m_board; set => m_board = value; }

        private void Update()
        {
            if (m_scale == null || m_board == null) return;

            Right = Math.Max(m_scale.Objects/*.Where(o=>o.VisualObject.VisualType != TypeVisualObject.Path)*/.Max(o=>o.VisualObject.Left + o.VisualObject.Width), m_scale.Objects.Max(o => o.VisualObject.Left + o.VisualObject.Width));
            Left =  Math.Min(m_board.Objects/*.Where(o => o.VisualObject.VisualType != TypeVisualObject.Path)*/.Min(o => o.VisualObject.Left), m_scale.Objects.Min(o => o.VisualObject.Left));
        }

        public void MoveHorizontal(int shift)
        {
            MoveConfig(m_scale, shift);
            MoveConfig(m_board, shift);
            Update();
        }

        private void MoveConfig(Config config, int shift)
        {
            foreach (var o in config.Objects)
            {
                o.VisualObject.Left += shift;
            }
        }

        public ScaleBoard Clone()
        {
            return new ScaleBoard
            {
                Scale = m_scale.Clone(),
                Board = m_board.Clone()
            };
        }

        public void SetName(string name)
        {
            if(m_scale != null)
            {
                m_scale.ConfigName = name;
            }
            if (m_board != null)
            {
                m_board.ConfigName = name;
            }
        }

        public void SetBodyColor(Color color1, Color color2)
        {
            foreach (var o in m_scale.Objects)
            {
                o.VisualObject.Color1 = color1;
                o.VisualObject.Color2 = color2;

            }
        }

        public int Width => Right - Left;
        public int Left { get; private set; } = 0;
        public int Right { get; private set; } = 0;
        public string Name => m_scale?.ConfigName ?? "No name";
        public string ScaleName => m_scale?.Name ?? "No name";
    }
}
