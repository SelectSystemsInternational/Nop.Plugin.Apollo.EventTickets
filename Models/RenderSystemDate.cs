using Nop.Web.Framework.Models.DataTables;

namespace Nop.Plugin.Apollo.EventTickets.Models;

/// <summary>
/// Represents date render for DataTables column
/// </summary>
public partial class RenderSystemDate : RenderDate
{
    #region Constants

    /// <summary>
    /// Default date format
    /// </summary>
    /// private string DEFAULT_DATE_FORMAT = "DD/MM/YYYY HH:mm:ss";

    #endregion

    #region Ctor

    public RenderSystemDate(string systemFormat)
    {
        //set default values
        Format = systemFormat;
    }

    #endregion
}