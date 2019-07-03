using Common.Interfaces;
using Common.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Common;

namespace BooliPriceEstimator
{
    public class BooliPriceEstimator : IBooliPriceEstimator
    {
        private readonly IBooliPriceEstimatorRepository _booliPriceEstimatorRepository;

        public BooliPriceEstimator(IBooliPriceEstimatorRepository booliPriceEstimatorRepository)
        {
            _booliPriceEstimatorRepository = booliPriceEstimatorRepository;
        }

        public async Task GetPriceEstimation(PriceEstimationInputModel inputModel)
        {
            const double AdditionalAreaFactor = 0.3; //Additional area is worth 30% as much as the main area

            // Get Sold objects in area
            var previouslySoldObjectsInArea = await _booliPriceEstimatorRepository.GetSoldObjectsInArea(inputModel.AreaName);

            // Filter
            previouslySoldObjectsInArea = previouslySoldObjectsInArea.Where(l => l.LivingArea > 0 && l.SoldPrice > 0).ToList();

            var estimationModels = previouslySoldObjectsInArea.Select(_ => new PriceEstimationModel(_)).ToList();

            //var adjustedSquareMeterPrice = GetSquareMeterPriceBasedOnRent(estimationModels, inputModel);

            var adjustedSquareMeterPrice = GetSquareMeterPriceBasedOnLocation(estimationModels, inputModel);

            // Is it close to water?

            // Is it a new building?

            // Is it high up?

            // What direction is the price heading/ trending? Can we get a 1st or 2nd degree coefficient? By only focusing on last 2Q we can ignore this.... 

            // How many objects have been sold? 

            var priceEstimate = adjustedSquareMeterPrice * inputModel.Size;

            if (inputModel.AdditionalArea != 0)
            {
                priceEstimate += (int)Math.Round(inputModel.AdditionalArea * AdditionalAreaFactor);
            }


            Console.WriteLine($"A apartment in {inputModel.AreaName} that is {inputModel.Size} square meters large and with a rent of {inputModel.Rent} SEK/ month is estimated to: ");
            Console.WriteLine(string.Format("{0:#,0}", priceEstimate)+ " SEK");
            Console.ReadLine();
        }

        private int GetSquareMeterPriceBasedOnLocation(List<PriceEstimationModel> estimationModels, PriceEstimationInputModel inputModel)
        {
            const int ThresholdNrOfObjects = 10;
            const int SearchDistanceKm = 1;

            // Maybe do these two itteratively until we find a good number of candidates?
            //1. 
            var now = DateTime.Now;
            var recentlySold = estimationModels.Where(em => (now - em.SoldDate.SoldDateTime).Days < 365).ToList();
            if (recentlySold.Count < ThresholdNrOfObjects)
            {
                throw new Exception($"{recentlySold.Count}, too few sold objects found in the area dating a year back!");
            }

            //2. 
            var recentlySoldNearObjectOfInterest = recentlySold.Where(rs => (double)Calculations.PositionsToMetersDistance(rs.Position, inputModel.Position) / 1000.0 < SearchDistanceKm).ToList();
            if (recentlySoldNearObjectOfInterest.Count < ThresholdNrOfObjects)
            {
                throw new Exception($"{recentlySoldNearObjectOfInterest.Count}, too few sold objects in within a {SearchDistanceKm} km radius found!");
            }

            SetRentRange(recentlySoldNearObjectOfInterest, inputModel);
            var rentFactor = GetRentFactor(inputModel.RentRange);

            var medianSquareMeterPrice = GetMedianSquareMeterPrice(recentlySoldNearObjectOfInterest);

            return (int)(medianSquareMeterPrice * rentFactor);
            //return (int)(medianSquareMeterPrice ); //I found it more accurate if I removed rent factor
        }

        /// <summary>
        /// Higher rent means lower sqm price
        /// </summary>
        /// <param name="rentRange"></param>
        /// <returns></returns>
        private double GetRentFactor(RentRange rentRange)
        {
            const double RentFactor = 0.30; // +- 30% on square meter price depending on if an apartment is below or abov average rent

            switch (rentRange)
            {
                case RentRange.Average:
                    return 1;
                case RentRange.Above:
                    return 1-RentFactor;
                case RentRange.Below:
                    return 1+RentFactor;
                default:
                    throw new Exception($"unexpected renr range type: {rentRange}");
            }
        }

        private void SetRentRange(List<PriceEstimationModel> recentlySoldNearObjectOfInterest, PriceEstimationInputModel inputModel)
        {
            const double RentPercentageDiffThreshold = 0.2;// +- 20% threshold of median rent

            var medianRentPrice = GetMedianRentPrice(recentlySoldNearObjectOfInterest);

            //var maxVariation = GetLowestRent

            // 2. Figure out if inputModel is +- 20% of median
            var rentDiff = (double)inputModel.Rent / (double)medianRentPrice;

            // rent diff is less than 80% of median
            if (rentDiff < (1 - RentPercentageDiffThreshold)) //20% less than median
            {
                inputModel.RentRange = RentRange.Below;
            }

            // rent diff is more than 120% of median
            else if (rentDiff > (1 + RentPercentageDiffThreshold)) //20% less than median
            {
                inputModel.RentRange = RentRange.Above;
            }

            else
            {
                inputModel.RentRange = RentRange.Average;
            }
        }

