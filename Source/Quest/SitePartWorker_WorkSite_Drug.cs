using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace WarOnDrug.Quest
{
    public class SitePartWorker_WorkSite_Drug : SitePartWorker_WorkSite
    {
        public override IEnumerable<PreceptDef> DisallowedPrecepts => new PreceptDef[] { DefDatabase<PreceptDef>.GetNamed("DrugUse_Prohibited") };

        public override PawnGroupKindDef WorkerGroupKind => PawnGroupKindDefOf.Combat;

        public static new readonly SimpleCurve PointsMarketValue = new SimpleCurve
        {
            new CurvePoint(100f, 400f),
            new CurvePoint(250f, 850f),
            new CurvePoint(800f, 4000f),
            new CurvePoint(10000f, 10000f)
        };


        protected override void OnLootChosen(Site site, SitePart sitePart, CampLootThingStruct loot)
        {
            site.customLabel = sitePart.def.label.Formatted(NamedArgumentUtility.Named(loot.thing, "THING"));
        }
    }
}
