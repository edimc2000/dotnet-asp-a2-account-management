//using AccountManagement.Support;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

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

    /// <summary>Returns successful all accounts response.</summary>
    /// <param name="accountList">The list of Account objects to return in the response.</param>
    /// <returns>An IResult containing the success response with account data.</returns>
    public static IResult SearchSuccess(List<Account> accountList)
    {
        string textMessage = $"{
            (accountList.Count < 1 ? "There are no accounts on the database"
                : accountList.Count > 1
                    ? $"Total of {accountList.Count} accounts retrieved successfully"
                    : "Account retrieved successfully")}";

        return Results.Ok(new
        {
            success = true,
            message = textMessage,
            data = accountList.ToList()
        });
    }



    //public static IResult AddSuccess(List<Account> newAccount)
    public static IResult AddSuccess(Account newAccount, int newIdNumber)
    {
        return Results.CreatedAtRoute( "GetAccountById",new {id=newIdNumber},new
        {
            success = true,
            message = "Account created successfully",
            data = newAccount
        });
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


    /// <summary>Returns formatted bad request response.</summary>
    /// <param name="message">Error message.</param>
    public static IResult ConflictResult(string message)
    {
        return Results.Conflict(new
        {
            success = false,
            message = $"{message}"
        });
    }



    public static bool TryUpdatePropertyIfChanged<T>( bool hasChanges, 
        T currentValue, T newValue, Action<T> setter)
        where T : IEquatable<T>
    {
        if (newValue != null && !currentValue.Equals(newValue))
        {
            setter(newValue);
            return true;
        }
        return false;
    }




}