using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarOnDrug
{
    internal class StockGenerator_BuyTradeTag_Expensive: StockGenerator_BuyTradeTag
    {
        public new PriceType price = PriceType.Expensive;

        public new bool TryGetPriceType(ThingDef thingDef, TradeAction action, out PriceType priceType)
        {
                priceType = PriceType.Expensive;
            return true;
        }
    }
}
