using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;

using Nop.Plugin.Apollo.EventTickets.Domain;

namespace Nop.Plugin.Apollo.EventTickets.Services;

public partial interface IEventTicketsMessageService
{

    public Task<IList<int>> SendOrderApprovedCustomerNotificationAsync(Order order, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null);

    public Task<IList<int>> SendOrderFinalisedCustomerNotificationAsync(Order order, int languageId,
                string attachmentFilePath = null, string attachmentFileName = null);

    public Task<IList<int>> SendSummaryReportNotificationAsync(Store store, Vendor vendor, Affiliate affiliate, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null);

    public Task<IList<int>> SendEventDoorReportNotificationAsync(Store store, Vendor vendor, Product ticket, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null);
}
