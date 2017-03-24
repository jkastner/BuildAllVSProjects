using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace BuildAllVSProjects.ViewModels
{
    [Export(typeof(ReportViewModel))]
    class ReportViewModel : Screen
    {
        private readonly List<String> _outputText = new List<string>();
        private string _syasdag;

        public void ReportOnCurLine(string info)
        {
            _outputText[0] = _outputText[0] + info;
            NotifyOfPropertyChange(() => ReportText);
        }

        public String ReportText
        {
            get
            {
                return String.Join("\n", _outputText);
            }
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
