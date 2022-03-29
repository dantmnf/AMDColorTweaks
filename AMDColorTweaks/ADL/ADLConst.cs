using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMDColorTweaks.ADL
{
    internal class ADLConst
    {
        public const int ADL_MAX_CHAR = 4096;
        public const int ADL_MAX_PATH = 256;
        public const int ADL_MAX_ADAPTERS = 250;
        public const int ADL_MAX_DISPLAYS = 150;
        public const int ADL_MAX_DEVICENAME = 32;
        public const int ADL_ADAPTER_INDEX_ALL = -1;

    }

    public enum ADLResultCode
    {
        /// All OK, but need to wait
        ADL_OK_WAIT = 4,
        /// All OK, but need restart
        ADL_OK_RESTART = 3,
        /// All OK but need mode change
        ADL_OK_MODE_CHANGE = 2,
        /// All OK, but with warning
        ADL_OK_WARNING = 1,
        /// ADL function completed successfully
        ADL_OK = 0,
        /// Generic Error. Most likely one or more of the Escape calls to the driver failed!
        ADL_ERR = -1,
        /// ADL not initialized
        ADL_ERR_NOT_INIT = -2,
        /// One of the parameter passed is invalid
        ADL_ERR_INVALID_PARAM = -3,
        /// One of the parameter size is invalid
        ADL_ERR_INVALID_PARAM_SIZE = -4,
        /// Invalid ADL index passed
        ADL_ERR_INVALID_ADL_IDX = -5,
        /// Invalid controller index passed
        ADL_ERR_INVALID_CONTROLLER_IDX = -6,
        /// Invalid display index passed
        ADL_ERR_INVALID_DIPLAY_IDX = -7,
        /// Function  not supported by the driver
        ADL_ERR_NOT_SUPPORTED = -8,
        /// Null Pointer error
        ADL_ERR_NULL_POINTER = -9,
        /// Call can't be made due to disabled adapter
        ADL_ERR_DISABLED_ADAPTER = -10,
        /// Invalid Callback
        ADL_ERR_INVALID_CALLBACK = -11,
        /// Display Resource conflict
        ADL_ERR_RESOURCE_CONFLICT = -12,
        //Failed to update some of the values. Can be returned by set request that include multiple values if not all values were successfully committed.
        ADL_ERR_SET_INCOMPLETE = -20,
        /// There's no Linux XDisplay in Linux Console environment
        ADL_ERR_NO_XDISPLAY = -21,
    }

    [Flags]
    public enum ADLDisplayInfoFlags : int
    {
        ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED = 0x00000001,
        ADL_DISPLAY_DISPLAYINFO_DISPLAYMAPPED = 0x00000002,
        ADL_DISPLAY_DISPLAYINFO_NONLOCAL = 0x00000004,
        ADL_DISPLAY_DISPLAYINFO_FORCIBLESUPPORTED = 0x00000008,
        ADL_DISPLAY_DISPLAYINFO_GENLOCKSUPPORTED = 0x00000010,
        ADL_DISPLAY_DISPLAYINFO_MULTIVPU_SUPPORTED = 0x00000020,
        ADL_DISPLAY_DISPLAYINFO_LDA_DISPLAY = 0x00000040,
        ADL_DISPLAY_DISPLAYINFO_MODETIMING_OVERRIDESSUPPORTED = 0x00000080,

        ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_SINGLE = 0x00000100,
        ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_CLONE = 0x00000200,

        /// Legacy support for XP
        ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2VSTRETCH = 0x00000400,
        ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2HSTRETCH = 0x00000800,
        ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_EXTENDED = 0x00001000,

        /// More = support manners
        ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCH1GPU = 0x00010000,
        ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCHNGPU = 0x00020000,
        ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED2 = 0x00040000,
        ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED3 = 0x00080000,

        /// Projector display type
        ADL_DISPLAY_DISPLAYINFO_SHOWTYPE_PROJECTOR = 0x00100000
    }

    [Flags]
    public enum ADLColorFlags : int
    {
        ADL_DISPLAY_COLOR_BRIGHTNESS = (1 << 0),
        ADL_DISPLAY_COLOR_CONTRAST = (1 << 1),
        ADL_DISPLAY_COLOR_HUE = (1 << 2),
        ADL_DISPLAY_COLOR_SATURATION = (1 << 3),
        ADL_DISPLAY_COLOR_TEMPERATURE = (1 << 4),
        ADL_DISPLAY_COLOR_TEMPERATURE_SOURCE_EDID = (1 << 5),
        ADL_DISPLAY_COLOR_TEMPERATURE_SOURCE_USER = (1 << 6)
    }

    [Flags]
    public enum ADLGamutSpace : int
    {
        [Description("Unknown")]
        UNKNOWN = 0,
        [Description("sRGB / Rec. 709")]
        ADL_GAMUT_SPACE_CCIR_709 = (1 << 0),
        [Description("Rec. 601")]
        ADL_GAMUT_SPACE_CCIR_601 = (1 << 1),
        [Description("Adobe RGB")]
        ADL_GAMUT_SPACE_ADOBE_RGB = (1 << 2),
        [Description("CIE RGB")]
        ADL_GAMUT_SPACE_CIE_RGB = (1 << 3),
        [Description("Custom")]
        ADL_GAMUT_SPACE_CUSTOM = (1 << 4),
        [Description("Rec. 2020")]
        ADL_GAMUT_SPACE_CCIR_2020 = (1 << 5),
        [Description("Application Controlled")]
        ADL_GAMUT_SPACE_APPCTRL = (1 << 6)
    }

    [Flags]
    public enum ADLWhitePoint : int
    {
        [Description("Unknown")]
        UNKNOWN = 0,
        [Description("5000K")]
        ADL_WHITE_POINT_5000K = (1 << 0),
        [Description("6500K")]
        ADL_WHITE_POINT_6500K = (1 << 1),
        [Description("7500K")]
        ADL_WHITE_POINT_7500K = (1 << 2),
        [Description("9300K")]
        ADL_WHITE_POINT_9300K = (1 << 3),
        [Description("Custom")]
        ADL_WHITE_POINT_CUSTOM = (1 << 4)
    }

    public enum ADLGamutReference : int
    {
        DestinationGraphics,
        SourceGraphics,
        DestinationOverlay,
        SourceOverlay
    }

    [Flags]
    public enum ADLGamutFeature
    {
        ADL_CUSTOM_WHITE_POINT = 1 << 0,
        ADL_CUSTOM_GAMUT = 1 << 1,
        ADL_GAMUT_REMAP_ONLY = 1 << 2,
    }


}
