using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WarOnDrug
{
    internal class Window_WarStatus : Window
    {
        public enum WodWinTab : byte
        {
            Stats,
            Contacts,
        }
        private WodWinTab tab;
        public override Vector2 InitialSize => new Vector2(950f, 760f);
        protected override float Margin => 0f;
        public static WarEffortManager Manager;

        public Window_WarStatus()
        {
            Manager = Find.World.GetComponent<WarEffortManager>();
            forcePause = true;
            doCloseButton = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            soundAppear = SoundDefOf.InfoCard_Open;
            soundClose = SoundDefOf.InfoCard_Close;
            StatsReportUtility.Reset();
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InfoCard, KnowledgeAmount.Total);
        }

        public override void PreOpen()
        {
            //Get window size and resize
            //this.windowRect = new Rect((float)(Screen.width / 2 - 150), (float)(Screen.height / 2 - 150), Screen.width * 0.8f, Screen.height * 0.8f);
            base.PreOpen();
        }

        public override void DoWindowContents(Rect inRect)
        {

            /*            Text.Font = GameFont.Medium;
                        Widgets.LabelFit(new Rect(0, 0, inRect.width, 30), "WarOnDrug".Translate());
                        Widgets.LabelFit(new Rect(0, 30, inRect.width, 30), "Status".Translate());
                        //Widgets.Label(new Rect(0, 0, inRect.width, 30), "WarOnDrug".Translate());
                        Text.Font = GameFont.Small;

                        var rect = new Rect(0, 30, inRect.width, inRect.height - 30);

                        Widgets.DrawMenuSection(rect);

                        string statusText = "";
                        WarEffortManager manager = Find.World.GetComponent<WarEffortManager>();
                        foreach (var faction in manager.ManagedFactions)
                        {
                            statusText += string.Format("{0} has {1} market size and {2} corruption\n", faction.Value.faction.Name, faction.Value.marketSize, faction.Value.corruption);
                        }



                        Widgets.TextArea(rect, statusText, true);*/

            Rect rect = new Rect(inRect);
            rect = rect.ContractedBy(18f);
            rect.height = 34f;
            rect.x += 34f;
            Text.Font = GameFont.Medium;
            Widgets.LabelFit(rect, "WODStatusPanel".Translate());
            Rect rect2 = new Rect(inRect.x + 9f, rect.y, 34f, 34f);

            Rect rect3 = new Rect(inRect);
            rect3.yMin = rect.yMax;
            rect3.yMax -= 38f;
            Rect rect4 = rect3;
            List<TabRecord> list = new List<TabRecord>();
            TabRecord item = new TabRecord("TabStats".Translate(), delegate
            {
                tab = WodWinTab.Stats;
            }, tab == WodWinTab.Stats);
            list.Add(item);
            TabRecord item2 = new TabRecord("TabContacts".Translate(), delegate
            {
                tab = WodWinTab.Contacts;
            }, tab == WodWinTab.Contacts);
            list.Add(item2);
            if (list.Count > 1)
            {
                rect4.yMin += 45f;
                TabDrawer.DrawTabs(rect4, list);
            }
            FillCard(rect4.ContractedBy(18f));

        }

        protected void FillCard(Rect cardRect)
        {
            if (tab == WodWinTab.Stats)
            {
                FillStatsCard(cardRect);
            }
            else if (tab == WodWinTab.Contacts)
            {
                FillContactsCard(cardRect);
            }
        }

        private void FillStatsCard(Rect cardRect)
        {
            Widgets.LabelFit(new Rect(cardRect.x, cardRect.y, cardRect.width, 30), "WarOnDrug".Translate());
            Widgets.LabelFit(new Rect(cardRect.x, cardRect.y + 30, cardRect.width, 30), "Status".Translate());
            Text.Font = GameFont.Small;
            var rect = new Rect(cardRect.x, cardRect.y + 60, cardRect.width, cardRect.height - 60);
            Widgets.DrawMenuSection(rect);

            string statusText = "";
            WarEffortManager manager = Find.World.GetComponent<WarEffortManager>();
            foreach (var faction in manager.ManagedFactions)
            {
                statusText += string.Format("{0} has {1} market size and {2} corruption\n", faction.Value.faction.Name, faction.Value.marketSize, faction.Value.corruption);
            }
            Widgets.TextArea(rect, statusText, true);
        }

        private static readonly Texture2D SwitchFactionIcon = ContentFinder<Texture2D>.Get("UI/Icons/SwitchFaction");
        private static readonly Texture2D AddIcon = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");
        public static Faction selectedFaction;
        public Dictionary<ThingDef, int> tempResources = new Dictionary<ThingDef, int>();


        private void FillContactsCard(Rect rect)
        {
            rect.yMax -= 4f;

            Rect rect2 = new Rect(rect.x, rect.y, 32f, 32f);
            if (Widgets.ButtonImage(rect2, SwitchFactionIcon))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Faction item in Find.FactionManager.AllFactionsVisibleInViewOrder)
                {
                    if (!item.IsPlayer && !item.def.hidden)
                    {
                        Faction localFaction = item;
                        list.Add(new FloatMenuOption(localFaction.Name + $"({Manager.ManagedFactions[item.loadID].contacts.Count})", delegate
                        {
                            selectedFaction = localFaction;
                        }, localFaction.def.FactionIcon, localFaction.Color));
                    }
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            TooltipHandler.TipRegion(rect2, "SwitchFaction_Desc".Translate());

            if (selectedFaction != null)
            {
                Widgets.DefIcon(new Rect(rect.width / 2 + rect.x, rect.y, 32f, 32f), selectedFaction.def);
                Widgets.LabelFit(new Rect(rect.width / 2 + rect.x + 32f, rect.y, rect.width / 2 - 32f, 32f), selectedFaction.Name);
            }

            DrawMissionConfig(new Rect(rect.width / 2 + rect.x, rect.y + 32f, rect.width, rect.height));
        }

        public enum contectActions : int
        {
            None = 0,
            Sell,
            Bribe,

        }

        private contectActions ca = 0;

        List<ThingDef> resourcesItems = new List<ThingDef>();
        List<int> resourcesItemsCount = new List<int>();
        private void DrawMissionConfig(Rect rect)
        {
            Rect actionBtn = new Rect(rect.x, rect.y, 64f, 32f);
            if (Widgets.ButtonText(actionBtn, this.ca == 0 ? "Select Action" : this.ca.ToString()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (contectActions suit in Enum.GetValues(typeof(contectActions)))
                {
                    list.Add(new FloatMenuOption(suit.ToString(), delegate
                    {
                        this.ca = suit;
                    }));

                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Rect textAreaResources = new Rect(actionBtn.x + actionBtn.width, rect.y, 256f, 32f);
            Widgets.Label(textAreaResources, "Resources");


            Rect resourcesBtn = new Rect(textAreaResources.x + textAreaResources.width, rect.y, 32f, 32f);
            if (Widgets.ButtonImage(resourcesBtn, AddIcon))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (ThingDef good in WarOnDrug.DrugList.Keys)
                {
                    Texture2D texture = Widgets.GetIconFor(good);
                    list.Add(new FloatMenuOption(good.label, delegate
                    {
                        resourcesItems.Add(good);
                        resourcesItemsCount.Add(0);
                    }, good, texture));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            for (int i = 0; i < resourcesItems.Count; i++)
            {
                ThingDef key = resourcesItems[i];
                Rect resourceItem = new Rect(actionBtn.x , actionBtn.y + actionBtn.height + 32f * i, 32f, 32f);
                Widgets.DefIcon(new Rect(resourceItem.x, resourceItem.y, 32f, 32f), key);
                Widgets.LabelFit(new Rect(resourceItem.x + 32f, resourceItem.y, 128f, 32f), key.label);
                int count = resourcesItemsCount[i];
                string buffer = null;
                Widgets.TextFieldNumeric<int>(new Rect(resourceItem.x + 32f + 128f, resourceItem.y, 64f, 32f), ref count, ref buffer);
                resourcesItemsCount[i] = count;
                if (Widgets.ButtonText(new Rect(resourceItem.x + 32f + 128f + 64f, resourceItem.y , 64f, 32f), "CancelButton".Translate()))
                {
                    resourcesItems.RemoveAt(i);
                    resourcesItemsCount.RemoveAt(i); 
                    i--;
                }
            }
        }



        public void SetTab(WodWinTab infoCardTab)
        {
            tab = infoCardTab;
        }
    }

    internal class Window_setResources : Window
    {
        public TransferableOneWayWidget itemsTransfer;
        private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);
        public bool changed;
        public TransferableOneWay transferables;

        public Window_setResources()
        {

        }

        public override void DoWindowContents(Rect inRect)
        {
            itemsTransfer.OnGUI(inRect, out changed);
            DoBottomButtons(inRect);
        }

        private void DoBottomButtons(Rect rect)
        {
            Rect rect2 = new Rect(rect.width / 2f - BottomButtonSize.x / 2f, rect.height - 55f, BottomButtonSize.x, BottomButtonSize.y);
            if (Widgets.ButtonText(rect2, "AcceptButton".Translate()))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Close(doCloseSound: false);
            }

            if (Widgets.ButtonText(new Rect(rect2.x - 10f - BottomButtonSize.x, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "ResetButton".Translate()))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                itemsTransfer = new TransferableOneWayWidget(new List<TransferableOneWay>(), null, null, "FormCaravanColonyThingCountTip".Translate(), drawMass: true, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, includePawnsMassInMassUsage: false, null, 0f, ignoreSpawnedCorpseGearAndInventoryMass: false, -1, drawMarketValue: true, drawEquippedWeapon: false, drawNutritionEatenPerDay: false, drawMechEnergy: false, drawItemNutrition: true, drawForagedFoodPerDay: false, drawDaysUntilRot: true);

            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 10f, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "CancelButton".Translate()))
            {
                Close();
            }
        }


    }
}
