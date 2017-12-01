using Harmony;
using RimWorld;
using System.Reflection;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;




namespace DraftingPatcher
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = HarmonyInstance.Create("com.geneticrim");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

     
    }
    [HarmonyPatch(typeof(PawnComponentsUtility))]
    [HarmonyPatch("AddAndRemoveDynamicComponents")]
    public static class PawnComponentsUtility_AddAndRemoveDynamicComponents_Patch
    {
        [HarmonyPostfix]
        static void AddTameability(Pawn pawn)
        
        {
            //Log.Message(pawn.kindDef.ToString());
            bool flag = pawn.Faction != null && pawn.Faction.IsPlayer;
            if ((pawn.kindDef.ToString() == "GR_Bearodile")||(pawn.kindDef.ToString() == "GR_Boomsnake") && flag)
            {
                Log.Message("Patching "+ pawn.kindDef.ToString() + " with a draft controller and equipment tracker");
                pawn.drafter = new Pawn_DraftController(pawn);
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
            }
        }
    }

   

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("GetGizmos")]

    static class Pawn_GetGizmos_Patch
    {
        [HarmonyPostfix]
      
        public static void AddGizmo(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            var pawn = __instance;
            var gizmos = __result.ToList();
            bool flag = pawn.Faction != null && pawn.Faction.IsPlayer;

            if ((pawn.drafter != null) && ((pawn.kindDef.ToString() == "GR_Bearodile") || (pawn.kindDef.ToString() == "GR_Boomsnake")) && flag)
            {
                Command_Action mygizmo = new Command_Action();
                mygizmo.action = delegate
                {
                    pawn.drafter.Drafted= !pawn.drafter.Drafted;
                };
                mygizmo.defaultDesc = "CommandToggleDraftDesc".Translate();
                mygizmo.icon = TexCommand.Draft;
                gizmos.Insert(0, mygizmo);
            }

            if ((pawn.drafter != null) && ((pawn.kindDef.ToString() == "GR_Bearodile")) && flag && pawn.drafter.Drafted)
            {

                Command_Target command_Target = new Command_Target();
                command_Target.defaultLabel = "CommandMeleeAttack".Translate();
                command_Target.defaultDesc = "CommandMeleeAttackDesc".Translate();
                command_Target.targetingParams = TargetingParameters.ForAttackAny();
                command_Target.icon = TexCommand.AttackMelee;
               
                command_Target.action = delegate (Thing target)
                {
                    IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where(delegate (object x)
                    {
                        Pawn pawn2 = x as Pawn;
                        return pawn2 != null &&  pawn2.Drafted;
                    }).Cast<Pawn>();
                    foreach (Pawn current in enumerable)
                    {
                        Job job = new Job(JobDefOf.AttackMelee, target);
                        Pawn pawn2 = target as Pawn;
                        if (pawn2 != null)
                        {
                            job.killIncappedTarget = pawn2.Downed;
                        }
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                };
                gizmos.Insert(1, command_Target);

            }

            if ((pawn.drafter != null) && ((pawn.kindDef.ToString() == "GR_Boomsnake")) && flag)
            {
                Command_Action kamikaze = new Command_Action();
                kamikaze.action = delegate
                {

                    pawn.health.AddHediff(HediffDef.Named("GR_Kamikaze"));
                    HealthUtility.AdjustSeverity(pawn, HediffDef.Named("GR_Kamikaze"),1);
                };
                kamikaze.defaultDesc = "CommandToggleDraftDesc".Translate();
                kamikaze.icon = TexCommand.ReleaseAnimals;
                gizmos.Insert(1, kamikaze);
            }

            __result = gizmos;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch("CanTakeOrder")]
    public static class FloatMenuMakerMap_CanTakeOrder_Patch
    {
        [HarmonyPostfix]
        public static void MakePawnControllable(Pawn pawn, ref bool __result)

        {
            bool flag = pawn.Faction != null && pawn.Faction.IsPlayer;
            if (((pawn.kindDef.ToString() == "GR_Bearodile") || (pawn.kindDef.ToString() == "GR_Boomsnake")) && flag)
            {
                //Log.Message("You should be controllable now");
                __result = true;
            }
            
        }
    }









}
