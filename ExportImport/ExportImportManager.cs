using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Localization;
using Nop.Services.ExportImport.Help;
using ClosedXML.Excel;

using Nop.Plugin.Apollo.EventTickets.Services;


namespace Nop.Plugin.Apollo.EventTickets.ExportImport;

/// <summary>
/// Export manager
/// </summary>
public partial class ExportImportManager : IExportImportManager
{

    #region Fields

    protected readonly IProductService _productService;
    protected readonly IEventTicketService _eventTicketService;
    protected readonly ILogger _logger;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IPaymentService _paymentService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IWorkContext _workContext;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly IOrderService _orderService;
    protected readonly IAddressService _addressService;
    protected readonly ICountryService _countryService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly CatalogSettings _catalogSettings;

    #endregion

    #region Ctor

    public ExportImportManager(IProductService productService, 
            IEventTicketService eventTicketService, 
            ILogger logger,
            IDateTimeHelper dateTimeHelper, 
            IPaymentService paymentService, 
            ILocalizationService localizationService,
            IWorkContext workContext, 
            IPaymentPluginManager paymentPluginManager, 
            IOrderService orderService,
            IAddressService addressService, 
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            CatalogSettings catalogSettings)

    {
        _productService = productService;
        _eventTicketService = eventTicketService;
        _logger = logger;
        _dateTimeHelper = dateTimeHelper;
        _paymentService = paymentService;
        _localizationService = localizationService;
        _workContext = workContext;
        _paymentPluginManager = paymentPluginManager;
        _orderService = orderService;
        _addressService = addressService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _catalogSettings = catalogSettings;
    }

    #endregion

    #region Export eXcel

    #endregion

    #region Export

    /// <summary>
    /// Export event sales to XLSX
    /// </summary>
    /// <param name="orders">Orders</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<byte[]> ExportEventTicketsOrderItemToXlsxAsync(IList<Order> orders)
    {
        //a vendor should have access only to part of order information
        var ignore = await _workContext.GetCurrentVendorAsync() != null;

        //lambda expressions for choosing correct order address
        async Task<Address> orderBillingAddress(Order o) => await _addressService.GetAddressByIdAsync(o.BillingAddressId);

        //property array
        var properties = new[]
        {
            new PropertyByName<Order>("OrderId", (p, l) => p.Id),
            new PropertyByName<Order>("StoreId", (p, l) => p.StoreId),
            new PropertyByName<Order>("CustomerId", (p, l) => p.CustomerId, ignore),
            new PropertyByName<Order>("OrderSubtotalInclTax", (p, l) => p.OrderSubtotalInclTax, ignore),
            new PropertyByName<Order>("OrderSubtotalExclTax", (p, l) => p.OrderSubtotalExclTax, ignore),
            new PropertyByName<Order>("OrderSubTotalDiscountInclTax", (p, l) => p.OrderSubTotalDiscountInclTax, ignore),
            new PropertyByName<Order>("OrderSubTotalDiscountExclTax", (p, l) => p.OrderSubTotalDiscountExclTax, ignore),
            new PropertyByName<Order>("OrderShippingInclTax", (p, l) => p.OrderShippingInclTax, ignore),
            new PropertyByName<Order>("OrderShippingExclTax", (p, l) => p.OrderShippingExclTax, ignore),
            new PropertyByName<Order>("PaymentMethodAdditionalFeeInclTax", (p, l) => p.PaymentMethodAdditionalFeeInclTax, ignore),
            new PropertyByName<Order>("PaymentMethodAdditionalFeeExclTax", (p, l) => p.PaymentMethodAdditionalFeeExclTax, ignore),
            new PropertyByName<Order>("TaxRates", (p, l) => p.TaxRates, ignore),
            new PropertyByName<Order>("OrderTax", (p, l) => p.OrderTax, ignore),
            new PropertyByName<Order>("OrderTotal", (p, l) => p.OrderTotal, ignore),
            new PropertyByName<Order>("RefundedAmount", (p, l) => p.RefundedAmount, ignore),
            new PropertyByName<Order>("OrderDiscount", (p, l) => p.OrderDiscount, ignore),
            new PropertyByName<Order>("CurrencyRate", (p, l) => p.CurrencyRate),
            new PropertyByName<Order>("CustomerCurrencyCode", (p, l) => p.CustomerCurrencyCode),
            new PropertyByName<Order>("AffiliateId", (p, l) => p.AffiliateId, ignore),
            new PropertyByName<Order>("PaymentMethodSystemName", (p, l) => p.PaymentMethodSystemName, ignore),
            new PropertyByName<Order>("ShippingPickupInStore", (p, l) => p.PickupInStore, ignore),
            new PropertyByName<Order>("ShippingMethod", (p, l) => p.ShippingMethod),
            new PropertyByName<Order>("ShippingRateComputationMethodSystemName", (p, l) => p.ShippingRateComputationMethodSystemName, ignore),
            new PropertyByName<Order>("CustomValuesXml", (p, l) => p.CustomValuesXml, ignore),
            new PropertyByName<Order>("VatNumber", (p, l) => p.VatNumber, ignore),
            new PropertyByName<Order>("CreatedOnUtc", (p, l) => p.CreatedOnUtc),
            new PropertyByName<Order>("BillingFirstName", async (p, l) => (await orderBillingAddress(p))?.FirstName ?? string.Empty),
            new PropertyByName<Order>("BillingLastName", async (p, l) => (await orderBillingAddress(p))?.LastName ?? string.Empty),
            new PropertyByName<Order>("BillingEmail", async (p, l) => (await orderBillingAddress(p))?.Email ?? string.Empty),
            new PropertyByName<Order>("BillingCompany", async (p, l) => (await orderBillingAddress(p))?.Company ?? string.Empty),
            new PropertyByName<Order>("BillingCountry", async (p, l) => (await _countryService.GetCountryByAddressAsync(await orderBillingAddress(p)))?.Name ?? string.Empty),
            new PropertyByName<Order>("BillingStateProvince", async (p, l) => (await _stateProvinceService.GetStateProvinceByAddressAsync(await orderBillingAddress(p)))?.Name ?? string.Empty),
            new PropertyByName<Order>("BillingCounty", async (p, l) => (await orderBillingAddress(p))?.County ?? string.Empty),
            new PropertyByName<Order>("BillingCity", async (p, l) => (await orderBillingAddress(p))?.City ?? string.Empty),
            new PropertyByName<Order>("BillingAddress1", async (p, l) => (await orderBillingAddress(p))?.Address1 ?? string.Empty),
            new PropertyByName<Order>("BillingAddress2", async (p, l) => (await orderBillingAddress(p))?.Address2 ?? string.Empty),
            new PropertyByName<Order>("BillingZipPostalCode", async (p, l) => (await orderBillingAddress(p))?.ZipPostalCode ?? string.Empty),
            new PropertyByName<Order>("BillingPhoneNumber", async (p, l) => (await orderBillingAddress(p))?.PhoneNumber ?? string.Empty),
            new PropertyByName<Order>("BillingFaxNumber", async (p, l) => (await orderBillingAddress(p))?.FaxNumber ?? string.Empty),
        };

        return await ExportApolloSalesOrderItemsToXlsxAsync(properties, orders);
    }

