namespace WorldWar.Components.States;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public delegate Task AsyncEventHandler();
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix

public class InteractStates
{
	public bool IsShow { get; set; }

	public bool IsUnit { get; set; }

	public Guid Id { get; set; }

	public event AsyncEventHandler? OnChange;

	public void Show(Guid id, bool isUnit = false)
	{
		Id = id;
		IsShow = true;
		IsUnit = isUnit;
		NotifyStateChanged();
	}

	private void NotifyStateChanged()
	{
		OnChange?.Invoke();
	}
}

