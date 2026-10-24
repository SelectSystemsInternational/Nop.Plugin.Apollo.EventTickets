using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Apollo.EventTickets.Domain;

public class OrderTicketItem : OrderItem
{

    #region Properties

    /// <summary>
    ///     Order Item Id 
    /// </summary>
    public int OrderItemId;

    /// <summary>
    ///     Order Item order
    /// </summary>
    public Order Order;

    /// <summary>
    ///     Order Item product
    /// </summary>
    public Product Product;

    /// <summary>
    ///     Payment Method Friendly Name
    /// </summary>
    public string PaymentMethodFriendlyName;

    /// <summary>
    ///     Sales Outlet
    /// </summary>
    public string Outlet;

    /// <summary>
    ///     Additional names for the tickets
    /// </summary>
    public string AdditionalNames;

    #endregion

}
