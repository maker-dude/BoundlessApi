using System.Numerics;
using BoundlessApi.Enum;

namespace BoundlessApi.DataItems
{
    public class ShopItem
    {
        public string GuildTag { get; set; }

        public ItemIds ItemId { get; set; }

        public double Price { get; set; }

        public uint Quantity { get; set; }

        public Vector3 Position { get; set; }

        public string ShopName { get; set; }

        public int WorldId { get; set; }

        public override string ToString()
        {
            return $"{GuildTag}\t{ShopName}\t{ItemId}\t{Quantity}\t{Price}\t{WorldId}\t{Position.ToString()}";
        }
    }
}
