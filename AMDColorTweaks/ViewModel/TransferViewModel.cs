using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AMDColorTweaks.ADL;
using AMDColorTweaks.Util;
using PropertyChanged;

namespace AMDColorTweaks.ViewModel
{
    internal enum TransferType
    {
        DriverPredefined,
        ParametricOETF,
        ChannelLUT
    }

    internal class ParametricCurveViewModel : INotifyPropertyChanged
    {
        public string Channel { get; init; } = "Unknown";
        public double A0 { get; set; }
        public double A1 { get; set; }
        public double A2 { get; set; }
        public double A3 { get; set; }
        public double Gamma { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void CopyTo(ParametricCurveViewModel other)
        {
            other.A0 = A0;
            other.A1 = A1;
            other.A2 = A2;
            other.A3 = A3;
            other.Gamma = Gamma;
        }

        public double Evaluate(double L)
        {
            return L < A0
                ? A1 * L
                : (1 + A2) * Math.Pow(L, 1 / Gamma) - A3;
        }

        public (double, double) Stat()
        {
            var values = Enumerable.Range(0, 256).Select(i => Evaluate(i / 255.0)).ToArray();
            var mean = values.Average();
            var stdev = Math.Sqrt(values.Sum(x => Math.Pow(x - mean, 2)) / values.Length);
            return (values.Max() - values.Min(), stdev);
        }

    }

    internal class TransferViewModel : INotifyPropertyChanged
    {

        //public static EnumWithDescription<ADL.ADLRegammaExFeature>[] AllDriverPredefinedTransfer = new EnumWithDescription<ADL.ADLRegammaExFeature>[] {
        //    new(ADLRegammaExFeature.ADL_EDID_REGAMMA_COEFFICIENTS, "Use EDID gamma"),
        //    new(ADLRegammaExFeature.ADL_EDID_REGAMMA_PREDEFINED_SRGB, "sRGB"),
        //    new(ADLRegammaExFeature.ADL_EDID_REGAMMA_PREDEFINED_PQ, "SMPTE ST 2084 (PQ)"),
        //    new(ADLRegammaExFeature.ADL_EDID_REGAMMA_PREDEFINED_PQ_2084_INTERIM, "SMPTE ST 2084 (PQ), lower max nits"),
        //    new(ADLRegammaExFeature.ADL_EDID_REGAMMA_PREDEFINED_36, "Gamma 3.6"),
        //    new(ADLRegammaExFeature.ADL_EDID_REGAMMA_PREDEFINED_BT709, "Rec. 709"),
        //    new(ADLRegammaExFeature.ADL_EDID_REGAMMA_PREDEFINED_APPCTRL, "Application controlled (linear)"),
        //};

        public event PropertyChangedEventHandler? PropertyChanged;

        public TransferType TransferType { get; set; }

        //public ICollection<EnumWithDescriptionAndChecked<ADLRegammaExFeature>> AvailiablePredefined { get; } = new ObservableCollection<EnumWithDescriptionAndChecked<ADLRegammaExFeature>>();

        //public ADLRegammaExFeature PredefinedTransfer { get; set; }
        public bool UseUniformParametricCurve {
            get => object.ReferenceEquals(uniformCurves, ParametricCurves);
            set { ParametricCurves = value ? uniformCurves : rgbCurves; }
        }
        public ParametricCurveViewModel RedCurve { get; } = new() { Channel = "Red" };
        public ParametricCurveViewModel GreenCurve { get; } = new() { Channel = "Green" };
        public ParametricCurveViewModel BlueCurve { get; } = new() { Channel = "Blue" };
        public ParametricCurveViewModel UniformCurve { get; } = new() { Channel = "All" };

        private ParametricCurveViewModel[] rgbCurves;
        private ParametricCurveViewModel[] uniformCurves;


        public ICollection<ParametricCurveViewModel> ParametricCurves { get; private set; }


        public bool UseDegamma { get; set; }

        public ushort[] RedLUT { get; } = new ushort[256];
        public ushort[] GreenLUT { get; } = new ushort[256];
        public ushort[] BlueLUT { get; } = new ushort[256];

        private static readonly ushort[] linearLUT = Enumerable.Range(0, 256).Select(i => (ushort)((i << 8) | i)).ToArray();
        public static ReadOnlySpan<ushort> LinearLUT => linearLUT;

        public TransferViewModel()
        {
            rgbCurves = new[] { RedCurve, GreenCurve, BlueCurve };
            uniformCurves = new[] { UniformCurve };
            ParametricCurves = uniformCurves;
            //SetAvailiablePredefinedTransform();
        }

