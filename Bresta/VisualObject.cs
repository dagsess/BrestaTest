﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BrestaTest.Bresta
{
    public enum TypeVisualObject
    {
        Form,
        Button,
        Path
    }

    public class VisualObject
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Color Color1 { get; set; } = Colors.Red;
        public Color Color2 { get; set; } = Colors.Blue;
        public string ColorType { get; set; }
        public Color Stroke { get; set; } = Colors.Black;
        public string Name { get; set; }
        public string Sign { get; set; }
        public TypeVisualObject VisualType { get; set; } = TypeVisualObject.Form;

        public VisualObject Clone()
        {
            return new VisualObject
            {
                Color1 = Color1,
                Color2 = Color2,
                ColorType = ColorType,
                Height = Height,
                Left = Left,
                Name = Name,
                Sign = Sign,
                Stroke = Stroke,
                Top = Top,
                VisualType = VisualType,
                Width = Width
            };
        }
    }
}
