using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Apollo.EventTickets.Models;

public record EventTicketsSaleSearchModel : BaseSearchModel
{
    public EventTicketsSaleSearchModel()
    {
        AvailableGroupProducts = new List<SelectListItem>();
        AvailableAssociatedProducts = new List<SelectListItem>();
        AvailablePaymentMethods = new List<SelectListItem>();
        PaymentMethod = string.Empty;
    }

    [DataType(DataType.Date)]
    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.FromDate")]
    public DateTime? FromDate { get; set; }

    public DateTime? FDate { get; set; }

    [DataType(DataType.Date)]
    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.ToDate")]
    public DateTime? ToDate { get; set; }

    public DateTime? TDate { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.IsPay")]
    public bool Ispay { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.OrderbyName")]
    public bool OrderbyName { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.GroupProductId")]
    public int GroupProductId { get; set; }

    public IList<SelectListItem> AvailableGroupProducts { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.AssociatedProductId")]
    public int AssociatedProductId { get; set; }

    public IList<SelectListItem> AvailableAssociatedProducts { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.PaymentMethod")]
    public string PaymentMethod { get; set; }

    public List<SelectListItem> AvailablePaymentMethods { get; set; }
}
