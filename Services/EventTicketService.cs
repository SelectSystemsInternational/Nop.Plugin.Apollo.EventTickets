using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Affiliates;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;

using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.Core.Domain;
using Nop.Plugin.Apollo.Core.Services;
using Nop.Plugin.Apollo.EventTickets.Domain;
using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets.Services;

public partial class EventTicketService : IEventTicketService
{

    #region Fields

    CoreSystemHelper _systemHelper = new CoreSystemHelper();

    protected readonly IEventPublisher _eventPublisher;
    protected readonly IRepository<Order> _orderRepository;
    protected readonly IRepository<OrderItem> _orderItemRepository;
    protected readonly IRepository<Product> _productRepository;
    protected readonly IOrderService _orderService;
    protected readonly IProductService _productService;
    protected readonly IProductAttributeService _productAttributeService;
    protected readonly IProductTemplateService _productTemplateService;
    protected readonly IStoreContext _storeContext;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILogger _logger;
    protected readonly IWorkContext _workContext;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IAddressService _addressService;
    protected readonly ISettingService _settingService;
    protected readonly OrderSettings _orderSettings;
    protected readonly IPdfService _pdfService;
    protected readonly IShipmentService _shipmentService;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IEventTicketsMessageService _eventTicketsMessageService;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly IStoreService _storeService;
    protected readonly CatalogSettings _catalogSettings;
    protected readonly CommonSettings _commonSettings;
    protected readonly ILanguageService _languageService;
    protected readonly ICustomerService _customerService;
    protected readonly IAclService _aclService;
    protected readonly IStoreMappingService _storeMappingService;
    protected readonly IRepository<ProductCategory> _productCategoryRepository;
    protected readonly IRepository<ProductManufacturer> _productManufacturerRepository;
    protected readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
    protected readonly IRepository<StoreMapping> _storeMappingRepository;
    protected readonly IRepository<AclRecord> _aclRecordRepository;
    protected readonly IAttributeService<VendorAttribute, VendorAttributeValue> _vendorAttributeService;
    protected readonly IAttributeParser<VendorAttribute, VendorAttributeValue> _vendorAttributeParser;
    protected readonly IAffiliateService _affiliateService;
    protected readonly IRepository<GenericAttribute> _genericAttributeRepository;
    protected readonly IEncryptionService _encryptionService;
    protected readonly IVendorService _vendorService;
    protected readonly IApolloCoreService _apolloCoreService;
    protected readonly EventTicketSettings _eventTicketSettings;
    protected readonly IProductTypeExtensionsService _productTypeExtensionsService;
    protected readonly EventTicketsAccessSettings _eventTicketsAccessSettings;
    protected readonly IRepository<ProductTypeExtension> _productTypeExtensionRepository;

    #endregion

    #region Ctor

    public EventTicketService(IEventPublisher eventPublisher,
        IRepository<Order> orderRepository,
        IRepository<OrderItem> orderItemRepository,
        IRepository<Product> productRepository,
        IOrderService orderService,
        IProductService productService,
        IProductAttributeService productAttributeService,
        IProductTemplateService productTemplateService,
        IStoreContext storeContext,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        ILogger logger,
        IWorkContext workContext,
        IDateTimeHelper dateTimeHelper,
        IGenericAttributeService genericAttributeService,
        IAddressService addressService,
        ISettingService settingService,
        OrderSettings orderSettings,
        IPdfService pdfService,
        IShipmentService shipmentService,
        IStaticCacheManager staticCacheManager,
        IEventTicketsMessageService eventTicketsMessageService,
        IShoppingCartService shoppingCartService,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        IPaymentPluginManager paymentPluginManager,
        IStoreService storeService,
        CatalogSettings catalogSettings,
        CommonSettings commonSettings, ILanguageService languageService,
        ICustomerService customerService,
        IAclService aclService,
        IStoreMappingService storeMappingService,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductManufacturer> productManufacturerRepository,
        IRepository<LocalizedProperty> localizedPropertyRepository,
        IRepository<StoreMapping> storeMappingRepository,
        IRepository<AclRecord> aclRecordRepository,
        IAttributeService<VendorAttribute, VendorAttributeValue> vendorAttributeService,
        IAttributeParser<VendorAttribute, VendorAttributeValue> vendorAttributeParser,
        IAffiliateService affiliateService,
        IRepository<GenericAttribute> genericAttributeRepository,
        IEncryptionService encryptionService,
        IVendorService vendorService,
        IApolloCoreService apolloCoreService,
        EventTicketSettings eventTicketSettings,
        IProductTypeExtensionsService productTypeExtensionsService,
        EventTicketsAccessSettings eventTicketsAccessSettings,
        IRepository<ProductTypeExtension> productTypeExtensionRepository)
    {
        _eventPublisher = eventPublisher;
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
        _eventTicketSettings = eventTicketSettings;
        _orderService = orderService;
        _productService = productService;
        _productAttributeService = productAttributeService;
        _productTemplateService = productTemplateService;
        _storeContext = storeContext;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _logger = logger;
        _workContext = workContext;
        _dateTimeHelper = dateTimeHelper;
        _genericAttributeService = genericAttributeService;
        _addressService = addressService;
        _settingService = settingService;
        _orderSettings = orderSettings;
        _pdfService = pdfService;
        _shipmentService = shipmentService;
        _staticCacheManager = staticCacheManager;
        _shoppingCartService = shoppingCartService;
        _checkoutAttributeService = checkoutAttributeService;
        _paymentPluginManager = paymentPluginManager;
        _storeService = storeService;
        _catalogSettings = catalogSettings;
        _commonSettings = commonSettings;
        _languageService = languageService;
        _customerService = customerService;
        _aclService = aclService;
        _storeMappingService = storeMappingService;
        _productCategoryRepository = productCategoryRepository;
        _productManufacturerRepository = productManufacturerRepository;
        _localizedPropertyRepository = localizedPropertyRepository;
        _storeMappingRepository = storeMappingRepository;
        _aclRecordRepository = aclRecordRepository;
        _vendorAttributeService = vendorAttributeService;
        _vendorAttributeParser = vendorAttributeParser;
        _affiliateService = affiliateService;
        _genericAttributeRepository = genericAttributeRepository;
        _encryptionService = encryptionService;
        _vendorService = vendorService;
        _apolloCoreService = apolloCoreService;
        _productTypeExtensionsService = productTypeExtensionsService;
        _eventTicketsAccessSettings = eventTicketsAccessSettings;
        _productTypeExtensionRepository = productTypeExtensionRepository;
        _eventTicketsMessageService = eventTicketsMessageService;
    }

    #endregion

    #region Utility

    public async Task<bool> IsStoreTheme(string theme, string defaultValue = "DefaultClean")
    {
        var storeTheme = await GetStoreTheme(defaultValue);
        return storeTheme == theme;
    }

    public async Task<string> GetStoreTheme(string defaultValue = "DefaultClean")
    {
        return await _settingService.GetSettingByKeyAsync("storeinformationsettings.defaultstoretheme",
            defaultValue, _storeContext.GetCurrentStore().Id, true);
    }

    public async Task<string> GetSupportedStoreTheme(string defaultValue = "DefaultClean")
    {
        var storeTheme = await GetStoreTheme();

        if (!_eventTicketSettings.SupportedStoreThemes.Contains(storeTheme))
            return defaultValue;

        return storeTheme;
    }

    #endregion

    #region Custom Product

    /// <summary>
    /// Gets product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <returns>Product</returns>
    public virtual async Task<Product> GetTicketByIdAsync(int productId)
    {
        if (productId == 0)
            return null;

        return await _productRepository.GetByIdAsync(productId);
    }

    /// <summary>
    /// Updates a ticket product
    /// </summary>
    /// <param name="ticket">Product</param>
    public async Task UpdateTicketAsync(Product ticket)
    {
        await _productRepository.UpdateAsync(ticket);
    }

    #endregion

    #region Methods

    public async Task<bool> AuthorizeAsync(CoreSystemHelper.AccessMode requestedAccess, string publicKey = "", string privateKey = "")
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        string urlInstall = _systemHelper.GetDomainNameFromHost(store.Url);

        if (_eventTicketSettings.PublicKey.Contains("A0000000"))
        {
            try
            {
                string dateInstall = DateTime.Now.Date.ToString();
                var demoDateInstall = DateTime.Parse(dateInstall).AddDays(14);
                Guid pKeyInstall = _systemHelper.DateToGuid(demoDateInstall);
                string publicKeyInstall = pKeyInstall.ToString();
                string privateKeyInstall = _encryptionService.EncryptText(urlInstall, publicKeyInstall);
            }
            catch
            {
                string message = string.Format("Plugin {0} Error - Set Language to English to Enter License Keys",
                    CoreSystemHelper.ApolloDefaults.SystemName);
                await _logger.InformationAsync(message);
            }
        }

