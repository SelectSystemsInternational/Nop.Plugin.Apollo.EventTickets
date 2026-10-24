using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Apollo.EventTickets.Models;

/// <summary>
/// Represents a copy product model
/// </summary>
public record CopyTicketModel : BaseNopEntityModel
{

    #region Ctor

    public CopyTicketModel()
    {
        Name = string.Empty;
        AvailableTemplates = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Admin.Copy.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Admin.Copy.CopyImages")]
    public bool CopyImages { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Admin.Copy.Published")]
    public bool Published { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Admin.Copy.UseTemplate")]
    public bool UseTemplate { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Admin.Copy.SelectedTemplateId")]
    public int SelectedTemplateId { get; set; }

    public IList<SelectListItem> AvailableTemplates { get; set; }

    #endregion
}