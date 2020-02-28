namespace BoundlessApi.DataItems
{
    public class BoundlessWorld
    {
        public string ApiUrlString { get; set; }

        public int WorldId { get; set; }

        public string WorldName { get; set; }

        public override string ToString()
        {
            return $"{WorldId}\t{WorldName}\t{ApiUrlString}";
        }
    }
}