        string pKey = string.Empty;
        bool hasKey = false;
        string key = !string.IsNullOrEmpty(publicKey) ? publicKey : _eventTicketSettings.PublicKey;
        if (key != null)
        {
            if (key.ToString() == ApolloDefaults.SystemKey)
                hasKey = true;
            else if (key.ToString().Contains(ApolloDefaults.LicenceSystemKey))
            {
                // Purchase Key
                string sKey = _encryptionService.EncryptText("Licence", key);
                pKey = (!string.IsNullOrEmpty(privateKey) ? privateKey : _eventTicketSettings.PrivateKey);
                if (sKey != pKey)
                {
                    _eventTicketSettings.PrivateKey = sKey;
                    await _settingService.SaveSettingAsync(_eventTicketSettings);
                    hasKey = true;
                }
                else if (sKey == pKey)
                {
                    hasKey = true;
                }

                //string message = string.Format("Test Plugin {0} sKey {1} with PublicKey {2} and PrivateKey {3} and Status {4}",
                //    SystemHelper.ApolloDefaults.SystemName, sKey, key, pKey, hasKey);
                //await ApolloLogAsync(LogLevel.Debug, message);

            }
            else if (key.ToString() != ApolloDefaults.SystemKey)
            {
                try
                {
                    pKey = _encryptionService.EncryptText(urlInstall, key);
                    if (pKey == (!string.IsNullOrEmpty(privateKey) ? privateKey : _eventTicketSettings.PrivateKey))
                    {
                        Guid guid = Guid.Parse(key);
                        DateTime keyDate = _systemHelper.GuidToDate(guid);
                        if (keyDate > DateTime.Now)
                            hasKey = true;
                    }
                }
                catch (Exception exc)
                {
                    string message = string.Format("Error checking licence for Store {1} with PublicKey {2} and PrivateKey {3}",
                        SystemHelper.EventTicketDefaults.SystemName, urlInstall, key, pKey);
                    await _logger.ErrorAsync(message, exc);
                }
            }
        }

        if (hasKey)
        {
            return await _apolloCoreService.AccessAuthorizeAsync(requestedAccess);
        }
        else
        {
            string message = string.Format("Plugin {0} was installed for Store {1} with PublicKey {2} and PrivateKey {3}",
                SystemHelper.EventTicketDefaults.SystemName, urlInstall, key, pKey);
            await _logger.InformationAsync(message);
        }

