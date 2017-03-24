using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BuildAllVSProjects.Annotations;
using Caliburn.Micro;

namespace BuildAllVSProjects.Models
{
    class SolutionFile : PropertyChangedBase
    {
        private BuildSuccessStatus _buildStatus;


        public string FilePath { get; }

        public SolutionFile(string filePath)
        {
            BuildStatus = BuildSuccessStatus.NotAttempted;
            FilePath = filePath;
            Name = Path.GetFileNameWithoutExtension(filePath);

        }

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
