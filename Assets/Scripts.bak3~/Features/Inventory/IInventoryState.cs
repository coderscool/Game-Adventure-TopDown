namespace Game.Features.Inventory
{
    /// <summary>Inventory snapshot port. Implemented by the Presentation layer.</summary>
    public interface IInventoryState
    {
        InventoryItemData[] CaptureInventory();

        /// <summary>Rebuilds the inventory from a snapshot. A null/empty snapshot yields an empty inventory.</summary>
        void RestoreInventory(InventoryItemData[] data);
    }
}
