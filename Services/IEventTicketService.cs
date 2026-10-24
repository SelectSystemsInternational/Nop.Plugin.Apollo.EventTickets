using Microsoft.AspNetCore.Mvc.Rendering;

using Nop.Core;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;

using Nop.Web.Models.ShoppingCart;

using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.Core.Domain;
using Nop.Plugin.Apollo.EventTickets.Domain;
using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets.Services;

public partial interface IEventTicketService
{

    #region Custom Product

    public Task<Product> GetTicketByIdAsync(int productId);

    public Task UpdateTicketAsync(Product ticket);

    #endregion

    #region Utilities

    public Task<bool> IsStoreTheme(string theme, string defaultValue = "DefaultClean");

    public Task<string> GetStoreTheme(string defaultValue = "DefaultClean");

    public Task<string> GetSupportedStoreTheme(string defaultValue = "DefaultClean");

    #endregion

    #region Methods

    public Task<bool> AuthorizeAsync(CoreSystemHelper.AccessMode requestedAccess, string publicKey = "", string privateKey = "");

    public Task<EventTicketsAccessSettings> LoadSystemAccessAsync();

    public Task ApolloLogAsync(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null);

    //public Task<bool> IsApolloSystemProductAsync(int productId);

    //public Task<int> GetApolloSystemProductAsync(int productId);

    //public Task<bool> IsApolloProductAsync(int productId);

    //public Task<bool> IsEventTicketAsync(int productId);

    //public bool IsEventTicket(Product product);

    //public Task<bool> IsEventTicketsAsync(IList<ShoppingCartModel.ShoppingCartItemModel> items);

    //public Task<bool> IsEventTicketsInCartAsync();

    //public Task<bool> IsEventTicketsCheckoutAttributeAsync(ShoppingCartModel.CheckoutAttributeModel attribute);

    //public bool IsEventTicketsCheckoutAttribute(Nop.Core.Domain.Orders.CheckoutAttribute attribute);

    public Task<string> GetCustomerAdditionalNameDetailsAsync(int orderItemId);

    public Task<string> GetCustomerEmailDetailsAsync(OrderItem orderItem);

    public Task<string> GetCustomerFullNameDetailsAsync(OrderItem orderItem);

    public Task<string> GetProductNameAsync(int productId);

    public Task<string> GetTicketEventDate(int productId, Language lang = null);

    public Task<string> GetTicketEventVenue(int productId);

    public Task<string> GetLongDateAsync(DateTime datetime, Language lang = null, bool full = false);

    public int GetDateOrder(DateTime datetime);

    public Task<string> GetProductCalendarLocaleAsync();

    IList<Product> GetAllAssociatedEventTicketsProducts();

    IList<Product> GetAllEventTicketsProducts(CoreSystemHelper.SystemMode type = 0, int vendorId = 0, ProductType productType = 0);

    IList<Product> GetAllTemplateProducts(int searchType, CoreSystemHelper.SystemMode productTypeId, CoreSystemHelper.RecordType recordTypeId = 0, int vendorId = 0);

    Task<IList<Product>> GetAllGroupProducts(CoreSystemHelper.SystemMode productTypeId, CoreSystemHelper.RecordType recordTypeId = 0,
        int storeId = 0, int vendorId = 0, bool showHidden = false);

    IList<Product> GetAllAssociatedProducts(CoreSystemHelper.SystemMode productTypeId, CoreSystemHelper.RecordType recordTypeId = 0, int vendorId = 0);

    #endregion

    #region Event Tickets Sales Orders

    public Task<int[]> GetAllEventTicketsSalesItemsAsync();

    public IList<OrderItem> GetAllEventTicketsSalesMapping();

    public Task<IList<OrderItem>> GetEventTicketsOrderSalesItemsByOrderIdsAsync(int[] orderItemIds);

