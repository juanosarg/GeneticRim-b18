using System;
using Verse;
using Verse.AI;
using RimWorld;


using System.Linq;


namespace AnimalRangedVerbsUnlocker
{
    public class JobGiver_ReactToCloseMeleeThreatWithRanged : ThinkNode_JobGiver
    {
        private const int MaxMeleeChaseTicks = 200;

        protected override Job TryGiveJob(Pawn pawn)
        {
            Pawn meleeThreat = pawn.mindState.meleeThreat;
            if (meleeThreat == null)
            {
                return null;
            }
            if (this.IsHunting(pawn, meleeThreat))
            {
                return null;
            }
            if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
            {
                return null;
            }
            if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse && pawn.playerSettings.hostilityResponse != HostilityResponseMode.Attack)
            {
                return null;
            }
            if (!pawn.mindState.MeleeThreatStillThreat)
            {
                pawn.mindState.meleeThreat = null;
                return null;
            }
            if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
            {
                return null;
            }
            // return new Job(JobDefOf.AttackMelee, meleeThreat)
            Thing pawn2 = this.FindPawnTarget(pawn);
            return this.RangedAttackJob(pawn, pawn2);
            
        }

        private bool IsHunting(Pawn pawn, Pawn prey)
        {
            if (pawn.CurJob == null)
            {
                return false;
            }
            JobDriver_Hunt jobDriver_Hunt = pawn.jobs.curDriver as JobDriver_Hunt;
            if (jobDriver_Hunt != null)
            {
                return jobDriver_Hunt.Victim == prey;
            }
            JobDriver_PredatorHunt jobDriver_PredatorHunt = pawn.jobs.curDriver as JobDriver_PredatorHunt;
            return jobDriver_PredatorHunt != null && jobDriver_PredatorHunt.Prey == prey;
        }
        


        private bool TryFindShootPosition(Pawn pawn, Thing target, out IntVec3 dest)
        {

            Verb verb = TryGetRangedVerb(pawn);
            if (verb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }

            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = target,
                verb = verb,
                maxRangeFromTarget = verb.verbProps.range,
                wantCoverFromTarget = false
            }, out dest);
        }

        private Verb TryGetRangedVerb(Pawn pawn)
        {
            Verb verb = pawn.verbTracker.AllVerbs.Where(v => v.verbProps.range > 1.1f).RandomElement();
            return verb;
        }

      

        private Job RangedAttackJob(Pawn pawn, Thing target)
        {
            if (target == null)
            {
                return null;
            }
            Verb verb = TryGetRangedVerb(pawn);
            if (verb == null)
            {
                return null;
            }

            IntVec3 intVec;
            if (!this.TryFindShootPosition(pawn, target, out intVec))
            {
                return null;
            }
            if (intVec == pawn.Position)
            {
                TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToAll;
                Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(pawn, null, verb.verbProps.range, verb.verbProps.minRange, targetScanFlags);


                if (thing != null)
                {
                    return new Job(DefDatabase<JobDef>.GetNamed("AnimalRangedAttack"), thing, (int)verb.verbProps.warmupTime, true)
                    {
                        verbToUse = verb
                    };
                }
            }
            // Find.PawnDestinationManager.ReserveDestinationFor(pawn, intVec);
            return new Job(JobDefOf.Goto, intVec)
            {
                expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange,
                checkOverrideOnExpire = true
            };
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat, (Thing x) => x is Pawn && x.def.race.intelligence >= Intelligence.ToolUser, 0f, 9999f, default(IntVec3), 3.40282347E+38f, true);
        }
    }
}

