using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarOnDrug.Incident
{
    internal class IncidentWorker_CaravanArrivalDrugDestoryer: IncidentWorker_TraderCaravanArrival
    {
        public const string DrugCollectorTraderKindCategory = "DrugCollector";

        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            return f.def.defName == "RIM_DEA";
        }

        protected override bool TryResolveParmsGeneral(IncidentParms parms)
        {
            if (!base.TryResolveParmsGeneral(parms))
            {
                return false;
            }

            parms.traderKind = WodDefOf.WOD_Caravan_DEA_DrugDestoryer;


            return true;
        }
    }
}
