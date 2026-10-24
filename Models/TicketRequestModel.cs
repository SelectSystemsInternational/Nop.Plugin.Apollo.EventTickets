using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Apollo.EventTickets.Models;

public record TicketRequestModel : BaseNopEntityModel
{
    public TicketRequestModel()
    {
        Items = new List<TicketItemModel>();
        BillingAddress = new AddressModel();
    }

    public bool PrintMode { get; set; }

    public string CustomOrderNumber { get; set; }

    public DateTime CreatedOn { get; set; }

    public string OrderStatus { get; set; }

    public AddressModel BillingAddress { get; set; }

    public string PaymentMethod { get; set; }
    public string PaymentMethodStatus { get; set; }

    public bool ShowSku { get; set; }
    public IList<TicketItemModel> Items { get; set; }
    
    public bool ShowVendorName { get; set; }
    

    #region Nested Classes

    public record TicketItemModel : BaseNopEntityModel
    {
        public string Sku { get; set; }

        public int ProductId { get; set; }
        
        public string ProductName { get; set; }

        public string ProductSeName { get; set; }
        
        public int Quantity { get; set; }

        public string AttributeInfo { get; set; }

        public string VendorName { get; set; }

        public string TicketNumber { get; set; }

    }

		#endregion
}