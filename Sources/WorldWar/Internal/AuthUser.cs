using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Authentication;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Repository.Models;

namespace WorldWar.Internal;

internal class AuthUser : IAuthUser
{
	private readonly AuthenticationStateProvider _authenticationStateProvider;
	private readonly UserManager<WorldWarIdentity> _userManager;

	private WorldWarIdentity? _user;

	public AuthUser(AuthenticationStateProvider authenticationStateProvider, UserManager<WorldWarIdentity> userManager)
	{
		_authenticationStateProvider = authenticationStateProvider ??
									   throw new ArgumentNullException(nameof(authenticationStateProvider));
		_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
	}

	public async Task<IWorldWarIdentityUser> GetIdentity()
	{
		return _user ??= await GetUser();
	}

	private async Task<WorldWarIdentity> GetUser()
	{
		var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
		var user = await _userManager.GetUserAsync(authState.User);
		return user ?? throw new AuthenticationException("Unknown identity");
	}
}