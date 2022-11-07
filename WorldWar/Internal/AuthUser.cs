using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Authentication;
using WorldWar.Abstractions;
using WorldWar.Repository.Models;

namespace WorldWar.Internal;

public class AuthUser : IAuthUser
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserManager<MyIdentityUser> _userManager;

    private MyIdentityUser? _user;

    public AuthUser(AuthenticationStateProvider authenticationStateProvider, UserManager<MyIdentityUser> userManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userManager = userManager;
    }

    public async Task<IWorldWarIdentityUser> GetIdentity()
    {
        return _user ??= await GetUser().ConfigureAwait(true);
    }

    private async Task<MyIdentityUser> GetUser()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(true);
        var user = await _userManager.GetUserAsync(authState.User).ConfigureAwait(true);
        return user ?? throw new AuthenticationException("bad identity");
    }
}