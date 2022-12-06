using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Core.Tests
{
	[TestClass]
	public class CombatServiceTests
	{
		[TestMethod]
		public async Task AttackUnit_IfUserGuidNotExist_Returns()
		{
			// Arrange

			var mocker = new AutoMocker();
			Guid.TryParse("3BB75C5D-B402-49D3-8D17-D245069CA13D", out var userGuid);
			Guid.TryParse("3BB75C5D-B402-49D3-8D17-D245069CA13D", out var enemyGuid);

			Unit? bot = null;
			var unitStorageMock = new Mock<IStorage<Unit>>();
			unitStorageMock.Setup(x => x.TryGetValue(userGuid, out bot)).Returns(false);

			var cacheFactoryMock = new Mock<ICacheFactory>();
			cacheFactoryMock.Setup(x => x.Create<Unit>()).Returns(unitStorageMock.Object);

			mocker.Use(cacheFactoryMock);

			var target = mocker.CreateInstance<CombatService>();

			// Act

			await target.AttackUnit(userGuid, enemyGuid, CancellationToken.None);

			// Assert

			unitStorageMock.Verify(x => x.TryGetValue(It.IsAny<Guid>(), out It.Ref<Unit?>.IsAny), Times.Once);
		}

		[TestMethod]
		public async Task AttackUnit_IfEnemyGuidNotExist_Returns()
		{
			// Arrange

			var mocker = new AutoMocker();
			Guid.TryParse("AAAAAAAA-B402-49D3-8D17-D245069CA13D", out var userGuid);
			Guid.TryParse("DDDDDDDD-B402-49D3-8D17-D245069CA13D", out var enemyGuid);

			Unit? bot = new Bot(enemyGuid, "SomeBot", UnitTypes.Mob, 10.00f, 20.00f, 100);
			var unitStorageMock = new Mock<IStorage<Unit>>();
			unitStorageMock.Setup(x => x.TryGetValue(userGuid, out bot)).Returns(true);
			unitStorageMock.Setup(x => x.TryGetValue(enemyGuid, out bot)).Returns(false);

			var cacheFactoryMock = new Mock<ICacheFactory>();
			cacheFactoryMock.Setup(x => x.Create<Unit>()).Returns(unitStorageMock.Object);

			mocker.Use(cacheFactoryMock);

			var target = mocker.CreateInstance<CombatService>();

			// Act

			await target.AttackUnit(userGuid, enemyGuid, CancellationToken.None);

			// Assert

			unitStorageMock.Verify(x => x.TryGetValue(userGuid, out It.Ref<Unit?>.IsAny), Times.Once);
			unitStorageMock.Verify(x => x.TryGetValue(enemyGuid, out It.Ref<Unit?>.IsAny), Times.Once);
			unitStorageMock.Verify(x => x.TryGetValue(It.IsAny<Guid>(), out It.Ref<Unit?>.IsAny), Times.Exactly(2));
			mocker.Verify<IMovableService>(x =>
				x.Rotate(It.IsAny<Guid>(), It.IsAny<float>(), It.IsAny<float>(), CancellationToken.None), Times.Never);
			mocker.Verify<IMovableService>(x =>
				x.StartMove(It.IsAny<Guid>(), It.IsAny<Guid>(), CancellationToken.None, It.IsAny<float?>()), Times.Never);
		}

		[TestMethod]
		public async Task AttackUnit_IfExistsAndHandguns_StartsMoveToEnemyAndRotate()
		{
			// Arrange

			var mocker = new AutoMocker();
			Guid.TryParse("AAAAAAAA-B402-49D3-8D17-D245069CA13D", out var userGuid);
			Guid.TryParse("DDDDDDDD-B402-49D3-8D17-D245069CA13D", out var enemyGuid);

			Unit? user = new Bot(userGuid, "SomeBot", UnitTypes.Mob, 10.00f, 20.00f, 100, WeaponModels.Fist);
			Unit? bot = new Bot(enemyGuid, "SomeBot", UnitTypes.Mob, 10.01f, 20.01f, 100, WeaponModels.Fist);

			var unitStorageMock = new Mock<IStorage<Unit>>();
			unitStorageMock.Setup(x => x.TryGetValue(userGuid, out user)).Returns(true);

			unitStorageMock.SetupSequence(x => x.TryGetValue(enemyGuid, out bot))
				.Returns(true)
				.Returns(() =>
			{
				// It is necessary to bring the enemy under attack
				bot.Location.ChangeLocation(new Vector2(20.00f, 10.00f));
				bot.Location.SaveCurrentLocation();
				unitStorageMock.SetupSequence(x => x.TryGetValue(userGuid, out bot)).Returns(true);
				return true;
			});

			var cacheFactoryMock = new Mock<ICacheFactory>();
			cacheFactoryMock.Setup(x => x.Create<Unit>()).Returns(unitStorageMock.Object);

			mocker.Use(cacheFactoryMock);

			var target = mocker.CreateInstance<CombatService>();

			// Act

			await target.AttackUnit(userGuid, enemyGuid, CancellationToken.None);

			// Assert

			mocker.Verify<IMovableService>(x =>
			x.StartMove(userGuid, enemyGuid, CancellationToken.None, It.IsAny<float?>()), Times.Once);
			mocker.Verify<IMovableService>(x =>
				x.Rotate(userGuid, 10.01f, 20.01f, CancellationToken.None), Times.Once);

		}

		[TestMethod]
		public async Task AttackUnit_KillsEnemy()
		{
			// Arrange

			var mocker = new AutoMocker();
			Guid.TryParse("AAAAAAAA-B402-49D3-8D17-D245069CA13D", out var userGuid);
			Guid.TryParse("DDDDDDDD-B402-49D3-8D17-D245069CA13D", out var enemyGuid);

			Unit? user = new Bot(userGuid, "SomeBot", UnitTypes.Mob, 10.00f, 20.00f, 100, WeaponModels.Fist);
			Unit? bot = new Bot(enemyGuid, "SomeBot", UnitTypes.Mob, 10.00f, 20.00f, 1, WeaponModels.Fist);

			var unitStorageMock = new Mock<IStorage<Unit>>();
			unitStorageMock.Setup(x => x.TryGetValue(userGuid, out user)).Returns(true);
			unitStorageMock.Setup(x => x.TryGetValue(enemyGuid, out bot)).Returns(true);

			var cacheFactoryMock = new Mock<ICacheFactory>();
			cacheFactoryMock.Setup(x => x.Create<Unit>()).Returns(unitStorageMock.Object);

			mocker.Use(cacheFactoryMock);

			var target = mocker.CreateInstance<CombatService>();

			// Act

			await target.AttackUnit(userGuid, enemyGuid, CancellationToken.None);

			// Assert

			mocker.Verify<IYandexJsClientNotifier>(x =>
				x.KillUnit(enemyGuid), Times.Once);
		}
	}
}