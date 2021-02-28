using System;
using System.Collections.Generic;

namespace BondPricer
{

    class Program
    {

        static void Main(string[] args)
        {
            var b1 = new Bond(BondType.Fast);
            var b2 = new Bond(BondType.Long);
            var b3 = new Bond(BondType.SemiFast);

            var bonds = new List<Bond> { b1, b2, b3 };

            var local = new NodePricer();
            var grid = new GridPricer();

            var calculator = new BondPricer(local, grid);
            b1.ResultComputed += (obj, e) => { Console.WriteLine("Price available for : " + obj.ToString()); };

            var result = PricingStratey.GreedyStrategy(bonds, calculator);
            // shoud be equal 13
            Console.WriteLine(calculator.ComputingTime);
        }

    }
}
