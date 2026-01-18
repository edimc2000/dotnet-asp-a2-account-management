using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static AccountManagement.Support.DbOperation;
using static AccountManagement.Helper;

namespace AccountManagement;

public class AccountEndpoints
{
    public static async Task<IResult> SearchAll(AccountDb db)
    {
        List<Account> result = await Search(db);
        return SearchSuccess(result);
    }

    public static async Task<(IResult result, int counter)> SearchById(string id, AccountDb db)
    {
        if (!int.TryParse(id, out int parsedId))
            return (BadRequest($"'{id}' is not a valid account Id"), 0);

        List<Account> result = await Search(db, parsedId);
        return (SearchSuccess(result), result.Count);
    }

    public static async Task<(IResult result, int counter)> SearchByEmail
        (string email, AccountDb db)
    {
        List<Account> result = await Search(db, null, email);
        return (SearchSuccess(result), result.Count);
    }


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