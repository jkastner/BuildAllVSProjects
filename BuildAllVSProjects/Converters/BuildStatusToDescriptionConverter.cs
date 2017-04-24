using System;
using System.Globalization;
using System.Windows.Data;
using BuildAllVSProjects.Models;

namespace BuildAllVSProjects.Converters
{
    internal class BuildStatusToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isBuildStatus = value as BuildSuccessStatus? ?? BuildSuccessStatus.Undefined;
            if (isBuildStatus != BuildSuccessStatus.Undefined)
            {
                switch (isBuildStatus)
                {
                    case BuildSuccessStatus.Undefined:
                        break;
                    case BuildSuccessStatus.NotAttempted:
                        return "Not attempted";
                    case BuildSuccessStatus.IsBuilding:
                        return "Building...";
                    case BuildSuccessStatus.FailedOnLatest:
                        return "Build failed";
                    case BuildSuccessStatus.SucceededOnLatest:
                        return "Build succeeded";
                    case BuildSuccessStatus.FailedOnPrevious:
                        return "Build previously failed";
                    case BuildSuccessStatus.SucceededOnPrevious:
                        return "Build previously succeeded";
                    case BuildSuccessStatus.Exception:
                        return "Build exception";
                    default:
                        return "";
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}