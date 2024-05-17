using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WarOnDrug.HarmonyPatches
{
    [HarmonyPatch]
    public static class Postfix_NewGeneratedFactionPatch
    {
        [HarmonyPatch(typeof(FactionGenerator), "NewGeneratedFaction", new Type[] { typeof(FactionGeneratorParms) })]
        public static void Postfix(ref Faction __result)
        {
            WarEffortManager manager = Find.World.GetComponent<WarEffortManager>();

            //Do something here
            manager.OnFactionCreate(__result);
        }
    }
}
