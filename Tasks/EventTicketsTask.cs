using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.EventTickets.Domain;
using Nop.Plugin.Apollo.EventTickets.Services;
using Nop.Plugin.Apollo.EventTickets.Settings;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Apollo.EventTickets.Tasks;

/// <summary>
/// Represents task for my rewards points program
/// </summary>
public partial class EventTicketsTask : IScheduleTask
{

    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IEventTicketService _eventTicketService;
    protected readonly IStoreService _storeService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IStoreContext _storeContext;
    protected readonly IEventTicketsPdfService _eventTicketsPdfService;
    protected readonly IWorkContext _workContext;
    protected readonly IEventTicketsMessageService _eventTicketsMessageService;
    protected readonly LocalizationSettings _localizationSettings;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly IVendorService _vendorService; 
    protected readonly ISettingService _settingService;
    protected readonly EventTicketSettings _eventTicketSettings;
    protected readonly IAffiliateService _affiliateService;
    protected readonly IProductService _productService;
    protected readonly IEventPublisher _eventPublisher;

    #endregion

    #region Ctor

    public EventTicketsTask(ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        IEventTicketService eventTicketService,
        IStoreService storeService,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        IEventTicketsPdfService eventTicketsPdfService,
        IWorkContext workContext,
        IEventTicketsMessageService eventTicketsMessageService,
        LocalizationSettings localizationSettings,
        IOrderProcessingService orderProcessingService,
        IVendorService vendorService,
        ISettingService settingService, 
        EventTicketSettings eventTicketSettings,
        IAffiliateService affiliateService,
        IProductService productService,
        IEventPublisher eventPublisher)
    {
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _eventTicketService = eventTicketService;
        _storeService = storeService;
        _localizationService = localizationService;
        _storeContext = storeContext;
        _eventTicketsPdfService = eventTicketsPdfService;
        _workContext = workContext;
        _eventTicketsMessageService = eventTicketsMessageService;
        _localizationSettings = localizationSettings;
        _orderProcessingService = orderProcessingService;
        _vendorService = vendorService;
        _settingService = settingService;
        _eventTicketSettings = eventTicketSettings;
        _affiliateService = affiliateService;
        _productService = productService;
        _eventPublisher = eventPublisher;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Executes a task
    /// </summary>
    public async Task ExecuteAsync()
    {
        string fileName = string.Empty;
        string filePath = string.Empty;
        DateTime runTime;
        bool runVendorReports = false;
        bool runAffiliateReports = false;
        IList<OrderItem> events = null;

        var stores = await _storeService.GetAllStoresAsync();
        int storeId = _eventTicketSettings.DefaultStoreId;
        var defaultStore = await _storeService.GetStoreByIdAsync(storeId);

        // Off Sale 

        foreach (var store in stores)
        {

            var tickets = await _eventTicketService.SearchTicketsAsync(
                storeId: store.Id,
                showHidden: false
            );

            foreach (var ticket in tickets)
            {
                if (ticket.Published)
                {
                    var pte = await _eventTicketService.GetEventTicketProductTypeExtensionAsync(ticket);
                    if (pte != null)
                    {

                        var associatedTickets = await _productService.GetAssociatedProductsAsync(ticket.Id);

                        var reportDateTime = DateTime.SpecifyKind(pte.AvailabilityEndDate, DateTimeKind.Local);
                        var eventDateTime = DateTime.SpecifyKind(pte.AvailabilityStartDate, DateTimeKind.Local);
                        var timeNow = DateTime.Now;

                        var offTime = reportDateTime.AddMinutes(_eventTicketSettings.OffSaleTimeInMinutes);
                        var unpublishTime = eventDateTime.AddHours(_eventTicketSettings.UnPublishTimeInHours);

                        string timeDetails = "Off time: " + offTime.ToString() + " Unpublish Time: " + unpublishTime.ToString();

                        if (reportDateTime.CompareTo(timeNow) < 0)
                        {

                            if (defaultStore != null && ticket.ParentGroupedProductId == 0 && _eventTicketService.CanMarkTicketReportPrinted(ticket))
                            {

                                (filePath, fileName) = await _eventTicketsPdfService.PrintEventDoorReportToPDFAsync(ticket, (await _workContext.GetWorkingLanguageAsync()).Id, defaultStore, null, null);
                                var eventDoorReportNotificationQueuedEmailIds = await _eventTicketsMessageService.SendEventDoorReportNotificationAsync(defaultStore, null,
                                    ticket, _localizationSettings.DefaultAdminLanguageId, filePath, fileName);
                                if (eventDoorReportNotificationQueuedEmailIds.Any())
                                    await _eventTicketService.ApolloLogAsync(LogLevel.Information,
                                        $"\"Event Door Report\" email (to store owner) has been queued. Queued email identifiers: " +
                                        $"{string.Join(", ", eventDoorReportNotificationQueuedEmailIds)}.");

                                var vendor = await _vendorService.GetVendorByIdAsync(ticket.VendorId);
                                if (vendor != null)
                                {
                                    if (vendor.Active && !vendor.Email.Contains("noreport"))
                                    {

                                        eventDoorReportNotificationQueuedEmailIds = await _eventTicketsMessageService.SendEventDoorReportNotificationAsync(defaultStore, vendor,
                                            ticket, _localizationSettings.DefaultAdminLanguageId, filePath, fileName);
                                        if (eventDoorReportNotificationQueuedEmailIds.Any())
                                            await _eventTicketService.ApolloLogAsync(LogLevel.Information,
                                                $"\"Event Door Report\" email (to vendor) has been queued. Queued email identifiers: " +
                                                $"{string.Join(", ", eventDoorReportNotificationQueuedEmailIds)}.");
                                    }
                                }

                                await _eventTicketService.MarkTicketAsReportPrintedAsync(ticket);

                                foreach(var at in associatedTickets)
                                    await _eventTicketService.MarkTicketAsReportPrintedAsync(at);
                            }

                            ticket.DisableBuyButton = true;
                            ticket.DisableWishlistButton = true;

                            await _eventTicketService.ApolloLogAsync(LogLevel.Information, 
                                "Ticket Offsale: Ticket Id = " + ticket.Id + " Time: " + timeDetails);

                            if (ticket.ParentGroupedProductId != 0)
                            {
                                ticket.Published = false;
                                await _eventTicketService.ApolloLogAsync(LogLevel.Information, 
                                    "Ticket Unpublished: Ticket Id = " + ticket.Id + " Time: " + timeDetails);
                            }

                            await _eventTicketService.UpdateTicketAsync(ticket);

                        }
                        else if (pte.RecordStatusId == CoreSystemHelper.RecordStatus.Initialised)
                            await _eventTicketService.MarkTicketAsOperatingAsync(ticket);

                        if (unpublishTime.CompareTo(timeNow) < 0)
                        {
                            // Event Door Report

                            ticket.Published = false;
                            await _eventTicketService.UpdateTicketAsync(ticket);

                            await _eventTicketService.ApolloLogAsync(LogLevel.Information, "Ticket Unpublished: Ticket Id = " + ticket.Id, timeDetails);

                        }
                    }

                    if (!ticket.Published || ticket.DisableBuyButton)
                    {
                        //event notification
                        await _eventPublisher.EntityUpdatedAsync(ticket);
                    }
                }
            }
        }

        //Send email reports

        if (_eventTicketSettings.TestMode)
            await _eventTicketService.ApolloLogAsync(LogLevel.Debug, "Running Daily Reports - Run Time: " + DateTime.Now.ToString(), null);

        string defaultValue = DateTime.Now.AddDays(-1).ToString();
        DateTime lastRun = DateTime.Now.AddDays(-8); // Set default value in case never run before

        if (defaultStore != null)
        {
            // run default store reports 

            events = _eventTicketService.GetAllEventTicketsOnSale(storeId: 0);
            if (events != null && events.Count > 0)
            {
                // Daily Reports 

                var lastRunDateTime = await _genericAttributeService.GetAttributeAsync<string>(defaultStore, 
                    SystemHelper.EventTicketDefaults.SummaryReportLastRun, defaultStore.Id, defaultValue);
                bool valid = DateTime.TryParse(lastRunDateTime, out runTime);
                if (valid)
                {
                    if (_eventTicketSettings.TestMode)
                        await _eventTicketService.ApolloLogAsync(LogLevel.Debug, "Sales Summary Reports - Last Run: " + runTime.ToString(), null);

                    lastRun = runTime;
                    if (await RunReportNow(1, lastRun, defaultStore))
                    {
                        if (_eventTicketSettings.TestMode)
                        {
                            await _eventTicketService.ApolloLogAsync(LogLevel.Debug, 
                                "Total Events for Store: " + defaultStore.Id.ToString() + 
                                " Count: " + events.Count().ToString(), null);
                        }

                        (filePath, fileName) = await _eventTicketsPdfService.PrintSalesSummaryReportToPDFAsync(events, (await _workContext.GetWorkingLanguageAsync()).Id, defaultStore);
                        var summaryReportNotificationQueuedEmailIds = await _eventTicketsMessageService.SendSummaryReportNotificationAsync(defaultStore, null, null,
                            _localizationSettings.DefaultAdminLanguageId, filePath, fileName);
                        if (summaryReportNotificationQueuedEmailIds.Any())
                            await _eventTicketService.ApolloLogAsync(LogLevel.Information, $"\"Summary Report\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", summaryReportNotificationQueuedEmailIds)}.");

                        runVendorReports = true;
                        runAffiliateReports = true;

                        string defaultRunTime = _eventTicketSettings.DefaultReportDateTime;
                        valid = DateTime.TryParse(defaultRunTime, out runTime);
                        if (valid)
                        {
                            await _genericAttributeService.SaveAttributeAsync<string>(defaultStore, 
                                SystemHelper.EventTicketDefaults.SummaryReportLastRun, runTime.ToString(), defaultStore.Id);
                        }

                    }
                }
            }

            if (runVendorReports)
            {

                var vendors = await GetEventVendorsAsync(events);

                if (_eventTicketSettings.TestMode)
                {
                    await _eventTicketService.ApolloLogAsync(LogLevel.Debug, "Total Vendors for Report: " + vendors.Count().ToString() +
                        " for Event Count: " + events.Count().ToString(), null);
                }

                foreach (var vendor in vendors)
                {
                    if (vendor.Active && !vendor.Email.Contains("noreport"))
                    {
                        events = _eventTicketService.GetAllEventTicketsOnSale(0, vendor.Id);
                        if (events != null && events.Count > 0)
                        {
                            if (_eventTicketSettings.TestMode)
                            {
                                await _eventTicketService.ApolloLogAsync(LogLevel.Debug, "Total Events for Vendor: " + vendor.Name.ToString() + 
                                    " Event Count: " + events.Count().ToString(), null);
                            }

                            (filePath, fileName) = await _eventTicketsPdfService.PrintSalesSummaryReportToPDFAsync(events, (await _workContext.GetWorkingLanguageAsync()).Id, defaultStore, vendor);

                            var summaryReportVendorNotificationQueuedEmailIds = await _eventTicketsMessageService.SendSummaryReportNotificationAsync(defaultStore, vendor, null,
                                _localizationSettings.DefaultAdminLanguageId, filePath, fileName);
                            if (summaryReportVendorNotificationQueuedEmailIds.Any())
                                await _eventTicketService.ApolloLogAsync(LogLevel.Information, $"\"Summary Report\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", summaryReportVendorNotificationQueuedEmailIds)}.");
                        }
                    }
                }
            }

            if (runAffiliateReports)
            {

                var affiliates = await _affiliateService.GetAllAffiliatesAsync();

                if (_eventTicketSettings.TestMode)
                {
                    await _eventTicketService.ApolloLogAsync(LogLevel.Debug, "Total Affiliates for Report: " + affiliates.Count().ToString() +
                        " for Event Count: " + events.Count().ToString(), null);
                }

                foreach (var affiliate in affiliates)
                {
                    if (affiliate.Active)
                    {
                        events = await _eventTicketService.GetAllAffiliateTicketsOnSaleAsync(0, affiliate.Id);
                        if (events != null && events.Count > 0)
                        {
                            string name = await _eventTicketService.GetAffiliateNameAsync(affiliate);
                            if (_eventTicketSettings.TestMode)
                            {
                                await _eventTicketService.ApolloLogAsync(LogLevel.Debug, "Total Events for Affiliate: " + name +
                                    " for Event Count: " + events.Count().ToString(), null);
                            }

                            (filePath, fileName) = await _eventTicketsPdfService.PrintSalesSummaryReportToPDFAsync(events, (await _workContext.GetWorkingLanguageAsync()).Id, defaultStore, null, null, affiliate);

                            var summaryReportAffiliateNotificationQueuedEmailIds = await _eventTicketsMessageService.SendSummaryReportNotificationAsync(defaultStore, null, affiliate,
                                _localizationSettings.DefaultAdminLanguageId, filePath, fileName);
                            if (summaryReportAffiliateNotificationQueuedEmailIds.Any())
                                await _eventTicketService.ApolloLogAsync(LogLevel.Information, $"\"Summary Report\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", summaryReportAffiliateNotificationQueuedEmailIds)}.");
                        }
                    }
                }
            }

