using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Repository.interfaces;

public interface IDbRepository
{
    IReadOnlyCollection<Weapon> Weapons { get; }

    IReadOnlyCollection<BodyProtection> BodyProtections { get; }

    IReadOnlyCollection<HeadProtection> HeadProtections { get; }

    IReadOnlyCollection<Unit> Units { get; }

    public Task<Weapon> GetWeapon(int id);

    Task<BodyProtection> GetBodyProtection(int id);

    Task<HeadProtection> GetHeadProtection(int id);

    public Task<Unit> GetUnit(Guid id);

    public Task SetUnit(Unit unit);

    Task UpdateUnit(Unit unit);

    Task SetUnits(IEnumerable<Unit> units, CancellationToken cancellationToken);
}