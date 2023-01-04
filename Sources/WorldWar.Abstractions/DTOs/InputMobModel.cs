using WorldWar.Abstractions.Models;

namespace WorldWar.Abstractions.DTOs;

public class InputMobModel
{
	public string MobGuid { get; set; } = Guid.NewGuid().ToString();

	public UnitTypes UnitType { get; set; }

	public string IconImageHref => $"img/{UnitType}.png";
}