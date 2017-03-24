using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BuildAllVSProjects.Models;

namespace BuildAllVSProjects.ViewModels
{
    [Export(typeof(BuildService))]
    internal class BuildService
    {
        private readonly ReportViewModel _reporter;

        [ImportingConstructor]
        public BuildService(ReportViewModel reporter)
        {
            _reporter = reporter;
        }

        public async Task<bool> Build(bool rebuild, string vsLocations, IEnumerable<SolutionFile> allProjects,
            CancelObject co)
        {
            int errorCount = 0;
            var buildDesc = "/Build";

            if (rebuild)
            {
                buildDesc = "/Rebuild";
            }


            var solutionFiles = allProjects.ToList();
            foreach (var cur in solutionFiles.Where(x => x.BuildStatus == BuildSuccessStatus.FailedOnLatest))
            {
                cur.BuildStatus = BuildSuccessStatus.FailedOnPrevious;
            }
            foreach (var cur in solutionFiles.Where(x => x.BuildStatus == BuildSuccessStatus.SucceededOnLatest))
            {
                cur.BuildStatus = BuildSuccessStatus.SucceededOnPrevious;
            }
            var allVsLocations = vsLocations.Split(',').Select(x => x.Trim()).ToList();

            foreach (var curVsLoc in allVsLocations)
            {
                if (!File.Exists(curVsLoc))
                {
                    _reporter.Report("VS not found at " + curVsLoc);
                    return true;
                }
            }
            var projectsToBuild = solutionFiles.Where(x => !SolutionFile.BuildSuccessful(x.BuildStatus)).ToList();
            if (rebuild)
            {
                projectsToBuild = solutionFiles.ToList();
            }
            if (!projectsToBuild.Any())
            {
                _reporter.Report("No projects to build.");
            }
            else
            {
                _reporter.Report("Building "+projectsToBuild.Count+" projects.");
            }
            foreach (var cur in projectsToBuild)
            {
                foreach (var curVsLoc in allVsLocations)
                {
                    if (co.ShouldCancel)
                    {
                        continue;
                    }
                    try
                    {
                        cur.BuildStatus = BuildSuccessStatus.IsBuilding;
                        var vsLocQuote = Enquote(curVsLoc);
                        var projFile = Path.GetFileName(cur.FilePath);
                        var projLoc = Path.GetDirectoryName(cur.FilePath);
                        var baseDir = Directory.GetDirectoryRoot(projLoc)[0] + ":";

                        //The build command
                        var command = vsLocQuote + " " + buildDesc + " Debug " +
                                      projFile;

                        //Change to that dir to avoid some crazy issues with spaces in the path name
                        command = "cd " + projLoc + "&&" + command;
                        //Also, be in that dir
                        command = baseDir + "&&" + command;

                        var dosLoc = @"C:\Windows\SysWOW64\cmd.exe";

                        //For debug, use /k
                        var myInfo = new ProcessStartInfo(dosLoc, " /c " + command);
                        myInfo.Verb = "runas";
                        myInfo.UseShellExecute = false;
                        myInfo.RedirectStandardOutput = true;
                        myInfo.CreateNoWindow = true;
                        var proc = Process.Start(myInfo);
                        _reporter.Report("Building " + cur.FilePath + " with " + curVsLoc.Trim());


                        await Task.Factory.StartNew(() =>
                        {
                            //var res = Dos.CommandLine.Execute(command);
                            var res = proc?.StandardOutput.ReadToEnd();
                            proc?.WaitForExit(-1);

                            if (res != null && res.Contains("0 failed"))
                            {
                                _reporter.ReportOnCurLine("-- success");
                                cur.BuildStatus = BuildSuccessStatus.SucceededOnLatest;
                            }
                            else
                            {
                                _reporter.ReportOnCurLine(" -- fail");
                                cur.BuildStatus = BuildSuccessStatus.FailedOnLatest;
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        cur.BuildStatus = BuildSuccessStatus.Exception;
                        errorCount++;
                    }
                }
            }
            _reporter.Report("Completed with " + errorCount + " errors.");
            return errorCount>0;
        }

        [Pure]
        private static string Enquote(string s)
        {
            return "\"" + s + "\"";
        }
    }
}