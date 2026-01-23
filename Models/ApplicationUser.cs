// Legacy identity user representation — renamed to avoid collision with the
// canonical ApplicationUser in BordspelApp.Infrastructure.Identity.
// Do NOT use this class for Identity stores. Kept here temporarily to
// avoid breaking references in legacy code; consider removing when all
// references are migrated.
using Microsoft.AspNetCore.Identity;

namespace Bordspel_app.LegacyModels
{
	public class LegacyApplicationUser : IdentityUser
	{
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string Address { get; set; } = "";
		public DateTime created_at { get; set; }
	}
}
