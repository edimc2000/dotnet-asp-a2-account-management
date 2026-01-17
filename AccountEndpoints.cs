using System.ComponentModel.DataAnnotations;
using static AccountManagement.DbOperation;
using static AccountManagement.Helper;

namespace AccountManagement;

public class AccountEndpoints
{
    public static IResult SearchAll(AccountDb db)
    {
        List<Account> result = Search(db);
        return SearchSuccess(result);
    }

    public static (IResult result, int counter) SearchById(string id, AccountDb db)
    {
        if (!int.TryParse(id, out int parsedId))
            return (BadRequest($"'{id}' is not a valid account Id"), 0);
        List<Account> result = Search(db, parsedId);

        return (SearchSuccess(result), result.Count);
    }


    public static (IResult result, int counter) SearchByEmail(string email, AccountDb db)
    {
        List<Account> result = Search(db, null, email);
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


        int emailCount = SearchByEmail(emailAddress, db).counter;
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
}