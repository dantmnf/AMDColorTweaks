using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMDColorTweaks.ADL
{
    public delegate IntPtr ADL_MAIN_MALLOC_CALLBACK(int len);
    public unsafe class ADLNative
    {

        [DllImport("atiadlxx")]
        public static extern int ADL2_Main_Control_Create(ADL_MAIN_MALLOC_CALLBACK callback, int iEnumConnectedAdapters, out IntPtr context);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Main_Control_Destroy(IntPtr context);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Adapter_NumberOfAdapters_Get(IntPtr context, out int lpNumAdapters);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Adapter_AdapterInfo_Get(IntPtr context, [Out] AdapterInfo[] lpInfo, int iInputSize);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Adapter_Active_Get(IntPtr context, int iAdapterIndex, out int lpStatus);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Display_DisplayInfo_Get(IntPtr context, int iAdapterIndex, out int lpNumDisplays, out IntPtr lppInfo, int iForceDetect);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Display_ColorCaps_Get(IntPtr context, int iAdapterIndex, int iDisplayIndex, out ADLColorFlags lpCaps, out ADLColorFlags lpValids);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Display_Gamut_Caps(IntPtr context, int iAdapterIndex, int iDisplayIndex, ADLGamutReference gamut, out ADLGamutInfo lpCap);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Display_Gamut_Get(IntPtr context, int iAdapterIndex, int iDisplayIndex, ADLGamutReference gamut, out ADLGamutData lpSource);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Display_Gamut_Set(IntPtr context, int iAdapterIndex, int iDisplayIndex, ADLGamutReference gamut, in ADLGamutData lpSource);

        [DllImport("atiadlxx")]
        public static extern int ADL2_Display_RegammaR1_Get(IntPtr context, int iAdapterIndex, int iDisplayIndex, ref ADLRegammaEx regamma);
        [DllImport("atiadlxx")]
        public static extern int ADL2_Display_RegammaR1_Set(IntPtr context, int iAdapterIndex, int iDisplayIndex, in ADLRegammaEx regamma);
        [DllImport("atiadlxx")]
        public static extern int ADL2_Display_HDRState_Get(IntPtr context, int iAdapterIndex, ADLDisplayID iDisplayIndex, out int iSupport, out int iEnable);
    }
}
