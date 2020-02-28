# BoundlessApi
A simple and straightforward API for making tools against the Boundless Shop API

Example usage:
``` c
// Initialize the API.
BoundlessHttpApi api = new BoundlessHttpApi();
api.Init(new Uri("http://127.0.0.1:8950/"), "");

// Dump the world listing.
foreach (BoundlessWorld world in api.Worlds)
{
    Console.WriteLine(world.ToString());
}

// Dump the shop seller data for Bones.
foreach (ShopItem item in api.GetSellerDataForItem(ItemIds.ITEM_BONE_BASE).Result)
{
    Console.WriteLine(item.ToString());
}

// Dump the shop buyer data for Bones.
foreach (ShopItem item in api.GetBuyerDataForItem(ItemIds.ITEM_BONE_BASE).Result)
{
    Console.WriteLine(item.ToString());
}
```