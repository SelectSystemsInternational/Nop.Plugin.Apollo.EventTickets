namespace Nop.Plugin.Apollo.EventTickets.Services;


public partial interface IEventTicketsInstallService
{

    #region Methods

    public Task InstallEventTicketsProductsAsync(bool install);

    public Task InstallLocalisationAsync(bool install);

    public Task InstallPermissionsAsync(bool install);

    public Task InstallTasksAsync(bool install);

    #endregion

}
