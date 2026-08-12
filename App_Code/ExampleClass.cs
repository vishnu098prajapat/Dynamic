using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ExampleClass
/// </summary>
public class ExampleClass
{
    public ExampleClass()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public async Task<int> PerformAsyncOperation()
    {
        // Simulate some asynchronous operation
        await Task.Delay(1000); // Delay for 1 second
        return 42; // Return some result
    }

    public async Task ExampleUsage()
    {
        try
        {
            Console.WriteLine("Starting asynchronous operation...");
            int result = await PerformAsyncOperation();
            Console.WriteLine($"Asynchronous operation completed with result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

  
}