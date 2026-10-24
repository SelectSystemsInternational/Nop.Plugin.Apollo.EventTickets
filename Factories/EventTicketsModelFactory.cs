using Microsoft.AspNetCore.Mvc.Rendering;

using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Models.Catalog;

using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.Core.Domain;
using Nop.Plugin.Apollo.Core.Services;
using Nop.Plugin.Apollo.EventTickets.Models;
using Nop.Plugin.Apollo.EventTickets.Services;
using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets.Factories;

public partial class EventTicketsModelFactory : IEventTicketsModelFactory
{

    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly CatalogSettings _catalogSettings;
    protected readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    protected readonly ICategoryService _categoryService;
    protected readonly IDiscountService _discountService;
    protected readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILocalizedModelFactory _localizedModelFactory;
    protected readonly IProductService _productService;
    protected readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly IStoreContext _storeContext;
    protected readonly IProductAttributeService _productAttributeService;
    protected readonly ISettingService _settingService;
    protected readonly IEventTicketService _eventTicketService;
    protected readonly INotificationService _notificationService;
    protected readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
    protected readonly IPictureService _pictureService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IOrderService _orderService;
    protected readonly IAddressService _addressService;
    protected readonly IVendorService _vendorService;
    protected readonly ICustomerService _customerService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly ICountryService _countryService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly IApolloCoreService _apolloCoreService;

    #endregion

    #region Ctor

    public EventTicketsModelFactory(IWorkContext workContext,
        CatalogSettings catalogSettings,
        IAclSupportedModelFactory aclSupportedModelFactory,
        ICategoryService categoryService,
        IDiscountService discountService,
        IDiscountSupportedModelFactory discountSupportedModelFactory,
        ILocalizationService localizationService,
        ILocalizedModelFactory localizedModelFactory,
        IProductService productService,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        IUrlRecordService urlRecordService,
        IStoreContext storeContext,
        IProductAttributeService productAttributeService,
        ISettingService settingService,
        IEventTicketService eventTicketService,
        INotificationService notificationService,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        IPictureService pictureService,
        IDateTimeHelper dateTimeHelper,
        IOrderService orderService,
        IAddressService addressService,
        IVendorService vendorService,
        ICustomerService customerService,
        IPaymentPluginManager paymentPluginManager,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        IApolloCoreService apolloCoreService)
    {
        _workContext = workContext;
        _catalogSettings = catalogSettings;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _categoryService = categoryService;
        _discountService = discountService;
        _discountSupportedModelFactory = discountSupportedModelFactory;
        _localizationService = localizationService;
        _localizedModelFactory = localizedModelFactory;
        _productService = productService;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _urlRecordService = urlRecordService;
        _storeContext = storeContext;
        _productAttributeService = productAttributeService;
        _settingService = settingService;
        _eventTicketService = eventTicketService;
        _notificationService = notificationService;
        _checkoutAttributeService = checkoutAttributeService;
        _pictureService = pictureService;
        _dateTimeHelper = dateTimeHelper;
        _orderService = orderService;
        _addressService = addressService;
        _vendorService = vendorService;
        _customerService = customerService;
        _paymentPluginManager = paymentPluginManager;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _apolloCoreService = apolloCoreService;
    }

    #endregion

    #region Utilities

    #endregion

    #region Methods

    /// <summary>
    /// Prepare Configuration model
    /// </summary>
    /// <param name="configModel">Accommodation model</param>
    /// <returns>Accommodation model</returns>
    public async Task<ConfigureModel> PrepareConfigurationModelAsync(ConfigureModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var apolloConfigSettings = await _settingService.LoadSettingAsync<EventTicketSettings>(storeScope);

        var eventTicketsAccessSettings = await _settingService.LoadSettingAsync<EventTicketsAccessSettings>(storeScope);

        bool access = await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Configure, apolloConfigSettings.PublicKey, apolloConfigSettings.PrivateKey);

        model.Enabled = apolloConfigSettings.Enabled;
        if (model.Enabled)
        {
            if (!access)
                model.Enabled = false;
        }

        model.PublicKey = apolloConfigSettings.PublicKey;
        model.PrivateKey = apolloConfigSettings.PrivateKey;

        model.EventTicketsEnabled = apolloConfigSettings.EventTicketsEnabled;
        model.EventTicketsAccessEnabled = eventTicketsAccessSettings.AccessEventTickets ? "" : "none";

        model.ActiveStoreScopeConfiguration = storeScope;
        if (!String.IsNullOrEmpty(apolloConfigSettings.ProductAttributeIds))
        {
            model.ProductAttributeIds = apolloConfigSettings.ProductAttributeIds.Split(',').Select(int.Parse).ToList();
        }
        model.ActiveStoreScopeConfiguration = storeScope;
        if (!String.IsNullOrEmpty(apolloConfigSettings.CheckoutAttributeIds))
        {
            model.CheckoutAttributeIds = apolloConfigSettings.CheckoutAttributeIds.Split(',').Select(int.Parse).ToList();
        }

