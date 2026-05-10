// 

using SilkySouls3.Models;

namespace SilkySouls3.Interfaces;

public interface IItemService
{
    void SpawnItem(int itemId, int quantity, bool shouldAdjustQuantity, int maxQuantity);
    void DropItem(ItemDrop itemDrop);
}