using WorldWar.Abstractions.Models.Items.Base;

namespace WorldWar.Abstractions.Models;

public class DraggableItem
{
	private (string Source, Item? Item) _value;
	public (string Source, Item? Item) Value
	{
		get => _value;
		set
		{
			_value = value;
			NotifyDataChanged();
		}
	}

	public event Action? OnChange;

	private void NotifyDataChanged() => OnChange?.Invoke();
}