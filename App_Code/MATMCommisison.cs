using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for MATMCommisison
/// </summary>
public class MATMCommisison
{
    [Required]
    [RegularExpression(@"^[0-9]{3,3}$", ErrorMessage = "error Message ")]
    public Int32 StartVal;

    [Range(3001, 10000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public Int32 EndVal;
    public MATMCommisison()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}