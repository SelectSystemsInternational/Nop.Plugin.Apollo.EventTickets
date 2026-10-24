using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Apollo.EventTickets.Models;

public record ConfigureModel : BaseNopModel
{
    public ConfigureModel()
    {
        PublicKey = string.Empty;
        PrivateKey = string.Empty;
        AttachFileLocation = string.Empty;
        ProductAttributeIds = new List<int>();
        AvailableProductAttributes = new List<SelectListItem>();
        EventTicketsURLPattern = string.Empty;
        EventTicketsAccessEnabled = "none";
        CheckoutAttributeIds = new List<int>();
        AvailableCheckoutAttributes = new List<SelectListItem>();
    }
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.PublicKey")]
    public string PublicKey { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.PrivateKey")]
    public string PrivateKey { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.IsEnabled")]
    public bool Enabled { get; set; }
    public bool Enabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.TestMode")]
    public bool TestMode { get; set; }
    public bool TestMode_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.EventTicketsIsEnabled")]
    public bool EventTicketsEnabled { get; set; }
    public bool EventTicketsEnabled_OverrideForStore { get; set; }
    public string EventTicketsAccessEnabled { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.EventTicketsURLPattern")]
    public string EventTicketsURLPattern { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.ProductAttribute")]
    public IList<int> ProductAttributeIds { get; set; }
    public bool ProductAttributeId_OverrideForStore { get; set; }
    public IList<SelectListItem> AvailableProductAttributes { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.CheckoutAttribute")]
    public IList<int> CheckoutAttributeIds { get; set; }
    public bool CheckoutAttributeId_OverrideForStore { get; set; }
    public IList<SelectListItem> AvailableCheckoutAttributes { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.MaximumNumberOfOrders")]
    public int MaximumNumberOfOrders { get; set; }
    public bool MaximumNumberOfOrders_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.DeleteTablesOnUninstall")]
    public bool DeleteTablesOnUninstall { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.DeleteConfigurationDataOnUninstall")]
    public bool DeleteConfigurationDataOnUninstall { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.AttachFileLocation")]
    public string AttachFileLocation { get; set; }
    public bool AttachFileLocation_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.UseNopTemplatesAjaxCart")]
    public bool UseNopTemplatesAjaxCart { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.SeperateTickets")]
    public bool SeperateTickets { get; set; }

    public bool SeperateTickets_OverrideForStore { get; set; }
}
