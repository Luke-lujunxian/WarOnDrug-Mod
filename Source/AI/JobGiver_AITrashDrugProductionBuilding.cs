using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;


namespace WarOnDrug.AI
{
    public class JobGiver_AITrashDrugProductionBuilding : JobGiver_AITrashBuildingsDistant
    {

        private static List<Building> tmpTrashableBuildingCandidates = new List<Building>();
        protected override Job TryGiveJob(Pawn pawn)
        {
            List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
            if (allBuildingsColonist.Count == 0)
            {
                return null;
            }

            tmpTrashableBuildingCandidates.Clear();
            foreach (Building item in allBuildingsColonist)
            {
                if (item.def == WodDefOf.DrugLab)
                {
                    tmpTrashableBuildingCandidates.Add(item);
                }
            }
#if DEBUG
            Log.Message("Drug Production Building Count: " + tmpTrashableBuildingCandidates.Count);
#endif

            if (tmpTrashableBuildingCandidates.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < 75; i++)
            {
                Building building = tmpTrashableBuildingCandidates.RandomElement();
                if (!building.IsBurning())
                {
/*                    if (building.InteractionCell.IsValid && pawn.CanReach(building.InteractionCell, PathEndMode.OnCell, Danger.Deadly, mode:TraverseMode.PassDoors))
                    {
                        Job job2 = JobMaker.MakeJob(JobDefOf.Goto, building.InteractionCell, 500, checkOverrideOnExpiry: true);
                        BreachingUtility.FinalizeTrashJob(job2);
                        job2.canBashDoors = true;
                        return job2;
                    }*/

                    Job job = TrashUtility.TrashJob(pawn, building, true);
                    job.canBashDoors = true;
                    job.canUseRangedWeapon = true;

                    if (job != null)
                    {
#if DEBUG
                        Log.Message($"Assigning {pawn}, {job} to trash drug production building");
#endif
                        return job;
                    }
                }
            }

            return null;
        }
    }
}
