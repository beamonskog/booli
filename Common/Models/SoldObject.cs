using System.Collections.Generic;

namespace Common.Models
{
    public class SoldObject
    {
        public int Id { get; set; }
        public Location Location { get; set; }
        public double LivingArea { get; set; }
        public int AdditionalArea { get; set; }
        public double Rooms { get; set; }
        public string ObjectType { get; set; } // borde vara enum
        public int BooliId { get; set; }// irrelevant
        public string SoldDate { get; set; }
        public int SoldPrice { get; set; }
        public int? Rent { get; set; }
    }

    public class Location
    {
        public int Id { get; set; }
        public Position Position { get; set; }
        public List<NamedArea> NamedAreas { get; set; }
    }

    public class NamedArea
    {
        public int Id { get; set; }
        public string Area { get; set; }
    }

    public class Position
    {
        public Position(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
