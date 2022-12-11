using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Utils;
using WorldWar.AI;
using WorldWar.Areas.Identity;
using WorldWar.Components.States;
using WorldWar.Core.Extensions;
using WorldWar.Interfaces;
using WorldWar.Internal;
using WorldWar.Repository;
using WorldWar.Repository.Models;
using WorldWar.YandexClient;
using WorldWar.YandexClient.Hubs;

var builder = WebApplication.CreateBuilder(args);

var identityBuilder =
	builder.Services.AddDefaultIdentity<WorldWarIdentity>(options => options.SignIn.RequireConfirmedAccount = true);
builder.Services.AddDbRepository(builder.Configuration.GetConnectionString("DefaultConnection"), identityBuilder);
identityBuilder.AddUserValidator<UserEmailValidator<WorldWarIdentity>>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<WorldWarIdentity>>();
builder.Services.AddSingleton<ITaskDelay, TaskDelay>();
builder.Services.AddScoped<IWorldWarMapService, WorldWarMapService>();

builder.Services.AddScoped<IUnitManagementService, UnitManagementService>();
builder.Services.AddScoped<IPlayerManager, PlayerManager>();

builder.Services.AddScoped<IAuthUser, AuthUser>();
builder.Services.AddScoped<UserManager<WorldWarIdentity>>();
builder.Services.AddScoped<IRegisterModelServices, RegisterModelServices>();
builder.Services.AddScoped<DraggableItem>();
builder.Services.AddScoped<UnitEquipmentDialogState>();
builder.Services.AddScoped<InteractStates>();

builder.Services.AddScoped<IInteractionObjectsService, InteractionObjectsService>();

builder.Services.AddYandexClient(options => builder.Configuration.GetSection("YandexMap").Bind(options));
builder.Services.AddAiService();

builder.Services.AddStorage();
builder.Services.AddUnitServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapHub<YandexMapHub>("/yandexMapHub");
app.MapFallbackToPage("/_Host");

app.Run();
