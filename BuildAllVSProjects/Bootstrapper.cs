using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows;
using BuildAllVSProjects.Properties;
using BuildAllVSProjects.ViewModels;
using Caliburn.Micro;

namespace BuildAllVSProjects
{
    internal sealed class Bootstrapper : BootstrapperBase
    {
        private CompositionContainer _container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var assemblies = new HashSet<Assembly>();

            assemblies.Add(typeof(ShellViewModel).Assembly);
            return assemblies;
        }

        protected override void Configure()
        {
            var priorityAssemblies = SelectAssemblies();

            var priorityCatalog = new AggregateCatalog(priorityAssemblies.Select(x => new AssemblyCatalog(x)));
            var catalog = new CatalogExportProvider(priorityCatalog);
            _container = new CompositionContainer(catalog);
            catalog.SourceProvider = _container;

            var batch = new CompositionBatch();
            BindServices(batch);
            batch.AddExportedValue(catalog);

            _container.Compose(batch);

            base.Configure();
        }

        private void BindServices(CompositionBatch batch)
        {
            batch.AddExportedValue<IWindowManager>(new WindowManager());
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            if (string.IsNullOrEmpty(key) && serviceType == null) return null;

            var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;

            var exports = _container.GetExportedValues<object>(contract);
            var enumerable = exports as object[] ?? exports.ToArray();
            if (enumerable.Any())
            {
                return enumerable.First();
            }

            throw new Exception($"Couldn't locate any instances of contract {contract}.");
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            _container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var width = Settings.Default.ScreenWidth; //Previous window width 
            var height = Settings.Default.ScreenHeight; //Previous window height

            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            if (width > screenWidth)
            {
                width = screenWidth - 10;
            }
            if (height > screenHeight)
            {
                height = screenHeight - 10;
            }

            var windowSettings = new Dictionary<string, object>();

            windowSettings.Add("Width", width);
            windowSettings.Add("Height", height);

            DisplayRootViewFor<ShellViewModel>();
        }
    }
}