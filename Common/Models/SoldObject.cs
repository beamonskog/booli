using System.Collections.Generic;

namespace Common.Models
{
    public class SoldObject
    {
        public int Id { get; set; }
        public Location Location { get; set; }
        //public int ListPrice { get; set; } // irrelevant
        public double LivingArea { get; set; }
        public int AdditionalArea { get; set; }
        public int PlotArea { get; set; }
        //public Source Source { get; set; } // irrelevant
        public double Rooms { get; set; }
        //public string Published { get; set; } // irrelevant
        //public int ConstructionYear { get; set; }// relevant men prioriterar bort
        public string ObjectType { get; set; } // borde vara enum
        public int BooliId { get; set; }// irrelevant
        public string SoldDate { get; set; }
        public int SoldPrice { get; set; }
        //public string SoldPriceSource { get; set; }// irrelevant
        //public string Url { get; set; }// irrelevant
        public int? Rent { get; set; }
        public double? Floor { get; set; }
        //public string ApartmentNumber { get; set; }// irrelevant
    }

    public class Location
    {
        public int Id { get; set; }
        //public string Address { get; set; }  // relevant men prioriterar bort
        public Position Position { get; set; }
        public List<NamedArea> NamedAreas { get; set; } // relevant men prioriterar bort
        //public Region Region { get; set; } // relevant men prioriterar bort
        //public int DistanceFromOcean { get; set; } // relevant men prioriterar bort
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
