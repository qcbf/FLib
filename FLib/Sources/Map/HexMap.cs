////==================={By Qcbf|qcbf@qq.com|1/11/2022 9:28:03 PM}===================

//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System.Text;

//namespace FLib
//{
//    public class HexMap
//    {
//        public HexMapTileSize TileSize = HexMapTileSize.Default;
//        public byte[] Tiles = Array.Empty<byte>();


//        public ref byte this[in FVector2Int pos]
//        {
//            get => ref Tiles[PosToIndex(pos)];
//        }
//        public ref byte this[int x, int y]
//        {
//            get => ref Tiles[PosToIndex(new FVector2Int(x, y))];
//        }


//        public int Width
//        {
//            get;
//            private set;
//        }

//        public int Height
//        {
//            get;
//            private set;
//        }


//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public FVector2Int IndexToPos(int index) => new FVector2Int { X = index % Width, Y = index / Width };
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public int PosToIndex(in FVector2Int pos) => pos.Y * Width + pos.X;

//        /// <summary>
//        /// 
//        /// </summary>
//        public HexMap SetData(int width, byte[] tiles)
//        {
//            Width = width;
//            Height = tiles.Length / width;
//            Tiles = (byte[])tiles.Clone();
//            return this;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public HexMap SetSize(int newWidth, int newHeight)
//        {
//            if (Tiles == null)
//            {
//                Tiles = new byte[(Width = newWidth) * newHeight];
//                Height = Tiles.Length / Width;
//            }
//            else
//            {
//                var temp = new byte[newWidth * newHeight];
//                var copyHeight = Math.Min(newHeight, Height);
//                var copyWidth = Math.Min(newWidth, Width);
//                for (int y = 0; y < copyHeight; y++)
//                {
//                    for (int x = 0; x < copyWidth; x++)
//                    {
//                        if (y < Height && x < Width)
//                        {
//                            temp[y * newWidth + x] = Tiles[y * Width + x];
//                        }
//                    }
//                }
//                Tiles = temp;
//                Width = newWidth;
//                Height = newHeight;
//            }
//            return this;
//        }

//        /// <summary>
//        /// 检查位置是否等于指定状态
//        /// </summary>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public bool IsValid(in FVector2Int pos, byte flags, byte size)
//        {
//            if (size <= 1)
//            {
//                return IsValid(pos, flags);
//            }
//            else if (size == 2)
//            {
//                var cubePos = new HexMapCubePos(pos);
//                return IsValid(new HexMapCubePos(cubePos.X + 1, cubePos.Y, cubePos.Z - 1), flags) &&
//                    IsValid(new HexMapCubePos(cubePos.X + 1, cubePos.Y - 1, cubePos.Z), flags) &&
//                    IsValid(new HexMapCubePos(cubePos.X, cubePos.Y - 1, cubePos.Z + 1), flags) &&
//                    IsValid(new HexMapCubePos(cubePos.X - 1, cubePos.Y, cubePos.Z + 1), flags) &&
//                    IsValid(new HexMapCubePos(cubePos.X - 1, cubePos.Y + 1, cubePos.Z), flags) &&
//                    IsValid(new HexMapCubePos(cubePos.X, cubePos.Y + 1, cubePos.Z - 1), flags);
//            }
//            else
//            {
//                var cubePos = new HexMapCubePos(pos);
//                for (int x = -size; x < size; x++)
//                {
//                    for (int y = -size; y < size; y++)
//                    {
//                        for (int z = -size; z < size; z++)
//                        {
//                            var tempPos = (FVector2Int)new HexMapCubePos(cubePos.X + x, cubePos.Y + y, cubePos.Z + z);
//                            if (!IsValid(tempPos, flags))
//                            {
//                                return false;
//                            }
//                        }
//                    }
//                }
//                return true;
//            }
//        }

//        /// <summary>
//        /// 检查位置是否等于指定状态
//        /// </summary>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public bool IsValid(in FVector2Int pos, byte flags)
//        {
//            var index = PosToIndex(pos);
//            return IsValid(pos) && ((flags == 0 && Tiles[index] == 0) || (Tiles[index] & flags) != 0);
//        }

//        /// <summary>
//        /// 检查位置是否等于指定状态
//        /// </summary>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public bool IsValid(in FVector2Int pos)
//        {
//            return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
//        }

//        /// <summary>
//        /// 获取最近六个位置
//        /// </summary>
//        public void GetNearestPos(in FVector2Int pos, ref Span<FVector2Int> result)
//        {
//            var cubePos = new HexMapCubePos(pos);
//            result[0] = new HexMapCubePos(cubePos.X + 1, cubePos.Y, cubePos.Z - 1);
//            result[1] = new HexMapCubePos(cubePos.X + 1, cubePos.Y - 1, cubePos.Z);
//            result[2] = new HexMapCubePos(cubePos.X, cubePos.Y - 1, cubePos.Z + 1);
//            result[3] = new HexMapCubePos(cubePos.X - 1, cubePos.Y, cubePos.Z + 1);
//            result[4] = new HexMapCubePos(cubePos.X - 1, cubePos.Y + 1, cubePos.Z);
//            result[5] = new HexMapCubePos(cubePos.X, cubePos.Y + 1, cubePos.Z - 1);
//        }

