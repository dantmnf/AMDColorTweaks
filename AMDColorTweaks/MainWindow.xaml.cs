using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using AMDColorTweaks.ADL;
using AMDColorTweaks.ViewModel;
using Microsoft.Win32;

namespace AMDColorTweaks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshADLDisplays();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void RefreshADLDisplays()
        {
            viewModel.RefreshDisplays();
            if (viewModel.CurrentDisplay == null)
            {
                viewModel.CurrentDisplay = viewModel.AvailiableDisplays.FirstOrDefault();
            }
            else
            {
                var index = viewModel.AvailiableDisplays.IndexOf(viewModel.CurrentDisplay);
                if (index == -1)
                {
                    viewModel.CurrentDisplay = null;
                }
                else
                {
                    viewModel.CurrentDisplay = viewModel.AvailiableDisplays[index];
                }
            }
        }

        private unsafe void HandleApply(object sender, RoutedEventArgs e)
        {
            if (viewModel.UseRegamma)
            {
                var (minrange, minstdev) = viewModel.TransferSetting.Stat();
                if (minrange < 0.5 || minstdev < 0.1)
                {
                    if (MessageBox.Show(this, $"You are about to apply a transfer with maximum range {minrange} and maximum stdev {minstdev}.\n\nAre you sure?", Title, MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }

            var display = viewModel.CurrentDisplay;
            if (display == null) return;

            var adapterid = display.AdapterId;
            var displayid = display.DisplayId;

            using var adlContext = new ADLContext(true);

            if (display.GetHdrStatus(adlContext).Enabled)
            {
                MessageBox.Show(this, "HDR not supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            GamutViewModel? oldDstGamut = null;
            GamutViewModel? oldSrcGamut = null;
            TransferViewModel? oldTransfer = null;
            if (viewModel.UseDestinationSetting)
            {
                var dstmodel = viewModel.CurrentDestinationViewModel;
                if (dstmodel != null)
                {
                    oldDstGamut = display.GetDestinationGamut(adlContext);
                    var dstgamutdata = dstmodel.ToADL();
                    ADLNative.ADL2_Display_Gamut_Set(adlContext, adapterid, displayid, ADLGamutReference.DestinationGraphics, dstgamutdata);
                    if (viewModel.UseRegamma)
                    {
                        oldTransfer = display.GetOutputTransfer(adlContext);
                        ADLNative.ADL2_Display_RegammaR1_Set(adlContext, adapterid, displayid, viewModel.TransferSetting.ToADLRegammaEx());
                    }
                }
            }
            if (viewModel.UseSourceSetting)
            {
                oldSrcGamut = display.GetSourceGamut(adlContext);
                var srcmodel = viewModel.CurrentSourceViewModel;
                if (srcmodel != null)
                {
                    var srcgamutdata = srcmodel.ToADL();
                    ADLNative.ADL2_Display_Gamut_Set(adlContext, adapterid, displayid, ADLGamutReference.SourceGraphics, srcgamutdata);
                }
            }
            var confirmDlg = new ConfirmSettingsWindow() { Owner = this };

            if (confirmDlg.ShowDialog().GetValueOrDefault(false))
            {
                // confirmed
                RefreshADLDisplays();
            }
            else
            {
                // revert
                if (viewModel.UseDestinationSetting && oldDstGamut != null)
                {
                    ADLNative.ADL2_Display_Gamut_Set(adlContext, adapterid, displayid, ADLGamutReference.DestinationGraphics, oldDstGamut.ToADL());
                    if (viewModel.UseRegamma && oldTransfer != null)
                    {
                        ADLNative.ADL2_Display_RegammaR1_Set(adlContext, adapterid, displayid, oldTransfer.ToADLRegammaEx());
                    }
                }
                if (viewModel.UseSourceSetting && oldSrcGamut != null)
                {
                    ADLNative.ADL2_Display_Gamut_Set(adlContext, adapterid, displayid, ADLGamutReference.SourceGraphics, oldSrcGamut.ToADL());
                }
            }
        }


        private void HandleReset(object sender, RoutedEventArgs e)
        {
            var display = viewModel.CurrentDisplay;
            if (display == null) return;

            var adapterid = display.AdapterId;
            var displayid = display.DisplayId;

            using var adlContext = new ADLContext(true);

            var hdr = display.GetHdrStatus(adlContext).Enabled;
            if (hdr)
            {
                MessageBox.Show(this, "HDR not supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var gamutdata = new ADLGamutData() { iPredefinedGamut = hdr ? ADLGamutSpace.ADL_GAMUT_SPACE_CCIR_2020 : ADLGamutSpace.ADL_GAMUT_SPACE_CIE_RGB, iPredefinedWhitePoint = ADLWhitePoint.ADL_WHITE_POINT_6500K };
            var transfer = new TransferViewModel() { TransferType = TransferType.ParametricOETF, UseUniformParametricCurve = true };
            transfer.UniformCurve.A0 = 0.0031308;
            transfer.UniformCurve.A1 = 12.92;
            transfer.UniformCurve.A2 = 0.055;
            transfer.UniformCurve.A3 = 0.055;
            transfer.UniformCurve.Gamma = 2.4;

            var gammadata = transfer.ToADLRegammaEx();
            ADLNative.ADL2_Display_Gamut_Set(adlContext, adapterid, displayid, ADLGamutReference.SourceGraphics, gamutdata);
            ADLNative.ADL2_Display_Gamut_Set(adlContext, adapterid, displayid, ADLGamutReference.DestinationGraphics, gamutdata);
            ADLNative.ADL2_Display_RegammaR1_Set(adlContext, adapterid, displayid, gammadata);
            //gammadata.Feature |= ADLRegammaExFeature.ADL_APPLY_DEGAMMA;
            //ADLNative.ADL2_Display_RegammaR1_Set(adlContext, adapterid, displayid, gammadata);
            RefreshADLDisplays();
        }

        private void HandleClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleRefersh(object sender, RoutedEventArgs e)
        {
            RefreshADLDisplays();
        }

        private void HandleAbout(object sender, RoutedEventArgs e)
        {
            var dlg = new AboutWindow() { Owner = this };
            dlg.ShowDialog();
        }

        private void EditTransferButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new EditTransferWindow();
            dlg.Owner = Window.GetWindow(this);
            var tvm = viewModel.TransferSetting.Clone();
            dlg.DataContext = tvm;
            var result = dlg.ShowDialog();
            if (result.GetValueOrDefault(false))
            {
                viewModel.TransferSetting = tvm;
            }
        }

        private void LoadDestinationICC_Click(object sender, RoutedEventArgs e)
        {
            //LittleCms.TestRead.Foo();
            var opendlg = new OpenFileDialog();
            opendlg.Filter = "ICC profile (*.icc, *.icm)|*.icc;*.icm|All files|*.*";
            if (opendlg.ShowDialog(this).GetValueOrDefault(false))
            {
                try
                {
                    var icc = File.ReadAllBytes(opendlg.FileName);
                    var (gamut, transfer) = LittleCms.IccReader.ReadICC(icc, viewModel.MergeVcgtIntoTrc);
                    gamut.IsDestinationSetting = true;
                    var oldUseRelativeColormetric = viewModel.CurrentDestinationViewModel?.UseRelativeColorimetric;
                    gamut.UseRelativeColorimetric = oldUseRelativeColormetric.GetValueOrDefault(true);
                    viewModel.CurrentDestinationViewModel = gamut;
                    viewModel.TransferSetting = transfer;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
