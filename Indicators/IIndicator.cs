using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Indicators
{
    public interface IIndicator
    {
        void Update(Prices prices);
    }
}
