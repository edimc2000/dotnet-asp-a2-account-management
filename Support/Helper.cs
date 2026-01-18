using System.Text.Json;

namespace AccountManagement;

/// <summary>Helper methods for product API operations.</summary>
/// <para>Author: Eddie C.</para>
/// <para>Version: 1.0</para>
/// <para>Date: Jan. 17, 2026</para>
internal partial class Helper
{
    /// <summary>Array of restricted account IDs that cannot be modified or deleted.</summary>
    internal static int[] restrictedIds = { 200, 201, 202, 203 };

    /// <summary>Converts JSON elements to account input data properties.</summary>
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

    /// <summary>Returns successful account creation response.</summary>
    /// <param name="newAccount">The newly created Account object.</param>
    /// <param name="newIdNumber">The ID number assigned to the new account.</param>
    /// <returns>An IResult containing the success response with created account data.</returns>
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

    /// <summary>Returns successful account deletion response.</summary>
    /// <returns>An IResult containing the success response for account deletion.</returns>
    public static IResult DeleteSuccess()
    {
        return Results.Ok(
            new
            {
                success = true,
                message = "Account deleted successfully"
            });
    }

    /// <summary>Returns successful account update response.</summary>
    /// <param name="changes">Dictionary containing the field names and values that were changed.</param>
    /// <returns>An IResult containing the success response with update details.</returns>
    public static IResult UpdateSuccess(Dictionary<string, object?> changes)
    {
        if (changes.Count == 0)
            return Results.Ok(new
            {
                success = true,
                message = "Request processed successfully. No modifications made. " +
                          "Null fields and empty values were ignored to preserve existing data."
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
    /// <returns>An IResult containing the bad request error response.</returns>
    public static IResult BadRequest(string message)
    {
        return Results.BadRequest(new
        {
            success = false,
            message = message
        });
    }

    /// <summary>Returns formatted unprocessable entity response.</summary>
    /// <param name="message">Error message explaining why the entity is unprocessable.</param>
    /// <returns>An IResult containing the unprocessable entity error response.</returns>
    public static IResult UnprocessableEntity(string message)
    {
        return Results.UnprocessableEntity(new
        {
            success = false,
            message = $"{message}"
        });
    }


    /// <summary>Returns formatted conflict response for duplicate email addresses.</summary>
    /// <returns>An IResult containing the conflict error response.</returns>
    public static IResult ConflictResult()
    {
        return Results.Conflict(new
        {
            success = false,
            message = $"The email address is either tied to an account or cannot be used " +
                      $"for registration"
        });
    }

    /// <summary>Attempts to update a property if the new value differs from the current value.</summary>
    /// <typeparam name="T">The type of the property being compared.</typeparam>
    /// <param name="hasChanges">Boolean indicating whether changes have already been detected.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="newValue">The new value to potentially assign to the property.</param>
    /// <param name="setter">Action that sets the property value if changed.</param>
    /// <param name="incomingChanges">Dictionary tracking changes made during update.</param>
    /// <param name="propertyName">The name of the property being evaluated.</param>
    /// <returns>A tuple containing boolean indicating if changes were made and the updated changes dictionary.</returns
    // This function was created with AI - Deepseek assistance 
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