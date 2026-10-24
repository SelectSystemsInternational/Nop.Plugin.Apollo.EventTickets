using Microsoft.AspNetCore.Mvc;

using Nop.Core;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;

using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Affiliates;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.EventTickets.Models;
using Nop.Plugin.Apollo.EventTickets.Services;

namespace Nop.Plugin.Apollo.EventTickets.Controllers;

public partial class CustomAffiliateController : AffiliateController
{

    #region Fields

    protected readonly IEventTicketService _eventTicketService;
    protected readonly IEventTicketsPdfService _eventTicketsPdfService;
    protected readonly OrderSettings _orderSettings;
    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public CustomAffiliateController(IAddressService addressService,
        IAffiliateModelFactory affiliateModelFactory,
        IAffiliateService affiliateService,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IEventTicketService eventTicketService,
        IEventTicketsPdfService eventTicketsPdfService,
        OrderSettings orderSettings,
        IWorkContext workContext,
        IStoreContext storeContext,
        IPermissionService permissionService) : base(addressService,
            affiliateModelFactory,
            affiliateService,
            customerActivityService,
            localizationService,
            notificationService,
            permissionService)
    {
        _eventTicketService = eventTicketService;
        _eventTicketsPdfService = eventTicketsPdfService;
        _orderSettings = orderSettings;
        _workContext = workContext;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    public async virtual Task<IActionResult> ListAgent()
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliateSearchModelAsync(new AffiliateSearchModel());

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/Agent/ListAgent.cshtml", model);
    }

    [HttpPost]
    public async virtual Task<IActionResult> ListAgent(AffiliateSearchModel searchModel)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliateListModelAsync(searchModel);

        return Json(model);
    }

    public async virtual Task<IActionResult> CreateAgent()
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliateModelAsync(new AffiliateModel(), null);

        model.Address.CountryRequired = false;
        model.Address.StreetAddressRequired = false;
        model.Address.StreetAddress2Required = false;
        model.Address.CityRequired = false;
        model.Address.ZipPostalCodeRequired = false;
        model.Address.FaxRequired = false;

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/Agent/CreateAgent.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public async virtual Task<IActionResult> CreateAgent(AffiliateModel model, bool continueEditing)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var address = model.Address.ToEntity<Address>();

            address.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            await _addressService.InsertAddressAsync(address);

            var affiliate = model.ToEntity<Affiliate>();

            //validate friendly URL name
            var friendlyUrlName = await _affiliateService.ValidateFriendlyUrlNameAsync(affiliate, model.FriendlyUrlName);
            affiliate.FriendlyUrlName = friendlyUrlName;
            affiliate.AddressId = address.Id;

            await _affiliateService.InsertAffiliateAsync(affiliate);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewAffiliate",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewAffiliate"), affiliate.Id), affiliate);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Affiliates.Added"));

            return continueEditing ? RedirectToAction("EditAgent", new { id = affiliate.Id }) : RedirectToAction("ListAgent");
        }

        //prepare model
        model = await _affiliateModelFactory.PrepareAffiliateModelAsync(model, null, true);

        //model.Address.CountyEnabled = false;
        //model.Address.StateProvinceEnabled = false;
        //model.Address.CountryEnabled = false;
        //model.Address.StreetAddressEnabled = false;
        //model.Address.StreetAddress2Enabled = false;
        //model.Address.CityEnabled = false;
        //model.Address.ZipPostalCodeEnabled = false;
        //model.Address.FaxEnabled = false;

        //if we got this far, something failed, redisplay form
        return View("~/Plugins/SSI.Apollo.EventTickets/Views/Agent/EditAgent.cshtml", model);
    }

    public async virtual Task<IActionResult> EditAgent(int id)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get an affiliate with the specified id
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(id);
        if (affiliate == null || affiliate.Deleted)
            return RedirectToAction("ListAgent");

        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliateModelAsync(null, affiliate);

        //model.Address.CountyEnabled = false;
        //model.Address.StateProvinceEnabled = false;
        //model.Address.CountryEnabled = false;
        //model.Address.StreetAddressEnabled = false;
        //model.Address.StreetAddress2Enabled = false;
        //model.Address.CityEnabled = false;
        //model.Address.ZipPostalCodeEnabled = false;
        //model.Address.FaxEnabled = false;

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/Agent/EditAgent.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async virtual Task<IActionResult> EditAgent(AffiliateModel model, bool continueEditing)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get an affiliate with the specified id
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(model.Id);
        if (affiliate == null || affiliate.Deleted)
            return RedirectToAction("ListAgent");

        if (ModelState.IsValid)
        {
            var address = await _addressService.GetAddressByIdAsync(affiliate.AddressId);
            address = model.Address.ToEntity(address);

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            await _addressService.UpdateAddressAsync(address);

            affiliate = model.ToEntity(affiliate);

            //validate friendly URL name
            var friendlyUrlName = await _affiliateService.ValidateFriendlyUrlNameAsync(affiliate, model.FriendlyUrlName);
            affiliate.FriendlyUrlName = friendlyUrlName;
            affiliate.AddressId = address.Id;

            await _affiliateService.UpdateAffiliateAsync(affiliate);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditAffiliate",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditAffiliate"), affiliate.Id), affiliate);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Affiliates.Updated"));

            if (!continueEditing)
                return RedirectToAction("ListAgent");

            return RedirectToAction("EditAgent", new { id = affiliate.Id });
        }

        //prepare model
        model = await _affiliateModelFactory.PrepareAffiliateModelAsync(model, affiliate, true);

        //if we got this far, something failed, redisplay form
        return View("~/Plugins/SSI.Apollo.EventTickets/Views/Agent/ListAgent.cshtml", model);
    }

    //delete
    [HttpPost]
    public async override Task<IActionResult> Delete(int id)
    {
        if (!await _eventTicketService.AuthorizeAsync(CoreSystemHelper.AccessMode.Manage))
            return AccessDeniedView();

        //try to get an affiliate with the specified id
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(id);
        if (affiliate == null)
            return RedirectToAction("ListAgent");

        await _affiliateService.DeleteAffiliateAsync(affiliate);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteAffiliate",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteAffiliate"), affiliate.Id), affiliate);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Affiliates.Deleted"));

        return RedirectToAction("ListAgent");
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("EditAgent")]
    [FormValueRequired("pdf-report-affiliate-summary")]
    public async virtual Task<IActionResult> PdfAffiliateReportSummary(ExtendedAffiliateProductSearchModel model)
    {
        var events = await _eventTicketService.GetAllAffiliateTicketsOnSaleAsync(0, model.SearchProductId);

        //ensure that we at least one order selected
        if (!events.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"));
            return RedirectToAction("EditAgent");
        }

        try
        {
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _eventTicketsPdfService.PrintSalesSummaryReportAsync(stream, events,
                    _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id,
                    await _storeContext.GetCurrentStoreAsync(), await _workContext.GetCurrentVendorAsync());
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, "EventSummaryReport_" + DateTime.Now.ToShortDateString() + ".pdf");

        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("EditAgent");
        }
    }

    #endregion

}