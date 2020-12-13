using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges
{
    using Technician;

    public class IndexStochasticMeasurer : VolatilityMeasurer
    {
        //
        public double StochTreshold { get; set; }

        // stochastic over the volatility index
        public MovingStochastic Stoch { get; set; }

        public override void Measure()
        {
            IsPriceVolatile = Stoch.Get() >= StochTreshold;
        }
    }
}
