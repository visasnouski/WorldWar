using Microsoft.AspNetCore.Identity;
using WorldWar.Repository.Models;

namespace WorldWar.Internal;

internal class UserEmailValidator<TUser> : IUserValidator<TUser>
	where TUser : WorldWarIdentity
{
	public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
	{
		var errors = new List<IdentityError>();

		var owner = await manager.FindByEmailAsync(user.Email).ConfigureAwait(true);
		if (owner != null &&
			!string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)))
		{
			errors.Add((new IdentityErrorDescriber()).DuplicateEmail(user.Email));
		}

		return errors.Any()
			? IdentityResult.Failed(errors.ToArray())
			: IdentityResult.Success;
	}
}