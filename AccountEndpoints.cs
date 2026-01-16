using static AccountManagement.Helper;
using static AccountManagement.DbOperation;

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


}