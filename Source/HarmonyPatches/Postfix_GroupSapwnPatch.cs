using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace WarOnDrug.HarmonyPatches
{
    [HarmonyPatch]
    public static class Postfix_GroupSapwnPatch
    {
        [HarmonyPatch(typeof(IncidentWorker_NeutralGroup), "SpawnPawns",new Type[] {typeof(IncidentParms)})]
        public static void Postfix(ref List<Pawn> __result, IncidentParms parms, IncidentWorker_NeutralGroup __instance)
        {
            Random rand = new Random();
            Find.World.GetComponent<WarEffortManager>().ManagedFactions.TryGetValue(parms.faction.loadID, out var factionStatus);
            if (factionStatus is null) return;//not active faction
            for (int i = 0; i < __result.Count; i++)
            {
                Pawn pawn = __result[i];

            }
        }


        
    }


}
