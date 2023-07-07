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

        public bool ShowError { get; set; } = false;
        public string ErrorMessage { get; set; } = "";

        public TransferViewModel TransferSetting { get; set; } = new TransferViewModel();


        private void OnCurrentDisplayChanged()
        {
            ReloadCurrentDisplay();
        }

        public void ReloadCurrentDisplay()
        {
            ShowError = false;
            var display = CurrentDisplay;
            if (display == null) return;
            try
            {
                using var adlContext = new ADLContext(true);
                IsCurrentDisplayHDR = display.GetHdrStatus(adlContext).Enabled;
                UseSourceSetting = UseDestinationSetting = !IsCurrentDisplayHDR;
                if (!IsCurrentDisplayHDR)
                {
                    CurrentSourceViewModel = display.GetSourceGamut(adlContext);
                    CurrentDestinationViewModel = display.GetDestinationGamut(adlContext);
                    TransferSetting = display.GetOutputTransfer(adlContext);
                    CurrentDestinationViewModel.IsDestinationSetting = true;
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                CurrentSourceViewModel = new();
                CurrentDestinationViewModel = new();
            }
        }

        public void RefreshDisplays()
        {
            AvailiableDisplays.Clear();
            var dedupFilter = new HashSet<(int, int)>();
            try
            {
                using var adlContext = new ADLContext(true);
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
                                var dispid = dispinfo.displayID.iDisplayPhysicalIndex;
                                var logicalAdpId = dispinfo.displayID.iDisplayLogicalAdapterIndex;
                                if (logicalAdpId < 0 || logicalAdpId >= iNumberAdapters)
                                {
                                    logicalAdpId = dispinfo.displayID.iDisplayPhysicalAdapterIndex;
                                }
                                var dedupkey = (dispinfo.displayID.iDisplayPhysicalAdapterIndex, dispinfo.displayID.iDisplayPhysicalIndex);
                                if (dedupFilter.Contains(dedupkey)) { continue; }
                                dedupFilter.Add(dedupkey);
                                AvailiableDisplays.Add(new(lpAdapterInfo[dispinfo.displayID.iDisplayPhysicalAdapterIndex], lpAdapterInfo[logicalAdpId], dispinfo));
                            }
                        }
                    }
                    finally
                    {
                        if (lpAdlDisplayInfo != IntPtr.Zero) Marshal.FreeHGlobal(lpAdlDisplayInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }

        public void SetError(string message)
        {
            ShowError = true;
            ErrorMessage = message;
        }
    }
}