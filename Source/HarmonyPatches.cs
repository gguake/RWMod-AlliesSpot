using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using HarmonyLib;

namespace AlliesSpot
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("rimworld.gguake.alliesspot");
            
            harmony.Patch(AccessTools.Method(typeof(LordToil_HuntEnemies), "UpdateAllDuties"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(LordToil_HuntEnemies_UpdateAllDutiesTranspiler)));
            
            harmony.Patch(AccessTools.Method(typeof(RaidStrategyWorker_ImmediateAttack), "MakeLordJob"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(RaidStrategyWorker_ImmediateAttack_MakeLordJobTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(AutoHomeAreaMaker), "Notify_BuildingSpawned"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(AutoHomeAreaMaker_Notify_BuildingSpawnedPrefix)));

            harmony.Patch(AccessTools.Method(typeof(PawnsArrivalModeWorker_EdgeDrop), "TryResolveRaidSpawnCenter"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(TryResolveRaidSpawnCenterPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnsArrivalModeWorker_CenterDrop), "TryResolveRaidSpawnCenter"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(TryResolveRaidSpawnCenterPostfix)));
        }

        public static IEnumerable<CodeInstruction> LordToil_HuntEnemies_UpdateAllDutiesTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (CodeInstruction inst in codeInstructions)
            {
                if (inst.opcode == OpCodes.Call && 
                    (MethodInfo)inst.operand == AccessTools.Method(typeof(RCellFinder), "TryFindRandomSpotJustOutsideColony", parameters: new System.Type[] { typeof(Pawn), typeof(IntVec3).MakeByRefType() }))
                {
                    inst.operand = AccessTools.Method(typeof(AlliesSpotFinder), "TryFindAlliesSpot",
                        parameters: new System.Type[] { typeof(Pawn), typeof(IntVec3).MakeByRefType() });
                }

                yield return inst;
            }
        }

        public static IEnumerable<CodeInstruction> RaidStrategyWorker_ImmediateAttack_MakeLordJobTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (CodeInstruction inst in codeInstructions)
            {
                if (inst.opcode == OpCodes.Call && 
                    (MethodInfo)inst.operand == AccessTools.Method(typeof(RCellFinder), "TryFindRandomSpotJustOutsideColony", parameters: new System.Type[] { typeof(IntVec3), typeof(Map), typeof(IntVec3).MakeByRefType() }))
                {
                    inst.operand = AccessTools.Method(typeof(AlliesSpotFinder), "TryFindAlliesSpot",
                        parameters: new System.Type[] { typeof(IntVec3), typeof(Map), typeof(IntVec3).MakeByRefType() });
                }

                yield return inst;
            }
        }

        public static bool AutoHomeAreaMaker_Notify_BuildingSpawnedPrefix(Thing b)
        {
            if (b is AlliesSpot)
            {
                return false;
            }

            return true;
        }

        public static void TryResolveRaidSpawnCenterPostfix(IncidentParms parms)
        {
            if (parms.raidStrategy == RaidStrategyDefOf.ImmediateAttackFriendly)
            {
                IntVec3 alliesSpot = IntVec3.Invalid;
                if (AlliesSpotFinder.TryFindAlliesSpot((Map)parms.target, out alliesSpot))
                {
                    IntVec3 replaceSpot = parms.spawnCenter;
                    if (DropCellFinder.TryFindDropSpotNear(alliesSpot, (Map)parms.target, out replaceSpot, false, false))
                    {
                        parms.spawnCenter = replaceSpot;
                    }
                }
            }
        }
    }
}
