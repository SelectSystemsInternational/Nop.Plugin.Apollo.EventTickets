using Microsoft.AspNetCore.Mvc.Razor;

using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Plugins;
using Nop.Services.Configuration;

using Nop.Web.Framework.Themes;

using Nop.Plugin.Apollo.EventTickets.Services;
using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets.ViewEngine;

public class CustomViewEngine : IViewLocationExpander
{
    public string StoreTheme = "DefaultClean";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.AreaName?.Equals("Admin") ?? false)
            return;

        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var storeContext = EngineContext.Current.Resolve<IStoreContext>();

        StoreTheme = settingService.GetSettingByKeyAsync("storeinformationsettings.defaultstoretheme", "DefaultClean", storeContext.GetCurrentStore().Id, true).Result;

        var themeContext = (IThemeContext)context.ActionContext.HttpContext.RequestServices.GetService(typeof(IThemeContext));
        if (themeContext != null)
            context.Values[StoreTheme] = themeContext.GetWorkingThemeNameAsync().ConfigureAwait(false).ToString();
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {

        var pluginService = EngineContext.Current.Resolve<IPluginService>();
        var eventTicketSettings = EngineContext.Current.Resolve<EventTicketSettings>();
        var eventTicketService = EngineContext.Current.Resolve<IEventTicketService>();

        var plugin = pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("Apollo.EventTickets", LoadPluginsMode.InstalledOnly).Result;
        if (plugin != null)
        {
            if (eventTicketSettings.Enabled)
            {
                if (context.AreaName == "Admin")
                {
                    if (context.ControllerName == "Ticket")
                    {
                        viewLocations = new[] {
                            $"~/Plugins/SSI.{plugin.SystemName}/Views/{context.ControllerName}/{context.ViewName}.cshtml",
                            $"~/Plugins/SSI.{plugin.SystemName}/Views/{context.ViewName}.cshtml",
                            $"~/Areas/Admin/Views/Product/{context.ViewName}.cshtml"
                        }.Concat(viewLocations);
                    }
                }
                else if (context.AreaName == null)
                {
                    #region public side

                    if (!context.Values.TryGetValue(StoreTheme, out string? theme))
                        return viewLocations;

                    if (context.ControllerName == "EventTickets")
                    {
                        if (context.ViewName == "_SKU_Man_GTIN_Ven")
                            viewLocations = new[] { $"~/Plugins/SSI.Apollo.EventTickets/Views/FrontView/_SKU_Man_GTIN_Ven.cshtml" }.Concat(viewLocations);
                        else if (context.ViewName != "")
                        {
                            var newViewLocations = new[]
                            {
                                $"~/Themes/" + StoreTheme + "/Views/Shared/" + context.ViewName + ".cshtml",
                                $"~/Themes/" + StoreTheme + "/Views/Product/" + context.ViewName + ".cshtml",
                                $"~/Views/Shared/" + context.ViewName + ".cshtml",
                                $"~/Views/Product/" + context.ViewName + ".cshtml",
                            };

                            viewLocations = viewLocations.Concat(newViewLocations);
                        }
                    }


                    #endregion
                }
            }
        }

        return viewLocations;
    }
}