using System.ComponentModel;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

using Nop.Web.Areas.Admin.Factories;

using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.Core.Services;
using Nop.Plugin.Apollo.Core.Models;
using Nop.Plugin.Apollo.EventTickets.Models;
using Nop.Plugin.Apollo.EventTickets.Services;

namespace Nop.Plugin.Apollo.EventTickets.Factories;

/// <summary>
/// Represents the product model factory implementation
/// </summary>
public partial class TicketModelFactory : ITicketModelFactory
{

    #region Fields

    protected readonly CatalogSettings _catalogSettings;
    protected readonly CurrencySettings _currencySettings;
    protected readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    protected readonly IAddressService _addressService;
    protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
    protected readonly ICategoryService _categoryService;
    protected readonly ICurrencyService _currencyService;
    protected readonly ICustomerService _customerService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IDiscountService _discountService;
    protected readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILocalizedModelFactory _localizedModelFactory;
    protected readonly IManufacturerService _manufacturerService;
    protected readonly IMeasureService _measureService;
    protected readonly IOrderService _orderService;
    protected readonly IPictureService _pictureService;
    protected readonly IProductAttributeFormatter _productAttributeFormatter;
    protected readonly IProductAttributeParser _productAttributeParser;
    protected readonly IProductAttributeService _productAttributeService;
    protected readonly IProductService _productService;
    protected readonly IProductTagService _productTagService;
    protected readonly IProductTemplateService _productTemplateService;
    protected readonly ISettingModelFactory _settingModelFactory;
    protected readonly IShipmentService _shipmentService;
    protected readonly IShippingService _shippingService;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly ISpecificationAttributeService _specificationAttributeService;
    protected readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    protected readonly IStoreService _storeService;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly IWarehouseService _warehouseService;
    protected readonly IWorkContext _workContext;
    protected readonly MeasureSettings _measureSettings;
    protected readonly TaxSettings _taxSettings;
    protected readonly VendorSettings _vendorSettings;
    protected readonly IEventTicketService _eventTicketService;
    protected readonly IApolloCoreService _apolloCoreService;

    #endregion

    #region Ctor

    public TicketModelFactory(CatalogSettings catalogSettings,
        CurrencySettings currencySettings,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IAddressService addressService,
        IBaseAdminModelFactory baseAdminModelFactory,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IDiscountService discountService,
        IDiscountSupportedModelFactory discountSupportedModelFactory,
        ILocalizationService localizationService,
        ILocalizedModelFactory localizedModelFactory,
        IManufacturerService manufacturerService,
        IMeasureService measureService,
        IOrderService orderService,
        IPictureService pictureService,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IProductTagService productTagService,
        IProductTemplateService productTemplateService,
        ISettingModelFactory settingModelFactory,
        IShipmentService shipmentService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        ISpecificationAttributeService specificationAttributeService,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        IStoreService storeService,
        IUrlRecordService urlRecordService,
        IWarehouseService warehouseService,
        IWorkContext workContext,
        MeasureSettings measureSettings,
        TaxSettings taxSettings,
        VendorSettings vendorSettings,
        IEventTicketService eventTicketService,
        IApolloCoreService apolloCoreService)
    {
        _catalogSettings = catalogSettings;
        _currencySettings = currencySettings;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _addressService = addressService;
        _baseAdminModelFactory = baseAdminModelFactory;
        _categoryService = categoryService;
        _currencyService = currencyService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _discountService = discountService;
        _discountSupportedModelFactory = discountSupportedModelFactory;
        _localizationService = localizationService;
        _localizedModelFactory = localizedModelFactory;
        _manufacturerService = manufacturerService;
        _measureService = measureService;
        _measureSettings = measureSettings;
        _orderService = orderService;
        _pictureService = pictureService;
        _productAttributeFormatter = productAttributeFormatter;
        _productAttributeParser = productAttributeParser;
        _productAttributeService = productAttributeService;
        _productService = productService;
        _productTagService = productTagService;
        _productTemplateService = productTemplateService;
        _settingModelFactory = settingModelFactory;
        _shipmentService = shipmentService;
        _shippingService = shippingService;
        _shoppingCartService = shoppingCartService;
        _specificationAttributeService = specificationAttributeService;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _storeService = storeService;
        _urlRecordService = urlRecordService;
        _warehouseService = warehouseService;
        _workContext = workContext;
        _taxSettings = taxSettings;
        _vendorSettings = vendorSettings;
        _eventTicketService = eventTicketService;
        _apolloCoreService = apolloCoreService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepare copy product model
    /// </summary>
    /// <param name="model">Copy product model</param>
    /// <param name="product">Product</param>
    /// <returns>Copy product model</returns>
    protected virtual async Task<CopyProductModel> PrepareCopyProductModelAsync(CopyProductModel model, Product product)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        model.Id = product.Id;
        model.Name = string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copy.Name.New"), product.Name);
        model.Published = true;

        return model;
    }

