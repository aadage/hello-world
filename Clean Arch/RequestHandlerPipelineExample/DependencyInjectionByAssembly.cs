namespace TimeTracker;

public class DependencyInjectionByAssembly
{
    private readonly IServiceCollection services;

    public DependencyInjectionByAssembly(IServiceCollection services)
    {
        this.services = services;
    }

    public void RegisterAllImplementersOfInterfaceInAssembly(Type type, Assembly assembly)
    {
        foreach (var assType in assembly.GetExportedTypes())
        {
            if (assType.GetInterface(type.Name) != null)
            {
                services.AddTransient(assType);
            }
        }
    }
}

