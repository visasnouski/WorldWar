using Microsoft.AspNetCore.Identity;

namespace WorldWar.Exceptions;

public class IdentityException : Exception
{
	private readonly IReadOnlyCollection<IdentityError> _identityErrors;

	public IdentityException(IReadOnlyCollection<IdentityError> identityErrors, string? message = null)
		: base(message)
	{
		_identityErrors = identityErrors;
	}

	public override string ToString() => string.Join(Environment.NewLine, _identityErrors.Select(x => x.Description));
}