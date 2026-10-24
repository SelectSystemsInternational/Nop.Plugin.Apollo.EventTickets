using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Data;
using Nop.Core.Infrastructure;
using Nop.Services.Plugins;
using Nop.Services.Configuration;
using Nop.Web.Framework.Mvc.Routing;

using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets;

public partial class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {

        //Confirm database is installed
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var pluginService = EngineContext.Current.Resolve<IPluginService>();

        var plugin = pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("Apollo.EventTickets", LoadPluginsMode.All).GetAwaiter().GetResult();
        if (plugin != null)
        {
            if (plugin.SystemName == "Apollo.EventTickets")
            {
                string urlPattern = string.Empty;
                var settingService = EngineContext.Current.Resolve<ISettingService>();
                var storeContext = EngineContext.Current.Resolve<IStoreContext>();

                var storeScope = storeContext.GetCurrentStore().Id;
                var eventTicketSettings = settingService.LoadSettingAsync<EventTicketSettings>(storeScope).GetAwaiter().GetResult();

                urlPattern = "EventTickets" + "/{SeName}";
                var setting = eventTicketSettings.EventTicketsURLPattern;
                if (!string.IsNullOrEmpty(setting))
                    urlPattern = setting + "/{SeName}";
                endpointRouteBuilder.MapControllerRoute("Nop.Plugin.Apollo.EventTickets.Front", urlPattern,
                  new { controller = "EventTickets", action = "EventTicketsFront" });

                endpointRouteBuilder.MapControllerRoute("Nop.Plugin.Apollo.EventTickets.ProductTypeExtensionSettingsCreateOrUpdate",
                    "Plugins/EventTicketsProductTypeExtensionSettings/ProductTypeExtensionSettingsCreateOrUpdate",
                    new { controller = "EventTicketsProductTypeExtensionSettings", action = "ProductTypeExtensionSettingsCreateOrUpdate" });

                endpointRouteBuilder.MapControllerRoute("Apollo.EventTickets.TicketRequest",
                    "Plugins/EventTickets/TicketRequest",
                    new { controller = "EventTickets", action = "TicketRequest" });

                endpointRouteBuilder.MapControllerRoute("Apollo.EventTickets.TicketRequestDisplay",
                    "Plugins/EventTickets/TicketRequestDisplay",
                    new { controller = "EventTickets", action = "TicketRequestDisplay" });

                endpointRouteBuilder.MapControllerRoute("Apollo.EventTickets.TicketRequestDisplayOrder",
                    "Plugins/EventTickets/TicketRequestDisplayOrder",
                    new { controller = "EventTickets", action = "TicketRequestDisplayOrder" });

                endpointRouteBuilder.MapControllerRoute("Apollo.EventTickets.TicketRequestError",
                    "Plugins/EventTickets/TicketRequestError",
                    new { controller = "EventTickets", action = "TicketRequestError" });

            }
        }
    }

    public int Priority
    {
        get { return 100; }
    }
}
