using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SlabModel
/// </summary>
public class SlabModel
{
    public Int32 OpID { get; set; }
    public decimal startVal { get; set; }
    public decimal endVal { get; set; }
    public decimal surcharge { get; set; }
    public decimal Gst { get; set; }
    public bool IsGst { get; set; }
    public bool IsPlusMinGst { get; set; }
    public decimal Tds { get; set; }
    public bool IsTds { get; set; }
}