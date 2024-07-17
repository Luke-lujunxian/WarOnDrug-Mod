using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WarOnDrug.HarmonyPatches
{
    [HarmonyPatch]

    public static class Postfix_TryExecutePatch
    {
        [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
        public static void Postfix(TradeDeal __instance, bool actuallyTraded, Dictionary<ThingDef, int> __state)
        {
            if (TradeSession.trader.TraderKind.orbital)
            {
                return; //ignore obital traders
            }
            WarEffortManager warEffortManager = Find.World.GetComponent<WarEffortManager>();

            //Log.Message(Yayo);
#if DEBUG
            Log.Message("Trying Trade");
#endif
            if (actuallyTraded)
            {
#if DEBUG
                Log.Message("Traded");
#endif
                Dictionary<ThingDef, int> transaction = __state;

                foreach (var drugKV in WarOnDrug.DrugList)
                {
                    var drug = drugKV.Key;
#if DEBUG
                    Log.Message($"{drug}");
#endif


                    if (transaction.TryGetValue(drug, -114514) != -114514)
                    {
                        int count = transaction.GetValueSafe(drug);
                        warEffortManager.ManagedFactions.TryGetValue(TradeSession.trader.Faction.loadID, out var factionStatus);
                        if (factionStatus is null) return;//not active faction
#if DEBUG
                        Log.Message(string.Format("Selling {0}x {1} to {2}", count, drug.defName, TradeSession.trader.Faction.Name));
#endif

                        if (TradeSession.trader.TraderKind == WodDefOf.WOD_Caravan_DrugCollector)//Collector will share influx with others
                        {
                            foreach (var faction in Find.World.GetComponent<WarEffortManager>().ManagedFactions)
                            {
                                if (faction.Key == TradeSession.trader.Faction.loadID) continue;
                                faction.Value.dailyInflux += drugKV.Value * (count/2) / Find.World.GetComponent<WarEffortManager>().ManagedFactions.Count;
                            }
                            factionStatus.dailyInflux = drugKV.Value * (count/2);
                        }else if (TradeSession.trader.TraderKind == WodDefOf.WOD_Caravan_DEA_DrugDestoryer)
                        {
                            //Will not contribute to market size
                        }
                        else
                        {
                            factionStatus.dailyInflux = drugKV.Value * count;
                        }
                    }
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
#if DEBUG
                    Log.Message(tradeable.ThingDef);
#endif
                    if (WarOnDrug.DrugList.Keys.Contains(tradeable.ThingDef))
                    {
                        __state.SetOrAdd(tradeable.ThingDef, tradeable.CountToTransferToDestination);

                    }
                }
            }
        }
    }
}
