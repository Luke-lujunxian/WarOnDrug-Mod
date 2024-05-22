using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WarOnDrug.HarmonyPatches
{
    [HarmonyPatch]
    public static class Postfix_TryGenerateRaidInfoPatch
    {
        [HarmonyPatch(typeof(IncidentWorker_Raid), nameof(IncidentWorker_Raid.TryGenerateRaidInfo))]
        public static void Postfix(ref bool __result, ref IncidentWorker_Raid __instance, ref List<Pawn> pawns, IncidentParms parms)
        {

            if (pawns is null || parms is null || parms.target is null) return;
            Find.World.GetComponent<WarEffortManager>().ManagedFactions.TryGetValue(parms.faction.loadID, out var factionStatus);
            if (factionStatus is null) return;//not active faction

            HediffDef ChemicalDamageModerate = DefDatabase<HediffDef>.GetNamed("ChemicalDamageModerate");
            HediffDef ChemicalDamageSevere = DefDatabase<HediffDef>.GetNamed("ChemicalDamageSevere");
            HediffDef Asthma = DefDatabase<HediffDef>.GetNamed("Asthma");
            HediffDef PsychiteAddiction = DefDatabase<HediffDef>.GetNamed("PsychiteAddiction");
            BodyPartDef Brain = DefDatabase<BodyPartDef>.GetNamed("Brain");

            for (int i = 0; i < pawns.Count; i++)
            {
                //Long term hediff
                Pawn pawn = pawns[i];
                if (UnityEngine.Random.value < factionStatus.corruption * 0.25)// give raider random chemical damage
                {
                    var kidney = GetRandomPartofDef(pawn, "Kidney");
                    //pawn.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Lung);
                    if (pawn.health?.hediffSet?.GetFirstHediffOfDef(ChemicalDamageSevere) == null && !pawn.health.hediffSet.PartIsMissing(kidney))
                    {
#if DEBUG
                        Log.Message(string.Format("Giving {0} Chemical damage", pawn.Name));
#endif
                        pawn.health.AddHediff(ChemicalDamageSevere, kidney);//a random not missing lung?
                    }

                }

                if (UnityEngine.Random.value < factionStatus.corruption * 0.5 * 0.1)
                { // Some on them get severe
                    if (pawn.health?.hediffSet?.GetFirstHediffOfDef(ChemicalDamageModerate) == null)
                    {
                        var brain = pawn.health.hediffSet.GetBodyPartRecord(Brain);
                        pawn.health.AddHediff(ChemicalDamageModerate, brain);
                    }

                }

                if (UnityEngine.Random.value<factionStatus.corruption * 0.1)
                {
                    Hediff hediff = HediffMaker.MakeHediff(Asthma, pawn, GetRandomPartofDef(pawn, "Lung"));
                    hediff.Severity = UnityEngine.Random.Range(0.10f, 0.50f);
                    pawn.health.AddHediff(hediff);
                }

                if (UnityEngine.Random.value < factionStatus.corruption * 0.5)//Addiction
                {
                    PawnAddictionHediffsGenerator.GenerateAddictionsAndTolerancesFor(pawn);
/*                    Log.Message(pawn.needs.AllNeeds.Where(need => need.GetType() == typeof(Need_Chemical)).Count());
                    foreach (var need in pawn.needs.AllNeeds)
                    {
                        Log.Message(need.def.defName);
                    }*/
                    if (UnityEngine.Random.value >= factionStatus.fulfilmentRate)
                    {
                        if (pawn.needs.TryGetNeed<Need_Chemical>() != null)
                        {
                            pawn.needs.AllNeeds.Where(need => need.GetType() == typeof(Need_Chemical)).RandomElement().CurLevel = 0.009f;
                        }
                        else
                        {
#if DEBUG
                            Log.Message("No need");
#endif
                        }
                    }
                }

                if (UnityEngine.Random.value < factionStatus.corruption * 0.01)//Overdoses
                {
                    HediffDef od = DefDatabase<HediffDef>.GetNamed("DrugOverdose");
                    Hediff hediff = HediffMaker.MakeHediff(od, pawn);
                    hediff.Severity = UnityEngine.Random.Range(0.5f, 0.999f);
                    pawn.health.AddHediff(hediff);
                }
            }
            
        }

        private static BodyPartRecord GetRandomPartofDef(Pawn pawn, string defName)
        {
            List<BodyPartRecord> parts = new List<BodyPartRecord>();
            pawn.def.race.body.AllParts.ForEach(part =>
            {
                if (part.def.defName == defName)
                {
                    parts.Add(part);
                }
            });

            if (parts.Count > 0)
            {
                return parts[UnityEngine.Random.Range(0, parts.Count)];
            }
            throw new Exception(string.Format("No part of name: {0} found in pawn {1}", defName, pawn.Name));
            //return null;
        }
    }
}