    public Task<IPagedList<OrderItem>> GetEventTicketsSalesOrderItemsListAsync(int[] orderItemIds, string paymentMethod, DateTime? fromDate, DateTime? toDate,
        bool isPay, bool orderbyName, int groupProductId, int associatedProductId, string searchProductName, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Event Tickets Process Orders

    public Task<int[]> GetAllEventTicketsProcessItemsAsync();

    public Task<IList<OrderItem>> GetAllEventTicketsProcessMappingAsync();

    public IList<OrderItem> GetEventTicketsProcessOrderItemsByOrderIds(int[] orderItemIds);

    public Task<IPagedList<OrderItem>> GetEventTicketsProcessOrderItemsListAsync(int[] orderItemIds, string paymentMethod, DateTime? fromDate, DateTime? toDate,
        bool isPay, bool orderbyName, int groupProductId, int associatedProductId, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Event Tickets Delivery Orders

    public Task<int[]> GetAllEventTicketsDeliveryItemsAsync();

    public IList<OrderItem> GetAllEventTicketsDeliveryMapping();

    public IList<OrderItem> GetEventTicketsDeliveryOrderItemsByOrderIds(int[] orderItemIds);

    public Task<IPagedList<OrderItem>> GetEventTicketsDeliveryOrderItemsListAsync(int[] orderItemIds, string paymentMethod, DateTime? fromDate, DateTime? toDate,
        bool isPay, bool orderbyName, int groupProductId, int associatedProductId, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Event Tickets Settlement Orders

    public Task<int[]> GetAllEventTicketsSettlementItemsAsync();

    public IList<Product> GetAllEventTicketsSettlementMapping();

    public IList<OrderItem> GetEventTicketsSettlementOrderItemsByOrderIds(int[] orderItemIds);

    public IPagedList<ProductTypeExtension> GetEventTicketsSettlementItemsList(int[] productItemIds, DateTime? fromDate, DateTime? toDate,
        int groupProductId, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Order Update Status

    bool CanMarkOrderAsApproved(Order order);

    Task MarkOrderAsApprovedAsync(Order order);

    bool CanMarkOrderReturnedToBeChecked(Order order);

    Task MarkOrderReturnedToBeCheckedAsync(Order order);

    bool CanMarkOrderAsProcessed(Order order);

    Task MarkOrderAsProcessedAsync(Order order);

    bool CanMarkOrderReturnedToBeProcessed(Order order);

    Task MarkOrderReturnedToBeProcessedAsync(Order order);

    bool CanMarkOrderAsInTransit(Order order);

    Task MarkOrderAsInTransitAsync(Order order);

    bool CanMarkTicketAsFinalised(Product order);

    Task MarkTicketAsFinalisedAsync(Product product);

    bool CanMarkTicketReportPrinted(Product product);

    Task MarkTicketAsReportPrintedAsync(Product product);

    public bool CanMarkTicketOperating(Product product);

    Task MarkTicketAsOperatingAsync(Product product);

    #endregion

    #region Shipping

    public Task CreateOrUpadeteShippingAsync(int orderNumber, string carrier, string service, string trackingNumber);

    #endregion

    #region Ticket Sales


    /// <summary>
    ///     Gets all order product variants
    /// </summary>
    /// <returns>Order collection</returns>
    public Task<List<OrderTicketItem>> GetAllOrderProductVariantsSummaryAsync(int storeId, int? productVariantId,
        DateTime? startTime, DateTime? endTime, bool includeOffline);

    /// <summary>
    ///     Gets quantity of product variant purchased by customer
    /// </summary>
    public int GetProductVariantOrderedTotal(int productVariantId,
        int? customerId,
        DateTime? dateFrom,
        DateTime? dateTo,
        OrderStatus? os,
        PaymentStatus? ps,
        ShippingStatus? ss);

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
        ShippingStatus? ss);

    #endregion

    #region Tickets

    Task<IPagedList<Product>> SearchTicketsAsync(
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
        bool searchAssociated = true);

    Task<ProductTypeExtension> InsertEventTicketProductTypeExtensionAsync(Product product,
        DateTime? startDate = null, DateTime? endDate = null, string? productTime = null);

    Task<ProductTypeExtension> UpdateEventTicketProductTypeExtensionAsync(Product product,
        DateTime startDate, DateTime endDate, CoreSystemHelper.RecordStatus recordStatus, string productTime);

    Task<ProductTypeExtension> GetEventTicketProductTypeExtensionAsync(Product product);

    Task<ProductTypeExtension> GetEventTicketProductTypeExtensionAsync(int productId);

    Task DeleteEventTicketProductTypeExtensionAsync(Product product);

    IList<OrderItem> GetAllEventTicketsOnSale(int storeId = 0, int vendorId = 0);

    /// <summary>
    /// Gets all order for tickets on sale for an affiliate
    /// </summary>
    /// <returns>List of Order Items </returns>
    Task<IList<OrderItem>> GetAllAffiliateTicketsOnSaleAsync(int storeId = 0, int affiliateId = 0);

    IList<OrderItem> GetAllEventTicketsSold(int storeId = 0, int vendorId = 0);

    Task<(int, decimal, decimal)> GetAllEventTicketsSoldValuesForReportAsync(Product product);

    #endregion

    #region Affiliate Methods

    public Task<string> GetAffiliateNameAsync(Affiliate affiliate);

    public Task<List<SelectListItem>> GetAllAffiliatesListAsync();

    public Task<IList<int>> GetAffiliatesForTicketAsync(int id, bool includeParent = false);

    public Task<IPagedList<Product>> GetProductsForAffiliateAsync(int id, int pageIndex = 0, int pageSize = int.MaxValue);

    public Task AddAffiliatesForTicketAsync(int ticketId, int addId);

    public Task DeleteAffiliatesForTicketAsync(int ticketId, int deleteId);

    #endregion

}
