using System.Collections.Generic;
using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace BuildAllVSProjects.ViewModels
{
    [Export(typeof(ReportViewModel))]
    internal class ReportViewModel : Screen
    {
        private readonly List<string> _outputText = new List<string>();

        public string ReportText
        {
            get { return string.Join("\n", _outputText); }
        }

        public void ReportOnCurLine(string info)
        {
            _outputText[0] = _outputText[0] + info;
            NotifyOfPropertyChange(() => ReportText);
        }


        public void Report(string info)
        {
            _outputText.Insert(0, info);
            NotifyOfPropertyChange(() => ReportText);
        }

        public void ClearText()
        {
            _outputText.Clear();
            NotifyOfPropertyChange(() => ReportText);
        }
    }
}