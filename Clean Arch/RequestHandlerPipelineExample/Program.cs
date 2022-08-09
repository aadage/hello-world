var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


#region IRequestHandler Registrations
(new DependencyInjectionByAssembly(builder.Services)).RegisterAllImplementersOfInterfaceInAssembly(typeof(IRequestHandler<,>), Assembly.GetAssembly(typeof(LogEntry)));
RegisterPipeline<AddLogEntryCommand, AddLogEntryResult, AddLogEntryCommandHandler>(builder.Services);
RegisterPipeline<DeleteLogEntryCommand, DeleteLogEntryResult, DeleteLogEntryCommandHandler>(builder.Services);
RegisterPipeline<GetLogEntriesQuery, GetLogEntriesResult, GetLogEntriesQueryHandler>(builder.Services);
#endregion

builder.Services.AddTransient<ILogRepository, LocalStorageLogRepository>();
builder.Services.AddTransient<ITimeTrackerOptions, TimeTrackerOptions>();

#region Omni
builder.Services.AddTransient<IClipboard, Clipboard>();
builder.Services.AddTransient<IDOMUtil, DOMUtil>();
builder.Services.AddTransient<ILocalStorage, LocalStorage>();
builder.Services.AddTransient<ILog, LogSink>();
builder.Services.AddSingleton(typeof(EventBus<>));
#endregion  //Omni

#if DEBUG
builder.Services.AddTransient<IBundleService, BundleServiceFake>();
#endif


await builder.Build().RunAsync();


void RegisterPipeline<TRequest, TResponse, THandler>(IServiceCollection services) where THandler : IRequestHandler<TRequest, TResponse>
{
    services.AddTransient<IRequestHandler<TRequest, TResponse>>(s =>
    {
        var timer = s.GetService<CommandTimerRequestHandler<TRequest, TResponse>>();
        var neet = s.GetService<NeetRequestHandler<TRequest, TResponse>>();
        var handler = s.GetService<THandler>();
        timer.SetNext(neet);
        neet.SetNext(handler);
        return timer;
    });

}