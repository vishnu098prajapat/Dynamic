using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IAPIService
/// </summary>
public interface IAPIService
{
    Task<DataTable> GetAPIIDData(string APIID);
}