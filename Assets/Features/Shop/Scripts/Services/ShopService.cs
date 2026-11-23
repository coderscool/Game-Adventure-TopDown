using Game.Features.Character.Model;
using Game.Features.Inventory;
using Game.Features.Shop.Data;

namespace Game.Features.Shop.Services
{
    /// <summary>The buying rules. No UI here, so it can be tested on its own.</summary>
    public static class ShopService
    {
        private const int PurchaseAmount = 1;

        public static BuyResult TryBuy(IPlayerState player, InventoryController inventory, ShopEntry entry)
        {
            if (player == null || inventory == null || entry == null || entry.Item == null)
                return BuyResult.Invalid;

            if (player.Gold < entry.Price)
                return BuyResult.NotEnoughGold;

            // Check room first so gold is never taken for an item that does not fit.
            if (!inventory.CanAddItem(entry.Item, PurchaseAmount))
                return BuyResult.InventoryFull;

            player.Gold -= entry.Price;
            inventory.AddItem(entry.Item, PurchaseAmount);
            return BuyResult.Success;
        }
    }
}
