using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace WarOnDrug
{
    public class SitePartWorker_WorkSite_Drug : SitePartWorker_WorkSite
    {
        public override IEnumerable<PreceptDef> DisallowedPrecepts => new PreceptDef[]{DefDatabase<PreceptDef>.GetNamed("DrugUse_Prohibited")};

        public override PawnGroupKindDef WorkerGroupKind => PawnGroupKindDefOf.Combat;


        protected override void OnLootChosen(Site site, SitePart sitePart, CampLootThingStruct loot)
        {
            site.customLabel = sitePart.def.label.Formatted(NamedArgumentUtility.Named(loot.thing, "THING"));
        }
    }
}
