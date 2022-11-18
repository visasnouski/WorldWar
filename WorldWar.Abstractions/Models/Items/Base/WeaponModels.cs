using WorldWar.Abstractions.Models.Items.Base.Weapons;

namespace WorldWar.Abstractions.Models.Items.Base;

public static class WeaponModels
{
	public static readonly Weapon Fist = new()
	{
		Id = 1000,
		Name = "Fist",
		ItemType = ItemTypes.Weapon,
		WeaponType = WeaponTypes.Handguns,
		Distance = 00.00002F,
		Accuracy = 100,
		Damage = 10,
		Ammo = 1,
		MagazineSize = 1,
		IconPath = Path.Combine("weapons", "Fist", "Fist.png"),
		DelayShot = TimeSpan.FromMilliseconds(900),
		ReloadTime = TimeSpan.FromMilliseconds(1500),
		ShotSoundLocation = "/weapons/Fist/Kick.wav",
		ReloadSoundLocation = "/weapons/Fist/Kick2.wav"
	};

	// ReSharper disable once InconsistentNaming
	public static readonly Weapon TT = new()
	{
		Id = 1001,
		Name = "TT",
		ItemType = ItemTypes.Weapon,
		WeaponType = WeaponTypes.Pistols,
		Distance = 00.00076F, // 50m
		Accuracy = 20,
		Damage = 100,
		MagazineSize = 8,
		IconPath = "weapons\\TT\\TT.png",
		DelayShot = TimeSpan.FromMilliseconds(800),
		ReloadTime = TimeSpan.FromMilliseconds(2800),
		ShotSoundLocation = "/weapons/TT/Shoot.wav",
		ReloadSoundLocation = "/weapons/TT/Reload.wav",
	};

	public static readonly Weapon DesertEagle = new()
	{
		Id = 1002,
		Name = "Desert Eagle",
		ItemType = ItemTypes.Weapon,
		WeaponType = WeaponTypes.Pistols,
		Distance = 0.00152F, // 100m
		Accuracy = 20,
		Damage = 200,
		MagazineSize = 7,
		IconPath = "weapons\\DesertEagle\\DesertEagle.png",
		DelayShot = TimeSpan.FromMilliseconds(1200),
		ReloadTime = TimeSpan.FromMilliseconds(2800),
		ShotSoundLocation = "/weapons/DesertEagle/Shoot.wav",
		ReloadSoundLocation = "/weapons/DesertEagle/Reload.wav",
	};

	public static readonly Weapon Ak47 = new()
	{
		Id = 1003,
		Name = "AK-47",
		ItemType = ItemTypes.Weapon,
		WeaponType = WeaponTypes.Rifles,
		Distance = 0.00460F, // 300m
		Accuracy = 20,
		Damage = 100,
		MagazineSize = 30,
		IconPath = "weapons\\AK47\\AK47.png",
		DelayShot = TimeSpan.FromMilliseconds(600),
		ReloadTime = TimeSpan.FromMilliseconds(5000),
		ShotSoundLocation = "/weapons/AK47/Shoot.wav",
		ReloadSoundLocation = "/weapons/AK47/Reload.wav",
	};
}