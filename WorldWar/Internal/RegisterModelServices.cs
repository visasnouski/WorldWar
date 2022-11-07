using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using WorldWar.Abstractions.DTOs;
using WorldWar.Exceptions;
using WorldWar.Interfaces;
using WorldWar.Repository.Models;

namespace WorldWar.Internal;

public class RegisterModelServices : IRegisterModelServices
{
    private readonly SignInManager<WorldWarIdentity> _signInManager;
    private readonly UserManager<WorldWarIdentity> _userManager;
    private readonly IUserStore<WorldWarIdentity> _userStore;
    private readonly IUserEmailStore<WorldWarIdentity> _emailStore;
    private readonly ILogger<RegisterModelServices> _logger;
    private readonly IEmailSender _emailSender;

    public RegisterModelServices(
        UserManager<WorldWarIdentity> userManager,
        IUserStore<WorldWarIdentity> userStore,
        SignInManager<WorldWarIdentity> signInManager,
        ILogger<RegisterModelServices> logger,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    public async Task RegisterAsync(InputModel input, string baseUri)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (baseUri == null)
        {
            throw new ArgumentNullException(nameof(baseUri));
        }

        var user = CreateUser();
        user.Latitude = input.Latitude;
        user.Longitude = input.Longitude;

        await _userStore.SetUserNameAsync(user, input.UserName, CancellationToken.None).ConfigureAwait(true);
        await _emailStore.SetEmailAsync(user, input.Email, CancellationToken.None).ConfigureAwait(true);
        var result = await _userManager.CreateAsync(user, input.Password).ConfigureAwait(true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account with password.");

            var userId = await _userManager.GetUserIdAsync(user).ConfigureAwait(true);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var uriBuilder = new UriBuilder($"{baseUri}Identity/Account/ConfirmEmail");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["area"] = "Identity";
            query["userId"] = userId;
            query["code"] = code;
            query["returnUrl"] = "/Identity/Account/Login?returnUrl=/WorldMap";
            uriBuilder.Query = query.ToString()!;

            _logger.LogInformation("Confirm your email.{ConfirmUri}", uriBuilder.ToString());

            await _emailSender.SendEmailAsync(input.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(uriBuilder.ToString())}'>clicking here</a>.").ConfigureAwait(true);

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return;
            }

            await _signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(true);
            return;
        }

        var errors = result.Errors.ToList();
        if (errors.Any())
        {
            throw new IdentityException(errors);
        }
    }

    private static WorldWarIdentity CreateUser()
    {
        try
        {
            return Activator.CreateInstance<WorldWarIdentity>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(WorldWarIdentity)}'. " +
                                                $"Ensure that '{nameof(WorldWarIdentity)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<WorldWarIdentity> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<WorldWarIdentity>)_userStore;
    }
}