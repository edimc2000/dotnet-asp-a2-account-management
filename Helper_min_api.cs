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


    //// Response models
    ///// <summary>
    ///// Standard API response
    ///// </summary>
    //public class ApiResponse
    //{
    //    /// <summary>
    //    /// Indicates if the request was successful
    //    /// </summary>
    //    /// <example>false</example>
    //    public bool Success { get; set; }

    //    /// <summary>
    //    /// Response message
    //    /// </summary>
    //    /// <example>Malformed JSON in request body</example>
    //    public string Message { get; set; }
    //}

//// Generic response with data
//    public class ApiResponse<T> : ApiResponse
//    {
//        public T Data { get; set; }
//    }

//    public class AccountData
//    {
//        /// <summary>
//        /// Indicates if the request was successful
//        /// </summary>
//        /// <example>false</example>
//        public int Id { get; set; }

//        /// <summary>
//        /// Indicates if the request was successful
//        /// </summary>
//        /// <example>false</example>
//        public string FirstName { get; set; }

//        /// <summary>
//        /// Indicates if the request was successful
//        /// </summary>
//        /// <example>false</example>
//        public string LastName { get; set; }

//        /// <summary>
//        /// Indicates if the request was successful
//        /// </summary>
//        /// <example>false</example>
//        public string EmailAddress { get; set; }
//    }


    //public class ApiResponseMalformed
    //{
    //public bool Success { get; set; }

    
    //    public string Message { get; set; }
    //}


    //public class ApiResponseNull
    //{
    //    [DefaultValue(false)] 
    //    public bool Success { get; set; }

    //    [DefaultValue("Unable to parse JSON body")]
    //    public string Message { get; set; }
    //}

  
  


    //public class ApiResponseDuplicate
    //{
    //    /// <summary>
    //    /// Always false for error responses
    //    /// </summary>
    //    /// <example>false</example>
    //    public bool Success { get; set; } = false;
    
    //    /// <summary>
    //    /// Error message describing the duplicate account issue
    //    /// </summary>
    //    /// <example>Account with email user@example.com already exists</example>
    //    public string Message { get; set; }
    
    //    /// <summary>
    //    /// Error code for programmatic handling
    //    /// </summary>
    //    /// <example>DUPLICATE_ACCOUNT</example>
    //    public string ErrorCode { get; set; } = "DUPLICATE_ACCOUNT";
    
    //    /// <summary>
    //    /// The email that caused the conflict
    //    /// </summary>
    //    /// <example>user@example.com</example>
    //    public string DuplicateEmail { get; set; }
    
    //    public ApiResponseDuplicate(string email = null)
    //    {
    //        Message = email != null 
    //            ? $"Account with email {email} already exists" 
    //            : "Account already exists";
    //        DuplicateEmail = email;
    //    }
    //}


}