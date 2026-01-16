using System.Runtime.InteropServices.Marshalling;
using Microsoft.EntityFrameworkCore;
using static AccountManagement.Helper;

namespace AccountManagement;

public class DbOperation
{
    public static List<Account> Search(AccountDb db, int? id = null, string? email = null)
    {
        if (id != null)
        {
            return db.Accounts
                .AsNoTracking()
                .Where(account => account.Id == id)
                .ToList();
        }

        else if (email != null)
            return db.Accounts
                .AsNoTracking()
                .Where(account => account.EmailAddress.Contains($"{email}"))
                .ToList();

        else
            return db.Accounts
                .AsNoTracking()
                .ToList();
    }


    public static int GetLastIdNumber(AccountDb db)
    {
        return db.Accounts
            .AsNoTracking()
            .Max(account => (account.Id));
    }
}