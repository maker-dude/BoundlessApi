using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using BoundlessApi.DataItems;
using BoundlessApi.Enum;
using Newtonsoft.Json;

namespace BoundlessApi
{
    public class BoundlessHttpApi
    {
        private Uri _Uri;
        public BoundlessWorld[] Worlds { get; set; } = new BoundlessWorld[0];

        public int RequestDelayMs { get; set; } = 1500;
        private DateTime _CanRequestAfter = DateTime.MinValue;
        bool _IsReady = false;
        HttpClient _ApiClient;


        public void Init(Uri apiUri, string apiKey)
        {
            _Uri = apiUri;
            _IsReady = true;
            _ApiClient = new HttpClient();
            _ApiClient.DefaultRequestHeaders.Add("Boundless-API-Key", apiKey);
            LoadWorlds();
        }

        private void LoadWorlds()
        {
            // Download the json data about the worlds
            string apiData = _ApiClient.GetStringAsync($"{_Uri.AbsoluteUri}list-gameservers").Result;

            // Update the request delay.
            _CanRequestAfter = DateTime.Now.AddMilliseconds(RequestDelayMs);

            // parse the json data.
            using (TextReader textReader = new StringReader(apiData))
            using (JsonReader reader = new JsonTextReader(textReader))
            {
                string worldName = string.Empty;
                string apiString = string.Empty;
                int worldId = 0;

                // Parse the json data for the items we care about and save them.
                List<BoundlessWorld> worlds = new List<BoundlessWorld>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        if (string.Equals(reader.Value, "apiURL"))
                        {
                            apiString = reader.ReadAsString();
                        }
                        else if (string.Equals(reader.Value, "displayName"))
                        {
                            worldName = reader.ReadAsString();
                        }
                        else if (string.Equals(reader.Value, "id"))
                        {
                            worldId = reader.ReadAsInt32().Value;
                        }
                    }
                    else if (reader.TokenType == JsonToken.EndObject && reader.Depth == 1)
                    {
                        // we've ended an item, that means we have everything we need to save a world off.
                        worlds.Add(new BoundlessWorld() { ApiUrlString = apiString, WorldName = worldName, WorldId = worldId });

                        // reset the values for the next loop.
                        worldName = string.Empty;
                        apiString = string.Empty;
                        worldId = 0;
                    }
                }

                // store all of the world information that was found.
                Worlds = worlds.ToArray();
            }

        }

        public async Task<ShopItem[]> GetSellerDataForItem(ItemIds itemId)
        {
            // If the api hasn't been initialized then get out.
            if (!_IsReady)
                throw new Exception("Api has not been initialized");

            // Request data about the specified item for each planet
            List<ShopItem> results = new List<ShopItem>();
            foreach (BoundlessWorld world in Worlds)
            {
                // Download the data, respecting delay times.
                byte[] apiData = await RequestBytes($"{world.ApiUrlString}/shopping/S/{(int)itemId}");

                // Parse the result data for return to the caller.			
                results.AddRange(ParseData(apiData, itemId, world.WorldId));
            }

            // Return the data we downloaded.
            return results.ToArray();
        }

        public async Task<ShopItem[]> GetSellerDataForItem(BoundlessWorld world, ItemIds itemId)
        {
            // If the api hasn't been initialized then get out.
            if (!_IsReady)
                throw new Exception("Api has not been initialized");

            // Download the data, respecting delay times.
            List<ShopItem> results = new List<ShopItem>();
            byte[] apiData = await RequestBytes($"{world.ApiUrlString}/shopping/S/{(int)itemId}");

            // Parse the result data for return to the caller.			
            return ParseData(apiData, itemId, world.WorldId).ToArray();
        }

        public async Task<IEnumerable<ShopItem>> GetBuyerDataForItem(ItemIds itemId)
        {
            // If the api hasn't been initialized then get out.
            if (!_IsReady)
                throw new Exception("Api has not been initialized");

            // Request data about the specified item for each planet
            List<ShopItem> results = new List<ShopItem>();
            foreach (BoundlessWorld world in Worlds)
            {
                // Download the data, respecting delay times.
                byte[] apiData = await RequestBytes($"{world.ApiUrlString}/shopping/B/{(int)itemId}");

                // Parse the result data for return to the caller.
                results.AddRange(ParseData(apiData, itemId, world.WorldId));
            }

            // Return the data we downloaded.
            return results.ToArray();
        }

        public async Task<ShopItem[]> GetBuyerDataForItem(BoundlessWorld world, ItemIds itemId)
        {
            // If the api hasn't been initialized then get out.
            if (!_IsReady)
                throw new Exception("Api has not been initialized");

            // Download the data, respecting delay times.
            List<ShopItem> results = new List<ShopItem>();
            byte[] apiData = await RequestBytes($"{world.ApiUrlString}/shopping/B/{(int)itemId}");

            // Parse the result data for return to the caller.			
            return ParseData(apiData, itemId, world.WorldId).ToArray();
        }

        private IEnumerable<ShopItem> ParseData(byte[] data, ItemIds itemId, int worldId)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    byte shopNameLength = reader.ReadByte();
                    byte guildTagLength = reader.ReadByte();
                    char[] shopName = reader.ReadChars(shopNameLength);
                    char[] guildTag = reader.ReadChars(guildTagLength);
                    uint quantity = reader.ReadUInt32();
                    uint activity = reader.ReadUInt32();
                    long price = reader.ReadInt64();
                    short xPos = reader.ReadInt16();
                    short zPos = reader.ReadInt16();
                    byte yPos = reader.ReadByte();
                    yield return new ShopItem()
                    {
                        GuildTag = new string(guildTag),
                        ShopName = new string(shopName),
                        Price = price / 100d,
                        Quantity = quantity,
                        Position = new Vector3(xPos, yPos, zPos),
                        ItemId = itemId,
                        WorldId = worldId,
                    };
                }
            }
        }

        private async Task<byte[]> RequestBytes(string requestString)
        {
            // Delay the request until it's safe to continue.
            DateTime now = DateTime.Now;
            if (now < _CanRequestAfter)
                Task.Delay(_CanRequestAfter.Subtract(now)).Wait();

            // Download the information requested.
            byte[] results = await _ApiClient.GetByteArrayAsync(requestString);

            // Determine when we are allowed to request again.
            _CanRequestAfter = DateTime.Now.AddMilliseconds(RequestDelayMs);

            // Return the requested data.
            return results;
        }
    }
}
