using System.ComponentModel.DataAnnotations;

public class UserRequest
{
    /// <summary>
    /// Full name of the user
    /// </summary>
    /// <example>John Doe</example>
    [Required]
    public string Name { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>
    /// <example>john.doe@company.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    /// <summary>
    /// User's role
    /// </summary>
    /// <example>Admin</example>
    public string Role { get; set; } = "User";
}

public class UserResponse
{
    /// <example>1</example>
    public int Id { get; set; }
    
    /// <example>John Doe</example>
    public string Name { get; set; }
    
    /// <example>john.doe@company.com</example>
    public string Email { get; set; }
    
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }
}