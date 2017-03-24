using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BuildAllVSProjects.Models;
using Caliburn.Micro;

namespace BuildAllVSProjects.ViewModels
{
    [Export(typeof (ProjectsViewModel))]
    internal class ProjectsViewModel : Screen
    {
        private readonly BuildService _buildService;
        private bool _buildRunning;
        private string _extensionTypes = ".sln";
        private string _targetDirectory = @"C:\Users\SESA222691\Documents\SharedProjects";
        private string _vsLocation = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com";

        [ImportingConstructor]
        public ProjectsViewModel(BuildService buildService, ReportViewModel reporter)
        {
            _buildService = buildService;
            Reporter = reporter;
            AllProjects = new ObservableCollection<SolutionFile>();
        }

        public ReportViewModel Reporter { get; }
        
        public bool CanStartBrowse
        {
            get { return !_buildRunning; }
        }

        public bool CanRebuildAll
        {
            get { return !_buildRunning && AllProjects.Any(); }
        }

        public bool CanBuildAll
        {
            get { return !_buildRunning && AllProjects.Any(); }
        }
        public bool CanCancelBuild
        {
            get { return _buildRunning && !_isCanceling; }
        }

        public async void CancelBuild()
        {
            _cancelObject.ShouldCancel = true;
            _isCanceling = true;
            NotifyOfPropertyChange(() => CanCancelBuild);
        }


        public string VSLocation
        {
            get { return _vsLocation; }
            set
            {
                if (value == _vsLocation) return;
                _vsLocation = value;
                NotifyOfPropertyChange(() => VSLocation);
            }
        }


        public ObservableCollection<SolutionFile> AllProjects { get; set; }

        public string TargetDirectory
        {
            get { return _targetDirectory; }
            set
            {
                if (value == _targetDirectory) return;
                _targetDirectory = value;
                NotifyOfPropertyChange(() => TargetDirectory);
            }
        }


        public string ExtensionTypes
        {
            get { return _extensionTypes; }
            set
            {
                if (value == _extensionTypes) return;
                _extensionTypes = value;
                NotifyOfPropertyChange(() => ExtensionTypes);
            }
        }

        public void RebuildAll()
        {
            StartBuild(true);
        }

        CancelObject _cancelObject;
        private bool _isCanceling;

        private async Task<bool> StartBuild(bool rebuild)
        {
            Reporter.ClearText();
            _isCanceling = false;
            _buildRunning = true;
            NotifyCanBuild();
            _cancelObject = new CancelObject();
            Task<bool> qq = _buildService.Build(rebuild, VSLocation, AllProjects, _cancelObject);
            await qq;
            _buildRunning = false;
            NotifyCanBuild();
            return true;
        }

        private void NotifyCanBuild()
        {
            NotifyOfPropertyChange(() => CanStartBrowse);
            NotifyOfPropertyChange(() => CanRebuildAll);
            NotifyOfPropertyChange(() => CanBuildAll);
            NotifyOfPropertyChange(() => CanCancelBuild);
            

        }

        public async void StartBrowse()
        {
            PopulateFromDir();
            NotifyCanBuild();
        }

        public async Task<bool> BuildAll()
        {
            return await StartBuild(false);
        }

        Dictionary<String, SolutionFile> _solutionFileCache = new Dictionary<string, SolutionFile>();
        private string _reportText;

        public void PopulateFromDir()
        {
            AllProjects.Clear();
            var allSlnFiles = GetAllFilesByExentions(TargetDirectory);
            foreach (var cur in allSlnFiles)
            {
                if (!_solutionFileCache.ContainsKey(cur))
                {
                    _solutionFileCache.Add(cur, new SolutionFile(cur));
                }
                AllProjects.Add(_solutionFileCache[cur]);
            }
        }

        public void OpenProject(SolutionFile toOpen)
        {
            if (toOpen.BuildStatus == BuildSuccessStatus.NotAttempted)
            {
                return;
            }
            try
            {
                Process.Start(toOpen.FilePath);
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not start " + toOpen.FilePath + "\nException: \n" + e.Message);
            }
        }


        private IEnumerable<string> GetAllFilesByExentions(string target)
        {
            List<String> ret = new List<string>();
            GetAllExtensionFilesHelper(target, ret);
            return ret;


        }

        private void GetAllExtensionFilesHelper(string target, List<string> ret)
        {

            if (!Directory.Exists(target))
            {
                Reporter.Report("Directory "+target+" does not exist.");
                return;
            }
            foreach (
                var curExt in ExtensionTypes.Split(',').Select(x => x.Trim().Trim()).Where(x => !String.IsNullOrWhiteSpace(x)))
                
            {
                var allCurFiles = Directory.GetFiles(target).Where(x => Path.GetExtension(x).ToLower().Equals(curExt));
                ret.AddRange(allCurFiles);
            }
            
            
            foreach (var cur in Directory.GetDirectories(target))
            {
                try
                {
                    GetAllExtensionFilesHelper(cur, ret);

                }
                catch (Exception e)
                {
                    
                }
            }
        }
    }
}