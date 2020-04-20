using System.Linq;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace AlliesSpot
{
    public static class AlliesSpotFinder
    {
        public static bool TryFindAlliesSpot(Map map, out IntVec3 result)
        {
            if (map == null)
            {
                result = IntVec3.Invalid;
                return false;
            }

            foreach (Building spot in map.listerBuildings.allBuildingsColonist.Where(x => x.def == AlliesSpotDefOf.AlliesSpot))
            {
                result = spot.Position;
                return true;
            }

            result = IntVec3.Invalid;
            return false;
        }

        public static bool TryFindAlliesSpot(IntVec3 originCell, Map map, out IntVec3 result)
        {
            return TryFindAlliesSpot(originCell, map, null, out result);
        }

        public static bool TryFindAlliesSpot(Pawn searcher, out IntVec3 result)
        {
            return TryFindAlliesSpot(searcher.Position, searcher.Map, searcher, out result);
        }

        public static bool TryFindAlliesSpot(IntVec3 root, Map map, Pawn searcher, out IntVec3 result)
        {
            if (map == null)
            {
                result = IntVec3.Invalid;
                return false;
            }

            foreach (Building spot in map.listerBuildings.allBuildingsColonist.Where(x => x.def == AlliesSpotDefOf.AlliesSpot))
            {
                result = spot.Position;
                return true;
            }

            return RCellFinder.TryFindRandomSpotJustOutsideColony(root, map, searcher, out result);
        }
    }
}
