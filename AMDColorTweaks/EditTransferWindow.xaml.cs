using AMDColorTweaks.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AMDColorTweaks
{
    /// <summary>
    /// Interaction logic for EditTransferWindow.xaml
    /// </summary>
    public partial class EditTransferWindow : Window
    {
        public EditTransferWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SetLinearLUTButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TransferViewModel;
            if (vm != null)
            {
                TransferViewModel.LinearLUT.CopyTo(vm.RedLUT);
                TransferViewModel.LinearLUT.CopyTo(vm.GreenLUT);
                TransferViewModel.LinearLUT.CopyTo(vm.BlueLUT);
                vm.NotifyLUTChange();
            }
        }

        private ushort ValidateOutputValue(string channel, double value, int line)
        {
            if (value < 0 || value > 65535)
            {
                throw new FileFormatException($"{channel} value at line {line} out of range");
            }
            return (ushort)Math.Round(value);
        }

        private void ReadCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var opendlg = new OpenFileDialog();
            opendlg.Filter = "CSV file (*.csv)|*.csv|All files|*.*";
            if (opendlg.ShowDialog(this).GetValueOrDefault(false))
            {
                try
                {
                    using var fs = new FileStream(opendlg.FileName, FileMode.Open, FileAccess.Read);
                    using var reader = new StreamReader(fs, Encoding.UTF8, true);
                    using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
                    csv.Read();
                    csv.ReadHeader();
                    if (!(csv.HeaderRecord.Contains("input") && csv.HeaderRecord.Contains("r") && csv.HeaderRecord.Contains("g") && csv.HeaderRecord.Contains("b")))
                    {
                        throw new FileFormatException("need columns: input, r, g, b");
                    }
                    var vm = DataContext as TransferViewModel;
                    if (vm == null)
                    {
                        throw new ArgumentException("DataContext");
                    }
                    while (csv.Read())
                    {
                        var input = csv.GetField<int>("input");
                        var line = csv.CurrentIndex;
                        if (input < 0 || input > 255) throw new FileFormatException($"input value at line {line} out of range");
                        vm.RedLUT[input] = ValidateOutputValue("r", csv.GetField<double>("r"), line);
                        vm.GreenLUT[input] = ValidateOutputValue("g", csv.GetField<double>("g"), line);
                        vm.BlueLUT[input] = ValidateOutputValue("b", csv.GetField<double>("b"), line);
                    }
                    vm.NotifyLUTChange();
                }
                catch (ArgumentException)
                {
                    MessageBox.Show(this, "Not bound to a EditTransferViewModel", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (FileFormatException exc)
                {
                    MessageBox.Show(this, "Incorrect file format: " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(this, "Failed to read file: " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        internal record class LutRecord(int input, int r, int g, int b);

        private void SaveCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TransferViewModel;
            if (vm == null)
            {
                MessageBox.Show(this, "Not bound to a EditTransferViewModel", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var savedlg = new SaveFileDialog();
            
            savedlg.Filter = "CSV file|*.csv";
            if (savedlg.ShowDialog(this).GetValueOrDefault(false))
            {
                using var writer = new StreamWriter(savedlg.FileName, false, new UTF8Encoding(false));
                using var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(Enumerable.Range(0, 256).Select(i => new LutRecord(input: i, r: vm.RedLUT[i], g: vm.GreenLUT[i], b: vm.BlueLUT[i])));
            }
        }
    }
}
