using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.CustomCode.Models;

public enum NumberOfNightsOption : byte
{
    [Display(Name = "7")]
    Days7 = 7,

    [Display(Name = "14")]
    Days14 = 14
}
