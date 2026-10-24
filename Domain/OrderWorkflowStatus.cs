using System.Drawing;

using Nop.Core.Infrastructure;
using Nop.Services.Configuration;

using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets.Domain;

public enum EventTicketsStatus
{

    /// <summary>
    ///     Closed
    /// </summary>
    Closed = 1,

    /// <summary>
    ///     Booked
    /// </summary>
    Booked = 2,

    /// <summary>
    ///     Available
    /// </summary>
    Available = 3,

    /// <summary>
    ///     Not Available
    /// </summary>
    NotAvailable = 4,

    /// <summary>
    ///     Low Availability
    /// </summary>
    LowAvailability = 5,

    /// <summary>
    ///    Deposit Paid
    /// </summary>
    DepositPaid = 6,

    /// <summary>
    ///     Not Paid
    /// </summary>
    NotPaid = 7,

    /// <summary>
    ///     Paid
    /// </summary>
    Paid = 8,

    /// <summary>
    ///     Items picked up for Rental
    /// </summary>
    Processing = 10,

    /// <summary>
    ///     Items picked up for Rental
    /// </summary>
    PickedUp = 20,

    /// <summary>
    ///     Items returned for Rental
    /// </summary>
    Returned = 30,

    /// <summary>
    ///     Configuration
    /// </summary>
    Configuration = 100,

}

public enum EventTicketsColor
{
    Green = 0,
    Brown = 1,
    Gray = 2,
    Black = 3,
    Coral = 4,
    Maroon = 5,
    IndianRed = 6,
    Olive = 7,
    Lime = 8,
    AquaMarine = 9,
    Aqua = 10,
    DarkSeaGreen = 11,
    LightGreen = 12,
    Navy = 13,
    SteelBlue = 14,
    Pink = 15,
    Purple = 16,
    OrangeRed = 17
}

public class EventTicketsExtensions
{

    /// <summary>
    ///     Gets the EventTickets Status
    /// </summary>
    ///

    public EventTicketsStatus Status(int status)
    {
        switch (status)
        {
            case 1:
                return EventTicketsStatus.Closed;
            case 2:
                return EventTicketsStatus.Booked;
            case 3:
                return EventTicketsStatus.Available;
            case 4:
                return EventTicketsStatus.NotAvailable;
            case 5:
                return EventTicketsStatus.LowAvailability;
            case 6:
                return EventTicketsStatus.DepositPaid;
            case 7:
                return EventTicketsStatus.NotPaid;
            case 10:
                return EventTicketsStatus.Processing;
            case 20:
                return EventTicketsStatus.PickedUp;
            case 30:
                return EventTicketsStatus.Returned;
            case 100:
                return EventTicketsStatus.Configuration;
        }

        return 0;
    }

    /// <summary>
    ///     Gets the color for the status
    /// </summary>

    public static async Task<string> EventTicketsColourForStatus(int status)
    {

        var settingService = EngineContext.Current.Resolve<ISettingService>();         
        var eventTicketsColorSettings = await settingService.LoadSettingAsync<EventTicketsStatusColorSettings>();

        Color color;

        switch (status)
        {

            case 1:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusClosed); //Blue
                break;
            
            case 2:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusBooked); //Red
                break;
            
            case 3:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusAvailable); //Green
                break;
            
            case 4:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusNotAvailable); //DarkCyan
                break;
            
            case 5:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusLowAvailability); //Orange,
                break;
            
            case 6:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusDepositPaid); //Fuchsia
                break;
            
            case 7:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusNotPaid); //Red
                break;
            
            case 8:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusPaid); //Green
                break;

            case 10:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusProcessing); // Coral
                break;

            case 20:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusProcessing); // Olive
                break;
            
            case 30:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusReturned); // Purple
                break;
            
            case 100:
                color = ColorTranslator.FromHtml(eventTicketsColorSettings.EventTicketsStatusConfiguration); // BlueGrey
                break;

            case 0:
            default:
                color = ColorTranslator.FromHtml("#FFFFFF");
                break;
        }

        string colour = System.Drawing.ColorTranslator.ToHtml(color);

        return colour;
    }

    /// <summary>
    ///     Gets the status from the color
    /// </summary>
    public static EventTicketsStatus EventTicketsColourStatus(string status)
    {
        EventTicketsStatus result = 0;

        switch (status)
        {
            case "#0000FF": // Blue
                result = EventTicketsStatus.Closed;
                break;
            case "#FF0000": // Red
                result = EventTicketsStatus.Booked;
                break;
            case "#008000": // Green
                result = EventTicketsStatus.Available;
                break;
            case "#008B8B": // DarkCyan
                result = EventTicketsStatus.NotAvailable;
                break;
            case "#FFA500": // Orange
                result = EventTicketsStatus.LowAvailability;
                break;
            case "#FF00FF": // Fuchsia
                result = EventTicketsStatus.DepositPaid;
                break;
        }

        return (result);
    }

}
