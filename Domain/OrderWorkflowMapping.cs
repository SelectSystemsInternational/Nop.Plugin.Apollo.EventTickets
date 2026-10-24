using Nop.Core;

namespace Nop.Plugin.Apollo.EventTickets.Domain;

public class EventTicketsMapping : BaseEntity
{

    #region Properties
    public int AccommodationAvailabilityId { get; set; }

    public int OrderItemId { get; set; }

    public int ShoppingCartItemId { get; set; }

    public int Status { get; set; }

    public bool Isdelete { get; set; }

    #endregion

}
