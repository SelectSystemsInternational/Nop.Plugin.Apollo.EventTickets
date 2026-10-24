using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;

using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

using Nop.Web.Areas.Admin.Controllers;

using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.EventTickets.Factories;
using Nop.Plugin.Apollo.EventTickets.Services;
using Nop.Plugin.Apollo.EventTickets.Domain;
using Nop.Plugin.Apollo.EventTickets.Models;

namespace Nop.Plugin.Apollo.EventTickets.Controllers;

public partial class TicketController : BaseAdminController
{

    #region Fields

    protected readonly IAclService _aclService;
    protected readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
    protected readonly ICategoryService _categoryService;
    protected readonly ICopyProductService _copyProductService;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly ICustomerService _customerService;
    protected readonly IDiscountService _discountService;
    protected readonly IDownloadService _downloadService;
    protected readonly IExportManager _exportManager;
    protected readonly IImportManager _importManager;
    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILocalizedEntityService _localizedEntityService;
    protected readonly IManufacturerService _manufacturerService;
    protected readonly INopFileProvider _fileProvider;
    protected readonly INotificationService _notificationService;
    protected readonly IPdfService _pdfService;
    protected readonly IPermissionService _permissionService;
    protected readonly IPictureService _pictureService;
    protected readonly IProductAttributeParser _productAttributeParser;
    protected readonly IProductAttributeService _productAttributeService;
    protected readonly IProductModelFactory _productModelFactory;
    protected readonly IProductService _productService;
    protected readonly IProductTagService _productTagService;
    protected readonly ISettingService _settingService;
    protected readonly IShippingService _shippingService;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly ISpecificationAttributeService _specificationAttributeService;
    protected readonly IStoreContext _storeContext;
    protected readonly IStoreMappingService _storeMappingService;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly IWarehouseService _warehouseService;
    protected readonly IWorkContext _workContext;
    protected readonly VendorSettings _vendorSettings;
    protected readonly ITicketModelFactory _ticketModelFactory;
    protected readonly IEventTicketService _eventTicketService;
    protected readonly IEventTicketsPdfService _eventTicketsPdfService;
    protected readonly OrderSettings _orderSettings;
    protected readonly IRepository<UrlRecord> _urlRecordRepository;
    protected readonly IStoreService _storeService;

    #endregion

    #region Ctor

    public TicketController(IAclService aclService,
        IBackInStockSubscriptionService backInStockSubscriptionService,
        ICategoryService categoryService,
        ICopyProductService copyProductService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDiscountService discountService,
        IDownloadService downloadService,
        IExportManager exportManager,
        IImportManager importManager,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IManufacturerService manufacturerService,
        INopFileProvider fileProvider,
        INotificationService notificationService,
        IPdfService pdfService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IProductTagService productTagService,
        ISettingService settingService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        ISpecificationAttributeService specificationAttributeService,
        IStoreMappingService storeMappingService,
        IStoreContext storeContext,
        IUrlRecordService urlRecordService,
        IWarehouseService warehouseService,
        IWorkContext workContext,
        VendorSettings vendorSettings,
        ITicketModelFactory ticketModelFactory,
        IEventTicketService eventTicketService,
        IEventTicketsPdfService eventTicketsPdfService,
        OrderSettings orderSettings,
        IRepository<UrlRecord> urlRecordRepository,
        IStoreService storeService)
    {
        _aclService = aclService;
        _backInStockSubscriptionService = backInStockSubscriptionService;
        _categoryService = categoryService;
        _copyProductService = copyProductService;
        _customerActivityService = customerActivityService;
        _customerService = customerService;
        _discountService = discountService;
        _downloadService = downloadService;
        _exportManager = exportManager;
        _importManager = importManager;
        _languageService = languageService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _manufacturerService = manufacturerService;
        _fileProvider = fileProvider;
        _notificationService = notificationService;
        _pdfService = pdfService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _productAttributeParser = productAttributeParser;
        _productAttributeService = productAttributeService;
        _productModelFactory = productModelFactory;
        _productService = productService;
        _productTagService = productTagService;
        _settingService = settingService;
        _shippingService = shippingService;
        _shoppingCartService = shoppingCartService;
        _specificationAttributeService = specificationAttributeService;
        _storeMappingService = storeMappingService;
        _storeContext = storeContext;
        _urlRecordService = urlRecordService;
        _warehouseService = warehouseService;
        _workContext = workContext;
        _vendorSettings = vendorSettings;
        _ticketModelFactory = ticketModelFactory;
        _eventTicketService = eventTicketService;
        _eventTicketsPdfService = eventTicketsPdfService;
        _orderSettings = orderSettings;
        _urlRecordRepository = urlRecordRepository;
        _storeService = storeService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(Product product, ProductModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.ShortDescription,
                localized.ShortDescription,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.FullDescription,
                localized.FullDescription,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.MetaKeywords,
                localized.MetaKeywords,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.MetaDescription,
                localized.MetaDescription,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.MetaTitle,
                localized.MetaTitle,
                localized.LanguageId);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(product, localized.SeName, localized.Name, false);
            await _urlRecordService.SaveSlugAsync(product, seName, localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(ProductTag productTag, ProductTagModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(productTag,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            var seName = await _urlRecordService.ValidateSeNameAsync(productTag, string.Empty, localized.Name, false);
            await _urlRecordService.SaveSlugAsync(productTag, seName, localized.LanguageId);
        }
    }

    protected virtual async Task UpdatePictureSeoNamesAsync(Product product)
    {
        foreach (var pp in await _productService.GetProductPicturesByProductIdAsync(product.Id))
            await _pictureService.SetSeoFilenameAsync(pp.PictureId, await _pictureService.GetPictureSeNameAsync(product.Name));
    }

    protected virtual async Task SaveProductAclAsync(Product product, ProductModel model)
    {
        product.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _productService.UpdateProductAsync(product);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(product);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                    await _aclService.InsertAclRecordAsync(product, customerRole.Id);
            }
            else
            {
                //remove role
                var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                if (aclRecordToDelete != null)
                    await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
            }
        }
    }