        model.MaximumNumberOfOrders = apolloConfigSettings.MaximumNumberOfOrders;
        model.DeleteTablesOnUninstall = apolloConfigSettings.DeleteTablesOnUninstall;
        model.DeleteConfigurationDataOnUninstall = apolloConfigSettings.DeleteConfigurationDataOnUninstall;
        model.AttachFileLocation = apolloConfigSettings.AttachFileLocation;
        model.EventTicketsURLPattern = apolloConfigSettings.EventTicketsURLPattern;
        model.TestMode = apolloConfigSettings.TestMode;
        model.UseNopTemplatesAjaxCart = apolloConfigSettings.UseNopTemplatesAjaxCart;
        model.SeperateTickets = apolloConfigSettings.SeperateTickets;

        //prepare available product attributes
        model.AvailableProductAttributes = (await _productAttributeService.GetAllProductAttributesAsync()).Select(productAttribute => new SelectListItem
        {
            Text = productAttribute.Name,
            Value = productAttribute.Id.ToString()
        }).ToList();

        //prepare available checkout attributes
        model.AvailableCheckoutAttributes = (await _checkoutAttributeService.GetAllAttributesAsync()).Select(checkoutAttribute => new SelectListItem
        {
            Text = checkoutAttribute.Name,
            Value = checkoutAttribute.Id.ToString()
        }).ToList();

        model.ActiveStoreScopeConfiguration = storeScope;
        if (storeScope > 0)
        {
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(apolloConfigSettings, x => x.Enabled, storeScope);
            model.EventTicketsEnabled_OverrideForStore = await _settingService.SettingExistsAsync(apolloConfigSettings, x => x.EventTicketsEnabled, storeScope);
            model.MaximumNumberOfOrders_OverrideForStore = await _settingService.SettingExistsAsync(apolloConfigSettings, x => x.MaximumNumberOfOrders, storeScope);
            model.ProductAttributeId_OverrideForStore = await _settingService.SettingExistsAsync(apolloConfigSettings, x => x.ProductAttributeIds, storeScope);
            model.CheckoutAttributeId_OverrideForStore = await _settingService.SettingExistsAsync(apolloConfigSettings, x => x.CheckoutAttributeIds, storeScope);
            model.AttachFileLocation_OverrideForStore = await _settingService.SettingExistsAsync(apolloConfigSettings, x => x.AttachFileLocation, storeScope);
            model.TestMode_OverrideForStore = await _settingService.SettingExistsAsync(apolloConfigSettings, x => x.TestMode, storeScope);
            model.SeperateTickets_OverrideForStore = await _settingService.SettingExistsAsync(apolloConfigSettings, x => x.SeperateTickets, storeScope);
        }

        if (!access)
            _notificationService.ErrorNotification("Demo Version has Expired - Please enter the Public and Private Keys");