        //private void SetAvailiablePredefinedTransform()
        //{
        //    AvailiablePredefined.Clear();
        //    foreach(var item in AllDriverPredefinedTransfer)
        //    {
        //        var subvm = new EnumWithDescriptionAndChecked<ADLRegammaExFeature>(item.Value, item.Description);
        //        subvm.IsChecked = item.Value == PredefinedTransfer;
        //        subvm.PropertyChanged += (sender, args) =>
        //        {
        //            var sender2 = (EnumWithDescriptionAndChecked<ADLRegammaExFeature>)sender;
        //            if (args.PropertyName == "IsChecked")
        //            {
        //                if (sender2.IsChecked)
        //                {
        //                    PredefinedTransfer = sender2.Value;
        //                    foreach (var item2 in AvailiablePredefined)
        //                    {
        //                        if (!Object.ReferenceEquals(item2, sender))
        //                        {
        //                            item2.IsChecked = false;
        //                        }
        //                    }
        //                }
        //            }
        //        };
        //        AvailiablePredefined.Add(subvm);
        //    }
        //}

        public void NotifyLUTChange()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RedLUT)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GreenLUT)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlueLUT)));
        }

        public static TransferViewModel FromADLRegammaEx(ADLRegammaEx x)
        {
            var result = new TransferViewModel();
            var uniform = true;
            uniform &= ArrayHelper.AllEqual(x.coefficients.CoefficientA0);
            uniform &= ArrayHelper.AllEqual(x.coefficients.CoefficientA1);
            uniform &= ArrayHelper.AllEqual(x.coefficients.CoefficientA2);
            uniform &= ArrayHelper.AllEqual(x.coefficients.CoefficientA3);
            uniform &= ArrayHelper.AllEqual(x.coefficients.Gamma);

            result.UseUniformParametricCurve = uniform;

            result.UniformCurve.A0 = x.coefficients.CoefficientA0[0] / 10000000.0;
            result.UniformCurve.A1 = x.coefficients.CoefficientA1[0] / 1000.0;
            result.UniformCurve.A2 = x.coefficients.CoefficientA2[0] / 1000.0;
            result.UniformCurve.A3 = x.coefficients.CoefficientA3[0] / 1000.0;
            result.UniformCurve.Gamma = x.coefficients.Gamma[0] / 1000.0;

            result.RedCurve.A0 = x.coefficients.CoefficientA0[0] / 10000000.0;
            result.RedCurve.A1 = x.coefficients.CoefficientA1[0] / 1000.0;
            result.RedCurve.A2 = x.coefficients.CoefficientA2[0] / 1000.0;
            result.RedCurve.A3 = x.coefficients.CoefficientA3[0] / 1000.0;
            result.RedCurve.Gamma = x.coefficients.Gamma[0] / 1000.0;

            result.GreenCurve.A0 = x.coefficients.CoefficientA0[1] / 10000000.0;
            result.GreenCurve.A1 = x.coefficients.CoefficientA1[1] / 1000.0;
            result.GreenCurve.A2 = x.coefficients.CoefficientA2[1] / 1000.0;
            result.GreenCurve.A3 = x.coefficients.CoefficientA3[1] / 1000.0;
            result.GreenCurve.Gamma = x.coefficients.Gamma[1] / 1000.0;

            result.BlueCurve.A0 = x.coefficients.CoefficientA0[2] / 10000000.0;
            result.BlueCurve.A1 = x.coefficients.CoefficientA1[2] / 1000.0;
            result.BlueCurve.A2 = x.coefficients.CoefficientA2[2] / 1000.0;
            result.BlueCurve.A3 = x.coefficients.CoefficientA3[2] / 1000.0;
            result.BlueCurve.Gamma = x.coefficients.Gamma[2] / 1000.0;

            x.gamma.AsSpan().Slice(0, 256).CopyTo(result.RedLUT);
            x.gamma.AsSpan().Slice(256, 256).CopyTo(result.GreenLUT);
            x.gamma.AsSpan().Slice(512, 256).CopyTo(result.BlueLUT);

            if (false)
            {
                // prepare some reasonable value if all zero
                x.coefficients.CoefficientA0.AsSpan().Fill(31308);
                x.coefficients.CoefficientA1.AsSpan().Fill(12920);
                x.coefficients.CoefficientA2.AsSpan().Fill(55);
                x.coefficients.CoefficientA3.AsSpan().Fill(55);
                x.coefficients.Gamma.AsSpan().Fill(2400);
                linearLUT.CopyTo(result.RedLUT, 0);
                linearLUT.CopyTo(result.GreenLUT, 0);
                linearLUT.CopyTo(result.BlueLUT, 0);
            }

            if (x.Feature == 0){
                // use parametric curves
                result.TransferType = TransferType.ParametricOETF;
            }
            else if (x.Feature.HasFlag(ADLRegammaExFeature.ADL_USE_GAMMA_RAMP))
            {
                result.TransferType = TransferType.ChannelLUT;
                if (x.Feature.HasFlag(ADLRegammaExFeature.ADL_APPLY_DEGAMMA))
                {
                    result.UseDegamma = true;
                }
            }
            else
            {
                result.TransferType = TransferType.DriverPredefined;
                //result.PredefinedTransfer = x.Feature;
            }
            return result;
        }


        private static void SetCoefficient(ref ADLRegammaEx x, ParametricCurveViewModel RedCurve, ParametricCurveViewModel GreenCurve, ParametricCurveViewModel BlueCurve)
        {
            x.coefficients.CoefficientA0 = new[] { (int)Math.Round(RedCurve.A0 * 10000000.0), (int)Math.Round(GreenCurve.A0 * 10000000.0), (int)Math.Round(BlueCurve.A0 * 10000000.0) };
            x.coefficients.CoefficientA1 = new[] { (int)Math.Round(RedCurve.A1 * 1000.0), (int)Math.Round(GreenCurve.A1 * 1000.0), (int)Math.Round(BlueCurve.A1 * 1000.0) };
            x.coefficients.CoefficientA2 = new[] { (int)Math.Round(RedCurve.A2 * 1000.0), (int)Math.Round(GreenCurve.A2 * 1000.0), (int)Math.Round(BlueCurve.A2 * 1000.0) };
            x.coefficients.CoefficientA3 = new[] { (int)Math.Round(RedCurve.A3 * 1000.0), (int)Math.Round(GreenCurve.A3 * 1000.0), (int)Math.Round(BlueCurve.A3 * 1000.0) };
            x.coefficients.CoefficientA3 = new[] { (int)Math.Round(RedCurve.A3 * 1000.0), (int)Math.Round(GreenCurve.A3 * 1000.0), (int)Math.Round(BlueCurve.A3 * 1000.0) };
            x.coefficients.Gamma = new[] { (int)Math.Round(RedCurve.Gamma * 1000.0), (int)Math.Round(GreenCurve.Gamma * 1000.0), (int)Math.Round(BlueCurve.Gamma * 1000.0) };
        }

        public ADLRegammaEx ToADLRegammaEx()
        {
            var result = new ADLRegammaEx();
            if (UseUniformParametricCurve)
            {
                SetCoefficient(ref result, UniformCurve, UniformCurve, UniformCurve);
            }
            else
            {
                SetCoefficient(ref result, RedCurve, GreenCurve, BlueCurve);
            }
            RedLUT.CopyTo(result.gamma, 0);
            GreenLUT.CopyTo(result.gamma, 256);
            BlueLUT.CopyTo(result.gamma, 512);

            if (TransferType == TransferType.DriverPredefined)
            {
                //result.Feature = PredefinedTransfer;
            }
            else if (TransferType == TransferType.ParametricOETF)
            {
                result.Feature = 0;
            }
            else if (TransferType == TransferType.ChannelLUT)
            {
                result.Feature = ADLRegammaExFeature.ADL_USE_GAMMA_RAMP;
            }
            if (UseDegamma)
            {
                result.Feature |= ADLRegammaExFeature.ADL_APPLY_DEGAMMA;
            }
            return result;
        }

        public override string ToString()
        {
            if (TransferType == TransferType.ParametricOETF)
            {
                // parameteric curve
                return DescribeParametericCurve();
            }
            else if (TransferType == TransferType.ChannelLUT)
            {
                return "Channel LUT";
            }
            else
            {
                //var value = EnumWithDescription<ADLRegammaExFeature>.FindDescription(PredefinedTransfer, AllDriverPredefinedTransfer).Description;
                //if (string.IsNullOrEmpty(value)) value = "Other driver-defined transfer";
                //return value;
                return "Other";
            }
        }

        private static (double, double) StatLUT(ushort[] lut)
        {
            var values = Enumerable.Range(0, 256).Select(i => lut[i] / 65535.0 ).ToArray();
            var mean = values.Average();
            var stdev = Math.Sqrt(values.Sum(x => Math.Pow(x - mean, 2)) / values.Length);
            return (values.Max() - values.Min(), stdev);
        }

        public (double maxRange, double maxStdev) Stat()
        {
            var maxRange = 0.0;
            var maxStdev = 0.0;

            if (TransferType == TransferType.ParametricOETF)
            {
                if (UseUniformParametricCurve)
                {
                    return UniformCurve.Stat();
                }
                else
                {
                    var (range, stdev) = RedCurve.Stat();
                    maxRange = Math.Max(maxRange, range);
                    maxStdev = Math.Max(maxStdev, stdev);
                    (range, stdev) = GreenCurve.Stat();
                    maxRange = Math.Max(maxRange, range);
                    maxStdev = Math.Max(maxStdev, stdev);
                    (range, stdev) = BlueCurve.Stat();
                    maxRange = Math.Max(maxRange, range);
                    maxStdev = Math.Max(maxStdev, stdev);
                    return (maxRange, maxStdev);
                }
            }
            else if (TransferType == TransferType.ChannelLUT)
            {
                var (range, stdev) = StatLUT(RedLUT);
                maxRange = Math.Max(maxRange, range);
                maxStdev = Math.Max(maxStdev, stdev);
                (range, stdev) = StatLUT(GreenLUT);
                maxRange = Math.Max(maxRange, range);
                maxStdev = Math.Max(maxStdev, stdev);
                (range, stdev) = StatLUT(BlueLUT);
                maxRange = Math.Max(maxRange, range);
                maxStdev = Math.Max(maxStdev, stdev);
                return (maxRange, maxStdev);
            }
            return (maxRange, maxStdev);
        }

        private static bool Equal3(double a, double b, double c, double tolerance = 0.000001)
        {
            return Math.Abs(Math.Max(a,Math.Max(b,c)) - Math.Min(a, Math.Min(b,c))) < tolerance;
        }

        private bool IsUniformParam()
        {
            return Equal3(RedCurve.A0, GreenCurve.A0, BlueCurve.A0, 1e-8) &&
                   Equal3(RedCurve.A1, GreenCurve.A1, BlueCurve.A1, 1e-4) &&
                   Equal3(RedCurve.A2, GreenCurve.A2, BlueCurve.A2, 1e-4) &&
                   Equal3(RedCurve.A3, GreenCurve.A3, BlueCurve.A3, 1e-4) &&
                   Equal3(RedCurve.Gamma, GreenCurve.Gamma, BlueCurve.Gamma, 1e-4);
        }

        private bool CompareCurve(ParametricCurveViewModel curve, double A0, double A1, double A2, double A3, double gamma)
        {
            return Math.Abs(curve.A0 - A0) < 1e-8 &&
                Math.Abs(curve.A1 - A1) < 1e-4 &&
                Math.Abs(curve.A2 - A2) < 1e-4 &&
                Math.Abs(curve.A3 - A3) < 1e-4 &&
                Math.Abs(curve.Gamma - gamma) < 1e-4;
        }

        private string DescribeParametericCurve()
        {

            if (!IsUniformParam())
            {
                return "Non-uniform parameteric OETF";
            }
            var useCurve = UseUniformParametricCurve ? UniformCurve : RedCurve;
            if (CompareCurve(useCurve, 0.0031308, 12.92, 0.055, 0.055, 2.4))
            {
                return "sRGB";
            }
            if (CompareCurve(useCurve, 0.018, 4.5, 0.099, 0.099, 2.2))
            {
                return "Rec. 709 (AMD definition)";
            }
            if (useCurve.A0 == 0 && useCurve.A1 == 0 && useCurve.A2 == 0 && useCurve.A3 == 0)
            {
                return useCurve.Gamma switch
                {
                    0 => "Invalid setting",
                    1 => "Linear",
                    _ => $"Pure gamma {useCurve.Gamma:0.00}"
                };
            }
            return "Uniform parameteric OETF";
        }

        public TransferViewModel Clone()
        {
            var result = new TransferViewModel();
            result.TransferType = TransferType;
            //result.PredefinedTransfer = PredefinedTransfer;
            result.UseUniformParametricCurve = UseUniformParametricCurve;
            result.UseDegamma = UseDegamma;
            RedCurve.CopyTo(result.RedCurve);
            GreenCurve.CopyTo(result.GreenCurve);
            BlueCurve.CopyTo(result.BlueCurve);
            UniformCurve.CopyTo(result.UniformCurve);
            RedLUT.CopyTo(result.RedLUT, 0);
            GreenLUT.CopyTo(result.GreenLUT, 0);
            BlueLUT.CopyTo(result.BlueLUT, 0);
            return result;
        }
    }
}
