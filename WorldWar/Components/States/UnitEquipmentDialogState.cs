namespace WorldWar.Components.States
{
    public class UnitEquipmentDialogState
    {
        public bool IsShow { get; set; }

        public event AsyncEventHandler? OnChange;

        public void Show()
        {
            IsShow = true;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}
