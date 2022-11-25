using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Core.Interfaces;

//TODO Split this interface
public interface IMapStorage
{
    public Task<Unit> GetUnit(Guid id);

    public Task<Box> GetItem(Guid id);

    public Task SetUnit(Unit unit);

    public Task SetUnits(IEnumerable<Unit> units);

    public Task SetItem(Box item);

    Task<IEnumerable<Unit>> GetUnits();

    Task<IEnumerable<Box>> GetItems();

    Task RemoveUnit(Unit unit);

    Task RemoveItem(Box box);

    Task<IEnumerable<Unit>> GetVisibleUnits(Guid id);

    Task<IEnumerable<Box>> GetVisibleItems(Guid id);
}