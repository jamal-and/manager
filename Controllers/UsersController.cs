using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public readonly UserManager<User> _userManager;

    public UsersController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _context.Users
                                      .Include(u => u.Contacts)
                                      .ToListAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        var user = await _context.Users
        .Include(u => u.Contacts)
        .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }


        return user;
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        for (int i = 0; i < user.Contacts.Count; i++)
        {
            ModelState.Remove($"Contacts[{i}].UserId");
            ModelState.Remove($"Contacts[{i}].User");
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        foreach (var contact in user.Contacts)
        {
            contact.UserId = user.Id;
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }


    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(string id, User user)
    {
        var existingUser = await _context.Users.Include(u => u.Contacts).FirstOrDefaultAsync(u => u.Id == id);
        if (existingUser == null)
        {
            return NotFound();
        }

        existingUser.UserName = user.UserName;
        existingUser.Credit = user.Credit;

        _context.Contacts.RemoveRange(existingUser.Contacts);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return StatusCode(500, "A database error occurred.");
        }

        return NoContent();
    }



    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _context.Users.Include(u => u.Contacts).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Contacts.RemoveRange(user.Contacts);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(model.Username);

        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Ok(true);
        }

        return Ok(false);
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            return Ok("Password reset successful");
        }

        return BadRequest("Error resetting password");
    }
    [HttpPost("{userId}/add-contact")]
    public async Task<IActionResult> AddContact(string userId, [FromBody] Contact newContact)
    {
        var user = await _context.Users.Include(u => u.Contacts).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound();
        }

        newContact.UserId = userId;

        user.Contacts.Add(newContact);

        await _context.SaveChangesAsync();

        return Ok(newContact);
    }
    [HttpDelete("{userId}/delete-contact/{contactId}")]
    public async Task<IActionResult> DeleteContact(string userId, int contactId)
    {
        var user = await _context.Users.Include(u => u.Contacts).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound();
        }

        var contact = user.Contacts.FirstOrDefault(c => c.ContactId == contactId);
        if (contact == null)
        {
            return NotFound();
        }

        user.Contacts.Remove(contact);

        await _context.SaveChangesAsync();

        return NoContent();
    }



}