        return model;
    }

    /// <summary>
    /// Prepare Management model
    /// </summary>
    /// <param name="Management">Management model</param>
    /// <returns>Management model</returns>
    public virtual async Task<ManagementModel> PrepareManagementModelAsync(ManagementModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var apolloConfigSettings = await _settingService.LoadSettingAsync<EventTicketSettings>(storeScope);

        return model;
    }

    /// <summary>
    /// Prepare Management model
    /// </summary>
    /// <param name="Management">Management model</param>
    /// <returns>Management model</returns>
    public virtual async Task<EventTicketProductDetailsModel> PrepareEventTicketProductDetailsModelAsync(Nop.Web.Models.Catalog.ProductDetailsModel productDetailsModel,
        ProductTypeExtension productTypeExtensionModel)
    {
        if (productDetailsModel == null)
            throw new ArgumentNullException(nameof(productDetailsModel));

        var model = new EventTicketProductDetailsModel();

        model.ProductDetailsModel = productDetailsModel;

        model.ProductTypeExtensionModel.EventTicketDateTime = productTypeExtensionModel.AvailabilityStartDate;
        model.ProductTypeExtensionModel.AvailabilityStartDate = await _apolloCoreService.GetLocalisedDateTimeAsync(model.ProductTypeExtensionModel.EventTicketDateTime, null, true);
        
        model.ProductTypeExtensionModel.EventTicketReportDateTime = productTypeExtensionModel.AvailabilityEndDate;
        model.ProductTypeExtensionModel.AvailabilityEndDate = await _apolloCoreService.GetLocalisedDateTimeAsync(model.ProductTypeExtensionModel.EventTicketReportDateTime, null, true);

        model.ProductTypeExtensionModel.ProductTime = productTypeExtensionModel.ProductTime;

        model.ProductTypeExtensionModel.VenueAddress = string.Empty;
        var vendor = await _vendorService.GetVendorByIdAsync(productDetailsModel.VendorModel.Id);
        if (vendor != null)
        {
            model.VendorModel = new VendorBriefInfoModel
            {
                Id = vendor.Id,
                Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                SeName = await _urlRecordService.GetSeNameAsync(vendor),
            };

            var venueAddress = await _addressService.GetAddressByIdAsync(vendor.AddressId);
            if (venueAddress != null)
            {
                var lang = await _workContext.GetWorkingLanguageAsync();
                model.ProductTypeExtensionModel.VenueAddress = $"{venueAddress.Address1 + venueAddress.Address2}, " +
                              $"{(!string.IsNullOrEmpty(venueAddress.City) ? $"{venueAddress.City}, " : string.Empty)}" +
                              $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(venueAddress) is StateProvince stateProvince ? (await _localizationService.GetLocalizedAsync(stateProvince, x => x.Abbreviation, lang.Id)) : string.Empty)}";
            }
        }

        return model;
    }


    /// <summary>
    /// Prepare paged product list model
    /// </summary>
    /// <param name="searchModel">Product search model</param>
    /// <returns>Product list model</returns>
    public async virtual Task<ExtendedAffiliateProductListModel> PrepareAffiliateProductListModelAsync(ExtendedAffiliateProductSearchModel searchModel)
    {

        var products = await _eventTicketService.GetProductsForAffiliateAsync(searchModel.SearchProductId);

        //prepare list model
        var model = await new ExtendedAffiliateProductListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                //fill in model values from the entity
                var productModel = new ExtendedAffiliateProductModel();

                productModel.Id = product.Id;
                productModel.Price = product.Price;
                productModel.Published = product.Published;
                productModel.ProductId = product.Id;

                var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                (productModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);

                productModel.Name = product.Name;
                if (product.ParentGroupedProductId != 0)
                {
                    var parentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                    if (parentProduct != null)
                    {
                        productModel.Name = parentProduct.Name + " - " + productModel.Name;
                    }
                }

                return productModel;

            });
        });

        return model;

    }

    /// <summary>
    /// Prepare the ticket request model
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>Order details model</returns>
    public async virtual Task<TicketRequestModel> PrepareTicketRequestModelAsync(Order order, int ticketNumber = 0)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        var model = new TicketRequestModel
        {
            Id = order.Id,
            CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
            OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
            CustomOrderNumber = order.CustomOrderNumber,
        };

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
        if (billingAddress != null)
        {
            model.BillingAddress.Id = billingAddress.Id;
            model.BillingAddress.FirstName = billingAddress.FirstName;
            model.BillingAddress.LastName = billingAddress.LastName;
            model.BillingAddress.Email = billingAddress.Email;
            model.BillingAddress.CountryId = billingAddress.CountryId;
            model.BillingAddress.CountryName = (await _countryService.GetCountryByAddressAsync(billingAddress)) is Country country ? (await _localizationService.GetLocalizedAsync(country, x => x.Name)) : null;
            model.BillingAddress.StateProvinceId = billingAddress.StateProvinceId;
            model.BillingAddress.StateProvinceName = (await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress)) is StateProvince stateProvince ? (await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name)) : null;
            model.BillingAddress.County = billingAddress.County;
            model.BillingAddress.City = billingAddress.City;
            model.BillingAddress.Address1 = billingAddress.Address1;
            model.BillingAddress.Address2 = billingAddress.Address2;
            model.BillingAddress.ZipPostalCode = billingAddress.ZipPostalCode;
            model.BillingAddress.PhoneNumber = billingAddress.PhoneNumber;
        }

        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        //payment method
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var paymentMethod = await _paymentPluginManager
            .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName, customer, order.StoreId);
        model.PaymentMethod = paymentMethod != null ? (await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, languageId)) : order.PaymentMethodSystemName;
        model.PaymentMethodStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);

        //purchased products
        model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
        model.ShowVendorName = true;

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

        foreach (var orderItem in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            string productName = product.Name;
            if (product.ParentGroupedProductId != 0)
            {
                var parentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentProduct != null)
                    productName = parentProduct.Name + " - " + productName;
            }

            int quantity = 0;
            if (ticketNumber == 0)
                quantity = orderItem.Quantity;
            else
                quantity = 1;

            var orderItemModel = new TicketRequestModel.TicketItemModel
            {
                Id = orderItem.Id,
                Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml),
                VendorName = (await _vendorService.GetVendorByIdAsync(product.VendorId))?.Name ?? string.Empty,
                ProductId = product.Id,
                ProductName = productName,
                ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                Quantity = quantity,
                AttributeInfo = orderItem.AttributeDescription,
                TicketNumber = ticketNumber == 0 ? string.Empty : ticketNumber.ToString()
            };

            model.Items.Add(orderItemModel);

        }

        return model;
    }

    #endregion
}
