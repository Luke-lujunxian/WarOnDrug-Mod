using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarOnDrug.OnionPod
{
    public class CompProperties_LaunchableOnion: CompProperties_Launchable
    {
        public CompProperties_LaunchableOnion():base()
        {
            
            compClass = typeof(CompLaunchableOnion);
        }
    }
}
