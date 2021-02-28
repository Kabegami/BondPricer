using System;
using System.Linq;
using System.Collections.Generic;

namespace BondPricer
{
    public class PricingStratey
    {

        public static int timeComparison(Bond b1, Bond b2)
        {
            int time1 = PriceLib.EstimedTime(b1);
            int time2 = PriceLib.EstimedTime(b2);
            return -1 * time1.CompareTo(time2);
        }

        public static List<Tuple<Bond, int>> GreedyStrategy(List<Bond> bonds, BondPricer calculator)
        {
            var sortedBonds = new List<Bond>(bonds);
            sortedBonds.Sort(timeComparison);
            foreach (var bond in sortedBonds)
            {
                if (calculator.Local.EstimedTime(bond) <= calculator.Grid.EstimedTime(bond))
                {
                    calculator.Local.Send(bond);
                }
                else
                {
                    calculator.Grid.Send(bond);
                }
            }

            var task = calculator.Compute();
            task.Wait();
            return bonds.Select(bond => Tuple.Create(bond, bond.Price())).ToList();




        }

    }
}
