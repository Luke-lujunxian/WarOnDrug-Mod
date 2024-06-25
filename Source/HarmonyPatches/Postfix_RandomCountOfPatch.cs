using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WarOnDrug.HarmonyPatches
{
    [HarmonyPatch(typeof(StockGenerator), "RandomCountOf")]
    public static class Postfix_RandomCountOfPatch
    {
        static void Postfix(ref int __result, StockGenerator __instance, ThingDef def)
        {

            if (__instance.trader == WodDefOf.WOD_Caravan_DrugCollector || __instance.trader == WodDefOf.WOD_Caravan_DEA_DrugDestoryer)
            {
                if (def == ThingDefOf.Silver)
                {
#if DEBUG
                    Log.Message(string.Format("StockGenerator: {0} - {1}", __instance.trader.defName, def.defName));
#endif
                    var factions = Find.World.GetComponent<WarEffortManager>().ManagedFactions;
                    float totalMarketSize = 0;

                    foreach (KeyValuePair<int, WODFactionStatus> faction in factions)
                    {
                        totalMarketSize += faction.Value.marketSize;
                    }

                    float totalVal = (WarOnDrug.DrugList.Values.Count) - (WarOnDrug.DrugList.Values).Sum();


                    foreach (var item in WarOnDrug.DrugList)
                    {
                        __result += (int)(item.Key.GetStatValueAbstract(StatDefOf.MarketValue) * totalMarketSize * ((1 - item.Value) / totalVal));
                    }
#if DEBUG
                    Log.Message(string.Format("Adjusting {0} to {1}", def.defName, __result));
#endif
                }
            }
        }
    }
}
