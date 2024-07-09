using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
namespace WarOnDrug.HarmonyPatches
{
    [HarmonyPatch]
    public static class PrisonerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.ExitMap))]
        public static void ExitMap_Postfix(ref Pawn __instance, bool __state)
        {
            if (__instance.NonHumanlikeOrWildMan() || __instance.Faction == null || __instance.Faction.IsPlayer || __instance.Faction.defeated)
            {
                return;
            }

            if (__state)
            {
                ContactInfoComp contactInfoComp = __instance.TryGetComp<ContactInfoComp>();
                if (contactInfoComp == null || !contactInfoComp.contactReady)
                {
                    return;
                }

                WarEffortManager manager = Find.World.GetComponent<WarEffortManager>();
                manager.ManagedFactions[__instance.Faction.loadID].contacts.Add(__instance);
                try
                {
                    contactInfoComp.isContact = true;
                }
                catch (System.NullReferenceException e)
                {
                    Log.Error("ContactInfoComp not found on Pawn " + __instance);
                }
#if DEBUG
                Log.Message($"Prisoner {__instance} released as contact");
                Log.Message($"Faction {__instance.Faction} has {manager.ManagedFactions[__instance.Faction.loadID].contacts.Count} contacts now");
#endif
            }
            else
            {
#if DEBUG
                Log.Message($"Prisoner {__instance} ReleaseAsContact not set");
#endif
            }


        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.ExitMap))]
        public static void ExitMap_Prefix(ref Pawn __instance, out bool __state)
        {
            __state = __instance.guest.Released;
#if DEBUG
            Log.Message($"Prisoner {__instance} Released {__state}");
#endif
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_GuestTracker),nameof(Pawn_GuestTracker.ToggleNonExclusiveInteraction))]
        public static void ToggleNonExclusiveInteraction_Postfix(Pawn ___pawn, PrisonerInteractionModeDef def, bool enabled)
        {
            if (def == WodDefOf.ReleaseAsContact)
            {
                if (enabled)
                {
                    ___pawn.TryGetComp<ContactInfoComp>().contactReady = true;
                    if (Prefs.DevMode && (___pawn.guest.resistance != 0 || ___pawn.guest.will != 0 || !___pawn.guest.Recruitable))
                    {
                        Messages.Message("WOD_ContactNotReadyDevForced".Translate(), MessageTypeDefOf.CautionInput, false);
                    }
                }
                else
                {
                    ___pawn.TryGetComp<ContactInfoComp>().contactReady = false;
                }
            }
        }


        public static void CanUsePrisonerInteractionMode_Postfix(Pawn pawn, PrisonerInteractionModeDef mode, ref bool __result)
        {
            if (mode != WodDefOf.ReleaseAsContact)// Not my job
            {
                return;
            }

            if (pawn.Faction.IsPlayer) // is colony pawn
            {
                __result = false;
                return;
            }

            if (pawn.guest.resistance != 0 || pawn.guest.will != 0 || !pawn.guest.Recruitable) //not recruitable or not breaked
            {
                if (Prefs.DevMode)
                { 
                    __result = true;
                    return;
                }

                __result = false;
                return;
            }


        }

    }
}
