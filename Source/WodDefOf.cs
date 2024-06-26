﻿using RimWorld;
using Verse;
using Verse.AI;


namespace WarOnDrug
{
    [DefOf]
    internal class WodDefOf
    {
        public static TraderKindDef WOD_Caravan_DrugCollector;
        public static TraderKindDef WOD_Caravan_DEA_DrugDestoryer;

        public static JobDef WOD_ConfiscateDrugs;

        public static DutyDef WOD_Confiscate;
        //public static DutyDef WOD_DestoryLab;

        public static FactionDef RIM_DEA;

        public static ThingDef DrugLab;


        static WodDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WodDefOf));
        }
    }
}
