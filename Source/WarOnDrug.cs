using HarmonyLib;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using WarOnDrug.HarmonyPatches;
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

            //Special patch for local method
            harmony.Patch(typeof(ITab_Pawn_Visitor).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First((MethodInfo m) => m.Name.Contains("DoPrisonerTab") && m.Name.Contains("CanUsePrisonerInteractionMode")), null, new HarmonyMethod(typeof(PrisonerPatches), nameof(PrisonerPatches.CanUsePrisonerInteractionMode_Postfix)));


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
                        Log.Message(info1.ToString());
                        Log.Message(info2.ToString());
#endif
                    }
                }))();
            }
            catch (TypeLoadException ex) { Log.Error("[WOD] Error when patching VanillaTradingExpanded" + ex); }

            /*            if (Harmony.HasAnyPatches("OskarPotocki.VanillaTradingExpanded"))
                        {
                            Log.Message("[WOD] VanillaTradingExpanded detected");
                            VTE = true;
                        }
                        else
                        {
                            VTE = false;
                        }*/
            Log.Message($"[WOD] DrugList building");
            LongEventHandler.QueueLongEvent(GenerateDrugList, "loadDrugList", false, null);
            
            
        }

         
        

        public static void GenerateDrugList()
        {
            if (DrugList != null && DrugList.Count != 0)
            {
                return;
            }

            //Only hard drug metters?
            Dictionary<ThingDef, float> drugList = new Dictionary<ThingDef, float>(10);
/*            {
                //The vlaue is one item's contribution to influx, calculated by Duration of high (assuming no tolerance)/24h
                //1 is the amount for one pawn to consume
                {DefDatabase<ThingDef>.GetNamed("Yayo"), 0.5f },
                {DefDatabase<ThingDef>.GetNamed("Flake"), 0.25f },
                {DefDatabase<ThingDef>.GetNamed("GoJuice"), 0.67f },
                {DefDatabase<ThingDef>.GetNamed("WakeUp"), 0.5f },
                {DefDatabase<ThingDef>.GetNamed("SmokeleafJoint"), 0.5f*0.25f },

            };*/
            foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (item.IsWithinCategory(ThingCategoryDefOf.Drugs)  && item.ingestible != null)
                {
                    if (item.ingestible.drugCategory == DrugCategory.Hard)
                    {
                        bool added = false;
                        foreach(IngestionOutcomeDoer outcome in item.ingestible.outcomeDoers)
                        {
                            if(outcome is IngestionOutcomeDoer_GiveHediff)
                            {
                                IngestionOutcomeDoer_GiveHediff hediff = (IngestionOutcomeDoer_GiveHediff)outcome;
                                if(hediff.hediffDef.hediffClass == typeof(Hediff_High) || hediff.hediffDef.defName.Contains("High"))
                                {
                                    HediffCompProperties_SeverityPerDay Comp_SeverityPerDay = hediff.hediffDef.CompProps<HediffCompProperties_SeverityPerDay>();
                                    if (Comp_SeverityPerDay is null)
                                    {
                                        Log.Warning($"[WOD] Drug type {item}'s Hediff_High/likely high outcome {hediff.hediffDef.hediffClass} does not have HediffCompProperties_SeverityPerDay\n" +
                                                                                       $"This is likely to be a mod item with special design, please report this to WOD developer");
                                        continue;
                                    }

                                    float severityPday = Comp_SeverityPerDay.severityPerDay;
                                    if(severityPday > 0) {
                                        Log.Warning($"[WOD] Drug type {item}'s Hediff_High outcome {hediff.hediffDef.hediffClass} has positive severityPerDay value {severityPday}\n" +
                                            $"This is likely to be a mod item with special design, please report this to WOD developer");
                                    }
                                    severityPday = Math.Abs(severityPday);
                                    if (drugList.ContainsKey(item))
                                    {
                                        Log.Warning($"Drug type {item} contains more than one Hediff_High outcome.\n " +
                                            $"This is likely to be a mod item with special design, please report this to WOD developer");
                                        drugList[item] = Math.Min(drugList[item], hediff.severity / severityPday);
                                    }
                                    else
                                    {
                                        drugList.Add(item, hediff.severity/ severityPday);
                                    }
                                    added = true;
#if DEBUG
                                    Log.Message($"Drug type {item} added, value {hediff.severity / severityPday}");
#endif
                                }
                            }
                        }
                        if (!added)
                        {
                            Log.Warning($"[WOD] Hard drug type {item} dose not give any hediff of type Hediff_High, this may be intented or a oversight of mod author.\n " +
                                $"It is now ignored by WOD drug market, if you think it should be include, please report this to WOD developer.");
                        }
                    }
                }
            }
            DrugList = drugList;
            Log.Message($"DrugList builded with {DrugList.Count} drugs");
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

        [DebugAction("WarOnDrugDebug", "Make RDEA hostile", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        private static void PissRDEA()
        {
            Find.FactionManager.FirstFactionOfDef(WodDefOf.RIM_DEA).SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile);
        }

    }


}

