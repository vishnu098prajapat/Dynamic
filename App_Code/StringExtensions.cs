using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for StringExtensions
/// </summary>
public static class StringExtensions
{

    public static Int32 ToInt32(this string i)
    {
        return Convert.ToInt32(i);
    }

    public static bool ToBoolen(this string i)
    {
        return Convert.ToBoolean(i);
    }

    public static string NoneImage(this string i)
    {
        return (!string.IsNullOrEmpty(i) ? i : ConstantsData.NoImage);
    }

    public static string IsFlatString(this string i)
    {
        return (i.ToString().ToUpper()=="TRUE"? "Flat" : "Percentage");
    }

    public static string GetColor(this string i)
    {
        return (i.ToUpper()=="SUCCESS" ? "btn btn-success" :(i.ToUpper() == "PENDING" ? "btn btn-warning":"btn btn-danger"));
    }



    public static string ToDescErrorsString(this IEnumerable<ValidationResult> source, string mensajeColeccionVacia = null)
    {

        StringBuilder resultado = new StringBuilder();

        if (source.Count() > 0)
        {
            resultado.AppendLine("There are validation errors:");
            source.ToList()
                .ForEach(
                    s =>
                        resultado.AppendFormat("  {0} --> {1}{2}", s.MemberNames.FirstOrDefault(), s.ErrorMessage,
                            Environment.NewLine));
        }
        else
            resultado.AppendLine(mensajeColeccionVacia ?? string.Empty);

        return resultado.ToString();
    }
}