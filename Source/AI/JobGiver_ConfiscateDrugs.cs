using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse;
using System.Collections.Generic;
using System;

namespace WarOnDrug.AI
{
    internal class JobGiver_ConfiscateDrugs: ThinkNode_JobGiver
    {
        public const float ItemsSearchRadiusInitial = 7f;

        private const float ItemsSearchRadiusOngoing = 12f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!RCellFinder.TryFindBestExitSpot(pawn, out var spot))
            {
                return null;
            }

            if (TryFindDrugToTake(pawn.Position, pawn.Map, 12f, out var item, pawn) && !GenAI.InDangerousCombat(pawn))
            {
                Job job = JobMaker.MakeJob(JobDefOf.Steal);
                job.targetA = item;
                job.targetB = spot;
                job.count = Mathf.Min(item.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity) / item.def.VolumePerUnit));
                return job;
            }

            return null;
        }

        public static bool TryFindDrugToTake(IntVec3 root, Map map, float maxDist, out Thing item, Pawn thief, List<Thing> disallowed = null)
        {
            if (map == null)
            {
                item = null;
                return false;
            }

            if (thief != null && !thief.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                item = null;
                return false;
            }

            if ((thief != null && !map.reachability.CanReachMapEdge(thief.Position, TraverseParms.For(thief, Danger.Some))) || (thief == null && !map.reachability.CanReachMapEdge(root, TraverseParms.For(TraverseMode.PassDoors, Danger.Some))))
            {
                item = null;
                return false;
            }

            Predicate<Thing> validator = delegate (Thing t)
            {
                if (thief != null && !thief.CanReserve(t))
                {
                    return false;
                }

                if (disallowed != null && disallowed.Contains(t))
                {
                    return false;
                }

                if (!t.def.stealable)
                {
                    return false;
                }

                return (!t.IsBurning()) ? true : false;
            };

            item = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(root, map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), maxDist, validator, (Thing x) => GetValue(x), 15, 15);
            
            if (item != null && GetValue(item) < 320f)
            {
                item = null;
            }



            return item != null;
        }

        public static float GetValue(Thing thing)
        {
            return thing.MarketValue * (float)thing.stackCount;
        }
    }
}
