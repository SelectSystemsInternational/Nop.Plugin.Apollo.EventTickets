
namespace Nop.Plugin.Apollo.EventTickets.Models;

public partial class ExportEventTicketsModel  
{
    public ExportEventTicketsModel()
    {
        ProductName = string.Empty;
        RelatedDate = string.Empty;
        EndDate = string.Empty;
        Status = string.Empty;
        ProductCost = string.Empty;
        OrderProductVarinatID = string.Empty;
        Available = string.Empty;
        Remaining = string.Empty;
        OrderDescription = string.Empty;
        PurchaseNote = string.Empty;
        WaitingList = string.Empty;
        Color = string.Empty;
        SKU = string.Empty;
    }

    #region Properties

    public int ProductId { get; set; }        
   
    public string ProductName { get; set; }

    public string RelatedDate { get; set; }

    public string EndDate { get; set; }

    public string Status { get; set; }

    public string ProductCost { get; set; }

    public string OrderProductVarinatID { get; set; }

    public string Available { get; set; }

    public string Remaining { get; set; }

    public string OrderDescription { get; set; }

    public string PurchaseNote { get; set; }

    public string WaitingList { get; set; }

    public string Color { get; set; }

    public string SKU { get; set; }
    #endregion
}
