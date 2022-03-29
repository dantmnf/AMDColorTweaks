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
using AMDColorTweaks.ViewModel;

namespace AMDColorTweaks
{
    /// <summary>
    /// Interaction logic for ChromaticityControl.xaml
    /// </summary>
    public partial class ChromaticityControl : UserControl
    {
        public ChromaticityValue? Value { get; set; }
        public ChromaticityControl()
        {
            InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var box = (TextBox)sender;
            box.SelectAll();
        }
        private void SelectivelyIgnoreMouseButton(object sender, RoutedEventArgs e)
        {
            var box = (TextBox)sender;
            if (!box.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                box.Focus();
            }
        }
    }
}
