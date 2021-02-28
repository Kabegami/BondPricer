using System;

namespace BondPricer
{
    public enum BondType
    {
        Fast,
        SemiFast,
        Long,
    }

    public class Bond
    {

        public Guid Id { get; }
        public BondType Type { get; }

        /* We don't want to wait that all the bond are calculated to get back our price.
        * Thus we store the price inside the bond object 
        * and use an event handler to display price on gui or do other stuff when it is avaible
        */
        private bool isPriceAvailable;
        private int price;

        public event EventHandler ResultComputed;

        public virtual void OnResultComputed(int price, EventArgs e)
        {
            this.price = price;
            this.isPriceAvailable = true;
            ResultComputed?.Invoke(this, e);
        }

        public Bond(BondType type)
        {
            Id = Guid.NewGuid();
            Type = type;
            isPriceAvailable = false;

        }

        public override String ToString()
        {
            String template = "Bond[{0};{1}]";
            return String.Format(template, Id, Type.ToString());
        }

        public int Price()
        {
            if (!isPriceAvailable)
            {
                throw new Exception("The bond doesn't have a price yet, please use the Compute Method of a Bond Pricer !");
            }
            return price;
        }

    }

    public class PriceLib
    {
        public static int ComputePrice(Bond bond)
        {
            int price = 42;
            return price;
        }

        public static int EstimedTime(Bond bond)
        {
            int time = 0;
            switch (bond.Type)
            {
                case BondType.Fast:
                    time = 2;
                    break;
                case BondType.SemiFast:
                    time = 5;
                    break;
                case BondType.Long:
                    time = 10;
                    break;
            }
            return time;
        }
    }

}
