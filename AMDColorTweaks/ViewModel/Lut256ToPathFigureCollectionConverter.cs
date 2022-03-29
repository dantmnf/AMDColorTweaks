using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace AMDColorTweaks.ViewModel
{
    internal class Lut256ToPathFigureCollectionConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // enum to checked
            if (value is ushort[] lut && lut.Length == 256)
            {
                var fig = new PathFigure();
                fig.IsClosed = false;
                fig.IsFilled = false;
                fig.StartPoint = new(0, lut[0] / 65535.0);
                var seg = new PolyLineSegment(lut.Select((value, index) => new System.Windows.Point(index / 255.0, value / 65535.0)), true);
                fig.Segments.Add(seg);
                var col = new PathFigureCollection();
                col.Add(fig);
                return col;
            }
            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