    /// <summary>
    /// Prepare product warehouse inventory models
    /// </summary>
    /// <param name="models">List of product warehouse inventory models</param>
    /// <param name="product">Product</param>
    protected virtual async Task PrepareProductWarehouseInventoryModelsAsync(IList<ProductWarehouseInventoryModel> models, Product product)
    {
        if (models == null)
            throw new ArgumentNullException(nameof(models));

        foreach (var warehouse in await _warehouseService.GetAllWarehousesAsync())
        {
            var model = new ProductWarehouseInventoryModel
            {
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name
            };

            if (product != null)
            {
                var productWarehouseInventory = (await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id))?
                    .FirstOrDefault(inventory => inventory.WarehouseId == warehouse.Id);
                if (productWarehouseInventory != null)
                {
                    model.WarehouseUsed = true;
                    model.StockQuantity = productWarehouseInventory.StockQuantity;
                    model.ReservedQuantity = productWarehouseInventory.ReservedQuantity;
                    model.PlannedQuantity = await _shipmentService.GetQuantityInShipmentsAsync(product, productWarehouseInventory.WarehouseId, true, true);
                }
            }

            models.Add(model);
        }
    }

    /// <summary>
    /// Prepare product attribute mapping validation rules string
    /// </summary>
    /// <param name="attributeMapping">Product attribute mapping</param>
    /// <returns>Validation rules string</returns>
    protected virtual async Task<string> PrepareProductAttributeMappingValidationRulesString(ProductAttributeMapping attributeMapping)
    {
        if (!attributeMapping.ValidationRulesAllowed())
            return string.Empty;

        var validationRules = new StringBuilder(string.Empty);
        if (attributeMapping.ValidationMinLength.HasValue)
        {
            validationRules.AppendFormat("{0}: {1}<br />",
                await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MinLength"),
                attributeMapping.ValidationMinLength);
        }

        if (attributeMapping.ValidationMaxLength.HasValue)
        {
            validationRules.AppendFormat("{0}: {1}<br />",
                await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MaxLength"),
                attributeMapping.ValidationMaxLength);
        }

        if (!string.IsNullOrEmpty(attributeMapping.ValidationFileAllowedExtensions))
        {
            validationRules.AppendFormat("{0}: {1}<br />",
                await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileAllowedExtensions"),
                WebUtility.HtmlEncode(attributeMapping.ValidationFileAllowedExtensions));
        }

        if (attributeMapping.ValidationFileMaximumSize.HasValue)
        {
            validationRules.AppendFormat("{0}: {1}<br />",
                await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileMaximumSize"),
                attributeMapping.ValidationFileMaximumSize);
        }

        if (!string.IsNullOrEmpty(attributeMapping.DefaultValue))
        {
            validationRules.AppendFormat("{0}: {1}<br />",
                await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.DefaultValue"),
                WebUtility.HtmlEncode(attributeMapping.DefaultValue));
        }

        return validationRules.ToString();
    }

    /// <summary>
    /// Prepare product attribute condition model
    /// </summary>
    /// <param name="model">Product attribute condition model</param>
    /// <param name="productAttributeMapping">Product attribute mapping</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrepareProductAttributeConditionModelAsync(ProductAttributeConditionModel model,
        ProductAttributeMapping productAttributeMapping)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (productAttributeMapping == null)
            throw new ArgumentNullException(nameof(productAttributeMapping));

        model.ProductAttributeMappingId = productAttributeMapping.Id;
        model.EnableCondition = !string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml);

        //pre-select attribute and values
        var selectedPva = (await _productAttributeParser
            .ParseProductAttributeMappingsAsync(productAttributeMapping.ConditionAttributeXml))
            .FirstOrDefault();

        var attributes = (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productAttributeMapping.ProductId))
            //ignore non-combinable attributes (should have selectable values)
            .Where(x => x.CanBeUsedAsCondition())
            //ignore this attribute (it cannot depend on itself)
            .Where(x => x.Id != productAttributeMapping.Id)
            .ToList();
        foreach (var attribute in attributes)
        {
            var attributeModel = new ProductAttributeConditionModel.ProductAttributeModel
            {
                Id = attribute.Id,
                ProductAttributeId = attribute.ProductAttributeId,
                Name = (await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId)).Name,
                TextPrompt = attribute.TextPrompt,
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType
            };

            if (attribute.ShouldHaveValues())
            {
                //values
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                foreach (var attributeValue in attributeValues)
                {
                    var attributeValueModel = new ProductAttributeConditionModel.ProductAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = attributeValue.Name,
                        IsPreSelected = attributeValue.IsPreSelected
                    };
                    attributeModel.Values.Add(attributeValueModel);
                }

                //pre-select attribute and value
                if (selectedPva != null && attribute.Id == selectedPva.Id)
                {
                    //attribute
                    model.SelectedProductAttributeId = selectedPva.Id;

                    //values
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            if (!string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues =
                                    await _productAttributeParser.ParseProductAttributeValuesAsync(productAttributeMapping
                                        .ConditionAttributeXml);
                                foreach (var attributeValue in selectedValues)
                                    foreach (var item in attributeModel.Values)
                                        if (attributeValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are supported as conditions
                            break;
                    }
                }
            }

            model.ProductAttributes.Add(attributeModel);
        }
    }

    /// <summary>
    /// Prepare related product search model
    /// </summary>
    /// <param name="searchModel">Related product search model</param>
    /// <param name="product">Product</param>
    /// <returns>Related product search model</returns>
    protected virtual RelatedProductSearchModel PrepareRelatedProductSearchModel(RelatedProductSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare cross-sell product search model
    /// </summary>
    /// <param name="searchModel">Cross-sell product search model</param>
    /// <param name="product">Product</param>
    /// <returns>Cross-sell product search model</returns>
    protected virtual CrossSellProductSearchModel PrepareCrossSellProductSearchModel(CrossSellProductSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare associated product search model
    /// </summary>
    /// <param name="searchModel">Associated product search model</param>
    /// <param name="product">Product</param>
    /// <returns>Associated product search model</returns>
    protected virtual AssociatedProductSearchModel PrepareAssociatedProductSearchModel(AssociatedProductSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare product picture search model
    /// </summary>
    /// <param name="searchModel">Product picture search model</param>
    /// <param name="product">Product</param>
    /// <returns>Product picture search model</returns>
    protected virtual ProductPictureSearchModel PrepareProductPictureSearchModel(ProductPictureSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare product order search model
    /// </summary>
    /// <param name="searchModel">Product order search model</param>
    /// <param name="product">Product</param>
    /// <returns>Product order search model</returns>
    protected virtual ProductOrderSearchModel PrepareProductOrderSearchModel(ProductOrderSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare tier price search model
    /// </summary>
    /// <param name="searchModel">Tier price search model</param>
    /// <param name="product">Product</param>
    /// <returns>Tier price search model</returns>
    protected virtual TierPriceSearchModel PrepareTierPriceSearchModel(TierPriceSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare stock quantity history search model
    /// </summary>
    /// <param name="searchModel">Stock quantity history search model</param>
    /// <param name="product">Product</param>
    /// <returns>Stock quantity history search model</returns>
    protected virtual async Task<StockQuantityHistorySearchModel> PrepareStockQuantityHistorySearchModelAsync(StockQuantityHistorySearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare available warehouses
        await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare product attribute mapping search model
    /// </summary>
    /// <param name="searchModel">Product attribute mapping search model</param>
    /// <param name="product">Product</param>
    /// <returns>Product attribute mapping search model</returns>
    protected virtual ProductAttributeMappingSearchModel PrepareProductAttributeMappingSearchModel(ProductAttributeMappingSearchModel searchModel,
        Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare product attribute value search model
    /// </summary>
    /// <param name="searchModel">Product attribute value search model</param>
    /// <param name="productAttributeMapping">Product attribute mapping</param>
    /// <returns>Product attribute value search model</returns>
    protected virtual ProductAttributeValueSearchModel PrepareProductAttributeValueSearchModel(ProductAttributeValueSearchModel searchModel,
        ProductAttributeMapping productAttributeMapping)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (productAttributeMapping == null)
            throw new ArgumentNullException(nameof(productAttributeMapping));

        searchModel.ProductAttributeMappingId = productAttributeMapping.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare product attribute combination search model
    /// </summary>
    /// <param name="searchModel">Product attribute combination search model</param>
    /// <param name="product">Product</param>
    /// <returns>Product attribute combination search model</returns>
    protected virtual ProductAttributeCombinationSearchModel PrepareProductAttributeCombinationSearchModel(
        ProductAttributeCombinationSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare product specification attribute search model
    /// </summary>
    /// <param name="searchModel">Product specification attribute search model</param>
    /// <param name="product">Product</param>
    /// <returns>Product specification attribute search model</returns>
    protected virtual ProductSpecificationAttributeSearchModel PrepareProductSpecificationAttributeSearchModel(
        ProductSpecificationAttributeSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        searchModel.ProductId = product.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare product search model
    /// </summary>
    /// <param name="searchModel">Product search model</param>
    /// <returns>Product search model</returns>
    public virtual async Task<TicketSearchModel> PrepareProductSearchModelAsync(TicketSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //a vendor should have access only to his products
        searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
        searchModel.AllowVendorsToImportProducts = _vendorSettings.AllowVendorsToImportProducts;

        //prepare available categories
        await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

        //prepare available manufacturers
        await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

        //prepare available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        //prepare available vendors
        await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

        //prepare available product types
        await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

        //prepare available warehouses
        await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

        searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

        //prepare "published" filter (0 - published only; 1 - unpublished only; 2 - all)
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.PublishedOnly")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.All")
        });

        //prepare grid
        searchModel.SetGridPageSize();

        searchModel.SearchTemplates = false;

        searchModel.SearchAssociated = false;

        return searchModel;
    }

    /// <summary>
    /// Prepare paged product list model
    /// </summary>
    /// <param name="searchModel">Product search model</param>
    /// <returns>Product list model</returns>
    public virtual async Task<ProductListModel> PrepareProductListModelAsync(TicketSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter comments
        var overridePublished = searchModel.SearchPublishedId == 2 ? null : (bool?)(searchModel.SearchPublishedId == 0);
        if (await _workContext.GetCurrentVendorAsync() != null)
            searchModel.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;
        var categoryIds = new List<int> { searchModel.SearchCategoryId };
        if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
        {
            var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
            categoryIds.AddRange(childCategoryIds);
        }

        if (searchModel.SearchTemplates)
        {
            searchModel.SearchProductName = "Template - ";
            overridePublished = false;
        }

        //get products
        var products = await _eventTicketService.SearchTicketsAsync(showHidden: true,
            categoryIds: categoryIds,
            manufacturerId: searchModel.SearchManufacturerId,
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            warehouseId: searchModel.SearchWarehouseId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
            overridePublished: overridePublished,
            searchAssociated: searchModel.SearchAssociated);


        //prepare list model
        var model = await new ProductListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                //fill in model values from the entity
                var productModel = product.ToModel<ProductModel>();

                //little performance optimization: ensure that "FullDescription" is not returned
                productModel.FullDescription = string.Empty;

                //fill in additional values (not existing in the entity)
                productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                var defaultProductPictureId = 0;
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                if (picture != null)
                {
                    defaultProductPictureId = picture.Id;
                    productModel.PictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(defaultProductPictureId, 75);
                }

                productModel.ProductTypeName = (await _localizationService.GetLocalizedEnumAsync(product.ProductType)).Replace("(product with variants)", "");
                if (product.ProductType == ProductType.SimpleProduct && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    productModel.StockQuantityStr = (await _productService.GetTotalStockQuantityAsync(product)).ToString();

                string productName = product.Name;
                if (product.ParentGroupedProductId != 0)
                {
                    var parentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                    if (parentProduct != null)
                    {
                        productModel.Name = parentProduct.Name + " - " + productName;
                    }
                }

                return productModel;

            });
        });

        return model;
    }

    /// <summary>
    /// Prepare product model
    /// </summary>
    /// <param name="model">Product model</param>
    /// <param name="product">Product</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>Product model</returns>
    public virtual async Task<TicketModel> PrepareTicketModelAsync(TicketModel model, Product product, bool excludeProperties = false)
    {
        //fill in model values from the entity
        if (model == null)
        {
            model = new TicketModel();
        }

        if (product != null)
        {
            model.ProductModel = await PrepareProductModelAsync(null, product, excludeProperties);

            model.ProductTypeExtension = await PrepareEventTicketProductTypeExtensionModelAsync(null, product);

            model.CopyTicketModel.Id = model.ProductModel.Id;
            model.CopyTicketModel.Name = model.ProductModel.CopyProductModel.Name;
            model.CopyTicketModel.Published = model.ProductModel.CopyProductModel.Published;
            model.CopyTicketModel.CopyImages = false;
            model.CopyTicketModel.UseTemplate = true;
            
            var templateProducts = _eventTicketService.GetAllTemplateProducts(product.ProductTypeId, CoreSystemHelper.SystemMode.EventTickets, CoreSystemHelper.RecordType.Configuration, model.ProductModel.VendorId);
            model.CopyTicketModel.AvailableTemplates.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.SelectTemplate") });
            foreach (var tp in templateProducts)
                model.CopyTicketModel.AvailableTemplates.Add(new SelectListItem { Value = tp.Id.ToString(), Text = tp.Name });

            model.AvailableAffiliates = await _eventTicketService.GetAllAffiliatesListAsync();
            model.SelectedAffiliateIds = await _eventTicketService.GetAffiliatesForTicketAsync(product.Id);

        }
        else
        {
            model.ProductModel = await PrepareProductModelAsync(new ProductModel(), null);
        }

       return model;
    }

    /// <summary>
    /// Prepare product model
    /// </summary>
    /// <param name="model">Product model</param>
    /// <param name="product">Product</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>Product model</returns>
    public virtual async Task<ProductModel> PrepareProductModelAsync(ProductModel model, Product product, bool excludeProperties = false)
    {
        Func<ProductLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (product != null)
        {
            //fill in model values from the entity
            if (model == null)
            {
                model = product.ToModel<ProductModel>();
                model.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
            }

            var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
            if (parentGroupedProduct != null)
            {
                model.AssociatedToProductId = product.ParentGroupedProductId;
                model.AssociatedToProductName = parentGroupedProduct.Name;
            }

            model.LastStockQuantity = product.StockQuantity;

            model.SelectedProductTags = (await _productTagService.GetAllProductTagsByProductIdAsync(product.Id)).Select(tag => tag.Name).ToList();
            model.AvailableProductTags = (await _productTagService.GetAllProductTagsAsync())
                .Select(pt => new SelectListItem { Text = pt.Name, Value = pt.Name }).ToList();

            model.ProductAttributesExist = (await _productAttributeService.GetAllProductAttributesAsync()).Any();

            model.CanCreateCombinations = false;

            if (!excludeProperties)
            {
                model.SelectedCategoryIds = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true))
                    .Select(productCategory => productCategory.CategoryId).ToList();
                model.SelectedManufacturerIds = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true))
                    .Select(productManufacturer => productManufacturer.ManufacturerId).ToList();
            }

            //prepare copy product model
            await PrepareCopyProductModelAsync(model.CopyProductModel, product);

            //prepare nested search model
            PrepareRelatedProductSearchModel(model.RelatedProductSearchModel, product);
            PrepareCrossSellProductSearchModel(model.CrossSellProductSearchModel, product);
            PrepareAssociatedProductSearchModel(model.AssociatedProductSearchModel, product);
            PrepareProductPictureSearchModel(model.ProductPictureSearchModel, product);
            PrepareProductSpecificationAttributeSearchModel(model.ProductSpecificationAttributeSearchModel, product);
            PrepareProductOrderSearchModel(model.ProductOrderSearchModel, product);
            PrepareTierPriceSearchModel(model.TierPriceSearchModel, product);
            await PrepareStockQuantityHistorySearchModelAsync(model.StockQuantityHistorySearchModel, product);
            PrepareProductAttributeMappingSearchModel(model.ProductAttributeMappingSearchModel, product);
            PrepareProductAttributeCombinationSearchModel(model.ProductAttributeCombinationSearchModel, product);

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Name = await _localizationService.GetLocalizedAsync(product, entity => entity.Name, languageId, false, false);
                locale.FullDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.FullDescription, languageId, false, false);
                locale.ShortDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.ShortDescription, languageId, false, false);
                locale.MetaKeywords = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaKeywords, languageId, false, false);
                locale.MetaDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaDescription, languageId, false, false);
                locale.MetaTitle = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaTitle, languageId, false, false);
                locale.SeName = await _urlRecordService.GetSeNameAsync(product, languageId, false, false);
            };
        }

        //set default values for the new model
        if (product == null)
        {
            model.MaximumCustomerEnteredPrice = 0;
            model.MaxNumberOfDownloads = 0;
            model.RecurringCycleLength = 0;
            model.RecurringTotalCycles = 0;
            model.RentalPriceLength = 1;
            model.StockQuantity = 100;
            model.NotifyAdminForQuantityBelow = 5;
            model.OrderMinimumQuantity = 1;
            model.OrderMaximumQuantity = 10;
            model.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;
            model.UnlimitedDownloads = true;
            model.IsShipEnabled = true;
            model.AllowCustomerReviews = true;
            model.Published = true;
            model.VisibleIndividually = true;

            var productTemplateSimpleProduct = (await _productTemplateService.GetAllProductTemplatesAsync()).Where(pt => pt.Name == "Event Tickets").First();
            if (productTemplateSimpleProduct == null)
                throw new Exception("Event Tickets template could not be loaded");
            else
                model.ProductTemplateId = productTemplateSimpleProduct.Id;

        }

        model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
        model.BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name;
        model.BaseDimensionIn = (await _measureService.GetMeasureDimensionByIdAsync(_measureSettings.BaseDimensionId)).Name;
        model.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
        model.HasAvailableSpecificationAttributes =
            (await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync()).Any();

        var currentVendor = await _workContext.GetCurrentVendorAsync();
        model.IsLoggedInAsVendor = currentVendor != null;

        //prepare localized models
        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        //prepare editor settings
        model.ProductEditorSettingsModel = await _settingModelFactory.PrepareProductEditorSettingsModelAsync();

        //prepare available product templates
        await _baseAdminModelFactory.PrepareProductTemplatesAsync(model.AvailableProductTemplates, false);

        //prepare available product types
        var productTemplates = await _productTemplateService.GetAllProductTemplatesAsync();
        foreach (var productType in Enum.GetValues(typeof(ProductType)).OfType<ProductType>())
        {
            model.ProductsTypesSupportedByProductTemplates.Add((int)productType, new List<SelectListItem>());
            foreach (var template in productTemplates)
            {
                var list = (IList<int>)TypeDescriptor.GetConverter(typeof(List<int>)).ConvertFrom(template.IgnoredProductTypes) ?? new List<int>();
                if (string.IsNullOrEmpty(template.IgnoredProductTypes) || !list.Contains((int)productType))
                {
                    model.ProductsTypesSupportedByProductTemplates[(int)productType].Add(new SelectListItem
                    {
                        Text = template.Name,
                        Value = template.Id.ToString()
                    });
                }
            }
        }

        //prepare available delivery dates
        await _baseAdminModelFactory.PrepareDeliveryDatesAsync(model.AvailableDeliveryDates,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.DeliveryDate.None"));

        //prepare available product availability ranges
        await _baseAdminModelFactory.PrepareProductAvailabilityRangesAsync(model.AvailableProductAvailabilityRanges,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.ProductAvailabilityRange.None"));

        //prepare available vendors
        await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Vendor.None"));

        //prepare available tax categories
        await _baseAdminModelFactory.PrepareTaxCategoriesAsync(model.AvailableTaxCategories);

        //prepare available warehouses
        await _baseAdminModelFactory.PrepareWarehousesAsync(model.AvailableWarehouses,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Warehouse.None"));
        await PrepareProductWarehouseInventoryModelsAsync(model.ProductWarehouseInventoryModels, product);

        //prepare available base price units
        var availableMeasureWeights = (await _measureService.GetAllMeasureWeightsAsync())
            .Select(weight => new SelectListItem { Text = weight.Name, Value = weight.Id.ToString() }).ToList();
        model.AvailableBasepriceUnits = availableMeasureWeights;
        model.AvailableBasepriceBaseUnits = availableMeasureWeights;

        //prepare model categories
        await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories, false);
        foreach (var categoryItem in model.AvailableCategories)
        {
            categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
                && model.SelectedCategoryIds.Contains(categoryId);
        }

        //prepare model manufacturers
        await _baseAdminModelFactory.PrepareManufacturersAsync(model.AvailableManufacturers, false);
        foreach (var manufacturerItem in model.AvailableManufacturers)
        {
            manufacturerItem.Selected = int.TryParse(manufacturerItem.Value, out var manufacturerId)
                && model.SelectedManufacturerIds.Contains(manufacturerId);
        }

        //prepare model discounts
        var availableDiscounts = await _discountService.GetAllDiscountsAsync(
            discountType: DiscountType.AssignedToSkus,
            showHidden: true, isActive: null,
            vendorId: currentVendor?.Id ?? 0);

        await _discountSupportedModelFactory.PrepareModelDiscountsAsync(model, product, availableDiscounts, excludeProperties);

        //prepare model stores
        await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, product, excludeProperties);

        return model;
    }

    /// <summary>
    /// Prepare product model
    /// </summary>
    /// <param name="model">Product model</param>
    /// <param name="product">Product</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>Product model</returns>
    public virtual async Task<ProductTypeExtensionModel> PrepareEventTicketProductTypeExtensionModelAsync(ProductTypeExtensionModel model, Product product, bool excludeProperties = false)
    {

        if (product != null)
        {
            var productTypeExtension = await _eventTicketService.GetEventTicketProductTypeExtensionAsync(product);
            if (productTypeExtension != null)
            {

                //fill in model values from the entity
                if (model == null)
                {
                    model = new ProductTypeExtensionModel();
                    model.ProductId = productTypeExtension.ProductId;
                    model.ProductName = product.Name;
                    model.ProductTypeId = productTypeExtension.ProductTypeId;
                    model.RecordTypeId = CoreSystemHelper.RecordType.Configuration;
                    model.EventTicketDateTime = productTypeExtension.AvailabilityStartDate;
                    model.AvailabilityStartDate = await _apolloCoreService.GetLocalisedDateAsync(model.EventTicketDateTime, null);
                    model.EventTicketReportDateTime = productTypeExtension.AvailabilityEndDate;
                    model.AvailabilityEndDate = await _apolloCoreService.GetLocalisedDateTimeAsync(model.EventTicketReportDateTime, null);
                    model.AvailabilityDuration = productTypeExtension.AvailabilityDuration;
                    model.Quantity = productTypeExtension.Quantity;
                    model.WeeklyPurchaseLimit = productTypeExtension.WeeklyPurchaseLimit;
                    model.MonthlyPurchaseLimit = productTypeExtension.MonthlyPurchaseLimit;
                    model.YearlyPurchaseLimit = productTypeExtension.YearlyPurchaseLimit;
                    model.RecordStatusId = productTypeExtension.RecordStatusId;
                    model.ProductTime = productTypeExtension.ProductTime;
                }
                else
                {


                }

                foreach (var status in Enum.GetValues(typeof(CoreSystemHelper.RecordStatus)).OfType<CoreSystemHelper.RecordStatus>())
                    model.AvailableRecordStatus.Add(new SelectListItem { Text = status.ToString(), Value = ((int)status).ToString() });

            }
        }

        //set default values for the new model
        if (product == null)
        {
            model = new ProductTypeExtensionModel();
        }

        return model;
    }

    
    /// <summary>
    /// Prepare required product search model to add to the product
    /// </summary>
    /// <param name="searchModel">Required product search model to add to the product</param>
    /// <returns>Required product search model to add to the product</returns>
    public virtual async Task<AddRequiredProductSearchModel> PrepareAddRequiredProductSearchModelAsync(AddRequiredProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

        //prepare available categories
        await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

        //prepare available manufacturers
        await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

        //prepare available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        //prepare available vendors
        await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

        //prepare available product types
        await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare required product list model to add to the product
    /// </summary>
    /// <param name="searchModel">Required product search model to add to the product</param>
    /// <returns>Required product list model to add to the product</returns>
    public virtual async Task<AddRequiredProductListModel> PrepareAddRequiredProductListModelAsync(AddRequiredProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
            searchModel.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //get products
        var products = await _eventTicketService.SearchTicketsAsync(showHidden: false,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerId: searchModel.SearchManufacturerId,
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddRequiredProductListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                var productModel = product.ToModel<ProductModel>();
                productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                return productModel;
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare paged related product list model
    /// </summary>
    /// <param name="searchModel">Related product search model</param>
    /// <param name="product">Product</param>
    /// <returns>Related product list model</returns>
    public virtual async Task<RelatedProductListModel> PrepareRelatedProductListModelAsync(RelatedProductSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        //get related products
        var relatedProducts = (await _productService.GetRelatedProductsByProductId1Async(productId1: product.Id, showHidden: true)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new RelatedProductListModel().PrepareToGridAsync(searchModel, relatedProducts, () =>
        {
            return relatedProducts.SelectAwait(async relatedProduct =>
            {
                //fill in model values from the entity
                var relatedProductModel = relatedProduct.ToModel<RelatedProductModel>();

                //fill in additional values (not existing in the entity)
                relatedProductModel.Product2Name = (await _productService.GetProductByIdAsync(relatedProduct.ProductId2))?.Name;

                return relatedProductModel;
            });
        });
        return model;
    }

    /// <summary>
    /// Prepare related product search model to add to the product
    /// </summary>
    /// <param name="searchModel">Related product search model to add to the product</param>
    /// <returns>Related product search model to add to the product</returns>
    public virtual async Task<AddRelatedProductSearchModel> PrepareAddRelatedProductSearchModelAsync(AddRelatedProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

        //prepare available categories
        await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

        //prepare available manufacturers
        await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

        //prepare available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        //prepare available vendors
        await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

        //prepare available product types
        await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged related product list model to add to the product
    /// </summary>
    /// <param name="searchModel">Related product search model to add to the product</param>
    /// <returns>Related product list model to add to the product</returns>
    public virtual async Task<AddRelatedProductListModel> PrepareAddRelatedProductListModelAsync(AddRelatedProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
            searchModel.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //get products
        var products = await _eventTicketService.SearchTicketsAsync(showHidden: false,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerId: searchModel.SearchManufacturerId,
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddRelatedProductListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                var productModel = product.ToModel<ProductModel>();
                productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                return productModel;
            });
        });

        return model;
    }

    #endregion

}