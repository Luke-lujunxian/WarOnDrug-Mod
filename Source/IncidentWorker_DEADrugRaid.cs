using RimWorld;

using Verse;

namespace WarOnDrug
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
    }
}
