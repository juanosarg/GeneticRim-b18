using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using System.Diagnostics;
using Verse.AI.Group;

namespace DraftingPatcher
{
    [StaticConstructorOnStartup]
    class Main
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
            if ((pawn.kindDef.ToString() == "GR_Bearodile") && flag)
            {
                Log.Message("Patching Bearodile with a draft controller");
                pawn.drafter = new Pawn_DraftController(pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("GetGizmos")]
    public static class Pawn_GetGizmos_Patch
    {
        [HarmonyPostfix]
        static IEnumerable<Gizmo> GiveGizmo(Pawn pawn)

        {
            //Log.Message(pawn.kindDef.ToString());
            bool flag = pawn.Faction != null && pawn.Faction.IsPlayer;
            if ((pawn.kindDef.ToString() == "GR_Bearodile") && flag)
            {
                foreach (Gizmo c2 in this.drafter.GetGizmos())
                {
                    yield return c2;
                }
            }
        }
    }


}
