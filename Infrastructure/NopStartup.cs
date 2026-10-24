using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Nop.Core.Infrastructure;
using Nop.Services.Common;

using Nop.Plugin.Apollo.Core.Services;
using Nop.Plugin.Apollo.EventTickets.ActionFilter;
using Nop.Plugin.Apollo.EventTickets.ExportImport;
using Nop.Plugin.Apollo.EventTickets.Factories;
using Nop.Plugin.Apollo.EventTickets.Services;
using Nop.Plugin.Apollo.EventTickets.ViewEngine;

namespace Nop.Plugin.Apollo.EventTickets.Infrastructure;

public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    { 

        services.AddScoped<ITicketModelFactory, TicketModelFactory>();
        services.AddScoped<IProductTypeExtensionsService, ProductTypeExtensionsService>();
        services.AddScoped<IEventTicketsInstallService, EventTicketsInstallService>();
        services.AddScoped<IEventTicketService, EventTicketService>();
        services.AddScoped<IEventTicketsMessageService, EventTicketsMessageService>();
        services.AddScoped<IEventTicketsModelFactory, EventTicketsModelFactory>();
        services.AddScoped<IEventTicketsPdfService, EventTicketsPdfService>();
        services.Replace(ServiceDescriptor.Scoped<IPdfService, EventTicketsPdfService>());
        services.AddScoped<IExportImportManager, ExportImportManager>();

        //add object context
        services.Configure<MvcOptions>(config => {
            config.Filters.Add<EventTicketsActionFilter>();
        });

        services.Configure<RazorViewEngineOptions>(o =>
        {
            o.ViewLocationExpanders.Add(new CustomViewEngine());
        });
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => int.MaxValue;
}