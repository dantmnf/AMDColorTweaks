using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace AMDColorTweaks.ADL
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AdapterInfo {
        /// Size of the structure.
        public int iSize;
        /// The ADL index handle. One GPU may be associated with one or two index handles
        public int iAdapterIndex;
        /// The unique device ID associated with this adapter.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADLConst.ADL_MAX_PATH)]
        public string strUDID;
        /// The BUS number associated with this adapter.
        public int iBusNumber;
        /// The driver number associated with this adapter.
        public int iDeviceNumber;
        /// The function number.
        public int iFunctionNumber;
        /// The vendor ID associated with this adapter.
        public int iVendorID;
        /// Adapter name.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADLConst.ADL_MAX_PATH)]
        public string strAdapterName;
        /// Display name. For example, "\\\\Display0" for Windows or ":0:0" for Linux.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADLConst.ADL_MAX_PATH)]
        public string strDisplayName;
        /// Present or not; 1 if present and 0 if not present.It the logical adapter is present, the display name such as \\\\.\\Display1 can be found from OS
        public int iPresent;
        /// Exist or not; 1 is exist and 0 is not present.
        public int iExist;
        /// Driver registry path.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADLConst.ADL_MAX_PATH)]
        public string strDriverPath;
        /// Driver registry path Ext for.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADLConst.ADL_MAX_PATH)]
        public string strDriverPathExt;
        /// PNP string from Windows.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADLConst.ADL_MAX_PATH)]
        public string strPNPString;
        /// It is generated from EnumDisplayDevices.
        public int iOSDisplayIndex;

    }
    
    public struct ADLDisplayID
    {
        /// <summary>The logical display index belonging to this adapter.</summary>
        public int iDisplayLogicalIndex;

        /// <summary>
        /// The physical display index.
        /// For example, display index 2 from adapter 2 can be used by current adapter 1.
        /// So current adapter may enumerate this adapter as logical display 7 but the physical display
        /// index is still 2.
        /// </summary>
        public int iDisplayPhysicalIndex;

        /// <summary>The persistent logical adapter index for the display.</summary>
        public int iDisplayLogicalAdapterIndex;

        /// <summary>
        /// The persistent physical adapter index for the display.
        /// It can be the current adapter or a non-local adapter. \n
        /// If this adapter index is different than the current adapter,
        /// the Display Non Local flag is set inside DisplayInfoValue.
        /// </summary>
        public int iDisplayPhysicalAdapterIndex;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ADLDisplayInfo {
        /// <summary>
        /// The DisplayID structure. 
        /// </summary>
        public ADLDisplayID displayID;

        /// <summary>The controller index to which the display is mapped. Will not be used in the future</summary>
        [Obsolete]
        public int iDisplayControllerIndex;

        /// The display's EDID name.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADLConst.ADL_MAX_PATH)]
        public string strDisplayName;

        /// The display's manufacturer name.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADLConst.ADL_MAX_PATH)]
        public string strDisplayManufacturerName;

        /// The Display type. For example: CRT, TV, CV, DFP.
        public int iDisplayType;

        /// The display output type. For example: HDMI, SVIDEO, COMPONMNET VIDEO.
        public int iDisplayOutputType;

        /// The connector type for the device.
        public int iDisplayConnector;

        ///\brief The bit mask identifies the number of bits ADLDisplayInfo is currently using. \n
        /// It will be the sum all the bit definitions in ADL_DISPLAY_DISPLAYINFO_xxx.
        public ADLDisplayInfoFlags iDisplayInfoMask;

        /// The bit mask identifies the display status. \ref define_displayinfomask
        public ADLDisplayInfoFlags iDisplayInfoValue;
    }

    public struct ADLGamutInfo
    {
        public ADLGamutSpace SupportedGamutSpace;
        public ADLWhitePoint SupportedWhitePoint;
    }

    public struct ADLPoint
    {
        /// x coordinate
        public int iX;
        /// y coordinate
        public int iY;
    }

    public struct ADLGamutCoordinates
    {
        /// red channel chromasity coordinate
        public ADLPoint Red;
        /// green channel chromasity coordinate
        public ADLPoint Green;
        /// blue channel chromasity coordinate
        public ADLPoint Blue;

    }


    public struct ADLGamutData {
        public ADLGamutFeature iFeature;
        public ADLGamutSpace iPredefinedGamut;
        public ADLWhitePoint iPredefinedWhitePoint;
        public ADLPoint CustomWhitePoint;
        public ADLGamutCoordinates CustomGamut;
    }


    // from https://github.com/search?q=ADLRegammaEx&type=code
    // we believe that AMD has published this before.
    public struct ADL_Display_RegammaCoeffEx
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 3)]
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A0_DIVIDER
        public int[] CoefficientA0;
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A1A2A3_DIVIDER
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 3)]
        public int[] CoefficientA1;
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A1A2A3_DIVIDER
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 3)]
        public int[] CoefficientA2;
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A1A2A3_DIVIDER
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 3)]
        public int[] CoefficientA3;
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A1A2A3_DIVIDER
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 3)]
        public int[] Gamma;
    }

    public unsafe struct ADL_Display_RegammaCoeffEx2
    {
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A0_DIVIDER
        public fixed int CoefficientA0[3];
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A1A2A3_DIVIDER
        public fixed int CoefficientA1[3];
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A1A2A3_DIVIDER
        public fixed int CoefficientA2[3];
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A1A2A3_DIVIDER
        public fixed int CoefficientA3[3];
        /// uses divider defined in adl_defines.h: ADL_REGAMMA_COEFFICIENT_A1A2A3_DIVIDER
        public fixed int Gamma[3];
    }

    [Flags]
    public enum ADLRegammaExFeature : int
    {
        ///describes whether the coefficients are from EDID or custom user values.
        ADL_EDID_REGAMMA_COEFFICIENTS = 1 << 0,
        ///Used for struct ADLRegamma. Feature if set use gamma ramp, if missing use regamma coefficents
        ADL_USE_GAMMA_RAMP = 1 << 4,
        ///Used for struct ADLRegamma. If the gamma ramp flag is used then the driver could apply de gamma corretion to the supplied curve and this depends on this flag
        ADL_APPLY_DEGAMMA = 1 << 5,
        ///specifies that standard SRGB gamma should be applied
        ADL_EDID_REGAMMA_PREDEFINED_SRGB = 1 << 1,
        ///specifies that PQ gamma curve should be applied
        ADL_EDID_REGAMMA_PREDEFINED_PQ = 1 << 2,
        ///specifies that PQ gamma curve should be applied, lower max nits
        ADL_EDID_REGAMMA_PREDEFINED_PQ_2084_INTERIM = 1 << 3,
        ///specifies that 3.6 gamma should be applied
        ADL_EDID_REGAMMA_PREDEFINED_36 = 1 << 6,
        ///specifies that BT709 gama should be applied
        ADL_EDID_REGAMMA_PREDEFINED_BT709 = 1 << 7,
        ///specifies that regamma should be disabled, and application controls regamma content (of the whole screen)
        ADL_EDID_REGAMMA_PREDEFINED_APPCTRL = 1 << 8,
    }

    public unsafe struct ADLRegammaEx
    {
        public ADLRegammaExFeature Feature = default;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = 256*3)]
        public ushort[] gamma = new ushort[256*3];
        public ADL_Display_RegammaCoeffEx coefficients = default;

        public ADLRegammaEx() { }
    }
}
