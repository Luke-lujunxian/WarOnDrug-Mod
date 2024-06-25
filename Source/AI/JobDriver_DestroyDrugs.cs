using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WarOnDrug.AI
{
    internal class JobDriver_DestroyDrugs : JobDriver
    {
        protected const int BaseDurationTicks = 180;

        protected readonly IntRange FilthCount = new IntRange(3, 5);

        protected const float FireFleckSpawnChance = 0.1f;

        protected const float SmokeFleckSpawnChance = 0.3f;

        protected readonly FloatRange FleckSize = new FloatRange(0.3f, 0.5f);

        protected const TargetIndex DrugIndex = TargetIndex.A;

        protected Thing TargetDrug => job.GetTarget(TargetIndex.A).Thing;

        protected int DurationTicks => Mathf.CeilToInt(pawn.GetStatValue(StatDefOf.GeneralLaborSpeed) * 180f);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetDrug, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            Toil wait = Toils_General.Wait(DurationTicks, TargetIndex.A).PlaySustainerOrSound(SoundDefOf.FireBurning).WithEffect(EffecterDefOf.Fire_Burst, TargetIndex.A)
                .WithProgressBarToilDelay(TargetIndex.A)
                .FailOnDespawnedOrNull(TargetIndex.A);
            wait.tickAction = delegate
            {
                TryThrowFlecks(TargetDrug.DrawPos, TargetDrug.Map);
            };
            yield return wait;
            yield return Toils_General.Do(CompleteJob);
        }

        protected void TryThrowFlecks(Vector3 vector, Map map)
        {
            if (Rand.Chance(0.3f))
            {
                FleckMaker.ThrowSmoke(vector, map, FleckSize.RandomInRange);
            }
            if (Rand.Chance(0.1f))
            {
                FleckMaker.ThrowFireGlow(vector, map, FleckSize.RandomInRange);
            }
        }

        protected void CompleteJob()
        {
            Thing targetDrug = TargetDrug;
            IntVec3 position = targetDrug.Position;
            Map map = targetDrug.Map;
            targetDrug.Destroy();
        }
    }
}
