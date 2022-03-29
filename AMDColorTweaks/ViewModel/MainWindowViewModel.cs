using AMDColorTweaks.ADL;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMDColorTweaks.ViewModel
{
    internal class DisplayItem
    {
        public string Name { get; init; }
        public string AdapterName { get; init; }
        public int AdapterId { get; init; }
        public int DisplayId { get; init; }
        public string OSDisplayName { get; init; }
        public ADLDisplayID AdlId { get; init; }
        public DisplayItem(in AdapterInfo adapterInfo, in ADLDisplayInfo displayInfo)
        {
            var dispname = displayInfo.strDisplayName ?? "<Unknown Display>";
            var dispid = displayInfo.displayID.iDisplayLogicalIndex;
            Name = dispname;
            AdapterId = adapterInfo.iAdapterIndex;
            AdapterName = adapterInfo.strAdapterName;
            OSDisplayName = adapterInfo.strDisplayName;
            DisplayId = dispid;
            AdlId = displayInfo.displayID;
            //AvailiableDisplays.Add(new(dispname, adapterid, adaptername, dispid, lpAdapterInfo[i].strDisplayName, dispinfo.displayID));
        }
        public override string ToString()
        {
            return $"{OSDisplayName} - {Name} ({DisplayId}) on {AdapterName} ({AdapterId})";
        }

        public (bool Supported, bool Enabled) GetHdrStatus(ADLContext context)
        {
            ADLContext.RaiseForError(ADLNative.ADL2_Display_HDRState_Get(context, AdapterId, AdlId, out var supportHdr, out var enableHdr));
            return (supportHdr != 0, enableHdr != 0);
        }

        public GamutViewModel GetSourceGamut(ADLContext context)
        {
            ADLContext.RaiseForError(ADLNative.ADL2_Display_Gamut_Get(context, AdapterId, DisplayId, ADLGamutReference.SourceGraphics, out var srcgamutdata));
            return GamutViewModel.FromADL(srcgamutdata);
        }

        public GamutViewModel GetDestinationGamut(ADLContext context)
        {
            ADLContext.RaiseForError(ADLNative.ADL2_Display_Gamut_Get(context, AdapterId, DisplayId, ADLGamutReference.DestinationGraphics, out var dstgamutdata));
            return GamutViewModel.FromADL(dstgamutdata);
        }

        public TransferViewModel GetOutputTransfer(ADLContext context)
        {
            var regamma = new ADLRegammaEx();
            ADLContext.RaiseForError(ADLNative.ADL2_Display_RegammaR1_Get(context, AdapterId, DisplayId, ref regamma));
            return TransferViewModel.FromADLRegammaEx(regamma);
        }
    }
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<DisplayItem> AvailiableDisplays { get; } = new ObservableCollection<DisplayItem>();

        [OnChangedMethod(nameof(OnCurrentDisplayChanged))]
        public DisplayItem? CurrentDisplay { get; set; }

        public bool IsCurrentDisplayHDR { get; set; }
        public bool UseSourceSetting { get; set; } = true;
        public bool UseDestinationSetting { get; set; } = true;

        public GamutViewModel? CurrentSourceViewModel { get; set; }

        public GamutViewModel? CurrentDestinationViewModel { get; set; }

        public bool UseRegamma { get; set; } = true;
        public bool MergeVcgtIntoTrc { get; set; } = true;

        public TransferViewModel TransferSetting { get; set; } = new TransferViewModel();


        private void OnCurrentDisplayChanged()
        {
            ReloadCurrentDisplay();
        }

        public void ReloadCurrentDisplay()
        {
            var display = CurrentDisplay;
            if (display == null) return;
            using var adlContext = new ADLContext(true);
            IsCurrentDisplayHDR = display.GetHdrStatus(adlContext).Enabled;
            UseSourceSetting = UseDestinationSetting = !IsCurrentDisplayHDR;
            if (!IsCurrentDisplayHDR)
            {
                try
                {
                    CurrentSourceViewModel = display.GetSourceGamut(adlContext);
                    CurrentDestinationViewModel = display.GetDestinationGamut(adlContext);
                    TransferSetting = display.GetOutputTransfer(adlContext);
                }
                catch
                {
                    CurrentSourceViewModel = new();
                    CurrentDestinationViewModel = new();
                }
                CurrentDestinationViewModel.IsDestinationSetting = true;
            }

        }

        public void RefreshDisplays()
        {
            using var adlContext = new ADLContext(true);
            AvailiableDisplays.Clear();
            ADLNative.ADL2_Adapter_NumberOfAdapters_Get(adlContext, out var iNumberAdapters);
            var lpAdapterInfo = new AdapterInfo[iNumberAdapters];
            ADLNative.ADL2_Adapter_AdapterInfo_Get(adlContext, lpAdapterInfo, Marshal.SizeOf<AdapterInfo>() * iNumberAdapters);
            for (int i = 0; i < iNumberAdapters; i++)
            {
                var adapterid = lpAdapterInfo[i].iAdapterIndex;
                ADLNative.ADL2_Adapter_Active_Get(adlContext, adapterid, out var active);
                if (active == 0) continue;
                var adaptername = lpAdapterInfo[i].strAdapterName ?? "<Unknown Adapter>";
                var lpAdlDisplayInfo = IntPtr.Zero;
                var iNumDisplays = 0;
                try
                {
                    ADLNative.ADL2_Display_DisplayInfo_Get(adlContext, adapterid, out iNumDisplays, out lpAdlDisplayInfo, 0);
                    for (int j = 0; j < iNumDisplays; j++)
                    {
                        var dispinfo = Marshal.PtrToStructure<ADLDisplayInfo>(IntPtr.Add(lpAdlDisplayInfo, j * Marshal.SizeOf<ADLDisplayInfo>()));
                        if (dispinfo.iDisplayInfoMask.HasFlag(ADLDisplayInfoFlags.ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED) && dispinfo.iDisplayInfoValue.HasFlag(ADLDisplayInfoFlags.ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED))
                        {
                            var dispname = dispinfo.strDisplayName ?? "<Unknown Display>";
                            var dispid = dispinfo.displayID.iDisplayLogicalIndex;
                            AvailiableDisplays.Add(new(lpAdapterInfo[i], dispinfo));
                        }
                    }
                }
                finally
                {
                    if (lpAdlDisplayInfo != IntPtr.Zero) Marshal.FreeHGlobal(lpAdlDisplayInfo);
                }
            }
        }

    }
}
