using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WorldWar.Abstractions;

namespace WorldWar.Internal;

public class MyIdentityUser : IdentityUser, IWorldWarIdentityUser
{
	public Guid GuidId => Guid.Parse(Id);

	[Required]
	[DisplayName("Longitude")]
	public double Longitude { get; set; }

	[Required]
	[DisplayName("Latitude")]
	public double Latitude { get; set; }
}