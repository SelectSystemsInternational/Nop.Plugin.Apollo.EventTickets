using Microsoft.AspNetCore.Http;

using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.EventTickets.Settings;
using Nop.Plugin.Apollo.EventTickets.Services;

namespace Nop.Plugin.Apollo.EventTickets.Infrastructure;

/// <summary>
/// Represents the plugin event consumer
/// </summary>
public class AdminMenuEventConsumer :
    BaseAdminMenuCreatedEventConsumer,
    IConsumer<SystemWarningCreatedEvent>
{

    #region Fields

    private readonly IAdminMenu _adminMenu;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly IShipmentService _shipmentService;
    private readonly IPermissionService _permissionService;
    private readonly IWorkContext _workContext;
    private readonly IEventTicketService _eventTicketService;
    private readonly EventTicketSettings _eventTicketSettings;

    #endregion

    #region Ctor

    public AdminMenuEventConsumer(IAdminMenu adminMenu,
        IGenericAttributeService genericAttributeService,
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService,
        IShipmentService shipmentService,
        IPermissionService permissionService,
        IPluginManager<IPlugin> pluginManager,
        IWorkContext workContext,
        IEventTicketService eventTicketService,
        EventTicketSettings eventTicketSettings) : base(pluginManager)
    {
        _adminMenu = adminMenu;
        _genericAttributeService = genericAttributeService;
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;
        _shipmentService = shipmentService;
        _permissionService = permissionService;
        _workContext = workContext;
        _eventTicketService = eventTicketService;
        _eventTicketSettings = eventTicketSettings;
    }

    #endregion

    #region Utitites

    /// <summary>
    /// Checks is the current customer has rights to access this menu item
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the true if access is granted, otherwise false
    /// </returns>
    protected override async Task<bool> CheckAccessAsync()
    {
        return await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Operate) || 
               await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage);
    }

    /// <summary>
    /// Gets the menu item
    /// </summary>
    /// <param name="plugin">The instance of <see cref="IPlugin"/> interface</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the instance of <see cref="AdminMenuItem"/>
    /// </returns>
    protected override async Task<AdminMenuItem> GetAdminMenuItemAsync(IPlugin plugin)
    {
        var descriptor = plugin.PluginDescriptor;

        AdminMenuItem mainMenuItem = new AdminMenuItem();
        AdminMenuItem eventTicketsSales = new AdminMenuItem();
        AdminMenuItem eventTicketsDelivery = new AdminMenuItem();
        AdminMenuItem eventTicketsProcess = new AdminMenuItem();
        AdminMenuItem eventTicketsSettlement = new AdminMenuItem();
        AdminMenuItem productCreateOrUpdateTickets = new AdminMenuItem();
        AdminMenuItem eventAffiliates = new AdminMenuItem();

        AdminMenuItem apolloSales = new AdminMenuItem();
        AdminMenuItem configureSystem = new AdminMenuItem();
        AdminMenuItem manageSystem = new AdminMenuItem();

        var vendor = await _workContext.GetCurrentVendorAsync();

        var lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Menu");
        if (lrs != null)
        {
            mainMenuItem = new AdminMenuItem()
            {
                SystemName = "Nop.Plugin.Apollo.EventTickets.Menu",
                Title = lrs,
                IconClass = "far fa-calendar-days",
                Visible = true
            };
        }

        if (_eventTicketSettings.Enabled && await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Operate))
        {
            lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.EventTicketsSales");
            if (lrs != null)
            {
                eventTicketsSales = new AdminMenuItem()
                {
                    SystemName = "Nop.Plugin.Apollo.EventTickets.EventTicketsSales",
                    Title = lrs,
                    Url = _adminMenu.GetMenuItemUrl("EventTickets", "EventTicketsSales"),
                    Visible = true,
                    IconClass = "fa fa-genderless",
                };
            }

            lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.EventTicketsDelivery");
            if (lrs != null)
            {
                eventTicketsDelivery = new AdminMenuItem()
                {
                    SystemName = "Nop.Plugin.Apollo.EventTickets.EventTicketsDelivery",
                    Title = lrs,
                    Url = _adminMenu.GetMenuItemUrl("EventTickets", "EventTicketsDelivery"),
                    Visible = true,
                    IconClass = "fa fa-genderless",
                };
            }

            lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.EventTicketsProcess");
            if (lrs != null)
            {
                eventTicketsProcess = new AdminMenuItem()
                {
                    SystemName = "Nop.Plugin.Apollo.EventTickets.EventTicketsProcess",
                    Title = lrs,
                    Url = _adminMenu.GetMenuItemUrl("EventTickets", "EventTicketsProcess"),
                    Visible = true,
                    IconClass = "fa fa-genderless",
                };
            }

            lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.EventTicketsSettlement");
            if (lrs != null)
            {
                eventTicketsSettlement = new AdminMenuItem()
                {
                    SystemName = "Nop.Plugin.Apollo.EventTickets.EventTicketsSettlement",
                    Title = lrs,
                    Url = _adminMenu.GetMenuItemUrl("EventTickets", "EventTicketsSettlement"),
                    Visible = true,
                    IconClass = "fa fa-genderless",
                };
            }

            if (await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            {
                lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.CreateOrUpdateTickets");
                if (lrs != null)
                {
                    productCreateOrUpdateTickets = new AdminMenuItem()
                    {
                        SystemName = "Nop.Plugin.Apollo.EventTickets.CreateOrUpdateTickets",
                        Title = lrs,
                        Url = _adminMenu.GetMenuItemUrl("Ticket", "List"),
                        Visible = true,
                        IconClass = "fa fa-genderless",
                    };
                }

                lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Agents");
                if (lrs != null)
                {
                    eventAffiliates = new AdminMenuItem()
                    {
                        SystemName = "Nop.Plugin.Apollo.EventTickets.Agents",
                        Title = lrs,
                        Url = _adminMenu.GetMenuItemUrl("CustomAffiliate", "ListAgent"),
                        Visible = true,
                        IconClass = "fa fa-genderless",
                    };
                }

                lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Manage");
                if (lrs != null)
                {
                    manageSystem = new AdminMenuItem()
                    {
                        SystemName = "Nop.Plugin.Apollo.EventTickets.Manage",
                        Title = lrs,
                        Url = _adminMenu.GetMenuItemUrl("EventTickets", "EventTicketsManage"),
                        Visible = true,
                        IconClass = "fa fa-genderless",
                    };
                }
            }

            mainMenuItem.ChildNodes.Add(eventTicketsSales);
            mainMenuItem.ChildNodes.Add(eventTicketsDelivery);
            mainMenuItem.ChildNodes.Add(eventTicketsProcess);
            mainMenuItem.ChildNodes.Add(eventTicketsSettlement);
            mainMenuItem.ChildNodes.Add(productCreateOrUpdateTickets);

            if (_eventTicketSettings.Enabled && await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            {
                if (manageSystem != null)
                    mainMenuItem.ChildNodes.Add(manageSystem);
            }
        }

        if (await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) && vendor == null)
        {
            lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Configure");
            if (lrs != null)
            {
                configureSystem = new AdminMenuItem()
                {
                    SystemName = "Nop.Plugin.Apollo.EventTickets.Configure",
                    Title = lrs,
                    Url = _adminMenu.GetMenuItemUrl("EventTickets", "Configure"),
                    Visible = true,
                    IconClass = "fa fa-genderless",
                };
            }

            if (configureSystem != null)
                mainMenuItem.ChildNodes.Add(configureSystem);
        }

        return mainMenuItem;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handle system warning created event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task HandleEventAsync(SystemWarningCreatedEvent eventMessage)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    protected override string PluginSystemName => SystemHelper.EventTicketDefaults.SystemName;

    /// <summary>
    /// Menu item insertion type
    /// </summary>
    protected override MenuItemInsertType InsertType => MenuItemInsertType.TryAfterThanBefore;

    /// <summary>
    /// The system name of the menu item after with need to insert the current one
    /// </summary>
    protected override string AfterMenuSystemName => CoreSystemHelper.ApolloMenuSystem.FifthMenu;

    /// <summary>
    /// The system name of the menu item before with need to insert the current one
    /// </summary>
    protected override string BeforeMenuSystemName => CoreSystemHelper.ApolloMenuSystem.BaseBeforeMenu;

    #endregion
}