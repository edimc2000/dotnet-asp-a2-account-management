using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Support;

public class DbOperation
{
    public static async Task<List<Account>> Search(AccountDb db, int? id = null, string? email = null)
    {
        if (id != null)
        {
            return await db.Accounts
                .AsNoTracking()
                .Where(account => account.Id == id)
                .ToListAsync();
        }

        else if (email != null)
            return await db.Accounts
                .AsNoTracking()
                .Where(account => account.EmailAddress.Contains($"{email}"))
                .ToListAsync();

        else
            return await db.Accounts
                .AsNoTracking()
                .ToListAsync();
    }


    public static  int GetLastIdNumber(AccountDb db)
    {
        return  db.Accounts
            .AsNoTracking()
            .Max(account => (account.Id));
    }


}