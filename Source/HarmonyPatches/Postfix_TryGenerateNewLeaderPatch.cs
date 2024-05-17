using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace WarOnDrug
{
    //Handle leader lost attitude change
    [HarmonyPatch]
        public static class Postfix_TryGenerateNewLeaderPatch
        {
        [HarmonyPatch(typeof(Faction), nameof(Faction.TryGenerateNewLeader))]
        public static void Postfix(Faction __instance, bool __state)
            {
                //When generating factionLeader, but not when generating a faction
                if (__state)
                {
                    //do change leader event
                }
            }
        [HarmonyPatch(typeof(Faction), nameof(Faction.TryGenerateNewLeader))]
        public static void Prefix(Faction __instance,ref bool __state)
        {
            //When generating factionLeader, but not when generating a faction
            if (__instance.leader != null)
            {
                __state = true;
            }
        }
    }
    
}
