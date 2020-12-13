using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges
{
    public abstract class VolatilityMeasurer
    {
        public bool IsPriceVolatile { get; protected set; }
        public bool IsMeasuring { get; protected set; }

        public abstract void Measure();
    }
}
