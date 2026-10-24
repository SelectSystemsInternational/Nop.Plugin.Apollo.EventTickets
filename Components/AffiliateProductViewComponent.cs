using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Affiliates;

using Nop.Services.Directory;
using Nop.Services.Common;
using Nop.Services.Logging;

using Nop.Plugin.Apollo.EventTickets.Factories;
using Nop.Plugin.Apollo.EventTickets.Models;
using Nop.Plugin.Apollo.EventTickets.Services;


namespace Nop.Plugin.Apollo.EventTickets.Components;

public class AffiliateProductViewComponent: NopViewComponent
{

    protected readonly IWorkContext _workContext;
    protected readonly ILocalizationService _localizationService;
    protected readonly IStoreContext _storeContext;
    protected readonly ICountryService _countryService;
    protected readonly IAddressService _addressService;
    protected readonly ILogger _logger;
    protected readonly OrderSettings _orderSettings;

    protected readonly IEventTicketService _eventTicketService;
    protected readonly IEventTicketsModelFactory _eventTicketsModelFactory;

    public AffiliateProductViewComponent(IWorkContext workContext,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        ICountryService countryService,
        IAddressService addressService,
        ILogger logger,
        OrderSettings orderSettings,
        IEventTicketService eventTicketService,
        IEventTicketsPdfService eventTicketsPdfService,
        IEventTicketsModelFactory eventTicketsModelFactory)
    {
        _workContext = workContext;
        _localizationService = localizationService;
        _storeContext = storeContext;
        _countryService = countryService;
        _addressService = addressService;
        _logger = logger;
        _orderSettings = orderSettings;
        _eventTicketService = eventTicketService;            
        _eventTicketsModelFactory = eventTicketsModelFactory;
    }

    /// <returns>A task that represents the asynchronous operation</returns>
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (!(additionalData is AffiliateModel affiliateModel)) return Content("");

        var searchModel = new ExtendedAffiliateProductSearchModel
        {
            SearchProductId = affiliateModel.Id,
        };

        searchModel.SetGridPageSize();

        return View("~/Plugins/SSI.Apollo.EventTickets/Views/Admin.AffilfiateProducts.cshtml", searchModel);
    }
}
