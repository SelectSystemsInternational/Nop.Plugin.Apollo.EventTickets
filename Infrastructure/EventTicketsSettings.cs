using Nop.Core.Configuration;

namespace Nop.Plugin.Apollo.EventTickets.Settings;

public class EventTicketSettings : ISettings
{
    public EventTicketSettings()
    {
        SupportedStoreThemes = string.Empty;
        PublicKey = string.Empty;
        PrivateKey = string.Empty;
        Enabled = false;
        EventTicketsEnabled = false;
        ProductAttributeIds = string.Empty;
        MaximumNumberOfOrders = 0;
        DeleteTablesOnUninstall = false;
        DeleteConfigurationDataOnUninstall = false;
        AttachFileLocation = string.Empty;
        EventTicketsURLPattern = string.Empty;
        CheckoutAttributeIds = string.Empty;
        DefaultReportDateTime = string.Empty;
        TicketFontColor = "#000000"; // Default to black
        FontFileName = string.Empty;
    }

    public string SupportedStoreThemes { get; set; }

    public string PublicKey { get; set; }

    public string PrivateKey { get; set; }

    public bool Enabled { get; set; }

    public bool EventTicketsEnabled { get; set; }

    public string EventTicketsURLPattern { get; set; }

    public string ProductAttributeIds { get; set; }

    public int MaximumNumberOfOrders { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tables should be deleted on plugin uninstall
    /// </summary>
    public bool DeleteTablesOnUninstall { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the configuration should be deleted on plugin uninstall
    /// </summary>
    public bool DeleteConfigurationDataOnUninstall { get; set; }

    public string AttachFileLocation { get; set; }

    public bool UseGroupProducts { get; set; }

    public bool UseAssociatedProducts { get; set; }

    public string CheckoutAttributeIds { get; set; }

    public string DefaultReportDateTime { get; set; }

    public int OffSaleTimeInMinutes { get; set; }

    public int UnPublishTimeInHours { get; set; }

    public bool TestMode { get; set; }

    public int DefaultStoreId { get; set; }

    public bool UseNopTemplatesAjaxCart { get; set; }

    public bool SeperateTickets { get; set; }

    public float LogoSize { get; set; }

    public string TicketFontColor { get; set; }

    public string FontFileName { get; set; }

}

public class EventTicketsStatusColorSettings : ISettings
{
    public EventTicketsStatusColorSettings()
    {
        EventTicketsStatusClosed = "#0000FF";             // Blue
        EventTicketsStatusBooked = "#FF0000";             // Red
        EventTicketsStatusAvailable = "#008000";          // Green
        EventTicketsStatusNotAvailable = "#008B8B";       // DarkCyan
        EventTicketsStatusLowAvailability = "#FFA500";    // Orange,
        EventTicketsStatusDepositPaid = "#FF00FF";        // Fuchsia
        EventTicketsStatusNotPaid = "#FF0000";            // Red;
        EventTicketsStatusPaid = "#008000";               // Green
        EventTicketsStatusProcessing = "#FF7F50";         // Coral
        EventTicketsStatusPickedUp = "#808000";           // Olive
        EventTicketsStatusReturned = "#800080";           // Purple
        EventTicketsStatusConfiguration = "#B0BEC5";      // BlueGrey
    }

    public string EventTicketsStatusClosed { get; set; }

    public string EventTicketsStatusBooked { get; set; }

    public string EventTicketsStatusAvailable { get; set; }

    public string EventTicketsStatusNotAvailable { get; set; }

    public string EventTicketsStatusLowAvailability { get; set; }

    public string EventTicketsStatusDepositPaid { get; set; }

    public string EventTicketsStatusNotPaid { get; set; }

    public string EventTicketsStatusPaid { get; set; }

    public string EventTicketsStatusProcessing { get; set; }

    public string EventTicketsStatusPickedUp { get; set; }

    public string EventTicketsStatusReturned { get; set; }

    public string EventTicketsStatusConfiguration { get; set; }

}

public class EventTicketsAccessSettings : ISettings
{
    public bool AccessSystem { get; set; }

    public bool AccessEventTickets { get; set; }

}
