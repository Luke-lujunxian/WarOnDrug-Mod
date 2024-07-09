using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WarOnDrug.Incident
{
    internal class IncidentWorker_DEADrugRaid: IncidentWorker_RaidEnemy
    {
        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {

            if (base.FactionCanBeGroupSource(f, map, desperate) || !f.HostileTo(Faction.OfPlayer))
            {
                return f.def.defName == "RIM_DEA";
            }
            else
            {
                return false;
            }
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            base.ResolveRaidStrategy(parms, groupKind);

            
            //All center drop?
            if (!parms.raidStrategy.defName.Contains("Siege") && (parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeWalkIn))
            {
                parms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
            }
        }
    }

}
