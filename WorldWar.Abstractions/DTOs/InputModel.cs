﻿using System.ComponentModel.DataAnnotations;

namespace WorldWar.Abstractions.DTOs;

public class InputModel
{
	[Required]
	[Display(Name = "UserName")]
	public string UserName { get; set; } = null!;

	[Required]
	[EmailAddress]
	[Display(Name = "Email")]
	public string Email { get; set; } = null!;

	[Required]
	[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
	[DataType(DataType.Password)]
	[Display(Name = "Password")]
	public string Password { get; set; } = null!;

	[DataType(DataType.Password)]
	[Display(Name = "Confirm password")]
	[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
	public string ConfirmPassword { get; set; } = null!;

	public float Latitude { get; set; }

	public float Longitude { get; set; }
}