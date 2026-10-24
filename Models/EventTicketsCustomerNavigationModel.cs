using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Apollo.EventTickets.Models;

public enum EventTicketsCustomerNavigationEnum
{
    Info = 0,
    Addresses = 10,
    Orders = 20,
    BackInStockSubscriptions = 30,
    ReturnRequests = 40,
    DownloadableProducts = 50,
    RewardPoints = 60,
    ChangePassword = 70,
    Avatar = 80,
    ForumSubscriptions = 90,
    ProductReviews = 100,
    VendorInfo = 110,
    GdprTools = 120,
    CheckGiftCardBalance = 130,
    CustomerCalendar = 200
}