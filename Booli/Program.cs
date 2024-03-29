﻿
using Common.Interfaces;
using Common.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BooliPriceEstimator
{
    class Program
    {
        static void Main(string[] args)
        {
            // configure services
            var services = new ServiceCollection()
                .AddTransient<IBooliPriceEstimatorRepository, BooliPriceEstimatorRepository>()
                .AddTransient<IBooliApiHandler, BooliApiHandler>()
                .AddTransient<IBooliPriceEstimator, BooliPriceEstimator>()
                .AddDbContext<BooliPriceEstimatorContext>(_ => _.UseSqlServer(@"Server=localhost;Database=Booli.Kodprov;Trusted_Connection=True;ConnectRetryCount=0"));

            // create a service provider from the service collection
            var serviceProvider = services.BuildServiceProvider();

            // resolve the dependency graph
            //var booliApiHandler = serviceProvider.GetService<IBooliApiHandler>();
            //booliApiHandler.SyncSoldObjectsItteratively().Wait();

            var booliPriceEstimator = serviceProvider.GetService<IBooliPriceEstimator>();

            //booliApiHandler.SyncSoldObjectsItteratively().Wait();
            var estimationInputModel = new PriceEstimationInputModel(74, "Årsta", 5000, StreetNameExamples.Siljansvagen72);
            booliPriceEstimator.GetPriceEstimation(estimationInputModel).Wait();

            
        }

        const int radiusKm = 10;

        public void EstimateApartment(string street, int size, int rent)
        {

        }
    }
}
