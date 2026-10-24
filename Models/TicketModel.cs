using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

using Nop.Web.Areas.Admin.Models.Catalog;

using Nop.Plugin.Apollo.Core.Models;

namespace Nop.Plugin.Apollo.EventTickets.Models;

/// <summary>
/// Represents a product model
/// </summary>
public record TicketModel : BaseNopEntityModel, ILocalizedModel<TicketLocalizedModel>
{
    #region Ctor

    public TicketModel()
    {
        ProductModel = new ProductModel();
        ProductTypeExtension = new ProductTypeExtensionModel();
        CopyTicketModel = new CopyTicketModel();
        Locales = new List<TicketLocalizedModel>();
        AvailableAffiliates = new List<SelectListItem>();
        SelectedAffiliateIds = new List<int>();
    }

    #endregion

    #region Properties

    public ProductModel ProductModel { get; set; }

    public ProductTypeExtensionModel ProductTypeExtension { get; set; }

    //copy product
    public CopyTicketModel CopyTicketModel { get; set; }

    public IList<TicketLocalizedModel> Locales { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Fields.AffiliatesIds.Affiliates")]
    public IList<int> SelectedAffiliateIds { get; set; }
    public IList<SelectListItem> AvailableAffiliates { get; set; }


    #endregion
}

public partial class TicketLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.ShortDescription")]
    public string ShortDescription { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.FullDescription")]
    public string FullDescription { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.MetaKeywords")]
    public string MetaKeywords { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.MetaDescription")]
    public string MetaDescription { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.MetaTitle")]
    public string MetaTitle { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.SeName")]
    public string SeName { get; set; }
}