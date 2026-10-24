using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;

using Nop.Plugin.Apollo.Core.Infrastructure;
using Nop.Plugin.Apollo.EventTickets.Components;
using Nop.Plugin.Apollo.EventTickets.Services;
using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets;

public class EventTicketsPlugin : BasePlugin, IApolloCorePlugin
{

    int _maximumNumberOfOrders = 10;

    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly WidgetSettings _widgetSettings;
    protected readonly INopFileProvider _fileProvider;
    protected readonly IStoreContext _storeContext;
    protected readonly IRepository<EmailAccount> _emailAccountRepository;
    protected readonly IRepository<MessageTemplate> _messageTemplateRepository;
    protected readonly IRepository<Store> _storeRepository;
    protected readonly IRepository<Picture> _pictureRepository;
    protected readonly IRepository<Product> _productRepository;
    protected readonly IRepository<ProductAttribute> _productAttributeRepository;
    protected readonly IRepository<ProductTag> _productTagRepository;
    protected readonly IRepository<ProductTemplate> _productTemplateRepository;
    protected readonly IRepository<RelatedProduct> _relatedProductRepository;
    protected readonly IRepository<Category> _categoryRepository;
    protected readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
    protected readonly IRepository<TaxCategory> _taxCategoryRepository;
    protected readonly IRepository<UrlRecord> _urlRecordRepository;
    protected readonly IEventTicketService _eventTicketService;
    protected readonly IPermissionService _permissionService;
    protected readonly IEventTicketsInstallService _apolloInstallService;
    protected readonly IRepository<PermissionRecord> _permissionRecordRepository;
    protected readonly IWorkContext _workContext;
    protected readonly INotificationService _notificationService;
    protected readonly EventTicketsAccessSettings _eventTicketsAccessSettings;
    protected readonly IEventTicketsInstallService _eventTicketsInstallService;
    protected readonly EventTicketSettings _eventTicketSettings;
    protected readonly IEncryptionService _encryptionService;

    SystemHelper _systemHelper = new SystemHelper();

    #endregion

    #region Ctor

