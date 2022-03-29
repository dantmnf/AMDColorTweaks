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
using System.Windows.Threading;

namespace AMDColorTweaks
{
    /// <summary>
    /// Interaction logic for RevertCountdownWindow.xaml
    /// </summary>
    public partial class ConfirmSettingsWindow : Window
    {
        DispatcherTimer? timer;
        int countdown = 15;
        public ConfirmSettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            countdown -= 1;
            if (countdown <= 0)
            {
                timer?.Stop();
                DialogResult = false;
            }
            else
            {
                countdownDisplay.Text = $"{countdown}";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            timer?.Stop();
        }
    }
}
