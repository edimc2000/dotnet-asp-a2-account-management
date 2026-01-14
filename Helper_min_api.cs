//using AccountManagement.Support;

using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Text.Json;

using Swashbuckle.AspNetCore.Annotations;

//using static AccountManagement.Support.ProductSeed;


namespace AccountManagement;

/// <summary>Helper methods for product API operations.</summary>
/// <para>Author: Eddie C.</para>
/// <para>Version: 1.0</para>
/// <para>Date: Jan. 09, 2026</para>
internal partial class Helper
{
    // data conversion json element to account element 

    internal class InputDataConverter
    {
        public JsonElement FirstName { get; set; }
        public JsonElement LastName { get; set; }
        public JsonElement EmailAddress { get; set; }
    }

    /// <summary>Returns formatted bad request response.</summary>
    /// <param name="message">Error message.</param>
    public static IResult BadRequest(string message)
    {
        return Results.BadRequest(new
        {
            success = false,
            message = $"{message}"
        });
    }
    public static IResult UnprocessableEntity(string message)
    {
        return Results.UnprocessableEntity(new
        {
            success = false,
            message = $"{message}"
        });
    }


    /// <summary>Returns successful all products response.</summary>
    public static IResult SearchAllSuccess( List<Account> accountList)
    {
        return Results.Ok(new
        {
            success = true,
            message = $"Total of {accountList.Count} products retrieved successfully",
            data = accountList.ToList()
        });
    }

}