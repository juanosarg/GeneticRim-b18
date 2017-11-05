
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace AnimalRangedVerbsUnlocker
{
    class JobDriver_AnimalRangedAttack : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return this.Fire(this.TargetThingA).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield break;
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }


        private Toil Fire(Thing target)
        {
            Toil toil = new Toil();

            Log.Message("Pawn: " + pawn.ToString() + ", target: " + target.ToString());

            toil.initAction = delegate
            {
                Pawn pawn = this.pawn;

                this.GetActor().CurJob.verbToUse.TryStartCastOn(target);
            };

            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}
