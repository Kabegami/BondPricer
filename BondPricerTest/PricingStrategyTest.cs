using Microsoft.VisualStudio.TestTools.UnitTesting;
using BondPricer;
using System;
using System.Linq;

using System.Collections.Generic;

namespace BondPricerTest
{
    [TestClass]
    public class PricingStrategyTest
    {

        Func<List<Bond>, BondPricer.BondPricer,  List<Tuple<Bond, int>>> strategy = PricingStratey.GreedyStrategy;

        public List<Bond> GenerateBonds(int fastNumber, int semiFastNumber, int longNumber)
        {
            var result = new List<Bond>();

            for (int i = 0; i < fastNumber; i++)
            {
                result.Add(new Bond(BondType.Fast));
            }
            for (int i = 0; i < semiFastNumber; i++)
            {
                result.Add(new Bond(BondType.SemiFast));
            }

            for (int i = 0; i < longNumber; i++)
            {
                result.Add(new Bond(BondType.Long));
            }
            return result;

        }

        [TestMethod]
        public void test_optimal_for_simple_instance()
        {
            IBondPricer local = new NodePricer();
            IBondPricer grid = new GridPricer();
            BondPricer.BondPricer calculator = new BondPricer.BondPricer(local, grid);
            var bonds = new List<Bond> { new Bond(BondType.SemiFast), new Bond(BondType.Long), new Bond(BondType.Fast) };
            var result = strategy(bonds, calculator);
            Assert.AreEqual(13, calculator.ComputingTime);
        }

        [TestMethod]
        public void test_optimal_for_simple_instance2()
        {
            IBondPricer local = new NodePricer();
            IBondPricer grid = new GridPricer();
            BondPricer.BondPricer calculator = new BondPricer.BondPricer(local, grid);
            var bonds = GenerateBonds(4, 2, 0);
            var result = strategy(bonds, calculator);
            Assert.AreEqual(10, calculator.ComputingTime);
        }

        [TestMethod]
        public void test_optimal_for_simple_instance3()
        {
            IBondPricer local = new NodePricer();
            IBondPricer grid = new GridPricer();
            BondPricer.BondPricer calculator = new BondPricer.BondPricer(local, grid);
            var bonds = GenerateBonds(1, 1, 0);
            var result = strategy(bonds, calculator);
            Assert.AreEqual(7, calculator.ComputingTime);
        }

        [TestMethod]
        public void test_all_bond_ids_are_differents()
        {
            var bonds = GenerateBonds(250, 300, 500);
            Assert.IsTrue(bonds.Select(x => x.Id).Distinct().Count() == bonds.Count);

        }

        [TestMethod]
        public void test_grid_should_not_waste_node()
        {
            IBondPricer local = new NodePricer();
            IBondPricer grid = new GridPricer();
            BondPricer.BondPricer calculator = new BondPricer.BondPricer(local, grid);
            var bonds = GenerateBonds(1, 1, 2);
            strategy(bonds, calculator);

            GridPricer castedGrid = (GridPricer)grid;
            Assert.AreEqual(2, castedGrid.NumberOfNodes());
        }

        [TestMethod]
        public void test_should_take_less_than_18_seconds()
        {
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    for (int z = 0; z < 20; z++)
                    {
                        IBondPricer local = new NodePricer();
                        IBondPricer grid = new GridPricer();
                        BondPricer.BondPricer calculator = new BondPricer.BondPricer(local, grid);
                        var bonds = GenerateBonds(4, 2, 0);
                        var result = strategy(bonds, calculator);
                        Assert.AreEqual(10, calculator.ComputingTime);
                    }
                }
            }

        }


        [TestMethod]
        public void test_must_calculate_all_bonds()
        {
            IBondPricer local = new NodePricer();
            IBondPricer grid = new GridPricer();
            BondPricer.BondPricer calculator = new BondPricer.BondPricer(local, grid);
            var bonds = GenerateBonds(4, 2, 6);
            var result = strategy(bonds, calculator);
            foreach(var bond in bonds)
            {
                Assert.AreEqual(bond.Price(), 42);
            }

        }

        [TestMethod]
        public void test_should_raise_exception_if_all_bounds_are_not_calculated()
        {
            IBondPricer local = new NodePricer();
            IBondPricer grid = new GridPricer();
            BondPricer.BondPricer calculator = new BondPricer.BondPricer(local, grid);
            var bonds = GenerateBonds(4, 2, 6);
            Assert.ThrowsException<Exception>(() => bonds[0].Price());
        }


        [TestMethod]
        public void test_simulated_computed_time_should_be_equal_real_time()
        {
            BondPricer.BondPricer simulatedCalculator = new BondPricer.BondPricer(new NodePricer(), new GridPricer());
            BondPricer.BondPricer realTimeCalculator = new BondPricer.BondPricer(new NodePricer() { Simulated = false }, new GridPricer() { Simulated = false });
            realTimeCalculator.Simulation = false;
            var bonds = GenerateBonds(4, 2, 6);
            strategy(bonds, simulatedCalculator);
            strategy(bonds, realTimeCalculator);
            Func<int, int, bool> almostEqual = (x, y) => Math.Abs(x - y) < 0.001;

            Assert.IsTrue(almostEqual(simulatedCalculator.ComputingTime, realTimeCalculator.ComputingTime));


        }

    }
}
