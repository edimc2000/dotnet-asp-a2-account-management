using System.Runtime.InteropServices.Marshalling;
using Microsoft.EntityFrameworkCore;
using static AccountManagement.Helper;

namespace AccountManagement;


public class DbOperation
{

//using AccountDb db = new();
//DbSet<Account> accounts = db.Accounts;




    //public static IResult Search( DbSet<Account> allAccounts)
    //{
    //    //using AccountDb db = new();
    //    //DbSet<Account> allAccounts = db.Accounts;
    //    List<Account> accountList = allAccounts.ToList();
    //    WriteLine("All Accounts:");
    //    WriteLine("------search all module -------");

    //    foreach (Account item in accountList)
    //    {
    //        WriteLine($"ID: {item.Id}");
    //        WriteLine($"Name: {item.FirstName} {item.LastName}");
    //        WriteLine($"Email: {item.EmailAddress}");
    //        WriteLine($"Created: {item.CreatedAt}");
    //        WriteLine($"Updated: {item.UpdatedAt}");
    //        WriteLine();
    //    }
    //    return SearchSuccess(accountList);
    //}

    
    public static List<Account> Search(AccountDb db, int? id = null, string? email = null)
    {


        if (id != null)
            return db.Accounts
                .AsNoTracking()
                .Where(account => account.Id == id)
                .ToList();
       
        else if (email != null)
        {
            return db.Accounts
                .AsNoTracking()
                
                .Where(account => account.EmailAddress.Contains($"{email}"))
                .ToList();
        }

        else
            return db.Accounts
                .AsNoTracking()
                .ToList();

    }
}


    