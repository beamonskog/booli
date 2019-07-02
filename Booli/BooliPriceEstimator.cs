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

        public async Task<int> GetPriceEstimation(PriceEstimationInputModel inputModel)
        {
            const double AdditionalAreaFactor = 0.3; //Additional area is worth 30% as much as the main area

            // Get Sold objects in area
            var previouslySoldObjectsInArea = await _booliPriceEstimatorRepository.GetSoldObjectsInArea(inputModel.AreaName);

            // Filter
            previouslySoldObjectsInArea = previouslySoldObjectsInArea.Where(l => l.LivingArea > 0 && l.SoldPrice > 0).ToList();

            var estimationModels = previouslySoldObjectsInArea.Select(_ => new PriceEstimationModel(_)).ToList();

            var adjustedSquareMeterPrice = GetSquareMeterPriceBasedOnRent(estimationModels, inputModel);

            var adjustedSquareMeterPrice2 = GetSquareMeterPriceBasedOnLocation(estimationModels, inputModel);

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

            throw new NotImplementedException();
        }

        private int GetSquareMeterPriceBasedOnLocation(List<PriceEstimationModel> estimationModels, PriceEstimationInputModel inputModel)
        {
            const int ThresholdNrOfObjects = 10;
            const int SearchDistanceKm = 3;

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


        }

        private void SetRentRange(List<PriceEstimationModel> recentlySoldNearObjectOfInterest, PriceEstimationInputModel inputModel)
        {
            const double RentPercentageDiffThreshold = 0.2; //If

            var medianRentPrice = GetMedianRentPrice(recentlySoldNearObjectOfInterest);

            // 1. Figure out how much the rent varies, 
            // If it doesn't, return "average"

            //var maxVariation = GetLowestRent

            // 2. Figure out if inputModel is +- 20% of median
            var rentDiff = (double)inputModel.Rent / (double)medianRentPrice;

            if (rentDiff< (1- RentPercentageDiffThreshold)) //20% less than median
            {
                inputModel.RentRange = RentRange.Below;
            }

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

        //private int GetCurrentSquareMeterPriceMedian(List<PriceEstimationModel> estimationModels)
        //{
        //    const int thresholdNrOfObjects = 10;

        //    //Get mean for the last 2 quarters. 
        //    var last2QSoldObjects = GetPreviousTwoQuartersSoldData(estimationModels);
        //    if (last2QSoldObjects.Count < thresholdNrOfObjects)
        //    {
        //        //do something cool
        //    }

        //    //Median value to exclude "spikes"
        //    var medianSquareMeterPrice = GetMedianSoldPriceSquareMeter(last2QSoldObjects);
        //    return (int)medianSquareMeterPrice;
        //}

        //private int GetCurrentRentPriceMedian(List<PriceEstimationModel> estimationModels)
        //{
        //    const int thresholdNrOfObjects = 10;

        //    //Get mean for the last 2 quarters. 
        //    var last2QSoldObjects = GetPreviousTwoQuartersSoldData(estimationModels);
        //    if (last2QSoldObjects.Count < thresholdNrOfObjects)
        //    {
        //        //do something cool
        //    }

        //    //Median value to exclude "spikes"
        //    var medianSquareMeterPrice = GetMedianRentPrice(last2QSoldObjects);
        //    return (int)medianSquareMeterPrice;
        //}

        private int GetMedianRentPrice(List<PriceEstimationModel> last2QSoldObjects)
        {
            var ordered = last2QSoldObjects.OrderBy(_ => _.Rent).ToList();
            var medianSqPrice = ordered.ElementAt((int)Math.Ceiling((double)ordered.Count / 2.0)).Rent;
            return (int)medianSqPrice;
        }

        //private int GetMedianSquareMeterPrice(List<PriceEstimationModel> last2QSoldObjects)
        //{
        //    var ordered = last2QSoldObjects.OrderBy(_ => _.SquareMeterPrice).ToList();
        //    var medianSqPrice = ordered.ElementAt((int)Math.Ceiling((double)ordered.Count / 2.0)).SquareMeterPrice;
        //    return (int)medianSqPrice;
        //}

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
