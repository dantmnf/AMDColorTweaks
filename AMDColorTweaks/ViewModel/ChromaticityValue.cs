using AMDColorTweaks.ADL;
using System;
using System.ComponentModel;

namespace AMDColorTweaks.ViewModel
{
    public class ChromaticityValue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public double X { get; set; }

        public double Y { get; set; }


        public void SetFromADLPoint(ADL.ADLPoint pt)
        {
            X = pt.iX / 10000.0;
            Y = pt.iY / 10000.0;
        }

        public ADLPoint ToADLPoint()
        {
            return new() { iX = (int)Math.Round(X * 10000), iY = (int)Math.Round(Y * 10000) };
        }
    }
}