using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Vendors;

using Nop.Plugin.Apollo.Core;
using Nop.Plugin.Apollo.Core.Services;
using Nop.Plugin.Apollo.Core.Settings;
using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets.Services;

public class EventTicketsInstallService : ApolloInstallService, IEventTicketsInstallService
{

    int _maximumTicketQuantity = 28;

    private const string EVENT_TICKETS_TASK_TYPE = "Nop.Plugin.Apollo.EventTickets.Tasks.EventTicketsTask, Nop.Plugin.Apollo.EventTickets";

    #region Fields

    protected readonly IVendorService _vendorService;   
    protected readonly IProductAvailabilityService _productAvailabilityService;
    protected readonly IEventTicketService _eventTicketService;
    protected readonly EventTicketSettings _eventTicketSettings;

    CoreSystemHelper _systemHelper = new CoreSystemHelper();

    #endregion

    #region Ctor

    public EventTicketsInstallService(ILocalizationService localizationService,
        ISettingService settingService,
        INopFileProvider fileProvider,
        IStoreContext storeContext,
        IRepository<EmailAccount> emailAccountRepository,
        IRepository<MessageTemplate> messageTemplateRepository,
        IRepository<Store> storeRepository,
        IRepository<Picture> pictureRepository,
        IRepository<Product> productRepository,
        IRepository<ProductAttribute> productAttributeRepository,
        IRepository<ProductTag> productTagRepository,
        IRepository<ProductProductTagMapping> productProductTagMappingRepository,
        IRepository<ProductTemplate> productTemplateRepository,
        IRepository<Category> categoryRepository,
        IRepository<CategoryTemplate> categoryTemplateRepository,
        IRepository<TaxCategory> taxCategoryRepository,
        IRepository<UrlRecord> urlRecordRepository,
        IEventTicketService eventTicketService,
        IProductAttributeService productAttributeService,
        INopDataProvider dataProvider,
        IProductTypeExtensionsService productTypeExtensionsService,
        IVendorService vendorService,
        IScheduleTaskService scheduleTaskService,
        IPermissionService permissionService,
        IRepository<PermissionRecord> permissionRecordRepository,
        ICustomerService customerService,
        IWorkContext workContext,
        IStaticCacheManager cacheManager,
        IUrlRecordService urlRecordService,
        IApolloCoreService apolloCoreService,
        IProductService productService,            
        IProductAvailabilityService productAvailabilityService,
        ApolloCoreSettings apolloCoreSettings,
        EventTicketSettings eventTicketSettings) : base(localizationService,
            settingService,
            fileProvider,
            storeContext,
            emailAccountRepository,
            messageTemplateRepository,
            storeRepository,
            pictureRepository,
            productRepository,
            productAttributeRepository,
            productTagRepository,
            productProductTagMappingRepository,
            productTemplateRepository,
            categoryRepository,
            categoryTemplateRepository,
            taxCategoryRepository,
            urlRecordRepository,
            productAttributeService,
            dataProvider,
            productTypeExtensionsService,
            customerService,
            cacheManager,
            permissionService,
            permissionRecordRepository,
            scheduleTaskService,
            urlRecordService,
            apolloCoreService,
            apolloCoreSettings)
    {
        _eventTicketService = eventTicketService; 
        _vendorService = vendorService;
        _productAvailabilityService = productAvailabilityService;
        _eventTicketSettings = eventTicketSettings;
    }

    #endregion

    #region Utility

    protected new async Task DeleteProductAsync(Product product)
    {
        await _productAvailabilityService.DeleteAllProductAvailabilityByProductIdAsync(product.Id);
        await base.DeleteProductAsync(product);
    }

    protected virtual string GetSamplesPath()
    {
        string filePath = _fileProvider.MapPath("~/Plugins/SSI.Apollo.EventTickets/Content/Images/");
        return filePath;
    }

    #endregion

    #region Methods

    public async Task InstallPermissionsAsync(bool install)
    {

        var permissions = new List<PermissionRecord>();

        if (install)
        {

            PermissionRecord manageEventTickets = _systemHelper.GetAccessPermission(CoreSystemHelper.AccessMode.Manage);
            var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync()).Where(x => x.SystemName == manageEventTickets.SystemName);
            if (permissionRecord == null || permissionRecord.Count() == 0)
            {
                permissions.Add(manageEventTickets);
                await _permissionRecordRepository.InsertAsync(manageEventTickets);
            }

            PermissionRecord operateApollo = _systemHelper.GetAccessPermission(CoreSystemHelper.AccessMode.Operate);
            permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync()).Where(x => x.Name == operateApollo.Name);
            if (permissionRecord == null || permissionRecord.Count() == 0)
            {
                await _permissionRecordRepository.InsertAsync(operateApollo);
                permissions.Add(operateApollo);
            }

