using System;
using System.Collections.Generic;

namespace Common.Models
{
    public class PriceEstimationModel
    {
        public PriceEstimationModel(SoldObject soldObject)
        {
            try
            {
                LivingArea = soldObject.LivingArea;
                Rent = (int?)soldObject.Rent;
                SoldPrice = soldObject.SoldPrice;
                SoldDate = GetSoldDate(soldObject.SoldDate);
                Position = new Position(soldObject.Location.Position.Latitude, soldObject.Location.Position.Latitude);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public PriceEstimationModel(int livingArea, int rent, int soldPrice, string soldDate)
        {
            LivingArea = livingArea;
            Rent = rent;
            SoldPrice = soldPrice;
            SoldDate = GetSoldDate(soldDate);
        }

        private SoldDate GetSoldDate(string soldDate)
        {
            if (soldDate.Length != 10)
            {
                throw new Exception($"Unexpected sold date string '{soldDate}'");
            }
            var year = int.Parse(soldDate.Substring(0, 4));
            var month = int.Parse(soldDate.Substring(5, 2));
            var day = int.Parse(soldDate.Substring(8, 2));
            //var soldQuarter = GetQuarter(soldDate);
            var soldDateTime = new DateTime(year, month, day);
            return new SoldDate(soldDateTime);
        }

        public Position Position { get; set; }
        public double LivingArea { get; set; }
        public int AdditionalArea { get; set; }
        public int PlotArea { get; set; }
        public SoldDate SoldDate { get; set; }
        public int SoldPrice { get; set; }
        public int? Rent { get; set; }
        public double SquareMeterPrice => (int)Math.Round((double)SoldPrice / (double)LivingArea);
    }

    public class SoldDate
    {
        public SoldDate()
        {

        }

        public SoldDate(DateTime dateTime)
        {
            SoldDateTime = dateTime;
            YearQuarter = GetQuarter(dateTime);
            Year = dateTime.Year;
        }

        public DateTime SoldDateTime { get; set; }
        public int Year { get; set; }
        public string YearQuarter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputDate">YYYY-MM-DD</param>
        /// <returns></returns>
        private string GetQuarter(DateTime inputDate)
        {
            //var monthString = inputDate.Substring(5, 2);
            //var monthNumber = int.Parse(monthString);
            if (inputDate.Month > 0 && inputDate.Month < 4)
            {
                return "Q1";
            }
            else if (inputDate.Month < 7)
            {
                return "Q2";
            }
            else if (inputDate.Month < 10)
            {
                return "Q3";
            }
            else if (inputDate.Month > 9 && inputDate.Month < 13)
            {
                return "Q4";
            }
            else throw new Exception($"Invalid month {inputDate.Month}");
        }
    }
}
