using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BuildAllVSProjects.Models;
using Caliburn.Micro;


namespace BuildAllVSProjects.ViewModels
{
    [Export(typeof(ProjectsViewModel))]
    internal class ProjectsViewModel : Screen
    {
        private readonly BuildService _buildService;

        private readonly Dictionary<string, SolutionFile> _solutionFileCache = new Dictionary<string, SolutionFile>();
        private bool _buildRunning;

        private CancelObject _cancelObject;
        private string _extensionTypes = ".sln";
        private bool _isCanceling;
        private const string VsLocKey = "VSLocKey";
        private const string TargetDirectoryKey = "TargetDirectoryKey ";
        private readonly Configuration _configuration;
        private const string DefaultVsLocation = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com";

        [ImportingConstructor]
        public ProjectsViewModel(BuildService buildService, ReportViewModel reporter)
        {
            _buildService = buildService;
            _configuration = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Reporter = reporter;
            AllProjects = new ObservableCollection<SolutionFile>();
            LoadDefaults();
        }
        private string _vsLocation;
        private string _targetDirectory;
        private void LoadDefaults()
        {
            string vsLoc = ConfigurationManager.AppSettings[VsLocKey];
            string targetDir = ConfigurationManager.AppSettings[TargetDirectoryKey];
            
            if (String.IsNullOrWhiteSpace(vsLoc))
            {
                vsLoc = DefaultVsLocation;
            }
            if (String.IsNullOrWhiteSpace(targetDir))
            {
                string myName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                targetDir = "C:\\Users\\" + myName + "\\Documents";
            }
            VSLocation = vsLoc;
            TargetDirectory = targetDir;
        }

        private void SetConfigSetting(string key, string value)
        {
            _configuration.AppSettings.Settings.Remove(key);
            _configuration.AppSettings.Settings.Add(key, value);
            _configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
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

        ~ProjectsViewModel()
        {
            SetConfigSetting(VsLocKey, VSLocation);
            SetConfigSetting(TargetDirectoryKey, TargetDirectory);
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

        public void CancelBuild()
        {
            Reporter.Report("Canceling...");
            _cancelObject.ShouldCancel = true;
            _isCanceling = true;
             NotifyOfPropertyChange(() => CanCancelBuild);
        }

        public void RebuildAll()
        {
            StartBuild(true);
        }

        private async Task<bool> StartBuild(bool rebuild)
        {
            Reporter.ClearText();
            _isCanceling = false;
            _buildRunning = true;
            NotifyCanBuild();
            _cancelObject = new CancelObject();
            var qq = _buildService.Build(rebuild, VSLocation, AllProjects, _cancelObject);
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
            Reporter.Report(AllProjects.Count+" projects found in directory.");
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
            var ret = new List<string>();
            GetAllExtensionFilesHelper(target, ret);
            return ret;
        }

        private void GetAllExtensionFilesHelper(string target, List<string> ret)
        {
            if (!Directory.Exists(target))
            {
                Reporter.Report("Directory " + target + " does not exist.");
                return;
            }
            foreach (
                var curExt in
                    ExtensionTypes.Split(',').Select(x => x.Trim().Trim()).Where(x => !string.IsNullOrWhiteSpace(x)))

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
                catch (Exception)
                {
                }
            }
        }
    }
}