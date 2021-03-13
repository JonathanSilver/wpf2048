using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF2048
{
    /// <summary>
    /// Interaction logic for Element.xaml
    /// </summary>
    public partial class Element : UserControl
    {
        public Element()
        {
            InitializeComponent();

            Value = 0;
            Element2 = null;

            colorList[0] = Colors.Orange;
            colorList[1] = Colors.Orange;
            colorList[2] = Colors.DarkOrange;
            colorList[3] = Colors.DarkOrange;
            colorList[4] = Colors.OrangeRed;
            colorList[5] = Colors.OrangeRed;
            colorList[6] = Colors.Firebrick;
            colorList[7] = Colors.Firebrick;
            colorList[8] = Colors.Firebrick;
            colorList[9] = Colors.DarkRed;
            colorList[10] = Colors.DarkRed;
        }

        Color[] colorList = new Color[11];

        public Color ElementBackColor
        {
            set
            {
                RadialGradientBrush rgb = new RadialGradientBrush();
                rgb.Center = new Point(0.4, 0.4);
                rgb.RadiusX = 0.5;
                rgb.RadiusY = 0.5;
                rgb.GradientOrigin = new Point(0.3, 0.3);
                rgb.GradientStops.Add(new GradientStop(Colors.LightYellow, 0));
                rgb.GradientStops.Add(new GradientStop(value, 0.6));
                rect_Background.Fill = rgb;
            }
        }

        public Color TextColor
        {
            set { tb_Value.Foreground = new SolidColorBrush(value); }
        }

        int _value;

        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (value == 0)
                    tb_Value.Text = "";
                else
                {
                    if (value > 2048)
                    {
                        ElementBackColor = Colors.Red;
                    }
                    else
                    {
                        ElementBackColor = colorList[(int)Math.Log(value, 2) - 1];
                    }
                    tb_Value.Text = value.ToString();
                }
            }
        }

        public Element Element2 { get; set; }
    }
}
