using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Verse;

namespace WarOnDrug.Quest
{
    public class QuestNode_DrugTradeRequest_GetRequestedDrug : QuestNode_TradeRequest_GetRequestedThing
    {
        private static Dictionary<ThingDef, int> requestCountDict = new Dictionary<ThingDef, int>();
        private static readonly IntRange BaseValueWantedRange = new IntRange(500, 7500);

        new private static readonly SimpleCurve ValueWantedFactorFromWealthCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.6f),
            new CurvePoint(50000f, 2f),
            new CurvePoint(300000f, 4f)
        };

        private static int RandomRequestCount(ThingDef thingDef, Map map)
        {
            Rand.PushState(Find.TickManager.TicksGame ^ thingDef.GetHashCode() ^ 0x343820DB);
            float num = BaseValueWantedRange.RandomInRange;
            Rand.PopState();
            num *= ValueWantedFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal);
            
            return ThingUtility.RoundedResourceStackCount(Mathf.Max(1, Mathf.RoundToInt(num / thingDef.BaseMarketValue)));
        }

        private static bool TryFindRandomRequestedThingDef(Map map, out ThingDef thingDef, out int count, List<ThingDef> dontRequest)
        {
            requestCountDict.Clear();
            Func<ThingDef, bool> globalValidator = delegate (ThingDef td)
            {
/*                if (td.BaseMarketValue / td.BaseMass < 5f)
                {
                    return false;
                }

                if (!td.alwaysHaulable)
                {
                    return false;
                }

                CompProperties_Rottable compProperties = td.GetCompProperties<CompProperties_Rottable>();
                if (compProperties != null && compProperties.daysToRotStart < 10f)
                {
                    return false;
                }

                if (td.ingestible != null && td.ingestible.HumanEdible)
                {
                    return false;
                }

                if (td == ThingDefOf.Silver)
                {
                    return false;
                }

                if (!td.PlayerAcquirable)
                {
                    return false;
                }*/

                int num = RandomRequestCount(td, map);
                requestCountDict.Add(td, num);
                /*                if (!PlayerItemAccessibilityUtility.PossiblyAccessible(td, num, map))
                                {
                                    return false;
                                }

                                if (!PlayerItemAccessibilityUtility.PlayerCanMake(td, map))
                                {
                                    return false;
                                }

                                if (td.thingSetMakerTags != null && td.thingSetMakerTags.Contains("RewardStandardHighFreq"))
                                {
                                    return false;
                                }*/

                return true;
            };
            if (WarOnDrug.DrugList.Keys.TryRandomElement(out thingDef))
            {
                count = RandomRequestCount(thingDef, map);
                requestCountDict.Add(thingDef, count);
                return true;
            }

            count = 0;
            return false;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            if (TryFindRandomRequestedThingDef(slate.Get<Map>("map"), out var thingDef, out var count, dontRequest.GetValue(slate)))
            {
                Faction requester = slate.Get<Faction>("faction");
                WODFactionStatus status = WarEffortManager.GetWarEffortManager.ManagedFactions.TryGetValue(requester.loadID);
                if (status != null)
                {
                    status.dailyInflux += requestCountDict[thingDef] * WarOnDrug.DrugList.TryGetValue(thingDef);
                }
                slate.Set(storeThingAs.GetValue(slate), thingDef);
                slate.Set(storeThingCountAs.GetValue(slate), count*Math.Max(0,2-status.fulfilmentRate));
                slate.Set(storeMarketValueAs.GetValue(slate), thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * (float)count);
                slate.Set(storeHasQualityAs.GetValue(slate), thingDef.HasComp(typeof(CompQuality)));
            }
        }

        protected override bool TestRunInt(Slate slate)
        {
            if (TryFindRandomRequestedThingDef(slate.Get<Map>("map"), out var thingDef, out var count, dontRequest.GetValue(slate)))
            {
                Faction requester = slate.Get<Faction>("faction");
                WODFactionStatus status = WarEffortManager.GetWarEffortManager.ManagedFactions.TryGetValue(requester.loadID);
                if (status != null)
                {
                    if (status.marketSize < 100 || status.fulfilmentRate>1.5)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                slate.Set(storeThingAs.GetValue(slate), thingDef);
                slate.Set(storeThingCountAs.GetValue(slate), count);
                slate.Set(storeMarketValueAs.GetValue(slate), thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * (float)count);
                return true;
            }

            return false;
        }
    }
}
