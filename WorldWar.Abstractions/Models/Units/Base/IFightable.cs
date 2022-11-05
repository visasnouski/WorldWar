using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;

namespace WorldWar.Abstractions.Models.Units.Base;

public interface IFightable
{
	public int Health { get; set; }

	public Weapon Weapon { get; set; }

	public HeadProtection HeadProtection { get; set; }

	public BodyProtection BodyProtection { get; set; }

	public void AddDamage(int damage);

	public void SetWeapon(Weapon weapon);

	public void SetBodyProtection(BodyProtection bodyProtection);

	public void SetHeadProtection(HeadProtection headProtection);
}