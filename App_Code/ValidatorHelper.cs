using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public static class ValidatorHelper
{
    public static string Validate<T>(T model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        var isValid = Validator.TryValidateObject(model, context, results, true);

        if (!isValid)
        {
            // Join all errors into a single message (separated by newlines)
            return string.Join(",", results.Select(r => r.ErrorMessage));
        }

        return string.Empty; // means success
    }
}
