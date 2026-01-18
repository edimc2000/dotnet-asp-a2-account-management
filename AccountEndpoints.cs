using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AccountManagement.EntityModels;
using static AccountManagement.Support.DbOperation;
using static AccountManagement.Support.Helper;


namespace AccountManagement;

/// <summary>Contains endpoint methods for account management operations.</summary>
public class AccountEndpoints
{
    /// <summary>Searches for and returns all accounts in the database.</summary>
    /// <param name="db">The database context for account operations.</param>
    /// <returns>An IResult containing the search results.</returns>
    public static async Task<IResult> SearchAll(AccountDb db)
    {
        List<Account> result = await Search(db);
        return SearchSuccess(result);
    }

    /// <summary>Searches for an account by its ID.</summary>
    /// <param name="id">The account ID to search for.</param>
    /// <param name="db">The database context for account operations.</param>
    /// <returns>A tuple containing the IResult response and the count of matching accounts.</returns>
    public static async Task<(IResult result, int counter)> SearchById(string id, AccountDb db)
    {
        if (!int.TryParse(id, out int parsedId))
            return (BadRequest($"'{id}' is not a valid account Id"), 0);

        List<Account> result = await Search(db, parsedId);
        return (SearchSuccess(result), result.Count);
    }

    /// <summary>Searches for accounts by email address or fragment.</summary>
    /// <param name="email">The email address or fragment to search for.</param>
    /// <param name="db">The database context for account operations.</param>
    /// <returns>A tuple containing the IResult response and the count of matching accounts.</returns>
    public static async Task<(IResult result, int counter)> SearchByEmail
        (string email, AccountDb db)
    {
        List<Account> result = await Search(db, null, email);
        return (SearchSuccess(result), result.Count);
    }


    /// <summary>Deletes an account by its ID.</summary>
    /// <param name="id">The account ID to delete.</param>
    /// <param name="db">The database context for account operations.</param>
    /// <returns>An IResult containing the deletion response.</returns>
    public static async Task<IResult> DeleteById
        (string id, AccountDb db)
    {
        if (!int.TryParse(id, out int parsedId))
            return BadRequest($"'{id}' is not a valid account Id");

        Account? account = await db.Accounts.FindAsync(parsedId);

        if (account == null)
            return BadRequest($"'{id}' is not a valid account Id");

        if (restrictedIds.Contains(parsedId))
            return BadRequest($"Account ID '{id}' is restricted and cannot be deleted");

        db.Accounts.Remove(account);
        await db.SaveChangesAsync();

        return DeleteSuccess();
    }


    /// <summary>Adds a new account to the database.</summary>
    /// <param name="context">The HTTP context containing the request data.</param>
    /// <param name="db">The database context for account operations.</param>
    /// <returns>An IResult containing the account creation response.</returns>
    public static async Task<IResult> AddAccount(HttpContext context, AccountDb db)
    {
        (InputDataConverter? dataConverter, IResult? error) =
            await TryReadJsonBodyAsync<InputDataConverter>(context.Request);
        if (error != null) return error;

        int newIdNumber = GetLastIdNumber(db) + 1;
        string firstName = dataConverter.FirstName.ToString();
        string lastName = dataConverter.LastName.ToString();
        string emailAddress = dataConverter.EmailAddress.ToString();

        Account newAccount = new()
        {
            Id = newIdNumber,
            FirstName = firstName,
            LastName = lastName,
            EmailAddress = emailAddress
        };

        ValidationContext validationContext = new(newAccount);
        List<ValidationResult> validationResults = new();
        bool isValid = Validator.TryValidateObject(newAccount,
            validationContext,
            validationResults,
            true);

        if (!isValid)
        {
            string errors = string.Join("; ",
                validationResults.Select(r => r.ErrorMessage));
            return BadRequest($"Validation failed: {errors}");
        }


        int emailCount = SearchByEmail(emailAddress, db).Result.counter;

        if (emailCount != 0)
            return ConflictResult();

        db.Add(newAccount);
        await db.SaveChangesAsync();
        return AddSuccess(newAccount, newAccount.Id);
    }


    /// <summary>Updates an existing account with new data.</summary>
    /// <param name="context">The HTTP context containing the request data.</param>
    /// <param name="db">The database context for account operations.</param>
    /// <param name="id">The account ID to update.</param>
    /// <returns>An IResult containing the update response.</returns>
    public static async Task<IResult> UpdateAccount(HttpContext context, AccountDb db, string id)
    {
        (InputDataConverter? dataConverter, IResult? error) =
            await TryReadJsonBodyAsync<InputDataConverter>(context.Request);


        if (error != null)
            return error;

        if (!int.TryParse(id, out int parsedId))
            return BadRequest($"'{id}' is not a valid account Id");


        if (restrictedIds.Contains(parsedId))
            return BadRequest($"Account ID '{id}' is restricted and cannot be updated");

        bool hasChanges = false;

        string firstName = dataConverter.FirstName.ToString();
        string lastName = dataConverter.LastName.ToString();
        string emailAddress = dataConverter.EmailAddress.ToString();

        Account? account = await db.Accounts.FindAsync(parsedId);

        if (account == null)
            return BadRequest($"'{id}' is not a valid account Id");


        Dictionary<string, object?> changes = new();

        // Check if email is being changed to an existing email
        if (emailAddress != account.EmailAddress)
        {
            bool emailExists = await db.Accounts
                .AnyAsync(a => a.EmailAddress == emailAddress);

            if (emailExists)
                return ConflictResult();
        }


        // FirstName: update value only when there is a change,
        // null and empty will be ignored to preserve data 
        (bool nameChanged, changes) = TryUpdatePropertyIfChanged(hasChanges,
            account.FirstName,
            firstName,
            value => account.FirstName = value,
            changes,
            "FirstName");

        // LastName: update value only when there is a change,
        // null and empty will be ignored to preserve data 
        (bool lastNameChanged, changes) = TryUpdatePropertyIfChanged(hasChanges,
            account.LastName,
            lastName,
            value => account.LastName = value,
            changes,
            "LastName");

        // EmailAddress: update value only when there is a change,
        // null and empty will be ignored to preserve data 
        (bool emailChanged, changes) = TryUpdatePropertyIfChanged(hasChanges,
            account.EmailAddress,
            emailAddress,
            value => account.EmailAddress = value,
            changes,
            "EmailAddress");

        hasChanges = nameChanged || lastNameChanged || emailChanged;

        account.UpdatedAt = DateTime.UtcNow;
        if (hasChanges) await db.SaveChangesAsync();

        return UpdateSuccess(changes);
    }
}