using Nop.Core.Domain.Orders;

using Nop.Plugin.Apollo.Core.Domain;
using Nop.Plugin.Apollo.EventTickets.Models;

namespace Nop.Plugin.Apollo.EventTickets.Factories;

public partial interface IEventTicketsModelFactory
{

    /// <summary>
    /// Prepare Configuration model
    /// </summary>
    /// <param name="model">ConfigureModel</param>
    /// <returns>Configuration model</returns>
    Task<ConfigureModel> PrepareConfigurationModelAsync(ConfigureModel model);

    /// <summary>
    /// Prepare Management model
    /// </summary>
    /// <param name="model">ManagementModel model</param>
    /// <returns>Configuration model</returns>
    Task<ManagementModel> PrepareManagementModelAsync(ManagementModel model);

    Task<EventTicketProductDetailsModel> PrepareEventTicketProductDetailsModelAsync(Nop.Web.Models.Catalog.ProductDetailsModel productDetailsModel,
        ProductTypeExtension productTypeExtensionModel);

    /// <summary>
    /// Prepare paged product list model
    /// </summary>
    /// <param name="searchModel">Product search model</param>
    /// <returns>Product list model</returns>
    public Task<ExtendedAffiliateProductListModel> PrepareAffiliateProductListModelAsync(ExtendedAffiliateProductSearchModel searchModel);

    /// <summary>
    /// Prepare the ticket request model
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>Order details model</returns>
    public Task<TicketRequestModel> PrepareTicketRequestModelAsync(Order order, int ticketNumber = 0);
}
