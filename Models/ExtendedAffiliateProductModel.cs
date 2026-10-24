using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;


namespace Nop.Plugin.Apollo.EventTickets.Models;

public record ExtendedAffiliateProductModel : BaseNopEntityModel
{
    public ExtendedAffiliateProductModel()
    {

    }

    public int ProductId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
    public string Name { get; set; }

    //picture thumbnail
    [NopResourceDisplayName("Admin.Catalog.Products.Fields.PictureThumbnailUrl")]
    public string PictureThumbnailUrl { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
    public decimal Price { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
    public bool Published { get; set; }

}

/// <summary>
/// Represents a product search model
/// </summary>
public record ExtendedAffiliateProductSearchModel : BaseSearchModel
{

    public int SearchProductId { get; set; }

}

public record ExtendedAffiliateProductListModel : BasePagedListModel<ExtendedAffiliateProductModel>
{

}