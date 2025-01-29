using Microsoft.AspNetCore.Identity;

namespace Bordspel_app.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string Address { get; set; } = "";
		public DateTime created_at { get; set; }

	}
}
