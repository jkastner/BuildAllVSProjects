using System.IO;
using Caliburn.Micro;

namespace BuildAllVSProjects.Models
{
    internal class SolutionFile : PropertyChangedBase
    {
        private BuildSuccessStatus _buildStatus;

        public SolutionFile(string filePath)
        {
            BuildStatus = BuildSuccessStatus.NotAttempted;
            FilePath = filePath;
            Name = Path.GetFileNameWithoutExtension(filePath);
        }


        public string FilePath { get; }

        public string Name { get; }

        public BuildSuccessStatus BuildStatus
        {
            get { return _buildStatus; }
            set
            {
                if (value == _buildStatus) return;
                _buildStatus = value;
                NotifyOfPropertyChange(() => BuildStatus);
            }
        }

        public static bool BuildSuccessful(BuildSuccessStatus buildStatus)
        {
            return buildStatus == BuildSuccessStatus.SucceededOnLatest ||
                   buildStatus == BuildSuccessStatus.SucceededOnPrevious;
        }
    }
}