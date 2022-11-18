namespace WorldWar.Abstractions.Interfaces;

public interface IAuthUser
{
    Task<IWorldWarIdentityUser> GetIdentity();
}