    private async Task<byte[]> ExportApolloSalesOrderItemsToXlsxAsync(PropertyByName<Order>[] properties, IEnumerable<Order> itemsToExport)
    {
        var orderItemProperties = new[]
        {
            new PropertyByName<OrderItem>("Name", async (oi,l) => (await _productService.GetProductByIdAsync(oi.ProductId)).Name),
            new PropertyByName<OrderItem>("AttributeDescription", (oi,l) => oi.AttributeDescription),
            new PropertyByName<OrderItem>("Sku", async (oi,l) => await _productService.FormatSkuAsync(await _productService.GetProductByIdAsync(oi.ProductId), oi.AttributesXml)),
            new PropertyByName<OrderItem>("PriceExclTax", (oi,l) => oi.UnitPriceExclTax),
            new PropertyByName<OrderItem>("PriceInclTax", (oi,l) => oi.UnitPriceInclTax),
            new PropertyByName<OrderItem>("Quantity", (oi,l) => oi.Quantity),
            new PropertyByName<OrderItem>("DiscountExclTax", (oi,l) => oi.DiscountAmountExclTax),
            new PropertyByName<OrderItem>("DiscountInclTax", (oi,l) => oi.DiscountAmountInclTax),
            new PropertyByName<OrderItem>("TotalExclTax", (oi,l) => oi.PriceExclTax),
            new PropertyByName<OrderItem>("TotalInclTax", (oi,l) => oi.PriceInclTax)
        };

        var orderItemsManager = new PropertyManager<OrderItem>(orderItemProperties, _catalogSettings);
        await using var stream = new MemoryStream();
        // ok, we can run the real code of the sample now
        using (var workbook = new XLWorkbook())
        {
            // uncomment this line if you want the XML written out to the outputDir
            //xlPackage.DebugMode = true; 

            // get handles to the worksheets
            // Worksheet names cannot be more than 31 characters
            var worksheet = workbook.Worksheets.Add(typeof(Order).Name);
            var fpWorksheet = workbook.Worksheets.Add("DataForProductsFilters");
            fpWorksheet.Visibility = XLWorksheetVisibility.VeryHidden;

            //create Headers and format them 
            var manager = new PropertyManager<Order>(properties, _catalogSettings);
            manager.WriteDefaultCaption(worksheet);

            var row = 2;
            foreach (var order in itemsToExport)
            {
                manager.CurrentObject = order;
                await manager.WriteDefaultToXlsxAsync(worksheet, row++);

                //a vendor should have access only to his products
                var vendor = await _workContext.GetCurrentVendorAsync();
                var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendor?.Id ?? 0);

                if (!orderItems.Any())
                    continue;

                orderItemsManager.WriteDefaultCaption(worksheet, row, 2);
                worksheet.Row(row).OutlineLevel = 1;
                worksheet.Row(row).Collapse();

                foreach (var orderItem in orderItems)
                {
                    row++;
                    orderItemsManager.CurrentObject = orderItem;
                    await orderItemsManager.WriteDefaultToXlsxAsync(worksheet, row, 2, fpWorksheet);
                    worksheet.Row(row).OutlineLevel = 1;
                    worksheet.Row(row).Collapse();
                }

                row++;
            }

            workbook.SaveAs(stream);
        }

        return stream.ToArray();
    }

    #endregion

    #region Import

    #endregion

}