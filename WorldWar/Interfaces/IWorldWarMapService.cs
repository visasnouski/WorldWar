namespace WorldWar.Interfaces;

public interface IWorldWarMapService
{
    Task RunItemsAutoRefresh(bool viewAllItems = false);

    Task RunUnitsAutoRefreshAsync(bool viewAllUnits = false);
}