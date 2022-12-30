namespace WorldWar.Components.States;

public delegate Task AsyncEventHandler();

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

