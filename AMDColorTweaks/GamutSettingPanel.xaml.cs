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
    /// Interaction logic for GamutSetting.xaml
    /// </summary>
    public partial class GamutSettingPanel : UserControl
    {

        private static GamutViewModel defaultDataContext = new();

        public GamutSettingPanel()
        {
            if (DataContext == null) DataContext = defaultDataContext;
            InitializeComponent();
            GamutSelect.ItemsSource = GamutViewModel.GamutItems;
            WhitePointSelect.ItemsSource = GamutViewModel.WhitePointItems;
        }


    }
}
