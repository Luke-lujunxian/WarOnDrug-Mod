using RimWorld;
using Verse;
using Verse.AI;


namespace WarOnDrug
{
#pragma warning disable CS0649

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

        public static PrisonerInteractionModeDef ReleaseAsContact;
        public static ThingDef OnionTransportPod;
        public static ThingDef OnionDropPodLeaving;



        static WodDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WodDefOf));
        }
    }
}