//        /// <summary>
//        /// 寻找最近一个可用位置
//        /// </summary>
//        public FVector2Int? FindNearValidPos(in FVector2Int pos, byte flags)
//        {
//            if (IsValid(pos, flags)) return pos;
//            var cubePos = new HexMapCubePos(pos);
//            var newPos = (FVector2Int)new HexMapCubePos(cubePos.X + 1, cubePos.Y, cubePos.Z - 1);
//            if (IsValid(newPos, flags)) return newPos;
//            newPos = new HexMapCubePos(cubePos.X + 1, cubePos.Y - 1, cubePos.Z);
//            if (IsValid(newPos, flags)) return newPos;
//            newPos = new HexMapCubePos(cubePos.X, cubePos.Y - 1, cubePos.Z + 1);
//            if (IsValid(newPos, flags)) return newPos;
//            newPos = new HexMapCubePos(cubePos.X - 1, cubePos.Y, cubePos.Z + 1);
//            if (IsValid(newPos, flags)) return newPos;
//            newPos = new HexMapCubePos(cubePos.X - 1, cubePos.Y + 1, cubePos.Z);
//            if (IsValid(newPos, flags)) return newPos;
//            newPos = new HexMapCubePos(cubePos.X, cubePos.Y + 1, cubePos.Z - 1);
//            if (IsValid(newPos, flags)) return newPos;

//            for (int i = 0; i < Tiles.Length; i++)
//            {
//                for (int y = i; y >= -i; y--)
//                {
//                    for (int x = i; x >= -i; x--)
//                    {
//                        var temp = new FVector2Int(pos.X + x, pos.Y + y);
//                        if (IsValid(temp, flags)) return temp;
//                    }
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// 获取到终点下一步最近的有效位置
//        /// </summary>
//        public FVector2Int? GetNextStepValidPos(in FVector2Int from, in FVector2Int to, byte flags, HashSet<FVector2Int> blackPoints = null)
//        {
//            Span<FVector2Int> nearestPositions = stackalloc FVector2Int[6];
//            GetNearestPos(from, ref nearestPositions);
//            var dist = int.MaxValue;
//            var posIndex = -1;
//            var toWorldPos = MapToWorldPos(to);
//            for (int i = 0; i < nearestPositions.Length; i++)
//            {
//                if (IsValid(nearestPositions[i], flags) && blackPoints?.Contains(nearestPositions[i]) != true)
//                {
//                    var temp = Distance(nearestPositions[i], to);
//                    if (temp < dist)
//                    {
//                        dist = temp;
//                        posIndex = i;
//                    }
//                }
//            }
//            if (posIndex >= 0)
//            {
//                return nearestPositions[posIndex];
//            }
//            return null;
//        }

//        /// <summary>
//        /// 地图坐标到世界坐标
//        /// </summary>
//        public FVector2 MapToWorldPos(in FVector2Int pos)
//        {
//            var v = new FVector2(pos.X * TileSize.InnerRadius_x2, pos.Y * TileSize.RadiusMulOneHalf);
//            if ((pos.Y & 1) == 1) v.X += TileSize.InnerRadius;
//            return v;
//        }

//        /// <summary>
//        /// 世界坐标到地图坐标
//        /// </summary>
//        public FVector2Int WorldToMapPos(in FVector2 pos)
//        {
//            var mapY = (int)Math.Round(pos.Y / TileSize.RadiusMulOneHalf);
//            var x = pos.X;
//            if ((mapY & 1) == 1) x -= TileSize.InnerRadius;
//            var mapX = (int)Math.Round(x / TileSize.InnerRadius_x2);
//            return new FVector2Int(mapX, mapY);
//        }

//        /// <summary>
//        /// 获取距离
//        /// </summary>
//        public static int Distance(in FVector2Int from, in FVector2Int to)
//        {
//            var f = new HexMapCubePos(from);
//            var t = new HexMapCubePos(to);
//            return Math.Max(Math.Max(Math.Abs(f.X - t.X), Math.Abs(f.Y - t.Y)), Math.Abs(f.Z - t.Z));
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public string GetFormatString()
//        {
//            var strbuf = new StringBuilder();
//            var height = Tiles.Length / Width;
//            for (short y = 0; y < height; y++)
//            {
//                for (short x = 0; x < Width; x++)
//                {
//                    strbuf.Append('[').Append(this[x, y]).Append(x).Append('>').Append(y).Append(',').Append(']');
//                }
//                strbuf.AppendLine();
//            }
//            return strbuf.ToString();
//        }

//    }
//}
