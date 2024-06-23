using HarmonyLib;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using WarOnDrug.HarmonyPatches.VTE;

namespace WarOnDrug
{
    public class WarOnDrug : Mod
    {
        public static bool VTE = false;
        public static Dictionary<ThingDef, float> DrugList;
        public WarOnDrug(ModContentPack content) : base(content)
        {
            
            var harmony = new Harmony("lke.warondrug.faction.thisisanid");
#if DEBUG
            Harmony.DEBUG = true;
            foreach (var mod in LoadedModManager.RunningModsListForReading)
            {
                Log.Message(mod.Name);
            }
            Log.Message("------------------------------------------------------");
#endif
            harmony.PatchAllUncategorized();
            

            try
            {
                ((Action)(() =>
                {
                    if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == ("Vanilla Trading Expanded")))
                    {
                        Log.Message("[WOD] VanillaTradingExpanded detected");
                        VTE = true;
                        var info1 = harmony.Patch(AccessTools.Method("VanillaTradingExpanded.Contract:ContractFulfilmentChance"),
                            postfix: new HarmonyMethod(typeof(Postfix_VTEPatches), nameof(Postfix_VTEPatches.Postfix)));
                        var info2 = harmony.Patch(AccessTools.Method("VanillaTradingExpanded.TradingManager:ProcessPlayerTransactions"),
                            prefix: new HarmonyMethod(typeof(Postfix_RegisterSoldThingPatch), nameof(Postfix_RegisterSoldThingPatch.Prefix)));
#if DEBUG
                        Log.Message(info1);
                        Log.Message(info2);
#endif
                    }
                }))();
            }
            catch (TypeLoadException ex) { Log.Error("[WOD] Error when patching VanillaTradingExpanded "); }

/*            if (Harmony.HasAnyPatches("OskarPotocki.VanillaTradingExpanded"))
            {
                Log.Message("[WOD] VanillaTradingExpanded detected");
                VTE = true;
            }
            else
            {
                VTE = false;
            }*/

            
        }

        
        

        public static Dictionary<ThingDef, float> GenerateDrugList()
        {
            //Only hard drug metters?
            Dictionary<ThingDef, float> drugList = new Dictionary<ThingDef, float>
            {
                //The vlaue is one item's contribution to influx, calculated by Duration of high (assuming no tolerance)/24h
                //1 is the amount for one pawn to consume
                {DefDatabase<ThingDef>.GetNamed("Yayo"), 0.5f },
                {DefDatabase<ThingDef>.GetNamed("Flake"), 0.25f },
                {DefDatabase<ThingDef>.GetNamed("GoJuice"), 0.67f },
                {DefDatabase<ThingDef>.GetNamed("WakeUp"), 0.5f },
                {DefDatabase<ThingDef>.GetNamed("SmokeleafJoint"), 0.5f*0.25f },

            };
            return drugList;
        }

        [DebugAction("WarOnDrugDebug", "Print WarEffortManager", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        private static void PrintStatus()
        {
            WarEffortManager manager = Find.World.GetComponent<WarEffortManager>();
            foreach (var faction in manager.ManagedFactions)
            {
                Log.Message(string.Format("{0} has {1} market size and {2} corruption", faction.Value.faction.Name, faction.Value.marketSize, faction.Value.corruption));
            }
        }

    }


}

