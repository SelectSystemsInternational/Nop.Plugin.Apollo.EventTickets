using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using Nop.Plugin.Apollo.Core.Models;

namespace Nop.Plugin.Apollo.EventTickets.Models;

public record ManagementModel : BaseNopModel
{
    public ManagementModel()
    {
        EventTicketsProductTypeExtensionSettingsSearchModel = new ProductTypeExtensionSettingsSearchModel();
    }

    public ProductTypeExtensionSettingsSearchModel EventTicketsProductTypeExtensionSettingsSearchModel { get; set; }
}
