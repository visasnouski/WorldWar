namespace WorldWar.Abstractions;

public interface IAuthUser
{
    Task<IWorldWarIdentityUser> GetIdentity();
}