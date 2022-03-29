using AMDColorTweaks.ADL;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMDColorTweaks.ViewModel
{
    internal partial class GamutViewModel : INotifyPropertyChanged
    {
        public static EnumWithDescription<ADLGamutSpace>.Collection GamutItems = new() {
            {ADLGamutSpace.ADL_GAMUT_SPACE_CIE_RGB, "Default" }, // ???
            {ADLGamutSpace.ADL_GAMUT_SPACE_CCIR_709, "sRGB / Rec. 709" },
            {ADLGamutSpace.ADL_GAMUT_SPACE_CCIR_601, "Rec. 601" },
            {ADLGamutSpace.ADL_GAMUT_SPACE_ADOBE_RGB, "Adobe RGB" },
            {ADLGamutSpace.ADL_GAMUT_SPACE_CCIR_2020, "Rec. 2020" },
            {ADLGamutSpace.ADL_GAMUT_SPACE_CUSTOM, "Custom" },
        };
        public static EnumWithDescription<ADLWhitePoint>.Collection WhitePointItems = new() {
            { ADLWhitePoint.ADL_WHITE_POINT_5000K, "D50" },
            { ADLWhitePoint.ADL_WHITE_POINT_6500K, "sRGB / D65" },
            { ADLWhitePoint.ADL_WHITE_POINT_7500K, "D75" },
            { ADLWhitePoint.ADL_WHITE_POINT_9300K, "D93" },
            { ADLWhitePoint.ADL_WHITE_POINT_CUSTOM, "Custom" }
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool UseCustomGamut { get; set; }
        public bool UseCustomWhitePoint { get; set; }
        public bool UseRelativeColorimetric { get; set; }
        public bool IsDestinationSetting { get; set; }

        private ADLGamutSpace predefinedGamut = ADLGamutSpace.ADL_GAMUT_SPACE_CCIR_709;
        public ADLGamutSpace PredefinedGamut { 
            get => predefinedGamut;
            set { 
                predefinedGamut = value;
                UseCustomGamut = value == ADLGamutSpace.ADL_GAMUT_SPACE_CUSTOM;
            }
        }

        private ADLWhitePoint predefinedWhitePoint = ADLWhitePoint.ADL_WHITE_POINT_6500K;
        public ADLWhitePoint PredefinedWhitePoint
        {
            get => predefinedWhitePoint;
            set
            {
                predefinedWhitePoint = value;
                UseCustomWhitePoint = value == ADLWhitePoint.ADL_WHITE_POINT_CUSTOM;
            }
        }

        public ChromaticityValue CustomRed { get; } = new ChromaticityValue();
        public ChromaticityValue CustomGreen { get; } = new ChromaticityValue();
        public ChromaticityValue CustomBlue { get; } = new ChromaticityValue();
        public ChromaticityValue CustomWhitePoint { get; } = new ChromaticityValue();


        public ADLGamutData ToADL()
        {
            var result = new ADLGamutData();
            result.iFeature = UseRelativeColorimetric ? ADLGamutFeature.ADL_GAMUT_REMAP_ONLY : 0;
            result.CustomGamut.Red = CustomRed.ToADLPoint();
            result.CustomGamut.Green = CustomGreen.ToADLPoint();
            result.CustomGamut.Blue = CustomBlue.ToADLPoint();
            result.CustomWhitePoint = CustomWhitePoint.ToADLPoint();
            result.iPredefinedGamut = PredefinedGamut;
            if (PredefinedGamut == ADLGamutSpace.ADL_GAMUT_SPACE_CUSTOM)
            {
                result.iFeature |= ADLGamutFeature.ADL_CUSTOM_GAMUT;
            }
            result.iPredefinedWhitePoint = PredefinedWhitePoint;
            if (PredefinedWhitePoint == ADLWhitePoint.ADL_WHITE_POINT_CUSTOM)
            {
                result.iFeature |= ADLGamutFeature.ADL_CUSTOM_WHITE_POINT;
            }
            return result;
        }

        public static GamutViewModel FromADL(ADLGamutData value)
        {
            var result = new GamutViewModel();
            result.PredefinedWhitePoint = value.iPredefinedWhitePoint;
            result.PredefinedGamut = value.iPredefinedGamut;
            if (value.iFeature.HasFlag(ADLGamutFeature.ADL_CUSTOM_GAMUT))
            {
                result.UseCustomGamut = true;
                result.PredefinedGamut = ADLGamutSpace.ADL_GAMUT_SPACE_CUSTOM;
            }
            if (value.iFeature.HasFlag(ADLGamutFeature.ADL_CUSTOM_WHITE_POINT))
            {
                result.UseCustomWhitePoint = true;
                result.PredefinedWhitePoint = ADLWhitePoint.ADL_WHITE_POINT_CUSTOM;
            }
            result.UseRelativeColorimetric = value.iFeature.HasFlag(ADLGamutFeature.ADL_GAMUT_REMAP_ONLY);
            result.CustomRed.SetFromADLPoint(value.CustomGamut.Red);
            result.CustomGreen.SetFromADLPoint(value.CustomGamut.Green);
            result.CustomBlue.SetFromADLPoint(value.CustomGamut.Blue);
            result.CustomWhitePoint.SetFromADLPoint(value.CustomWhitePoint);
            return result;
        }

    }
}