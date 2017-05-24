using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BuildAllVSProjects.Models;

namespace BuildAllVSProjects.Converters
{
    public class BuildStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isBuildStatus = value as BuildSuccessStatus? ?? BuildSuccessStatus.Undefined;
            if (isBuildStatus != BuildSuccessStatus.Undefined)
            {
                switch (isBuildStatus)
                {
                    case BuildSuccessStatus.NotAttempted:
                        return Brushes.Transparent;
                    case BuildSuccessStatus.IsBuilding:
                        return Brushes.Yellow;
                    case BuildSuccessStatus.FailedOnLatest:
                        return Brushes.Red;
                    case BuildSuccessStatus.SucceededOnLatest:
                        return Brushes.LawnGreen;
                    case BuildSuccessStatus.FailedOnPrevious:
                        return Brushes.IndianRed;
                    case BuildSuccessStatus.SucceededOnPrevious:
                        return Brushes.OliveDrab;
                    case BuildSuccessStatus.Exception:
                        return Brushes.Red;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}