    public EventTicketsPlugin(ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings,
        INopFileProvider fileProvider,
        IStoreContext storeContext,
        IRepository<EmailAccount> emailAccountRepository,
        IRepository<MessageTemplate> messageTemplateRepository,
        IRepository<Store> storeRepository,
        IRepository<Picture> pictureRepository,
        IRepository<Product> productRepository,
        IRepository<ProductAttribute> productAttributeRepository,
        IRepository<ProductTag> productTagRepository,
        IRepository<ProductTemplate> productTemplateRepository,
        IRepository<RelatedProduct> relatedProductRepository,
        IRepository<Category> categoryRepository,
        IRepository<CategoryTemplate> categoryTemplateRepository,
        IRepository<TaxCategory> taxCategoryRepository,
        IRepository<UrlRecord> urlRecordRepository,
        IEventTicketService eventTicketService,
        IPermissionService permissionService,
        IEventTicketsInstallService apolloInstallService,
        IRepository<PermissionRecord> permissionRecordRepository,
        IWorkContext workContext,
        INotificationService notificationService,
        EventTicketsAccessSettings eventTicketsAccessSettings,
        IEventTicketsInstallService eventTicketsInstallService,
        EventTicketSettings eventTicketSettings,
        IEncryptionService encryptionService)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
        _fileProvider = fileProvider;
        _emailAccountRepository = emailAccountRepository;
        _messageTemplateRepository = messageTemplateRepository;
        _storeContext = storeContext;
        _storeRepository = storeRepository;
        _pictureRepository = pictureRepository;
        _productRepository = productRepository;
        _productAttributeRepository = productAttributeRepository;
        _productTagRepository = productTagRepository;
        _productTemplateRepository = productTemplateRepository;
        _relatedProductRepository = relatedProductRepository;
        _categoryRepository = categoryRepository;
        _categoryTemplateRepository = categoryTemplateRepository;
        _taxCategoryRepository = taxCategoryRepository;
        _urlRecordRepository = urlRecordRepository;
        _eventTicketService = eventTicketService;
        _permissionService = permissionService;
        _apolloInstallService = apolloInstallService;
        _permissionRecordRepository = permissionRecordRepository;
        _workContext = workContext;
        _notificationService = notificationService;
        _eventTicketsAccessSettings = eventTicketsAccessSettings;
        _eventTicketsInstallService = eventTicketsInstallService;
        _eventTicketSettings = eventTicketSettings;
        _encryptionService = encryptionService; 
    }

    #endregion

    #region Methods

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            AdminWidgetZones.AffiliateDetailsBlock
        });
    }

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/EventTickets/Configure";
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone.Equals(AdminWidgetZones.AffiliateDetailsBlock))
            return typeof(AffiliateProductViewComponent);

        return null;
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        string dateInstall = DateTime.Now.Date.ToString();
        var demoDateInstall = DateTime.Parse(dateInstall).AddDays(14);
        Guid pKeyInstall = _systemHelper.DateToGuid(demoDateInstall);
        string publicKeyInstall = pKeyInstall.ToString();
        publicKeyInstall = publicKeyInstall.Substring(0, 27) + "A00100000"; // Access EventTickets System for Demo
        string urlInstall = _systemHelper.GetDomainNameFromHost(store.Url);
        string privateKeyInstall = _encryptionService.EncryptText(urlInstall, publicKeyInstall);

        string message = string.Format("Installing plugin {0} for Store {1} with PublicKey {2} and PrivateKey {3}",
            SystemHelper.EventTicketDefaults.SystemName, urlInstall, publicKeyInstall, privateKeyInstall);

        var eventTicketsAccessSettings = await _settingService.LoadSettingAsync<EventTicketsAccessSettings>();
        eventTicketsAccessSettings.AccessEventTickets = false;
        await _settingService.SaveSettingAsync(eventTicketsAccessSettings);

        var eventTicketsInstallSettings = await _settingService.LoadSettingAsync<EventTicketSettings>();

        if (eventTicketsInstallSettings.Enabled)
        {
            // Settings remain from last installation
            eventTicketsInstallSettings.Enabled = false;
            await _settingService.SaveSettingAsync(eventTicketsInstallSettings);
        }
        else
        {

            eventTicketsInstallSettings = new EventTicketSettings
            {
                PublicKey = publicKeyInstall,
                PrivateKey = privateKeyInstall,
                Enabled = false,
                EventTicketsEnabled = false,
                EventTicketsURLPattern = "events",
                MaximumNumberOfOrders = _maximumNumberOfOrders,
                DeleteTablesOnUninstall = false,
                DeleteConfigurationDataOnUninstall = false,
                ProductAttributeIds = string.Empty,
                AttachFileLocation = "~/wwwroot/images/uploaded/",
                TestMode = false,
                SeperateTickets = false,
                DefaultReportDateTime = "04:00:00 PM",
                OffSaleTimeInMinutes = 5,
                UnPublishTimeInHours = 6,
                DefaultStoreId = store.Id,
                UseNopTemplatesAjaxCart = false,
                SupportedStoreThemes = "Venture,Motion,Emporium,Prisma,Pacific",
                LogoSize = 200f,
                TicketFontColor = "Blue",
                FontFileName = "Calibri Regular.ttf"
            };

            await _settingService.SaveSettingAsync(eventTicketsInstallSettings);

            //locales
            var localResources = GetPluginResources();
            await _localizationService.AddOrUpdateLocaleResourceAsync(localResources);

            await _eventTicketsInstallService.InstallPermissionsAsync(true);

            await _eventTicketsInstallService.InstallTasksAsync(true);

        }

        await base.InstallAsync();

    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    { 

        var eventTicketsUninstallSettings = await _settingService.LoadSettingAsync<EventTicketSettings>();

        await _eventTicketsInstallService.InstallTasksAsync(false);

        if (eventTicketsUninstallSettings.Enabled)
            await _apolloInstallService.InstallLocalisationAsync(false);

        if (eventTicketsUninstallSettings.EventTicketsEnabled)
            await _apolloInstallService.InstallEventTicketsProductsAsync(false);

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugin.Apollo.EventTickets");

        if (eventTicketsUninstallSettings.DeleteConfigurationDataOnUninstall)
            await _eventTicketsInstallService.InstallPermissionsAsync(false);

        await base.UninstallAsync();
    }

    public IDictionary<string, string> GetPluginResources()
    {

        var list = new Dictionary<string, string>
        {
            ["Plugin.Apollo.EventTickets.Title"] = "Event Tickets System",

            ["Plugin.Apollo.EventTickets.Note.Restart"] = "Please restart the application once a configure has been modified.",

            ["Plugin.Apollo.EventTickets.Note"] = "Sample Data will be installed when a system (Event Tickets) is enabled for All Stores. To display the demonstration data in other stores manually adjust the Product and Category Records.",

            ["Plugin.Apollo.EventTickets.PublicKey"] = "License Key",
            ["Plugin.Apollo.EventTickets.PublicKey.Hint"] = "Please enter the Access Key provided",

            ["Plugin.Apollo.EventTickets.PrivateKey"] = "Private Key",
            ["Plugin.Apollo.EventTickets.PrivateKey.Hint"] = "Enter the Private License Key supplied via email after plugin purchase",

            ["Plugin.Apollo.EventTickets.IsEnabled"] = "Plugin Enabled",
            ["Plugin.Apollo.EventTickets.IsEnabled.Hint"] = "Check to enable the EventTickets plugin.",

            ["Plugin.Apollo.EventTickets.TestMode"] = "Test Mode",
            ["Plugin.Apollo.EventTickets.TestMode.Hint"] = "Check to enable test Mode and log information messages",

            ["Plugin.Apollo.EventTickets.EventTicketsIsEnabled"] = "Event Tickets System Enabled",
            ["Plugin.Apollo.EventTickets.EventTicketsIsEnabled.Hint"] = "Check to enable the Booking System and load the default configuration",
            ["Plugin.Apollo.EventTickets.EventTicketsURLPattern"] = "EventTickets URL Pattern",
            ["Plugin.Apollo.EventTickets.EventTicketsURLPattern.Hint"] = "Enter the URL pattern token for Event Tickets Products",

            ["Plugin.Apollo.EventTickets.ProductAttribute"] = "Hidden Product Attribute",
            ["Plugin.Apollo.EventTickets.ProductAttribute.Hint"] = "Select Product Attributes which are Hidden in the Booking form",

            ["Plugin.Apollo.EventTickets.CheckoutAttribute"] = "Checkout Attributes",
            ["Plugin.Apollo.EventTickets.CheckoutAttribute.Hint"] = "Select Checkout Attributes which are associated with Event Tickets Products",
            ["Plugin.Apollo.EventTickets.CheckoutAttribute.NoCheckoutAttributesAvailable"] = "No Checkout Attributes are available for selection",

            ["Plugin.Apollo.EventTickets.MaximumNumberOfOrders"] = "Maximum Number of Orders",
            ["Plugin.Apollo.EventTickets.MaximumNumberOfOrders.Hint"] = "Select Maximum Number of Orders to display",

            ["Plugin.Apollo.EventTickets.UseNopTemplatesAjaxCart"] = "Use NopTemplates Ajax Cart",
            ["Plugin.Apollo.EventTickets.UseNopTemplatesAjaxCart.Hint"] = "Select this option if you are using the Nop-Templates Ajax Cart Plugin",

            ["Plugin.Apollo.EventTickets.SeperateTickets"] = "Seperate Tickets",
            ["Plugin.Apollo.EventTickets.SeperateTickets.Hint"] = "Select this option if you want seperate tickets to be created for an order",

            ["Plugin.Apollo.EventTickets.DeleteTablesOnUninstall"] = "Delete Tables on Uninstall",
            ["Plugin.Apollo.EventTickets.DeleteTablesOnUninstall.Hint"] = "Select to Delete Tables when Plugin Uninstalled",

            ["Plugin.Apollo.EventTickets.DeleteConfigurationDataOnUninstall"] = "Delete Configuration on Uninstall",
            ["Plugin.Apollo.EventTickets.DeleteConfigurationDataOnUninstall.Hint"] = "Select to Delete the Configuration when Plugin Uninstalled",

            ["Plugin.Apollo.EventTickets.AttachFileLocation"] = "Attachment Files Directory Location",
            ["Plugin.Apollo.EventTickets.AttachFileLocation.Hint"] = "Enter the directory location where files are stored e.g. ~/wwwroot/files/exportimport",

            ["Plugin.Apollo.EventTickets.Menu"] = "Event Tickets System",
            ["Plugin.Apollo.EventTickets.EventTicketsSales"] = "Order Sales",
            ["Plugin.Apollo.EventTickets.EventTicketsDelivery"] = "Order Delivery",
            ["Plugin.Apollo.EventTickets.EventTicketsProcess"] = "Order Complete",
            ["Plugin.Apollo.EventTickets.EventTicketsSettlement"] = "Settlements",
            ["Plugin.Apollo.EventTickets.Configure"] = "Configure System",
            ["Plugin.Apollo.EventTickets.Manage"] = "Manage System",
            ["Plugin.Apollo.EventTickets.Agents"] = "Manage Agents",

            ["Plugin.Apollo.EventTickets.Configure.Settings"] = "Plugin Configuration",
            ["Plugin.Apollo.EventTickets.Configure.ProductSettings"] = "Product Configuration",

            ["Plugin.Apollo.EventTickets.Delivery.Order"] = "Order",
            ["Plugin.Apollo.EventTickets.Delivery.Shipment"] = "Shipment",
        };

        return list;
    }


    #region Properties

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;

    #endregion

}

#endregion
