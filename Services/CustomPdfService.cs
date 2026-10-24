using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
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

using iTextSharp.text;
using iTextSharp.text.pdf;

using Nop.Plugin.Apollo.Core.Services;
using Nop.Plugin.Apollo.Core.Settings;
using Nop.Plugin.Apollo.EventTickets.Settings;


namespace Nop.Plugin.Apollo.EventTickets.Services;

public class CustomPdfService : ApolloPdfService
{

    #region Fields

    protected readonly EventTicketSettings _eventTicketSettings;
    protected readonly IProductTypeExtensionsService _productTypeExtensionsService;

    #endregion

    #region Ctor

    public CustomPdfService(AddressSettings addressSettings,
        CatalogSettings catalogSettings,
        CurrencySettings currencySettings,
        IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
        IAddressService addressService,
        ICountryService countryService,
        ICurrencyService currencyService,
        IDateTimeHelper dateTimeHelper,
        IGiftCardService giftCardService,
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
        MeasureSettings measureSettings,
        PdfSettings pdfSettings,
        TaxSettings taxSettings,
        VendorSettings vendorSettings,      
        ICustomerService customerService,
        ITopicService topicService,
        IWebHelper webHelper,
        IHtmlFormatter htmlFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductAttributeFormatter productAttributeFormatter,
        IApolloCoreService apolloCoreService,
        ApolloCoreSettings apolloCoreSettings,
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
        _eventTicketSettings = eventTicketSettings;
        _productTypeExtensionsService = productTypeExtensionsService;
    }

    #endregion

    #region Utilities

    #endregion

    #region Methods

    /// <summary>
    /// Write ZIP archive with invoices to the specified stream
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="orders">Orders</param>
    /// <param name="language">Language; null to use a language used when placing an order</param>
    /// <param name="vendor">Vendor to limit products; null to print all products. If specified, then totals won't be printed</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    //public override async Task PrintOrdersToPdfAsync(Stream stream, IList<Order> orders, Language language = null, Vendor vendor = null)
    //{
    //    ArgumentNullException.ThrowIfNull(stream);

    //    ArgumentNullException.ThrowIfNull(orders);

    //    var currentStore = await _storeContext.GetCurrentStoreAsync();

    //    var pageSize = PageSize.A4;

    //    //by default _pdfSettings contains settings for the current active store
    //    //and we need PdfSettings for the store which was used to place an order
    //    //so let's load it based on a store of the current order
    //    var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(currentStore.Id);

    //    if (pdfSettingsByStore.LetterPageSizeEnabled)
    //    {
    //        pageSize = PageSize.Letter;
    //    }

    //    var doc = new Document(pageSize);
    //    var pdfWriter = PdfWriter.GetInstance(doc, stream);
    //    doc.Open();

    //    //fonts
    //    var titleFont = GetFont();
    //    titleFont.SetStyle(Font.BOLD);
    //    titleFont.Color = BaseColor.Black;
    //    var font = GetFont();
    //    var attributesFont = GetFont();
    //    attributesFont.SetStyle(Font.ITALIC);

    //    var ordCount = orders.Count;
    //    var ordNum = 0;

    //    foreach (var order in orders)
    //    {
    //        var entryName = string.Format(await _localizationService.GetResourceAsync("Pdf.Order"), order.CustomOrderNumber);

    //        bool isTicket = false;

    //        var eventTicketService = EngineContext.Current.Resolve<IEventTicketService>();

    //        isTicket = await _apolloCoreService.IsEventTicketOrderAsync(order);

    //        if (!isTicket)
    //            await base.PrintOrdersToPdfAsync(stream, orders, language, vendor);

    //        var lang = await _languageService.GetLanguageByIdAsync(language.Id == 0 ? order.CustomerLanguageId : language.Id);
    //        if (lang == null || !lang.Published)
    //            lang = await _workContext.GetWorkingLanguageAsync();

    //        //header
    //        await PrintHeaderAsync(pdfSettingsByStore, lang, order, font, titleFont, doc);

    //        //addresses
    //        await PrintAddressesAsync(vendor.Id, lang, titleFont, order, font, doc);

    //        await PrintTicketConditionsAsync(doc, lang, titleFont, font);

    //        //a vendor should have access only to products
    //        bool first = true;
    //        var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendor?.Id ?? 0);
    //        foreach (var orderItem in orderItems)
    //        {
    //            if (await _apolloCoreService.IsEventTicketAsync(orderItem.ProductId))
    //            {
    //                if (!first)
    //                    doc.NewPage();

    //                await PrintTicket(pdfSettingsByStore, doc, lang, orderItem, order.AuthorizationTransactionId);
    //                first = false;
    //            }
    //        }

    //        //footer
    //        PrintFooter(pdfSettingsByStore, pdfWriter, pageSize, lang, font);

    //        ordNum++;
    //        if (ordNum < ordCount)
    //        {
    //            doc.NewPage();
    //        }

