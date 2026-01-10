//==================={By Qcbf|qcbf@qq.com|10/14/2022 10:56:47 AM}===================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using FLib;

namespace FLib
{
    public class QuadMap : IBytesSerializable
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly FVector2Int[] NearestEightPositions = { new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0), new(-1, -1), new(0, -1), new(1, -1) };

        /// <summary>
        /// 
        /// </summary>
        public FVector2 Offset;

        /// <summary>
        /// 
        /// </summary>
        public FNum TileSize = FNum.One;

        /// <summary>
        /// 
        /// </summary>
        public FVector2Int TerrainSize;

        /// <summary>
        /// 
        /// </summary>
        public BitArray[] Terrains;

        /// <summary>
        /// 
        /// </summary>
        public int LayerCount => (Terrains?.Length).GetValueOrDefault();

        /// <summary>
        /// 
        /// </summary>
        public FVector2 WorldSize => new(TerrainSize.X * TileSize, TerrainSize.Y * TileSize);

        /// <summary>
        /// 
        /// </summary>
        public FRect WorldRect => new(Offset, Offset + WorldSize);

        /// <summary>
        /// 
        /// </summary>
        public bool this[int layer, in FVector2Int pos]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this[layer, PosToIdx(pos)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[layer, PosToIdx(pos)] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool this[int layer, in int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this[layer, PosToIdx(new FVector2Int(x, y))];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[layer, PosToIdx(new FVector2Int(x, y))] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool this[int layer, int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Terrains[layer][index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Terrains[layer][index] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector2 MapToWorldPos(in FVector2Int pos)
        {
            var half = TileSize * FNum.OneHalf;
            return new FVector2(pos.X * TileSize + half + Offset.X, pos.Y * TileSize + half + Offset.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector2Int WorldToMapPos(FVector2 pos) => new((int)FNum.Floor((pos.X - Offset.X) / TileSize), (int)FNum.Floor((pos.Y - Offset.Y) / TileSize));

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector2Int IdxToPos(int index) => new() { X = index % TerrainSize.X, Y = index / TerrainSize.X };

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PosToIdx(in FVector2Int pos) => pos.Y * TerrainSize.X + pos.X;

        /// <summary>
        /// 
        /// </summary>
        public virtual QuadMap SetLayers(int count)
        {
            Array.Resize(ref Terrains, count);
            var tileCount = TerrainSize.X * TerrainSize.Y;
            for (var i = 0; i < LayerCount; i++)
                Terrains[i] = new BitArray(tileCount);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual QuadMap SetSize(in FVector2Int size, int layerCount = -1)
        {
            var w = Math.Min(TerrainSize.X, size.X);
            var h = Math.Min(TerrainSize.Y, size.Y);
            TerrainSize = size;
            if (layerCount > 0)
                Array.Resize(ref Terrains, layerCount);
            for (var i = 0; i < LayerCount; i++)
            {
                var oldTerrain = Terrains[i];
                Terrains[i] = new BitArray(size.X * size.Y);
                if (oldTerrain != null)
                {
                    for (var y = 0; y < h; y++)
                    {
                        for (var x = 0; x < w; x++)
                            Terrains[i].Set(y * size.X + x, oldTerrain[y * w + x]);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckTile(in FVector2Int pos) => pos.X >= 0 && pos.Y >= 0 && pos.X < TerrainSize.X && pos.Y < TerrainSize.Y;

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckTile(int layer, in FVector2Int pos, bool value) => CheckTile(pos) && Terrains[layer][PosToIdx(pos)] == value;

        /// <summary>
        /// 
        /// </summary>
        public bool CheckTile(int layer, in FVector2Int pos, FVector2Int size, bool value)
        {
            for (var y = 0; y < size.Y; y++)
            {
                for (var x = 0; x < size.X; x++)
                {
                    if (!CheckTile(layer, new FVector2Int(pos.X + x, pos.Y + y), value))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool TryFindNearPos(int layer, FVector2Int pos, FVector2Int size, out FVector2Int o, int findMaxDist = 0, bool value = false, Func<QuadMap, FVector2Int, bool> checker = null)
        {
            o = FindNearPos(layer, pos, size, findMaxDist, value, checker);
            return o == FVector2Int.None;
        }

        /// <summary>
        /// 
        /// </summary>
        public FVector2Int FindNearPos(int layer, FVector2Int pos, FVector2Int size, int findMaxDist = 0, bool value = false, Func<QuadMap, FVector2Int, bool> checker = null)
        {
            if (CheckTile(layer, pos, size, value) && checker?.Invoke(this, pos) != false)
                return pos;
            if (findMaxDist <= 0)
                findMaxDist = Math.Max(TerrainSize.X, TerrainSize.Y) / 2;
            else
                ++findMaxDist;
            for (var i = 1; i < findMaxDist; i++)
            {
                var from = pos - i;
                var to = pos + i;
                for (var x = -i; x <= i; x++)
                {
                    var mapPos = new FVector2Int(pos.X + x, from.Y);
                    if (CheckTile(layer, mapPos, size, value) && checker?.Invoke(this, mapPos) != false)
                        return mapPos;
                    mapPos.Y = to.Y;
                    if (CheckTile(layer, mapPos, size, value))
                        return mapPos;
                }

                for (var y = -i + 1; y < i; y++)
                {
                    var mapPos = new FVector2Int(from.X, pos.Y + y);
                    if (CheckTile(layer, mapPos, size, value) && checker?.Invoke(this, mapPos) != false)
                        return mapPos;
                    mapPos.X = to.X;
                    if (CheckTile(layer, mapPos, size, value) && checker?.Invoke(this, mapPos) != false)
                        return mapPos;
                }
            }
            return FVector2Int.None;
        }

        /// <summary>
        /// 
        /// </summary>
        public FVector2Int FindNearNextStepPos(int layer, in FVector2Int from, in FVector2Int to, bool value = false, Func<QuadMap, FVector2Int, bool> checker = null, HashSet<FVector2Int> blackPositions = null)
        {
            var segment = (FNum)45;
            var half = segment * FNum.OneHalf;
            var index = (int)((FVector2.Angle360(FVector2.Right, to - from) + half) / segment) % 8;
            var next = from + NearestEightPositions[index];
            if (CheckTile(layer, next, value) && checker?.Invoke(this, next) != false)
                return next;
            for (var i = 1; i < 4; i++)
            {
                next = from + NearestEightPositions[(index + i) % 8];
                if (CheckTile(layer, next, value) && blackPositions?.Contains(next) != true && checker?.Invoke(this, next) != false)
                    return next;
                next = from + NearestEightPositions[(index - i + 8) % 8];
                if (CheckTile(layer, next, value) && blackPositions?.Contains(next) != true && checker?.Invoke(this, next) != false)
                    return next;
            }
            next = from + NearestEightPositions[(index + 8) % 8];
            if (CheckTile(layer, next, value) && blackPositions?.Contains(next) != true && checker?.Invoke(this, next) != false)
                return next;
            return FVector2Int.None;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool TryBoundPos(int layer, bool value, ref FVector2 pos)
        {
            var worldRect = WorldRect;
            var result = false;
            if (pos.X < worldRect.Min.X)
            {
                pos.X = worldRect.Min.X;
                result = true;
            }
            else if (pos.X >= worldRect.Max.X)
            {
                pos.X = worldRect.Max.X - FNum.Thousandth;
                result = true;
            }
            if (pos.Y < worldRect.Min.Y)
            {
                pos.Y = worldRect.Min.Y;
                result = true;
            }
            else if (pos.Y >= worldRect.Max.Y)
            {
                pos.Y = worldRect.Max.Y - FNum.Thousandth;
                result = true;
            }
            var mapPos = WorldToMapPos(pos);
            if (!CheckTile(layer, mapPos, value))
            {
                var foundMapPos = FindNearPos(0, mapPos, FVector2Int.One);
                if (foundMapPos != FVector2Int.None)
                    pos = MapToWorldPos(foundMapPos);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        ///
        /// </summary>
        public string ToString(bool isVerbose)
        {
            if (Terrains == null)
                return string.Empty;
            if (!isVerbose)
                return $"layers:{Terrains.Length}|{string.Join(',', Terrains.Select(v => v.Length))}";

            var strbuf = new StringBuilder(TerrainSize.X * TerrainSize.Y * Terrains.Length);
            strbuf.AppendLine($"Layers {Terrains.Length}");
            for (var i = 0; i < Terrains.Length; i++)
            {
                for (var y = 0; y < TerrainSize.Y; y++)
                {
                    for (var x = 0; x < TerrainSize.X; x++)
                        strbuf.Append(Convert.ToInt32(this[i, x, y])).Append('\t');
                    strbuf.AppendLine();
                }
            }
            return strbuf.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        public int[][] ToIntArray(int[][] dest = null)
        {
            dest ??= new int[Terrains.Length][];
            for (var i = 0; i < Terrains.Length; i++)
            {
                dest[i] = new int[Terrains[i].Length / 32];
                Terrains[i].CopyTo(dest[i], 0);
            }
            return dest;
        }

        #region BytesSerializer
        public virtual void Z_BytesWrite(ref BytesWriter writer)
        {
            writer.Push(Offset);
            writer.Push(TileSize);
            writer.Push(TerrainSize);
            var len = (Terrains?.Length).GetValueOrDefault();
            writer.PushLength(len);
            if (len == 0)
                return;
            int[] tempArray = null;
            foreach (var terrain in Terrains!)
            {
                len = terrain.Length / 32;
                if ((tempArray?.Length).GetValueOrDefault() < len)
                    tempArray = new int[terrain.Length];
                terrain.CopyTo(tempArray!, 0);
                writer.Push(tempArray.AsSpan(0, len));
            }
        }

        public virtual void Z_BytesRead(ref BytesReader reader)
        {
            reader.Read(ref Offset);
            reader.Read(ref TileSize);
            reader.Read(ref TerrainSize);
            var terrainCount = reader.ReadLength();
            Terrains = new BitArray[terrainCount];
            for (var i = 0; i < terrainCount; i++)
            {
                var arr = reader.ReadArray<int>();
                Terrains[i] = new BitArray(arr);
            }
        }
        #endregion
    }

    public class QuadMap<TTileData> : QuadMap
    {
        public TTileData[][] TileDatas = Array.Empty<TTileData[]>();

        public override QuadMap SetSize(in FVector2Int size, int layerCount = -1)
        {
            base.SetSize(in size, layerCount);
            InitializeData();
            return this;
        }

        private void InitializeData()
        {
            TileDatas = new TTileData[LayerCount][];
            for (var i = 0; i < LayerCount; i++)
                TileDatas[i] = new TTileData[Terrains[i].Length];
        }

        public override void Z_BytesRead(ref BytesReader reader)
        {
            base.Z_BytesRead(ref reader);
            InitializeData();
        }

        public ref TTileData GetTileData(byte layer, FVector2Int pos) => ref TileDatas[layer][PosToIdx(pos)];
    }
}
