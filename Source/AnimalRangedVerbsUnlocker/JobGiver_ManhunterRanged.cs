
using RimWorld;
using System.Collections.Generic;

using System.Linq;

using Verse;
using Verse.AI;


namespace AnimalRangedVerbsUnlocker
{
    public class JobGiver_ManhunterRanged : RimWorld.JobGiver_Manhunter
    {
        public List<VerbProperties> verbs;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(false) == null)
            {
                return null;
            }
            Thing pawn2 = this.FindPawnTarget(pawn);
            if (pawn2 != null && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false) && HasRangedVerb(pawn))
            {
                return this.RangedAttackJob(pawn, pawn2);
            } else

           
            if (pawn2 != null && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                return this.MeleeAttackJob(pawn, pawn2);
            }
            Thing building = this.FindTurretTarget(pawn);
            if (building != null)
            {
                return this.MeleeAttackJob(pawn, building);
            }
            if (pawn2 != null)
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
                {
                    Job result;
                    if (!pawnPath.Found)
                    {
                        result = null;
                        return result;
                    }
                    IntVec3 loc;
                    if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out loc))
                    {
                        Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + pawn2.LabelCap);
                        result = null;
                        return result;
                    }
                    IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map, RegionType.Set_Passable), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null, RegionType.Set_Passable).RandomCell;
                    if (randomCell == pawn.Position)
                    {
                        result = new Job(JobDefOf.Wait, 30, false);
                        return result;
                    }
                    result = new Job(JobDefOf.Goto, randomCell);
                    return result;
                }
            }
            return null;



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


        private bool TryFindShootPosition(Pawn pawn, Thing target, out IntVec3 dest) {

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

        private Verb TryGetRangedVerb(Pawn pawn) {
            Verb verb = pawn.verbTracker.AllVerbs.Where(v => v.verbProps.range > 1.1f).RandomElement();
            return verb;
        }

        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            return new Job(JobDefOf.AttackMelee, target)
            {
                maxNumMeleeAttacks = 1,
                expiryInterval = Rand.Range(420, 900),
                attackDoorIfTargetLost = true
            };
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

        private Building FindTurretTarget(Pawn pawn)
        {
            return (Building)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing t) => t is Building, 0f, 70f, default(IntVec3), 3.40282347E+38f, false);
        }
    }
}