    //        doc.Close();

    //    }
    //}

    /// <summary>
    /// Print totals
    /// </summary>
    /// <param name="vendorId">Vendor identifier</param>
    /// <param name="lang">Language</param>
    /// <param name="order">Order</param>
    /// <param name="font">Text font</param>
    /// <param name="titleFont">Title font</param>
    /// <param name="doc">PDF document</param>
    protected virtual async Task PrintTotalsAsync(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc)
    {
        //vendors cannot see totals
        if (vendorId != 0)
            return;

        //subtotal
        var totalsTable = new PdfPTable(1)
        {
            RunDirection = GetDirection(lang),
            WidthPercentage = 100f
        };
        totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;

        var languageId = lang.Id;

        var orderSubtotalExclTaxInCustomerCurrency =
            _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
        var orderSubtotalExclTaxStr = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true,
            order.CustomerCurrencyCode, languageId, false);

        var pEx = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Sub-Total", languageId)} {orderSubtotalExclTaxStr}", font);
        pEx.HorizontalAlignment = Element.ALIGN_RIGHT;
        pEx.Border = Rectangle.NO_BORDER;
        totalsTable.AddCell(pEx);

        //discount (applied to order subtotal)
        if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
        {
            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                var orderSubTotalDiscountInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                var orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(
                    -orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax

                var orderSubTotalDiscountExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                var orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(
                    -orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
        }

        //shipping
        if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
        {
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax
                var orderShippingInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                var orderShippingInclTaxStr = await _priceFormatter.FormatShippingPriceAsync(
                    orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Shipping", languageId)} {orderShippingInclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax
                var orderShippingExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                var orderShippingExclTaxStr = await _priceFormatter.FormatShippingPriceAsync(
                    orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Shipping", languageId)} {orderShippingExclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
        }

        //payment fee
        if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
        {
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                var paymentMethodAdditionalFeeInclTaxStr = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                    paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeInclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                var paymentMethodAdditionalFeeExclTaxStr = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                    paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeExclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
        }

        //tax
        var taxStr = string.Empty;
        var taxRates = new SortedDictionary<decimal, decimal>();
        bool displayTax;
        var displayTaxRates = true;
        if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
        {
            displayTax = false;
        }
        else
        {
            if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

                displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                displayTax = !displayTaxRates;

                var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                    false, languageId);
            }
        }

        if (displayTax)
        {
            var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Tax", languageId)} {taxStr}", font);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);
        }

        if (displayTaxRates)
        {
            foreach (var item in taxRates)
            {
                var taxRate = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.TaxRate", languageId),
                    _priceFormatter.FormatTaxRate(item.Key));
                var taxValue = await _priceFormatter.FormatPriceAsync(
                    _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                    false, languageId);

                var p = GetPdfCell($"{taxRate} {taxValue}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
        }

        //discount (applied to order total)
        if (order.OrderDiscount > decimal.Zero)
        {
            var orderDiscountInCustomerCurrency =
                _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
            var orderDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency,
                true, order.CustomerCurrencyCode, false, languageId);

            var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderDiscountInCustomerCurrencyStr}", font);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);
        }

        //gift cards
        foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
        {
            var gcTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.GiftCardInfo", languageId),
                (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode);
            var gcAmountStr = await _priceFormatter.FormatPriceAsync(
                -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
                order.CustomerCurrencyCode, false, languageId);

            var p = GetPdfCell($"{gcTitle} {gcAmountStr}", font);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);
        }

        //reward points
        if (order.RedeemedRewardPointsEntryId.HasValue && 
            await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
        {
            var rpTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.RewardPoints", languageId),
                -redeemedRewardPointsEntry.Points);
            var rpAmount = await _priceFormatter.FormatPriceAsync(
                -_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                true, order.CustomerCurrencyCode, false, languageId);

            var p = GetPdfCell($"{rpTitle} {rpAmount}", font);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);
        }

        //order total
        var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
        var orderTotalStr = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

        var pTotal = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.OrderTotal", languageId)} {orderTotalStr}", titleFont);
        pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
        pTotal.Border = Rectangle.NO_BORDER;
        totalsTable.AddCell(pTotal);

        doc.Add(totalsTable);

    }

    protected virtual async Task PrintTicketConditionsAsync(Document doc, Language lang, Font titleFont, Font bodyFont)
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

    /// <summary>
    /// Print addresses
    /// </summary>
    /// <param name="vendorId">Vendor identifier</param>
    /// <param name="lang">Language</param>
    /// <param name="titleFont">Title font</param>
    /// <param name="order">Order</param>
    /// <param name="font">Text font</param>
    /// <param name="doc">Document</param>
    protected virtual async Task PrintTicket(PdfSettings pdfSettingsByStore, Document doc, Language lang, OrderItem orderItem, string transactionId)
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

    #endregion

}
