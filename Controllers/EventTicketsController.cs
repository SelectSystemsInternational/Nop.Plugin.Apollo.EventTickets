using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Seo;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.Core.Domain;
using Nop.Plugin.Apollo.Core.Services;
using Nop.Plugin.Apollo.EventTickets.ExportImport;
using Nop.Plugin.Apollo.EventTickets.Factories;
using Nop.Plugin.Apollo.EventTickets.Models;
using Nop.Plugin.Apollo.EventTickets.Services;
using Nop.Plugin.Apollo.EventTickets.Settings;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using IAdminProductModelFactory = Nop.Web.Areas.Admin.Factories.IProductModelFactory;
using IProductModelFactory = Nop.Web.Factories.IProductModelFactory;

namespace Nop.Plugin.Apollo.EventTickets.Controllers;

public class EventTicketsController : BasePluginController
{

    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IStoreService _storeService;
    protected readonly ICustomerService _customerService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;
    protected readonly IEventTicketsModelFactory _eventTicketsModelFactory;
    protected readonly IEventTicketService _eventTicketService;
    protected readonly IProductService _productService;
    protected readonly IProductModelFactory _productModelFactory;
    protected readonly HttpContext _context;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly ICurrencyService _currencyService;
    protected readonly IProductAttributeService _productAttributeService;
    protected readonly INopDataProvider _dataProvider;
    protected readonly IOrderService _orderService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IPaymentService _paymentService;
    protected readonly IPaymentModelFactory _paymentModelFactory;
    protected readonly ILogger _logger;
    protected readonly IExportImportManager _exportImportManager;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IEventTicketsPdfService _eventTicketsPdfService;
    protected readonly OrderSettings _orderSettings;
    protected readonly IOrderModelFactory _orderModelFactory;
    protected readonly ShoppingCartSettings _shoppingCartSettings;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly IProductTemplateService _productTemplateService;
    protected readonly INotificationService _notificationService;
    protected readonly IPluginService _pluginService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly ICategoryService _categoryService;
    protected readonly IEventTicketsInstallService _eventTicketsInstallService;
    protected readonly IStaticCacheManager _cacheManager;
    protected readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
    protected readonly IRepository<UrlRecord> _urlRecordRepository;
    protected readonly IProductTypeExtensionsService _productTypeExtensionsService;
    protected readonly EventTicketsAccessSettings _eventTicketsAccessSettings;
    protected readonly INopFileProvider _fileProvider;
    protected readonly IShippingPluginManager _shippingPluginManager;
    protected readonly EventTicketSettings _eventTicketSettings;
    protected readonly IWebHelper _webHelper;
    protected readonly IShipmentService _shipmentService;
    protected readonly ITicketModelFactory _ticketModelFactory;
    protected readonly IVendorService _vendorService;
    protected readonly IApolloCoreService _apolloCoreService;

    #endregion

    #region Ctor

    public EventTicketsController(IWorkContext workContext,
        IStoreService storeService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext,
        IEventTicketsModelFactory eventTicketsModelFactory,
        IEventTicketService eventTicketService,
        IProductService productService,
        IProductModelFactory productModelFactory,
        IHttpContextAccessor httpContextAccessor,
        IPriceFormatter priceFormatter,
        ICurrencyService currencyService,
        IProductAttributeService productAttributeService,
        INopDataProvider dataProvider,
        IDateTimeHelper dateTimeHelper,
        ILogger logger,
        IExportImportManager exportImportManager,
        ICustomerActivityService customerActivityService,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IGenericAttributeService genericAttributeService,
        IPaymentService paymentService,
        IPaymentModelFactory paymentModelFactory,
        IEventTicketsPdfService eventTicketsPdfService,
        OrderSettings orderSettings,
        IOrderModelFactory orderModelFactory,
        ShoppingCartSettings shoppingCartSettings,
        IUrlRecordService urlRecordService,
        IProductTemplateService productTemplateService,
        INotificationService notificationService,
        IPluginService pluginService,
        IPaymentPluginManager paymentPluginManager,
        IShoppingCartService shoppingCartService,
        ICategoryService categoryService,
        IEventTicketsInstallService apolloInstallService,
        IStaticCacheManager cacheManager,
        IRecentlyViewedProductsService recentlyViewedProductsService,
        IRepository<UrlRecord> urlRecordRepository,
        IProductTypeExtensionsService productTypeExtensionsService,
        EventTicketsAccessSettings eventTicketsAccessSettings,
        INopFileProvider fileProvider,
        IShippingPluginManager shippingPluginManager,
        EventTicketSettings eventTicketSettings,
        IWebHelper webHelper,
        IShipmentService shipmentService,
        ITicketModelFactory ticketModelFactory,
        IVendorService vendorService,
        IApolloCoreService apolloCoreService)   
    {
        _workContext = workContext;
        _storeService = storeService;
        _customerService = customerService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
        _eventTicketsModelFactory = eventTicketsModelFactory;
        _eventTicketService = eventTicketService;
        _productService = productService;
        _productModelFactory = productModelFactory;
        _httpContextAccessor = httpContextAccessor;
        _context = this._httpContextAccessor.HttpContext;
        _priceFormatter = priceFormatter;
        _currencyService = currencyService;
        _productAttributeService = productAttributeService;
        _dataProvider = dataProvider;
        _orderService = orderService;
        _paymentService = paymentService;
        _paymentModelFactory = paymentModelFactory;
        _dateTimeHelper = dateTimeHelper;
        _logger = logger;
        _exportImportManager = exportImportManager;
        _customerActivityService = customerActivityService;
        _orderProcessingService = orderProcessingService;
        _genericAttributeService = genericAttributeService;
        _eventTicketsPdfService = eventTicketsPdfService;
        _orderSettings = orderSettings;
        _orderModelFactory = orderModelFactory;
        _shoppingCartSettings = shoppingCartSettings;
        _urlRecordService = urlRecordService;
        _productTemplateService = productTemplateService;
        _notificationService = notificationService;
        _pluginService = pluginService;
        _paymentPluginManager = paymentPluginManager;
        _shoppingCartService = shoppingCartService;
        _categoryService = categoryService;
        _eventTicketsInstallService = apolloInstallService;
        _cacheManager = cacheManager;
        _recentlyViewedProductsService = recentlyViewedProductsService;
        _urlRecordRepository = urlRecordRepository;
        _productTypeExtensionsService = productTypeExtensionsService;
        _eventTicketsAccessSettings = eventTicketsAccessSettings;
        _fileProvider = fileProvider;
        _shippingPluginManager = shippingPluginManager;
        _eventTicketSettings = eventTicketSettings;
        _webHelper = webHelper;
        _shipmentService = shipmentService;
        _ticketModelFactory = ticketModelFactory;
        _vendorService = vendorService;
        _apolloCoreService = apolloCoreService;
    }

    #endregion

    #region Utilities

    protected virtual async Task LogEditOrderAsync(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);

        await _customerActivityService.InsertActivityAsync("EditOrder",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
    }

    protected virtual async Task LogEditTicketAsync(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);

