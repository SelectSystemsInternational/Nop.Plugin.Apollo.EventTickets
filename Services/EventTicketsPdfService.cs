using Nop.Core;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Services.Vendors;

using QRCoder;
using iTextSharp.text;
using iTextSharp.text.pdf;

using Nop.Plugin.Apollo.Core.Services;
using Nop.Plugin.Apollo.Core.Settings;
using Nop.Plugin.Apollo.EventTickets.Domain;
using Nop.Plugin.Apollo.EventTickets.Settings;
using Nop.Services.Customers;


namespace Nop.Plugin.Apollo.EventTickets.Services;

public class EventTicketsPdfService : ApolloPdfService, IEventTicketsPdfService
{

    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly IEventTicketService _eventTicketService;
    protected readonly EventTicketSettings _eventTicketSettings;
    protected readonly IProductTypeExtensionsService _productTypeExtensionsService;

    #endregion

    #region Ctor

    public EventTicketsPdfService(AddressSettings addressSettings,
        CatalogSettings catalogSettings,
        CurrencySettings currencySettings,
        IAddressService addressService,
        IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
        ICountryService countryService,
        ICurrencyService currencyService,
        IDateTimeHelper dateTimeHelper,
        IGiftCardService giftCardService,
        IHtmlFormatter htmlFormatter,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IMeasureService measureService,
        INopFileProvider fileProvider,
        IOrderService orderService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IRewardPointService rewardPointService,
        ISettingService settingService,
        IShipmentService shipmentService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        IStoreService storeService,
        IThumbService thumbService,
        IVendorService vendorService,
        IWorkContext workContext,
        PdfSettings pdfSettings,
        MeasureSettings measureSettings,
        TaxSettings taxSettings,
        VendorSettings vendorSettings,
        IWebHelper webHelper,
        ITopicService topicService,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductAttributeFormatter productAttributeFormatter,
        IApolloCoreService apolloCoreService,
        ApolloCoreSettings apolloCoreSettings,
        ICustomerService customerService,
        IEventTicketService eventTicketService,
        EventTicketSettings eventTicketSettings,
        IProductTypeExtensionsService productTypeExtensionsService) : base(pdfSettings,
            addressSettings,
            catalogSettings,
            currencySettings,
            addressService,
            addressAttributeFormatter,
            countryService,
            currencyService,
            dateTimeHelper,
            giftCardService,
            htmlFormatter,
            languageService,
            localizationService,
            measureService,
            fileProvider,
            orderService,
            paymentPluginManager,
            paymentService,
            pictureService,
            priceFormatter,
            productService,
            rewardPointService,
            settingService,
            shipmentService,
            stateProvinceService,
            storeContext,
            storeService,
            thumbService,
            vendorService,
            workContext,
            measureSettings,
            taxSettings,
            vendorSettings,
            webHelper,
            topicService,
            productAttributeParser,
            productAttributeService,
            productAttributeFormatter,
            apolloCoreService,
            apolloCoreSettings)
    {
        _customerService = customerService;
        _eventTicketService = eventTicketService;
        _eventTicketSettings = eventTicketSettings;
        _productTypeExtensionsService = productTypeExtensionsService;
    }

    #endregion

    #region Utilities


    /// <summary>
    /// Print header
    /// </summary>
    /// <param name="pdfSettingsByStore">PDF settings</param>
    /// <param name="lang">Language</param>
    /// <param name="order">Order</param>
    /// <param name="font">Text font</param>
    /// <param name="titleFont">Title font</param>
    /// <param name="doc">Document</param>
    protected override async Task PrintHeaderAsync(PdfSettings pdfSettingsByStore, Language lang, Order order, Font font, Font titleFont, Document doc)
    {
        //logo
        var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
        var logoExists = logoPicture != null;

        //header
        var headerTable = new PdfPTable(logoExists ? 2 : 1)
        {
            RunDirection = GetDirection(lang)
        };
        headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

        //store info
        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? (await _storeContext.GetCurrentStoreAsync());
        var anchor = new Anchor(store.Url.Trim('/'), font)
        {
            Reference = store.Url
        };

        var cellHeader = GetPdfCell(string.Format(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.Order#", lang.Id), order.CustomOrderNumber), titleFont);
        cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellHeader.Phrase.Add(new Phrase(anchor));
        cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellHeader.Phrase.Add(await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.OrderDate", lang, font, 
            await _apolloCoreService.GetLocalisedDateTimeAsync(order.CreatedOnUtc, lang)));
        cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
        cellHeader.Border = Rectangle.NO_BORDER;

        headerTable.AddCell(cellHeader);

        if (logoExists)
            headerTable.SetWidths(lang.Rtl ? new[] { 0.2f, 0.8f } : new[] { 0.8f, 0.2f });
        headerTable.WidthPercentage = 100f;

        //logo               
        if (logoExists)
        {
            var (pictureUrl, _) = await _pictureService.GetPictureUrlAsync(logoPicture, pdfSettingsByStore.ImageTargetSize, false);
            var logoFilePath = await _thumbService.GetThumbLocalPathAsync(pictureUrl);
            var logo = Image.GetInstance(logoFilePath);
            logo.Alignment = GetAlignment(lang, true);
            float size = 100;
            if (_eventTicketSettings.LogoSize != 0)
                size = _eventTicketSettings.LogoSize;
            logo.ScaleToFit(size, size);

            var cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
            cellLogo.AddElement(logo);
            headerTable.AddCell(cellLogo);
        }

        doc.Add(headerTable);
    }

    /// <summary>
    /// Print header
    /// </summary>
    /// <param name="pdfSettingsByStore">PDF settings</param>
    /// <param name="lang">Language</param>
    /// <param name="order">Order</param>
    /// <param name="font">Text font</param>
    /// <param name="titleFont">Title font</param>
    /// <param name="doc">Document</param>
    protected async virtual Task PrintReportHeaderAsync(string reportName, string reportDetails, PdfSettings pdfSettingsByStore, Language lang, Font font, Font titleFont, Document doc,
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null, Product product = null)
    {
        titleFont.Size = 12;

        //logo
        var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
        var logoExists = logoPicture != null;

        //header
        var headerTable = new PdfPTable(logoExists ? 2 : 1)
        {
            RunDirection = GetDirection(lang)
        };
        headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
        headerTable.WidthPercentage = 100f;

        if (logoExists)
            headerTable.SetWidths(new[] { 55, 45 });

        //store info
        var anchor = new Anchor(store.Url.Trim('/'), font)
        {
            Reference = store.Url
        };

        var cellHeader = GetPdfCell(await _localizationService.GetResourceAsync(reportName, lang.Id, defaultValue: "Report Name"), titleFont);
        cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellHeader.Phrase.Add(new Phrase(anchor));
        cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellHeader.Phrase.Add(await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.Date", lang, font, 
            await _apolloCoreService.GetLocalisedDateAsync(DateTime.UtcNow, lang)));
        cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
        cellHeader.Border = Rectangle.NO_BORDER;

        headerTable.AddCell(cellHeader);

        var cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };

        //logo           
        if (logoExists)
        {
            var (pictureUrl, _) = await _pictureService.GetPictureUrlAsync(logoPicture, pdfSettingsByStore.ImageTargetSize, false);
            var logoFilePath = await _thumbService.GetThumbLocalPathAsync(pictureUrl);
            var logo = Image.GetInstance(logoFilePath);
            float size = 100;
            if (_eventTicketSettings.LogoSize != 0)
                size = _eventTicketSettings.LogoSize;
            logo.ScaleToFit(size, size);
            logo.Border = 0;

            cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
            cellLogo.VerticalAlignment = Cell.ALIGN_TOP;
            cellLogo.HorizontalAlignment = Cell.ALIGN_RIGHT;
            cellLogo.AddElement(logo);
            headerTable.AddCell(cellLogo);
        }

        string name = store.Name;
        if (vendor != null)
            name = vendor.Name;
        else if (manufacturer != null)
            name = manufacturer.Name;
        else if (product != null)
            name = product.Name;

        var cellPromoLeftHeader = GetPdfCell(string.Format(await _localizationService.GetResourceAsync(reportDetails, lang.Id), name), titleFont);
        cellPromoLeftHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellPromoLeftHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellPromoLeftHeader.VerticalAlignment = Element.ALIGN_BOTTOM;
        cellPromoLeftHeader.HorizontalAlignment = Element.ALIGN_LEFT;
        cellPromoLeftHeader.Border = Rectangle.NO_BORDER;
        headerTable.AddCell(cellPromoLeftHeader);

