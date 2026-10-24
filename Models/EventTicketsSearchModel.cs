using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Apollo.EventTickets.Models;

public record EventTicketsSearchModel : BaseSearchModel
{
    #region Ctor

    public EventTicketsSearchModel()
    {
        SearchProductName = string.Empty;
        AvailableCategories = new List<SelectListItem>();
        AvailableManufacturers = new List<SelectListItem>();
        AvailableStores = new List<SelectListItem>();
        AvailableVendors = new List<SelectListItem>();
        AvailableProductTypes = new List<SelectListItem>();
        AvailablePublishedOptions = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
    public string SearchProductName { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
    public int SearchCategoryId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchIncludeSubCategories")]
    public bool SearchIncludeSubCategories { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
    public int SearchManufacturerId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
    public int SearchVendorId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
    public int SearchProductTypeId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchPublished")]
    public int SearchPublishedId { get; set; }

    public bool IsLoggedInAsVendor { get; set; }

    public bool AllowVendorsToImportProducts { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }

    public IList<SelectListItem> AvailableManufacturers { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableVendors { get; set; }

    public IList<SelectListItem> AvailableProductTypes { get; set; }

    public IList<SelectListItem> AvailablePublishedOptions { get; set; }

    #endregion
}
