using AccountManagement.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Support;

/// <summary>Provides database operation methods for account management.</summary>
public class DbOperation
{
    /// <summary>Searches for accounts based on ID, email, or retrieves all accounts.</summary>
    /// <param name="db">The database context for account operations.</param>
    /// <param name="id">Optional account ID to search for a specific account.</param>
    /// <param name="email">Optional email address or fragment to search for accounts.</param>
    /// <returns>A list of Account objects matching the search criteria.</returns>
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

    /// <summary>Retrieves the highest account ID number from the database.</summary>
    /// <param name="db">The database context for account operations.</param>
    /// <returns>The maximum account ID value present in the database.</returns>
    public static  int GetLastIdNumber(AccountDb db)
    {
        return  db.Accounts
            .AsNoTracking()
            .Max(account => (account.Id));
    }


}