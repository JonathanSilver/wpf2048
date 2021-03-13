using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WPF2048
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            btnOK.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard s = new Storyboard();
            DoubleAnimation d = new DoubleAnimation();
            d.From = -300;
            d.To = 0;
            d.Duration = TimeSpan.FromSeconds(0.5);
            Storyboard.SetTarget(d, p);
            Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.LeftProperty));
            s.Children.Add(d);
            s.Begin();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
