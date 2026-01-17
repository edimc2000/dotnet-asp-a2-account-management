using System.ComponentModel.DataAnnotations;

namespace AccountManagement;

/// <summary>
/// Interface for account input data
/// </summary>
public interface IJsonAccountInput
{
    /// <summary>
    /// Account holder's first name
    /// </summary>
    /// <example>John</example>
    string FirstName { get; set; }

    /// <summary>
    /// Account holder's last name
    /// </summary>
    /// <example>Doe</example>
    string LastName { get; set; }

    /// <summary>
    /// Account holder's email address
    /// </summary>
    /// <example>john.doe@example.com</example>
    string EmailAddress { get; set; }
}

/// <summary>
/// Implementation of account input data
/// </summary>
public class AccountData : IJsonAccountInput
{
    /// <summary>
    /// Unique identifier of the account
    /// </summary>
    /// <example>100</example>
    public int Id { get; set; }

    /// <summary>
    /// Account holder's first name
    /// </summary>
    /// <example>John</example>
    public required string FirstName { get; set; }

    /// <summary>
    /// Account holder's last name
    /// </summary>
    /// <example>Doe</example>
    public required string LastName { get; set; }

    /// <summary>
    /// Account holder's email address
    /// </summary>
    /// <example>john.doe@example.com</example>
    public required string EmailAddress { get; set; }



}

public class UpdateAccount : IJsonAccountInput
{

    /// <summary>
    /// Account holder's first name
    /// </summary>
    /// <example>John</example>
    [StringLength(100, MinimumLength = 2)]
    public  string FirstName { get; set; }

    /// <summary>
    /// Account holder's last name
    /// </summary>
    /// <example>Doe</example>
    [StringLength(100, MinimumLength = 2)]
    public  string LastName { get; set; }

    /// <summary>
    /// Account holder's email address
    /// </summary>
    /// <example>john.doe@example.com</example>
    [EmailAddress]
    public  string EmailAddress { get; set; }
    
    //[Required]
    [DataType(DataType.DateTime)]
    public DateTime? UpdatedAt { get; set; } 



}