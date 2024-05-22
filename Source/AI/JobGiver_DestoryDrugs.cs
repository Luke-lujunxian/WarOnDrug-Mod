
using System.Linq;
using Verse.AI;
using RimWorld;
using Verse;

namespace WarOnDrug.AI
{
    internal class JobGiver_DestoryDrugs : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver).Any((Thing t) => t.def.IsDrug))
            {
                Thing thing = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver).Where((Thing t) => t.def.IsDrug).RandomElement();
                return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("WOD_DestroyDrugs"), thing);
            }
            return null;
        }
    }
}
