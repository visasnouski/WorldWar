using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using WorldWar.Abstractions;
using WorldWar.Abstractions.Models;
using WorldWar.Areas.Identity;
using WorldWar.Components.States;
using WorldWar.Data;
using WorldWar.Data.Repository;
using WorldWar.Interfaces;
using WorldWar.Internal;
using WorldWar.YandexClient;
using WorldWar.YandexClient.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// TODO Adjust RequireConfirmedAccount
builder.Services.AddDefaultIdentity<MyIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddUserValidator<UserEmailValidator<MyIdentityUser>>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<MyIdentityUser>>();
builder.Services.AddSingleton<IMapStorage, MapStorage>();

builder.Services.AddScoped<ITaskDelay, TaskDelay>();
builder.Services.AddScoped<IWorldWarMapService, WorldWarMapService>();
builder.Services.AddScoped<IUnitManagementService, UnitManagementService>();
builder.Services.AddScoped<ICombatService, CombatService>();
builder.Services.AddScoped<IMovableService, MovableService>();
builder.Services.AddScoped<IAuthUser, AuthUser>();
builder.Services.AddScoped<UserManager<MyIdentityUser>>();
builder.Services.AddScoped<IRegisterModelServices, RegisterModelServices>();
builder.Services.AddScoped<DraggableItem>();
builder.Services.AddScoped<UnitEquipmentDialogState>();
builder.Services.AddScoped<InteractStates>();

builder.Services.AddScoped<IInteractionObjectsService, InteractionObjectsService>();

builder.Services.AddSingleton<IDbRepository, DbRepository>();
builder.Services.Configure<YandexSettings>(options => builder.Configuration.GetSection("YandexMap").Bind(options));

builder.Services.AddYandexClient();

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
