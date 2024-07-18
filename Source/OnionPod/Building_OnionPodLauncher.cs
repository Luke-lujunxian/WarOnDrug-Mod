using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WarOnDrug.OnionPod
{
    public class Building_OnionPodLauncher : Building_PodLauncher
    {
        public Building_OnionPodLauncher() : base() { 
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            AcceptanceReport acceptanceReport = GenConstruct.CanPlaceBlueprintAt(ThingDefOf.TransportPod, FuelingPortUtility.GetFuelingPortCell(this), ThingDefOf.TransportPod.defaultPlacingRot, base.Map);
            Designator_Build designator_Build = BuildCopyCommandUtility.FindAllowedDesignator(WodDefOf.OnionTransportPod);
            if (designator_Build != null)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "BuildThing".Translate(WodDefOf.OnionTransportPod.label);
                command_Action.icon = designator_Build.icon;
                command_Action.defaultDesc = designator_Build.Desc;
                command_Action.action = delegate
                {
                    IntVec3 fuelingPortCell = FuelingPortUtility.GetFuelingPortCell(this);
                    GenConstruct.PlaceBlueprintForBuild(WodDefOf.OnionTransportPod, fuelingPortCell, base.Map, WodDefOf.OnionTransportPod.defaultPlacingRot, Faction.OfPlayer, null);
                };
                if (!acceptanceReport.Accepted)
                {
                    command_Action.Disable(acceptanceReport.Reason);
                }

                yield return command_Action;
            }
        }
    }
}