            var customerRole = (await _customerService.GetAllCustomerRolesAsync()).Where(cr => cr.Name == "Administrators").FirstOrDefault();
            if (customerRole != null)
            {
                foreach (var permission in permissions)
                {
                    var mapping = await _permissionService.GetMappingByPermissionRecordIdAsync(permission.Id);
                    if (mapping != null && mapping.Count() == 0)
                    {
                        var prcrm = new PermissionRecordCustomerRoleMapping { CustomerRoleId = customerRole.Id, PermissionRecordId = permission.Id };
                        await _permissionService.InsertPermissionRecordCustomerRoleMappingAsync(prcrm);
                    }
                }

                await _cacheManager.ClearAsync();
            }


            var scannerRole = (await _customerService.GetAllCustomerRolesAsync()).Where(cr => cr.Name == "Scanner").FirstOrDefault();
            if (scannerRole == null)
            {
                scannerRole = new CustomerRole
                {
                    Name = "Scanner",
                    Active = true,
                };
                await _customerService.InsertCustomerRoleAsync(scannerRole);
            }
        }
        else
        {
            PermissionRecord manageEventTickets = _systemHelper.GetAccessPermission(CoreSystemHelper.AccessMode.Manage);
            var permissionRecords = (await _permissionService.GetAllPermissionRecordsAsync()).Where(x => x.Name == manageEventTickets.Name);
            foreach (var record in permissionRecords)
                await _permissionRecordRepository.DeleteAsync(record);

            PermissionRecord operateEventTickets = _systemHelper.GetAccessPermission(CoreSystemHelper.AccessMode.Operate);
            permissionRecords = (await _permissionService.GetAllPermissionRecordsAsync()).Where(x => x.Name == operateEventTickets.Name);
            foreach (var record in permissionRecords)
                await _permissionRecordRepository.DeleteAsync(record);

            var scannerRole = (await _customerService.GetAllCustomerRolesAsync()).Where(cr => cr.Name == "Scanner").FirstOrDefault();
            if (scannerRole != null)
                await _customerService.DeleteCustomerRoleAsync(scannerRole);
        }

        await _cacheManager.ClearAsync();
    }

    public async Task InstallLocalisationAsync(bool install)
    {

        var resourcesList = new Dictionary<string, string>
        {
            ["Plugin.Apollo.EventTickets.Sales"] = "Order Sales",
            ["Plugin.Apollo.EventTickets.Delivery"] = "Order Delivery",
            ["Plugin.Apollo.EventTickets.Process"] = "Order Complete",

            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.Name"] = "Name",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.AttributeDescription"] = "Product Details",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.CustomerEmail"] = "Customer",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.OrderId"] = "Order",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.PaymentStatus"] = "Status",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.PaymentDate"] = "Payment Date",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.Price"] = "Price",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.Quantity"] = "Quantity",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.TotalPrice"] = "Total",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.TotalSales"] = "Total Sales",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.TotalCost"] = "Total Cost",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.PaymentMethod"] = "Payment Method",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.ShippingMethod"] = "Shipping Method",
            ["Plugin.Apollo.EventTickets.Sales.Values.fields.Action"] = "Edit",
            ["Plugin.Apollo.EventTickets.Sales.Values.Fields.OrderDate"] = "Order Date",

            ["Plugin.Apollo.EventTickets.Admin.PdfReports"] = "PDF Reports",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.All"] = "PDF Report All",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Selected"] = "PDF Report Selected",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.NoOrders"] = "No Orders Found",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.NoProducts"] = "No Products Found",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Summary"] = "Sales Summary Report",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Settlement"] = "Settlement Report",

            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Name"] = "Orders Report",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Category"] = "Customer Sales",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Customer"] = "Customer",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.OrderDetails"] = "Order Details",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.PaidDate"] = "Paid Date",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Order"] = "Order",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Payment"] = "Payment",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Method"] = "Method",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Order#"] = "Order Number: {0}",
            ["Plugin.Apollo.EventTickets.Admin.PdfReport.OrderDate"] = "Order Date: {0}",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport"] = " Summary Report",

            ["Plugin.Apollo.EventTickets.Sales.Fields.SearchProductName"] = "Search Product Name",
            ["Plugin.Apollo.EventTickets.Sales.Fields.SearchProductName.Hint"] = "Enter the name to search",

            ["Plugin.Apollo.EventTickets.Sales.Fields.FromDate"] = "Order Created Start Date",
            ["Plugin.Apollo.EventTickets.Sales.Fields.FromDate.Hint"] = "Select Start Date of Search",

            ["Plugin.Apollo.EventTickets.Sales.Fields.ToDate"] = "Order Created End Date",
            ["Plugin.Apollo.EventTickets.Sales.Fields.ToDate.Hint"] = "Select the End Date of Search",

            ["Plugin.Apollo.EventTickets.Sales.Fields.Ispay"] = "Order Not Paid",
            ["Plugin.Apollo.EventTickets.Sales.Fields.Ispay.Hint"] = "Select to display Orders Not Paid",

            ["Plugin.Apollo.EventTickets.Sales.Fields.OrderbyName"] = "Order by Name",
            ["Plugin.Apollo.EventTickets.Sales.Fields.OrderbyName.Hint"] = "Select Order by Name",

            ["Plugin.Apollo.EventTickets.Sales.Fields.PaymentMethod"] = "Payment Method",
            ["Plugin.Apollo.EventTickets.Sales.Fields.PaymentMethod.Hint"] = "Select Payment Method",

            ["Plugin.Apollo.EventTickets.Fields.EventTicketReportDateTime"] = "Report Date and Time",
            ["Plugin.Apollo.EventTickets.Fields.EventTicketReportDateTime.Hint"] = "Report Date and Time for the Event Door Report to be sent",

            ["Plugin.Apollo.EventTickets.Fields.EventTicketDateTime"] = "Event the Date and Time",
            ["Plugin.Apollo.EventTickets.Fields.EventTicketDateTime.Hint"] = "Event the Date and Time for the event to start",
            
            ["Plugin.Apollo.EventTickets.Fields.ProductTime"] = "Doors",
            ["Plugin.Apollo.EventTickets.Fields.ProductTime.Hint"] = "Enter door information",

            ["Plugin.Apollo.EventTickets.Fields.RecordStatusId"] = "Event Status",
            ["Plugin.Apollo.EventTickets.Fields.RecordStatusId.Hint"] = "Set the event status which controls the workflow for each event",

            ["Plugin.Apollo.EventTickets.Fields.GroupProductId"] = "Order Product",
            ["Plugin.Apollo.EventTickets.Fields.GroupProductId.Hint"] = "Select the Group Product for the search",

            ["Plugin.Apollo.EventTickets.Fields.AssociatedProductId"] = "Selected Product",
            ["Plugin.Apollo.EventTickets.Fields.AssociatedProductId.Hint"] = "Select Product for the search",

            ["Plugin.Apollo.EventTickets.Admin.SelectGroupProduct"] = "Please select the product",
            ["Plugin.Apollo.EventTickets.Admin.SelectAssociatedProduct"] = "Please select the product",

            ["Plugin.Apollo.EventTickets.Fields.AppointmentAvailabilityStartDate"] = "Start Time",
            ["Plugin.Apollo.EventTickets.Fields.AppointmentAvailabilityEndDate"] = "End Time",
            ["Plugin.Apollo.EventTickets.Fields.AppointmentSlotDuration"] = "Duration",
            ["Plugin.Apollo.EventTickets.Fields.Quantity"] = "Quantity",
            ["Plugin.Apollo.EventTickets.Fields.ProductName"] = "Product Name",
            ["Plugin.Apollo.EventTickets.Fields.EventTicketDateTime"] = "Event Date",
            ["Plugin.Apollo.EventTickets.Fields.VenueAddress"] = "Venue Address",

            ["Plugin.Apollo.EventTickets.ProductTypeExtensionSettings.Fields.ProductId"] = "Product Name",
            ["Plugin.Apollo.EventTickets.ProductTypeExtensionSettings.Fields.ProductTypeId"] = "Product Type",

            ["Plugin.Apollo.EventTickets.ProductTypeExtensionSettings.SelectProduct"] = "Please Select the Product",
            ["Plugin.Apollo.EventTickets.ProductTypeExtensionSettings.SelectProductType"] = "Please Select the Product Type",
            ["Plugin.Apollo.EventTickets.ProductTypeExtensionSettings.SelectRecordType"] = "Please Select the Record Type",
            ["Plugin.Apollo.EventTickets.ProductTypeExtensionSettings.Note"] = "Product Configuration Settings define the Product Type for a Product",
            ["Plugin.Apollo.EventTickets.ProductTypeExtensionSettings.AddNew"] = "Add New Product Type Settings",
            ["Plugin.Apollo.EventTickets.ProductTypeExtensionSettings.Edit"] = "Edit Product Type Settings",

            ["Plugin.Apollo.EventTickets.Fields.ProductTypeId"] = "Please Select Apollo Product Type",
            ["Plugin.Apollo.EventTickets.Fields.RecordTypeId"] = "Please Select Apollo Record Type",
            ["Plugin.Apollo.EventTickets.Fields.ProductId"] = "Please Select the Ticket",

            ["Plugin.Apollo.EventTickets.Sales.SetOrdersAsPaid"] = "Set All Orders as Paid",
            ["Plugin.Apollo.EventTickets.Sales.SetOrdersAsApproved"] = "Set All Orders as Approved",
            ["Plugin.Apollo.EventTickets.Process.ReturnOrdersToCheck"] = "Return Orders to Check",
            ["Plugin.Apollo.EventTickets.Process.SetOrdersAsProcessed"] = "Set Orders as Processed",
            ["Plugin.Apollo.EventTickets.Delivery.ReturnOrdersToProcess"] = "Return Orders to Process",
            ["Plugin.Apollo.EventTickets.Delivery.SetOrdersInTransit"] = "Set Orders in Transit",
            ["Plugin.Apollo.EventTickets.Settlement.Finalise"] = "Finalise",
            ["Plugin.Apollo.EventTickets.CreateTicketsDetails"] = "Create Ticket",
            ["Plugin.Apollo.EventTickets.Admin.Tickets.BackToList"] = "back to ticket list",

            ["Plugin.Apollo.EventTickets.Deleted"] = "The ticket has been deleted successfully.",
            ["Plugin.Apollo.EventTickets.AssociatedProducts.AddNew"] = "Add new associated ticket",
            ["Plugin.Apollo.EventTickets.CreateOrUpdateTicketsDetails"] = "Edit ticket details",
            ["Plugin.Apollo.EventTickets.CreateOrUpdateTickets"] = "Tickets",

            ["Plugin.Apollo.EventTickets.Book"] = "Book",
            ["Plugin.Apollo.EventTickets.OffSale"] = "The tickets for this event are no longer On Sale. Please check with Venue for Door Ticket sales options",
            ["Plugin.Apollo.EventTickets.OutofStock"] = "Sorry. The tickets for this event are now sold out",

            ["Plugin.Apollo.EventTickets.Admin.PdfReport.Date"] = "Report Date: {0}",

            ["Plugin.Apollo.EventTickets.Admin.Tickets.Copy.Button"] = "Copy ticket",

            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Name"] = "Event Door Report",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Button"] = "Event Door Report",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Header"] = "Event Ticket Sales {0}",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Details"] = "Event Ticket Sales Details",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.EventName"] = "Event Name: ",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Date"] = "Event Date: ",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Doors"] = "Doors: ",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Venue"] = "Event Venue: ",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TicketSalesSummary"] = "Ticket Sales Summary",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TicketType"] = "Ticket Type: ",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TotalTicketSales"] = "Total Ticket Sales",

            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Purchaser"] = "Ticket Name",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TicketTypes"] = "Ticket Type",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.User"] = "Additional Names",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Qty"] = "Quantity",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.TransactionId"] = "Tx Id",
            ["Plugin.Apollo.EventTickets.Admin.PdfEventDoorReport.Order"] = "Order No.",

            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Name"] = " Event Summary Report",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.TotalSales"] = "Total Sales",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.SoldTotal"] = "Sold Total",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Rem"] = "Rem",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Total"] = "Total",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.24hr"] = "Sold 24hr",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Alloc"] = "Alloc",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Event"] = "Event",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.OrderDate"] = "Order Date: {0}",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Order#"] = "Order Number: {0}",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Sold"] = "Sold",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Cost"] = "Cost",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Outlet"] = "Outlet",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Breakdown"] = "Detailed Sales List",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Details"] = "Summary of All sales for {0}",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Promo"] = "Your local online ticket outlet",
            ["Plugin.Apollo.EventTickets.Admin.PdfSummaryReport.Address"] = "Enter your address",

            ["Plugin.Apollo.EventTickets.Admin.PdfSettlementReport.Name"] = "Event Settlement Report",
            ["Plugin.Apollo.EventTickets.Admin.PdfSettlementReport.Details"] = "Event: {0}",
            ["Plugin.Apollo.EventTickets.Admin.PdfSettlementReport.RCTI"] = "Recipient Generated Tax Invoice",
            ["Plugin.Apollo.EventTickets.Admin.PdfSettlementReport.Breakdown"] = "Detailed Ticket Sales for this Event",

            ["Plugin.Apollo.EventTickets.Admin.Copy"] = "Copy ticket",

            ["Plugin.Apollo.EventTickets.Admin.Copy.Published"] = "Published",
            ["Plugin.Apollo.EventTickets.Admin.Copy.Published.Hint"] = "Check to mark a product as published.",

            ["Plugin.Apollo.EventTickets.Admin.Copy.Name"] = "New event name",
            ["Plugin.Apollo.EventTickets.Admin.Copy.Name.Hint"] = "The name of the new event product.",

            ["Plugin.Apollo.EventTickets.Admin.Copy.CopyImages"] = "Copy images",
            ["Plugin.Apollo.EventTickets.Admin.Copy.CopyImages.Hint"] = "Check to copy the images.",

            ["Plugin.Apollo.EventTickets.Admin.Copy.UseTemplate"] = "Use Template",
            ["Plugin.Apollo.EventTickets.Admin.Copy.UseTemplate.Hint"] = "Use the Selected Template to build a ticket - Leave unselected to copy existing ticket",

            ["Plugin.Apollo.EventTickets.Admin.SelectTemplate"] = "Initial Event Build (Or Select Template)",

            ["Plugin.Apollo.EventTickets.Admin.Copy.SelectedTemplateId"] = "Available Templates",
            ["Plugin.Apollo.EventTickets.Admin.Copy.SelectedTemplateId.Hint"] = "Leave unselected for Initial Event Build - Select Template to Build Event",


            ["Plugin.Apollo.EventTickets.Admin.List.SearchTemplates"] = "Search Templates",
            ["Plugin.Apollo.EventTickets.Admin.List.SearchTemplates.Hint"] = "Select this option to display the available events templates",

            ["Plugin.Apollo.EventTickets.Admin.List.SearchAssociated"] = "Associated",
            ["Plugin.Apollo.EventTickets.Admin.List.SearchAssociated.Hint"] = "Select this option to display the associated ticket items",

            ["Plugin.Apollo.EventTickets.Admin.OnSale.Notification"] = "Warning: This product is now on sale - Edit with Caution",
            
            ["Plugin.Apollo.EventTickets.Fields.AffiliatesIds.NoAvailableAffiliates"] = "No available agents",
            ["Plugin.Apollo.EventTickets.Fields.AffiliatesIds.Affiliates"] = "Agents",
            ["Plugin.Apollo.EventTickets.Fields.AffiliatesIds.Affiliates.Hint"] = "Select the list of agents who recieve regular summary reports",
            ["Plugin.Apollo.EventTickets.Affiliate.Products"] = "Agent Products",

            ["Admin.ContentManagement.MessageTemplates.Description.SummaryReportNotification.EventTicket"] = "Event Tickets Summary Report",
            ["Admin.ContentManagement.MessageTemplates.Description.DoorReportNotification.EventTicket"] = "Event Door Report",

            ["Admin.ContentManagement.MessageTemplates.Description.OrderPlaced.CustomerNotification.EventTicket"] = "Order Placed Customer Notification EventTicket Template",
            ["Admin.ContentManagement.MessageTemplates.Description.OrderApproved.CustomerNotification.EventTicket"] = "Order Completed Customer Notification EventTicket Template",
            ["Admin.ContentManagement.MessageTemplates.Description.OrderCompleted.EmailListNotification.EventTicket"] = "Order Completed Email List Notification EventTicket Template",
            ["Admin.ContentManagement.MessageTemplates.Description.OrderFinalised.EmailListNotification.EventTicket"] = "Order Cancelled Email List Notification EventTicket Template",
            ["Admin.ContentManagement.MessageTemplates.Description.OrderReminder.CustomerNotification.EventTicket"] = "Order Reminder Customer Notification EventTicket Template",

            ["Plugin.Apollo.EventTickets.TicketDetails"] = "Ticket Details",
            ["Plugin.Apollo.EventTickets.TicketError"] = "Ticket Error",
            ["Plugin.Apollo.EventTickets.TicketNumber"] = "Ticket Number",
            ["Plugin.Apollo.EventTickets.EventName"] = "Event Name",
            ["Plugin.Apollo.EventTickets.Tickets"] = "Ticket(s)",
            ["Plugin.Apollo.EventTickets.TicketHolder"] = "Ticket Holder",
            ["Plugin.Apollo.EventTickets.TicketAdmit"] = "Admit:",

            // Update Existing or Shared Resources

            ["Order.GetPDFInvoice"] = "Print Ticket or Invoice",
            ["Plugin.Apollo.Appointments.Book"] = "Book",
            ["Plugin.Apollo.Appointments.Rent"] = "Rent",

        };

        if (install)
        {
            //add or update locales

            await _localizationService.AddOrUpdateLocaleResourceAsync(resourcesList);

        }
        else
        {
            foreach (var item in resourcesList)
                await _localizationService.DeleteLocaleResourceAsync(item.Key);
        }
    }

    public async Task InstallEventTicketsProductsAsync(bool install)
    {

        var productTemplates = new List<ProductTemplate>
        {
            new ProductTemplate
            {
                Name = "Event Tickets",
                ViewPath = "ProductTemplate.EventTickets",
                DisplayOrder = 500,
                IgnoredProductTypes = string.Empty
            }
        };

        if (install)
            await _productTemplateRepository.InsertAsync(productTemplates);
        else
        {
            var template = _productTemplateRepository.Table.FirstOrDefault(pt => pt.Name == "Event Tickets");
            if (template != null)
                await _productTemplateRepository.DeleteAsync(template);
        }

        var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
        if (eaGeneral == null)
            throw new Exception("Default email account cannot be loaded");

        var messageTemplates = new List<MessageTemplate>
        {
            new MessageTemplate
            {
                Name = "OrderPlaced.CustomerNotification.EventTicket",
                Subject = "Rental Order receipt from %Store.Name%.",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = eaGeneral.Id
            },
            new MessageTemplate
            {
                Name = "OrderCompleted.CustomerNotification.EventTicket",
                Subject = "%Store.Name%. Your Rental order completed",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Your order has been completed. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = eaGeneral.Id
            },
            new MessageTemplate
            {
                Name = "OrderCompleted.EmailListNotification.EventTicket",
                Subject = "%Store.Name%. Rental created",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Dear %Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}A booking has been created. Below is the summary of the details.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = eaGeneral.Id
            },
            new MessageTemplate
            {
                Name = "OrderCancelled.CustomerNotification.EventTicket",
                Subject = "%Store.Name%. Your Rental order cancelled",
                Body = $"<p><a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />Hello %Order.CustomerFullName%,{Environment.NewLine}<br />Your order has been cancelled. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />Order Number: % Order.OrderNumber %{ Environment.NewLine}< br />   Order Details: <a href = \"%Order.OrderURLForCustomer%\" target=\"_blank\">% Order.OrderURLForCustomer %</ a >{ Environment.NewLine}< br />Date Ordered: % Order.CreatedOn %{ Environment.NewLine}< br />{ Environment.NewLine}< br /> % Order.Product(s) %{ Environment.NewLine}</p>",
                IsActive = true,
                EmailAccountId = eaGeneral.Id
            },
            new MessageTemplate
            {
                Name = "OrderCancelled.EmailListNotification.EventTicket",
                Subject = "%Store.Name%. Rental cancelled",
                Body = $"<p><a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />Dear %Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />This booking has been cancelled. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />Order Number: % Order.OrderNumber %{ Environment.NewLine}<br />   Order Details: <a href = \"%Order.OrderURLForCustomer%\" target=\"_blank\">% Order.OrderURLForCustomer %</a>{ Environment.NewLine}< br />{ Environment.NewLine}< br /> % Order.Product(s) %</ p>{ Environment.NewLine}",
                IsActive = true,
                EmailAccountId = eaGeneral.Id
            },
            new MessageTemplate
            {
                Name = "OrderReminder.CustomerNotification.EventTicket",
                Subject = "Rental Booking Reminder from %Store.Name%.",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Thanks for booking with <a href=\"%Store.URL%\">%Store.Name%</a>. This is a reminder for your Booking.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = eaGeneral.Id
            },
            new MessageTemplate
            {
                Name = "SummaryReportNotification.EventTicket",
                Subject = "Event Summary Report for %Store.Name%",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Please find attached the Event Summary Report{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = eaGeneral.Id
            },
            new MessageTemplate
            {
                Name = "DoorReportNotification.EventTicket",
                Subject = "Event Door Report for %Product.Name%",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Please find attached the Event Door Report for %Product.Name%{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = eaGeneral.Id
            }
        };

        if (install)
            await _messageTemplateRepository.InsertAsync(messageTemplates);
        else
        {
            var template = _messageTemplateRepository.Table.FirstOrDefault(mt => mt.Name == "OrderPlaced.CustomerNotification.EventTicket");
            if (template != null)
                await _messageTemplateRepository.DeleteAsync(template);
            template = _messageTemplateRepository.Table.FirstOrDefault(mt => mt.Name == "OrderCompleted.CustomerNotification.EventTicket");
            if (template != null)
                await _messageTemplateRepository.DeleteAsync(template);
            template = _messageTemplateRepository.Table.FirstOrDefault(mt => mt.Name == "OrderCompleted.EmailListNotification.EventTicket");
            if (template != null)
                await _messageTemplateRepository.DeleteAsync(template);
            template = _messageTemplateRepository.Table.FirstOrDefault(mt => mt.Name == "OrderCancelled.CustomerNotification.EventTicket");
            if (template != null)
                await _messageTemplateRepository.DeleteAsync(template);
            template = _messageTemplateRepository.Table.FirstOrDefault(mt => mt.Name == "OrderCancelled.EmailListNotification.EventTicket");
            if (template != null)
                await _messageTemplateRepository.DeleteAsync(template);
            template = _messageTemplateRepository.Table.FirstOrDefault(mt => mt.Name == "OrderReminder.CustomerNotification.EventTicket");
            if (template != null)
                await _messageTemplateRepository.DeleteAsync(template);
            template = _messageTemplateRepository.Table.FirstOrDefault(mt => mt.Name == "SummaryReportNotification.EventTicket");
            if (template != null)
                await _messageTemplateRepository.DeleteAsync(template);
            template = _messageTemplateRepository.Table.FirstOrDefault(mt => mt.Name == "DoorReportNotification.EventTicket");
            if (template != null)
                await _messageTemplateRepository.DeleteAsync(template);
        }

        var taxCategories = new List<TaxCategory>
        {
            new TaxCategory
            {
                Name = "Event Tickets",
                DisplayOrder = 100
            }
        };

        if (install)
            await _taxCategoryRepository.InsertAsync(taxCategories);
        else
        {
            foreach (var taxCategory in taxCategories)
            {
                var tCategory = _taxCategoryRepository.Table.FirstOrDefault(tc => tc.Name == taxCategory.Name);
                if (tCategory != null)
                    await _taxCategoryRepository.DeleteAsync(tCategory);
            }
        }

        var productAttributes = new List<ProductAttribute>
        {
            new ProductAttribute
            {
                Name = "Document Link"
            }
        };

        if (install)
        {
            foreach (var productAttribute in productAttributes)
            {
                var pAttributes = _productAttributeRepository.Table.FirstOrDefault(pa => pa.Name == productAttribute.Name);
                if (pAttributes == null)
                    await _productAttributeRepository.InsertAsync(productAttribute);
            }
        }
        else
        {
            // Uninstall sample data for All Stores

            foreach (var productAttribute in productAttributes)
            {
                var pAttributes = _productAttributeRepository.Table.FirstOrDefault(pa => pa.Name == productAttribute.Name);
                if (pAttributes != null)
                    await _productAttributeRepository.DeleteAsync(pAttributes);
            }
        }

        //pictures
        var pictureService = EngineContext.Current.Resolve<IPictureService>();
        var sampleImagesPath = GetSamplesPath();

        var categoryTemplateInGridAndLines = _categoryTemplateRepository
            .Table.FirstOrDefault(pt => pt.Name == "Products in Grid or Lines");
        if (categoryTemplateInGridAndLines == null)
            throw new Exception("Category template cannot be loaded");

        //categories
        var allCategories = new List<Category>();

        if (install)
        {
            var category = _categoryRepository.Table.FirstOrDefault(c => c.Name == "Event Tickets");
            if (category == null || category.Deleted)
            {
                if (category == null)
                {
                    category = new Category
                    {
                        Name = "Event Tickets",
                        CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                        AllowCustomersToSelectPageSize = false,
                        PageSize = 6,
                        PageSizeOptions = "6, 3, 9",
                        PictureId = await InsertPictureAsync(sampleImagesPath + "category_eventtickets.jpg", "Event Tickets"),
                        ShowOnHomepage = true,
                        Published = true,
                        DisplayOrder = 1,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    await _categoryRepository.InsertAsync(category);
                }

                category = _categoryRepository.Table.FirstOrDefault(c => c.Name == "Event Tickets");

                if (category != null)
                {
                    if (category.Deleted)
                    {
                        category.Deleted = false;
                        await _categoryRepository.UpdateAsync(category);
                    }

                    allCategories.Add(category);

                    await _urlRecordRepository.InsertAsync(new UrlRecord
                    {
                        EntityId = category.Id,
                        EntityName = typeof(Category).Name,
                        LanguageId = 0,
                        IsActive = true,
                        Slug = await _urlRecordService.ValidateSeNameAsync(category, string.Empty, category.Name, true)
                    });
                }
            }
        }
        else
        {
            var category = _categoryRepository.Table.FirstOrDefault(c => c.Name == "Event Tickets");
            if (category != null)
            {
                string name = await pictureService.GetPictureSeNameAsync("Event Tickets");
                var picture = _pictureRepository.Table.FirstOrDefault(p => p.SeoFilename == name);
                if (picture != null)
                    await _pictureRepository.DeleteAsync(picture);

                var slug = await _urlRecordService.ValidateSeNameAsync(category, string.Empty, category.Name, true);
                var urlRecord = _urlRecordRepository.Table.FirstOrDefault(u => u.Slug == slug);
                if (urlRecord != null)
                    await _urlRecordRepository.DeleteAsync(urlRecord);

                await _categoryRepository.DeleteAsync(category);
            }
        }

        //default store
        var defaultStore = _storeRepository.Table.FirstOrDefault();
        if (defaultStore == null)
            throw new Exception("No default store could be loaded");

        var vendor = new Vendor
        {
            Name = "The Ticket Venue",
            Email = "tickets@gmail.com",
            Description = "The event Vendor sells tickets",
            AdminComment = string.Empty,
            PictureId = 0,
            Active = true,
            DisplayOrder = 1,
            PageSize = 6,
            AllowCustomersToSelectPageSize = true,
            PageSizeOptions = "6, 3, 9, 18"
        };

        if (install)
        {
            await _vendorService.InsertVendorAsync(vendor);

            await _urlRecordRepository.InsertAsync(new UrlRecord
            {
                EntityId = vendor.Id,
                EntityName = typeof(Vendor).Name,
                LanguageId = 0,
                IsActive = true,
                Slug = await _urlRecordService.ValidateSeNameAsync(vendor, string.Empty, vendor.Name, true)
            });

        }
        else
        {
            var slug = await _urlRecordService.ValidateSeNameAsync(vendor, string.Empty, vendor.Name, true);
            var urlRecord = _urlRecordRepository.Table.FirstOrDefault(u => u.Slug == slug);
            if (urlRecord != null)
                await _urlRecordRepository.DeleteAsync(urlRecord);

            await _vendorService.DeleteVendorAsync(vendor);
        }

        //products
        var allProducts = new List<Product>();

        //related products
        var relatedProducts = new List<RelatedProduct>();

        //Install Event Tickets Products
        await InstallEventTicketsAsync(install, allProducts, sampleImagesPath, pictureService, relatedProducts, vendor);

        //search engine names
        foreach (var product in allProducts)
        {
            if (install)
            {
                await _eventTicketService.InsertEventTicketProductTypeExtensionAsync(product, null, null, "7pm");

                await _urlRecordRepository.InsertAsync(new UrlRecord
                {
                    EntityId = product.Id,
                    EntityName = typeof(Product).Name,
                    LanguageId = 0,
                    IsActive = true,
                    Slug = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true)
                });

            }
            else
            {
                await _eventTicketService.DeleteEventTicketProductTypeExtensionAsync(product);

                var slug = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true);
                var urlRecord = _urlRecordRepository.Table.FirstOrDefault(u => u.Slug == slug);
                if (urlRecord != null)
                    await _urlRecordRepository.DeleteAsync(urlRecord);
            }
        }

    }

    protected async Task InstallEventTicketsAsync(bool install, List<Product> allProducts, string sampleImagesPath,
        IPictureService pictureService, List<RelatedProduct> relatedProducts, Vendor vendor)
    {

        if (install)
        {
            var productTemplate = _productTemplateRepository.Table.FirstOrDefault(pt => pt.Name == "Event Tickets");
            if (productTemplate == null)
                throw new Exception("Event Tickets template could not be loaded");

            var productEventType = new Product
            {
                ProductType = ProductType.GroupedProduct,
                VisibleIndividually = true,
                Name = "Template - Event Ticket Product",
                Sku = "ET-GP-1",
                ShortDescription = "This a short decription for and Event Ticket Product",
                FullDescription = "<p>This is the Full Description for a group for tickets available for an Event</p>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Price = 0M,
                OldPrice = 0M,
                IsShipEnabled = false,
                IsFreeShipping = false,
                Weight = 0,
                Length = 0,
                Width = 0,
                Height = 0,
                TaxCategoryId = _taxCategoryRepository.Table.FirstOrDefault(tc => tc.Name == "Event Tickets").Id,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                StockQuantity = 1,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = false,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = _maximumTicketQuantity,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                VendorId = vendor.Id,
                Gtin = "7pm",
                ShowOnHomepage = true
            };

            await _productRepository.InsertAsync(productEventType);

            var category = _categoryRepository.Table.Where(p => p.Name == "Event Tickets").FirstOrDefault();
            if (category != null && productEventType != null)
            {
                InsertInstallationData(new ProductCategory
                {
                    ProductId = productEventType.Id,
                    CategoryId = category.Id,
                    DisplayOrder = 1
                });
            }

            allProducts.Add(productEventType);

            await InsertProductPictureAsync(productEventType, sampleImagesPath, "product_eventtickets.jpg");

            await AddProductTagAsync(productEventType, "Ticket");

            var productEventTicket = new Product
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = false,
                ParentGroupedProductId = productEventType.Id,
                Name = "Template - Event Ticket",
                Sku = "ET-1",
                ShortDescription = "Template - Event Ticket",
                FullDescription = "<p>This is a ticket for an Event</p>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Price = 20M,
                OldPrice = 20M,
                IsShipEnabled = false,
                IsFreeShipping = false,
                Weight = 0,
                Length = 0,
                Width = 0,
                Height = 0,
                TaxCategoryId = _taxCategoryRepository.Table.FirstOrDefault(tc => tc.Name == "Event Tickets").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10,
                NotifyAdminForQuantityBelow = 5,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = false,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10,
                AllowedQuantities = "1,2,3,4,5,6,7,8,9,10",
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                DisplayOrder = 1,
                VendorId = vendor.Id,
                Gtin = "7pm"
            };

            await _productRepository.InsertAsync(productEventTicket);

            allProducts.Add(productEventTicket);

            await InsertProductPictureAsync(productEventTicket, sampleImagesPath, "product_event.jpg");

            var productSimpleTicket = new Product
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                ParentGroupedProductId = 0,
                Name = "Template - Single Ticket",
                Sku = "ET-S-1",
                ShortDescription = "Template - Single Ticket",
                FullDescription = "<p>This is a ticket for an Event</p>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Price = 20M,
                OldPrice = 20M,
                IsShipEnabled = false,
                IsFreeShipping = false,
                Weight = 0,
                Length = 0,
                Width = 0,
                Height = 0,
                TaxCategoryId = _taxCategoryRepository.Table.FirstOrDefault(tc => tc.Name == "Event Tickets").Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10,
                NotifyAdminForQuantityBelow = 5,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = false,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                DisplayOrder = 1,
                VendorId = vendor.Id,
                Gtin = "7pm",
                ShowOnHomepage = true
            };

            await _productRepository.InsertAsync(productSimpleTicket);

            category = _categoryRepository.Table.Where(p => p.Name == "Event Tickets").FirstOrDefault();
            if (category != null && productEventType != null)
            {
                InsertInstallationData(new ProductCategory
                {
                    ProductId = productSimpleTicket.Id,
                    CategoryId = category.Id,
                    DisplayOrder = 2
                });
            }

            allProducts.Add(productSimpleTicket);

            await InsertProductPictureAsync(productSimpleTicket, sampleImagesPath, "product_event.jpg");

            await AddProductTagAsync(productSimpleTicket, "Ticket");

        }
        else
        {
            await DeleteProductTagAsync("Ticket");

            var product = _productRepository.Table.Where(p => p.Name == "Template - Event Ticket");
            foreach (var p in product)
            {
                if (p.Deleted)
                    continue;
                await DeleteProductAsync(p);
            }

            product = _productRepository.Table.Where(p => p.Name == "Template - Event Ticket Product");
            foreach (var p in product)
            {
                if (p.Deleted)
                    continue;
                await DeleteProductAsync(p);
            }

            product = _productRepository.Table.Where(p => p.Name == "Template - Single Ticket");
            foreach (var p in product)
            {
                if (p.Deleted)
                    continue;
                await DeleteProductAsync(p);
            }
        }
    }

    #endregion

}
