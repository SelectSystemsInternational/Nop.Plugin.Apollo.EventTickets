using Microsoft.AspNetCore.Mvc;

using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Apollo.Core.Services;
using Nop.Plugin.Apollo.EventTickets.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Controllers;
using Nop.Web.Factories;

namespace Nop.Plugin.Apollo.EventTickets.Controllers.Public;

public partial class EventTicketsOrderController : OrderController
{

    #region Fields

    protected readonly IUrlRecordService _urlRecordService;
    protected readonly IProductService _productService;
    protected readonly IApolloCoreService _apolloCoreService;

    #endregion

    #region Ctor

    public EventTicketsOrderController(ILocalizationService localizationService,
        INotificationService notificationService, 
        ICustomerService customerService,
        IOrderModelFactory orderModelFactory,
        IOrderProcessingService orderProcessingService, 
        IOrderService orderService, 
        IPaymentService paymentService, 
        IPdfService pdfService,
        IShipmentService shipmentService, 
        IWebHelper webHelper,
        IWorkContext workContext,
        OrderSettings orderSettings,
        RewardPointsSettings rewardPointsSettings,
        IUrlRecordService urlRecordService,
        IProductService productService,
        IApolloCoreService apolloCoreService) : base(customerService,
            localizationService,
            notificationService,
            orderModelFactory,
            orderProcessingService,
            orderService,
            paymentService,
            pdfService,
            shipmentService,
            webHelper,
            workContext,
            orderSettings,
            rewardPointsSettings)
    {
        _urlRecordService = urlRecordService;
        _productService = productService;
        _apolloCoreService = apolloCoreService;
    }

    #endregion

    #region Methods

    //My account / Order details page / re-order
    public override async Task<IActionResult> ReOrder(int orderId)
    {
        string url = _webHelper.GetStoreLocation();

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
            return Challenge();

        var orderItem = (await _orderService.GetOrderItemsAsync(order.Id)).FirstOrDefault();

        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
        if (product != null)
        {
            var seName = await _urlRecordService.GetSeNameAsync(product);
            url = _webHelper.GetStoreLocation() + seName;
            if (_apolloCoreService.IsEventTicket(product))
            {
                return RedirectToRoute("Nop.Plugin.Apollo.Front.Accommodation", new { SeName = seName });
            }
            else
            {
                await _orderProcessingService.ReOrderAsync(order);
                url = "ShoppingCart";
            }
        }
        
        return RedirectToRoute(url);

    }
    
    #endregion

}