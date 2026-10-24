using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Apollo.EventTickets.Models;

public record EventTicketsCustomerCalendarModel : BaseNopModel
{
    public EventTicketsCustomerCalendarModel()
    {

    }

    public int EventId { get; set; }

}