        string promo = await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Promo", lang.Id);
        var cellPromoRightHeader = GetPdfCell("   " + promo, titleFont);
        cellPromoRightHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellPromoRightHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellPromoRightHeader.Phrase.Add(new Phrase(Environment.NewLine));
        cellPromoRightHeader.HorizontalAlignment = Element.ALIGN_LEFT;
        cellPromoRightHeader.Border = Rectangle.NO_BORDER;
        headerTable.AddCell(cellPromoRightHeader);

        doc.Add(headerTable);
    }

    protected virtual async Task PrintOrdersReportHeaderAsync(int vendorId, Language lang, Font titleFont, Document doc, Font font, Font attributesFont)
    {
        var productsHeader = new PdfPTable(1)
        {
            RunDirection = GetDirection(lang),
            WidthPercentage = 100f
        };

        var count = 3;

        var headerTable = new PdfPTable(count)
        {
            RunDirection = GetDirection(lang),
            WidthPercentage = 100f
        };

        var widths = new Dictionary<int, int[]>
        {
            { 3, new[] { 50, 40, 35 } }             
        };

        headerTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

        // Report Name

        // Report Name
        var cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.Name", lang, titleFont);
        cellProductItem.Border = Rectangle.NO_BORDER;
        headerTable.AddCell(cellProductItem);

        //Category       
        
        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.Category", lang, titleFont);
        //cellProductItem.BackgroundColor = BaseColor.LightGray;
        //cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        cellProductItem.Border = Rectangle.NO_BORDER;
        headerTable.AddCell(cellProductItem);

        //Date
        var dateString = "Date: " + DateTime.UtcNow;
        cellProductItem = GetPdfCell(dateString, titleFont);
        cellProductItem.Border = Rectangle.NO_BORDER;
        headerTable.AddCell(cellProductItem);

        doc.Add(headerTable);
        doc.Add(new Paragraph(" "));
 
    }

    #endregion

    #region Ticket
    /// <summary>
    /// Print addresses
    /// </summary>
    /// <param name="vendorId">Vendor identifier</param>
    /// <param name="lang">Language</param>
    /// <param name="titleFont">Title font</param>
    /// <param name="order">Order</param>
    /// <param name="font">Text font</param>
    /// <param name="doc">Document</param>
    protected override async Task PrintTicket(PdfSettings pdfSettingsByStore, Document doc, Language lang, OrderItem orderItem, string transactionId)
    {

        //fonts

        var font6 = GetFont();
        font6.Size = 6;

        var font12 = GetFont();
        font12.Size = 12;

        var titleFont14 = GetFont();
        titleFont14.SetStyle(Font.BOLD);
        titleFont14.Size = 14;
        titleFont14.Color = BaseColor.Blue;

        if (!string.IsNullOrEmpty(_eventTicketSettings.TicketFontColor))
        {
            switch (_eventTicketSettings.TicketFontColor)
            {
                case "Blue":
                    titleFont14.Color = BaseColor.Blue;
                    break;
                case "Cyan":
                    titleFont14.Color = BaseColor.Cyan;
                    break;
                case "DarkGray":
                    titleFont14.Color = BaseColor.DarkGray;
                    break;
                case "Gray":
                    titleFont14.Color = BaseColor.Gray;
                    break;
                case "Green":
                    titleFont14.Color = BaseColor.Green;
                    break;
                case "LightGray":
                    titleFont14.Color = BaseColor.LightGray;
                    break;
                case "Magenta":
                    titleFont14.Color = BaseColor.Magenta;
                    break;
                case "Orange":
                    titleFont14.Color = BaseColor.Orange;
                    break;
                case "Pink":
                    titleFont14.Color = BaseColor.Pink;
                    break;
                case "Red":
                    titleFont14.Color = BaseColor.Red;
                    break;
            }
        }

        var attributesFont12 = GetFont();
        attributesFont12.Size = 12;
        attributesFont12.SetStyle(Font.ITALIC);

        var ticketTable = new PdfPTable(3) { RunDirection = GetDirection(lang) };
        ticketTable.DefaultCell.Border = Rectangle.BOX;
        ticketTable.DefaultCell.BorderWidth = 2;

        ticketTable.WidthPercentage = 100f;
        ticketTable.SetWidths(new[] { 14, 46, 40 });

        var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
        var logoExists = logoPicture != null;

        //logo               
        if (logoExists)
        {
            var (pictureUrl, _) = await _pictureService.GetPictureUrlAsync(logoPicture, pdfSettingsByStore.ImageTargetSize, false);
            var logoFilePath = await _thumbService.GetThumbLocalPathAsync(pictureUrl);
            var logo = Image.GetInstance(logoFilePath);
            logo.RotationDegrees = 90;
            float size = 100;
            if (_eventTicketSettings.LogoSize != 0)
                size = _eventTicketSettings.LogoSize;
            logo.ScaleToFit(size, size);

            var cellLogo = new PdfPCell { Border = Rectangle.BOX };
            cellLogo.BorderWidth = 2;
            cellLogo.AddElement(logo);
            cellLogo.PaddingTop = 5;
            ticketTable.AddCell(cellLogo);
        }

        string doorDetails = "Door Time";
        string eventDetails = "Event Date and Time";

        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
        if (product != null)
        {

            //product name
            var name = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id);
            name = CommonHelper.EnsureMaximumLength(name, 72);

            if (product.ParentGroupedProductId != 0)
            {
                var parentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentProduct != null)
                {
                    var parentName = await _localizationService.GetLocalizedAsync(parentProduct, x => x.Name, lang.Id);
                    parentName = CommonHelper.EnsureMaximumLength(parentName, 72);
                    name = parentName + "\n" + name;
                }
            }

            //event info
            var eventTicketService = EngineContext.Current.Resolve<IEventTicketService>();
            var pte = await eventTicketService.GetEventTicketProductTypeExtensionAsync(product.Id);
            if (pte != null)
            {
                eventDetails = await _apolloCoreService.GetLocalisedDateTimeAsync(pte.AvailabilityStartDate, lang, true);
                doorDetails = string.Format("{0}{1}", await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Doors", lang.Id), product.Gtin);
            }

            string venue = string.Empty;
            string addressLine = string.Empty;
            if (product.VendorId != 0)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor != null)
                {
                    venue = vendor.Name;
                    var venueAddress = await _addressService.GetAddressByIdAsync(vendor.AddressId);
                    if (venueAddress != null)
                    {
                        addressLine = $"{venueAddress.Address1 + venueAddress.Address2}, " +
                                      $"{(!string.IsNullOrEmpty(venueAddress.City) ? $"{venueAddress.City}, " : string.Empty)}" +
                                      $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(venueAddress) is StateProvince stateProvince ? (await _localizationService.GetLocalizedAsync(stateProvince, x => x.Abbreviation, lang.Id)) : string.Empty)}";
                    }
                }
            }

            string quantity = "Admit: " + orderItem.Quantity.ToString();

            var ticketDetails = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            ticketDetails.DefaultCell.Border = Rectangle.NO_BORDER;
            ticketDetails.DefaultCell.BorderWidth = 2;

            var cellTicketItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.TicketDetails", lang, font12);
            cellTicketItem.BackgroundColor = BaseColor.LightGray;
            cellTicketItem.PaddingBottom = 10;
            cellTicketItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            // Blank Line
            cellTicketItem = GetPdfCell("", font6);
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            cellTicketItem = GetPdfCell(name, titleFont14);
            cellTicketItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            // Blank Line
            cellTicketItem = GetPdfCell("", font6);
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            cellTicketItem = GetPdfCell(CommonHelper.EnsureMaximumLength(product.ShortDescription, 115), attributesFont12);
            cellTicketItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            // Blank Line
            cellTicketItem = GetPdfCell("", font6);
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            cellTicketItem = GetPdfCell(CommonHelper.EnsureMaximumLength(eventDetails, 38), titleFont14);
            cellTicketItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            cellTicketItem = GetPdfCell(doorDetails, attributesFont12);
            cellTicketItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            // Blank Line
            cellTicketItem = GetPdfCell("", font6);
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            cellTicketItem = GetPdfCell(CommonHelper.EnsureMaximumLength(venue, 38), titleFont14);
            cellTicketItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            cellTicketItem = GetPdfCell(CommonHelper.EnsureMaximumLength(addressLine, 115), attributesFont12);
            cellTicketItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            // Blank Line
            cellTicketItem = GetPdfCell("", font6);
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);
            ticketDetails.AddCell(cellTicketItem);

            cellTicketItem = GetPdfCell(CommonHelper.EnsureMaximumLength(quantity, 38), titleFont14);
            cellTicketItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellTicketItem.Border = Rectangle.NO_BORDER;
            ticketDetails.AddCell(cellTicketItem);

            ticketTable.AddCell(ticketDetails);

        }

        PrintQRCodeInfo(lang, orderItem.OrderId, transactionId, font12, ticketTable, 170f);

        doc.Add(ticketTable);
    }

    protected override async Task PrintTicketConditionsAsync(Document doc, Language lang, Font titleFont, Font bodyFont)
    {

        var conditionsHeader = new PdfPTable(1)
        {
            RunDirection = GetDirection(lang),
            WidthPercentage = 100f
        };

        var allTopics = await _topicService.GetAllTopicsAsync((await _storeContext.GetCurrentStoreAsync()).Id, true);
        var ticketConditions = allTopics.Where(x => x.SystemName != null && x.SystemName.Equals("TicketConditions")).FirstOrDefault();
        if (ticketConditions != null)
        {
            var cellTopics = GetPdfCell(ticketConditions.Title, titleFont); // No language translation
            cellTopics.Border = Rectangle.NO_BORDER;
            conditionsHeader.AddCell(cellTopics);
            doc.Add(cellTopics);
            doc.Add(new Paragraph(" "));

            string conditions = ticketConditions.Body.Replace("<p>", "");
            conditions = conditions.Replace("</p>", "");

            cellTopics = GetPdfCell(_htmlFormatter.ConvertHtmlToPlainText(conditions, true, true), bodyFont);
            cellTopics.Border = Rectangle.NO_BORDER;
            conditionsHeader.AddCell(cellTopics);
            doc.Add(conditionsHeader);
            doc.Add(new Paragraph(" "));
        }
    }

    #endregion

    #region Orders Report

    public virtual async Task PrintOrdersToPdfAsync(Stream stream, IList<OrderItem> orderItems, int languageId = 0, int vendorId = 0)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (orderItems == null)
            throw new ArgumentNullException(nameof(orderItems));

        var store = _storeContext.GetCurrentStoreAsync();

        //by default _pdfSettings contains settings for the current active store
        //and we need PdfSettings for the store which was used to place an order
        //so let's load it based on a store of the current order
        var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(store.Id);

        var pageSize = PageSize.A4;
        if (pdfSettingsByStore.LetterPageSizeEnabled)
        {
            pageSize = PageSize.Letter;
        }

        var doc = new Document(pageSize);
        var pdfWriter = PdfWriter.GetInstance(doc, stream);
        doc.Open();

        doc.SetMargins(36, 36, 36, 40);

        pdfWriter.PageEvent = new PageHeaderFooter();

        //fonts
        var font = GetFont(_eventTicketSettings.FontFileName);
        var attributesFont = GetFont(_eventTicketSettings.FontFileName);
        var titleFont = GetFont(_eventTicketSettings.FontFileName);
        
        titleFont.SetStyle(Font.BOLD);
        titleFont.Color = BaseColor.Black;

        attributesFont.SetStyle(Font.ITALIC);

        // var ordCount = orderItems.Count;
        var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? 1 : languageId);

        if (lang == null || !lang.Published)
            lang = await _workContext.GetWorkingLanguageAsync();

        await PrintOrdersReportHeaderAsync(vendorId, lang, titleFont, doc, font, attributesFont);

        var count = 6;

        var ordersTable = new PdfPTable(count)
        {
            RunDirection = GetDirection(lang),
            WidthPercentage = 100f
        };

        var widths = new Dictionary<int, int[]>
        {
            { 6, new[] { 20, 8, 40, 10, 15, 15 } },
        };

        ordersTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

        var ordersHeader = new PdfPTable(1)
        {
            RunDirection = GetDirection(lang),
            WidthPercentage = 100f
        };

        //Customer

        var cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.Customer", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        //Order Number
        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.Order", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        //Order Details
        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.OrderDetails", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        //Payment 
        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.Payment", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        // Paid Date
        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.PaidDate", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        // Payment Method
        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.Method", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        foreach (var orderItem in orderItems)
        {
            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
           
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            // a vendor should have access only to his products
            if (vendorId > 0 && product.VendorId != vendorId)
                continue;

            // Customer
            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var customerId = billingAddress.FirstName + " " + billingAddress.LastName;
            cellProductItem = GetPdfCell(customerId, font);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            // Order Number
            cellProductItem = GetPdfCell(order.CustomOrderNumber, font);

            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);            
           
            var pAttribTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

            // Order Details
            var getParentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
            if(getParentProduct != null)
            {
                pAttribTable.AddCell(new Paragraph(getParentProduct.Name, font));
                cellProductItem.AddElement(new Paragraph(getParentProduct.Name, font));
            }
            
            var name = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id);
            pAttribTable.AddCell(new Paragraph(name, font));
            cellProductItem.AddElement(new Paragraph(name, font));
            // Product Attributes
            if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
            {
                var attributesParagraph =
                    new Paragraph(_htmlFormatter.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                        attributesFont);
                pAttribTable.AddCell(attributesParagraph);
            }
            ordersTable.AddCell(pAttribTable);

            // Payment Status                
            var paymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
            cellProductItem = GetPdfCell(paymentStatus, font);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            //Paid Date
            var paidDateUtc = "Not Paid";
            if (order.PaidDateUtc != null)
            {
                //Payment Status
                paidDateUtc = order.PaidDateUtc.Value.ToString(await _apolloCoreService.GetLocalisedDisplayShortDateFormatAsync());
            }

            cellProductItem = GetPdfCell(paidDateUtc, font);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            //Payment Method
            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
            var paymentMethodFriendlyName = paymentMethod != null ? 
                await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, 
                (await _workContext.GetWorkingLanguageAsync()).Id) : (order.PaymentMethodSystemName==null ? "" : order.PaymentMethodSystemName);

            cellProductItem = GetPdfCell(paymentMethodFriendlyName, font);
            cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
            ordersTable.AddCell(cellProductItem);

        }

        doc.Add(ordersTable);

        doc.NewPage();
        doc.Close();
    }

    #endregion

    #region Sales Summary Report

    /// <summary>
    /// Print a summary report to PDF
    /// </summary>
    /// <param name="fileName">the fileName for the pdf that is procuded</param>
    /// <param name="orderItems">List of orderItems to display on the report</param>
    /// <param name="languageId">Language</param>
    /// <param name="store">Store for the sale of the event ticket products</param>
    /// <param name="vendor">Vendor identifier to limit products; null to print all products</param>/// 
    /// <param name="manufacturer">Manufacturer identifier to limit products; null to print all products</param>/// 
    /// <returns>A path of generated file</returns>
    public virtual async Task<(string, string)> PrintSalesSummaryReportToPDFAsync(IList<OrderItem> orderItems, int languageId = 0, 
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null, Affiliate affiliate = null)
    {
        string fileName = string.Empty;
        var dateTime = DateTime.Now;
        string dateTimeTxt = String.Format("{0:dd-MM-y}", dateTime);

        if (store == null)
            throw new ArgumentNullException(nameof(store));
        else if (vendor != null)
            fileName = $"summary_report_{vendor.Name}_{dateTimeTxt}.pdf";
        else if (affiliate != null)
            fileName = $"summary_report_{affiliate.Id}_{dateTimeTxt}.pdf";
        else
            fileName = $"summary_report_{store.Name}_{dateTimeTxt}.pdf";


        var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/exportimport"), fileName);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await PrintSalesSummaryReportAsync(fileStream, orderItems, languageId, store, vendor);
        }

        return (filePath, fileName);
    }

    /// <summary>
    ///     Print event product summary report
    /// </summary>
    /// <param name="orderCollection">Order collection</param>
    /// <param name="filePath">File path</param>
    /// <remarks>[ncc-006]</remarks>
    public virtual async Task<bool> PrintSalesSummaryReportAsync(Stream stream, IList<OrderItem> orderItems, int languageId = 0,
                Store store = null, Vendor vendor = null, Manufacturer manufacturer = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        string message = "Running Sales Summart Report for ";
        if (store != null)
            message += "Store: " + store.Name + " ";
        if (vendor != null)
            message += "Vendor: " + vendor.Name + " ";
        if (manufacturer != null)
            message += "Manufacturer: " + manufacturer.Name + " ";

        string fullMessage = "Total Order Items: " + orderItems.Count().ToString();

        if (_eventTicketSettings.TestMode)
            await _apolloCoreService.ApolloLogAsync(LogLevel.Debug, message, fullMessage, null);

        //by default _pdfSettings contains settings for the current active store
        //and we need PdfSettings for the store which was used to place an order
        //so let's load it based on a store of the current order
        var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(store.Id);

        var pageSize = PageSize.A4;
        if (pdfSettingsByStore.LetterPageSizeEnabled)
        {
            pageSize = PageSize.Letter;
        }

        var doc = new Document(pageSize);
        var pdfWriter = PdfWriter.GetInstance(doc, stream);
        doc.Open();
        doc.SetMargins(36, 36, 36, 40);

        pdfWriter.PageEvent = new PageHeaderFooter();

        //fonts
        var font = GetFont(_eventTicketSettings.FontFileName);
        var attributesFont = GetFont(_eventTicketSettings.FontFileName);
        var titleFont = GetFont(_eventTicketSettings.FontFileName);

        titleFont.SetStyle(Font.BOLD);
        titleFont.Color = BaseColor.Black;

        attributesFont.SetStyle(Font.ITALIC);

        // var ordCount = orderItems.Count;
        var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? 1 : languageId);

        if (lang == null || !lang.Published)
            lang = await _workContext.GetWorkingLanguageAsync();

        FooterDetails = "Address";
        if (store != null)
            FooterDetails = store.Name + " - ";
        if (vendor != null)
            FooterDetails = vendor.Name + " - ";
        if (manufacturer != null)
            FooterDetails = manufacturer.Name + " - ";

        FooterDetails += _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Address", lang.Id);

        //header
        await PrintReportHeaderAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Name",
            "Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Details",
            pdfSettingsByStore, lang, attributesFont, titleFont, doc, store, vendor);

        var productList = new List<Product>(); //ToDo

        foreach (var orderItem in orderItems)
        {
            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (!productList.Contains(product))
            {
                if (product.ParentGroupedProductId == 0 && product.ProductType == ProductType.GroupedProduct)
                    productList.Add(product);
                if (product.ParentGroupedProductId == 0 && product.ProductType == ProductType.SimpleProduct)
                    productList.Add(product);
                else if (product.ParentGroupedProductId != 0 && product.ProductType == ProductType.SimpleProduct)
                {
                    var productParent = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                    if (productParent == null)
                        throw new ArgumentNullException(nameof(productParent));
                    if (!productList.Contains(productParent))
                        productList.Add(productParent);
                }
            }

        }

        productList = productList.OrderBy(p => p.DisplayOrder).ToList();

        var productCollection = await AddSummaryProductCollectionAsync(doc, productList, lang, font);

        if (productCollection != null && productCollection.Count > 0)
            await AddDetailedProductCollectionAsync(doc, productCollection, lang, font, titleFont);

        doc.NewPage();
        doc.Close();

        return (true);
    }

    protected virtual async Task<List<Product>> AddSummaryProductCollectionAsync(Document doc, List<Product> products, Language lang, Font font, int storeId = 0)
    {

        var fontBold = GetFont(_eventTicketSettings.FontFileName);
        fontBold.SetStyle(Font.BOLD);

        var ordersTable = new PdfPTable(6);
        ordersTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
        ordersTable.DefaultCell.BorderWidthBottom = 0.5f;

        ordersTable.WidthPercentage = 100f;
        ordersTable.SetWidths(new[] { 10, 2, 2, 2, 2, 3 });

        //Order ID

        var cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Event", lang, fontBold);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Alloc", lang, fontBold);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.24hr", lang, fontBold);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.SoldTotal", lang, fontBold);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Rem", lang, fontBold);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.TotalSales", lang, fontBold);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        decimal totalValue = Decimal.Zero;
        int totalSold = 0;
        int total24hr = 0;
        string total = string.Empty;
        string value = string.Empty;
        var detailedProductCollection = new List<Product>();

        foreach (var product in products)
        {
            var pte = await _productTypeExtensionsService.GetProductTypeExtensionById(product.Id);
            if (pte != null)
            {
                var productTicketVariants = new List<Product>();
                var opvCollection = new List<OrderTicketItem>();

                if (product.ProductType == ProductType.SimpleProduct)
                    productTicketVariants.Add(product);
                else if (product.ProductType == ProductType.GroupedProduct)
                {
                    var pvs = await _productService.GetAssociatedProductsAsync(product.Id, storeId, showHidden: true);
                    productTicketVariants.AddRange(pvs);
                }

                foreach (var ptv in productTicketVariants)
                {
                    var opvcOnline = await _eventTicketService.GetAllOrderProductVariantsSummaryAsync(0, ptv.Id, null, null, true);
                    opvCollection.AddRange(opvcOnline);
                }

                if (opvCollection.Count > 0)
                {

                    int sold = opvCollection.Sum(x => x.Quantity);
                    if (sold > 0)
                        detailedProductCollection.Add(product); // add to detailed breakdown list

                    int sold24hr = opvCollection.Where(x => x.Order != null && DateTime.UtcNow.Subtract(x.Order.CreatedOnUtc).TotalHours <= 24).Sum(x => x.Quantity);
                    int allocation = 0; 
                    int remaining = 0;

                    var tmpCollection = opvCollection.OrderBy(x => x.Product.Name).GroupBy(x => x.Product.Id).Select(x => new
                    {
                        Name = GetProductName(x.First()),
                        Alloc = GetOrderAllocationValue(x),
                        Qty = GetOrderQuantity(x),
                        Rem = GetOrderRemainingValue(x),
                    });

                    foreach (var group in tmpCollection)
                    {
                        allocation += group.Alloc;
                        remaining += group.Rem;
                    }

                    (value, total) = await GetOrderTotalProductCostAsync(opvCollection);
                    totalValue += decimal.Parse(total);
                    totalSold += sold;
                    total24hr += sold24hr;

                    string eventDate = " on " + await _apolloCoreService.GetLocalisedDateAsync(pte.AvailabilityStartDate, lang);

                    cellProductItem = GetPdfCell(String.Format("{0}{1}", product.Name, eventDate) ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    ordersTable.AddCell(cellProductItem);

                    cellProductItem = GetPdfCell(allocation.ToString() ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    ordersTable.AddCell(cellProductItem);

                    cellProductItem = GetPdfCell(sold24hr.ToString() ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    ordersTable.AddCell(cellProductItem);

                    cellProductItem = GetPdfCell(sold.ToString() ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    ordersTable.AddCell(cellProductItem);

                    cellProductItem = GetPdfCell(remaining.ToString() ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    ordersTable.AddCell(cellProductItem);

                    cellProductItem = GetPdfCell(value.ToString() ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    ordersTable.AddCell(cellProductItem);

                }
            }
        }

        if (products.Count != 0)
        {
            cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.TotalSales", lang, fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("", fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell(total24hr.ToString(), fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell(totalSold.ToString(), fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("", fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            var price = await _priceFormatter.FormatPriceAsync(totalValue, false, await _workContext.GetWorkingCurrencyAsync(), lang.Id, true);
            cellProductItem = GetPdfCell(price.ToString(), fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            doc.Add(ordersTable);
            doc.Add(new Paragraph(" "));

            return (detailedProductCollection);
        }

        return null;
    }

    protected virtual async Task AddDetailedProductCollectionAsync(Document doc, List<Product> products, Language lang, Font font, Font titleFont, int storeId = 0)
    {

        var fontBold = GetFont(_eventTicketSettings.FontFileName);
        fontBold.SetStyle(Font.BOLD);

        var headerTable = new PdfPTable(1);
        headerTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
        headerTable.DefaultCell.BorderWidthBottom = 0.5f;

        headerTable.WidthPercentage = 100f;

        var cellHeader = GetPdfCell(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Breakdown", lang.Id), titleFont);
        cellHeader.Border = Rectangle.NO_BORDER;
        cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
        headerTable.AddCell(cellHeader);

        doc.Add(headerTable);
        doc.Add(new Paragraph(" "));

        foreach (var product in products)
        {
            var pte = await _productTypeExtensionsService.GetProductTypeExtensionById(product.Id);
            if (pte != null)
            {
                var productTicketVariants = new List<Product>();

                if (product.ProductType == ProductType.SimpleProduct)
                    productTicketVariants.Add(product);
                else if (product.ProductType == ProductType.GroupedProduct)
                {
                    var pvs = await _productService.GetAssociatedProductsAsync(product.Id, storeId, showHidden: true);
                    productTicketVariants.AddRange(pvs);
                }

                var ordersTable = new PdfPTable(7);
                ordersTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
                ordersTable.DefaultCell.BorderWidthBottom = 0.5f;

                ordersTable.WidthPercentage = 100f;
                ordersTable.SetWidths(new[] { 10, 5, 1.5f, 2, 1.5f, 1.5f, 3 });

                //Order ID
                string eventDate = " on " + await _apolloCoreService.GetLocalisedDateAsync(pte.AvailabilityStartDate, lang);

                string eventVenue = string.Empty;
                if (product.VendorId != 0)
                {
                    var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                    if (vendor != null)
                    {
                        eventVenue = vendor.Name;
                    }
                }

                var cellProductItem = GetPdfCell(string.Format("{0}{1} @ {2}", product.Name, eventDate, eventVenue), fontBold);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Outlet", lang, fontBold);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Alloc", lang, fontBold);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Cost", lang, fontBold);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Sold", lang, fontBold);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Rem", lang, fontBold);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Total", lang, fontBold);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                decimal totalValue = Decimal.Zero;
                int totalSold = 0;
                int totalAlloc = 0;
                int totalRem = 0;
                string total = string.Empty;
                string value = string.Empty;

                foreach (var ptv in productTicketVariants)
                {
                    var opvCollection = new List<OrderTicketItem>();

                    var opvcOnline = await _eventTicketService.GetAllOrderProductVariantsSummaryAsync(0, ptv.Id, null, null, true);
                    if (opvcOnline.Count > 0)
                    {
                        var dtNow = DateTime.UtcNow;
                        opvCollection.AddRange(opvcOnline);

                        var tmpCollection = await opvCollection.OrderBy(x => x.Product.Name).GroupBy(x => x.Outlet).SelectAwait(async x => new
                        {
                            Name = x.First().Product.Name,
                            Outlet = x.First().Outlet,
                            Alloc = GetOrderAllocationValue(x),
                            Cost = await GetProductVariantUnitProductCostForReportAsync(x.First()),
                            Price = await GetProductVariantUnitPriceAsync(x.First()),
                            Qty = GetOrderQuantity(x),
                            Rem = GetOrderRemainingValue(x),
                            Qty24 = GetOrderQuantity(x.Where(i => i.Order != null && dtNow.Subtract(i.Order.CreatedOnUtc).TotalHours <= 24)),
                            TotalCost = await GetReportLineTotalCostAsync(x),
                            Total = await GetOrderProductVariantSubTotalAsync(x)

                        }).ToListAsync();

                        var groupTemp = tmpCollection.GroupBy(x => x.Name);

                        foreach(var group in groupTemp)
                        {
                            bool first = true;
                            foreach (var item in group)
                            {
                                (value, total) = await GetOrderTotalProductCostAsync(opvCollection);
                                totalValue += decimal.Parse(total);
                                totalSold += item.Qty;
                                totalAlloc += item.Alloc;
                                totalRem += item.Rem;

                                if (first)
                                    cellProductItem = GetPdfCell(item.Name ?? string.Empty, font);
                                else
                                    cellProductItem = GetPdfCell(string.Empty ?? string.Empty, font);

                                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                                ordersTable.AddCell(cellProductItem);

                                cellProductItem = GetPdfCell(item.Outlet.ToString() ?? string.Empty, font);
                                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                                ordersTable.AddCell(cellProductItem);

                                cellProductItem = GetPdfCell(item.Alloc.ToString() ?? string.Empty, font);
                                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                                ordersTable.AddCell(cellProductItem);

                                cellProductItem = GetPdfCell(item.Cost.ToString() ?? string.Empty, font);
                                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                                ordersTable.AddCell(cellProductItem);

                                cellProductItem = GetPdfCell(item.Qty.ToString() ?? string.Empty, font);
                                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                                ordersTable.AddCell(cellProductItem);

                                cellProductItem = GetPdfCell(item.Rem.ToString() ?? string.Empty, font);
                                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                                ordersTable.AddCell(cellProductItem);

                                cellProductItem = GetPdfCell(item.TotalCost.ToString() ?? string.Empty, font);
                                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                                ordersTable.AddCell(cellProductItem);
                            }
                        }
                    }
                }

                cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.TotalSales", lang, fontBold);
                cellProductItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell("", fontBold);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(totalAlloc.ToString(), fontBold);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell("", fontBold);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(totalSold.ToString(), fontBold);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(totalRem.ToString(), fontBold);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                var price = await _priceFormatter.FormatPriceAsync(totalValue, false, await _workContext.GetWorkingCurrencyAsync(), lang.Id, true);
                cellProductItem = GetPdfCell(price.ToString(), fontBold);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                doc.Add(ordersTable);
                doc.Add(new Paragraph(" "));
            }
        }
    }

    #endregion

    #region Event Door Report

    /// <summary>
    /// Print a event door report to PDF
    /// </summary>
    /// <param name="fileName">the fileName for the pdf that is procuded</param>
    /// <param name="orderItems">List of orderItems to display on the report</param>
    /// <param name="languageId">Language</param>
    /// <param name="store">Store for the sale of the event ticket products</param>
    /// <param name="vendor">Vendor identifier to limit products; null to print all products</param>/// 
    /// <param name="manufacturer">Manufacturer identifier to limit products; null to print all products</param>/// 
    /// <returns>A path of generated file</returns>
    public virtual async Task<(string, string)> PrintEventDoorReportToPDFAsync(Product product, int languageId = 0,
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        var dateTime = DateTime.Now;
        string dateTimeTxt = String.Format("{0:dd-MM-y}", dateTime);
        string fileName = $"Event Door Report - {product.Name} {dateTimeTxt}.pdf";
        var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/exportimport"), fileName);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await PrintEventDoorReportAsync(fileStream, product, languageId, store, vendor);
        }

        return (filePath, fileName);
    }

    /// <summary>
    ///     Print event product summary report
    /// </summary>
    /// <param name="orderCollection">Order collection</param>
    /// <param name="filePath">File path</param>
    /// <remarks>[ncc-006]</remarks>
    public virtual async Task<bool> PrintEventDoorReportAsync(Stream stream, Product product, int languageId = 0,
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        //by default _pdfSettings contains settings for the current active store
        //and we need PdfSettings for the store which was used to place an order
        //so let's load it based on a store of the current order
        var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(store.Id);

        var pageSize = PageSize.A4;
        if (pdfSettingsByStore.LetterPageSizeEnabled)
        {
            pageSize = PageSize.Letter;
        }

        var doc = new Document(pageSize);
        var pdfWriter = PdfWriter.GetInstance(doc, stream);
        doc.Open();
        doc.SetMargins(36, 36, 36, 40);

        pdfWriter.PageEvent = new PageHeaderFooter();

        //fonts
        var font = GetFont(_eventTicketSettings.FontFileName);
        var titleFont = GetFont(_eventTicketSettings.FontFileName);
        var attributesFont = GetFont(_eventTicketSettings.FontFileName);
        var fontBold = GetFont(_eventTicketSettings.FontFileName);

        titleFont.SetStyle(Font.BOLD);
        titleFont.Color = BaseColor.Black;
        titleFont.Size = 16;

        attributesFont.SetStyle(Font.ITALIC);
        
        fontBold.SetStyle(Font.BOLD);

        // var ordCount = orderItems.Count;
        var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? 1 : languageId);

        if (lang == null || !lang.Published)
            lang = await _workContext.GetWorkingLanguageAsync();

        FooterDetails = "Address";
        if (store != null)
            FooterDetails = store.Name + " - ";
        if (vendor != null)
            FooterDetails = vendor.Name + " - ";
        if (manufacturer != null)
            FooterDetails = manufacturer.Name + " - ";

        FooterDetails += await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Address", lang.Id);

        //header
        await PrintReportHeaderAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Name",
            "Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Details",
            pdfSettingsByStore, lang, attributesFont, titleFont, doc, store);

        var pte = await _eventTicketService.GetEventTicketProductTypeExtensionAsync(product);
        if (pte != null)
        {
            var orderTicketItems = new List<OrderTicketItem>();
            var productTicketVariants = new List<Product>();

            if (product.ProductType == ProductType.SimpleProduct)
                productTicketVariants.Add(product);
            else if (product.ProductType == ProductType.GroupedProduct)
            {
                var pvs = await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true);
                productTicketVariants.AddRange(pvs);
            }

            foreach (var ptv in productTicketVariants)
            {
                var opv = await _eventTicketService.GetAllOrderProductVariantsSummaryAsync(0, ptv.Id, null, null, true);
                orderTicketItems.AddRange(opv);
            }

            var headerTable = new PdfPTable(2);
            headerTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
            headerTable.DefaultCell.BorderWidthBottom = 0.5f;

            headerTable.WidthPercentage = 100f;

            var cellHeader = GetPdfCell(string.Format(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Details", lang.Id), ""), titleFont);
            cellHeader.Border = Rectangle.NO_BORDER;
            headerTable.AddCell(cellHeader);

            cellHeader = PrintQRCodeInfo(lang, product.Id.ToString(), titleFont, font, headerTable);
            cellHeader.Rowspan = 5;
            cellHeader.Padding = 0;
            cellHeader.PaddingLeft = 50;
            headerTable.AddCell(cellHeader);

            cellHeader = await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.EventName", product.Name, lang, font);
            headerTable.AddCell(cellHeader);

            cellHeader = await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Date",
                await _apolloCoreService.GetLocalisedDateAsync(pte.AvailabilityStartDate, lang), lang, font);
            headerTable.AddCell(cellHeader);

            if (vendor == null)
            {
                vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor != null)
                {
                    cellHeader = await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Venue", vendor.Name, lang, font);
                    headerTable.AddCell(cellHeader);
                }
            }

            cellHeader = await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Doors", pte.ProductTime, lang, font);
            headerTable.AddCell(cellHeader);

            doc.Add(headerTable);
            doc.Add(new Paragraph(" "));

            var productCollection = await AddDoorReportProductCollectionAsync(doc, product, orderTicketItems, lang, titleFont, font, attributesFont);

            doc.NewPage();
            doc.Close();

            return true;
        }

        return false;
    }

    protected virtual async Task<List<Product>> AddDoorReportProductCollectionAsync(Document doc, Product product, IList<OrderTicketItem> orderTicketItems, Language lang,
        Font titleFont, Font font, Font attributesFont, int storeId = 0, int vendorId = 0)
    {

        var ticketCollection = new List<dynamic>();
        var orderTicketItemsList = await orderTicketItems.SelectAwait(async x => new
        {
            ProductName = x.Product.Name,
            OrderCustomerFullName = await _customerService.GetCustomerFullNameAsync(await _customerService.GetCustomerByIdAsync(x.Order.CustomerId)),
            OrderAdditionalNames = await _eventTicketService.GetCustomerAdditionalNameDetailsAsync(x.OrderItemId),
            x.Quantity,
            OrderTransactionId = x.Order.AuthorizationTransactionId == null ? "" : x.Order.AuthorizationTransactionId,
            OrderId = x.Order.Id,
            x.Product.Sku
        }).ToListAsync();

        ticketCollection.AddRange(orderTicketItemsList);

        var salesSummaryTable = new PdfPTable(2);
        salesSummaryTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
        salesSummaryTable.DefaultCell.BorderWidthBottom = 0.5f;
        salesSummaryTable.WidthPercentage = 70f;
        salesSummaryTable.SetWidths(new[] { 5, 2 });

        var cellHeader = GetPdfCell(string.Format(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TicketSalesSummary", lang.Id), ""), titleFont);
        cellHeader.Border = Rectangle.BOTTOM_BORDER;
        salesSummaryTable.AddCell(cellHeader);

        cellHeader = GetPdfCell("", titleFont);
        cellHeader.Border = Rectangle.BOTTOM_BORDER;
        salesSummaryTable.AddCell(cellHeader);

        foreach (string name in ticketCollection.Select(x => x.ProductName).Distinct())
        {
            var tmp = ticketCollection.Where(x => x.ProductName.Equals(name));

            cellHeader = await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TicketType", name, lang, font);
            cellHeader.Border = Rectangle.BOTTOM_BORDER;
            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            salesSummaryTable.AddCell(cellHeader);

            int quantity = tmp.Sum(x => x.Quantity);
            cellHeader = GetPdfCell(quantity.ToString(), titleFont);
            cellHeader.Border = Rectangle.BOTTOM_BORDER;
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            salesSummaryTable.AddCell(cellHeader);
        }

        cellHeader = GetPdfCell(string.Format(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TotalTicketSales", lang.Id), ""), font);
        cellHeader.Border = Rectangle.NO_BORDER;
        cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
        salesSummaryTable.AddCell(cellHeader);

        int totalQuantity = ticketCollection.Sum(x => x.Quantity);
        cellHeader = GetPdfCell(totalQuantity.ToString(), font);
        cellHeader.Border = Rectangle.NO_BORDER;
        cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;

        salesSummaryTable.AddCell(cellHeader);

        doc.Add(salesSummaryTable);
        doc.Add(new Paragraph(" "));

        var ordersTable = new PdfPTable(6);
        ordersTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
        ordersTable.DefaultCell.BorderWidthBottom = 0.5f;

        ordersTable.WidthPercentage = 100f;
        ordersTable.SetWidths(new[] { 5, 4, 4, 1.5f, 2, 2f });

        font.SetStyle(Font.BOLD);

        var cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Purchaser", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TicketTypes", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.User", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Qty", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TransactionId", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Order", lang, font);
        cellProductItem.BackgroundColor = BaseColor.LightGray;
        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
        ordersTable.AddCell(cellProductItem);

        font.SetStyle(Font.NORMAL);

        ordersTable.DefaultCell.Border = Rectangle.NO_BORDER;

        foreach (var purchaser in ticketCollection.Select(x => x.OrderCustomerFullName).Distinct().OrderBy(x => x))
        {

            foreach (var oti in ticketCollection.Where(x => x.OrderCustomerFullName.Equals(purchaser)))
            {

                cellProductItem = GetPdfCell(oti.OrderCustomerFullName ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(oti.ProductName ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(oti.OrderAdditionalNames ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(oti.Quantity.ToString() ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(oti.OrderTransactionId.ToString() ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(oti.OrderId.ToString() ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                ordersTable.AddCell(cellProductItem);

            }
        }

        doc.Add(ordersTable);
        doc.Add(new Paragraph(" "));

        return null;
    }

    private async Task AddOrderDetailsCollectionAsync(Document doc, List<Order> orders, Language lang, Font font)
    {
        var ordersCollection = orders.SelectAwait(async x => new
        {
            Id = x.Id,
            OrderName = GetOrderName(x),
            PaidDate = await GetOrderPaidDateAsync(x, lang),
            ReceiptNo = GetOrderReceiptNo(x),
            Name = GetPaymentMethod(x),
            FriendlyName = GetPaymentMethodFriendlyName(x),
            Price = GetOrderTotalValue(x)
        }).ToListAsync();

        var groupOrdersCollection = (await ordersCollection).GroupBy(x => x.Name);

        foreach (var items in groupOrdersCollection)
        {
            var ordersTable = new PdfPTable(1);
            ordersTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
            ordersTable.DefaultCell.BorderWidthBottom = 0.5f;

            ordersTable.WidthPercentage = 100f;
            ordersTable.SetWidths(new[] { 2, 4, 4, 4, 4 });

            //Order Id

            var cellProductItem = await GetPdfCellAsync("Admin.Orders.OrderIDColumn", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            //Payment Date Column

            cellProductItem = await GetPdfCellAsync("Admin.Orders.PaymentDateColumn", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            //Reciept No 

            cellProductItem = await GetPdfCellAsync("Admin.Orders.RecieptNo", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            // Payment Method

            cellProductItem = await GetPdfCellAsync("Admin.SoldProducts.Products.Paymentmethod", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            // Order Total

            cellProductItem = await GetPdfCellAsync("Admin.Orders.OrderTotalColumn", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            foreach (var item in items)
            {

                // Payment Status                
                cellProductItem = GetPdfCell(item.Id, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_MIDDLE;
                ordersTable.AddCell(cellProductItem);

                // Payment Status                
                cellProductItem = GetPdfCell(item.PaidDate, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_MIDDLE;
                ordersTable.AddCell(cellProductItem);

                // Payment Status                
                cellProductItem = GetPdfCell(item.ReceiptNo, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_MIDDLE;
                ordersTable.AddCell(cellProductItem);

                // Payment Status                
                cellProductItem = GetPdfCell(item.FriendlyName, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_MIDDLE;
                ordersTable.AddCell(cellProductItem);


                // Payment Status
                var price = await _priceFormatter.FormatPriceAsync(item.Price, false, await _workContext.GetWorkingCurrencyAsync(), lang.Id, true);
                cellProductItem = GetPdfCell(price, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_MIDDLE;
                ordersTable.AddCell(cellProductItem);

            }

            doc.Add(ordersTable);

            var total = await _priceFormatter.FormatPriceAsync(items.Sum(x => x.Price), false, await _workContext.GetWorkingCurrencyAsync(), lang.Id, true); //ToDo

            var pEx = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Sub-Total", lang.Id)} {items.Count().ToString()}", font);
            pEx.HorizontalAlignment = Element.ALIGN_RIGHT;
            pEx.Border = Rectangle.NO_BORDER;
            ordersTable.AddCell(pEx);
        }
    }

    #endregion

    #region Event Settlement Report

    /// <summary>
    /// Print a event summary report to PDF
    /// </summary>
    /// <param name="fileName">the fileName for the pdf that is procuded</param>
    /// <param name="orderItems">List of orderItems to display on the report</param>
    /// <param name="languageId">Language</param>
    /// <param name="store">Store for the sale of the event ticket products</param>
    /// <param name="vendor">Vendor identifier to limit products; null to print all products</param>/// 
    /// <param name="manufacturer">Manufacturer identifier to limit products; null to print all products</param>/// 
    /// <returns>A path of generated file</returns>
    public async virtual Task<(string, string)> PrintEventSettlementReportToPDFAsync(Product product, int languageId = 0,
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        var dateTime = DateTime.Now;
        string dateTimeTxt = String.Format("{0:dd-MM-y}", dateTime);
        string fileName = $"summary_report_{store.Name}_{dateTimeTxt}.pdf";
        var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/exportimport"), fileName);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await PrintEventSettlementReportAsync(fileStream, product, languageId, store, vendor);
        }

        return (filePath, fileName);
    }

    /// <summary>
    ///     Print event product summary report
    /// </summary>
    /// <param name="orderCollection">Order collection</param>
    /// <param name="filePath">File path</param>
    /// <remarks>[ncc-006]</remarks>
    public async virtual Task<bool> PrintEventSettlementReportAsync(Stream stream, Product product, int languageId = 0,
        Store store = null, Vendor vendor = null, Manufacturer manufacturer = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        //by default _pdfSettings contains settings for the current active store
        //and we need PdfSettings for the store which was used to place an order
        //so let's load it based on a store of the current order
        var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(store.Id);

        var pageSize = PageSize.A4;
        if (pdfSettingsByStore.LetterPageSizeEnabled)
        {
            pageSize = PageSize.Letter;
        }

        var doc = new Document(pageSize);
        var pdfWriter = PdfWriter.GetInstance(doc, stream);
        doc.Open();
        doc.SetMargins(36, 36, 36, 40);

        pdfWriter.PageEvent = new PageHeaderFooter();

        //fonts

        var font = GetFont(_eventTicketSettings.FontFileName);
        var attributesFont = GetFont(_eventTicketSettings.FontFileName);
        var titleFont = GetFont(_eventTicketSettings.FontFileName);

        titleFont.SetStyle(Font.BOLD);
        titleFont.Color = BaseColor.Black;
        titleFont.Size = 16;
                
        attributesFont.SetStyle(Font.ITALIC);

        var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? 1 : languageId);

        if (lang == null || !lang.Published)
            lang = await _workContext.GetWorkingLanguageAsync();

        FooterDetails = "Address";
        if (store != null)
            FooterDetails = store.Name + " - ";
        if (vendor != null)
            FooterDetails = vendor.Name + " - ";
        if (manufacturer != null)
            FooterDetails = manufacturer.Name + " - ";

        FooterDetails += await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Address", lang.Id);

        //header
        await PrintReportHeaderAsync("Plugin.Apollo.EventTickets.Admin.PdfSettlementReport.Name",
            "Plugin.Apollo.EventTickets.Admin.PdfSettlementReport.Details",
            pdfSettingsByStore, lang, attributesFont, titleFont, doc, store);

        var pte = await _eventTicketService.GetEventTicketProductTypeExtensionAsync(product);
        if (pte != null)
        {

            var headerTable = new PdfPTable(2);
            headerTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
            headerTable.DefaultCell.BorderWidthBottom = 0.5f;

            headerTable.WidthPercentage = 100f;

            var cellHeader = GetPdfCell(string.Format(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Details", lang.Id), ""), titleFont);
            cellHeader.Border = Rectangle.NO_BORDER;
            headerTable.AddCell(cellHeader);

            cellHeader = PrintQRCodeInfo(lang, product.Id.ToString(), titleFont, font, headerTable);
            cellHeader.Rowspan = 5;
            cellHeader.Padding = 0;
            cellHeader.PaddingLeft = 50;
            headerTable.AddCell(cellHeader);

            cellHeader = await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.EventName", product.Name, lang, font);
            headerTable.AddCell(cellHeader);

            cellHeader = await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Date",
                await _apolloCoreService.GetLocalisedDateTimeAsync(pte.AvailabilityStartDate, lang, true), lang, font);
            headerTable.AddCell(cellHeader);

            if (vendor != null)
            {
                cellHeader = await GetParagraphAsync("Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Venue", vendor.Name, lang, font);
                headerTable.AddCell(cellHeader);
            }

            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));

            doc.Add(headerTable);
            doc.Add(new Paragraph(" "));

            await AddProductSettlementAsync(doc, product, vendor, lang, font, titleFont, store.Id);

            doc.NewPage();
            doc.Close();

            return true;
        }

        return false;
    }

    protected async virtual Task AddProductSettlementAsync(Document doc, Product product, Vendor vendor, Language lang, Font font, Font titleFont, int storeId = 0)
    {

        var fontBold = GetFont(_eventTicketSettings.FontFileName);
        fontBold.SetStyle(Font.BOLD);

        var headerTable = new PdfPTable(1);
        headerTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
        headerTable.DefaultCell.BorderWidthBottom = 0.5f;

        headerTable.WidthPercentage = 100f;

        var cellHeader = GetPdfCell(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfSettlementReport.Breakdown", lang.Id), titleFont);
        cellHeader.Border = Rectangle.NO_BORDER;
        cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
        headerTable.AddCell(cellHeader);

        doc.Add(headerTable);
        doc.Add(new Paragraph(" "));

        var pte = await _eventTicketService.GetEventTicketProductTypeExtensionAsync(product);
        if (pte != null)
        {
            var productTicketVariants = new List<Product>();

            if (product.ProductType == ProductType.SimpleProduct)
                productTicketVariants.Add(product);
            else if (product.ProductType == ProductType.GroupedProduct)
            {
                var pvs = await _productService.GetAssociatedProductsAsync(product.Id, storeId, showHidden: true);
                productTicketVariants.AddRange(pvs);
            }

            var ordersTable = new PdfPTable(5);
            ordersTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
            ordersTable.DefaultCell.BorderWidthBottom = 0.5f;

            ordersTable.WidthPercentage = 100f;
            ordersTable.SetWidths(new[] { 10, 5, 2, 1.5f, 3 });

            var cellProductItem = GetPdfCell(string.Format("{0} - Ticket Type", product.Name), fontBold);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Outlet", lang, fontBold);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Cost", lang, fontBold);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Sold", lang, fontBold);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Total", lang, fontBold);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            decimal totalValue = Decimal.Zero;
            int totalSold = 0;
            string total = string.Empty;

            foreach (var ptv in productTicketVariants)
            {
                var opvCollection = new List<OrderTicketItem>();

                var opvcOnline = await _eventTicketService.GetAllOrderProductVariantsSummaryAsync(0, ptv.Id, null, null, true);
                if (opvcOnline.Count > 0)
                {
                    var dtNow = DateTime.UtcNow;
                    opvCollection.AddRange(opvcOnline);

                    var tmpCollection = await opvCollection.OrderBy(x => x.Product.Name).GroupBy(x => x.Outlet).SelectAwait(async x => new
                    {
                        Name = x.First().Product.Name,
                        Outlet = x.First().Outlet,
                        Cost = await GetProductVariantUnitProductCostForReportAsync(x.First()),
                        Price = await GetProductVariantUnitPriceAsync(x.First()),
                        Qty = GetOrderQuantity(x),
                        TotalCost = await GetReportLineTotalCostAsync(x),
                        Total = await GetOrderProductVariantSubTotalAsync(x)
                    }).ToListAsync();

                    foreach (var group in tmpCollection.GroupBy(x => x.Name))
                    {
                        bool first = true;
                        foreach (var item in group)
                        {
                            string value = string.Empty;
                            (value, total) = await GetOrderTotalProductCostAsync(opvCollection);
                            totalValue += decimal.Parse(total);
                            totalSold += item.Qty;

                            if (first)
                                cellProductItem = GetPdfCell(item.Name ?? string.Empty, font);
                            else
                                cellProductItem = GetPdfCell(string.Empty ?? string.Empty, font);

                            cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                            ordersTable.AddCell(cellProductItem);

                            cellProductItem = GetPdfCell(item.Outlet.ToString() ?? string.Empty, font);
                            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                            ordersTable.AddCell(cellProductItem);

                            cellProductItem = GetPdfCell(item.Cost.ToString() ?? string.Empty, font);
                            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                            ordersTable.AddCell(cellProductItem);

                            cellProductItem = GetPdfCell(item.Qty.ToString() ?? string.Empty, font);
                            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                            ordersTable.AddCell(cellProductItem);

                            cellProductItem = GetPdfCell(item.TotalCost.ToString() ?? string.Empty, font);
                            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                            ordersTable.AddCell(cellProductItem);
                        }
                    }
                }
            }

            cellProductItem = await GetPdfCellAsync("Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.TotalSales", lang, fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("", fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("", fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell(totalSold.ToString(), fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            var price = await _priceFormatter.FormatPriceAsync(totalValue, false, await _workContext.GetWorkingCurrencyAsync(), lang.Id, true);
            cellProductItem = GetPdfCell(price.ToString(), fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            string abn = await _apolloCoreService.GetVendorABNAsync(vendor);
            string gst = "Includes GST of " + await _priceFormatter.FormatPriceAsync(totalValue * 1 / 11);
            string rctiText = string.Format(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfSettlementReport.RCTI", lang.Id), vendor.Name, abn, gst);

            cellProductItem = GetPdfCell("", font);
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase("  " + rctiText, fontBold));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));

            Phrase phrase = new Phrase("", font);
            Chunk chunk = new Chunk("  For: ", fontBold);
            phrase.Add(chunk);
            chunk = new Chunk(vendor.Name, font);
            phrase.Add(chunk);
            cellProductItem.Phrase.Add(phrase);
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));

            phrase = new Phrase("", font);
            chunk = new Chunk("  ABN: ", fontBold);
            phrase.Add(chunk);
            chunk = new Chunk(abn, font);
            phrase.Add(chunk);
            cellProductItem.Phrase.Add(phrase);
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));

            phrase = new Phrase("", font);
            chunk = new Chunk("  Amount: ", fontBold);
            phrase.Add(chunk);
            chunk = new Chunk(gst, font);
            phrase.Add(chunk);
            cellProductItem.Phrase.Add(phrase);
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));

            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("", font);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("", font);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("", fontBold);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            ordersTable.AddCell(cellProductItem);

            price = await _priceFormatter.FormatPriceAsync(totalValue, false, await _workContext.GetWorkingCurrencyAsync(), lang.Id, true);
            cellProductItem = GetPdfCell("", fontBold);
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellProductItem.Phrase.Add(new Phrase(price.ToString()));
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;

            ordersTable.AddCell(cellProductItem);

            doc.Add(ordersTable);
            doc.Add(new Paragraph(" "));
        }
    }

    #endregion

    #region Report Utilities

    public string GetOrderName(Order order)
    {
        return order.Id.ToString();
    }

    public async Task<string> GetOrderPaidDateAsync(Order order, Language lang)
    {
        string result = await _apolloCoreService.GetLocalisedDateAsync(DateTime.UtcNow, lang); 
        if (order != null)
        {
            var date = order.PaidDateUtc != null ? order.PaidDateUtc : DateTime.UtcNow;
            result = await _apolloCoreService.GetLocalisedDateAsync((DateTime)date, lang);
        }
        return result;
    }

    public string GetOrderReceiptNo(Order order)
    {
        string result = order.CaptureTransactionResult;
        return result;
    }

    public string GetPaymentMethod(Order order)
    {
        return order.PaymentMethodSystemName;
    }

    public string GetPaymentMethodFriendlyName(Order order)
    {
        if (order.PaymentMethodSystemName != null)
            return ("Friendly name");
        else
            return "Not via web";
    }

    public decimal GetOrderTotalValue(Order order)
    {
        return order.OrderTotal;
    }

    public string GetProductName(OrderTicketItem oti)
    {
        return oti.Product.Name;
    }

    public int GetOrderAllocationValue(IEnumerable<OrderTicketItem> opvs)
    {
        var pv = opvs.Select(x => x.Product).First();
        if (opvs.Select(x => x.Quantity).First() == 0)
            return pv.StockQuantity;
        else
            return Int32.Parse(GetProductVariantAllocation(pv));
    }

    public string GetProductVariantAllocation(Product pv)
    {
        int totalQty = 0;
        totalQty += pv.StockQuantity +
            _eventTicketService.GetProductVariantOrderedTotal(pv.Id, null, null, null, OrderStatus.Complete, null, null) +
            _eventTicketService.GetProductVariantOrderedTotal(pv.Id, null, null, null, OrderStatus.Processing, null, null) +
            _eventTicketService.GetProductVariantOrderedTotal(pv.Id, null, null, null, OrderStatus.Pending, null, null);

        return totalQty.ToString();
    }

    public string GetOrderRemaining(IEnumerable<OrderTicketItem> opvs)
    {
        var pv = opvs.Select(x => x.Product).First();
        if (opvs.Select(x => x.Quantity).First() != 0)
            return String.Format("{0}", pv.StockQuantity);
        else
            return GetProductVariantAllocation(pv);
    }

    public int GetOrderRemainingValue(IEnumerable<OrderTicketItem> opvs)
    {
        var pv = opvs.Select(x => x.Product).First();
        if (opvs.Select(x => x.Quantity).First() != 0)
            return pv.StockQuantity;
        else
            return Int32.Parse(GetProductVariantAllocation(pv));
    }

    public static int GetOrderQuantity(IEnumerable<OrderTicketItem> opvs)
    {
        return opvs.Sum(x => x.Quantity);
    }

    public async Task<string> GetProductVariantUnitProductCostForReportAsync(OrderTicketItem oti)
    {
        string result = String.Empty;
        var pv = oti.Product;
        result = await _priceFormatter.FormatPriceAsync(pv.ProductCost, false, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, true);
        return result;
    }

    public async Task<string> GetProductVariantUnitPriceAsync(OrderTicketItem opv)
    {
        string result = String.Empty;
        var pv = opv.Product;
        result = await _priceFormatter.FormatPriceAsync(pv.ProductCost, false, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, true);
        return result;
    }

    public async Task<(string, string)> GetOrderTotalProductCostAsync(List<OrderTicketItem> opvCollection)
    {
        string result = String.Empty;
        decimal totalAmount = Decimal.Zero;
        decimal salesAmountSubTotal = Decimal.Zero;

        foreach (var opv in opvCollection)
        {
            salesAmountSubTotal = opv.Product.ProductCost * opv.Quantity;
            totalAmount += salesAmountSubTotal;
        }

        string value = totalAmount.ToString();

        result = String.Format("{0}", await _priceFormatter.FormatPriceAsync(totalAmount, false, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, true));
        return (result, value);
    }

    public decimal GetOrderTotalPriceInclTax(List<OrderTicketItem> opvCollection)
    {
        string result = String.Empty;
        decimal totalAmount = Decimal.Zero;
        decimal salesAmountSubTotal = Decimal.Zero;

        foreach (var opv in opvCollection)
        {
            salesAmountSubTotal = opv.PriceInclTax;
            totalAmount += salesAmountSubTotal;
        }

        return totalAmount;
    }

    public async Task<string> GetReportLineTotalCostAsync(IEnumerable<OrderTicketItem> opvs)
    {
        string result = String.Empty;
        decimal salesAmountSubTotal = 0;
        var pv = opvs.First();
        salesAmountSubTotal = pv.Product.ProductCost * opvs.Sum(x => x.Quantity);
        result = String.Format("{0}", await _priceFormatter.FormatPriceAsync(salesAmountSubTotal, false, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, true));
        return result;
    }

    public async Task<string> GetOrderProductVariantSubTotalAsync(IEnumerable<OrderTicketItem> opvs)
    {
        string result = String.Empty;
        var pv = opvs.First();
        result = String.Format("{0}", await _priceFormatter.FormatPriceAsync(pv.Product.Price, false, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, true, true));
        return result;
    }

    #endregion

}
