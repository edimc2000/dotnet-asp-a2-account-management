using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Get a user by ID
    /// </summary>
    /// <param name="id" example="123">The user ID</param>
    /// <returns>A user object</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetUser(int id)
    {
        return Ok(new UserResponse
        {
            Id = id,
            Name = "Sample User",
            Email = "sample@example.com",
            CreatedAt = DateTime.UtcNow
        });
    }
    
    /// <summary>
    /// Create a new user
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// ```
    /// POST /api/users
    /// {
    ///     "name": "John Doe",
    ///     "email": "john@example.com",
    ///     "role": "Admin"
    /// }
    /// ```
    /// </remarks>
    [HttpPost]
    public IActionResult CreateUser([FromBody] UserRequest request)
    {
        return CreatedAtAction(nameof(GetUser), new { id = 1 }, 
            new UserResponse 
            { 
                Id = 1, 
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow 
            });
    }
}