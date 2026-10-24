namespace Nop.Plugin.Apollo.EventTickets;

/// <summary>
/// Represents Event Tickets helper
/// </summary>
public class SystemHelper
{

    #region Constants

    public class EventTicketDefaults
    {

        /// <summary>
        /// Event Tickets system name
        /// </summary>
        public static string SystemName => "Apollo.EventTickets";

        /// <summary>
        /// Gets a system name of 'administrators' customer role
        /// </summary>
        public static string SummaryReportLastRun => "SummaryReportLastRun";

        /// <summary>
        /// Gets a generic attribute key to order item additional names
        /// </summary>
        public static string OrderItemAdditionalNames => "OrderItemAdditionalNames";

        /// </summary>
        public static string VendorsRoleName => "Vendors";

        /// </summary>
        public static int DisplayOrderDateOffset => 900000000;

    }

    #endregion

    #region Methods

    public Guid DateToGuid(DateTime accessDate)
    {
        var date = new DateTime(accessDate.Year, accessDate.Month, accessDate.Day, accessDate.Hour, accessDate.Minute, 00);
        var guid = date.ToGuid();
        return guid;
    }

    public DateTime GuidToDate(Guid accessGuid)
    {
        var date = accessGuid.ToDateTime();
        return date;
    }

    public string GetDomainNameFromHost(string url)
    {
        string domain = string.Empty;
        if (url.Contains("localhost"))
            domain = "localhost";
        if (url != null)
        {
            string[] names = url.Split(".");
            for (int count = 1; count < names.Count(); count++)
            {
                domain += names[count];
                if (count + 1 < names.Count())
                    domain += ".";
            }
        }

        domain = domain.Replace("/", "");
        return domain;
    }

    #endregion



}

public static class StringExtension
{
    public static string Truncate(this string s, int length)
    {
        return string.IsNullOrEmpty(s) || s.Length <= length ? s
            : length <= 0 ? string.Empty
            : s.Substring(0, length);
    }
}

public static class DateTimeExtensions
{
    public static Guid ToGuid(this DateTime dt)
    {
        var bytes = BitConverter.GetBytes(dt.Ticks);

        Array.Resize(ref bytes, 16);

        return new Guid(bytes);
    }
}

public static class GuidExtensions
{
    public static DateTime ToDateTime(this Guid guid)
    {
        var bytes = guid.ToByteArray();

        Array.Resize(ref bytes, 8);

        return new DateTime(BitConverter.ToInt64(bytes));
    }
}