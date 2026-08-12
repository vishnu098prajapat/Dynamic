using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ICheckValue
/// </summary>
public interface ICheckValue 
{
    bool IsAnyNullOrEmpty(object myObject);
}