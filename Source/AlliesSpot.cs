using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI.Group;
using RimWorld;
using HarmonyLib;

namespace AlliesSpot
{
    public class AlliesSpot : Building
    {
        private bool ticked = false;

        public AlliesSpot()
        {
            if (Current.Game.CurrentMap != null)
            {
                foreach (Building building in Current.Game.CurrentMap.listerBuildings.allBuildingsColonist.FindAll(x => x.def == AlliesSpotDefOf.AlliesSpot))
                {
                    building.Destroy(DestroyMode.Vanish);
                }
            }
        }

        public override void Tick()
        {
            if (!ticked)
            {
                foreach (var lord in Current.Game.CurrentMap.lordManager.lords.FindAll(x => x.LordJob is LordJob_AssistColony))
                {
                    if (lord.CurLordToil is LordToil_HuntEnemies)
                    {
                        var data = fieldInfoData.GetValue(lord.CurLordToil) as LordToilData_HuntEnemies;
                        data.fallbackLocation = this.Position;

                        lord.CurLordToil.UpdateAllDuties();
                    }
                }

                ticked = true;
            }
        }

        private static FieldInfo fieldInfoData = AccessTools.Field(typeof(LordToil_HuntEnemies), "data");
    }
}
