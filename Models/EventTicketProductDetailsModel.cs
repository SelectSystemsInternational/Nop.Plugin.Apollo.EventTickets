using System;
using Nop.Web.Models.Catalog;
using Nop.Web.Framework.Models;

using Nop.Plugin.Apollo.Core.Models;

namespace Nop.Plugin.Apollo.EventTickets.Models;

public record EventTicketProductDetailsModel : BaseNopEntityModel
{
    public EventTicketProductDetailsModel()
    {
        ProductDetailsModel = new Nop.Web.Models.Catalog.ProductDetailsModel();
        ProductTypeExtensionModel = new ProductTypeExtensionModel();
        ProductTypeExtensionModel.EventTicketDateTime = DateTime.Now;
        ProductTypeExtensionModel.EventTicketReportDateTime = DateTime.Now;
        VendorModel = new VendorBriefInfoModel();
    }

    public Nop.Web.Models.Catalog.ProductDetailsModel ProductDetailsModel { get; set; }

    public ProductTypeExtensionModel ProductTypeExtensionModel { get; set; }

    public VendorBriefInfoModel VendorModel { get; set; }

}
