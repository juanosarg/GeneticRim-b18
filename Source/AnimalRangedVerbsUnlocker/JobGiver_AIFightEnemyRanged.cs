
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

using RimWorld;

using System.Linq;


namespace AnimalRangedVerbsUnlocker
{
    public abstract class JobGiver_AIFightEnemyRanged : ThinkNode_JobGiver
    {
        private const int MinTargetDistanceToMove = 5;

        private const int TicksSinceEngageToLoseTarget = 400;

        public List<VerbProperties> verbs;

        private float targetAcquireRadius = 56f;

        private float targetKeepRadius = 65f;

        private bool needLOSToAcquireNonPawnTargets;

        private bool chaseTarget;

        public static readonly IntRange ExpiryInterval_ShooterSucceeded = new IntRange(450, 550);

        private static readonly IntRange ExpiryInterval_Melee = new IntRange(360, 480);

        protected abstract bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest);

        protected virtual float GetFlagRadius(Pawn pawn)
        {
            return 999999f;
        }

        protected virtual IntVec3 GetFlagPosition(Pawn pawn)
        {
            return IntVec3.Invalid;
        }

        protected virtual bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            return true;
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_AIFightEnemyRanged JobGiver_AIFightEnemyRanged = (JobGiver_AIFightEnemyRanged)base.DeepCopy(resolve);
            JobGiver_AIFightEnemyRanged.targetAcquireRadius = this.targetAcquireRadius;
            JobGiver_AIFightEnemyRanged.targetKeepRadius = this.targetKeepRadius;
            JobGiver_AIFightEnemyRanged.needLOSToAcquireNonPawnTargets = this.needLOSToAcquireNonPawnTargets;
            JobGiver_AIFightEnemyRanged.chaseTarget = this.chaseTarget;
            return JobGiver_AIFightEnemyRanged;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HasRangedVerb(pawn))
            {
                Thing pawn2 = this.FindPawnTarget(pawn);
                return this.RangedAttackJob(pawn, pawn2);
            }
            this.UpdateEnemyTarget(pawn);
            Thing enemyTarget = pawn.mindState.enemyTarget;
            
            if (enemyTarget == null)
            {
                return null;
            }
            bool allowManualCastWeapons = !pawn.IsColonist;
            Verb verb = pawn.TryGetAttackVerb(allowManualCastWeapons);
            if (verb == null)
            {
                return null;
            }
            
            if (verb.verbProps.MeleeRange)
            {
                return this.MeleeAttackJob(enemyTarget);
            }
            bool flag = CoverUtility.CalculateOverallBlockChance(pawn.Position, enemyTarget.Position, pawn.Map) > 0.01f;
            bool flag2 = pawn.Position.Standable(pawn.Map);
            bool flag3 = verb.CanHitTarget(enemyTarget);
            bool flag4 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
            if ((flag && flag2 && flag3) || (flag4 && flag3))
            {
                return new Job(JobDefOf.WaitCombat, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
            }
            IntVec3 intVec;
            if (!this.TryFindShootingPosition(pawn, out intVec))
            {
                return null;
            }
            if (intVec == pawn.Position)
            {
                return new Job(JobDefOf.WaitCombat, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
            }
            //pawn.Map.pawnDestinationManager.ReserveDestinationFor(pawn, intVec);
            return new Job(JobDefOf.Goto, intVec)
            {
                //expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange,
                checkOverrideOnExpire = true
            };
        }

        protected virtual Job MeleeAttackJob(Thing enemyTarget)
        {
            return new Job(JobDefOf.AttackMelee, enemyTarget)
            {
                expiryInterval = JobGiver_AIFightEnemyRanged.ExpiryInterval_Melee.RandomInRange,
                checkOverrideOnExpire = true,
                expireRequiresEnemiesNearby = true
            };
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat, (Thing x) => x is Pawn && x.def.race.intelligence >= Intelligence.ToolUser, 0f, 9999f, default(IntVec3), 3.40282347E+38f, true);
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

        private bool HasRangedVerb(Pawn pawn)
        {
            this.verbs = pawn.VerbProperties;

            for (int i = 0; i < verbs.Count; i++)
            {
                if (verbs[i].range > 1.1f) return true;
            }
            return false;
        }

        protected virtual void UpdateEnemyTarget(Pawn pawn)
        {
            Thing thing = pawn.mindState.enemyTarget;
            if (thing != null && (thing.Destroyed || Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn) || (float)(pawn.Position - thing.Position).LengthHorizontalSquared > this.targetKeepRadius * this.targetKeepRadius || ((IAttackTarget)thing).ThreatDisabled()))
            {
                thing = null;
            }
            if (thing == null)
            {
                thing = this.FindAttackTargetIfPossible(pawn);
                if (thing != null)
                {
                    //pawn.mindState.Notify_EngagedTarget();
                    Lord lord = pawn.GetLord();
                    if (lord != null)
                    {
                        lord.Notify_PawnAcquiredTarget(pawn, thing);
                    }
                }
            }
            else
            {
                Thing thing2 = this.FindAttackTargetIfPossible(pawn);
                if (thing2 == null && !this.chaseTarget)
                {
                    thing = null;
                }
                else if (thing2 != null && thing2 != thing)
                {
                    //pawn.mindState.Notify_EngagedTarget();
                    thing = thing2;
                }
            }
            pawn.mindState.enemyTarget = thing;
            Pawn pawn2 = thing as Pawn;
            if (pawn2 != null && pawn2.Faction == Faction.OfPlayer && pawn.Position.InHorDistOf(pawn2.Position, 30f))
            {
                Find.TickManager.slower.SignalForceNormalSpeed();
            }
        }

        private Thing FindAttackTargetIfPossible(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(!pawn.IsColonist) == null)
            {
                return null;
            }
            return this.FindAttackTarget(pawn);
        }

        protected virtual Thing FindAttackTarget(Pawn pawn)
        {
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat;
            if (this.needLOSToAcquireNonPawnTargets)
            {
                targetScanFlags |= TargetScanFlags.NeedLOSToNonPawns;
            }
            if (this.PrimaryVerbIsIncendiary(pawn))
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, (Thing x) => this.ExtraTargetValidator(pawn, x), 0f, this.targetAcquireRadius, this.GetFlagPosition(pawn), this.GetFlagRadius(pawn), false);
        }

        private bool PrimaryVerbIsIncendiary(Pawn pawn)
        {
            if (pawn.equipment != null && pawn.equipment.Primary != null)
            {
                List<VerbProperties> verbs = pawn.equipment.Primary.def.Verbs;
                for (int i = 0; i < verbs.Count; i++)
                {
                    if (verbs[i].isPrimary)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
