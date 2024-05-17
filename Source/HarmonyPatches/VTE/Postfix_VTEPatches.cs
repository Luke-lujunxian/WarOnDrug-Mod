using HarmonyLib;
using RimWorld;
using System.Linq;
using VanillaTradingExpanded;
using Verse;

namespace WarOnDrug.HarmonyPatches.VTE
{
    [HarmonyPatchCategory("VTE")]
    public static class Postfix_VTEPatches
    {
        public static readonly string WOD_SPECIAL_MARK = "*WOD SUPPLY DEMAND*";
        public static void Postfix(ref float __result, Contract __instance)
        {
            if (__instance.Name.Contains(WOD_SPECIAL_MARK))
            {
                __result = 0;
            }
        }
    }
    [HarmonyPatchCategory("VTE")]
    public static class Postfix_RegisterSoldThingPatch
    {

        public static void Prefix(TradingManager __instance)
        {
            WarEffortManager manager = Find.World.GetComponent<WarEffortManager>();
            TradingManager VTEmanager = __instance;
            //The total (1 - value of registered drug)
            float totalVal = (WarOnDrug.DrugList.Values.Count) - (WarOnDrug.DrugList.Values).Sum();
            foreach (var item in WarOnDrug.DrugList)
            {
                if (!Utils.tradeableItemsToIgnore.Contains(item.Key))
                {
                    if (VTEmanager.thingsAffectedBySoldPurchasedMarketValue.ContainsKey(item.Key))
                    {
                        //Less lasting drug is conumed more, so bought more. ......Right? idk, really. This dosen't make a lot of sense
                        VTEmanager.thingsAffectedBySoldPurchasedMarketValue[item.Key] -= item.Key.GetStatValueAbstract(StatDefOf.MarketValue) * manager.totalMarketSize * ((1 - item.Value) / totalVal);
                    }
                    else
                    {
                        VTEmanager.thingsAffectedBySoldPurchasedMarketValue[item.Key] = (0f - item.Key.GetStatValueAbstract(StatDefOf.MarketValue)) * manager.totalMarketSize * ((1 - item.Value) / totalVal);
                    }
                }
            }
            manager.totalMarketSize = 0;
        }
    }
}