    protected virtual async Task SaveCategoryMappingsAsync(Product product, ProductModel model)
    {
        var existingProductCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);

        //delete categories
        foreach (var existingProductCategory in existingProductCategories)
            if (!model.SelectedCategoryIds.Contains(existingProductCategory.CategoryId))
                await _categoryService.DeleteProductCategoryAsync(existingProductCategory);

        //add categories
        foreach (var categoryId in model.SelectedCategoryIds)
        {
            if (_categoryService.FindProductCategory(existingProductCategories, product.Id, categoryId) == null)
            {
                //find next display order
                var displayOrder = 1;
                var existingCategoryMapping = await _categoryService.GetProductCategoriesByCategoryIdAsync(categoryId, showHidden: true);
                if (existingCategoryMapping.Any())
                    displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
                await _categoryService.InsertProductCategoryAsync(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = categoryId,
                    DisplayOrder = displayOrder
                });
            }
        }
    }

    protected virtual async Task SaveManufacturerMappingsAsync(Product product, ProductModel model)
    {
        var existingProductManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);

        //delete manufacturers
        foreach (var existingProductManufacturer in existingProductManufacturers)
            if (!model.SelectedManufacturerIds.Contains(existingProductManufacturer.ManufacturerId))
                await _manufacturerService.DeleteProductManufacturerAsync(existingProductManufacturer);

        //add manufacturers
        foreach (var manufacturerId in model.SelectedManufacturerIds)
        {
            if (_manufacturerService.FindProductManufacturer(existingProductManufacturers, product.Id, manufacturerId) == null)
            {
                //find next display order
                var displayOrder = 1;
                var existingManufacturerMapping = await _manufacturerService.GetProductManufacturersByManufacturerIdAsync(manufacturerId, showHidden: true);
                if (existingManufacturerMapping.Any())
                    displayOrder = existingManufacturerMapping.Max(x => x.DisplayOrder) + 1;
                await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
                {
                    ProductId = product.Id,
                    ManufacturerId = manufacturerId,
                    DisplayOrder = displayOrder
                });
            }
        }
    }

    protected virtual async Task SaveDiscountMappingsAsync(Product product, ProductModel model)
    {
        var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToSkus, showHidden: true);

        foreach (var discount in allDiscounts)
        {
            if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
            {
                //new discount
                if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is null)
                    await _productService.InsertDiscountProductMappingAsync(new DiscountProductMapping { EntityId = product.Id, DiscountId = discount.Id });
            }
            else
            {
                //remove discount
                if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is DiscountProductMapping discountProductMapping)
                    await _productService.DeleteDiscountProductMappingAsync(discountProductMapping);
            }
        }

        await _productService.UpdateProductAsync(product);
    }

    protected virtual string[] ParseProductTags(string productTags)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(productTags))
            return result.ToArray();

        var values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var val in values)
            if (!string.IsNullOrEmpty(val.Trim()))
                result.Add(val.Trim());

        return result.ToArray();
    }

    protected virtual async Task SaveProductWarehouseInventoryAsync(Product product, ProductModel model)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (model.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
            return;

        if (!model.UseMultipleWarehouses)
            return;

        var warehouses = await _warehouseService.GetAllWarehousesAsync();

        var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

        foreach (var warehouse in warehouses)
        {
            //parse stock quantity
            var stockQuantity = 0;
            foreach (var formKey in formData.Keys)
            {
                if (!formKey.Equals($"warehouse_qty_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                int.TryParse(formData[formKey], out stockQuantity);
                break;
            }

            //parse reserved quantity
            var reservedQuantity = 0;
            foreach (var formKey in formData.Keys)
                if (formKey.Equals($"warehouse_reserved_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(formData[formKey], out reservedQuantity);
                    break;
                }

            //parse "used" field
            var used = false;
            foreach (var formKey in formData.Keys)
                if (formKey.Equals($"warehouse_used_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(formData[formKey], out var tmp);
                    used = tmp == warehouse.Id;
                    break;
                }

            //quantity change history message
            var message = $"{await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.MultipleWarehouses")} " +
                $"{await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit")}";

            var existingPwI = (await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id)).FirstOrDefault(x => x.WarehouseId == warehouse.Id);
            if (existingPwI != null)
            {
                if (used)
                {
                    var previousStockQuantity = existingPwI.StockQuantity;

                    //update existing record
                    existingPwI.StockQuantity = stockQuantity;
                    existingPwI.ReservedQuantity = reservedQuantity;
                    await _productService.UpdateProductWarehouseInventoryAsync(existingPwI);

                    //quantity change history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity - previousStockQuantity, existingPwI.StockQuantity,
                        existingPwI.WarehouseId, message);
                }
                else
                {
                    //delete. no need to store record for qty 0
                    await _productService.DeleteProductWarehouseInventoryAsync(existingPwI);

                    //quantity change history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, -existingPwI.StockQuantity, 0, existingPwI.WarehouseId, message);
                }
            }
            else
            {
                if (!used)
                    continue;

                //no need to insert a record for qty 0
                existingPwI = new ProductWarehouseInventory
                {
                    WarehouseId = warehouse.Id,
                    ProductId = product.Id,
                    StockQuantity = stockQuantity,
                    ReservedQuantity = reservedQuantity
                };

                await _productService.InsertProductWarehouseInventoryAsync(existingPwI);

                //quantity change history
                await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity, existingPwI.StockQuantity,
                    existingPwI.WarehouseId, message);
            }
        }
    }

    protected virtual async Task SaveAffiliateMappings(Product product, TicketModel model)
    {
        var existingAffiliates = await _eventTicketService.GetAffiliatesForTicketAsync(product.Id);

        //delete affiliates
        foreach (var existingAffiliate in existingAffiliates)
            if (!model.SelectedAffiliateIds.Contains(existingAffiliate))
                await _eventTicketService.DeleteAffiliatesForTicketAsync(product.Id, existingAffiliate);

        //add affiliates
        foreach (var affiliateId in model.SelectedAffiliateIds)
            await _eventTicketService.AddAffiliatesForTicketAsync(product.Id, affiliateId);
    }

    #endregion

    #region Ticket Methods 

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("pdf-print-door-report")]
    public virtual async Task<IActionResult> PrintEventDoorReport(TicketModel model)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Operate))
            return AccessDeniedView();

        var product = await _productService.GetProductByIdAsync(model.Id);
        if (product != null)
        {

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {

                    await _eventTicketsPdfService.PrintEventDoorReportAsync(stream, product,
                        _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id,
                        await _storeContext.GetCurrentStoreAsync(), await _workContext.GetCurrentVendorAsync());

                    bytes = stream.ToArray();
                }

                string filename = "Event Door Report - " + product.Name + ".pdf";
                return File(bytes, MimeTypes.ApplicationPdf, filename);
            }
            catch (Exception exc)
            {
                //ErrorNotification(exc);
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("Edit", new { id = product.Id });
            }
        }

        return RedirectToAction("Edit");
    }

    #endregion

    #region Methods

    #region Product list / create / edit / delete

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _ticketModelFactory.PrepareProductSearchModelAsync(new TicketSearchModel());

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/ListTickets.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<JsonResult> TicketList(TicketSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return await AccessDeniedJsonAsync();

        //prepare model
        var model = await _ticketModelFactory.PrepareProductListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost, ActionName("List")]
    [FormValueRequired("go-to-product-by-sku")]
    public virtual async Task<IActionResult> GoToSku(TicketSearchModel searchModel)
    {
        //try to load a product entity, if not found, then try to load a product attribute combination
        var productId = (await _productService.GetProductBySkuAsync(searchModel.GoDirectlyToSku))?.Id
            ?? (await _productAttributeService.GetProductAttributeCombinationBySkuAsync(searchModel.GoDirectlyToSku))?.ProductId;

        if (productId != null)
            return RedirectToAction("Edit", "Ticket", new { id = productId });

        //not found
        return await List();
    }

    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> Create()
    {

        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //validate maximum number of products per vendor
        if (_vendorSettings.MaximumProductNumber > 0 && await _workContext.GetCurrentVendorAsync() != null
            && await _productService.GetNumberOfProductsByVendorIdAsync((await _workContext.GetCurrentVendorAsync()).Id) >= _vendorSettings.MaximumProductNumber)
        {
            _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                _vendorSettings.MaximumProductNumber));
            return RedirectToAction("List");
        }

        //prepare model
        var model = await _ticketModelFactory.PrepareTicketModelAsync(new TicketModel(), null);

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/CreateTicket.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Create(TicketModel model, bool continueEditing)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //validate maximum number of products per vendor
        if (_vendorSettings.MaximumProductNumber > 0 && await _workContext.GetCurrentVendorAsync() != null
            && await _productService.GetNumberOfProductsByVendorIdAsync((await _workContext.GetCurrentVendorAsync()).Id) >= _vendorSettings.MaximumProductNumber)
        {
            _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                _vendorSettings.MaximumProductNumber));
            return RedirectToAction("List");
        }

        if (ModelState.IsValid)
        {
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
                model.ProductModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //vendors cannot edit "Show on home page" property
            if (await _workContext.GetCurrentVendorAsync() != null && model.ProductModel.ShowOnHomepage)
                model.ProductModel.ShowOnHomepage = false;

            //product
            var product = model.ProductModel.ToEntity<Product>();

            if (product.DisplayOrder > SystemHelper.EventTicketDefaults.DisplayOrderDateOffset)
                product.DisplayOrder = _eventTicketService.GetDateOrder(model.ProductTypeExtension.EventTicketDateTime);

            product.CreatedOnUtc = DateTime.UtcNow;
            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.InsertProductAsync(product);

            await _eventTicketService.InsertEventTicketProductTypeExtensionAsync(product,
                model.ProductTypeExtension.EventTicketDateTime,
                model.ProductTypeExtension.EventTicketReportDateTime,
                model.ProductTypeExtension.ProductTime);

            //search engine name
            model.ProductModel.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.ProductModel.SeName, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, model.ProductModel.SeName, 0);

            //locales
            await UpdateLocalesAsync(product, model.ProductModel);

            //categories
            await SaveCategoryMappingsAsync(product, model.ProductModel);

            //manufacturers
            await SaveManufacturerMappingsAsync(product, model.ProductModel);

            //ACL (customer roles)
            await SaveProductAclAsync(product, model.ProductModel);

            //stores
            await _storeMappingService.SaveStoreMappingsAsync(product, model.ProductModel.SelectedStoreIds);

            //discounts
            await SaveDiscountMappingsAsync(product, model.ProductModel);

            //tags
            await _productTagService.UpdateProductTagsAsync(product, model.ProductModel.SelectedProductTags.ToArray());

            //warehouses
            await SaveProductWarehouseInventoryAsync(product, model.ProductModel);

            //affiliates
            await SaveAffiliateMappings(product, model);

            //quantity change history
            await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product.Name), product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = product.Id });
        }

        //prepare model
        model = await _ticketModelFactory.PrepareTicketModelAsync(model, null, true);

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EditTicket.cshtml", model);
    }

    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null || product.Deleted)
            return RedirectToAction("List");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
            return RedirectToAction("List");

        //prepare model
        var model = await _ticketModelFactory.PrepareTicketModelAsync(null, product);

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/EditTicket.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> Edit(TicketModel model, bool continueEditing)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        model.ProductModel.Id = model.Id;

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(model.ProductModel.Id);
        if (product == null || product.Deleted)
            return RedirectToAction("List");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
            return RedirectToAction("List");

        //check if the product quantity has been changed while we were editing the product
        //and if it has been changed then we show error notification
        //and redirect on the editing page without data saving
        if (product.StockQuantity != model.ProductModel.LastStockQuantity)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
            return RedirectToAction("Edit", new { id = product.Id });
        }

        if (ModelState.IsValid)
        {
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
                model.ProductModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
            //vendors cannot edit "Show on home page" property
            if (await _workContext.GetCurrentVendorAsync() != null && model.ProductModel.ShowOnHomepage != product.ShowOnHomepage)
                model.ProductModel.ShowOnHomepage = product.ShowOnHomepage;

            //some previously used values
            var prevTotalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);
            var prevDownloadId = product.DownloadId;
            var prevSampleDownloadId = product.SampleDownloadId;
            var previousStockQuantity = product.StockQuantity;
            var previousWarehouseId = product.WarehouseId;
            var previousProductType = product.ProductType;

            //product
            product = model.ProductModel.ToEntity(product);

            if (product.DisplayOrder > 900000000)
                product.DisplayOrder = _eventTicketService.GetDateOrder(model.ProductTypeExtension.EventTicketDateTime);

            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.UpdateProductAsync(product);

            var pte = await _eventTicketService.UpdateEventTicketProductTypeExtensionAsync(product,
                        model.ProductTypeExtension.EventTicketDateTime, model.ProductTypeExtension.EventTicketReportDateTime,
                        model.ProductTypeExtension.RecordStatusId,
                        model.ProductTypeExtension.ProductTime);

            //remove or update associated products
            var vendorId = (await _workContext.GetCurrentVendorAsync())?.Id ?? 0;
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true);

            if (previousProductType == ProductType.GroupedProduct && product.ProductType == ProductType.SimpleProduct)
            {
                foreach (var associatedProduct in associatedProducts)
                {
                    associatedProduct.ParentGroupedProductId = 0;
                    await _productService.UpdateProductAsync(associatedProduct);
                }
            }
            else
            {
                foreach (var associatedProduct in associatedProducts)
                {
                    pte = await _eventTicketService.UpdateEventTicketProductTypeExtensionAsync(associatedProduct,
                        model.ProductTypeExtension.EventTicketDateTime, model.ProductTypeExtension.EventTicketReportDateTime,
                        model.ProductTypeExtension.RecordStatusId,
                        model.ProductTypeExtension.ProductTime);

                    associatedProduct.VendorId = product.VendorId;

                    await _productService.UpdateProductAsync(associatedProduct);
                }
            }

            //search engine name
            model.ProductModel.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.ProductModel.SeName, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, model.ProductModel.SeName, 0);

            //locales
            await UpdateLocalesAsync(product, model.ProductModel);

            //tags
            await _productTagService.UpdateProductTagsAsync(product, model.ProductModel.SelectedProductTags.ToArray());

            //warehouses
            await SaveProductWarehouseInventoryAsync(product, model.ProductModel);

            //categories
            await SaveCategoryMappingsAsync(product, model.ProductModel);

            //manufacturers
            await SaveManufacturerMappingsAsync(product, model.ProductModel);

            //ACL (customer roles)
            await SaveProductAclAsync(product, model.ProductModel);

            //stores
            await _storeMappingService.SaveStoreMappingsAsync(product, model.ProductModel.SelectedStoreIds);

            //discounts
            await SaveDiscountMappingsAsync(product, model.ProductModel);

            //picture seo names
            await UpdatePictureSeoNamesAsync(product);

            //back in stock notifications
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                await _productService.GetTotalStockQuantityAsync(product) > 0 &&
                prevTotalStockQuantity <= 0 &&
                product.Published &&
                !product.Deleted)
            {
                await _backInStockSubscriptionService.SendNotificationsToSubscribersAsync(product);
            }

            //delete an old "download" file (if deleted or updated)
            if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
            {
                var prevDownload = await _downloadService.GetDownloadByIdAsync(prevDownloadId);
                if (prevDownload != null)
                    await _downloadService.DeleteDownloadAsync(prevDownload);
            }

            //delete an old "sample download" file (if deleted or updated)
            if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
            {
                var prevSampleDownload = await _downloadService.GetDownloadByIdAsync(prevSampleDownloadId);
                if (prevSampleDownload != null)
                    await _downloadService.DeleteDownloadAsync(prevSampleDownload);
            }

            //quantity change history
            if (previousWarehouseId != product.WarehouseId)
            {
                //warehouse is changed 
                //compose a message
                var oldWarehouseMessage = string.Empty;
                if (previousWarehouseId > 0)
                {
                    var oldWarehouse = await _warehouseService.GetWarehouseByIdAsync(previousWarehouseId);
                    if (oldWarehouse != null)
                        oldWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
                }

                var newWarehouseMessage = string.Empty;
                if (product.WarehouseId > 0)
                {
                    var newWarehouse = await _warehouseService.GetWarehouseByIdAsync(product.WarehouseId);
                    if (newWarehouse != null)
                        newWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
                }

                var message = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                //record history
                await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId, message);
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
            }
            else
            {
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                    product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
            }

            //affiliates
            await SaveAffiliateMappings(product, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = product.Id });
        }

        //prepare model
        model = await _ticketModelFactory.PrepareTicketModelAsync(model, product, true);
        if (model != null)
            return RedirectToAction("Edit", new { id = product.Id });


        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return RedirectToAction("List");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
            return RedirectToAction("List");

        await _productService.DeleteProductAsync(product);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteTicket",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProduct"), product.Name), product);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Deleted"));

        return RedirectToAction("List");
    }

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        if (selectedIds != null)
        {
            await _productService.DeleteProductsAsync(await (await _productService.GetProductsByIdsAsync(selectedIds.ToArray()))
                .WhereAwait(async p => await _workContext.GetCurrentVendorAsync() == null || p.VendorId == (await _workContext.GetCurrentVendorAsync()).Id).ToListAsync());
        }

        return Json(new { Result = true });
    }

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CopyTicket(TicketModel model)
    {
        Product newProduct = null;
        int originalProductId = 0;

        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        var copyModel = model.CopyTicketModel;

        originalProductId = copyModel.Id;

        string copyFormat = NopCatalogDefaults.ProductCopyNameTemplate;

        try
        {
            var originalProduct = await _eventTicketService.GetTicketByIdAsync(originalProductId);

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && originalProduct.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                return RedirectToAction("List");

            if (originalProduct.ProductTypeId == (int)ProductType.GroupedProduct)
            {
                if (!copyModel.UseTemplate)
                {
                    // Copy Product and Associated Products 

                        newProduct = await CopyTicketAsync(originalProduct, originalProduct, copyModel);

                        var associatedProducts = await _productService.GetAssociatedProductsAsync(originalProduct.Id, showHidden: true);
                        foreach (var associatedProduct in associatedProducts)
                        {
                            var templateProduct = await _eventTicketService.GetTicketByIdAsync(newProduct.Id);
                            await CopyTicketAsync(templateProduct, associatedProduct, copyModel);
                        }

                }
                else
                {
                    if (copyModel.SelectedTemplateId == 0)
                    {
                        // Copy Product but not Associated Products
                        newProduct = await CopyTicketAsync(originalProduct, originalProduct, copyModel);
                    }
                    else
                    {
                        // Copy associated Products using template values

                        var associatedTemplates = await _productService.GetProductByIdAsync(copyModel.SelectedTemplateId);
                        if (associatedTemplates != null)
                        {
                            var associatedProducts = await _productService.GetAssociatedProductsAsync(associatedTemplates.Id, showHidden: true);
                            foreach (var associatedProduct in associatedProducts)
                            {
                                var templateProduct = await _eventTicketService.GetTicketByIdAsync(originalProduct.Id);
                                await CopyTicketAsync(templateProduct, associatedProduct, copyModel);
                            }
                        }

                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copied"));

                        return RedirectToAction("Edit", new { id = originalProduct.Id });
                    }
                }
            }
            else if (originalProduct.ProductTypeId == (int)ProductType.SimpleProduct)
            {
                if (!copyModel.UseTemplate || copyModel.SelectedTemplateId == 0)
                {
                    // Copy Product

                    newProduct = await CopyTicketAsync(originalProduct, originalProduct, copyModel);
                }
                else
                {
                    // Copy Product using template values

                    var templateProduct = await _productService.GetProductByIdAsync(copyModel.SelectedTemplateId);
                    if (templateProduct != null)
                    {
                        newProduct = await CopyTicketAsync(originalProduct, templateProduct, copyModel);
                    }
                }
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copied"));

            return RedirectToAction("Edit", new { id = newProduct.Id });

        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("Edit", new { id = copyModel.Id });
        }
    }

    public async Task<Product> CopyTicketAsync(Product templateProduct, Product originalProduct, CopyTicketModel copyModel)
    {
        var newProduct = await _copyProductService.CopyProductAsync(originalProduct, copyModel.Name, copyModel.Published, copyModel.CopyImages, false);

        var pte = await _eventTicketService.GetEventTicketProductTypeExtensionAsync(templateProduct.Id);
        if (pte != null)
        {
            await _eventTicketService.InsertEventTicketProductTypeExtensionAsync(newProduct,
                pte.AvailabilityStartDate, pte.AvailabilityEndDate, pte.ProductTime);
        }

        newProduct.DisplayOrder = _eventTicketService.GetDateOrder(pte.AvailabilityStartDate);

        if (newProduct.Name.Contains("Copy of "))
            newProduct.Name = newProduct.Name.Replace("Copy of ", "");

        if (newProduct.Sku.Contains("-copy"))
            newProduct.Sku = newProduct.Sku.Replace("-copy", "_" + newProduct.Id.ToString());
        else if (newProduct.Sku != originalProduct.Sku)
            newProduct.Sku += "_" + originalProduct.Sku;
        else
            newProduct.Sku += "_" + newProduct.Id.ToString();

        newProduct.ParentGroupedProductId = 0;

        if (templateProduct.Id != originalProduct.Id)
        {

            newProduct.Sku = templateProduct.Sku += "_" + originalProduct.Sku;

            if (templateProduct.ProductType == ProductType.GroupedProduct)
            {
                newProduct.ParentGroupedProductId = templateProduct.Id;
                newProduct.DisplayOrder = originalProduct.DisplayOrder;
            }

            if (!copyModel.Name.Contains("Template - "))
                newProduct.Name = originalProduct.Name.Replace("Template - ", "");

            string urlSlug = await _urlRecordService.GetActiveSlugAsync(newProduct.Id, "Product", 0);
            var slug = await _urlRecordService.GetBySlugAsync(urlSlug);
            if (slug != null)
            {
                string newProductName = copyModel.Name + "-" + newProduct.Name;
                slug.Slug = await _urlRecordService.ValidateSeNameAsync(newProduct, string.Empty, newProductName, true);
                await UpdateUrlRecordAsync(slug);
            }

            if (!copyModel.CopyImages)
            {
                foreach (var productPicture in await _productService.GetProductPicturesByProductIdAsync(originalProduct.Id))
                {
                    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                    await _productService.InsertProductPictureAsync(new ProductPicture
                    {
                        ProductId = newProduct.Id,
                        PictureId = picture.Id,
                        DisplayOrder = productPicture.DisplayOrder
                    });
                }
            }
        }
        else
        {
            string urlSlug = await _urlRecordService.GetActiveSlugAsync(newProduct.Id, "Product", 0);
            var slug = await _urlRecordService.GetBySlugAsync(urlSlug);
            if (slug != null)
            {
                string newProductName = copyModel.Name;
                slug.Slug = await _urlRecordService.ValidateSeNameAsync(newProduct, string.Empty, newProductName, true);
                await UpdateUrlRecordAsync(slug);
            }
        }

        await _productService.UpdateProductAsync(newProduct);

        return newProduct;
    }

    /// <summary>
    /// Updates the URL record
    /// </summary>
    /// <param name="urlRecord">URL record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task UpdateUrlRecordAsync(UrlRecord urlRecord)
    {
        await _urlRecordRepository.UpdateAsync(urlRecord);
    }

    //action displaying notification (warning) to a store owner that entered SKU already exists
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> SkuReservedWarning(int productId, string sku)
    {
        string message;

        //check whether product with passed SKU already exists
        var productBySku = await _productService.GetProductBySkuAsync(sku);
        if (productBySku != null)
        {
            if (productBySku.Id == productId)
                return Json(new { Result = string.Empty });

            message = string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Sku.Reserved"), productBySku.Name);
            return Json(new { Result = message });
        }

        //check whether combination with passed SKU already exists
        var combinationBySku = await _productAttributeService.GetProductAttributeCombinationBySkuAsync(sku);
        if (combinationBySku == null)
            return Json(new { Result = string.Empty });

        message = string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.Sku.Reserved"),
            (await _productService.GetProductByIdAsync(combinationBySku.ProductId))?.Name);

        return Json(new { Result = message });
    }

    #endregion

    #region Required products

    [HttpPost]
    public virtual async Task<IActionResult> LoadProductFriendlyNames(string productIds)
    {
        var result = string.Empty;

        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return Json(new { Text = result });

        if (string.IsNullOrWhiteSpace(productIds))
            return Json(new { Text = result });

        var ids = new List<int>();
        var rangeArray = productIds
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();

        foreach (var str1 in rangeArray)
        {
            if (int.TryParse(str1, out var tmp1))
                ids.Add(tmp1);
        }

        var products = await _productService.GetProductsByIdsAsync(ids.ToArray());
        for (var i = 0; i <= products.Count - 1; i++)
        {
            result += products[i].Name;
            if (i != products.Count - 1)
                result += ", ";
        }

        return Json(new { Text = result });
    }

    public virtual async Task<IActionResult> RequiredProductAddPopup()
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareAddRequiredProductSearchModelAsync(new AddRequiredProductSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RequiredProductAddPopupList(AddRequiredProductSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareAddRequiredProductListModelAsync(searchModel);

        return Json(model);
    }

    #endregion

    #region Related products

    [HttpPost]
    public virtual async Task<IActionResult> RelatedProductList(RelatedProductSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        //prepare model
        var model = await _productModelFactory.PrepareRelatedProductListModelAsync(searchModel, product);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RelatedProductUpdate(RelatedProductModel model)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a related product with the specified id
        var relatedProduct = await _productService.GetRelatedProductByIdAsync(model.Id)
            ?? throw new ArgumentException("No related product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            var product = await _productService.GetProductByIdAsync(relatedProduct.ProductId1);
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        relatedProduct.DisplayOrder = model.DisplayOrder;
        await _productService.UpdateRelatedProductAsync(relatedProduct);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> RelatedProductDelete(int id)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a related product with the specified id
        var relatedProduct = await _productService.GetRelatedProductByIdAsync(id)
            ?? throw new ArgumentException("No related product found with the specified id");

        var productId = relatedProduct.ProductId1;

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        await _productService.DeleteRelatedProductAsync(relatedProduct);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> RelatedProductAddPopup(int productId)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareAddRelatedProductSearchModelAsync(new AddRelatedProductSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RelatedProductAddPopupList(AddRelatedProductSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareAddRelatedProductListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> RelatedProductAddPopup(AddRelatedProductModel model)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var existingRelatedProducts = await _productService.GetRelatedProductsByProductId1Async(model.ProductId);
            foreach (var product in selectedProducts)
            {
                //a vendor should have access only to his products
                var vendor = await _workContext.GetCurrentVendorAsync();
                if (vendor != null)
                {
                    if (product.VendorId != vendor.Id)
                        continue;
                }

                if (_productService.FindRelatedProduct(existingRelatedProducts, model.ProductId, product.Id) != null)
                    continue;

                await _productService.InsertRelatedProductAsync(new RelatedProduct
                {
                    ProductId1 = model.ProductId,
                    ProductId2 = product.Id,
                    DisplayOrder = 1
                });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddRelatedProductSearchModel());
    }

    #endregion

    #region Cross-sell products

    [HttpPost]
    public virtual async Task<IActionResult> CrossSellProductList(CrossSellProductSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        //prepare model
        var model = await _productModelFactory.PrepareCrossSellProductListModelAsync(searchModel, product);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CrossSellProductDelete(int id)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a cross-sell product with the specified id
        var crossSellProduct = await _productService.GetCrossSellProductByIdAsync(id)
            ?? throw new ArgumentException("No cross-sell product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            var product = await _productService.GetProductByIdAsync(crossSellProduct.ProductId1);
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        await _productService.DeleteCrossSellProductAsync(crossSellProduct);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> CrossSellProductAddPopup(int productId)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareAddCrossSellProductSearchModelAsync(new AddCrossSellProductSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CrossSellProductAddPopupList(AddCrossSellProductSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareAddCrossSellProductListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CrossSellProductAddPopup(AddCrossSellProductModel model)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var existingCrossSellProducts = await _productService.GetCrossSellProductsByProductId1Async(model.ProductId);
            foreach (var product in selectedProducts)
            {
                //a vendor should have access only to his products
                var vendor = await _workContext.GetCurrentVendorAsync();
                if (vendor != null)
                {
                    if (product.VendorId != vendor.Id)
                        continue;
                }

                if (_productService.FindCrossSellProduct(existingCrossSellProducts, model.ProductId, product.Id) != null)
                    continue;

                await _productService.InsertCrossSellProductAsync(new CrossSellProduct
                {
                    ProductId1 = model.ProductId,
                    ProductId2 = product.Id
                });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddCrossSellProductSearchModel());
    }

    #endregion

    #region Associated products

    [HttpPost]
    public virtual async Task<IActionResult> AssociatedProductList(AssociatedProductSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        //prepare model
        var model = await _productModelFactory.PrepareAssociatedProductListModelAsync(searchModel, product);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociatedProductUpdate(AssociatedProductModel model)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get an associated product with the specified id
        var associatedProduct = await _productService.GetProductByIdAsync(model.Id)
            ?? throw new ArgumentException("No associated product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            if (associatedProduct != null && associatedProduct.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        associatedProduct.DisplayOrder = model.DisplayOrder;
        await _productService.UpdateProductAsync(associatedProduct);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociatedProductDelete(int id)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get an associated product with the specified id
        var product = await _productService.GetProductByIdAsync(id)
            ?? throw new ArgumentException("No associated product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        product.ParentGroupedProductId = 0;
        await _productService.UpdateProductAsync(product);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> AssociatedTicketAddPopup(int productId)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareAddAssociatedProductSearchModelAsync(new AddAssociatedProductSearchModel());

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/AssociatedTicketAddPopup.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociatedProductAddPopupList(AddAssociatedProductSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareAddAssociatedProductListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> AssociatedProductAddPopup(AddAssociatedProductModel model)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());

        var tryToAddSelfGroupedProduct = selectedProducts
            .Select(p => p.Id)
            .Contains(model.ProductId);

        if (selectedProducts.Any())
        {
            foreach (var product in selectedProducts)
            {
                if (product.Id == model.ProductId)
                    continue;

                //a vendor should have access only to his products
                var vendor = await _workContext.GetCurrentVendorAsync();
                if (vendor != null)
                {
                    if (product.VendorId != vendor.Id)
                        continue;
                }

                product.ParentGroupedProductId = model.ProductId;
                await _productService.UpdateProductAsync(product);
            }
        }

        if (tryToAddSelfGroupedProduct)
        {
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.AssociatedProducts.TryToAddSelfGroupedProduct"));

            var addAssociatedProductSearchModel = await _productModelFactory.PrepareAddAssociatedProductSearchModelAsync(new AddAssociatedProductSearchModel());
            //set current product id
            addAssociatedProductSearchModel.ProductId = model.ProductId;

            ViewBag.RefreshPage = true;

            return View(addAssociatedProductSearchModel);
        }

        ViewBag.RefreshPage = true;

        ViewBag.ClosePage = true;

        return View(new AddAssociatedProductSearchModel());
    }

    #endregion

    #region Product pictures

    public virtual async Task<IActionResult> ProductPictureAdd(int pictureId, int displayOrder,
        string overrideAltAttribute, string overrideTitleAttribute, int productId)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        if (pictureId == 0)
            throw new ArgumentException();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            if (product.VendorId != vendor.Id)
                return RedirectToAction("List");
        }

        if ((await _productService.GetProductPicturesByProductIdAsync(productId)).Any(p => p.PictureId == pictureId))
            return Json(new { Result = false });

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(pictureId)
            ?? throw new ArgumentException("No picture found with the specified id");

        await _pictureService.UpdatePictureAsync(picture.Id,
            await _pictureService.LoadPictureBinaryAsync(picture),
            picture.MimeType,
            picture.SeoFilename,
            overrideAltAttribute,
            overrideTitleAttribute);

        await _pictureService.SetSeoFilenameAsync(pictureId, await _pictureService.GetPictureSeNameAsync(product.Name));

        await _productService.InsertProductPictureAsync(new ProductPicture
        {
            PictureId = pictureId,
            ProductId = productId,
            DisplayOrder = displayOrder
        });

        return Json(new { Result = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductPictureList(ProductPictureSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }
        

        //prepare model
        var model = await _productModelFactory.PrepareProductPictureListModelAsync(searchModel, product);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductPictureUpdate(ProductPictureModel model)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a product picture with the specified id
        var productPicture = await _productService.GetProductPictureByIdAsync(model.Id)
            ?? throw new ArgumentException("No product picture found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId)
            ?? throw new ArgumentException("No picture found with the specified id");

        await _pictureService.UpdatePictureAsync(picture.Id,
            await _pictureService.LoadPictureBinaryAsync(picture),
            picture.MimeType,
            picture.SeoFilename,
            model.OverrideAltAttribute,
            model.OverrideTitleAttribute);

        productPicture.DisplayOrder = model.DisplayOrder;
        await _productService.UpdateProductPictureAsync(productPicture);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductPictureDelete(int id)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a product picture with the specified id
        var productPicture = await _productService.GetProductPictureByIdAsync(id)
            ?? throw new ArgumentException("No product picture found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        var pictureId = productPicture.PictureId;
        await _productService.DeleteProductPictureAsync(productPicture);

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(pictureId)
            ?? throw new ArgumentException("No picture found with the specified id");

        await _pictureService.DeletePictureAsync(picture);

        return new NullJsonResult();
    }

    #endregion

    #region Product tags

    public virtual async Task<IActionResult> ProductTags()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareProductTagSearchModelAsync(new ProductTagSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductTags(ProductTagSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareProductTagListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductTagDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE))
            return AccessDeniedView();

        //try to get a product tag with the specified id
        var tag = await _productTagService.GetProductTagByIdAsync(id)
            ?? throw new ArgumentException("No product tag found with the specified id");

        await _productTagService.DeleteProductTagAsync(tag);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.ProductTags.Deleted"));

        return RedirectToAction("ProductTags");
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductTagsDelete(ICollection<int> selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE))
            return AccessDeniedView();

        if (selectedIds != null)
        {
            var tags = await _productTagService.GetProductTagsByIdsAsync(selectedIds.ToArray());
            await _productTagService.DeleteProductTagsAsync(tags);
        }

        return Json(new { Result = true });
    }

    public virtual async Task<IActionResult> EditProductTag(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE))
            return AccessDeniedView();

        //try to get a product tag with the specified id
        var productTag = await _productTagService.GetProductTagByIdAsync(id);
        if (productTag == null)
            return RedirectToAction("List");

        //prepare tag model
        var model = await _productModelFactory.PrepareProductTagModelAsync(null, productTag);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> EditProductTag(ProductTagModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE))
            return AccessDeniedView();

        //try to get a product tag with the specified id
        var productTag = await _productTagService.GetProductTagByIdAsync(model.Id);
        if (productTag == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            productTag.Name = model.Name;
            await _productTagService.UpdateProductTagAsync(productTag);

            //locales
            await UpdateLocalesAsync(productTag, model);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.ProductTags.Updated"));

            return continueEditing ? RedirectToAction("EditProductTag", new { id = productTag.Id }) : RedirectToAction("ProductTags");
        }

        //prepare model
        model = await _productModelFactory.PrepareProductTagModelAsync(model, productTag, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    #endregion

    #region Purchased with order

    [HttpPost]
    public virtual async Task<IActionResult> PurchasedWithOrders(ProductOrderSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        //prepare model
        var model = await _productModelFactory.PrepareProductOrderListModelAsync(searchModel, product);

        return Json(model);
    }

    #endregion

    #region Export / Import

    [HttpPost, ActionName("DownloadCatalogPDF")]
    [FormValueRequired("download-catalog-pdf")]
    public virtual async Task<IActionResult> DownloadCatalogAsPdf(TicketSearchModel model)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
        {
            model.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;
        }

        var categoryIds = new List<int> { model.SearchCategoryId };
        //include subcategories
        if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

        //0 - all (according to "ShowHidden" parameter)
        //1 - published only
        //2 - unpublished only
        bool? overridePublished = null;
        if (model.SearchPublishedId == 1)
            overridePublished = true;
        else if (model.SearchPublishedId == 2)
            overridePublished = false;

        var products = await _eventTicketService.SearchTicketsAsync(
            categoryIds: categoryIds,
            manufacturerId: model.SearchManufacturerId,
            storeId: model.SearchStoreId,
            vendorId: model.SearchVendorId,
            warehouseId: model.SearchWarehouseId,
            productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
            keywords: model.SearchProductName,
            showHidden: false,
            overridePublished: overridePublished);

        try
        {
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _pdfService.PrintProductsToPdfAsync(stream, products);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, "pdfcatalog.pdf");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("List");
        }
    }


    [HttpPost, ActionName("ExportToExcel")]
    [FormValueRequired("exportexcel-all")]
    public virtual async Task<IActionResult> ExportExcelAll(TicketSearchModel model)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
        {
            model.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;
        }

        var categoryIds = new List<int> { model.SearchCategoryId };
        //include subcategories
        if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

        //0 - all (according to "ShowHidden" parameter)
        //1 - published only
        //2 - unpublished only
        bool? overridePublished = null;
        if (model.SearchPublishedId == 1)
            overridePublished = true;
        else if (model.SearchPublishedId == 2)
            overridePublished = false;

        var products = await _eventTicketService.SearchTicketsAsync(
            categoryIds: categoryIds,
            manufacturerId: model.SearchManufacturerId,
            storeId: model.SearchStoreId,
            vendorId: model.SearchVendorId,
            warehouseId: model.SearchWarehouseId,
            productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
            keywords: model.SearchProductName,
            showHidden: false,
            overridePublished: overridePublished);

        try
        {
            var bytes = await _exportManager.ExportProductsToXlsxAsync(products);
            return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("List");
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        var products = new List<Product>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            products.AddRange(await _productService.GetProductsByIdsAsync(ids));
        }
        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            products = products.Where(p => p.VendorId == vendor.Id).ToList();
        }

        try
        {
            var bytes = await _exportManager.ExportProductsToXlsxAsync(products);
            return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("List");
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
            //a vendor can not import products
            return AccessDeniedView();

        try
        {
            if (importexcelfile != null && importexcelfile.Length > 0)
            {
                await _importManager.ImportProductsFromXlsxAsync(importexcelfile.OpenReadStream());
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                return RedirectToAction("List");
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Imported"));
            return RedirectToAction("List");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("List");
        }
    }

    #endregion

    #region Product editor settings

    [HttpPost]
    public virtual async Task<IActionResult> SaveProductEditorSettings(ProductModel model, string returnUrl = "")
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //vendors cannot manage these settings
        if (await _workContext.GetCurrentVendorAsync() != null)
            return RedirectToAction("List");

        var productEditorSettings = await _settingService.LoadSettingAsync<ProductEditorSettings>();
        productEditorSettings = model.ProductEditorSettingsModel.ToSettings(productEditorSettings);
        await _settingService.SaveSettingAsync(productEditorSettings);

        //product list
        if (string.IsNullOrEmpty(returnUrl))
            return RedirectToAction("List");

        //prevent open redirection attack
        if (!Url.IsLocalUrl(returnUrl))
            return RedirectToAction("List");

        return Redirect(returnUrl);
    }

    #endregion

    #region Stock quantity history

    [HttpPost]
    public virtual async Task<IActionResult> StockQuantityHistory(StockQuantityHistorySearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            if (product != null && product.VendorId != vendor.Id)
                return Content("This is not your product");
        }

        //prepare model
        var model = await _productModelFactory.PrepareStockQuantityHistoryListModelAsync(searchModel, product);

        return Json(model);
    }

    #endregion

    #endregion
}