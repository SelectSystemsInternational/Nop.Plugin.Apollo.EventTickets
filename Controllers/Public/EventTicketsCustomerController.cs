using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Plugin.Apollo.EventTickets.Models;
using Nop.Plugin.Apollo.EventTickets.Services;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Factories;

namespace Nop.Plugin.Apollo.EventTickets.Controllers.Public;

public partial class EventTicketsCustomerController : Web.Controllers.CustomerController
{

    #region Fields

    protected readonly IEventTicketService _eventTicketService;

    #endregion

    #region Ctor

    public EventTicketsCustomerController(AddressSettings addressSettings,
        CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        IDownloadService downloadService,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        HtmlEncoder htmlEncoder,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        IAuthenticationService authenticationService,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ICustomerModelFactory customerModelFactory,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IExternalAuthenticationService externalAuthenticationService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        ILocalizationService localizationService,
        ILogger logger,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        INotificationService notificationService,
        IOrderService orderService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IShoppingCartService shoppingCartService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        ITaxService taxService,
        IWebHelper webHelper,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings,
        MediaSettings mediaSettings,
        MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
        StoreInformationSettings storeInformationSettings,
        TaxSettings taxSettings,
        IEventTicketService eventTicketService) : base(addressSettings,
            captchaSettings,
            customerSettings,
            dateTimeSettings,
            forumSettings,
            gdprSettings,
            htmlEncoder,
            addressModelFactory,
            addressService,
            addressAttributeParser,
            customerAttributeParser,
            customerAttributeService,
            authenticationService,
            countryService,
            currencyService,
            customerActivityService,
            customerModelFactory,
            customerRegistrationService,
            customerService,
            downloadService,
            eventPublisher,
            exportManager,
            externalAuthenticationService,
            gdprService,
            genericAttributeService,
            giftCardService,
            localizationService,
            logger,
            multiFactorAuthenticationPluginManager,
            newsLetterSubscriptionService,
            notificationService,
            orderService,
            permissionService,
            pictureService,
            priceFormatter,
            productService,
            stateProvinceService,
            storeContext,
            taxService,
            workContext,
            workflowMessageService,
            localizationSettings,
            mediaSettings,
            multiFactorAuthenticationSettings,
            storeInformationSettings,
            taxSettings)
    {
        _eventTicketService = eventTicketService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> EventTicketsCustomerCalendar()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        var model = new EventTicketsCustomerCalendarModel();

        model.EventId = 0;

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/FrontView/EventTicketsCustomerCalendar.cshtml", model);
    }

    [IgnoreAntiforgeryToken]
    [HttpPost, ActionName("GoToCustomerOrder")]
    public async Task<IActionResult> GoToCustomerOrder(int id)
    {
        if (id == 0)
            return Json(new { Success = false });

        var orderitem = await _orderService.GetOrderItemByIdAsync(id);
        if (orderitem == null)
            return Json(new { Success = false });
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(orderitem.OrderId);        
        if (order == null)
            return Json(new { Success = false });

        return Json(new { Id = order.Id });
    }

    #endregion

}