        //activity log
        await _customerActivityService.InsertActivityAsync("EditProduct",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);
    }

    protected virtual async ValueTask<bool> HasAccessToOrderItemAsync(OrderItem orderItem)
    {
        if (orderItem == null)
            throw new ArgumentNullException(nameof(orderItem));

        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            //not a vendor; has access
            return true;

        var vendorId = vendor.Id;
        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

        return product.VendorId == vendorId;
    }

    protected async virtual Task<bool> HasAccessToProductItemAsync(ProductTypeExtension productItem)
    {
        if (productItem == null)
            throw new ArgumentNullException(nameof(productItem));

        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            //not a vendor; has access
            return true;

        var vendorId = vendor.Id;
        var product = await _productService.GetProductByIdAsync(productItem.ProductId);

        return product.VendorId == vendorId;
    }


    #endregion

    #region Configure Methods

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<ActionResult> Configure()
    {
        ConfigureModel model = new ConfigureModel();

        model = await _eventTicketsModelFactory.PrepareConfigurationModelAsync(model);

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<ActionResult> Configure(ConfigureModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var apolloConfigSettings = await _settingService.LoadSettingAsync<EventTicketSettings>(storeScope);

        var eventTicketsAccessSettings = await _settingService.LoadSettingAsync<EventTicketsAccessSettings>();

        bool install = apolloConfigSettings.Enabled;
        bool enabled = model.Enabled;
        if (!enabled)
        {
            bool access = await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Configure, model.PublicKey, model.PrivateKey);
            if (!access)
                model.Enabled = enabled = false;
        }

        bool modelEnabled = apolloConfigSettings.Enabled;
        bool modelEventTicketsEnabled = apolloConfigSettings.EventTicketsEnabled;

        apolloConfigSettings.PublicKey = model.PublicKey;
        apolloConfigSettings.PrivateKey = model.PrivateKey;

        apolloConfigSettings.Enabled = model.Enabled;
        apolloConfigSettings.EventTicketsEnabled = model.EventTicketsEnabled;
        apolloConfigSettings.MaximumNumberOfOrders = model.MaximumNumberOfOrders;
        apolloConfigSettings.DeleteTablesOnUninstall = model.DeleteTablesOnUninstall;
        apolloConfigSettings.DeleteConfigurationDataOnUninstall = model.DeleteConfigurationDataOnUninstall;
        apolloConfigSettings.AttachFileLocation = model.AttachFileLocation;
        apolloConfigSettings.EventTicketsURLPattern = model.EventTicketsURLPattern;
        apolloConfigSettings.TestMode = model.TestMode;
        apolloConfigSettings.UseNopTemplatesAjaxCart = model.UseNopTemplatesAjaxCart;
        apolloConfigSettings.SeperateTickets = model.SeperateTickets;

        string str = string.Empty;
        if (model.ProductAttributeIds != null)
        {
            str = string.Join(",", model.ProductAttributeIds);
        }
        apolloConfigSettings.ProductAttributeIds = str;

        str = string.Empty;
        if (model.CheckoutAttributeIds != null)
        {
            str = string.Join(",", model.CheckoutAttributeIds);
        }
        apolloConfigSettings.CheckoutAttributeIds = str;

        await _settingService.SaveSettingAsync(apolloConfigSettings, x => x.PublicKey, 0, false);
        await _settingService.SaveSettingAsync(apolloConfigSettings, x => x.PrivateKey, 0, false);
        await _settingService.SaveSettingAsync(apolloConfigSettings, x => x.DeleteTablesOnUninstall, 0, false);
        await _settingService.SaveSettingAsync(apolloConfigSettings, x => x.DeleteConfigurationDataOnUninstall, 0, false);
        await _settingService.SaveSettingAsync(apolloConfigSettings, x => x.UseNopTemplatesAjaxCart);
        await _settingService.SaveSettingOverridablePerStoreAsync(apolloConfigSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(apolloConfigSettings, x => x.TestMode, model.TestMode_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(apolloConfigSettings, x => x.EventTicketsEnabled, model.EventTicketsEnabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(apolloConfigSettings, x => x.MaximumNumberOfOrders, model.MaximumNumberOfOrders_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(apolloConfigSettings, x => x.ProductAttributeIds, model.ProductAttributeId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(apolloConfigSettings, x => x.CheckoutAttributeIds, model.CheckoutAttributeId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(apolloConfigSettings, x => x.AttachFileLocation, model.AttachFileLocation_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(apolloConfigSettings, x => x.EventTicketsURLPattern, model.EventTicketsEnabled_OverrideForStore, storeScope, true);
        await _settingService.SaveSettingOverridablePerStoreAsync(apolloConfigSettings, x => x.SeperateTickets, model.SeperateTickets_OverrideForStore, storeScope, true);

        //Only install sample data for All Stores

        if (storeScope == 0)
        {
            if (modelEnabled && !apolloConfigSettings.Enabled)
                await _eventTicketsInstallService.InstallLocalisationAsync(false);
            else if (!modelEnabled && apolloConfigSettings.Enabled)
                await _eventTicketsInstallService.InstallLocalisationAsync(true);

            if (modelEventTicketsEnabled && !apolloConfigSettings.EventTicketsEnabled)
                await _eventTicketsInstallService.InstallEventTicketsProductsAsync(false);
            else if (!modelEventTicketsEnabled && apolloConfigSettings.EventTicketsEnabled)
                await _eventTicketsInstallService.InstallEventTicketsProductsAsync(true);
        }

        await _eventTicketService.LoadSystemAccessAsync();

        //now clear settings cache
        await _cacheManager.ClearAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion

    #region Manage Methods

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async Task<IActionResult> Manage()
    {
        ManagementModel model = new ManagementModel();

        //var localResources = _apolloCorePlugin.GetPluginResources();
        //await _localizationService.AddOrUpdateLocaleResourceAsync(localResources);

        model = await _eventTicketsModelFactory.PrepareManagementModelAsync(model);

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/Manage/Manage.cshtml", model);
    }

    [HttpPost]
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async Task<IActionResult> Manage(ConfigureModel model)
    {

        if (await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
        {
            if (!ModelState.IsValid)
                return await Manage();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var apolloConfigSettings = await _settingService.LoadSettingAsync<EventTicketSettings>(storeScope);

        }

        return await Manage();
    }

    #endregion

    #region Event Tickets

    public virtual IActionResult Index()
    {
        return RedirectToAction("Display");
    }

    #endregion

    #region Event Tickets Front

    public async Task <IActionResult> EventTicketsFront(string seName, int updateCartItemId = 0)
    {
        if (string.IsNullOrEmpty(seName))
            throw new ArgumentNullException("No product found with the specified SeName");

        var urlRecord = await _urlRecordService.GetBySlugAsync(seName);
        if (urlRecord == null)
            return RedirectToRoute("HomePage");

        var product = await _productService.GetProductByIdAsync(urlRecord.EntityId);
        if (product != null)
        {

            //visible individually : //ToDo
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("HomePage");

                return RedirectToRoute("Product", new { SeName = await _urlRecordService.GetSeNameAsync(parentGroupedProduct) });
            }

            //availability dates
            var available = _productService.ProductIsAvailable(product);
            if (available)
            {
                //save as recently viewed
                await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(product.Id);

                //display "edit" (manage) link
                if (await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) &&
                    await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW))
                {
                    //a vendor should have access only to his products
                    if (await _workContext.GetCurrentVendorAsync() == null || (await _workContext.GetCurrentVendorAsync()).Id == product.VendorId)
                    {
                        DisplayEditLink(Url.Action("Edit", "Ticket", new { id = product.Id, area = AreaNames.ADMIN }));
                    }
                }
            }
            else
            {
                return RedirectToRoute("HomePage");
            }

            //update existing shopping cart or wishlist item?
            ShoppingCartItem updateCartItem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updateCartItemId > 0)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), 
                    ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                updateCartItem = cart.FirstOrDefault(x => x.Id == updateCartItemId);
            }

            var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id,
                CoreSystemHelper.RecordType.Configuration, CoreSystemHelper.SystemMode.EventTickets);

            if (pte != null)
            { 

                var model = await _productModelFactory.PrepareProductDetailsModelAsync(product, updateCartItem);

                var eventTicketProductDetailsModel = await _eventTicketsModelFactory.PrepareEventTicketProductDetailsModelAsync(model, pte);

                var productTemplate = await _productTemplateService.GetProductTemplateByIdAsync(product.ProductTemplateId);

                if (productTemplate != null)
                {
                    string url = string.Empty;
                    var storeTheme = await _settingService.GetSettingByKeyAsync("storeinformationsettings.defaultstoretheme", "DefaultClean", (await _storeContext.GetCurrentStoreAsync()).Id, true);

                    if (product.ProductType == ProductType.GroupedProduct)
                    {
                        url = string.Format("~/Plugins/SSI.Apollo.EventTickets/Views/FrontView/Shared/Themes/Kingdom/ProductTemplate.Grouped.cshtml", storeTheme);
                    }
                    else if (product.ProductType == ProductType.SimpleProduct)
                    {
                        url = string.Format("~/Plugins/SSI.Apollo.EventTickets/Views/FrontView/Shared/Themes/Kingdom/ProductTemplate.EventTickets.cshtml", storeTheme);
                    }

                    return View(url, eventTicketProductDetailsModel);
                }
            }
        }

        return RedirectToRoute("HomePage");
    }

    #endregion

    #region Event Tickets Request

    public async Task<IActionResult> TicketRequest(string type, string requestId, string option)
    {
        Order order = null;
        int orderNumber = 0;
        int ticketNumber = 0;
        string ticketNumberString = string.Empty;

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsGuestAsync(customer))
        {

            if (type != null && type.Equals("ticket"))
            {
                if (requestId != null)
                {

                    if (!requestId.Contains("-"))
                    {
                        //Load order by identifier (if provided)
                        bool status = Int32.TryParse(requestId, out orderNumber);
                        if (status && orderNumber != 0)
                            order = await _orderService.GetOrderByIdAsync(orderNumber);
                    }
                    else
                    {
                        var requestSplit = requestId.Split("-");
                        if (requestSplit.Count() == 2)
                        {
                            //Load order by identifier (if provided)
                            bool status = Int32.TryParse(requestSplit[0], out orderNumber);
                            if (status && orderNumber != 0)
                                order = await _orderService.GetOrderByIdAsync(orderNumber);

                            status = Int32.TryParse(requestSplit[1], out ticketNumber);
                            if (status && ticketNumber != 0)
                                ticketNumberString = ticketNumber.ToString();
                        }
                    }
                }

                if (order != null && order.Deleted)
                {
                    //Order not active or does not belong to current customer 
                    return RedirectToRoute("HomePage");
                }

                if (!await _apolloCoreService.IsEventTicketOrderAsync(order))
                {
                    //Order not a ticket order
                    return RedirectToRoute("HomePage");
                }

                string url = _webHelper.GetStoreLocation() + "Plugins/EventTickets/TicketRequestError" +
                        "?check=" + customer.CustomerGuid + "&type=request&requestId=0";

                if (order != null)
                {

                    var registered = (await _customerService.GetAllCustomerRolesAsync()).Where(x => x.Name == "Registered").FirstOrDefault();
                    var scanner = (await _customerService.GetAllCustomerRolesAsync()).Where(x => x.Name == "Scanner").FirstOrDefault();
                    var manager = (await _customerService.GetAllCustomerRolesAsync()).Where(x => x.Name == "Manager").FirstOrDefault();

                    var roles = await _customerService.GetCustomerRoleIdsAsync(customer, false);

                    if (scanner != null && roles.Contains(scanner.Id))
                    {
                        if (order.OrderStatus != OrderStatus.Complete)
                            url = _webHelper.GetStoreLocation() + "Plugins/EventTickets/TicketRequestError" +
                                    "?check=" + customer.CustomerGuid + "&type=request&requestId=" + requestId;
                        else
                        {
                            if (option != null && option.Equals("DisplayOrder"))
                                url = _webHelper.GetStoreLocation() + "Plugins/EventTickets/TicketRequestDisplayOrder" +
                                        "?check=" + customer.CustomerGuid + "&type=request&requestId=" + order.Id.ToString();
                            else
                                url = _webHelper.GetStoreLocation() + "Plugins/EventTickets/TicketRequestDisplay" +
                                        "?check=" + customer.CustomerGuid + "&type=request&requestId=" + order.Id.ToString();

                            if (!string.IsNullOrEmpty(ticketNumberString))
                                url += "&ticketNumber=" + ticketNumberString;

                        }

                        return Redirect(url);
                    }
                    else if (manager != null && roles.Contains(manager.Id))
                    {
                        if (order != null)
                        {
                            url = _webHelper.GetStoreLocation() + "Admin/Order/Edit/" + requestId;
                            return Redirect(url);
                        }
                    }
                    else if (registered != null && roles.Contains(registered.Id))
                    {
                        if (customer.Id != order.CustomerId)
                        {
                            //Order does not belong to current customer 
                            return RedirectToRoute("HomePage");
                        }
                        else if (order != null)
                        {
                            url = _webHelper.GetStoreLocation() + "OrderDetails/" + requestId;
                            return Redirect(url);
                        }
                    }
                }
            }
        }

        return RedirectToRoute("HomePage");

    }

    public async Task<IActionResult> TicketRequestDisplay(string check, string type, int requestId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var isGuest = await _customerService.IsGuestAsync(customer);

        if (!isGuest)
        {
            if (type != null && type.Equals("request") && check != null && check.Equals(customer.CustomerGuid.ToString()))
            {
                var order = await _orderService.GetOrderByIdAsync(requestId);

                if (order != null && !order.Deleted)
                {
                    var model = await _eventTicketsModelFactory.PrepareTicketRequestModelAsync(order);
                    return View("~/Plugins/SSI.Apollo.EventTickets/Views/FrontView/TicketRequestDisplay.cshtml", model);
                }
            }
        }

        return RedirectToRoute("HomePage");
    }

    public async Task<IActionResult> TicketRequestDisplayOrder(string check, string type, int requestId)
    {

        var customer = await _workContext.GetCurrentCustomerAsync();
        var isGuest = await _customerService.IsGuestAsync(customer);

        if (!isGuest)
        {
            if (type != null && type.Equals("request") && check != null && check.Equals(customer.CustomerGuid.ToString()))
            {
                var order = await _orderService.GetOrderByIdAsync(requestId);
                if (order != null && !order.Deleted)
                {
                    var model = await _eventTicketsModelFactory.PrepareTicketRequestModelAsync(order);

                    return View("~/Plugins/SSI.Apollo.EventTickets/Views/FrontView/TicketRequestDisplayOrder.cshtml", model);
                }
            }
        }

        return RedirectToRoute("HomePage");
    }

    public async Task<IActionResult> TicketRequestError(string check, string type, int requestId)
    {

        var customer = await _workContext.GetCurrentCustomerAsync();
        var isGuest = await _customerService.IsGuestAsync(customer);

        if (!isGuest)
        {
            if (type != null && type.Equals("request") && check != null && check.Equals(customer.CustomerGuid.ToString()))
            {
                var model = new TicketRequestModel();
                return View("~/Plugins/SSI.Apollo.EventTickets/Views/FrontView/TicketRequestError.cshtml", model);
            }
        }

        return RedirectToRoute("HomePage");
    }

    #endregion

    #region Product Functions

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<IActionResult> GetAssociatedProductsByGroupId(int groupProductId)
    {
        int vendorId = 0;
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
            vendorId = vendor.Id;

        if (groupProductId == 0)
            return Json(null);

        var associatedProducts = await _productService.GetAssociatedProductsAsync(groupProductId, 
            (await _storeContext.GetCurrentStoreAsync()).Id, vendorId: vendorId, showHidden: true);
        if (associatedProducts != null)
        {
            var result = (from s in associatedProducts select new { id = s.Id, name = s.Name }).ToList();
            return Json(result);
        }

        return (null);
    }

    public async Task<IActionResult> ConvertCurrencyToCurrentStore(decimal total = 0)
    {
        var totalbookingprice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(total, await _workContext.GetWorkingCurrencyAsync());
        var totalbookingprice_wc = await _priceFormatter.FormatPriceAsync(totalbookingprice);
        return Json(new { Totalbookingprice = totalbookingprice_wc });
    }

    public async Task<decimal> ConvertCostToCurrentStoreCurrency(decimal total = 0)
    {
        var totalbookingprice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(total, await _workContext.GetWorkingCurrencyAsync());
        return totalbookingprice;
    }

    public async Task<IActionResult> FormatCurrencyForCurrentStore(decimal total = 0)
    {
        var totalbookingprice_wc = await _priceFormatter.FormatPriceAsync(total);
        return Json(new { Totalbookingprice = totalbookingprice_wc });
    }

    #endregion



    #region Order sales

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<IActionResult> EventTicketsSales()
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Operate))
            return AccessDeniedView();

        //prepare model
        var model = await PrepareEventTicketsSalesSearchModelAsync(new EventTicketsSalesSearchModel());

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EventTicketsSales.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<ActionResult> EventTicketsSalesListUpdate()
    {
        var eventTicketsOrder = new List<OrderItem>();
        foreach (var item in eventTicketsOrder)
        {
            var order = await _orderService.GetOrderItemByIdAsync(item.OrderId);
        }

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EventTicketsSales.cshtml");
    }

    private async Task<EventTicketsSalesSearchModel> PrepareEventTicketsSalesSearchModelAsync(EventTicketsSalesSearchModel searchModel)
    {
        var methodsModel = new PaymentMethodRestrictionModel();
        var paymentService = await _paymentModelFactory.PreparePaymentMethodRestrictionModelAsync(methodsModel);
        searchModel.AvailablePaymentMethods.Add(new SelectListItem()
        {
            Text = "All"
        });

        int storeId = 0;
        int vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        if (_eventTicketSettings.UseGroupProducts)
        {
            var groupedProducts = await _eventTicketService.GetAllGroupProducts(CoreSystemHelper.SystemMode.EventTickets,
                CoreSystemHelper.RecordType.Configuration, storeId, vendorId);

            if (groupedProducts != null)
            {
                searchModel.AvailableGroupProducts.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.SelectGroupProduct") });
                foreach (var gp in groupedProducts)
                    searchModel.AvailableGroupProducts.Add(new SelectListItem { Value = gp.Id.ToString(), Text = gp.Name });

                searchModel.AvailableAssociatedProducts.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.SelectAssociatedProduct") });
            }
        }
        else if (_eventTicketSettings.UseAssociatedProducts)
        {
            var associatedProducts = _eventTicketService.GetAllAssociatedProducts(CoreSystemHelper.SystemMode.EventTickets, CoreSystemHelper.RecordType.Configuration, vendorId);
            searchModel.AvailableAssociatedProducts.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.SelectAssociatedProduct") });
            foreach (var ap in associatedProducts)
                searchModel.AvailableAssociatedProducts.Add(new SelectListItem { Value = ap.Id.ToString(), Text = ap.Name });
        }
        else
        {
            searchModel.SearchProductName = string.Empty;
        }

        foreach (var method in paymentService.AvailablePaymentMethods)
        {
            searchModel.AvailablePaymentMethods.Add(new SelectListItem()
            {
                Text = method.FriendlyName,
                Value = method.SystemName
            });
        }

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<IActionResult> EventTicketsSalesList(EventTicketsSalesSearchModel searchModel)
    {
        //prepare model
        var model = await PrepareEventTicketsSalesListModelAsync(searchModel);

        return Json(model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual ActionResult EventTicketsSalesListDelete(EventTicketsSalesModel model)
    {
        //_apolloOrderMappingService.IsDeleteAccommodationOrderMapping(Model.Id);

        return new NullJsonResult();
    }

    public virtual async Task<EventTicketsSalesListModel> PrepareEventTicketsSalesListModelAsync(EventTicketsSalesSearchModel searchModel)
    {

        var orderItemIds = await _eventTicketService.GetAllEventTicketsSalesItemsAsync();
        var orderItem = await _eventTicketService.GetEventTicketsSalesOrderItemsListAsync(orderItemIds, searchModel.PaymentMethod,
                searchModel.FDate, searchModel.TDate, searchModel.Ispay, searchModel.OrderbyName,
                searchModel.GroupProductId, searchModel.AssociatedProductId, searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new EventTicketsSalesListModel().PrepareToGridAsync(searchModel, orderItem, () =>
        {
            //fill in model values from the entity

            var eventTicketsList = orderItem.SelectAwait(async orderItem =>
            {
                var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                var getParentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
                var paymentMethodFriendlyName = paymentMethod != null ?
                    await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.PaymentMethodSystemName;
                var shippingMethod = await _shippingPluginManager.LoadPluginBySystemNameAsync(order.ShippingRateComputationMethodSystemName);
                var shippingMethodFriendlyName = shippingMethod != null ? 
                    await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.ShippingRateComputationMethodSystemName;
                var startDateValue = string.Empty;

                try
                {

                    startDateValue = "Not Paid";
                    var currentTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
                    if (order.PaidDateUtc != null)
                    {
                        var paidDate = await _dateTimeHelper.ConvertToUserTimeAsync(order.PaidDateUtc.Value, DateTimeKind.Utc);
                        startDateValue = paidDate.ToString(await _apolloCoreService.GetLocalisedDisplayShortDateFormatAsync());
                    }
                }
                catch (Exception ex)
                {
                    var error = ex;
                }

                var productName = product.Name;
                if (getParentProduct != null)
                    productName = getParentProduct.Name + " - " + productName;

                var orders = new EventTicketsSalesModel()
                {
                    Id = orderItem.Id,
                    OrderItemId = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    Name = productName,
                    ParentProduct = (getParentProduct != null ? getParentProduct.Name : "") + " (" + product.Name + " )",
                    AttributeDescription = orderItem.AttributeDescription,
                    CustomerEmail = await _eventTicketService.GetCustomerEmailDetailsAsync(orderItem),
                    CustomerFullName = await _eventTicketService.GetCustomerFullNameDetailsAsync(orderItem),
                    OrderId = orderItem.OrderId,
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    PaymentDate = startDateValue,
                    Quantity = orderItem.Quantity,
                    Price = await _priceFormatter.FormatPriceAsync(orderItem.PriceInclTax, true, false),
                    TotalPrice = await _priceFormatter.FormatPriceAsync(orderItem.PriceInclTax, true, false),
                    PaymentMethodSystemName = paymentMethodFriendlyName,
                    ShippingMethodSystemName = shippingMethodFriendlyName,
                    DisplayUrl = GetEditOrderPageUrl(orderItem.OrderId)
                };

                return orders;
            });

            if (searchModel.OrderbyName)
            {
                eventTicketsList = eventTicketsList.OrderBy(x => x.CustomerFullName);
            }

            return eventTicketsList;
        });

        return model;
    }

    public string GetEditOrderPageUrl(int orderId)
    {
        return _webHelper.GetStoreLocation() + "Admin/Order/Edit/" + orderId.ToString();
    }

    public string GetEditProductPageUrl(int productId)
    {
        return _webHelper.GetStoreLocation() + "Admin/Ticket/Edit/" + productId.ToString();
    }

    public async Task<string> GetEditShipmentPageUrlAsync(int orderId)
    {
        var shipment = await _shipmentService.GetShipmentsByOrderIdAsync(orderId);
        if (shipment != null && shipment.Count != 0)
            return _webHelper.GetStoreLocation() + "Admin/Order/ShipmentDetails/" + shipment.FirstOrDefault().Id.ToString();
        else
            return _webHelper.GetStoreLocation() + "Admin/Order/ShipmentList/";
    }

    #endregion

    #region Order Delivery

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<IActionResult> EventTicketsDelivery()
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Operate))
            return AccessDeniedView();

        //prepare model
        var model = await PrepareEventTicketsDeliverySearchModelAsync(new EventTicketsDeliverySearchModel());

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EventTicketsDelivery.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<ActionResult> EventTicketsDeliveryListUpdate()
    {
        var eventTicketsOrder = new List<OrderItem>();
        foreach (var item in eventTicketsOrder)
        {
            var order = await _orderService.GetOrderItemByIdAsync(item.OrderId);
        }

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EventTicketsDelivery.cshtml");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual ActionResult EventTicketsDeliveryListDelete(EventTicketsDeliveryModel model)
    {
        //_apolloOrderMappingService.IsDeleteAccommodationOrderMapping(Model.Id);

        return new NullJsonResult();
    }

    private async Task<EventTicketsDeliverySearchModel> PrepareEventTicketsDeliverySearchModelAsync(EventTicketsDeliverySearchModel searchModel)
    {
        var methodsModel = new PaymentMethodRestrictionModel();
        var paymentService = await _paymentModelFactory.PreparePaymentMethodRestrictionModelAsync(methodsModel);
        searchModel.AvailablePaymentMethods.Add(new SelectListItem()
        {
            Text = "All"
        });

        int storeId = 0;
        if (await _storeContext.GetCurrentStoreAsync() != null)
            storeId = _storeContext.GetCurrentStoreAsync().Id;

        int vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        var groupedProducts = await _eventTicketService.GetAllGroupProducts(CoreSystemHelper.SystemMode.System,
            CoreSystemHelper.RecordType.Configuration, storeId, vendorId);

        if (groupedProducts != null)
        {
            searchModel.AvailableGroupProducts.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.SelectGroupProduct") });
            foreach (var gp in groupedProducts)
                searchModel.AvailableGroupProducts.Add(new SelectListItem { Value = gp.Id.ToString(), Text = gp.Name });

            searchModel.AvailableAssociatedProducts.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.SelectAssociatedProduct") });
        }

        foreach (var method in paymentService.AvailablePaymentMethods)
        {
            searchModel.AvailablePaymentMethods.Add(new SelectListItem()
            {
                Text = method.FriendlyName,
                Value = method.SystemName
            });
        }

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<IActionResult> EventTicketsDeliveryList(EventTicketsDeliverySearchModel searchModel)
    {
        //prepare model
        var model = await PrepareEventTicketsDeliveryListModel(searchModel);

        return Json(model);
    }

    public virtual async Task<EventTicketsDeliveryListModel> PrepareEventTicketsDeliveryListModel(EventTicketsDeliverySearchModel searchModel)
    {

        var orderItemIds = await _eventTicketService.GetAllEventTicketsDeliveryItemsAsync();
        var query = await _eventTicketService.GetEventTicketsDeliveryOrderItemsListAsync(orderItemIds, searchModel.PaymentMethod,
                searchModel.FDate, searchModel.TDate, searchModel.Ispay, searchModel.OrderbyName,
                searchModel.GroupProductId, searchModel.AssociatedProductId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new EventTicketsDeliveryListModel().PrepareToGridAsync(searchModel, query, () =>
        {
            //fill in model values from the entity

            var eventTicketsList = query.ToList().SelectAwait(async orderItem =>
            {
                var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                var getParentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
                var paymentMethodFriendlyName = paymentMethod != null ?
                    await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.PaymentMethodSystemName;
                var shippingMethod = await _shippingPluginManager.LoadPluginBySystemNameAsync(order.ShippingRateComputationMethodSystemName);
                var shippingMethodFriendlyName = shippingMethod != null ?
                    await _localizationService.GetLocalizedFriendlyNameAsync(shippingMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.ShippingRateComputationMethodSystemName;
                var startDateValue = string.Empty;
                try
                {
                    startDateValue = order.PaidDateUtc == null ? "Not Paid" :
                        _dateTimeHelper.ConvertToUserTime(order.PaidDateUtc.Value, TimeZoneInfo.Utc,
                        await _dateTimeHelper.GetCustomerTimeZoneAsync(await _workContext.GetCurrentCustomerAsync()))
                            .ToString(await _apolloCoreService.GetLocalisedDisplayShortDateFormatAsync());
                }
                catch (Exception ex)
                {
                    var error = ex;
                }

                var productName = product.Name;
                if (getParentProduct != null)
                    productName = getParentProduct.Name + " - " + productName;

                var orders = new EventTicketsDeliveryModel()
                {
                    Id = orderItem.Id,
                    OrderItemId = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    Name = productName,
                    ParentProduct = (getParentProduct != null ? getParentProduct.Name : "") + " (" + product.Name + " )",
                    AttributeDescription = orderItem.AttributeDescription,
                    CustomerEmail = await _eventTicketService.GetCustomerEmailDetailsAsync(orderItem),
                    CustomerFullName = await _eventTicketService.GetCustomerFullNameDetailsAsync(orderItem),
                    OrderId = orderItem.OrderId,
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    PaymentDate = startDateValue,
                    Quantity = orderItem.Quantity,
                    Price = await _priceFormatter.FormatPriceAsync(orderItem.PriceInclTax, true, false),
                    TotalPrice = await _priceFormatter.FormatPriceAsync(orderItem.PriceInclTax, true, false),
                    PaymentMethodSystemName = paymentMethodFriendlyName,
                    ShippingMethodSystemName = shippingMethodFriendlyName,
                    DisplayUrl = GetEditOrderPageUrl(orderItem.OrderId),
                    ShipmentUrl = await GetEditShipmentPageUrlAsync(orderItem.OrderId)
                };

                return orders;
            });

            if (searchModel.OrderbyName)
            {
                eventTicketsList = eventTicketsList.OrderBy(x => x.CustomerFullName);
            }

            return eventTicketsList;

        });

        return model;
    }

    #endregion

    #region Order Process

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<IActionResult> EventTicketsProcess()
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Operate))
            return AccessDeniedView();

        //prepare model
        var model = await PrepareEventTicketsProcessSearchModelAsync(new EventTicketsProcessSearchModel());

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EventTicketsProcess.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<IActionResult> EventTicketsProcessListUpdate()
    {
        var eventTicketsOrder = new List<OrderItem>();
        foreach (var item in eventTicketsOrder)
        {
            var order = await _orderService.GetOrderItemByIdAsync(item.OrderId);
        }

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EventTicketsProcess.cshtml");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual ActionResult EventTicketsProcessListDelete(EventTicketsProcessModel model)
    {
        //_apolloOrderMappingService.IsDeleteAccommodationOrderMapping(Model.Id);

        return new NullJsonResult();
    }

    private async Task<EventTicketsProcessSearchModel> PrepareEventTicketsProcessSearchModelAsync(EventTicketsProcessSearchModel searchModel)
    {
        var methodsModel = new PaymentMethodRestrictionModel();
        var paymentService = await _paymentModelFactory.PreparePaymentMethodRestrictionModelAsync(methodsModel);
        searchModel.AvailablePaymentMethods.Add(new SelectListItem()
        {
            Text = "All"
        });

        int storeId = 0;
        int vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        var groupedProducts = await _eventTicketService.GetAllGroupProducts(CoreSystemHelper.SystemMode.EventTickets,
            CoreSystemHelper.RecordType.Configuration, storeId, vendorId);

        if (groupedProducts != null)
        {
            searchModel.AvailableGroupProducts.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.SelectGroupProduct") });
            foreach (var gp in groupedProducts)
                searchModel.AvailableGroupProducts.Add(new SelectListItem { Value = gp.Id.ToString(), Text = gp.Name });

            searchModel.AvailableAssociatedProducts.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.SelectAssociatedProduct") });
        }

        foreach (var method in paymentService.AvailablePaymentMethods)
        {
            searchModel.AvailablePaymentMethods.Add(new SelectListItem()
            {
                Text = method.FriendlyName,
                Value = method.SystemName
            });
        }

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<IActionResult> EventTicketsProcessList(EventTicketsProcessSearchModel searchModel)
    {
        //prepare model
        var model = await PrepareEventTicketsProcessListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<EventTicketsProcessListModel> PrepareEventTicketsProcessListModelAsync(EventTicketsProcessSearchModel searchModel)
    {

        var orderItemIds = await _eventTicketService.GetAllEventTicketsProcessItemsAsync();
        var orderItem = await _eventTicketService.GetEventTicketsProcessOrderItemsListAsync(orderItemIds, searchModel.PaymentMethod,
                searchModel.FDate, searchModel.TDate, searchModel.Ispay, searchModel.OrderbyName,
                searchModel.GroupProductId, searchModel.AssociatedProductId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new EventTicketsProcessListModel().PrepareToGridAsync(searchModel, orderItem, () =>
        {
            //fill in model values from the entity

            var eventTicketsList = orderItem.SelectAwait(async orderItem =>
            {
                var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                var getParentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
                var paymentMethodFriendlyName = paymentMethod != null ?
                    await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.PaymentMethodSystemName;
                var shippingMethod = await _shippingPluginManager.LoadPluginBySystemNameAsync(order.ShippingRateComputationMethodSystemName);
                var shippingMethodFriendlyName = shippingMethod != null ?
                    await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.ShippingRateComputationMethodSystemName;
                var startDateValue = string.Empty;

                try
                {

                    startDateValue = "Not Paid";
                    var currentTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
                    if (order.PaidDateUtc != null)
                    {
                        var paidDate = await _dateTimeHelper.ConvertToUserTimeAsync(order.PaidDateUtc.Value, DateTimeKind.Utc);
                        startDateValue = paidDate.ToString(await _apolloCoreService.GetLocalisedDisplayShortDateFormatAsync());
                    }
                }
                catch (Exception ex)
                {
                    var error = ex;
                }

                var localOrderDateTime = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);
                var orderDate = await _apolloCoreService.GetLocalisedDateAsync(localOrderDateTime, null);

                var productName = product.Name;
                if (getParentProduct != null)
                    productName = getParentProduct.Name + " - " + productName;

                var orders = new EventTicketsProcessModel()
                {
                    Id = orderItem.Id,
                    OrderItemId = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    Name = productName,
                    ParentProduct = (getParentProduct != null ? getParentProduct.Name : "") + " (" + product.Name + " )",
                    AttributeDescription = orderItem.AttributeDescription,
                    CustomerEmail = await _eventTicketService.GetCustomerEmailDetailsAsync(orderItem),
                    CustomerFullName = await _eventTicketService.GetCustomerFullNameDetailsAsync(orderItem),
                    OrderId = orderItem.OrderId,
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    PaymentDate = startDateValue,
                    Quantity = orderItem.Quantity,
                    Price = await _priceFormatter.FormatPriceAsync(orderItem.PriceInclTax, true, false),
                    TotalPrice = await _priceFormatter.FormatPriceAsync(orderItem.PriceInclTax, true, false),
                    PaymentMethodSystemName = paymentMethodFriendlyName,
                    DisplayUrl = GetEditOrderPageUrl(orderItem.OrderId),
                    OrderDate = orderDate
                };

                return orders;
            });

            if (searchModel.OrderbyName)
            {
                eventTicketsList = eventTicketsList.OrderBy(x => x.CustomerFullName);
            }

            return eventTicketsList;
        });

        return model;
    }

    #endregion

    #region Order Settlement

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async virtual Task<IActionResult> EventTicketsSettlement()
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Operate))
            return AccessDeniedView();

        //prepare model
        var model = await PrepareEventTicketsSettlementSearchModel(new EventTicketsSettlementSearchModel());

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EventTicketsSettlement.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async virtual Task<ActionResult> EventTicketsSettlementListUpdate()
    {
        var eventTicketsOrder = new List<OrderItem>();
        foreach (var item in eventTicketsOrder)
        {
            var order = await _orderService.GetOrderItemByIdAsync(item.OrderId);
        }

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EventTicketsSettlement.cshtml");
    }

    private async Task<EventTicketsSettlementSearchModel> PrepareEventTicketsSettlementSearchModel(EventTicketsSettlementSearchModel searchModel)
    {

        var groupedProducts = _eventTicketService.GetAllEventTicketsSettlementMapping();
        if (groupedProducts != null)
        {
            searchModel.AvailableGroupProducts.Add(new SelectListItem { Value = "0", 
                Text = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.SelectGroupProduct") });
            foreach (var gp in groupedProducts)
            {
                var product = await _productService.GetProductByIdAsync(gp.Id);
                if (product != null)
                    searchModel.AvailableGroupProducts.Add(new SelectListItem { Value = product.Id.ToString(), Text = product.Name });
            }
        }

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public virtual async Task<IActionResult> EventTicketsSettlementList(EventTicketsSettlementSearchModel searchModel)
    {
        //prepare model
        var model = await PrepareEventTicketsSettlementListModel(searchModel);

        return Json(model);
    }

    public async virtual Task<EventTicketsSettlementListModel> PrepareEventTicketsSettlementListModel(EventTicketsSettlementSearchModel searchModel)
    {

        var productItemIds = await _eventTicketService.GetAllEventTicketsSettlementItemsAsync();
        var productItem = _eventTicketService.GetEventTicketsSettlementItemsList(productItemIds, searchModel.FDate, searchModel.TDate,
            searchModel.GroupProductId, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new EventTicketsSettlementListModel().PrepareToGridAsync(searchModel, productItem, () =>
        {
            //fill in model values from the entity

            var eventTicketsList = productItem.SelectAwait(async productItem =>
            {
                var product = await _productService.GetProductByIdAsync(productItem.ProductId);

                (int sold, decimal cost, decimal sales) = await _eventTicketService.GetAllEventTicketsSoldValuesForReportAsync(product);

                var products = new EventTicketsSettlementModel()
                {
                    Id = productItem.ProductId,
                    ProductId = productItem.ProductId,
                    Name = product.Name,
                    Quantity = sold,
                    TotalCost = await _priceFormatter.FormatPriceAsync(cost, false, true),
                    TotalSales = await _priceFormatter.FormatPriceAsync(sales, false, true),
                    DisplayUrl = GetEditProductPageUrl(productItem.ProductId)
                };

                return products;
            });

            return eventTicketsList;
        });

        return model;
    }

    #endregion

    #region Mark Order As 

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> MarkOrderAsPaid(string selectedIds)
    {

        if (selectedIds != null && selectedIds.Count() != 0)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            for (int i = 0; i < ids.Count(); i++)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                    return AccessDeniedView();

                //try to get an order with the specified id
                var ordrItemId = await _orderService.GetOrderItemByIdAsync(ids[i]);
                var order = await _orderService.GetOrderByIdAsync(ordrItemId.OrderId);

                try
                {
                    if (order.PaymentStatus != PaymentStatus.Paid)
                    {
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);
                        await LogEditOrderAsync(order.Id);
                    }
                }
                catch (Exception exc)
                {
                    //prepare model
                    var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);
                    await _notificationService.ErrorNotificationAsync(exc, false);
                    return RedirectToAction("EventTicketsSales");
                }
            }

        }

        return RedirectToAction("EventTicketsSales");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> MarkOrderAsApproved(string selectedIds)
    {

        if (selectedIds != null && selectedIds.Count() != 0)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            for (int i = 0; i < ids.Count(); i++)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                    return AccessDeniedView();

                //try to get an order with the specified id
                var ordrItemId = await _orderService.GetOrderItemByIdAsync(ids[i]);
                var order = await _orderService.GetOrderByIdAsync(ordrItemId.OrderId);

                try
                {
                    if (_eventTicketService.CanMarkOrderAsApproved(order))
                    {
                        await _eventTicketService.MarkOrderAsApprovedAsync(order);
                        await LogEditOrderAsync(order.Id);
                    }
                }
                catch (Exception exc)
                {
                    //prepare model
                    var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);
                    await _notificationService.ErrorNotificationAsync(exc, false);
                    return RedirectToAction("EventTicketsSales");
                }
            }

        }

        return RedirectToAction("EventTicketsSales");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> MarkOrderReturnedToBeChecked(string selectedIds)
    {

        if (selectedIds != null && selectedIds.Count() != 0)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            for (int i = 0; i < ids.Count(); i++)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                    return AccessDeniedView();

                //try to get an order with the specified id
                var ordrItemId = await _orderService.GetOrderItemByIdAsync(ids[i]);
                var order = await _orderService.GetOrderByIdAsync(ordrItemId.OrderId);

                try
                {
                    if (_eventTicketService.CanMarkOrderReturnedToBeChecked(order))
                    {
                        await _eventTicketService.MarkOrderReturnedToBeCheckedAsync(order);
                        await LogEditOrderAsync(order.Id);
                    }
                }
                catch (Exception exc)
                {
                    //prepare model
                    var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);
                    await _notificationService.ErrorNotificationAsync(exc, false);
                    return RedirectToAction("EventTicketsProcess");
                }
            }

        }

        return RedirectToAction("EventTicketsProcess");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> MarkOrderAsProcessed(string selectedIds)
    {

        if (selectedIds != null && selectedIds.Count() != 0)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            for (int i = 0; i < ids.Count(); i++)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                    return AccessDeniedView();

                //try to get an order with the specified id
                var ordrItemId = await _orderService.GetOrderItemByIdAsync(ids[i]);
                var order = await _orderService.GetOrderByIdAsync(ordrItemId.OrderId);

                try
                {
                    if (_eventTicketService.CanMarkOrderAsProcessed(order))
                    {
                        await _eventTicketService.MarkOrderAsProcessedAsync(order);
                        await LogEditOrderAsync(order.Id);
                    }
                }
                catch (Exception exc)
                {
                    //prepare model
                    var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);
                    await _notificationService.ErrorNotificationAsync(exc, false);
                    return RedirectToAction("EventTicketsProcess");
                }
            }

        }

        return RedirectToAction("EventTicketsProcess");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> MarkOrderReturnedToBeProcessed(string selectedIds)
    {

        if (selectedIds != null && selectedIds.Count() != 0)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            for (int i = 0; i < ids.Count(); i++)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                    return AccessDeniedView();

                //try to get an order with the specified id
                var ordrItemId = await _orderService.GetOrderItemByIdAsync(ids[i]);
                var order = await _orderService.GetOrderByIdAsync(ordrItemId.OrderId);

                try
                {
                    if (_eventTicketService.CanMarkOrderReturnedToBeProcessed(order))
                    {
                        await _eventTicketService.MarkOrderReturnedToBeProcessedAsync(order);
                        await LogEditOrderAsync(order.Id);
                    }
                }
                catch (Exception exc)
                {
                    //prepare model
                    var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);
                    await _notificationService.ErrorNotificationAsync(exc, false);
                    return RedirectToAction("EventTicketsDelivery");
                }
            }
        }

        return RedirectToAction("EventTicketsDelivery");
    }


    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> MarkOrderAsInTransit(string selectedIds)
    {

        if (selectedIds != null && selectedIds.Count() != 0)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            for (int i = 0; i < ids.Count(); i++)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                    return AccessDeniedView();

                //try to get an order with the specified id
                var ordrItemId = await _orderService.GetOrderItemByIdAsync(ids[i]);
                var order = await _orderService.GetOrderByIdAsync(ordrItemId.OrderId);

                try
                {
                    if (_eventTicketService.CanMarkOrderAsInTransit(order))
                    {
                        await _eventTicketService.MarkOrderAsInTransitAsync(order);
                        await LogEditOrderAsync(order.Id);
                    }
                }
                catch (Exception exc)
                {
                    //prepare model
                    var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);
                    await _notificationService.ErrorNotificationAsync(exc, false);
                    return RedirectToAction("EventTicketsDelivery");
                }
            }

        }

        return RedirectToAction("EventTicketsDelivery");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> MarkAsFinalised(string selectedIds)
    {

        if (selectedIds != null && selectedIds.Count() != 0)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            for (int i = 0; i < ids.Count(); i++)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                    return AccessDeniedView();

                //try to get an order with the specified id
                var product = await _productService.GetProductByIdAsync(ids[i]);

                try
                {
                    if (_eventTicketService.CanMarkTicketAsFinalised(product))
                    {
                        await _eventTicketService.MarkTicketAsFinalisedAsync(product);
                        await LogEditTicketAsync(product.Id);
                    }
                }
                catch (Exception exc)
                {
                    //prepare model
                    _notificationService.ErrorNotification(exc.Message, false);
                    return RedirectToAction("EventTicketsSettlement");
                }
            }

        }

        return RedirectToAction("EventTicketsSettlement");
    }

    #endregion

    #region Export

    #endregion

    #region Sales Export 

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("EventTicketsSales")]
    [FormValueRequired("exportexcel-all-sales")]
    public virtual async Task<IActionResult> ExportExcelAll(EventTicketsSalesModel model)
    {
        var orderList = new List<Order>();
        var orderItems = new List<OrderItem>();

        var orderItemIds = await _eventTicketService.GetAllEventTicketsSalesItemsAsync();
        var orderItemsFound = await _eventTicketService.GetEventTicketsSalesOrderItemsListAsync(orderItemIds, model.PaymentMethod, 
            model.FromDate, model.ToDate, model.Ispay, model.OrderbyName, model.GroupProductId, model.AssociatedProductId, string.Empty);
        orderItems.AddRange(await orderItemsFound.WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

        foreach (var item in orderItems)
        {
            var order = await _orderService.GetOrderByIdAsync(item.OrderId);
            if (!orderList.Contains(order))
                orderList.Add(order);
        }

        //ensure that we at least one order found
        if (!orderItems.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"));
            return RedirectToAction("EventTicketsSales");
        }

        try
        {
            var bytes = await _exportImportManager.ExportEventTicketsOrderItemToXlsxAsync(orderList);
            return File(bytes, MimeTypes.TextXlsx, "EventTicketsSales.xlsx");
        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsSales");
        }
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> ExportSalesExcelSelected(string selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
            return AccessDeniedView();

        var orderList = new List<Order>();
        var orderItems = new List<OrderItem>();

        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            orderItems.AddRange(await (await _eventTicketService.GetEventTicketsOrderSalesItemsByOrderIdsAsync(ids)).WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

            foreach (var item in orderItems)
            {
                var order = await _orderService.GetOrderByIdAsync(item.OrderId);
                if (!orderList.Contains(order))
                    orderList.Add(order);
            }
        }

        try
        {
            var bytes = await _exportImportManager.ExportEventTicketsOrderItemToXlsxAsync(orderList);
            return File(bytes, MimeTypes.TextXlsx, "EventTicketsSales.xlsx");
        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsSales");
        }
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> PdfSalesReportSelected(string selectedIds)
    {
        var orderItems = new List<OrderItem>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            orderItems.AddRange(await (await _eventTicketService.GetEventTicketsOrderSalesItemsByOrderIdsAsync(ids)).WhereAwait(HasAccessToOrderItemAsync).ToListAsync());
        }

        var vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //ensure that we at least one order selected
        if (!orderItems.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"), true);
            return RedirectToAction("EventTicketsSales");
        }

        byte[] bytes;
        using (var stream = new MemoryStream())
        {
            await _eventTicketsPdfService.PrintOrdersToPdfAsync(stream, orderItems, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id, vendorId);
            bytes = stream.ToArray();
        }

        return File(bytes, MimeTypes.ApplicationPdf, "OrderItems.pdf");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("EventTicketsSales")]
    [FormValueRequired("pdf-report-all-sales")]
    public virtual async Task<IActionResult> PdfReportAll(EventTicketsSalesModel model)
    {
        var orderItems = new List<OrderItem>();

        var orderItemIds = await _eventTicketService.GetAllEventTicketsSalesItemsAsync();
        var orderItemsFound = await _eventTicketService.GetEventTicketsSalesOrderItemsListAsync(orderItemIds, model.PaymentMethod, 
            model.FromDate, model.ToDate, model.Ispay, model.OrderbyName, model.GroupProductId, model.AssociatedProductId, string.Empty);
        orderItems.AddRange(await orderItemsFound.WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

        var vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //ensure that we at least one order selected
        if (!orderItems.Any())
        {
            //ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.PdfInvoice.NoOrders"));
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"));
            return RedirectToAction("EventTicketsSales");
        }

        try
        {
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _eventTicketsPdfService.PrintOrdersToPdfAsync(stream, orderItems, 
                    _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id, vendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, "OrderItems.pdf");

        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsSales");
        }

    }

    #endregion

    #region Process Export 

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("EventTicketsProcess")]
    [FormValueRequired("exportexcel-all-process")]
    public virtual async Task<IActionResult> ExportExcelAll(EventTicketsProcessModel model)
    {

        var orderList = new List<Order>();
        var orderItems = new List<OrderItem>();
        
        var orderItemIds = await _eventTicketService.GetAllEventTicketsProcessItemsAsync();
        var orderItemsFound = await _eventTicketService.GetEventTicketsProcessOrderItemsListAsync(orderItemIds, model.PaymentMethod,
            model.FromDate, model.ToDate, model.Ispay, model.OrderbyName, model.GroupProductId, model.AssociatedProductId);
        orderItems.AddRange(await orderItemsFound.WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

        foreach (var item in orderItems)
        {
            var order = await _orderService.GetOrderByIdAsync(item.OrderId);
            if (!orderList.Contains(order))
                orderList.Add(order);
        }

        //ensure that we at least one order found
        if (!orderItems.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"));
            return RedirectToAction("EventTicketsProcess");
        }

        try
        {
            var bytes = await _exportImportManager.ExportEventTicketsOrderItemToXlsxAsync(orderList);
            return File(bytes, MimeTypes.TextXlsx, "EventTicketsProcess.xlsx");
        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsProcess");
        }
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> ExportProcessExcelSelected(string selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
            return AccessDeniedView();

        var orderList = new List<Order>();
        var orderItems = new List<OrderItem>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            orderItems.AddRange(await (await _eventTicketService.GetEventTicketsOrderSalesItemsByOrderIdsAsync(ids)).WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

            foreach (var item in orderItems)
            {
                var order = await _orderService.GetOrderByIdAsync(item.OrderId);
                if (!orderList.Contains(order))
                    orderList.Add(order);
            }
        }

        try
        {
            var bytes = await _exportImportManager.ExportEventTicketsOrderItemToXlsxAsync(orderList);
            return File(bytes, MimeTypes.TextXlsx, "EventTicketsProcess.xlsx");
        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsProcess");
        }
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> PdfProcessReportSelected(string selectedIds)
    {
        var orderItems = new List<OrderItem>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            orderItems.AddRange(await (await _eventTicketService.GetEventTicketsOrderSalesItemsByOrderIdsAsync(ids)).WhereAwait(HasAccessToOrderItemAsync).ToListAsync());
        }

        var vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //ensure that we at least one order selected
        if (!orderItems.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"));
            return RedirectToAction("EventTicketsProcess");
        }

        byte[] bytes;
        using (var stream = new MemoryStream())
        {
            await _eventTicketsPdfService.PrintOrdersToPdfAsync(stream, orderItems, 
                _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id, vendorId);
            bytes = stream.ToArray();
        }

        return File(bytes, MimeTypes.ApplicationPdf, "OrderItems.pdf");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("EventTicketsProcess")]
    [FormValueRequired("pdf-report-all-process")]
    public virtual async Task<IActionResult> PdfReportAll(EventTicketsProcessModel model)
    {
        var orderItems = new List<OrderItem>();

        var orderItemIds = await _eventTicketService.GetAllEventTicketsProcessItemsAsync();
        var orderItemsFound = await _eventTicketService.GetEventTicketsProcessOrderItemsListAsync(orderItemIds, model.PaymentMethod,
            model.FromDate, model.ToDate, model.Ispay, model.OrderbyName, model.GroupProductId, model.AssociatedProductId);
        orderItems.AddRange(await orderItemsFound.WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

        var vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //ensure that we at least one order selected
        if (!orderItems.Any())
        {
            //ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.PdfInvoice.NoOrders"));
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"));
            return RedirectToAction("EventTicketsProcess");
        }

        try
        {
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _eventTicketsPdfService.PrintOrdersToPdfAsync(stream, orderItems, 
                    _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id, vendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, "OrderItems.pdf");

        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsProcess");
        }

    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("EventTicketsProcess")]
    [FormValueRequired("pdf-report-summary-process")]
    public virtual async Task<IActionResult> PdfReportSummary(EventTicketsProcessModel model)
    {
        var orderItems = new List<OrderItem>();
        string filePath = string.Empty;

        var orderItemIds = await _eventTicketService.GetAllEventTicketsProcessItemsAsync();
        var orderItemsFound = await _eventTicketService.GetEventTicketsProcessOrderItemsListAsync(orderItemIds, model.PaymentMethod,
            model.FromDate, model.ToDate, model.Ispay, model.OrderbyName, model.GroupProductId, model.AssociatedProductId);
        orderItems.AddRange(await orderItemsFound.WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

        //ensure that we at least one order selected
        if (!orderItems.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"));
            return RedirectToAction("EventTicketsProcess");
        }

        try
        {
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _eventTicketsPdfService.PrintSalesSummaryReportAsync(stream, orderItems,
                    _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id,
                    await _storeContext.GetCurrentStoreAsync(), await _workContext.GetCurrentVendorAsync());
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, "EventSummaryReport_" + DateTime.Now.ToShortDateString() + ".pdf");

        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsProcess");
        }
    }

    #endregion

    #region Delivery Export 

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("EventTicketsDelivery")]
    [FormValueRequired("exportexcel-all-delivery")]
    public virtual async Task<IActionResult> ExportExcelAll(EventTicketsDeliveryModel model)
    {
        var orderList = new List<Order>();
        var orderItems = new List<OrderItem>();

        var orderItemIds = await _eventTicketService.GetAllEventTicketsDeliveryItemsAsync();
        var orderItemsFound = await _eventTicketService.GetEventTicketsDeliveryOrderItemsListAsync(orderItemIds, model.PaymentMethod,
            model.FromDate, model.ToDate, model.Ispay, model.OrderbyName, model.GroupProductId, model.AssociatedProductId);
        orderItems.AddRange(await orderItemsFound.WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

        foreach (var item in orderItems)
        {
            var order = await _orderService.GetOrderByIdAsync(item.OrderId);
            if (!orderList.Contains(order))
                orderList.Add(order);
        }

        //ensure that we at least one order found
        if (!orderItems.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"));
            return RedirectToAction("EventTicketsDelivery");
        }

        try
        {
            var bytes = await _exportImportManager.ExportEventTicketsOrderItemToXlsxAsync(orderList);
            return File(bytes, MimeTypes.TextXlsx, "EventTicketsDelivery.xlsx");
        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsDelivery");
        }
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> ExportDeliveryExcelSelected(string selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
            return AccessDeniedView();

        var orderList = new List<Order>();
        var orderItems = new List<OrderItem>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            orderItems.AddRange(await (await _eventTicketService.GetEventTicketsOrderSalesItemsByOrderIdsAsync(ids)).WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

            foreach (var item in orderItems)
            {
                var order = await _orderService.GetOrderByIdAsync(item.OrderId);
                if (!orderList.Contains(order))
                    orderList.Add(order);
            }
        }

        try
        {
            var bytes = await _exportImportManager.ExportEventTicketsOrderItemToXlsxAsync(orderList);
            return File(bytes, MimeTypes.TextXlsx, "EventTicketsDelivery.xlsx");
        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsDelivery");
        }
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public virtual async Task<IActionResult> PdfDeliveryReportSelected(string selectedIds)
    {
        var orderItems = new List<OrderItem>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            orderItems.AddRange(await (await _eventTicketService.GetEventTicketsOrderSalesItemsByOrderIdsAsync(ids)).WhereAwait(HasAccessToOrderItemAsync).ToListAsync());
        }

        var vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //ensure that we at least one order selected
        if (!orderItems.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"));
            return RedirectToAction("EventTicketsDelivery");
        }

        byte[] bytes;
        using (var stream = new MemoryStream())
        {
            await _eventTicketsPdfService.PrintOrdersToPdfAsync(stream, orderItems, 
                _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id, vendorId);
            bytes = stream.ToArray();
        }

        return File(bytes, MimeTypes.ApplicationPdf, "OrderItems.pdf");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("EventTicketsDelivery")]
    [FormValueRequired("pdf-report-all-delivery")]
    public virtual async Task<IActionResult> PdfReportAll(EventTicketsDeliveryModel model)
    {
        var orderItems = new List<OrderItem>();

        var orderItemIds = await _eventTicketService.GetAllEventTicketsDeliveryItemsAsync();
        var orderItemsFound = await _eventTicketService.GetEventTicketsDeliveryOrderItemsListAsync(orderItemIds, model.PaymentMethod,
            model.FromDate, model.ToDate, model.Ispay, model.OrderbyName, model.GroupProductId, model.AssociatedProductId);
        orderItems.AddRange(await orderItemsFound.WhereAwait(HasAccessToOrderItemAsync).ToListAsync());

        var vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //ensure that we at least one order selected
        if (!orderItems.Any())
        {
            var lrs = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders");
            _notificationService.ErrorNotification(lrs, true);
            return RedirectToAction("EventTicketsDelivery");
        }

        try
        {
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _eventTicketsPdfService.PrintOrdersToPdfAsync(stream, orderItems, 
                    _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id, vendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, "OrderItems.pdf");

        }
        catch (Exception exc)
        {
            //ErrorNotification(exc);
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("EventTicketsDelivery");
        }

    }

    #endregion

    #region Settlement Reports 

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public async virtual Task<IActionResult> PDFEventDoorReport(string selectedIds)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Operate))
            return AccessDeniedView();

        if (selectedIds != null && selectedIds.Count() != 0)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            for (int i = 0; i < ids.Count(); i++)
            {

                var product = await _productService.GetProductByIdAsync(ids[i]);
                if (product != null)
                {
                    try
                    {
                        byte[] bytes;
                        using (var stream = new MemoryStream())
                        {
                            await _eventTicketsPdfService.PrintEventDoorReportAsync(stream, product,
                                _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id,
                                await  _storeContext.GetCurrentStoreAsync(), await _workContext.GetCurrentVendorAsync());
                            bytes = stream.ToArray();
                        }

                        string filename = "Event Door Report - " + product.Name + ".pdf";
                        return File(bytes, MimeTypes.ApplicationPdf, filename);
                    }
                    catch (Exception exc)
                    {
                        _notificationService.ErrorNotification(exc.Message);
                        return await EventTicketsSettlement();
                    }
                }
            }
        }
        else
            _notificationService.ErrorNotification(await  _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoProducts"));

        return await EventTicketsSettlement();
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public async virtual Task<IActionResult> PDFSettlementReport(string selectedIds)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Operate))
            return AccessDeniedView();

        int vendorId = 0;
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
            vendorId = vendor.Id;

        if (selectedIds != null && selectedIds.Count() != 0)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            for (int i = 0; i < ids.Count(); i++)
            {

                var product = await _productService.GetProductByIdAsync(ids[i]);
                if (product != null && vendorId == 0 || product.VendorId == vendorId)
                {
                    var productVendor = await _vendorService.GetVendorByIdAsync(product.VendorId);

                    try
                    {
                        byte[] bytes;
                        using (var stream = new MemoryStream())
                        {
                            await _eventTicketsPdfService.PrintEventSettlementReportAsync(stream, product,
                                _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id,
                                await _storeContext.GetCurrentStoreAsync(), productVendor);
                            bytes = stream.ToArray();
                        }

                        string filename = "Event Settlement Report - " + product.Name + ".pdf";
                        return File(bytes, MimeTypes.ApplicationPdf, filename);
                    }
                    catch (Exception exc)
                    {
                        _notificationService.ErrorNotification(exc.Message);
                        return await EventTicketsSettlement();
                    }
                }
            }
        }
        else
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoProducts"));

        return await EventTicketsSettlement();
    }

    #endregion

    #region Affiliates

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public async virtual Task<IActionResult> AffiliateProductsList(ExtendedAffiliateProductSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _eventTicketsModelFactory.PrepareAffiliateProductListModelAsync(searchModel);

        return Json(model);
    }

    #endregion

}