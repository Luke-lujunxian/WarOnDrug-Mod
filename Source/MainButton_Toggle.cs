using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WarOnDrug
{
    internal class MainButton_Toggle: MainButtonWorker
    {
/*        public override bool Disabled
        {
            get
            {
                this.def.buttonVisible = Settings.showIcon;

                return Find.CurrentMap == null
                    && (!def.validWithoutMap || def == MainButtonDefOf.World) || Find.WorldRoutePlanner.Active
                    && Find.WorldRoutePlanner.FormingCaravan
                    && (!def.validWithoutMap || def == MainButtonDefOf.World);
            }
        }*/

        public override void Activate()
        {
           
            if (Find.WindowStack.WindowOfType<Window_WarStatus>() != null)
            {
                Find.WindowStack.TryRemove(typeof(Window_WarStatus));
            }
            else
            {
                Find.WindowStack.Add(new Window_WarStatus());
            }
  
        }
    }

}