        //private int GetRecentlySoldObjects(List<PriceEstimationModel> estimationModels)
        //{
        //    var now = DateTime.Now;
        //    var recentlySold = estimationModels.Where(em => (now - em.SoldDate.SoldDateTime).Days < 365).ToList();
        //    return recentlySold;
        //}

        private int GetSquareMeterPriceBasedOnRent(List<PriceEstimationModel> estimationModels, PriceEstimationInputModel inputModel)
        {
            const int thresholdNrOfObjects = 10;

            //Get mean for the last 2 quarters. 
            var last2QSoldObjects = GetPreviousTwoQuartersSoldData(estimationModels);
            if (last2QSoldObjects.Count < thresholdNrOfObjects)
            {
                //do something cool
            }

            //Calculations.CoordinatesToMeters

            // Is the rent high for the area? 
            //var lowestNonZeroRentModel = last2QSoldObjects.Where(_ => _.Rent != 0).OrderBy(_ => _.Rent).First();
            //var highestNonZeroRentModel = last2QSoldObjects.Where(_ => _.Rent != 0).OrderByDescending(_ => _.Rent).First();

            // Where on the scale is the subject?
            var mostSimilarRentSubject = GetSoldObjectWithSimilarRent(last2QSoldObjects, inputModel);

            var baseRent = mostSimilarRentSubject.Rent;

            return (int)baseRent;
        }

        private PriceEstimationModel GetSoldObjectWithSimilarRent(List<PriceEstimationModel> last2QSoldObjects, PriceEstimationInputModel inputModel)
        {
            var soldModelsOrderedByRent = last2QSoldObjects.OrderBy(_ => _.Rent).ToList();

            var previousSoldModel = soldModelsOrderedByRent[0];
            for (int i = 1; i < soldModelsOrderedByRent.Count; i++)
            {
                var currentSoldModel = soldModelsOrderedByRent[i];
                if (currentSoldModel.Rent > inputModel.Rent)
                {
                    var nearestSoldModelByRent = GetNearestSoldModel(previousSoldModel, currentSoldModel, inputModel.Rent);
                    return nearestSoldModelByRent;
                }
            }

            return soldModelsOrderedByRent.Last();
        }

        private PriceEstimationModel GetNearestSoldModel(PriceEstimationModel modelA, PriceEstimationModel modelB, int rent)
        {
            var priceDiffA = Math.Abs(rent - (int)modelA.Rent);
            var priceDiffB = Math.Abs(rent - (int)modelB.Rent);

            return priceDiffA < priceDiffB ? modelA : modelB;
        }
        
        private int GetMedianRentPrice(List<PriceEstimationModel> estimationModels)
        {
            var ordered = estimationModels.OrderBy(_ => _.Rent).ToList();
            var medianSqPrice = ordered.ElementAt((int)Math.Ceiling((double)ordered.Count / 2.0)).Rent;
            return (int)medianSqPrice;
        }

        private int GetMedianSquareMeterPrice(List<PriceEstimationModel> estimationModels)
        {
            var ordered = estimationModels.OrderBy(_ => _.SquareMeterPrice).ToList();
            var medianSqPrice = ordered.ElementAt((int)Math.Ceiling((double)ordered.Count / 2.0)).SquareMeterPrice;
            return (int)medianSqPrice;
        }

        private List<PriceEstimationModel> GetPreviousTwoQuartersSoldData(List<PriceEstimationModel> estimationModels)
        {
            var now = DateTime.Now;
            var currentSoldDate = new SoldDate(now);

            switch (currentSoldDate.YearQuarter)
            {
                case "Q1"://This one's special. 
                    return estimationModels.Where(em =>
                    (em.SoldDate.Year == currentSoldDate.Year && em.SoldDate.YearQuarter == "Q1") ||
                    (em.SoldDate.Year == currentSoldDate.Year - 1 && em.SoldDate.YearQuarter == "Q4")
                    ).ToList();
                case "Q2":
                    return estimationModels.Where(em => em.SoldDate.Year == currentSoldDate.Year
                        && (em.SoldDate.YearQuarter == "Q2" || em.SoldDate.YearQuarter == "Q1")).ToList();
                case "Q3":
                    return estimationModels.Where(em => em.SoldDate.Year == currentSoldDate.Year
                        && (em.SoldDate.YearQuarter == "Q3" || em.SoldDate.YearQuarter == "Q2")).ToList();
                case "Q4":
                    return estimationModels.Where(em => em.SoldDate.Year == currentSoldDate.Year
                        && (em.SoldDate.YearQuarter == "Q4" || em.SoldDate.YearQuarter == "Q3")).ToList();

                default:
                    throw new Exception("{currentSoldDate.YearQuarter} is not a valid year-quarter! ");

            }
        }
    }
}
