﻿using System.Numerics;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Models.Units;

public abstract class Unit : IFightable, ICarriable, IMovable
{
	public string Name { get; init; }

	public float ViewingDistance => 00.0015F;

	public Guid Id { get; init; }

	public UnitTypes UnitType { get; private set; }

	public int Health { get; set; }

	public Weapon Weapon { get; set; }

	public HeadProtection HeadProtection { get; set; }

	public BodyProtection BodyProtection { get; set; }

	public double Rotate { get; private set; }

	public Loot Loot { get; init; }

	public float Speed
	{
		get
		{
			return UnitType switch
			{
				UnitTypes.Car => 00.00009F,
				UnitTypes.Player or UnitTypes.Mob => 00.00001F,
				_ => throw new InvalidOperationException("Unknown unit type")
			};
		}
	}

	public Location Location { get; init; }

	public float CurrentLatitude => Location.CurrentPos.Y;

	public float CurrentLongitude => Location.CurrentPos.X;

	public Unit(Guid id,
		string name,
		UnitTypes unitType,
		float latitude,
		float longitude,
		int health,
		Weapon? weapon = null,
		HeadProtection? headProtection = null,
		BodyProtection? bodyProtection = null,
		Loot? loot = null)
	{
		this.Id = id;
		this.Name = name;
		this.UnitType = unitType;
		this.Location = new Location(longitude, latitude);
		this.Health = health;
		//TODO implement values by default
		this.Weapon = weapon ?? WeaponModels.Fist;
		this.HeadProtection = headProtection ?? HeadProtectionModels.Bandana;
		this.BodyProtection = bodyProtection ?? BodyProtectionModels.WifeBeater;
		this.Loot = loot ?? new Loot() { Id = id.GetHashCode(), Items = new List<Item>() };
	}

	public string MobTypesString => UnitType.ToString();

	public void AddDamage(int damage)
	{
		var protection = HeadProtection.Defense + BodyProtection.Defense;
		var realDamage = damage - damage * protection / 100;
		Health -= realDamage;
	}

	public void SetWeapon(Weapon weapon)
	{
		this.Loot.Items.Add(Weapon);
		Weapon = weapon;
	}

	public void SetBodyProtection(BodyProtection bodyProtection)
	{
		this.Loot.Items.Add(BodyProtection);
		BodyProtection = bodyProtection;
	}

	public void SetHeadProtection(HeadProtection headProtection)
	{
		this.Loot.Items.Add(HeadProtection);
		HeadProtection = headProtection;
	}

	public void Move(TimeSpan deltaTime, float endLongitude, float endLatitude, int acceleration = 1)
	{
		var endPos = new Vector2(endLongitude, endLatitude);
		var movVec = Vector2.Subtract(endPos, Location.StartPos);
		var normMovVec = Vector2.Normalize(movVec);

		if (normMovVec.X is Single.NaN || normMovVec.Y is Single.NaN)
		{
			return;
		}

		var deltaVec = normMovVec * Convert.ToInt64(deltaTime.TotalSeconds) * Speed * acceleration;
		Location.ChangeLocation(Vector2.Add(Location.StartPos, deltaVec));
	}

	public void RotateUnit(float endLongitude, float endLatitude, float? fromLongitude = null, float? fromLatitude = null)
	{
		fromLongitude ??= Location.StartPos.X;
		fromLatitude ??= Location.StartPos.Y;
		Rotate = -90 + BearingCalculator.Calculate(fromLatitude.Value, fromLongitude.Value, endLatitude, endLongitude);
	}

	public void SaveCurrentLocation()
	{
		Location.SaveCurrentLocation();
	}

	public bool IsWithinReach(float endLongitude, float endLatitude, float? distance = null)
	{
		distance ??= Speed * 10;
		var currentDistance = Vector2.Distance(Location.CurrentPos, new Vector2(endLongitude, endLatitude));
		return currentDistance < distance;
	}

	public void ChangeUnitType(UnitTypes unitType)
	{
		UnitType = unitType;
	}
}