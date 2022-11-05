using System.ComponentModel.DataAnnotations;

namespace WorldWar.Abstractions.DTOs;

public class Mob
{
	[Key]
	[StringLength(50)]
	public string MobGuid { get; init; } = null!;

	public float Latitude { get; init; }

	public float Longitude { get; init; }

	public string MobType { get; init; } = null!;
}