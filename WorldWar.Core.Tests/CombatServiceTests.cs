using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Interfaces;
using WorldWar.Core.Tests.Data;

namespace WorldWar.Core.Tests
{
	[TestClass]
	public class CombatServiceTests
	{
		[DataTestMethod]
		[DataRow(0, 100)]
		[DataRow(100, 0)]
		public async Task AttackUnit_IfUserOrEnemyIsDead_Returns(int userHealth, int enemyHealth)
		{
			// Arrange

			var mocker = new AutoMocker();

			var user = new StubUnit(Guid.Parse("AAAAAAAA-B402-49D3-8D17-D245069CA13D"), userHealth);
			var enemy = new StubUnit(Guid.Parse("DDDDDDDD-B402-49D3-8D17-D245069CA13D"), enemyHealth);

			var target = mocker.CreateInstance<CombatService>();

			// Act

			await target.AttackUnit(user, enemy, CancellationToken.None);

			// Assert

			mocker.Verify<IMovableService>(x => x.Rotate(It.IsAny<Unit>(), It.IsAny<float>(), It.IsAny<float>(), CancellationToken.None), Times.Never);
		}

		[TestMethod]
		public async Task AttackUnit_IfExistsAndHandguns_StartsMoveToEnemyAndRotate()
		{
			// Arrange

			var mocker = new AutoMocker();

			Unit user = new Bot(Guid.Parse("AAAAAAAA-B402-49D3-8D17-D245069CA13D"), "SomeBot", UnitTypes.Mob, 10.00f, 20.00f, 100, WeaponModels.Fist);
			Unit bot = new Bot(Guid.Parse("DDDDDDDD-B402-49D3-8D17-D245069CA13D"), "SomeBot", UnitTypes.Mob, 10.01f, 20.01f, 100, WeaponModels.Fist);

			var mockMovableService = new Mock<IMovableService>();
			mockMovableService.Setup(x => x.StartMove(user, bot, CancellationToken.None, bot.Weapon.Distance)).Callback(() =>
			{
				bot.Health = 0;
			}).Returns(Task.CompletedTask);
			mocker.Use(mockMovableService);

			var target = mocker.CreateInstance<CombatService>();

			// Act

			await target.AttackUnit(user, bot, CancellationToken.None);

			// Assert

			mocker.Verify<IMovableService>(x =>
			x.StartMove(user, bot, CancellationToken.None, It.IsAny<float?>()), Times.Once);
			mocker.Verify<IMovableService>(x =>
				x.Rotate(user, 10.01f, 20.01f, CancellationToken.None), Times.Once);
		}

		[TestMethod]
		public async Task AttackUnit_KillsEnemy_RemovesTasksForEnemy()
		{
			// Arrange

			var mocker = new AutoMocker();

			var enemyGuid = Guid.Parse("DDDDDDDD-B402-49D3-8D17-D245069CA13D");

			Unit user = new Bot(Guid.Parse("AAAAAAAA-B402-49D3-8D17-D245069CA13D"), "SomeBot", UnitTypes.Mob, 10.00f, 20.00f, 100, WeaponModels.Fist);
			Unit bot = new Bot(enemyGuid, "SomeBot", UnitTypes.Mob, 10.00f, 20.00f, 1, WeaponModels.Fist);

			var taskStorageMock = new Mock<ITasksStorage>();
			mocker.Use(taskStorageMock);

			var target = mocker.CreateInstance<CombatService>();

			// Act

			await target.AttackUnit(user, bot, CancellationToken.None);

			// Assert

			taskStorageMock.Verify(x => x.TryRemove(enemyGuid), Times.Once);
		}
	}
}