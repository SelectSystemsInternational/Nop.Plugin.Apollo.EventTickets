using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Apollo.EventTickets.Models;


public record EventTicketsSettlementModel : BaseNopEntityModel
{
    public EventTicketsSettlementModel()
    {
        AvailableGroupProducts = new List<SelectListItem>();
        Name = string.Empty;
        TotalCost = string.Empty;
        TotalSales = string.Empty;
        DisplayUrl = string.Empty;
    }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Fields.FromDate")]
    public DateTime? FromDate { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Fields.ToDate")]
    public DateTime? ToDate { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.GroupProductId")]
    public int GroupProductId { get; set; }
    public IList<SelectListItem> AvailableGroupProducts { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Values.Fields.Quantity")]
    public int Quantity { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Values.Fields.TotalCost")]
    public string TotalCost { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Values.Fields.TotalSales")]
    public string TotalSales { get; set; }

    public string DisplayUrl { get; set; }

}

public record EventTicketsSettlementListModel : BasePagedListModel<EventTicketsSettlementModel>
{
}

public record EventTicketsSettlementSearchModel : BaseSearchModel
{
    public EventTicketsSettlementSearchModel()
    {
        AvailableGroupProducts = new List<SelectListItem>();
        AvailableAssociatedProducts = new List<SelectListItem>();
        SearchProductName = string.Empty;
    }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Fields.SearchProductName")]
    public string SearchProductName { get; set; }

    [DataType(DataType.Date)]
    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Fields.FromDate")]
    public DateTime? FromDate { get; set; }

    public DateTime? FDate { get; set; }

    [DataType(DataType.Date)]
    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Sales.Fields.ToDate")]
    public DateTime? ToDate { get; set; }

    public DateTime? TDate { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.GroupProductId")]
    public int GroupProductId { get; set; }

    public IList<SelectListItem> AvailableGroupProducts { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.AssociatedProductId")]
    public int AssociatedProductId { get; set; }

    public IList<SelectListItem> AvailableAssociatedProducts { get; set; }

}
