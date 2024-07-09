using RimWorld;
using Verse;

namespace WarOnDrug
{
    public class ContactInfoComp: ThingComp
    {
        public bool isContact = false;
        public bool contactReady = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isContact, "isContact", false);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            if (!isContact)
            {
                return;
            }
            Pawn pawn = (Pawn)parent;
            //Contact in raid will run asap when attacked
            if (pawn.Map.IsPlayerHome && pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                absorbed = true;
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, forced:true);

            }
        }

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            if (!isContact)
            {
                return;
            }

            Pawn pawn = (Pawn)parent;
            WarEffortManager manager = Find.World.GetComponent<WarEffortManager>();
            manager.ManagedFactions[pawn.Faction.loadID].contacts.Remove(pawn);
            //Send notification
            Find.LetterStack.ReceiveLetter("ContactKilled".Translate(), "ContactKilledDesc".Translate(pawn.Name), LetterDefOf.NeutralEvent, pawn);
        }


    }
}
