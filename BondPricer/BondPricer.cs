using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace BondPricer
{   
    public interface IBondPricer
    {
        public Task Compute();
        public int ComputingTime { get; }
        public void Send(Bond bond);
        public int EstimedTime(Bond bond);
    }

    public class NodePricer : IBondPricer
    {
        protected Queue<Bond> bonds = new Queue<Bond>();
        private int computingTime = 0;
        public int ComputingTime { get { return computingTime; } }
        //When simulated is true, we don't sleep for the computation thus it is faster to tests features and run tests
        public bool Simulated { get; set; } = true;

        public Task Compute()
        {
            if (Simulated)
            {
                return Task.Run(() =>
                {
                    foreach (var bond in bonds)
                    {
                        bond.OnResultComputed(PriceLib.ComputePrice(bond), null);
                        computingTime += PriceLib.EstimedTime(bond);
                    }
                });
            }
            else
            {
                return Task.Run(() =>
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    foreach (var bond in bonds)
                    {
                        Thread.Sleep(PriceLib.EstimedTime(bond) * 1000);
                        bond.OnResultComputed(PriceLib.ComputePrice(bond), null);
                    }
                    watch.Stop();
                    computingTime += (int)watch.ElapsedMilliseconds / 1000;
                });
            }

        }
        
        public void Send(Bond bond)
        {
            bonds.Enqueue(bond);
        }

        public int EstimedTime(Bond bond)
        {
            return bonds.Select(PriceLib.EstimedTime).Sum() + PriceLib.EstimedTime(bond);
        }
    }


    public class GridPricer : IBondPricer
    {
        protected List<NodePricer> nodes = new List<NodePricer>();
        public bool Simulated { get; set; } = true;

        public int NumberOfNodes()
        {
            return nodes.Count;
        }

        public int ComputingTime {get {
                if (nodes.Count == 0)
                    return 0;
                return nodes.Select(node => node.ComputingTime).Max() + 8; 
            } 
        }

        public Task Compute()
        {
            return Task.Run(() =>
            {
                Task[] tasks = new Task[nodes.Count];
                for (int i = 0; i < nodes.Count; i++)
                {
                    tasks[i] = nodes[i].Compute();
                }
                Task.WaitAll(tasks);
            });

        }

        public int EstimedTime(Bond bond)
        {
            if (GetAvailableNodeIndex(bond) != -1)
            {
                return 0;
            }
            return PriceLib.EstimedTime(bond) + 8;
        }

        public int GetAvailableNodeIndex(Bond bond)
        {
            /* Check if we can add the bond to a node without increasing the computation time.
             Return the index of the node or -1 if we can't find one */
            if (nodes.Count == 0)
                return -1;
            var longestComputation = nodes.Max(node => node.EstimedTime(bond)) - PriceLib.EstimedTime(bond);
            int index = 0;
            foreach(var node in nodes)
            {
                if (node.EstimedTime(bond) <= longestComputation)
                    return index;
                index++;
            }
            return -1;

        }

        public void Send(Bond bond)
        {
            int index = GetAvailableNodeIndex(bond);
            if (index != -1)
            {
                nodes[index].Send(bond);
            }
            else
            {
                var node = new NodePricer() { Simulated = Simulated };
                node.Send(bond);
                nodes.Add(node);
            }
        }
    }



    public class BondPricer : IBondPricer
    {
        public IBondPricer Local { get; }
        public IBondPricer Grid { get; }

        public bool Simulation { get; set; } = true;

        public int ComputingTime { get { return Math.Max(Local.ComputingTime, Grid.ComputingTime); } }

        public BondPricer(IBondPricer local, IBondPricer grid)
        {
            Local = local;
            Grid = grid;
        }

        public Task Compute()
        {
            return Task.Run(() =>
            {
                var t1 = Local.Compute();
                var t2 = Grid.Compute();
                t1.Wait();
                t2.Wait();
            });

        }

        public void Send(Bond bond)
        {
            if (Local.EstimedTime(bond) <= Grid.EstimedTime(bond))
            {
                Local.Send(bond);
            }
            else
            {
                Grid.Send(bond);

            }
        }

        public int EstimedTime(Bond bond)
        {
            return Math.Max(Local.EstimedTime(bond), Grid.EstimedTime(bond));
        }
    }


}
