using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Apollo.EventTickets.ExportImport;

/// <summary>
/// Export manager interface
/// </summary>
public partial interface IExportImportManager
{
    Task<byte[]> ExportEventTicketsOrderItemToXlsxAsync(IList<Order> orders);
}
