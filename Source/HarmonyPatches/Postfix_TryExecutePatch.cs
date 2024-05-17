using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WarOnDrug.HarmonyPatches
{
    [HarmonyPatch]

    public static class Postfix_TryExecutePatch
    {
        //static ThingDef Yayo = DefDatabase<ThingDef>.GetNamed("Yayo");
        [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
        public static void Postfix(TradeDeal __instance, bool actuallyTraded, Dictionary<ThingDef, int> __state)
        {
            //Log.Message(Yayo);
            Log.Message("Trying Trade");
            if (actuallyTraded)
            {
                Log.Message("Traded");
                Dictionary<ThingDef, int> transaction = __state;

                foreach (var drugKV in WarOnDrug.DrugList)
                {
                    var drug = drugKV.Key;

                    if (transaction.GetValueSafe(drug) != 0)
                    {
                        int count = transaction.GetValueSafe(drug);
                        Find.World.GetComponent<WarEffortManager>().ManagedFactions.TryGetValue(TradeSession.trader.Faction.loadID, out var factionStatus);
                        if (factionStatus is null) return;//not active faction
#if DEBUG
                        Log.Message(string.Format("Selling {0}x {1} to {2}}", count, drug.defName));
#endif
                        //factionStatus.corruption += 0.001f * count;
                        //factionStatus.dailyInflux = drugKV.Value * count;

                        if (TradeSession.trader.TraderKind == DefDatabase<TraderKindDef>.GetNamed("CaravanArrivalTributeCollector"))//Collector will share influx with others
                        {
                            foreach (var faction in Find.World.GetComponent<WarEffortManager>().ManagedFactions)
                            {
                                if (faction.Key == TradeSession.trader.Faction.loadID) continue;
                                faction.Value.dailyInflux += drugKV.Value * (count/2) / Find.World.GetComponent<WarEffortManager>().ManagedFactions.Count;
                            }
                            factionStatus.dailyInflux = drugKV.Value * (count/2);
                        }
                        else
                        {
                            factionStatus.dailyInflux = drugKV.Value * count;
                        }

                    }

                    /*                    if (WarOnDrug.VTE)//Counter the VTE price flucuation and create demand(todo)
                                        {
                                            TradingManager VTEmanager = Current.Game.GetComponent<TradingManager>();
                                            foreach (var item in transaction)
                                            {

                                                if (!Utils.tradeableItemsToIgnore.Contains(item.Key))
                                                {
                                                    if (VTEmanager.thingsAffectedBySoldPurchasedMarketValue.ContainsKey(item.Key))
                                                    {
                                                        VTEmanager.thingsAffectedBySoldPurchasedMarketValue[item.Key] -= item.Key.GetStatValueAbstract(StatDefOf.MarketValue) * (float)item.Value;
                                                    }
                                                    else
                                                    {
                                                        VTEmanager.thingsAffectedBySoldPurchasedMarketValue[item.Key] = (0f - item.Key.GetStatValueAbstract(StatDefOf.MarketValue)) * (float)item.Value;
                                                    }
                                                }
                                            }

                                        }*/
                }


            }
        }
        [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
        public static void Prefix(TradeDeal __instance, out Dictionary<ThingDef, int> __state)
        {
            __state = new Dictionary<ThingDef, int>();
            List<Tradeable> tradeables = __instance.AllTradeables;
            foreach (Tradeable tradeable in tradeables)
            {
                //Log.Message(tradeable);
                if (tradeable.ActionToDo == TradeAction.PlayerSells)
                {
                    Log.Message(tradeable.ThingDef);
                    __state.Add(tradeable.ThingDef, tradeable.CountToTransferToDestination);
                }
            }
        }
    }
}
