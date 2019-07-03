using System.Collections.Generic;

namespace Common.Models
{
    public class SoldObject
    {
        public Location Location { get; set; }
        public double LivingArea { get; set; }
        public int AdditionalArea { get; set; }
        //public int PlotArea { get; set; }// kollar främst på lägenheter
        public double Rooms { get; set; }
        public string ObjectType { get; set; } // borde vara enum
        public int BooliId { get; set; }// irrelevant
        public string SoldDate { get; set; }
        public int SoldPrice { get; set; }
        public int? Rent { get; set; }
        //public double? Floor { get; set; }// relevant men prioriterar bort. 
    }

    public class Location
    {
        public Position Position { get; set; }
        public List<NamedArea> NamedAreas { get; set; }
    }

    public class NamedArea
    {
        public string Area { get; set; }
    }

    public class Position
    {
        public Position(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
