using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace BuildAllVSProjects.ViewModels
{
    [Export(typeof(ShellViewModel))]
    internal class ShellViewModel : Screen
    {
        [ImportingConstructor]
        public ShellViewModel()
        {
            Projects = IoC.Get<ProjectsViewModel>();
            DisplayName = "Project Builder";
        }
        public ProjectsViewModel Projects { get; private set; }
    }

}