        return false;
    }

    public async Task<EventTicketsAccessSettings> LoadSystemAccessAsync()
    {

        string key = _eventTicketSettings.PublicKey;

        var eventTicketsAccessSettings = await _settingService.LoadSettingAsync<EventTicketsAccessSettings>();

        bool eventTicketsFlag16 = false;
        bool systemFlag256 = false;
        string eventTicketsStringFlag16 = "00100000";
        string systemStringFlag256 = "10000000";

        string keyString = key.Substring(28, 8);

        for (int i = 0; i < 8; i++)
        {
            if ((keyString[i] == '1') && (keyString[i] == eventTicketsStringFlag16[i]))
                eventTicketsFlag16 = true;

            if ((keyString[i] == '1') && (keyString[i] == systemStringFlag256[i]))
                systemFlag256 = true;
        }

        eventTicketsAccessSettings.AccessSystem = systemFlag256;
        eventTicketsAccessSettings.AccessEventTickets = eventTicketsFlag16;
        await _settingService.SaveSettingAsync(eventTicketsAccessSettings);

        return eventTicketsAccessSettings;
    }

    /// <summary>
    /// Place log in system log
    /// </summary>
    public async Task ApolloLogAsync(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
    {
        await _logger.InsertLogAsync(logLevel, shortMessage, fullMessage, customer);
    }

    /// <summary>
    /// Add order note
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="note">Note text</param>
    public async Task AddOrderNoteAsync(Order order, string note)
    {
        await _orderService.InsertOrderNoteAsync(new OrderNote
        {
            OrderId = order.Id,
            Note = note,
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow
        });
    }

    public async Task<string> GetCustomerAdditionalNameDetailsAsync(int orderItemId)
    {
        string formated = string.Empty;

        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
        if (orderItem != null)
        {
            formated = await _genericAttributeService.GetAttributeAsync<string>(orderItem, SystemHelper.EventTicketDefaults.OrderItemAdditionalNames);
        }

        return formated;
    }

    public async Task<string> GetCustomerEmailDetailsAsync(OrderItem orderItem)
    {
        string formated = string.Empty;
        var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
        if (order != null)
        {
            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            formated = billingAddress.FirstName + " " + billingAddress.LastName + " (" + billingAddress.Email + ")";
        }
        return formated;
    }

    public async Task<string> GetCustomerFullNameDetailsAsync(OrderItem orderItem)
    {
        string formatted = string.Empty;
        var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
        if (order != null)
        {
            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            formatted = billingAddress.FirstName + " " + billingAddress.LastName;
        }
        return formatted;
    }

    public async Task<string> GetProductNameAsync(int productId)
    {
        string name = string.Empty;
        var product = await _productService.GetProductByIdAsync(productId);
        if (product != null)
        {
            string[] stringSeparators = new string[] { "@" };
            var productName = product.Name;
            if (product.Name.Contains("@"))
                productName = product.Name.Split(stringSeparators, StringSplitOptions.None)[0];

            string[] stringSeparators1 = new string[] { "-" };
            String[] strlist = productName.Split(stringSeparators1, StringSplitOptions.None);

            int count = 1;
            foreach (var str in strlist)
            {
                name += str;
                if (count < strlist.Count())
                    name += "\n";
                count++;
            }

            //if (IsBooking(product))
            //    name += "\n";
            //else
            //    name += " ";

        }

        return name;
    }

    public async Task<string> GetTicketEventDate(int productId, Language lang = null)
    {
        string eventDetails = null;

        if (lang == null)
            lang = await _workContext.GetWorkingLanguageAsync();

        var product = await _productService.GetProductByIdAsync(productId);
        if (product != null)
        {
            if (_apolloCoreService.IsEventTicket(product))
            {
                var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, CoreSystemHelper.RecordType.Configuration);
                if (pte != null)
                {
                    eventDetails = (await _dateTimeHelper.ConvertToUserTimeAsync(pte.AvailabilityStartDate,
                        DateTimeKind.Utc)).ToString("dddd, dd MMMM yyyy", new CultureInfo(lang.LanguageCulture));
                }
            }
        }

        return eventDetails;
    }

    public async Task<string> GetTicketEventVenue(int productId)
    {
        string eventDetails = null;

        var product = await _productService.GetProductByIdAsync(productId);
        if (product != null)
        {
            if (_apolloCoreService.IsEventTicket(product))
            {
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor != null)
                    eventDetails = vendor.Name;
            }
        }

        return eventDetails;
    }

    public async Task<string> GetLongDateAsync(DateTime datetime, Language lang = null, bool full = false)
    {

        if (lang == null)
            lang = await _workContext.GetWorkingLanguageAsync();

        string formatedDateTime = string.Empty;
        string dateTimeFormat = await _apolloCoreService.GetLocalisedDisplayLongDateFormatAsync();

        formatedDateTime = datetime.ToString(dateTimeFormat, new CultureInfo(lang.LanguageCulture));

        return formatedDateTime;
    }

    public int GetDateOrder(DateTime datetime)
    {
        int year = datetime.Year * 10000;
        int month = datetime.Month * 100;

        int displayOrder = SystemHelper.EventTicketDefaults.DisplayOrderDateOffset + year + month + datetime.Day;

        return displayOrder;
    }

    public async Task<string> GetProductCalendarLocaleAsync()
    {
        CultureInfo cultureInfo = CultureInfo.GetCultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);
        string locale = "'" + cultureInfo.TwoLetterISOLanguageName + "'";
        return locale;
    }

    /// <summary>
    /// Gets all template products for type 
    /// </summary>
    /// <returns>Products</returns>
    public virtual IList<Product> GetAllTemplateProducts(int searchType, CoreSystemHelper.SystemMode productTypeId, CoreSystemHelper.RecordType recordTypeId = 0, int vendorId = 0)
    {
        var query = from p in _productRepository.Table
                    join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                    orderby p.Name, p.DisplayOrder
                    where (recordTypeId == CoreSystemHelper.RecordType.AllTypes || pte.RecordTypeId == recordTypeId) &&
                          (productTypeId == CoreSystemHelper.SystemMode.System || pte.ProductTypeId == productTypeId) &&
                           p.Name.Contains("Template -") &&
                           p.ProductTypeId == searchType &&
                           (vendorId == 0 || p.VendorId == vendorId) &&
                           !p.Published && !p.Deleted
                    select p;

        return query.ToList();
    }

    /// <summary>
    /// Gets all group products for type 
    /// </summary>
    /// <returns>Products</returns>
    public virtual async Task<IList<Product>> GetAllGroupProducts(CoreSystemHelper.SystemMode productTypeId, CoreSystemHelper.RecordType recordTypeId = 0,
        int storeId = 0, int vendorId = 0, bool showHidden = false)
    {
        //get products
        var products = await SearchProductsAsync(
            storeId: storeId,
            vendorId: vendorId,
            orderBy: ProductSortingEnum.NameAsc,
            pageIndex: 0,
            pageSize: int.MaxValue,
            showHidden: true);

        var query = from p in products
                    join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                    orderby p.Name, p.DisplayOrder
                    where (recordTypeId == CoreSystemHelper.RecordType.AllTypes || pte.RecordTypeId == recordTypeId) &&
                          (productTypeId == CoreSystemHelper.SystemMode.System || pte.ProductTypeId == productTypeId) &&
                           pte.RecordStatusId != CoreSystemHelper.RecordStatus.Finalised &&
                           p.ProductTypeId == (int)ProductType.GroupedProduct &&
                           (vendorId == 0 || p.VendorId == vendorId) &&
                           (p.Published || showHidden) &&
                           !p.Deleted
                    select p;

        return query.ToList();
    }

    /// <summary>
    /// Gets all associated products for type 
    /// </summary>
    /// <returns>Products</returns>
    public virtual IList<Product> GetAllAssociatedProducts(CoreSystemHelper.SystemMode productTypeId, CoreSystemHelper.RecordType recordTypeId = 0, int vendorId = 0)
    {
        var query = from p in _productRepository.Table
                    join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                    orderby p.Name, p.DisplayOrder
                    where (recordTypeId == CoreSystemHelper.RecordType.AllTypes || pte.RecordTypeId == recordTypeId) &&
                          (productTypeId == CoreSystemHelper.SystemMode.System || pte.ProductTypeId == productTypeId) &&
                           p.ProductTypeId == (int)ProductType.SimpleProduct &&
                           (vendorId == 0 || p.VendorId == vendorId) &&
                           p.Published && !p.Deleted
                    select p;

        return query.ToList();
    }

    public virtual IList<Product> GetAllAssociatedEventTicketsProducts()
    {
        var query = from p in _productRepository.Table
                    join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                    orderby p.DisplayOrder, p.Id
                    where p.ProductTypeId == (int)ProductType.SimpleProduct &&
                          pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                          pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                          p.Published && !p.Deleted
                    select p;

        return query.ToList();
    }

    /// <summary>
    /// Gets all associated products for type 
    /// </summary>
    /// <returns>Products</returns>
    public virtual IList<Product> GetAllEventTicketsProducts(CoreSystemHelper.SystemMode type = 0, int vendorId = 0, ProductType productType = 0) //ToDo
    {
        var query = from p in _productRepository.Table
                    orderby p.Name, p.DisplayOrder
                    where (vendorId == 0 || p.VendorId == vendorId) &&
                           !p.Deleted
                    select p;

        return query.ToList();
    }

    #endregion

    #region Event Tickets Sales Orders

    public async Task<int[]> GetAllEventTicketsSalesItemsAsync()
    {
        var eventTicketsSalesOrders = GetAllEventTicketsSalesMapping();
        int[] orderItemId = new int[eventTicketsSalesOrders.Count];
        int i = 0;
        foreach (var item in eventTicketsSalesOrders)
        {
            var orderItem = await _orderService.GetOrderItemByIdAsync(item.Id);
            if (orderItem != null)
            {
                orderItemId[i] = item.Id;
                i += 1;
            }
        }

        return orderItemId;
    }

    public virtual IList<OrderItem> GetAllEventTicketsSalesMapping()
    {
        int osPending = (int)OrderStatus.Pending;
        //int osProcessing = (int)OrderStatus.Processing;
        //int psPending = (int)PaymentStatus.Pending;
        //int psPaid = (int)PaymentStatus.Paid;
        //int psAuthorised = (int)PaymentStatus.Authorized;
        //int ssNotYetShipped = (int)ShippingStatus.NotYetShipped;
        //int ssPartiallyShipped = (int)ShippingStatus.PartiallyShipped;

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           o.OrderStatusId == osPending &&
                           !o.Deleted
                     select oi);

        return query.ToList();
    }

    public virtual async Task<IList<OrderItem>> GetEventTicketsOrderSalesItemsByOrderIdsAsync(int[] orderItemIds)
    {
        int vendorId = 0;
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
            vendorId = vendor.Id;

        var query = from oi in _orderItemRepository.Table
                    join o in _orderRepository.Table on oi.OrderId equals o.Id
                    join p in _productRepository.Table on oi.ProductId equals p.Id
                    where orderItemIds.Contains(oi.Id) &&
                          (vendorId == 0 || p.VendorId == vendorId) &&
                           o.OrderStatusId != 40 // Cancelled
                    select oi;

        return query.ToList();
    }

    public virtual async Task<IPagedList<OrderItem>> GetEventTicketsSalesOrderItemsListAsync(int[] orderItemIds, string paymentMethod, DateTime? fromDate, DateTime? toDate,
        bool isPay, bool orderbyName, int groupProductId, int associatedProductId, string searchProductName, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        int vendorId = 0;
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
            vendorId = vendor.Id;

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                            pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                            (vendorId == 0 || p.VendorId == vendorId) &&
                            orderItemIds.Contains(oi.Id) &&
                            o.OrderStatusId != 40 && // Cancelled
                            (paymentMethod == "All" || o.PaymentMethodSystemName == paymentMethod) &&
                            (isPay != true || o.PaymentStatusId != 30) // Order Not Paid
                     select oi).ToList();

        if (fromDate.HasValue || toDate.HasValue)
        {
            if (fromDate.HasValue)
                fromDate = _dateTimeHelper.ConvertToUtcTime(fromDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            if (toDate.HasValue)
                toDate = _dateTimeHelper.ConvertToUtcTime(toDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            if (query != null)
            {
                query = (from oi in query.Distinct().ToList()
                         join o in _orderRepository.Table on oi.OrderId equals o.Id
                         where (!fromDate.HasValue || fromDate.Value <= o.CreatedOnUtc) &&
                                (!toDate.HasValue || toDate.Value >= o.CreatedOnUtc)
                         select oi).ToList().Distinct().ToList();
            }
        }

        if (groupProductId > 0 && query.Count() > 0)
        {
            var associatedProducts = await _productService.GetAssociatedProductsAsync(groupProductId, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (associatedProducts != null)
            {
                var associatedProductIds = associatedProducts.Select(a => a.Id).ToArray();
                query = (from oi in query
                         where associatedProductIds.Contains(oi.ProductId)
                         select oi).ToList().Distinct().ToList();
            }
            if (associatedProductId > 0 && query.Count() > 0)
            {
                query = (from oi in query
                         where oi.ProductId == associatedProductId
                         select oi).ToList().Distinct().ToList();
            }
        }
        else if (associatedProductId > 0 && query.Count() > 0)
        {
            var associatedProducts = GetAllAssociatedEventTicketsProducts();

            var associatedProductIds = associatedProducts.Select(a => a.Id).ToArray();

            query = (from oi in query
                     where associatedProductIds.Contains(oi.ProductId)
                     select oi).ToList().Distinct().ToList();

            query = (from oi in query
                     where oi.ProductId == associatedProductId
                     select oi).ToList().Distinct().ToList();
        }
        else if (!string.IsNullOrEmpty(searchProductName))
        {
            //get products
            var categoryIds = new List<int> { 0 };
            var products = await SearchTicketsAsync(showHidden: false,
                categoryIds: categoryIds,
                manufacturerId: 0,
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                vendorId: vendorId,
                warehouseId: 0,
                productType: ProductType.SimpleProduct,
                keywords: searchProductName,
                pageIndex: pageIndex, pageSize: pageSize,
                overridePublished: null);

            query = (from oi in query
                     join p in products on oi.ProductId equals p.Id
                     select oi).ToList().Distinct().ToList();

        }

        query = query.OrderByDescending(o => o.OrderId).ToList();

        var records = new PagedList<OrderItem>(query, pageIndex, pageSize);

        return records;
    }
    #endregion

    #region Event Tickets Delivery Orders

    public async Task<int[]> GetAllEventTicketsDeliveryItemsAsync()
    {
        var eventTicketsDeliveryIOrders = GetAllEventTicketsDeliveryMapping();
        int[] orderItemId = new int[eventTicketsDeliveryIOrders.Count];
        int i = 0;
        foreach (var item in eventTicketsDeliveryIOrders)
        {
            var orderItem = await _orderService.GetOrderItemByIdAsync(item.Id);
            if (orderItem != null)
            {
                orderItemId[i] = item.Id;
                i += 1;
            }
        }

        return orderItemId;
    }

    public virtual IList<OrderItem> GetAllEventTicketsDeliveryMapping()
    {
        int osComplete = (int)OrderStatus.Complete;
        int psPaid = (int)PaymentStatus.Paid;

        //int osProcessing = (int)OrderStatus.Processing;
        //int psPending = (int)PaymentStatus.Pending;
        //int psAuthorised = (int)PaymentStatus.Authorized;
        //int ssShippingNotRequired = (int)ShippingStatus.ShippingNotRequired;
        //int ssPartiallyShipped = (int)ShippingStatus.PartiallyShipped;

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           o.OrderStatusId != osComplete &&
                           o.PaymentStatusId == psPaid &&
                           !o.Deleted
                     select oi);

        return query.ToList();
    }

    public virtual IList<OrderItem> GetEventTicketsDeliveryOrderItemsByOrderIds(int[] orderItemIds)
    {
        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     where orderItemIds.Contains(oi.Id) &&
                        o.OrderStatusId != 40 // Cancelled
                     select oi).ToList();
        return query;
    }

    public virtual async Task<IPagedList<OrderItem>> GetEventTicketsDeliveryOrderItemsListAsync(int[] orderItemIds, string paymentMethod, DateTime? fromDate, DateTime? toDate,
        bool isPay, bool orderbyName, int groupProductId, int associatedProductId, int pageIndex = 0, int pageSize = int.MaxValue)
    {

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           orderItemIds.Contains(oi.Id) &&
                           o.OrderStatusId != 40 && // Cancelled
                           (paymentMethod == "All" || o.PaymentMethodSystemName == paymentMethod) &&
                           (isPay != true || o.PaymentStatusId != 30) // Order Not Paid
                     select oi).ToList();

        if (fromDate.HasValue || toDate.HasValue)
        {
            if (query != null)
            {
                query = (from oi in query.Distinct().ToList()
                         join o in _orderRepository.Table on oi.OrderId equals o.Id
                         where (!fromDate.HasValue || fromDate.Value <= o.CreatedOnUtc) &&
                                (!toDate.HasValue || toDate.Value >= o.CreatedOnUtc)
                         select oi).ToList().Distinct().ToList();
            }
        }

        if (groupProductId > 0 && query.Count() > 0)
        {
            var associatedProducts = await _productService.GetAssociatedProductsAsync(groupProductId, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (associatedProducts != null)
            {
                var associatedProductsids = associatedProducts.Select(a => a.Id).ToArray();
                query = (from oi in query
                         where associatedProductsids.Contains(oi.ProductId)
                         select oi).ToList().Distinct().ToList();
            }
            if (associatedProductId > 0 && query.Count() > 0)
            {
                query = (from oi in query
                         where oi.ProductId == associatedProductId
                         select oi).ToList().Distinct().ToList();
            }
        }
        else if (associatedProductId > 0 && query.Count() > 0)
        {
            var associatedProducts = GetAllAssociatedEventTicketsProducts();

            var associatedProductsids = associatedProducts.Select(a => a.Id).ToArray();

            query = (from oi in query
                     where associatedProductsids.Contains(oi.ProductId)
                     select oi).ToList().Distinct().ToList();

            query = (from oi in query
                     where oi.ProductId == associatedProductId
                     select oi).ToList().Distinct().ToList();
        }

        query = query.OrderByDescending(o => o.OrderId).ToList();

        var records = new PagedList<OrderItem>(query, pageIndex, pageSize);

        return records;
    }

    #endregion

    #region Event Tickets Process Orders

    public async Task<int[]> GetAllEventTicketsProcessItemsAsync()
    {
        var eventTicketsProcessOrderItems = await GetAllEventTicketsProcessMappingAsync();
        int[] orderItemId = new int[eventTicketsProcessOrderItems.Count];
        int i = 0;
        foreach (var item in eventTicketsProcessOrderItems)
        {
            var orderItem = await _orderService.GetOrderItemByIdAsync(item.Id);
            if (orderItem != null)
            {
                orderItemId[i] = item.Id;
                i += 1;
            }
        }

        return orderItemId;
    }

    public virtual async Task<IList<OrderItem>> GetAllEventTicketsProcessMappingAsync()
    {
        int osComplete = (int)OrderStatus.Complete;
        //int osProcessing = (int)OrderStatus.Processing;
        //int psPending = (int)PaymentStatus.Pending;
        //int psPaid = (int)PaymentStatus.Paid;
        //int psAuthorised = (int)PaymentStatus.Authorized;
        //int ssNotYetShipped = (int)ShippingStatus.NotYetShipped;
        //int ssPartiallyShipped = (int)ShippingStatus.PartiallyShipped;

        int vendorId = 0;
        if (await _workContext.GetCurrentVendorAsync() != null)
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        int storeId = 0;
        if ((await _storeContext.GetCurrentStoreAsync()).Id != 0)
            storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           o.OrderStatusId == osComplete &&
                           pte.AvailabilityStartDate.Date > DateTime.Now.AddDays(-2) &&
                           (storeId == 0 || o.StoreId == storeId) &&
                           (vendorId == 0 || p.VendorId == vendorId) &&
                           !o.Deleted
                     select oi);

        return query.ToList();
    }

    public virtual IList<OrderItem> GetEventTicketsProcessOrderItemsByOrderIds(int[] orderItemIds)
    {
        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     where orderItemIds.Contains(oi.Id) &&
                            o.OrderStatusId != 40 // Cancelled
                     select oi).ToList();
        return query;
    }

    public virtual async Task<IPagedList<OrderItem>> GetEventTicketsProcessOrderItemsListAsync(int[] orderItemIds, string paymentMethod, DateTime? fromDate, DateTime? toDate,
        bool isPay, bool orderbyName, int groupProductId, int associatedProductId, int pageIndex = 0, int pageSize = int.MaxValue)
    {

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           orderItemIds.Contains(oi.Id) &&
                           o.OrderStatusId != 40 && // Cancelled
                           (paymentMethod == "All" || o.PaymentMethodSystemName == paymentMethod) &&
                           (isPay != true || o.PaymentStatusId != 30) // Order Not Paid
                     select oi).ToList();

        if (fromDate.HasValue || toDate.HasValue)
        {
            if (query != null)
            {
                query = (from oi in query.Distinct().ToList()
                         join o in _orderRepository.Table on oi.OrderId equals o.Id
                         where (!fromDate.HasValue || fromDate.Value <= o.CreatedOnUtc) &&
                                (!toDate.HasValue || toDate.Value >= o.CreatedOnUtc)
                         select oi).ToList().Distinct().ToList();
            }
        }

        if (groupProductId > 0 && query.Count() > 0)
        {
            var associatedProducts = await _productService.GetAssociatedProductsAsync(groupProductId, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (associatedProducts != null)
            {
                var associatedProductsids = associatedProducts.Select(a => a.Id).ToArray();
                query = (from oi in query
                         where associatedProductsids.Contains(oi.ProductId)
                         select oi).ToList().Distinct().ToList();
            }
            if (associatedProductId > 0 && query.Count() > 0)
            {
                query = (from oi in query
                         where oi.ProductId == associatedProductId
                         select oi).ToList().Distinct().ToList();
            }
        }
        else if (associatedProductId > 0 && query.Count() > 0)
        {
            var associatedProducts = GetAllAssociatedEventTicketsProducts();

            var associatedProductsids = associatedProducts.Select(a => a.Id).ToArray();

            query = (from oi in query
                     where associatedProductsids.Contains(oi.ProductId)
                     select oi).ToList().Distinct().ToList();

            query = (from oi in query
                     where oi.ProductId == associatedProductId
                     select oi).ToList().Distinct().ToList();
        }

        query = query.OrderByDescending(o => o.OrderId).ToList();

        var records = new PagedList<OrderItem>(query, pageIndex, pageSize);

        return records;
    }

    #endregion

    #region Event Tickets Settlement Orders

    public async Task<int[]> GetAllEventTicketsSettlementItemsAsync()
    {
        var eventTicketsSettlementProducts = GetAllEventTicketsSettlementMapping();
        int[] productItemId = new int[eventTicketsSettlementProducts.Count];
        int i = 0;
        foreach (var item in eventTicketsSettlementProducts)
        {
            var productItem = await _productService.GetProductByIdAsync(item.Id);
            if (productItem != null)
            {
                productItemId[i] = item.Id;
                i += 1;
            }
        }

        return productItemId;
    }

    public virtual IList<Product> GetAllEventTicketsSettlementMapping()
    {

        var query = (from p in _productRepository.Table
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           pte.RecordStatusId != CoreSystemHelper.RecordStatus.Finalised &&
                           pte.AvailabilityEndDate < DateTime.UtcNow &&
                           p.ParentGroupedProductId == 0 &&
                           !p.Name.Contains("Template - ") &&
                           !p.Deleted
                     select p);

        return query.ToList();
    }

    public virtual IList<OrderItem> GetEventTicketsSettlementOrderItemsByOrderIds(int[] orderItemIds)
    {
        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     where orderItemIds.Contains(oi.Id) &&
                            o.OrderStatusId != 40 // Cancelled
                     select oi).ToList();
        return query;
    }

    public virtual IPagedList<ProductTypeExtension> GetEventTicketsSettlementItemsList(int[] productItemIds, DateTime? fromDate, DateTime? toDate,
        int groupProductId, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = (from p in _productRepository.Table
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           pte.RecordStatusId != CoreSystemHelper.RecordStatus.Finalised &&
                           pte.AvailabilityEndDate < DateTime.UtcNow &&
                           p.ParentGroupedProductId == 0 &&
                           (groupProductId == 0 || p.Id == groupProductId) &&
                           !p.Name.Contains("Template - ") &&
                           !p.Deleted
                     select pte).Distinct().ToList();

        if (fromDate.HasValue || toDate.HasValue)
        {
            if (query != null)
            {
                query = (from pte in query.Distinct().ToList()
                         where (!fromDate.HasValue || fromDate.Value <= pte.AvailabilityStartDate) &&
                                (!toDate.HasValue || toDate.Value >= pte.AvailabilityStartDate)
                         select pte).ToList().Distinct().ToList();
            }
        }

        query = query.OrderByDescending(pte => pte.AvailabilityStartDate).ToList();

        var records = new PagedList<ProductTypeExtension>(query, pageIndex, pageSize);
        return records;
    }

    #endregion

    #region Order Update Status

    public bool CanMarkOrderAsApproved(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.OrderStatus == OrderStatus.Cancelled)
            return false;

        if (order.OrderStatus == OrderStatus.Pending)
            return true;

        return false;
    }

    public async Task MarkOrderAsApprovedAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (CanMarkOrderAsApproved(order))
        {

            // Send the email 

            if (order.PaymentStatusId == (int)PaymentStatus.Pending)
            {
                //var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ? await _pdfService.PrintOrderToPdfAsync(order) : null;
                //var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ? "order.pdf" : null;
                //await _eventTicketsMessageService.SendOrderApprovedCustomerNotificationAsync(order, order.CustomerLanguageId, 
                //    orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
            }

            //order.PaymentStatusId = (int)PaymentStatus.Paid; // for testing
            order.OrderStatusId = (int)OrderStatus.Processing;

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                if (product.IsShipEnabled)
                    order.ShippingStatusId = (int)ShippingStatus.NotYetShipped;
            }

            await _orderService.UpdateOrderAsync(order);

            //add a note
            await AddOrderNoteAsync(order, "Order has been marked as approved");
        }
    }

    public bool CanMarkOrderReturnedToBeChecked(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.OrderStatus == OrderStatus.Cancelled)
            return false;

        if (order.OrderStatus == OrderStatus.Processing &&
            order.ShippingStatus != ShippingStatus.Shipped)
            return true;

        return false;
    }

    public async Task MarkOrderReturnedToBeCheckedAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (CanMarkOrderReturnedToBeChecked(order))
        {
            order.OrderStatusId = (int)OrderStatus.Pending;
            order.ShippingStatusId = (int)ShippingStatus.NotYetShipped;

            await _orderService.UpdateOrderAsync(order);

            //add a note
            await AddOrderNoteAsync(order, "Order has been returned to be checked");
        }
    }

    public bool CanMarkOrderAsProcessed(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.OrderStatus == OrderStatus.Cancelled)
            return false;

        if ((order.PaymentStatus == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.Authorized) &&
            order.OrderStatus == OrderStatus.Processing)
            return true;

        return false;

    }

    public async Task MarkOrderAsProcessedAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (CanMarkOrderAsProcessed(order))
        {
            order.OrderStatusId = (int)OrderStatus.Processing;
            order.ShippingStatusId = (int)ShippingStatus.PartiallyShipped;

            await _orderService.UpdateOrderAsync(order);

            //add a note
            await AddOrderNoteAsync(order, "Order has been marked as processed");

            // Create a shipment 

            await CreateOrUpadeteShippingAsync(order.Id, "Carrier TBC", "Service TBC", order.Id.ToString());
        }
    }

    public bool CanMarkOrderReturnedToBeProcessed(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.OrderStatus == OrderStatus.Cancelled)
            return false;

        if (order.ShippingStatus != ShippingStatus.Shipped)
            return true;

        return false;
    }

    public async Task MarkOrderReturnedToBeProcessedAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (CanMarkOrderReturnedToBeProcessed(order))
        {
            order.OrderStatusId = (int)OrderStatus.Processing;
            order.ShippingStatusId = (int)ShippingStatus.NotYetShipped;

            await _orderService.UpdateOrderAsync(order);

            //add a note
            await AddOrderNoteAsync (order, "Order has been marked as returned to be processed");
        }

    }

    public bool CanMarkOrderAsInTransit(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.OrderStatus == OrderStatus.Cancelled)
            return false;

        if ((order.PaymentStatus == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.Authorized) &&
            order.OrderStatus == OrderStatus.Processing &&
            order.ShippingStatus == ShippingStatus.PartiallyShipped)
            return true;

        return false;
    }

    public async Task MarkOrderAsInTransitAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (CanMarkOrderAsInTransit(order))
        {
            order.ShippingStatusId = (int)ShippingStatus.Shipped;

            await _orderService.UpdateOrderAsync(order);

            //add a note
            await AddOrderNoteAsync(order, "Order has been marked as in transit");

        }
    }

    public bool CanMarkTicketAsFinalised(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, CoreSystemHelper.RecordType.Configuration);
        if (pte != null)
        {
            if (pte.RecordStatusId != CoreSystemHelper.RecordStatus.Finalised)
                return true;
        }

        return false;
    }

    public async Task MarkTicketAsFinalisedAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (CanMarkTicketAsFinalised(product))
        {
            var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, CoreSystemHelper.RecordType.Configuration);
            if (pte != null)
            {
                pte.RecordStatusId = CoreSystemHelper.RecordStatus.Finalised;
                await _productTypeExtensionsService.UpdateProductTypeExtension(pte);
            }
        }
    }

    public bool CanMarkTicketReportPrinted(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, CoreSystemHelper.RecordType.Configuration);
        if (pte != null)
        {
            if (pte.RecordStatusId < CoreSystemHelper.RecordStatus.ReportPrinted)
                return true;
        }

        return false;
    }

    public async Task MarkTicketAsReportPrintedAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (CanMarkTicketAsFinalised(product))
        {
            var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, CoreSystemHelper.RecordType.Configuration);
            if (pte != null)
            {
                pte.RecordStatusId = CoreSystemHelper.RecordStatus.ReportPrinted;
                await _productTypeExtensionsService.UpdateProductTypeExtension(pte);
            }
        }
    }

    public bool CanMarkTicketOperating(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, CoreSystemHelper.RecordType.Configuration);
        if (pte != null)
        {
            if (pte.RecordStatusId < CoreSystemHelper.RecordStatus.Operating)
                return true;
        }

        return false;
    }

    public async Task MarkTicketAsOperatingAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (CanMarkTicketOperating(product))
        {
            var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, CoreSystemHelper.RecordType.Configuration);
            if (pte != null)
            {
                pte.RecordStatusId = CoreSystemHelper.RecordStatus.Operating;
                await _productTypeExtensionsService.UpdateProductTypeExtension(pte);
            }
        }
    }

    #endregion

    #region Shipping

    /// <summary>
    /// Create or upadete shipping
    /// </summary>
    /// <param name="orderNumber"></param>
    /// <param name="carrier"></param>
    /// <param name="service"></param>
    /// <param name="trackingNumber"></param>
    public async Task CreateOrUpadeteShippingAsync(int orderNumber, string carrier, string service, string trackingNumber)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderNumber);

            if (order == null)
                return;

            var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(order.Id);

            if (!shipments.Any())
            {
                var shipment = new Shipment
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    ShippedDateUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                    TrackingNumber = trackingNumber
                };

                await _shipmentService.InsertShipmentAsync(shipment);

                var newShipment = (await _shipmentService.GetShipmentsByOrderIdAsync(order.Id)).FirstOrDefault();

                decimal totalWeight = 0;

                foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
                {
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                    //is shippable
                    if (!product.IsShipEnabled)
                        continue;

                    //ensure that this product can be shipped (have at least one item to ship)
                    var maxQtyToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);
                    if (maxQtyToAdd <= 0)
                        continue;

                    var warehouseId = product.WarehouseId;

                    //ok. we have at least one item. let's create a shipment (if it does not exist)

                    var orderItemTotalWeight = orderItem.ItemWeight * orderItem.Quantity;
                    if (orderItemTotalWeight.HasValue)
                        totalWeight += orderItemTotalWeight.Value;

                    //create a shipment item
                    var shipmentItem = new ShipmentItem
                    {
                        OrderItemId = orderItem.Id,
                        Quantity = orderItem.Quantity,
                        WarehouseId = warehouseId,
                        ShipmentId = newShipment.Id
                    };

                    await _shipmentService.InsertShipmentItemAsync(shipmentItem);
                }

                newShipment.TotalWeight += totalWeight;

                await _shipmentService.UpdateShipmentAsync(newShipment);
            }
            else
            {
                var shipment = shipments.FirstOrDefault();

                if (shipment == null)
                    return;

                shipment.TrackingNumber = trackingNumber;
                await _shipmentService.UpdateShipmentAsync(shipment);
            }

            order.ShippingStatus = ShippingStatus.PartiallyShipped;
            order.ShippingMethod = string.IsNullOrEmpty(service) ? carrier : service;

            await _orderService.UpdateOrderAsync(order);
        }
        catch (Exception e)
        {
            await _logger.ErrorAsync(e.Message, e);
        }
    }

    #endregion

    #region Ticket Sales

    public virtual IList<OrderItem> GetAllEventTicketsMapping()
    {

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           !o.Deleted
                     select oi);

        return query.ToList();
    }

    public virtual IList<OrderItem> GetAllEventTicketsOrderItemsMapping()
    {

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           !o.Deleted
                     select oi);

        return query.ToList();
    }

    /// <summary>
    ///     Gets all order product variants
    /// </summary>
    /// <returns>Order collection</returns>
    public async Task<List<OrderTicketItem>> GetAllOrderProductVariantsSummaryAsync(int storeId, int? productVariantId,
        DateTime? startTime, DateTime? endTime, bool includeOffline)
    {

        int osComplete = (int)OrderStatus.Complete;

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           o.OrderStatusId == osComplete &&
                           (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                           (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                           (storeId == 0 || o.StoreId == storeId) &&
                           (!productVariantId.HasValue || productVariantId.Value == 0 || productVariantId == p.Id) &&
                           !o.Deleted
                     select oi);

        var orderTicketItems = await query.ToList().SelectAwait(async x =>
        {

            var order = await _orderService.GetOrderByIdAsync(x.OrderId);
            var product = await _productService.GetProductByIdAsync(x.ProductId);
            var getParentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
            var paymentMethodFriendlyName = paymentMethod != null ?
                await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.PaymentMethodSystemName;
            var startDateValue = string.Empty;
            var store = await _storeService.GetStoreByIdAsync(order.StoreId);

            try
            {
                startDateValue = order.PaidDateUtc == null ? "Not Paid" :
                    _dateTimeHelper.ConvertToUserTime(order.PaidDateUtc.Value, TimeZoneInfo.Utc,
                    await _dateTimeHelper.GetCustomerTimeZoneAsync(await _workContext.GetCurrentCustomerAsync())).ToString(await _apolloCoreService.GetLocalisedDisplayShortDateFormatAsync());
            }
            catch (Exception ex)
            {
                var error = ex;
            }

            var orderTicketItem = new OrderTicketItem
            {
                OrderItemId = x.Id,
                Order = order,
                Product = product,
                PaymentMethodFriendlyName = paymentMethodFriendlyName,
                Quantity = x.Quantity,
                Outlet = store.Name,
                PriceInclTax = x.PriceInclTax,
                OriginalProductCost = x.OriginalProductCost * x.Quantity
            };

            return orderTicketItem;

        }).ToListAsync();

        return orderTicketItems;

    }

    /// <summary>
    ///     Gets quantity of product variant purchased by customer
    /// </summary>
    public int GetProductVariantOrderedTotal(int productVariantId,
        int? customerId,
        DateTime? dateFrom,
        DateTime? dateTo,
        OrderStatus? os,
        PaymentStatus? ps,
        ShippingStatus? ss)
    {
        int sum = 0;

        var orderProductVariants = GetAllOrderProductVariants(0, null, customerId, productVariantId, dateFrom, dateTo, os, ps, ss);
        foreach (var opv in orderProductVariants)
        {
            //if (opv.Order.OrderStatus != OrderStatus.Cancelled)
                sum += opv.Quantity;
        }

        return sum;
    }

    /// <summary>
    ///     Gets all order product variants
    /// </summary>
    /// <param name="orderId">Order identifier; null to load all records</param>
    /// <param name="customerId">Customer identifier; null to load all records</param>
    /// <param name="productVariantId">Product variant identifier; null to load all records</param>
    /// <param name="startTime">Order start time; null to load all records</param>
    /// <param name="endTime">Order end time; null to load all records</param>
    /// <param name="os">Order status; null to load all records</param>
    /// <param name="ps">Order payment status; null to load all records</param>
    /// <param name="ss">Order shippment status; null to load all records</param>
    /// <returns>Order collection</returns>
    public List<OrderItem> GetAllOrderProductVariants(int storeId,
        int? orderId,
        int? customerId,
        int? productVariantId,
        DateTime? startTime,
        DateTime? endTime,
        OrderStatus? os,
        PaymentStatus? ps,
        ShippingStatus? ss)
    {
        var list = new List<OrderTicketItem>(); //ToDo

        int? orderStatusId = null;
        if (os.HasValue)
            orderStatusId = (int)os.Value;

        int? paymentStatusId = null;
        if (ps.HasValue)
            paymentStatusId = (int)ps.Value;

        int? shippingStatusId = null;
        if (ss.HasValue)
            shippingStatusId = (int)ss.Value;

        int displayStoreId = 0;

        var query = (from oi in _orderItemRepository.Table
                        join o in _orderRepository.Table on oi.OrderId equals o.Id
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                        where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                              pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                              (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                              (!paymentStatusId.HasValue || paymentStatusId.Value == o.PaymentStatusId) &&
                              (!shippingStatusId.HasValue || shippingStatusId.Value == o.ShippingStatusId) &&
                              (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                              (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                              (displayStoreId == 0 || o.StoreId == displayStoreId) &&
                              (!customerId.HasValue || customerId.Value == 0 || customerId == o.CustomerId) &&
                              (!productVariantId.HasValue || productVariantId.Value == 0 || productVariantId == p.Id) &&
                              !o.Deleted && !p.Deleted
                        select oi);

        return query.ToList();

    }

    #endregion

    #region Ticket Methods

    /// <summary>
    /// Search products
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="categoryIds">Category identifiers</param>
    /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
    /// <param name="storeId">Store identifier; 0 to load all records</param>
    /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
    /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
    /// <param name="productType">Product type; 0 to load all records</param>
    /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
    /// <param name="markedAsNewOnly">A values indicating whether to load only products marked as "new"; "false" to load all records; "true" to load "marked as new" only</param>
    /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
    /// <param name="priceMin">Minimum price; null to load all records</param>
    /// <param name="priceMax">Maximum price; null to load all records</param>
    /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
    /// <param name="keywords">Keywords</param>
    /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
    /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
    /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
    /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
    /// <param name="languageId">Language identifier (search for text searching)</param>
    /// <param name="filteredSpecs">Filtered product specification identifiers</param>
    /// <param name="orderBy">Order by</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <param name="overridePublished">
    /// null - process "Published" property according to "showHidden" parameter
    /// true - load only "Published" products
    /// false - load only "Unpublished" products
    /// </param>
    /// <returns>Products</returns>
    public virtual async Task<IPagedList<Product>> SearchTicketsAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        IList<int> categoryIds = null,
        int manufacturerId = 0,
        int storeId = 0,
        int vendorId = 0,
        int warehouseId = 0,
        ProductType? productType = null,
        bool visibleIndividuallyOnly = false,
        bool markedAsNewOnly = false,
        bool? featuredProducts = null,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int productTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchManufacturerPartNumber = true,
        bool searchSku = true,
        bool searchProductTags = false,
        int languageId = 0,
        IList<int> filteredSpecs = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false,
        bool? overridePublished = null,
        bool searchAssociated = true)
    {
        //get products
        var products = await SearchProductsAsync(
            storeId: storeId,
            vendorId: vendorId,
            productType: productType,
            categoryIds: categoryIds,
            manufacturerIds: new List<int> { manufacturerId },
            keywords: keywords,
            orderBy: ProductSortingEnum.NameAsc,
            pageIndex: 0,
            pageSize: pageSize,
            showHidden: showHidden,
            overridePublished: overridePublished);

        var query = (from p in products
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           (searchAssociated || (!searchAssociated && p.ParentGroupedProductId == 0))
                     select p).ToList();

        return new PagedList<Product>(query.ToList(), pageIndex, pageSize); //totalRecords             
    }

    /// <summary>
    /// Search tickets
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="categoryIds">Category identifiers</param>
    /// <param name="manufacturerIds">Manufacturer identifiers</param>
    /// <param name="storeId">Store identifier; 0 to load all records</param>
    /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
    /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
    /// <param name="productType">Product type; 0 to load all records</param>
    /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
    /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
    /// <param name="priceMin">Minimum price; null to load all records</param>
    /// <param name="priceMax">Maximum price; null to load all records</param>
    /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
    /// <param name="keywords">Keywords</param>
    /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
    /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
    /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
    /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
    /// <param name="languageId">Language identifier (search for text searching)</param>
    /// <param name="filteredSpecOptions">Specification options list to filter products; null to load all records</param>
    /// <param name="orderBy">Order by</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <param name="overridePublished">
    /// null - process "Published" property according to "showHidden" parameter
    /// true - load only "Published" products
    /// false - load only "Unpublished" products
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the products
    /// </returns>
    public virtual async Task<List<Product>> SearchProductsAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null,
        int storeId = 0,
        int vendorId = 0,
        int warehouseId = 0,
        ProductType? productType = null,
        bool visibleIndividuallyOnly = false,
        bool excludeFeaturedProducts = false,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int productTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchManufacturerPartNumber = true,
        bool searchSku = true,
        bool searchProductTags = false,
        int languageId = 0,
        IList<SpecificationAttributeOption> filteredSpecOptions = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false,
        bool? overridePublished = null)
    {
        //some databases don't support int.MaxValue
        if (pageSize == int.MaxValue)
            pageSize = int.MaxValue - 1;

        var productsQuery = _productRepository.Table;

        if (!showHidden)
            productsQuery = productsQuery.Where(p => p.Published);
        else if (overridePublished.HasValue)
            productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

        //apply store mapping constraints
        productsQuery = ApplyStoreMapping(productsQuery, storeId);

        //apply ACL constraints
        if (!showHidden)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            productsQuery = await ApplyAclAsync(productsQuery, customer);
        }

        productsQuery =
            from p in productsQuery
            where !p.Deleted &&
                (vendorId == 0 || p.VendorId == vendorId) &&
                (productType == null || p.ProductTypeId == (int)productType)
            select p;

        if (!string.IsNullOrEmpty(keywords))
        {
            var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
            var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);

            IQueryable<int> productsByKeywords;

            productsByKeywords =
                    from p in _productRepository.Table
                    where p.Name.Contains(keywords) ||
                        (searchDescriptions &&
                            (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                        (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                        (searchSku && p.Sku == keywords)
                    select p.Id;

            if (searchLocalizedValue)
            {
                productsByKeywords = productsByKeywords.Union(
                            from lp in _localizedPropertyRepository.Table
                            let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                            lp.LocaleValue.Contains(keywords)
                            let checkShortDesc = searchDescriptions &&
                                            lp.LocaleKey == nameof(Product.ShortDescription) &&
                                            lp.LocaleValue.Contains(keywords)
                            let checkProductTags = searchProductTags &&
                                            lp.LocaleKeyGroup == nameof(ProductTag) &&
                                            lp.LocaleKey == nameof(ProductTag.Name) &&
                                            lp.LocaleValue.Contains(keywords)
                            where
                                lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc) ||
                                checkProductTags

                            select lp.EntityId);
            }

            productsQuery =
                from p in productsQuery
                from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                select p;
        }

        if (categoryIds != null)
        {
            if (categoryIds.Contains(0))
                categoryIds.Remove(0);

            if (categoryIds.Any())
            {
                var productCategoryQuery =
                    from pc in _productCategoryRepository.Table
                    where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                        categoryIds.Contains(pc.CategoryId)
                    group pc by pc.ProductId into pc
                    select new
                    {
                        ProductId = pc.Key,
                        DisplayOrder = pc.First().DisplayOrder
                    };

                productsQuery =
                    from p in productsQuery
                    join pc in productCategoryQuery on p.Id equals pc.ProductId
                    orderby pc.DisplayOrder, p.Name
                    select p;
            }
        }

        if (manufacturerIds != null)
        {
            if (manufacturerIds.Contains(0))
                manufacturerIds.Remove(0);

            if (manufacturerIds.Any())
            {
                var productManufacturerQuery =
                    from pm in _productManufacturerRepository.Table
                    where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                        manufacturerIds.Contains(pm.ManufacturerId)
                    group pm by pm.ProductId into pm
                    select new
                    {
                        ProductId = pm.Key,
                        DisplayOrder = pm.First().DisplayOrder
                    };

                productsQuery =
                    from p in productsQuery
                    join pm in productManufacturerQuery on p.Id equals pm.ProductId
                    orderby pm.DisplayOrder, p.Name
                    select p;
            }
        }

        return productsQuery.ToList();
    }

    /// <summary>
    /// Apply ACL to the passed query
    /// </summary>
    /// <typeparam name="TEntity">Type of entity that supports the ACL</typeparam>
    /// <param name="query">Query to filter</param>
    /// <param name="customer">Customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filtered query
    /// </returns>
    public virtual async Task<IQueryable<TEntity>> ApplyAclAsync<TEntity>(IQueryable<TEntity> query, Customer customer)
        where TEntity : BaseEntity, IAclSupported
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        if (customer is null)
            throw new ArgumentNullException(nameof(customer));

        var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
        return ApplyAcl(query, customerRoleIds);
    }
    /// <summary>
    /// Apply ACL to the passed query
    /// </summary>
    /// <typeparam name="TEntity">Type of entity that supports the ACL</typeparam>
    /// <param name="query">Query to filter</param>
    /// <param name="customerRoleIds">Identifiers of customer's roles</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filtered query
    /// </returns>
    public virtual IQueryable<TEntity> ApplyAcl<TEntity>(IQueryable<TEntity> query, int[] customerRoleIds)
        where TEntity : BaseEntity, IAclSupported
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        if (customerRoleIds is null)
            throw new ArgumentNullException(nameof(customerRoleIds));

        if (!customerRoleIds.Any() || _catalogSettings.IgnoreAcl || !IsEntityAclMappingExist<TEntity>())
            return query;

        return from entity in query
               where !entity.SubjectToAcl || _aclRecordRepository.Table.Any(acl =>
                    acl.EntityName == typeof(TEntity).Name && acl.EntityId == entity.Id && customerRoleIds.Contains(acl.CustomerRoleId))
               select entity;
    }

    /// <summary>
    /// Get a value indicating whether any ACL records exist for entity type are related to customer roles
    /// </summary>
    /// <typeparam name="TEntity">Type of entity that supports the ACL</typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the rue if exist; otherwise false
    /// </returns>
    protected virtual bool IsEntityAclMappingExist<TEntity>() where TEntity : BaseEntity, IAclSupported
    {
        var entityName = typeof(TEntity).Name;

        var query = from acl in _aclRecordRepository.Table
                    where acl.EntityName == entityName
                    select acl;

        if (query.Any())
            return true;

        return false;
    }


    /// <summary>
    /// Apply store mapping to the passed query
    /// </summary>
    /// <typeparam name="TEntity">Type of entity that supports store mapping</typeparam>
    /// <param name="query">Query to filter</param>
    /// <param name="storeId">Store identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filtered query
    /// </returns>
    public virtual IQueryable<TEntity> ApplyStoreMapping<TEntity>(IQueryable<TEntity> query, int storeId)
        where TEntity : BaseEntity, IStoreMappingSupported
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        if (storeId == 0 || _catalogSettings.IgnoreStoreLimitations || !IsEntityMappingExistsAsync<TEntity>())
            return query;

        return from entity in query
               where !entity.LimitedToStores || _storeMappingRepository.Table.Any(sm =>
                     sm.EntityName == typeof(TEntity).Name && sm.EntityId == entity.Id && sm.StoreId == storeId)
               select entity;
    }

    /// <summary>
    /// Get a value indicating whether a store mapping exists for an entity type
    /// </summary>
    /// <typeparam name="TEntity">Type of entity that supports store mapping</typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the rue if exists; otherwise false
    /// </returns>
    protected virtual bool IsEntityMappingExistsAsync<TEntity>() where TEntity : BaseEntity, IStoreMappingSupported
    {

        var entityName = typeof(TEntity).Name;
        var query = from sm in _storeMappingRepository.Table
                    where sm.EntityName == entityName
                    select sm.StoreId;

        if (query.Any())
            return true;

        return false;
    }

    public async Task<ProductTypeExtension> InsertEventTicketProductTypeExtensionAsync(Product product, 
        DateTime? startDate = null, DateTime? endDate = null, string? productTime = null)
    {
        var pte = new ProductTypeExtension();
        pte.ProductId = product.Id;
        pte.ProductTypeId = CoreSystemHelper.SystemMode.EventTickets;
        pte.RecordTypeId = CoreSystemHelper.RecordType.Configuration;

        if (startDate.HasValue)              
            pte.AvailabilityStartDate = _dateTimeHelper.ConvertToUtcTime(startDate.Value);
        else
            pte.AvailabilityStartDate = DateTime.UtcNow.AddDays(14).AddHours(14);

        if (endDate.HasValue)
            pte.AvailabilityEndDate = _dateTimeHelper.ConvertToUtcTime(endDate.Value);
        else
            pte.AvailabilityEndDate = DateTime.UtcNow.AddDays(14).AddHours(19);

        pte.RecordStatusId = CoreSystemHelper.RecordStatus.Initialised;

        pte.ProductTime = productTime;

        await _productTypeExtensionsService.InsertProductTypeExtension(pte);

        return pte;
    }

    public async Task<ProductTypeExtension> UpdateEventTicketProductTypeExtensionAsync(Product product, DateTime startDate, DateTime endDate,
        CoreSystemHelper.RecordStatus recordStatusId, string productTime)
    {
        var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, 
            CoreSystemHelper.RecordType.Configuration, CoreSystemHelper.SystemMode.EventTickets);
        if (pte != null)
        {
            pte.AvailabilityStartDate = _dateTimeHelper.ConvertToUtcTime(startDate);
            pte.AvailabilityEndDate = _dateTimeHelper.ConvertToUtcTime(endDate);

            pte.RecordStatusId = recordStatusId;

            pte.ProductTime = productTime;
        }

        await _productTypeExtensionsService.UpdateProductTypeExtension(pte);

        return pte;
    }

    public async Task<ProductTypeExtension> GetEventTicketProductTypeExtensionAsync(Product product)
    {
        if (product != null)
        {
            var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, CoreSystemHelper.RecordType.Configuration);
            if (pte != null)
            {
                pte.AvailabilityStartDate = await _dateTimeHelper.ConvertToUserTimeAsync(pte.AvailabilityStartDate, DateTimeKind.Utc);
                pte.AvailabilityEndDate = await _dateTimeHelper.ConvertToUserTimeAsync(pte.AvailabilityEndDate, DateTimeKind.Utc);
            }

            return pte;
        }

        return null;
    }

    public async Task<ProductTypeExtension> GetEventTicketProductTypeExtensionAsync(int productId)
    {
        var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(productId, CoreSystemHelper.RecordType.Configuration);
        if (pte != null)
        {
            pte.AvailabilityStartDate = await _dateTimeHelper.ConvertToUserTimeAsync(pte.AvailabilityStartDate, DateTimeKind.Utc);
            pte.AvailabilityEndDate = await _dateTimeHelper.ConvertToUserTimeAsync(pte.AvailabilityEndDate, DateTimeKind.Utc);
        }

        return pte;
    }

    public async Task DeleteEventTicketProductTypeExtensionAsync(Product product)
    {
        if (product != null)
        {
            var pte = _productTypeExtensionsService.GetProductTypeExtensionByProductId(product.Id, CoreSystemHelper.RecordType.Configuration);
            if (pte != null)
                await _productTypeExtensionsService.DeleteProductTypeExtension(pte);
        }
    }

    /// <summary>
    /// Gets all order for tickets on sale
    /// </summary>
    /// Payment Status can be Paid or Pending
    /// <returns>List of Order Items </returns>
    public virtual IList<OrderItem> GetAllEventTicketsOnSale(int storeId = 0, int vendorId = 0)
    {
        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           pte.AvailabilityStartDate.AddDays(1) > DateTime.UtcNow &&
                           (storeId == 0 || o.StoreId == storeId) &&
                           (vendorId == 0 || p.VendorId == vendorId) &&
                           !o.Deleted && !p.Deleted

                     select oi);

        return query.ToList();
    }

    /// <summary>
    /// Gets all order for tickets on sale for an affiliate
    /// </summary>
    /// <returns>List of Order Items </returns>
    public async virtual Task<IList<OrderItem>> GetAllAffiliateTicketsOnSaleAsync(int storeId = 0, int affiliateId = 0)
    {
        var finalList = new List<OrderItem>();

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           pte.AvailabilityStartDate.AddDays(1) > DateTime.UtcNow &&
                           (storeId == 0 || o.StoreId == storeId) &&
                           !o.Deleted && !p.Deleted
                     select oi);

        foreach (var q in query)
        {
            var affiliates = await GetAffiliatesForTicketAsync(q.ProductId, true);
            foreach (var affiliate in affiliates)
            {
                if (affiliate == affiliateId)
                    finalList.Add(q);
            }

        }

        return finalList.ToList();
    }

    /// <summary>
    /// Gets all order for tickets on sale
    /// </summary>
    /// Payment Status can be Paid or Pending
    /// <returns>List of Order Items </returns>
    public virtual IList<OrderItem> GetAllEventTicketsSold(int storeId = 0, int vendorId = 0)
    {
        int psPaid = (int)PaymentStatus.Paid;

        var query = (from oi in _orderItemRepository.Table
                     join o in _orderRepository.Table on oi.OrderId equals o.Id
                     join p in _productRepository.Table on oi.ProductId equals p.Id
                     join pte in _productTypeExtensionRepository.Table on p.Id equals pte.ProductId
                     where pte.ProductTypeId == CoreSystemHelper.SystemMode.EventTickets &&
                           pte.RecordTypeId == CoreSystemHelper.RecordType.Configuration &&
                           o.PaymentStatusId == psPaid &&
                           (storeId == 0 || o.StoreId == storeId) &&
                           (vendorId == 0 || p.VendorId == vendorId) &&
                           !o.Deleted && !p.Deleted

                     select oi);

        return query.ToList();
    }

    /// <summary>
    /// Gets all sales for product 
    /// </summary>        
    /// <returns>Total Sold and Total Values</returns>
    public async virtual Task<(int, decimal, decimal)> GetAllEventTicketsSoldValuesForReportAsync(Product product)
    {

        var productTicketVariants = new List<Product>();
        var opvCollection = new List<OrderTicketItem>();

        var pvs = await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true);
        productTicketVariants.AddRange(pvs);

        foreach (var ptv in productTicketVariants)
        {
            var opvcOnline = await GetAllOrderProductVariantsSummaryAsync(0, ptv.Id, null, null, true);
            opvCollection.AddRange(opvcOnline);
        }

        int totalSold = 0;
        decimal totalProductSales = 0;
        decimal totalProductCosts = 0;

        if (opvCollection.Count > 0)
        {
            totalSold = opvCollection.Sum(x => x.Quantity);
            totalProductCosts = opvCollection.Sum(x => x.OriginalProductCost);
            totalProductSales = opvCollection.Sum(x => x.PriceInclTax);
        }

        return (totalSold, totalProductCosts, totalProductSales);

    }

    #endregion

    #region Affiliate Methods

    public async Task<string> GetAffiliateNameAsync(Affiliate affiliate)
    {
        string name = "No Affiliate Name";

        var address = await _addressService.GetAddressByIdAsync(affiliate.AddressId);
        if (address != null)
            name = address.FirstName + " " + address.LastName + "(" + address.Email + ")";

        return name;
    }

    public async Task<List<SelectListItem>> GetAllAffiliatesListAsync()
    {
        var list = new List<SelectListItem>();

        var affiliates = await _affiliateService.GetAllAffiliatesAsync();
        foreach (var affiliate in affiliates)
        {
            string name = await GetAffiliateNameAsync(affiliate);
            var item = new SelectListItem
            {
                Text = name,
                Value = affiliate.Id.ToString()
            };

            list.Add(item);

        }

        return list;
    }

    public async Task<IList<int>> GetAffiliatesForTicketAsync(int id, bool includeParent = false)
    {
        var affiliateList = new List<int>();

        var gaList = await _genericAttributeService.GetAttributesForEntityAsync(id, "Ticket");
        foreach (var ga in gaList)
        {
            if (ga.Key == "Ticket.Agent")
            {
                int affiliateId = int.Parse(ga.Value);
                var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateId);
                if (affiliate != null)
                {
                    affiliateList.Add(affiliate.Id);
                }
            }
        }

        if (includeParent)
        {
            var parent = await _productService.GetProductByIdAsync(id);
            if (parent != null)
            {
                gaList = await _genericAttributeService.GetAttributesForEntityAsync(parent.ParentGroupedProductId, "Ticket");
                foreach (var ga in gaList)
                {
                    if (ga.Key == "Ticket.Agent")
                    {
                        int affiliateId = int.Parse(ga.Value);
                        var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateId);
                        if (affiliate != null)
                        {
                            affiliateList.Add(affiliate.Id);
                        }
                    }
                }
            }
        }

        return affiliateList;
    }

    public async Task<IPagedList<Product>> GetProductsForAffiliateAsync(int id, int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var productList = new List<Product>();

        var query = from ga in _genericAttributeRepository.Table
                    where ga.Key == "Ticket.Agent" &&
                          ga.KeyGroup == "Ticket" &&
                          ga.Value == id.ToString()
                    select ga;

        foreach (var item in query)
        {
            var product = await _productService.GetProductByIdAsync(item.EntityId);
            if (product != null)
            {
                productList.Add(product);
            }
        }

        return new PagedList<Product>(productList.ToList(), pageIndex, pageSize);
    }

    public async Task AddAffiliatesForTicketAsync(int ticketId, int addId)
    {
        bool found = false;
        var affiliateList = new List<Affiliate>();

        var gaList = await _genericAttributeService.GetAttributesForEntityAsync(ticketId, "Ticket");
        foreach (var ga in gaList)
        {
            if (ga.Key == "Ticket.Agent")
            {
                int affiliateId = int.Parse(ga.Value);
                if (affiliateId == addId)
                    found = true;
            }
        }

        if (!found)
        {
            var newGA = new GenericAttribute()
            {
                KeyGroup = "Ticket",
                Key = "Ticket.Agent",
                EntityId = ticketId,
                Value = addId.ToString(),
            };

            await _genericAttributeService.InsertAttributeAsync(newGA);
        }
    }

    public async Task DeleteAffiliatesForTicketAsync(int ticketId, int deleteId)
    {
        var affiliateList = new List<Affiliate>();

        var gaList = await _genericAttributeService.GetAttributesForEntityAsync(ticketId, "Ticket");
        foreach (var ga in gaList)
        {
            if (ga.Key == "Ticket.Agent")
            {
                int affiliateId = int.Parse(ga.Value);
                if (affiliateId == deleteId)
                {
                    await _genericAttributeService.DeleteAttributeAsync(ga);
                }
            }
        }
    }

    #endregion

}
