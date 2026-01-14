using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement;

[Table("account")]
[Index("EmailAddress", IsUnique = true)]
public partial class Account
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    public string LastName { get; set; } = null!;

    [Column("email_address")]
    public string EmailAddress { get; set; } = null!;

    [Column("created_at", TypeName = "DATETIME")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "DATETIME")]
    public DateTime? UpdatedAt { get; set; }
}
