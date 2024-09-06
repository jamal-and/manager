using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class User : IdentityUser
{
    public double Credit { get; set; }
    public List<Contact> Contacts { get; set; }

}

public class Contact
{
    [Key]
    public int ContactId { get; set; }
    public string Nickname { get; set; }
    public string PhoneNumber { get; set; }
    public string? UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

}
