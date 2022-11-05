using WorldWar.Abstractions.DTOs;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Items.Base.Protections;
using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Extensions
{
	public static class ModelExtensions
	{
		public static Unit ToUnit(this UnitDto unitDto, Func<ICollection<int>, ICollection<ItemDto>> funcItem)
		{
			if (unitDto == null) throw new ArgumentNullException(nameof(unitDto));
			if (funcItem == null) throw new ArgumentNullException(nameof(funcItem));

			var items = funcItem(unitDto.Loot.ItemIds);

			return unitDto.UnitType switch
			{
				UnitTypes.Car => new Car(unitDto.Id, unitDto.Name, unitDto.Latitude, unitDto.Longitude, 100,
					 unitDto.Weapon.ToWeapon(), unitDto.HeadProtection.ToHeadProtection(),
					unitDto.BodyProtection.ToBodyProtection(), unitDto.Loot.ToLoot(items)),

				UnitTypes.Player => new Player(unitDto.Id, unitDto.Name, unitDto.Latitude, unitDto.Longitude, 100,
					 unitDto.Weapon.ToWeapon(), unitDto.HeadProtection.ToHeadProtection(),
					unitDto.BodyProtection.ToBodyProtection(), unitDto.Loot.ToLoot(items)),

				UnitTypes.Mob => new Bot(unitDto.Id, unitDto.Name, unitDto.UnitType, unitDto.Latitude,
					unitDto.Longitude, 100, unitDto.Weapon.ToWeapon(),
					unitDto.HeadProtection.ToHeadProtection(), unitDto.BodyProtection.ToBodyProtection(),
					unitDto.Loot.ToLoot(items)),

				_ => throw new InvalidOperationException("Unknown unit type")
			};
		}

		public static UnitDto ToUnitDto(this Unit unit)
		{
			return new UnitDto()
			{
				Id = unit.Id,
				Name = unit.Name,
				UnitType = unit.UnitType,
				Longitude = unit.CurrentLongitude,
				Latitude = unit.CurrentLatitude,
				WeaponId = unit.Weapon.Id,
				Weapon = unit.Weapon.ToWeaponDto(),
				BodyProtectionId = unit.BodyProtection.Id,
				BodyProtection = unit.BodyProtection.ToBodyProtectionDto(),
				HeadProtectionId = unit.HeadProtection.Id,
				HeadProtection = unit.HeadProtection.ToHeadProtectionDto(),
				LootId = unit.Id.GetHashCode(),
				Loot = unit.Loot.ToLootDto(),
			};
		}

		public static HeadProtection ToHeadProtection(this HeadProtectionDto protectionDto)
		{
			if (protectionDto == null)
			{
				throw new ArgumentNullException(nameof(protectionDto));
			}

			return new HeadProtection()
			{
				Id = protectionDto.Id,
				Defense = protectionDto.Defense,
				ItemType = protectionDto.ItemType,
				Name = protectionDto.Name,
				IconPath = protectionDto.IconPath,
			};
		}

		public static BodyProtection ToBodyProtection(this BodyProtectionDto protectionDto)
		{
			if (protectionDto == null)
			{
				throw new ArgumentNullException(nameof(protectionDto));
			}

			// TODO use ItemType
			return new BodyProtection()
			{
				Id = protectionDto.Id,
				Defense = protectionDto.Defense,
				ItemType = protectionDto.ItemType,
				Name = protectionDto.Name,
				IconPath = protectionDto.IconPath,
			};
		}

		public static HeadProtectionDto ToHeadProtectionDto(this Protection protection)
		{
			if (protection == null)
			{
				throw new ArgumentNullException(nameof(protection));
			}

			return new HeadProtectionDto()
			{
				Id = protection.Id,
				Defense = protection.Defense,
				ItemType = protection.ItemType,
				Name = protection.Name,
				IconPath = protection.IconPath
			};
		}

		public static BodyProtectionDto ToBodyProtectionDto(this Protection protection)
		{
			if (protection == null)
			{
				throw new ArgumentNullException(nameof(protection));
			}

			return new BodyProtectionDto()
			{
				Id = protection.Id,
				Defense = protection.Defense,
				ItemType = protection.ItemType,
				Name = protection.Name,
				IconPath = protection.IconPath,
			};
		}

		public static Weapon ToWeapon(this WeaponDto weaponDto)
		{
			if (weaponDto == null)
			{
				throw new ArgumentNullException(nameof(weaponDto));
			}

			return new Weapon()
			{
				Id = weaponDto.Id,
				Name = weaponDto.Name,
				ItemType = weaponDto.ItemType,
				WeaponType = weaponDto.WeaponType,
				Distance = weaponDto.Distance,
				Damage = weaponDto.Damage,
				MagazineSize = weaponDto.MagazineSize,
				Accuracy = weaponDto.Accuracy,
				DelayShot = weaponDto.DelayShot,
				ReloadTime = weaponDto.ReloadTime,
				ReloadSoundLocation = weaponDto.ReloadSoundLocation,
				ShotSoundLocation = weaponDto.ShotSoundLocation,
				IconPath = weaponDto.IconPath,
			};
		}

		public static WeaponDto ToWeaponDto(this Weapon weapon)
		{
			if (weapon == null)
			{
				throw new ArgumentNullException(nameof(weapon));
			}

			return new WeaponDto()
			{
				Id = weapon.Id,
				Name = weapon.Name,
				ItemType = weapon.ItemType,
				WeaponType = weapon.WeaponType,
				Distance = weapon.Distance,
				Damage = weapon.Damage,
				MagazineSize = weapon.MagazineSize,
				Accuracy = weapon.Accuracy,
				DelayShot = weapon.DelayShot,
				ReloadTime = weapon.ReloadTime,
				ReloadSoundLocation = weapon.ReloadSoundLocation,
				ShotSoundLocation = weapon.ShotSoundLocation,
				IconPath = weapon.IconPath,
			};
		}

		public static Loot ToLoot(this LootDto lootDto, ICollection<ItemDto> items)
		{
			return new Loot() { Id = lootDto.Id, Items = items.ToItems().ToList() };
		}

		public static LootDto ToLootDto(this Loot loot)
		{
			return new LootDto() { Id = loot.Id, ItemIds = loot.Items.ToItemDtos() };
		}

		public static IEnumerable<Item> ToItems(this IEnumerable<ItemDto> itemDtos)
		{
			return itemDtos.Select(ToItem);
		}

		public static Item ToItem(this ItemDto itemDto)
		{
			if (itemDto == null)
			{
				throw new ArgumentNullException(nameof(itemDto));
			}

			return itemDto.ItemType switch
			{
				ItemTypes.Weapon => ((WeaponDto)itemDto).ToWeapon(),
				ItemTypes.BodyProtection => ((BodyProtectionDto)itemDto).ToBodyProtection(),
				ItemTypes.HeadProtection => ((HeadProtectionDto)itemDto).ToHeadProtection(),
				_ => throw new ArgumentOutOfRangeException(nameof(itemDto.ItemType), "Unknown type")
			};
		}

		public static ICollection<int> ToItemDtos(this IEnumerable<Item> loots)
		{
			return loots.Select(ToItemDto).ToList();
		}

		public static int ToItemDto(this Item item)
		{
			return item.Id;
		}
	}
}
