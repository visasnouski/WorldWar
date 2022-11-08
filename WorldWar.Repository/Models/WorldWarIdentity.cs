using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using WorldWar.Abstractions;

namespace WorldWar.Repository.Models;

public class WorldWarIdentity : IdentityUser, IWorldWarIdentityUser
{
	public Guid GuidId => Guid.Parse(Id);

	[Required]
	[DisplayName("Longitude")]
	public double Longitude { get; set; }

	[Required]
	[DisplayName("Latitude")]
	public double Latitude { get; set; }
}