using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace WarOnDrug.OnionPod
{
    public class TransportPodsArrivalAction_PopPawnAndLeave: TransportPodsArrivalAction
    {
        private static List<Pawn> tmpPawns = new List<Pawn>();

        public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
        {
            tmpPawns.Clear();
            for (int i = 0; i < pods.Count; i++)
            {
                ThingOwner innerContainer = pods[i].innerContainer;
                for (int num = innerContainer.Count - 1; num >= 0; num--)
                {
                    if (innerContainer[num] is Pawn item)
                    {
                        tmpPawns.Add(item);
                        
                    }
                    innerContainer.Remove(innerContainer[num]);
                }
            }

            if (!GenWorldClosest.TryFindClosestPassableTile(tile, out var foundTile))
            {
                foundTile = tile;
            }

            Caravan caravan = CaravanMaker.MakeCaravan(tmpPawns, Faction.OfPlayer, foundTile, addToWorldPawnsIfNotAlready: true);
            //Drop everything other than pawn

            if(tmpPawns.Count != 0)
            {
                Messages.Message("OnionArriveWithPawn".Translate(), caravan, MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                Messages.Message("OnionArriveWithoutPawn".Translate(), MessageTypeDefOf.TaskCompletion);
            }
            tmpPawns.Clear();
        }



    }
}
