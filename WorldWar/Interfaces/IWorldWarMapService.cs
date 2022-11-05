namespace WorldWar.Interfaces;

public interface IWorldWarMapService
{
    Task RunUnitsAutoRefresh(bool viewAllUnits = false);

    Task RunItemsAutoRefresh(bool viewAllItems = false);

    Task RunAutoDbSync();
}