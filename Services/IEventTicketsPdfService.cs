using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;


namespace Nop.Plugin.Apollo.EventTickets.Services;

public interface IEventTicketsPdfService
{
    public Task PrintOrdersToPdfAsync(Stream stream, IList<OrderItem> orderItems, int languageId = 0, int vendorId = 0);

    public Task<(string, string)> PrintSalesSummaryReportToPDFAsync(IList<OrderItem> orderItems, int languageId = 0,
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null, Affiliate affiliate = null);

    public Task<bool> PrintSalesSummaryReportAsync(Stream stream, IList<OrderItem> orderItems, int languageId = 0, 
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null);

    public Task<(string, string)> PrintEventDoorReportToPDFAsync(Product product, int languageId = 0,
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null);

    public Task<bool> PrintEventDoorReportAsync(Stream stream, Product product, int languageId = 0,
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null);

    Task<(string, string)> PrintEventSettlementReportToPDFAsync(Product product, int languageId = 0,
                Store store = null, Vendor vendor = null, Manufacturer manufacturer = null);

    Task<bool> PrintEventSettlementReportAsync(Stream stream, Product product, int languageId = 0,
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null);


}
