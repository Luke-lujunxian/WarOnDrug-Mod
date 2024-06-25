using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace WarOnDrug.Raid
{
    public class RaidStrategyWorker_RDEAImmediateAttackBreachingAndConfiscate : RaidStrategyWorker_ImmediateAttackBreaching
    {
        protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            Faction faction = parms.faction;
            bool canTimeoutOrFlee = parms.canTimeoutOrFlee;
            return new LordJob_RDEAAssaultColony(canKidnap: parms.canKidnap, canTimeoutOrFlee: canTimeoutOrFlee, sappers: false, canSteal: parms.canSteal, assaulterFaction: faction, useAvoidGridSmart: useAvoidGridSmart, breachers: true);
        }

        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            //Only RDEA will use this kind of raid
            if (parms.faction.def != WodDefOf.RIM_DEA)
            {
                return false;
            }
            return base.CanUseWith(parms, groupKind);
        }
    }

    public class LordJob_RDEAAssaultColony : LordJob_AssaultColony
    {
        private bool sappers;

        private bool useAvoidGridSmart;

        private bool canSteal = true;

        private bool breachers;

        private bool canPickUpOpportunisticWeapons;

        private Faction assaulterFaction;

        public LordJob_RDEAAssaultColony(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true, bool breachers = false, bool canPickUpOpportunisticWeapons = false)
            : base(assaulterFaction, false, canTimeoutOrFlee, sappers, useAvoidGridSmart, false, breachers, canPickUpOpportunisticWeapons)
        {
            this.assaulterFaction = assaulterFaction;
            this.sappers = sappers;
            this.useAvoidGridSmart = useAvoidGridSmart;
            this.canSteal = canSteal;
            this.breachers = breachers;
            this.canPickUpOpportunisticWeapons = canPickUpOpportunisticWeapons;
        }

        public override StateGraph CreateGraph()
        {

            StateGraph stateGraph = new StateGraph();
            List<LordToil> list = new List<LordToil>();
            LordToil lordToil = null;
            if (sappers)
            {
                lordToil = new LordToil_AssaultColonySappers();
                if (useAvoidGridSmart)
                {
                    lordToil.useAvoidGrid = true;
                }

                stateGraph.AddToil(lordToil);
                list.Add(lordToil);
                Transition transition = new Transition(lordToil, lordToil, canMoveToSameState: true);
                transition.AddTrigger(new Trigger_PawnLost());
                stateGraph.AddTransition(transition);
            }

            LordToil lordToil2 = null;
            if (breachers)
            {
                lordToil2 = new LordToil_AssaultColonyBreaching();
                if (useAvoidGridSmart)
                {
                    lordToil2.useAvoidGrid = useAvoidGridSmart;
                }

                stateGraph.AddToil(lordToil2);
                list.Add(lordToil2);
            }

            LordToil lordToil3 = new LordToil_AssaultColony(attackDownedIfStarving: false, canPickUpOpportunisticWeapons);
            if (useAvoidGridSmart)
            {
                lordToil3.useAvoidGrid = true;
            }

            stateGraph.AddToil(lordToil3);
/*            LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, canDig: false, interruptCurrentJob: true);
            lordToil_ExitMap.useAvoidGrid = true;
            stateGraph.AddToil(lordToil_ExitMap);*/

            if (sappers)
            {
                Transition transition2 = new Transition(lordToil, lordToil3);
                transition2.AddTrigger(new Trigger_NoFightingSappers());
                stateGraph.AddTransition(transition2);
            }

/*            //Break druglabs
            LordToil lordToil4 = stateGraph.AttachSubgraph(new LordJob_DestoryLab().CreateGraph()).StartingToil;
            Transition transition3 = new Transition(lordToil3, lordToil4);
            transition3.AddPreAction(new TransitionAction_Message("MessageRaidersDestoryingLab".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
            transition3.AddSources(list);
            stateGraph.AddTransition(transition3);*/



            LordToil startingToil2 = stateGraph.AttachSubgraph(new LordJob_Confiscate().CreateGraph()).StartingToil;
            Transition transition6 = new Transition(lordToil3, startingToil2);
            transition6.AddSources(list);
            transition6.AddPreAction(new TransitionAction_Message("MessageRaidersConfiscating".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
            transition6.AddTrigger(new Trigger_HighValueThingsAround());
            stateGraph.AddTransition(transition6);

            return stateGraph;
        }
    }

    public class LordJob_Confiscate : LordJob
    {
        public override bool GuiltyOnDowned => true;

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_ConfiscateCover lordToil_StealCover = new LordToil_ConfiscateCover
            {
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_StealCover);
            LordToil_ConfiscateCover lordToil_StealCover2 = new LordToil_ConfiscateCover
            {
                cover = false,
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_StealCover2);
            Transition transition = new Transition(lordToil_StealCover, lordToil_StealCover2);
            transition.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(1200));
            stateGraph.AddTransition(transition);
            return stateGraph;
        }
    }

    public class LordToil_ConfiscateCover : LordToil_DoOpportunisticTaskOrCover
    {
        protected override DutyDef DutyDef => WodDefOf.WOD_Confiscate;

        public override bool ForceHighStoryDanger => true;

        public override bool AllowSelfTend => false;

        protected override bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets)
        {
            if (pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDef && pawn.carryTracker.CarriedThing != null)
            {
                target = pawn.carryTracker.CarriedThing;
                return true;
            }

            return StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 7f, out target, pawn, alreadyTakenTargets);
        }
    }

 /*   public class LordJob_DestoryLab : LordJob
    {
        public override bool GuiltyOnDowned => true;

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_DestoryLabCover lordToil_StealCover = new LordToil_DestoryLabCover
            {
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_StealCover);
            LordToil_DestoryLabCover lordToil_StealCover2 = new LordToil_DestoryLabCover
            {
                cover = false,
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_StealCover2);
            Transition transition = new Transition(lordToil_StealCover, lordToil_StealCover2);
            transition.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(1200));
            stateGraph.AddTransition(transition);
            return stateGraph;
        }
    }

    public class LordToil_DestoryLabCover : LordToil_DoOpportunisticTaskOrCover
    {
        protected override DutyDef DutyDef => WodDefOf.WOD_DestoryLab;

        public override bool ForceHighStoryDanger => true;

        public override bool AllowSelfTend => false;

        protected override bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets)
        {
            if (pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDef && pawn.carryTracker.CarriedThing != null)
            {
                target = pawn.carryTracker.CarriedThing;
                return true;
            }

            return StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 7f, out target, pawn, alreadyTakenTargets);
        }
    }*/
}
