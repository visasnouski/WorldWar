using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Options;
using Moq.AutoMock;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Repository.Models;
using WorldWar.YandexClient.Internal;
using WorldWar.YandexClient.Model;
using WorldWar.YandexClient.Tests.Internal.Data;

namespace WorldWar.YandexClient.Tests.Internal
{
	[TestClass]
	public class YandexJsClientAdapterTests
	{
		[DataTestMethod]
		[DataRow(UnitTypes.Player, "addUnit")]
		[DataRow(UnitTypes.Mob, "addUnit")]
		[DataRow(UnitTypes.Car, "addCar")]
		public async Task AddUnit_CallsCorrect(UnitTypes unitTypes, string identifier)
		{
			// Arrange

			var player = new StubUnit(unitTypes);
			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();

			// Act

			await target.AddUnit(player);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>(identifier, new object?[] { player }), Times.Once);
		}

		[TestMethod]
		public async Task RemoveGeoObjects_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();
			var guids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

			// Act

			await target.RemoveGeoObjects(guids);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("removeGeoObjects", new object?[] { guids }), Times.Once);
		}

		[TestMethod]
		public async Task KillUnit_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();
			var guid = Guid.NewGuid();

			// Act

			await target.KillUnit(guid);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("killUnit", new object?[] { guid }), Times.Once);
		}


		[TestMethod]
		public async Task UpdateUnit_CallsCorrect()
		{
			// Arrange

			var player = new StubUnit();
			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();

			// Act

			await target.UpdateUnit(player);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("updateUnit", new object?[] { player }), Times.Once);
		}

		[TestMethod]
		public async Task ShootUnit_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();
			var guid = Guid.NewGuid();

			// Act

			await target.ShootUnit(guid, 10.00f, 20.00f);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("shoot", new object?[] { guid, 10.00f, 20.00f }), Times.Once);
		}

		[TestMethod]
		public async Task ShowMessage_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();
			var guid = Guid.NewGuid();

			// Act

			await target.ShowMessage(guid, "SomeMessage");

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("showMessage", new object?[] { guid, "SomeMessage" }), Times.Once);
		}

		[TestMethod]
		public async Task PlaySound_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();

			// Act

			await target.PlaySound("id", "SomeSrc");

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("playAudio", new object?[] { "id", "SomeSrc" }), Times.Once);
		}

		[TestMethod]
		public async Task RotateUnit_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();
			var guid = Guid.NewGuid();

			// Act

			await target.RotateUnit(guid, 10.00f, 20.00f);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("rotateUnit", new object?[] { guid, 10.00f, 20.00f }), Times.Once);
		}

		[TestMethod]
		public async Task AddBox_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();
			var guid = Guid.NewGuid();
			var box = new Box(guid, 10.00f, 20.00f, new List<Item>());

			// Act

			await target.AddBox(box);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("addBox", new object?[] { box }), Times.Once);
		}

		[TestMethod]
		public async Task SetPlayerManager_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);
			var playerManager = DotNetObjectReference.Create(Mock.Of<IPlayerManager>());

			var target = mocker.CreateInstance<YandexJsClientAdapter>();

			// Act

			await target.SetPlayerManager(playerManager);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("setPlayerManager", new object?[] { playerManager }), Times.Once);
		}

		[TestMethod]
		public async Task SetUnitEquipmentComponent_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);
			var someObject = DotNetObjectReference.Create(new object());

			var target = mocker.CreateInstance<YandexJsClientAdapter>();

			// Act

			await target.SetUnitEquipmentComponent(someObject);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("setUnitEquipmentComponent", new object?[] { someObject }), Times.Once);
		}

		[TestMethod]
		public async Task SetModalDialogBoxContents_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();
			var someObject = DotNetObjectReference.Create(new object());

			// Act

			await target.SetModalDialogBoxContents(someObject);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("setModalDialogBoxContents", new object?[] { someObject }), Times.Once);
		}

		[TestMethod]
		public async Task UpdateUnits_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();
			Unit[] units = { new StubUnit(UnitTypes.Mob), new StubUnit(UnitTypes.Mob) };

			// Act

			await target.UpdateUnits(units);

			// Assert

			jsObjectMock.Verify(x => x.InvokeAsync<IJSVoidResult>("updateUnits", new object?[] { units }), Times.Once);
		}

		[TestMethod]
		public async Task ConvertPixelCoordsToGlobal_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);

			var expectedCoords = new[] { 10.00f, 20.00f };
			jsObjectMock.Setup(x => x.InvokeAsync<float[]>("convertPixelCoordsToGlobal", new object?[] { 10d, 20d }))
				.ReturnsAsync(expectedCoords);

			var target = mocker.CreateInstance<YandexJsClientAdapter>();

			// Act

			var coords = await target.ConvertPixelCoordsToGlobal(10, 20);

			// Assert

			Assert.AreEqual(expectedCoords, coords);
		}

		[TestMethod]
		public async Task GetCenterCoords_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);
			var expectedCoords = new[] { 10.00f, 20.00f };

			var target = mocker.CreateInstance<YandexJsClientAdapter>();
			jsObjectMock.Setup(x => x.InvokeAsync<float[]>("getCenter", new object?[] { }))
				.ReturnsAsync(expectedCoords);

			// Act

			var coords = await target.GetCenterCoords();

			// Assert

			Assert.AreEqual(expectedCoords, coords);
		}

		[TestMethod]
		public async Task GetRoute_CallsCorrect()
		{
			// Arrange

			var mocker = SetupMocker(out var jsObjectMock);
			var startCoords = new[] { 10.00f, 20.00f };
			var endCoords = new[] { 11.00f, 21.00f };
			var expectedRoute = new[] { new[] { 10.00f }, new[] { 20.00f } };

			var target = mocker.CreateInstance<YandexJsClientAdapter>();

			jsObjectMock.Setup(x =>
					x.InvokeAsync<float[][]>("getRoute", new object[] { startCoords, endCoords, "someRoutingModel" }))
				.ReturnsAsync(expectedRoute);

			// Act

			var route = await target.GetRoute(startCoords, endCoords, "someRoutingModel");

			// Assert

			Assert.AreEqual(expectedRoute, route);
		}

		private static AutoMocker SetupMocker(out Mock<IJSObjectReference> jsObjectMock)
		{
			jsObjectMock = new Mock<IJSObjectReference>();
			var jsRuntimeMock = SetupJsRuntimeMock(jsObjectMock.Object);

			var authUserMock = new Mock<IAuthUser>();
			authUserMock.Setup(x => x.GetIdentity()).ReturnsAsync(new WorldWarIdentity()
			{ Latitude = 10.00, Longitude = 20.00 });

			var mocker = new AutoMocker();
			mocker.Use(jsRuntimeMock.Object);
			mocker.Use(authUserMock.Object);
			mocker.Use(Mock.Of<ITaskDelay>());
			mocker.Use(Options.Create(new YandexSettings()));
			return mocker;
		}

		private static Mock<IJSRuntime> SetupJsRuntimeMock(IJSObjectReference yandexMapModule)
		{
			var jsRuntimeMock = new Mock<IJSRuntime>();
			jsRuntimeMock.Setup(x => x.InvokeAsync<IJSObjectReference>("import", new object?[] { "./js/YandexMap.js" }))
				.ReturnsAsync(yandexMapModule);

			jsRuntimeMock.Setup(x => x.InvokeAsync<IJSObjectReference>("import", new object?[] { "./js/WorldWarMap.js" }))
				.ReturnsAsync(yandexMapModule);
			return jsRuntimeMock;
		}
	}
}
