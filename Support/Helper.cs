using System.Text.Json;

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


    public static IResult AddSuccess(Account newAccount, int newIdNumber)
    {
        return Results.CreatedAtRoute("GetAccountById",
            new { id = newIdNumber },
            new
            {
                success = true,
                message = "Account created successfully",
                data = newAccount
            });
    }



    public static IResult UpdateSuccess(Dictionary<string, object?> changes)
    {
        if (changes.Count == 0)
            return Results.Ok(new
            {
                success = true,
                message =
                    "Request processed successfully. No modifications made. Null fields and " +
                    "empty values were ignored to preserve existing data."
            });
        else
            return Results.Ok(new
            {
                success = true,
                message = "Update successful",
                changes = changes
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


    public static (bool hasChanges, Dictionary<string, object?> changesDict)
        TryUpdatePropertyIfChanged<T>
        (
            bool hasChanges,
            T currentValue,
            T newValue,
            Action<T> setter,
            Dictionary<string, object?> incomingChanges,
            string propertyName
        )
        where T : IEquatable<T>
    {
        bool isDifferent = (currentValue == null
                               ? newValue != null
                               : newValue == null || !currentValue.Equals(newValue))
                           && !string.IsNullOrEmpty(newValue?.ToString());

        if (isDifferent)
        {
            setter(newValue);
            incomingChanges[propertyName] = newValue;
            return (true, incomingChanges);
        }

        return (false, incomingChanges);
    }
}