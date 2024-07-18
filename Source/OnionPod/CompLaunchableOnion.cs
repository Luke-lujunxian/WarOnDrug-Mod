using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace WarOnDrug.OnionPod
{
    public class CompLaunchableOnion : CompLaunchable
    {
        private const float FuelPerTile = 3f;

        public CompLaunchableOnion() : base()
        {

        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (!LoadingInProgressOrReadyToLaunch)
            {
                yield break;
            }

            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandLaunchGroup".Translate();
            command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
            command_Action.icon = LaunchCommandTex;
            command_Action.alsoClickIfOtherInGroupClicked = false;
            command_Action.action = delegate
            {
                if (AnyInGroupHasAnythingLeftToLoad)
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(FirstThingLeftToLoadInGroup.LabelCapNoCount, FirstThingLeftToLoadInGroup), StartChoosingDestination));
                }
                else
                {

                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (ContectOperation.ContectMission mission in WarEffortManager.GetWarEffortManager.missions)
                    {
                        list.Add(new FloatMenuOption(mission.getDisplayID(), delegate
                        {
                            Find.WindowStack.Add(new Dialog_Confirm("ConfirmMissionID".Translate() + " " + mission.getDisplayID(), delegate
                                {
                                    //Lunch the mission
                                    TryLaunch();
                                }));
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            };
            if (WarEffortManager.GetWarEffortManager.missions.Count == 0)
            {
                command_Action.Disable("CommandLaunchGroupFailNoMission".Translate());
            }
            if (!AllInGroupConnectedToFuelingPort)
            {
                command_Action.Disable("CommandLaunchGroupFailNotConnectedToFuelingPort".Translate());
            }
            else if (!AllFuelingPortSourcesInGroupHaveAnyFuel)
            {
                command_Action.Disable("CommandLaunchGroupFailNoFuel".Translate());
            }
            else if (AnyInGroupIsUnderRoof)
            {
                command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
            }

            yield return command_Action;
        }

        public void TryLaunch()
        {
            if (!parent.Spawned)
            {
                Log.Error(string.Concat("Tried to launch ", parent, ", but it's unspawned."));
                return;
            }

            List<CompTransporter> transportersInGroup = TransportersInGroup;
            if (transportersInGroup == null)
            {
                Log.Error(string.Concat("Tried to launch ", parent, ", but it's not in any group."));
            }
            else
            {
                if (!LoadingInProgressOrReadyToLaunch || !AllInGroupConnectedToFuelingPort || !AllFuelingPortSourcesInGroupHaveAnyFuel)
                {
                    return;
                }

                Map map = parent.Map;

                Transporter.TryRemoveLord(map);
                int groupID = Transporter.groupID;
                int destinationTile = map.Tile + Random.Range(-10, 10);
                for (int i = 0; i < transportersInGroup.Count; i++)
                {
                    CompTransporter compTransporter = transportersInGroup[i];
                    compTransporter.Launchable.FuelingPortSource?.TryGetComp<CompRefuelable>().ConsumeFuel(compTransporter.Launchable.FuelingPortSourceFuel);//Use it all
                    ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
                    ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
                    activeDropPod.Contents = new ActiveDropPodInfo();
                    activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, canMergeWithExistingStacks: true, destroyLeftover: true);
                    FlyShipLeaving obj = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(WodDefOf.OnionDropPodLeaving, activeDropPod);
                    obj.groupID = groupID;
                    obj.destinationTile = destinationTile;
                    obj.arrivalAction = new TransportPodsArrivalAction_PopPawnAndLeave();
                    obj.worldObjectDef = WorldObjectDefOf.TravelingTransportPods;
                    compTransporter.CleanUpLoadingVars(map);
                    compTransporter.parent.Destroy();
                    GenSpawn.Spawn(obj, compTransporter.parent.Position, map);
                }

                //Do mission content add

            }
        }



    }
}
