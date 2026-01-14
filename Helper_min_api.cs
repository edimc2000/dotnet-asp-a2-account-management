//using AccountManagement.Support;

using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Text.Json;

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


    // Response models
    /// <summary>
    /// Standard API response
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        /// <example>false</example>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        /// <example>Malformed JSON in request body</example>
        public string Message { get; set; }
    }

// Generic response with data
    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }

    public class AccountResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
    }


    public class ApiResponseMalformed
    {
        [DefaultValue(false)] public bool Success { get; set; }

        [DefaultValue("Malformed JSON in request body")]
        public string Message { get; set; }
    }


    public class ApiResponseNull
    {
        [DefaultValue(false)] 
        public bool Success { get; set; }

        [DefaultValue("Unable to parse JSON body")]
        public string Message { get; set; }
    }

}