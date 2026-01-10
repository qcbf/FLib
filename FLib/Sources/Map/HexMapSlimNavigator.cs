//==================={By Qcbf|qcbf@qq.com|2/17/2022 5:26:53 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib
{
    public unsafe struct HexMapSlimNavigator
    {


        public struct OpenData
        {
            public OpenData* ParentPtr;
            public int PosIndex;
            public ushort F;
            public ushort G;
        }

        //public static void Navigate(HexMap map, in HexMapPos from, in HexMapPos to, ref SlimList<int> results, bool isAlwayFillResult = true)
        //{
        //    var count = Math.Min(MathEx.GetNextPowerOfTwo(map.Tiles.Length), 8192);
        //    var closeList = new StackHashSet<OpenData>(stackalloc ushort[count], stackalloc StackHashSet<OpenData>.Entry[count]);
        //    var openList = new SlimList<OpenData>(stackalloc OpenData[count]);
        //    Span<HexMapPos> nearestPositions = stackalloc HexMapPos[6];

        //    openList.Add(new OpenData { ParentPtr = default, PosIndex = map.PosToIndex(from), G = 0, F = (ushort)HexMap.Distance(from, to) });
        //    for (int i = 0; i < count && openList.Count > 0; i++)
        //    {
        //        var openIndex = openList.Count - 1;
        //        OpenData* parent = default;
        //        fixed (OpenData* ptr = &closeList.Entries[closeList.Add(openList[openIndex])].Value) parent = ptr;
        //        openList.RemoveAt(openIndex);
        //        map.GetNearestPos(map.IndexToPos(parent->PosIndex), ref nearestPositions);
        //        for (int j = 0; j < nearestPositions.Length; j++)
        //        {
        //            if (!map.IsValid(nearestPositions[i])) continue;
        //            else if (nearestPositions[j] == to)
        //            {
        //                FillResult(ref results, parent);
        //                return;
        //            }

        //            var open = new OpenData
        //            {
        //                ParentPtr = parent,
        //                PosIndex = map.PosToIndex(nearestPositions[j]),
        //                G = (ushort)(parent->G + 1),
        //            };
        //            open.F = (ushort)(HexMap.Distance(nearestPositions[j], to) + open.G);
        //            var index = 0;
        //            for (int k = openList.Count - 1; k >= 0; k--)
        //            {
        //                if (openList[k].F <= open.F)
        //                {
        //                    index = k;
        //                    break;
        //                }
        //            }
        //            openList.Insert(index, open);
        //        }
        //    }

        //    if (isAlwayFillResult)
        //    {
        //        // fallback
        //        var minH = ushort.MaxValue;
        //        var foundIndex = -1;
        //        for (int i = 0; i < openList.Count; i++)
        //        {
        //            var h = (ushort)(openList[i].F - openList[i].G);
        //            if (h < minH)
        //            {
        //                minH = h;
        //                foundIndex = i;
        //            }
        //        }
        //        OpenData* open;
        //        fixed (OpenData* ptr = &openList[foundIndex]) open = ptr;
        //        FillResult(ref results, open);
        //    }

        //}

        //private static void FillResult(ref SlimList<int> results, OpenData* open)
        //{
        //    results.Add(open->PosIndex);
        //    while (open->ParentPtr != default)
        //    {
        //        results.Add(open->PosIndex);
        //        open = open->ParentPtr;
        //    }
        //}

    }
}
