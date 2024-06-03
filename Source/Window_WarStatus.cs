using Verse;
using UnityEngine;

namespace WarOnDrug
{
    internal class Window_WarStatus : Window
    {
        public override Vector2 InitialSize => new Vector2(800, 600);

        public Window_WarStatus()
        {
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
        }
        
        public override void PreOpen()
        {
            //Get window size and resize
            this.windowRect = new Rect((float)(Screen.width / 2 - 150), (float)(Screen.height / 2 - 150), Screen.width*0.8f, Screen.height*0.8f);
            base.PreOpen();
        }

        public override void DoWindowContents(Rect inRect)
        {
            
            Text.Font = GameFont.Medium;
            Widgets.LabelFit(new Rect(0, 0, inRect.width, 30), "WarOnDrug".Translate());
            Widgets.LabelFit(new Rect(0, 0, inRect.width, 30), "Status".Translate());
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



            Widgets.TextArea(rect, statusText, true);


        }
    }
}
