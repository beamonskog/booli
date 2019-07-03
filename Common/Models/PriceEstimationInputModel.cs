namespace Common.Models
{
    public class PriceEstimationInputModel
    {
        public PriceEstimationInputModel(int size, string areaName, int rent, Position position)
        {
            Size = size;
            AreaName = areaName;
            Rent = rent;
            Position = position;
        }

        public int Size { get; set; }
        public int AdditionalArea { get; set; } = 0;
        public string AreaName { get; set; }
        public Position Position { get; set; }
        public int Rent { get; set; }
        public RentRange RentRange { get; set; }
    }
}
