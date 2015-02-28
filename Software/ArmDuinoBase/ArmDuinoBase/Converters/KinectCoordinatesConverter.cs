using ArmDuinoBase.ViewModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArmDuinoBase.Converters
{
    public class KinectCoordinatesConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (MainViewModel.Current.KinectHandler.Sensor == null) return 0;
            CoordinateMapper mapper = MainViewModel.Current.KinectHandler.Sensor.CoordinateMapper;
            SkeletonPoint position = (SkeletonPoint)value;
            var point = mapper.MapSkeletonPointToColorPoint(position, MainViewModel.Current.KinectHandler.Sensor.ColorStream.Format);
            if(parameter.Equals("X"))
            {
                return point.X - MainViewModel.Current.KinectGestureProcessor.EllipseSize / 2;
            }
            else if(parameter.Equals("Y"))
            {
                return point.Y - MainViewModel.Current.KinectGestureProcessor.EllipseSize / 2;
            }
            else return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
