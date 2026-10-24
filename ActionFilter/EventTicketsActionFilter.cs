using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Data;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Plugins;
using Nop.Services.Seo;
using Nop.Web.Factories;

using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets.ActionFilter;

public class EventTicketsActionFilter : IAsyncActionFilter
{
    public void OnActionExecuted(ActionExecutedContext filterContext)
    {

    }

    public async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
    {
        bool actioned = false;

        //Confirm database is installed
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var pluginService = EngineContext.Current.Resolve<IPluginService>();
        var productModelFactory = EngineContext.Current.Resolve<IProductModelFactory>();
        var productService = EngineContext.Current.Resolve<IProductService>();
        var urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();
        var eventTicketSettings = EngineContext.Current.Resolve<EventTicketSettings>();

        var plugin = await pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("Apollo.EventTickets", LoadPluginsMode.All);
        if (plugin != null)
        {
            if (plugin.SystemName == "Apollo.EventTickets" && eventTicketSettings.Enabled)
            {
                var actionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
                var actionName = actionDescriptor?.ActionName;
                var controllerName = actionDescriptor?.ControllerName;

                if (filterContext.HttpContext.Request.Method == "GET")
                {
                    if (actionName == "ProductDetails" && controllerName == "Product")
                    {
                        var path = string.Empty;
                        var productId = filterContext.RouteData.Values["productId"].ToString();
                        if (!string.IsNullOrEmpty(productId))
                        {
                            int pId = int.Parse(productId);
                            if (pId != 0)
                            {
                                path = filterContext.HttpContext.Request.Path.Value.ToString();
                                var product = await productService.GetProductByIdAsync(pId);
                                if (product != null)
                                {
                                    var seName = await urlRecordService.GetSeNameAsync(product);
                                    var productTemplateViewPath = await productModelFactory.PrepareProductTemplateViewPathAsync(product);
                                    if (productTemplateViewPath == "ProductTemplate.EventTickets")
                                    {
                                        int updateCartItemId = 0;
                                        var httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();
                                        var querystring = httpContextAccessor.HttpContext.Request.QueryString.Value;
                                        if (querystring != null && querystring.Contains("?") && querystring.Contains("="))
                                        {
                                            var updateCartItem = querystring.ToString().Replace("?", "").Split('=');
                                            if (updateCartItem[1] != string.Empty && updateCartItem[1] != "0")
                                                updateCartItemId = Convert.ToInt32(updateCartItem[1]);
                                        }
                                        if (updateCartItemId != 0)
                                        {
                                            actioned = true;
                                            filterContext.Result = new RedirectToRouteResult("Nop.Plugin.Apollo.EventTickets.Front",
                                                    new { SeName = seName, UpdateCartItemId = updateCartItemId });
                                        }
                                        else
                                        {
                                            actioned = true;
                                            filterContext.Result = new RedirectToRouteResult("Nop.Plugin.Apollo.EventTickets.Front",
                                                    new { SeName = seName });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (!actioned)
            await next();
    }

    public int Order => 100;
}
