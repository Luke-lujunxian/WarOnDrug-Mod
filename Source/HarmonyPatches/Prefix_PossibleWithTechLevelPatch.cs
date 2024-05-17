using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarOnDrug.HarmonyPatches
{
    [HarmonyPatch]
    public static class Prefix_PossibleWithTechLevelPatch
    {//PossibleWithTechLevel

        [HarmonyPatch(typeof(PawnAddictionHediffsGenerator), "PossibleWithTechLevel")]
        public static bool Prefix(ref bool __result)//Everyone get access to anything!
        {
            __result = true;
            return true;
        }
    }
}
