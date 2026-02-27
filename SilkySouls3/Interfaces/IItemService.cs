// 

namespace SilkySouls3.Interfaces;

public interface IItemService
{
    void SpawnItem(int itemId, int quantity, bool shouldAdjustQuantity, int maxQuantity);
}