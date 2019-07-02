using System.Collections.Generic;

namespace Common.Models.NativeBooli
{

    public class Address
    {
        public string streetAddress { get; set; }
    }

    public class Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Region
    {
        public string MunicipalityName { get; set; }
        public string COuntyName { get; set; }
    }

    public class Distance
    {
        public int Ocean { get; set; }
    }

    public class Location
    {
        public Address Address { get; set; }
        public Position Position { get; set; }
        public List<string> NamedAreas { get; set; }
        public Region Region { get; set; }
        public Distance Distance { get; set; }
    }

    public class Source
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }


    public class SearchParams
    {
        public int AreaId { get; set; }
    }

    public class RootObject
    {
        public int TotalCount { get; set; }
        public int Count { get; set; }
        public List<Sold> Sold { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public SearchParams SearchParams { get; set; }
    }

    public class Sold
    {
        public Location Location { get; set; }
        public int ListPrice { get; set; }
        public double LivingArea { get; set; }
        public int AdditionalArea { get; set; }
        public int PlotArea { get; set; }
        public Source Source { get; set; }
        public double Rooms { get; set; }
        public string Published { get; set; }
        public int ConstructionYear { get; set; }
        public string ObjectType { get; set; }
        public int BooliId { get; set; }
        public string SoldDate { get; set; }
        public int SoldPrice { get; set; }
        public string SoldPriceSource { get; set; }
        public string Url { get; set; }
        public int? Rent { get; set; }
        public double? Floor { get; set; }
        public string ApartmentNumber { get; set; }
    }
}
