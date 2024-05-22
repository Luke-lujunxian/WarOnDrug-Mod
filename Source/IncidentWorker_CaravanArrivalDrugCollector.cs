using RimWorld;
using Verse;

namespace WarOnDrug
{
    public class IncidentWorker_CaravanArrivalDrugCollector: IncidentWorker_TraderCaravanArrival
    {
        public const string DrugCollectorTraderKindCategory = "DrugCollector";

        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            if (base.FactionCanBeGroupSource(f, map, desperate) )
            {
                WarEffortManager manager = WarEffortManager.GetWarEffortManager;

                if(manager.ManagedFactions.TryGetValue(f.loadID, out var wODFactionStatus))
                {
                    return wODFactionStatus.marketSize > 100; //Only faction with large enought size
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        protected override bool TryResolveParmsGeneral(IncidentParms parms)
        {
            if (!base.TryResolveParmsGeneral(parms))
            {
                return false;
            }

/*            if (parms.traderKind == null)
            {
                Map map = (Map)parms.target;
                if (!parms.faction.def.caravanTraderKinds.TryRandomElementByWeight((TraderKindDef traderDef) => TraderKindCommonality(traderDef, map, parms.faction), out parms.traderKind))
                {
                    return false;
                }
            }*/

            parms.traderKind = DefDatabase<TraderKindDef>.GetNamed("WOD_Caravan_DrugCollector");
/*            WarEffortManager manager = WarEffortManager.GetWarEffortManager;
            manager.ManagedFactions.TryGetValue(parms.faction.loadID, out var wODFactionStatus);
            foreach (StockGenerator Stockgen in parms.traderKind.stockGenerators)
            {
                var customCountRanges = Stockgen.customCountRanges;
                if (customCountRanges != null)
                {
                    for (int i = 0; i < customCountRanges.Count; i++)
                    {
                        if (customCountRanges[i].thingDef == ThingDefOf.Silver)
                        {
                            customCountRanges[i].countRange.min = wODFactionStatus
                            break;
                        }
                    }
                }
            }*/

            return true;
        }
    }
}
