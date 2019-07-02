using Booli;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Models.NativeBooli;
using Common.Models;
using Common.Interfaces;
using System;
using System.Diagnostics;

namespace BooliPriceEstimator
{
    public class BooliApiHandler : IBooliApiHandler
    {
        private readonly IBooliPriceEstimatorRepository _booliPriceEstimatorRepository;

        public BooliApiHandler(IBooliPriceEstimatorRepository booliPriceEstimatorRepository)
        {
            _booliPriceEstimatorRepository = booliPriceEstimatorRepository;
        }

        //public async Task SyncSoldObjects()
        //{
        //    var booliApi = new BooliApi();


        //    var nativeResults = await booliApi.GetNativeSoldObjectsInArea("stockholm", 5);
        //    var mappedResults = MappModels(nativeResults);

        //    await _booliPriceEstimatorRepository.SyncSoldObjects(mappedResults);
        //}

        public async Task SyncSoldObjectsItteratively()
        {
            var area = "stockholm";
            var yearSpan = 10;
            var booliApi = new BooliApi();

            var numberOfPages = await booliApi.GetNumberOfPages(area, yearSpan);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < numberOfPages; i++)
            {
#if DEBUG
                Console.Clear();
                Console.WriteLine($"handling page {i}/{numberOfPages}");
#endif
                var nativeResults = await booliApi.GetNativeSoldObjectsInArea(area, yearSpan, i);
                var mappedResults = MappModels(nativeResults);

                await _booliPriceEstimatorRepository.SyncSoldObjects(mappedResults);
            }

            stopWatch.Stop();
            Console.WriteLine($"sync-time: {Math.Round((double)stopWatch.ElapsedMilliseconds / (double)1000)} seconds.");
        }

        private List<SoldObject> MappModels(List<Sold> nativeSoldObjects)
        {
            var mappedResults = new List<SoldObject>();

            foreach (var nativeSoldObject in nativeSoldObjects)
            {
                try
                {
                    var mappedModel = MappModel(nativeSoldObject);
                    mappedResults.Add(mappedModel);
                }
                catch (Exception e)
                {
                    Console.WriteLine("exception when mapping: " + e.Message);
                }
            }

            return mappedResults;
        }

        private SoldObject MappModel(Sold soldObject)
        {
            var namedAreas = new List<NamedArea>();
            foreach (var namedArea in soldObject.Location.NamedAreas)
            {
                namedAreas.Add(new NamedArea { Area = namedArea });
            }

            var location = new Common.Models.Location
            {
                NamedAreas = namedAreas,
                Position = new Common.Models.Position(soldObject.Location.Position.Latitude, soldObject.Location.Position.Longitude)
            };

            var mappedModel = new SoldObject
            {
                BooliId = soldObject.BooliId,
                AdditionalArea = soldObject.AdditionalArea,
                Floor = soldObject.Floor,
                LivingArea = soldObject.LivingArea,
                Location = location,
                ObjectType = soldObject.ObjectType,
                PlotArea = soldObject.PlotArea,
                Rent = soldObject.Rent,
                Rooms = soldObject.Rooms,
                SoldPrice = soldObject.SoldPrice,
                SoldDate = soldObject.SoldDate
            };

            return mappedModel;
        }
    }
}
