using AMDColorTweaks.ADL;

namespace AMDColorTweaks.ViewModel
{
    internal class DisplayItem
    {
        public string Name { get; init; }
        public string AdapterName { get; init; }
        public int AdapterId { get; init; }
        public int DisplayId { get; init; }
        public ADLDisplayID AdlId { get; init; }

        private string description;

        public DisplayItem(in AdapterInfo physicalAdapter, in AdapterInfo logicalAdapter, in ADLDisplayInfo displayInfo)
        {
            Name = displayInfo.strDisplayName ?? "<Unknown Display>";
            AdapterId = physicalAdapter.iAdapterIndex;
            AdapterName = physicalAdapter.strAdapterName;
            DisplayId = displayInfo.displayID.iDisplayPhysicalIndex;
            AdlId = displayInfo.displayID;

            // logical adapter is used only for Windows display ID
            string idstr;
            if (displayInfo.displayID.iDisplayLogicalAdapterIndex == -1)
            {
                idstr = "Inactive";
            }
            else
            {
                idstr = $"Display {logicalAdapter.iOSDisplayIndex + 1}";
            }
            description = $"{idstr}: {Name} ({GetConnectorType(displayInfo.iDisplayConnector)}) on {AdapterName}";
        }
        public override string ToString() => description;

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

        private static string GetConnectorType(int iConnectorType) => iConnectorType switch
        {
            1 => "VGA",
            2 => "DVI-D",
            3 => "DVI-I",
            10 => "HDMI",
            15 => "DisplayPort",
            16 => "eDP",
            17 => "Wireless",
            18 => "USB Type-C",
            _ => "Unknown"
        };
    }
}