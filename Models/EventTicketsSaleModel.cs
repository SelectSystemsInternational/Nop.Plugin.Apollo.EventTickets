using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Apollo.EventTickets.Models;

public record EventTicketsSaleModel : BaseNopEntityModel
{
    public EventTicketsSaleModel()
    {
        AvailableGroupProducts = new List<SelectListItem>();
        AvailableAssociatedProducts = new List<SelectListItem>();
        AvailablePaymentMethods = new List<SelectListItem>();
        PaymentDate = string.Empty;
        PaymentMethod = string.Empty;
        Name = string.Empty;
        ParentProduct = string.Empty;
        AttributeDescription = string.Empty;
        RentalDescription = string.Empty;
        customer = string.Empty;
        PaymentStatus = string.Empty;
        Price = string.Empty;
        TotalPrice = string.Empty;
        PaymentMethodSystemName = string.Empty;
        CustomerEmail = string.Empty;
        CustomerFullName = string.Empty;
    }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.FromDate")]
    public DateTime? FromDate { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.ToDate")]
    public DateTime? ToDate { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.PaymentDate")]
    public string PaymentDate { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.IsPay")]
    public bool Ispay { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.OrderbyName")]
    public bool OrderbyName { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Apollo.Fields.PaymentMethod")]
    public string PaymentMethod { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.GroupProductId")]
    public int GroupProductId { get; set; }
    public IList<SelectListItem> AvailableGroupProducts { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.AssociatedProductId")]
    public int AssociatedProductId { get; set; }

    public IList<SelectListItem> AvailableAssociatedProducts { get; set; }

    public List<SelectListItem> AvailablePaymentMethods { get; set; }

    public int OrderItemId { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; }

    public string ParentProduct { get; set; }

    public string AttributeDescription { get; set; }

    public string RentalDescription { get; set; }

    public string customer { get; set; }

    public int Quantity { get; set; }

    public int OrderId { get; set; }

    public string PaymentStatus { get; set; }

    public string Price { get; set; }

    public string TotalPrice { get; set; }

    public string PaymentMethodSystemName { get; set; }

    public string CustomerEmail { get; set; }

    public string CustomerFullName { get; set; }

}
