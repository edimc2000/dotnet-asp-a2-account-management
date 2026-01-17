using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static AccountManagement.DbOperation;
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


    public static async Task<IResult> AddAccount(HttpContext context, AccountDb db)
    {
        (InputDataConverter? dataConverter, IResult? error) =
            await TryReadJsonBodyAsync<InputDataConverter>(context.Request);
        if (error != null) return error;

        int newIdNumber = GetLastIdNumber(db) + 1;
        string firstName = dataConverter.FirstName.ToString();
        string lastName = dataConverter.LastName.ToString();
        string emailAddress = dataConverter.EmailAddress.ToString();

        WriteLine(
            $"-- data received (INPUT ---\n --- with valid json format ---\n" +
            $"   >>>> newID: {newIdNumber}" +
            $"FirstName: {dataConverter.FirstName}");
        WriteLine($"   >>>> LastName: {dataConverter.LastName}");
        WriteLine($"   >>>>* EmailAddress: {dataConverter.EmailAddress}");

        Account newAccount = new()
        {
            Id = newIdNumber,
            FirstName = firstName,
            LastName = lastName,
            EmailAddress = emailAddress
            //CreatedAt =  DateTime.UtcNow
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
        WriteLine(
            $"--- CHECKING FOR EXISTING RECORD - email record count {emailCount}---\n");

        if (emailCount != 0)
            return ConflictResult(
                $"The email address is either tied to an account or cannot be used for registration");

        WriteLine("    --- add account module --- ");

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

        bool hasChanges = false;
        // int queryId = parsedId;
        string firstName = dataConverter.FirstName.ToString();
        string lastName = dataConverter.LastName.ToString() ?? null;
        string emailAddress = dataConverter.EmailAddress.ToString();


        //UpdateAccount? updateAccountIncoming = new()
        //{
        //    UpdatedAt=DateTime.UtcNow 

        //};


        Account? account = await db.Accounts.FindAsync(parsedId);
        //Account? account = await db.Accounts
        //    .AsNoTracking()
        //    .FirstOrDefaultAsync(a => a.Id == parsedId);


        WriteLine($"Account Details:");
        WriteLine($"  ID: {account.Id}");
        WriteLine($"  First Name: {account.FirstName}");
        WriteLine($"  Last Name: {account.LastName}");
        WriteLine($"  Email: {account.EmailAddress}");
        WriteLine($"  Created At: {account.CreatedAt}");
        WriteLine($"  Updated At: {account.UpdatedAt}");


        //UpdateAccountProperty(updateAccountIncoming, "LastName", lastName, account);

        if (account == null)
            return BadRequest($"'{id}' is not a valid account Id");

        // Check if email is being changed to an existing email
        if (emailAddress != account.EmailAddress)
        {
            bool emailExists = await db.Accounts
                .AnyAsync(a => a.EmailAddress == emailAddress);

            if (emailExists)
                return ConflictResult($"The email address is either tied to an account or cannot be used for registration");
        }

        //account.FirstName = firstName;

        // Update only if value is provided AND different from current
        if (!string.IsNullOrEmpty(firstName) && account.FirstName != firstName)
        {
            account.FirstName = firstName;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(lastName) && account.LastName != lastName)
        {
            account.LastName = lastName;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(emailAddress) && account.EmailAddress != emailAddress)
        {
            //var emailExists = await db.Accounts
            //    .AnyAsync(a => a.EmailAddress == emailAddress && a.Id != parsedId);
        
            //if (emailExists)
            //    return BadRequest("Email address already exists");
        
            account.EmailAddress = emailAddress;
            hasChanges = true;
        }

        //account.LastName = lastName;
        //account.EmailAddress = emailAddress;
        account.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        //return true;

        return Results.Ok();
    }
}