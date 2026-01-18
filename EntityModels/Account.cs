using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.EntityModels;

// This is an auto generated data model using ef tool 


[Table("account")]
[Index("EmailAddress", IsUnique = true)]
public partial class Account
{

    /// <summary>
    /// Unique identifier of the account
    /// </summary>
    /// <example>10001</example>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Account holder's first name
    /// </summary>
    /// <example>John</example>
    [Column("first_name")]
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Account holder's last name
    /// </summary>
    /// <example>Doe</example>
    [Column("last_name")]
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Account holder's email address
    /// </summary>
    /// <example>john.doe@example.com</example>
    [Column("email_address")]
    [Required]
    [StringLength(100,MinimumLength = 4)]
    [EmailAddress]
    public string EmailAddress { get; set; } = null!;

    /// <summary>
    /// Account creation time
    /// </summary>
    /// <example>2026-01-02T00:00:00</example>
    [Column("created_at", TypeName = "DATETIME")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Account update time
    /// </summary>
    /// <example>2056-01-02T23:59:00</example>
    [Column("updated_at", TypeName = "DATETIME")]
    public DateTime? UpdatedAt { get; set; } 
}
