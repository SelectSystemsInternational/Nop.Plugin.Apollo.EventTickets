using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Apollo.EventTickets.Models;

/// <summary>
/// Represents a product search model
/// </summary>
public record TicketSearchModel : BaseSearchModel
{
    #region Ctor

    public TicketSearchModel()
    {
        AvailableCategories = new List<SelectListItem>();
        AvailableManufacturers = new List<SelectListItem>();
        AvailableStores = new List<SelectListItem>();
        AvailableWarehouses = new List<SelectListItem>();
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

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchWarehouse")]
    public int SearchWarehouseId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
    public int SearchProductTypeId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchPublished")]
    public int SearchPublishedId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.GoDirectlyToSku")]
    public string GoDirectlyToSku { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Admin.List.SearchTemplates")]
    public bool SearchTemplates { get; set; }

    [NopResourceDisplayName("Plugin.Apollo.EventTickets.Admin.List.SearchAssociated")]
    public bool SearchAssociated { get; set; }

    public bool IsLoggedInAsVendor { get; set; }

    public bool AllowVendorsToImportProducts { get; set; }

    public bool HideStoresList { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }

    public IList<SelectListItem> AvailableManufacturers { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableWarehouses { get; set; }

    public IList<SelectListItem> AvailableVendors { get; set; }

    public IList<SelectListItem> AvailableProductTypes { get; set; }

    public IList<SelectListItem> AvailablePublishedOptions { get; set; }

    #endregion
}