            foreach (var store in stores)
            {
                if (store.Id != defaultStore.Id)
                {
                    // run individual store reports 

                    var lastRunDateTime = await _genericAttributeService.GetAttributeAsync<string>(store, 
                        SystemHelper.EventTicketDefaults.SummaryReportLastRun, store.Id, defaultValue);
                    bool valid = DateTime.TryParse(lastRunDateTime, out runTime);
                    if (valid)
                    {
                        if (_eventTicketSettings.TestMode)
                            await _eventTicketService.ApolloLogAsync(LogLevel.Debug, 
                                "Sales Summary Reports - Last Run Time: " + runTime.ToString(), null);

                        lastRun = runTime;
                        if (await RunReportNow(1, lastRun, store))
                        {
                            events = _eventTicketService.GetAllEventTicketsOnSale(storeId: store.Id);
                            if (events != null && events.Count > 0)
                            {

                                if (_eventTicketSettings.TestMode)
                                {
                                    await _eventTicketService.ApolloLogAsync(LogLevel.Debug, "Total Events for Store: " + store.Id.ToString() + 
                                        " Count: " + events.Count().ToString(), null);
                                }

                                (filePath, fileName) = await _eventTicketsPdfService.PrintSalesSummaryReportToPDFAsync(events, (await _workContext.GetWorkingLanguageAsync()).Id, store);
                                var summaryReportNotificationQueuedEmailIds = await _eventTicketsMessageService.SendSummaryReportNotificationAsync(store, null, null,
                                    _localizationSettings.DefaultAdminLanguageId, filePath, fileName);
                                if (summaryReportNotificationQueuedEmailIds.Any())
                                    await _eventTicketService.ApolloLogAsync(LogLevel.Information, $"\"Summary Report\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", summaryReportNotificationQueuedEmailIds)}.");

                                runVendorReports = true;

                                string defaultRunTime = _eventTicketSettings.DefaultReportDateTime;
                                valid = DateTime.TryParse(defaultRunTime, out runTime);
                                if (valid)
                                {
                                    await _genericAttributeService.SaveAttributeAsync<string>(store, 
                                        SystemHelper.EventTicketDefaults.SummaryReportLastRun, runTime.ToString(), store.Id);
                                }
                            }

                        }
                    }
                }
            }

        }
    }

    /// <summary>
    /// Get a list of vendors in order (order items)
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>Vendors</returns>
    protected async virtual System.Threading.Tasks.Task<List<Vendor>> GetEventVendorsAsync(IList<OrderItem> events)
    {
        var vendorList = new List<Vendor>();
        var pIds = events.Select(x => x.ProductId).ToArray();
        if (pIds.Count() > 0)
        {
            var vendors = (await _vendorService.GetVendorsByProductIdsAsync(pIds)).GroupBy(v => v.Id).Distinct();
            foreach(var vendor in vendors)
            {
                vendorList.Add(vendor.FirstOrDefault());
            }

            return vendorList;
        }
        else
            return null;
    }

    private async Task<bool> RunReportNow(int reportDays, DateTime lastReportTime, Store store)
    {
        var dtNow = DateTime.Now;
        var timeleft = dtNow.Subtract(lastReportTime);

        if (timeleft.Days == 0) // If allready run today
            return (false);

        bool runReport = false;
        if (timeleft.Days > 7) // Never run or not run for over a week
        {
            DateTime runTime;
            string defaultRunTime = _eventTicketSettings.DefaultReportDateTime;
            bool valid = DateTime.TryParse(defaultRunTime, out runTime);
            if (valid)
            {
                runTime = runTime.AddDays(-1);
                await _genericAttributeService.SaveAttributeAsync<string>(store, 
                    SystemHelper.EventTicketDefaults.SummaryReportLastRun, runTime.ToString(), store.Id);
                timeleft = runTime.Subtract(runTime);
            }
        }

        switch (reportDays)
        {
            case 1:
                if (timeleft.Days > 0 && timeleft.Hours > 12) // Never run or not run for a day and a half
                {
                    DateTime runTime;
                    string defaultRunTime = _eventTicketSettings.DefaultReportDateTime;
                    bool valid = DateTime.TryParse(defaultRunTime, out runTime);
                    timeleft = dtNow.Subtract(runTime);
                }

                if (timeleft.Hours == 0 && timeleft.Minutes <= 15)
                    runReport = true;
                break;

            case 2:
                if ((dtNow.DayOfWeek == DayOfWeek.Tuesday ||
                    dtNow.DayOfWeek == DayOfWeek.Thursday) &&
                    timeleft.Hours == 0 && timeleft.Minutes < 15)
                    runReport = true;
                break;

            case 3:
                if ((dtNow.DayOfWeek == DayOfWeek.Monday ||
                    dtNow.DayOfWeek == DayOfWeek.Wednesday ||
                    dtNow.DayOfWeek == DayOfWeek.Friday) &&
                    timeleft.Hours == 0 && timeleft.Minutes < 15)
                    runReport = true;
                break;

            case 4:
                if (dtNow.DayOfWeek == DayOfWeek.Tuesday &&
                    timeleft.Hours == 0 && timeleft.Minutes < 15)
                    runReport = true;
                break;

            case 5:
                if (dtNow.DayOfWeek == DayOfWeek.Thursday &&
                    timeleft.Hours == 0 && timeleft.Minutes < 15)
                    runReport = true;
                break;
        }

        return (runReport);
    }

    #endregion
}
