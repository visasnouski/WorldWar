using System.Security.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Repository.Models;
using WorldWar.YandexClient.Internal;
using WorldWar.YandexClient.Model;

namespace WorldWar.YandexClient.Tests.Internal
{
	[TestClass]
	public class YandexJsClientTests
	{
		[TestMethod]
		public async Task GetYandexJsModule_IfGetIdentityThrowAuthenticationException_SetCoordsByDefault()
		{
			// Arrange

			var yandexMapModuleMock = new Mock<IJSObjectReference>();

			var jsRuntimeMock = SetupJsRuntimeMock(yandexMapModuleMock.Object);

			var authUserMock = new Mock<IAuthUser>();
			authUserMock.Setup(x => x.GetIdentity())
				.ThrowsAsync(new AuthenticationException("Unknown identity"));

#pragma warning disable CS0618 // Type or member is obsolete
			var yandexSettings = new YandexSettings { ApiKey = "SomeApiKey", ApiSrc = "https://WorldWar/{0}" };
#pragma warning restore CS0618 // Type or member is obsolete

			var mocker = SetupMocker(jsRuntimeMock.Object, authUserMock.Object, yandexSettings);
			var target = mocker.CreateInstance<YandexJsClient>();

			// Act

			await target.GetYandexJsModule("SomeScript.js");

			// Assert

			yandexMapModuleMock.Verify(x => x.InvokeAsync<IJSVoidResult>("addScript", It.Is<object?[]>(objects => objects.Contains(yandexSettings.YandexApiSrc))), Times.Once);
			yandexMapModuleMock.Verify(x => x.InvokeAsync<IJSVoidResult>("setCoords", It.Is<object?[]>(objects => objects.Contains(27.561831) && objects.Contains(53.902284))), Times.Once);
		}

		[TestMethod]
		public async Task GetYandexJsModule_InitializesCorrectJsModule()
		{
			// Arrange

			var yandexMapModuleMock = new Mock<IJSObjectReference>();

			var jsRuntimeMock = SetupJsRuntimeMock(yandexMapModuleMock.Object);

			var authUserMock = new Mock<IAuthUser>();
			authUserMock.Setup(x => x.GetIdentity()).ReturnsAsync(new WorldWarIdentity()
			{ Latitude = 10.00, Longitude = 20.00 });

#pragma warning disable CS0618 // Type or member is obsolete
			var yandexSettings = new YandexSettings { ApiKey = "SomeApiKey", ApiSrc = "https://WorldWar/{0}" };
#pragma warning restore CS0618 // Type or member is obsolete

			var mocker = SetupMocker(jsRuntimeMock.Object, authUserMock.Object, yandexSettings);
			var target = mocker.CreateInstance<YandexJsClient>();

			// Act

			await target.GetYandexJsModule("SomeScript.js");

			// Assert

			yandexMapModuleMock.Verify(x => x.InvokeAsync<IJSVoidResult>("addScript", It.Is<object?[]>(objects => objects.Contains(yandexSettings.YandexApiSrc))), Times.Once);
			yandexMapModuleMock.Verify(x => x.InvokeAsync<IJSVoidResult>("setCoords", It.Is<object?[]>(objects => objects.Contains(10.00) && objects.Contains(20.00))), Times.Once);
		}

		[TestMethod]
		public async Task GetYandexJsModule_ITaskDelay_Delay1Second()
		{
			// Arrange

			var yandexMapModuleMock = new Mock<IJSObjectReference>();
			var jsRuntimeMock = SetupJsRuntimeMock(yandexMapModuleMock.Object);

			var authUserMock = new Mock<IAuthUser>();
			authUserMock.Setup(x => x.GetIdentity()).ReturnsAsync(new WorldWarIdentity()
			{ Latitude = 10.00, Longitude = 20.00 });

#pragma warning disable CS0618 // Type or member is obsolete
			var yandexSettings = new YandexSettings { ApiKey = "SomeApiKey", ApiSrc = "https://WorldWar/{0}" };
#pragma warning restore CS0618 // Type or member is obsolete

			var taskDelayMock = new Mock<ITaskDelay>();
			var mocker = SetupMocker(jsRuntimeMock.Object, authUserMock.Object, yandexSettings, taskDelayMock.Object);

			var target = mocker.CreateInstance<YandexJsClient>();

			// Act

			await target.GetYandexJsModule("SomeScript.js");

			// Assert

			taskDelayMock.Verify(x => x.Delay(It.Is<TimeSpan>(delay => delay.TotalSeconds.Equals(1)), CancellationToken.None), Times.Once);
		}

		[TestMethod]
		public async Task GetYandexJsModule_ReturnsCorrectJsModule()
		{
			// Arrange

			var yandexMapModuleMock = new Mock<IJSObjectReference>();

			var jsRuntimeMock = SetupJsRuntimeMock(yandexMapModuleMock.Object);

			var authUserMock = new Mock<IAuthUser>();
			authUserMock.Setup(x => x.GetIdentity()).ReturnsAsync(new WorldWarIdentity()
			{ Latitude = 10.00, Longitude = 20.00 });

#pragma warning disable CS0618 // Type or member is obsolete
			var yandexSettings = new YandexSettings { ApiKey = "SomeApiKey", ApiSrc = "https://WorldWar/{0}" };
#pragma warning restore CS0618 // Type or member is obsolete

			var mocker = SetupMocker(jsRuntimeMock.Object, authUserMock.Object, yandexSettings);

			var target = mocker.CreateInstance<YandexJsClient>();

			// Act

			var result = await target.GetYandexJsModule("SomeScript.js");

			// Assert

			Assert.AreEqual(yandexMapModuleMock.Object, result);
		}

		private static AutoMocker SetupMocker(IJSRuntime jsRuntime, IAuthUser authUser, YandexSettings yandexSettings, ITaskDelay? taskDelay = null)
		{
			var mocker = new AutoMocker();
			mocker.Use(jsRuntime);
			mocker.Use(authUser);
			mocker.Use(taskDelay ?? Mock.Of<ITaskDelay>());
			mocker.Use(Options.Create(yandexSettings));
			return mocker;
		}

		private static Mock<IJSRuntime> SetupJsRuntimeMock(IJSObjectReference yandexMapModule)
		{
			var jsRuntimeMock = new Mock<IJSRuntime>();
			jsRuntimeMock.Setup(x => x.InvokeAsync<IJSObjectReference>("import", new object?[] { "./js/YandexMap.js" }))
				.ReturnsAsync(yandexMapModule);

			jsRuntimeMock.Setup(x => x.InvokeAsync<IJSObjectReference>("import", new object?[] { "SomeScript.js" }))
				.ReturnsAsync(yandexMapModule);
			return jsRuntimeMock;
		}
	}
}