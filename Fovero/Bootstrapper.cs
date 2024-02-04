using System.Windows;
using Caliburn.Micro;
using Fovero.UI;

namespace Fovero;

public class Bootstrapper : BootstrapperBase
{
    private readonly SimpleContainer _container = new();

    public Bootstrapper()
    {
        Initialize();
    }

    protected override void Configure()
    {
        _container.Instance(_container);
        _container
            .Singleton<IWindowManager, WindowManager>()
            .Singleton<IEventAggregator, EventAggregator>();

        var viewModelTypes = SelectAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("ViewModel"));

        foreach (var viewModelType in viewModelTypes)
        {
            _container.RegisterPerRequest(viewModelType, viewModelType.ToString(), viewModelType);
        }
    }

    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        ViewLocator.ConfigureTypeMappings(new TypeMappingConfiguration
        {
            IncludeViewSuffixInViewModelNames = false,
            ViewSuffixList = ["View", "Window"]
        });

        DisplayRootViewForAsync<MainViewModel>();
    }

    protected override object GetInstance(Type serviceType, string key)
    {
        return _container.GetInstance(serviceType, key);
    }

    protected override IEnumerable<object> GetAllInstances(Type serviceType)
    {
        return _container.GetAllInstances(serviceType);
    }

    protected override void BuildUp(object instance)
    {
        _container.BuildUp(instance);
    }
}
