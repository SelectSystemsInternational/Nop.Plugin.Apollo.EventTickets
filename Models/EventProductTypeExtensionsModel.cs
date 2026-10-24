using Microsoft.AspNetCore.Mvc.Rendering;

using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Apollo.Core.Models;

#region Product Type Extension Model

public record EventProductTypeExtensionModel : ProductTypeExtensionBaseModel
{
    public EventProductTypeExtensionModel()
    {
        ProductName = string.Empty;
        ProductType = string.Empty;
        AvailabilityStartDate = string.Empty;
        AvailabilityEndDate = string.Empty;
        EventTicketDateTime = DateTime.UtcNow;
        EventTicketReportDateTime = DateTime.UtcNow;
        AvailableRecordStatus = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.RecordStatusId")]
    public CoreSystemHelper.RecordStatus RecordStatusId { get; set; }

    public List<SelectListItem> AvailableRecordStatus { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.EventTicketDateTime")]
    public DateTime EventTicketDateTime { get; set; }


    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.EventTicketReportDateTime")]
    public DateTime EventTicketReportDateTime { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.ProductTime")]
    public string ProductTime { get; set; }


    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.Quantity")]
    public int Quantity { get; set; }


    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Settings.ProductEditor.Affiliates")]
    public bool Affiliates { get; set; }


}

#endregion