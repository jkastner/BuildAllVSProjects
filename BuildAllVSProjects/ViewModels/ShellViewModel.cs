using System.ComponentModel.Composition;
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