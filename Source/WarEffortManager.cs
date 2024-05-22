using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace WarOnDrug
{
    // This is the global manager for the War on drug status

    public class WarEffortManager : WorldComponent
    {
        Dictionary<int, WODFactionStatus> managedFactions;
        public float totalMarketSize = 0.0f;
        public WarEffortManager(World world) : base(world)
        {
            managedFactions = new Dictionary<int, WODFactionStatus>();
        }

        public void OnFactionCreate(Faction faction)
        {
            if (faction.temporary || faction.Hidden) { return; }//Do nothing with these two

            if (ModsConfig.IdeologyActive)// Check if ideology?
            {
                //Log.Message("Ideology detected");
                managedFactions.Add(faction.loadID, new WODFactionStatus(faction));
            }
            else
            {

                Log.Message(string.Format("[WOD] No Ideology detected, faction {0} is generated with random", faction.Name));
                managedFactions.Add(faction.loadID, new WODFactionStatus());
            }

        }

        public Dictionary<int, WODFactionStatus> ManagedFactions { get => this.managedFactions; }

        public void OnNewLeaderGenerate()
        {

        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref managedFactions, "WODmanagedFactions", LookMode.Value, LookMode.Deep);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if ((Find.TickManager.TicksGame - 114) % (GenDate.TicksPerDay) == 0) //Every day, but slightly later?
            {
                foreach (KeyValuePair<int, WODFactionStatus> faction in managedFactions)
                {
                    faction.Value.fulfilmentRate += faction.Value.dailyInflux / faction.Value.marketSize - 1;
                    float newMarket = Math.Max((faction.Value.dailyInflux - faction.Value.marketSize) * (1 + faction.Value.corruption) * 0.02f, 0);//Part of the excessive influx will create demand
                    faction.Value.marketSize += newMarket;
                    faction.Value.corruption = Math.Max(-1.0f, Math.Min(1.0f, faction.Value.corruption + newMarket * 0.01f));//Corruption is capped at -1.0f and 1.0f
                    totalMarketSize += faction.Value.marketSize;
                    faction.Value.dailyInflux = 0.0f;

                }

                if (WarOnDrug.VTE)//Counter the VTE price flucuation and create demand(todo)
                {


                    /*                        TradingManager VTEmanager = Current.Game.GetComponent<TradingManager>();
                                            //sum of (1-Val)
                                            float totalVal = (WarOnDrug.DrugList.Values.Count) + (WarOnDrug.DrugList.Values).Sum();
                                            foreach (var item in WarOnDrug.DrugList)
                                            {
                                                if (!Utils.tradeableItemsToIgnore.Contains(item.Key))
                                                {
                                                    if (VTEmanager.thingsAffectedBySoldPurchasedMarketValue.ContainsKey(item.Key))
                                                    {
                                                        //Less lasting drug is conumed more, so bought more. ......Right? idk, really. This dosen't make a lot of sense
                                                        VTEmanager.thingsAffectedBySoldPurchasedMarketValue[item.Key] -= item.Key.GetStatValueAbstract(StatDefOf.MarketValue) * totalMarketSize * ((1 - item.Value) / totalVal);
                                                    }
                                                    else
                                                    {
                                                        VTEmanager.thingsAffectedBySoldPurchasedMarketValue[item.Key] = (0f - item.Key.GetStatValueAbstract(StatDefOf.MarketValue)) * totalMarketSize * ((1 - item.Value) / totalVal);
                                                    }
                                                }
                                            }*/

                }
                else
                {
                    //If VTE exist, this value is used and reset on a patch to VTE
                    totalMarketSize = 0;
                }
            }


        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            WarOnDrug.DrugList = WarOnDrug.GenerateDrugList();
        }

        public static WarEffortManager GetWarEffortManager => Find.World.GetComponent<WarEffortManager>();

    }

    public class WODFactionStatus : IExposable
    {
        public Faction faction;

        //persentage
        public float corruption = 0.0f;
        //industralized 1
        public float marketSize = 0.0f;
        //market size
        //public float marketDemand = 0.0f;
        //daily influx
        public float dailyInflux = 0.0f;
        //fulfillment rate
        public float fulfilmentRate = 1.0f;
        public WODFactionStatus(Faction faction)
        {
            this.faction = faction;
            if (faction.IsPlayer)//Do nothing if is player
            {
                return;
            }
            foreach (Precept item in faction.ideos.PrimaryIdeo.PreceptsListForReading)
            {
                if (item.def.defName.Contains("DrugUse_"))
                {
                    string drugPolicy = item.def.defName.Split('_')[1];
                    switch (drugPolicy)
                    {
                        case "Prohibited":
                            corruption = -1.0f;
                            break;
                        case "MedicalOnly":
                            corruption = -0.5f;
                            break;
                        case "MedicalOrSocial":
                            corruption = 0.0f;
                            break;
                        case "Essential":
                            corruption = 0.5f;
                            marketSize = 100.0f;
                            break;
                    }
                    break;
                }
            }
#if DEBUG
            marketSize = UnityEngine.Random.Range(100, 1000);
#endif
        }

        public WODFactionStatus()
        {
            //Do nothing
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref corruption, "corruption", 0.0f);
            Scribe_Values.Look(ref marketSize, "marketSize", 0.0f);
            //Scribe_Values.Look(ref marketDemand, "marketDemand", 0.0f);
            Scribe_Values.Look(ref dailyInflux, "dailyInflux", 0.0f);
            Scribe_References.Look(ref faction, "faction", true);
        }

    }

    struct status
    {

    }
}
