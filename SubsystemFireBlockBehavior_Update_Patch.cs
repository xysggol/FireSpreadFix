using System;
using System.Collections.Generic;
using Engine;
using HarmonyLib;

namespace Game {
    // 修复原版火焰蔓延会穿过不可燃方块点燃另一侧可燃方块的 Bug。
    // 原版 SubsystemFireBlockBehavior.Update 中的远距离蔓延（m_expansionProbabilities）
    // 会在没有任何阻挡检测的情况下直接对目标调用 SetCellOnFire，导致火焰可以「隔墙传火」。
    // 这里完整接管 Update，仅在火焰格与目标可燃格之间存在不被实心方块阻挡的连通路径时才允许蔓延。
    [HarmonyPatch(typeof(SubsystemFireBlockBehavior), nameof(SubsystemFireBlockBehavior.Update))]
    internal static class SubsystemFireBlockBehaviorUpdatePatch {
        static bool Prefix(SubsystemFireBlockBehavior __instance, float dt) {
            try {
                UpdateFire(__instance, dt);
            }
            catch (Exception e) {
                Log.Error($"[FireSpreadFix] Fire update failed: {e}");
            }
            return false;
        }

        static void UpdateFire(SubsystemFireBlockBehavior instance, float dt) {
            if (instance.m_firePointsCopy.Count == 0) {
                instance.m_firePointsCopy.Count += instance.m_fireData.Count;
                instance.m_fireData.Keys.CopyTo(instance.m_firePointsCopy.Array, 0);
                instance.m_copyIndex = 0;
                instance.m_lastScanDuration = (float)(instance.m_subsystemTime.GameTime - instance.m_lastScanTime);
                instance.m_lastScanTime = instance.m_subsystemTime.GameTime;
                if (instance.m_firePointsCopy.Count == 0) {
                    instance.m_fireSoundVolume = 0f;
                }
            }
            if (instance.m_firePointsCopy.Count > 0) {
                float num = MathUtils.Min(1f * dt * instance.m_firePointsCopy.Count + instance.m_remainderToScan, 50f);
                int num2 = (int)num;
                instance.m_remainderToScan = num - num2;
                int num3 = MathUtils.Min(instance.m_copyIndex + num2, instance.m_firePointsCopy.Count);
                while (instance.m_copyIndex < num3) {
                    if (instance.m_fireData.TryGetValue(
                            instance.m_firePointsCopy.Array[instance.m_copyIndex],
                            out SubsystemFireBlockBehavior.FireData value
                        )) {
                        int x = value.Point.X;
                        int y = value.Point.Y;
                        int z = value.Point.Z;
                        int num4 = Terrain.ExtractData(instance.SubsystemTerrain.Terrain.GetCellValue(x, y, z));
                        instance.m_fireSoundIntensity += 1f
                            / (instance.m_subsystemAudio.CalculateListenerDistanceSquared(new Vector3(x, y, z)) + 0.01f);
                        if ((num4 & 1) != 0) {
                            value.Time0 -= instance.m_lastScanDuration;
                            if (value.Time0 <= 0f) {
                                instance.QueueBurnAway(x, y, z + 1, value.FireExpandability * 0.85f);
                            }
                            foreach (KeyValuePair<Point3, float> expansionProbability in instance.m_expansionProbabilities) {
                                int tx = x + expansionProbability.Key.X;
                                int ty = y + expansionProbability.Key.Y;
                                int tz = z + 1 + expansionProbability.Key.Z;
                                if (instance.m_random.Float(0f, 1f)
                                    < expansionProbability.Value * instance.m_lastScanDuration * value.FireExpandability
                                    && IsFireExpansionAllowed(instance.SubsystemTerrain, x, y, z, tx, ty, tz)) {
                                    instance.m_toExpand[new Point3(tx, ty, tz)] = value.FireExpandability * 0.85f;
                                }
                            }
                        }
                        if ((num4 & 2) != 0) {
                            value.Time1 -= instance.m_lastScanDuration;
                            if (value.Time1 <= 0f) {
                                instance.QueueBurnAway(x + 1, y, z, value.FireExpandability * 0.85f);
                            }
                            foreach (KeyValuePair<Point3, float> expansionProbability2 in instance.m_expansionProbabilities) {
                                int tx = x + 1 + expansionProbability2.Key.X;
                                int ty = y + expansionProbability2.Key.Y;
                                int tz = z + expansionProbability2.Key.Z;
                                if (instance.m_random.Float(0f, 1f)
                                    < expansionProbability2.Value * instance.m_lastScanDuration * value.FireExpandability
                                    && IsFireExpansionAllowed(instance.SubsystemTerrain, x, y, z, tx, ty, tz)) {
                                    instance.m_toExpand[new Point3(tx, ty, tz)] = value.FireExpandability * 0.85f;
                                }
                            }
                        }
                        if ((num4 & 4) != 0) {
                            value.Time2 -= instance.m_lastScanDuration;
                            if (value.Time2 <= 0f) {
                                instance.QueueBurnAway(x, y, z - 1, value.FireExpandability * 0.85f);
                            }
                            foreach (KeyValuePair<Point3, float> expansionProbability3 in instance.m_expansionProbabilities) {
                                int tx = x + expansionProbability3.Key.X;
                                int ty = y + expansionProbability3.Key.Y;
                                int tz = z - 1 + expansionProbability3.Key.Z;
                                if (instance.m_random.Float(0f, 1f)
                                    < expansionProbability3.Value * instance.m_lastScanDuration * value.FireExpandability
                                    && IsFireExpansionAllowed(instance.SubsystemTerrain, x, y, z, tx, ty, tz)) {
                                    instance.m_toExpand[new Point3(tx, ty, tz)] = value.FireExpandability * 0.85f;
                                }
                            }
                        }
                        if ((num4 & 8) != 0) {
                            value.Time3 -= instance.m_lastScanDuration;
                            if (value.Time3 <= 0f) {
                                instance.QueueBurnAway(x - 1, y, z, value.FireExpandability * 0.85f);
                            }
                            foreach (KeyValuePair<Point3, float> expansionProbability4 in instance.m_expansionProbabilities) {
                                int tx = x - 1 + expansionProbability4.Key.X;
                                int ty = y + expansionProbability4.Key.Y;
                                int tz = z + expansionProbability4.Key.Z;
                                if (instance.m_random.Float(0f, 1f)
                                    < expansionProbability4.Value * instance.m_lastScanDuration * value.FireExpandability
                                    && IsFireExpansionAllowed(instance.SubsystemTerrain, x, y, z, tx, ty, tz)) {
                                    instance.m_toExpand[new Point3(tx, ty, tz)] = value.FireExpandability * 0.85f;
                                }
                            }
                        }
                        if (num4 == 0) {
                            value.Time5 -= instance.m_lastScanDuration;
                            if (value.Time5 <= 0f) {
                                instance.QueueBurnAway(x, y - 1, z, value.FireExpandability * 0.85f);
                            }
                        }
                    }
                    instance.m_copyIndex++;
                }
                if (instance.m_copyIndex >= instance.m_firePointsCopy.Count) {
                    instance.m_fireSoundVolume = 0.75f * instance.m_fireSoundIntensity;
                    instance.m_firePointsCopy.Clear();
                    instance.m_fireSoundIntensity = 0f;
                }
            }
            if (instance.m_subsystemTime.PeriodicGameTimeEvent(5.0, 0.0)) {
                int num5 = 0;
                int num6 = 0;
                foreach (KeyValuePair<Point3, float> item in instance.m_toBurnAway) {
                    Point3 key = item.Key;
                    float value2 = item.Value;
                    instance.SubsystemTerrain.ChangeCell(key.X, key.Y, key.Z, Terrain.ReplaceContents(0));
                    if (value2 > 0.25f) {
                        for (int i = 0; i < 5; i++) {
                            Point3 point = CellFace.FaceToPoint3(i);
                            instance.SetCellOnFire(key.X + point.X, key.Y + point.Y, key.Z + point.Z, value2);
                        }
                    }
                    float num7 = instance.m_subsystemViews.CalculateDistanceFromNearestView(new Vector3(key));
                    if (num5 < 15
                        && num7 < 24f) {
                        instance.m_subsystemParticles.AddParticleSystem(
                            new BurntDebrisParticleSystem(instance.SubsystemTerrain, key.X, key.Y, key.Z)
                        );
                        num5++;
                    }
                    if (num6 < 4
                        && num7 < 16f) {
                        instance.m_subsystemAudio.PlayRandomSound(
                            "Audio/Sizzles",
                            1f,
                            instance.m_random.Float(-0.25f, 0.25f),
                            new Vector3(key.X, key.Y, key.Z),
                            3f,
                            true
                        );
                        num6++;
                    }
                }
                foreach (KeyValuePair<Point3, float> item2 in instance.m_toExpand) {
                    instance.SetCellOnFire(item2.Key.X, item2.Key.Y, item2.Key.Z, item2.Value);
                }
                instance.m_toBurnAway.Clear();
                instance.m_toExpand.Clear();
            }
            instance.m_subsystemAmbientSounds.FireSoundVolume = MathUtils.Max(
                instance.m_subsystemAmbientSounds.FireSoundVolume,
                instance.m_fireSoundVolume
            );
        }

        // 火焰能否从 from 格蔓延到 to 格：
        // 要求存在一条由可通行方格（空气/火焰/61）组成、步数不超过两者欧氏距离向上取整的连通路径。
        // 这样既能阻止「隔墙传火」，也能正确阻止斜角处被实心方块封死的拐角穿墙，
        // 同时保留合法的隔空/斜向/向上蔓延（例如绕过方块顶部）。
        static bool IsFireExpansionAllowed(SubsystemTerrain terrain, int fromX, int fromY, int fromZ, int toX, int toY, int toZ) {
            Terrain worldTerrain = terrain.Terrain;
            return IsFireExpansionAllowed(
                (x, y, z) => worldTerrain.GetCellValue(x, y, z),
                fromX,
                fromY,
                fromZ,
                toX,
                toY,
                toZ
            );
        }

        internal static bool IsFireExpansionAllowed(
            Func<int, int, int, int> getCellValue,
            int fromX,
            int fromY,
            int fromZ,
            int toX,
            int toY,
            int toZ
        ) {
            int dx = toX - fromX;
            int dy = toY - fromY;
            int dz = toZ - fromZ;
            int distanceSquared = dx * dx + dy * dy + dz * dz;
            int maxSteps = (int)Math.Ceiling(Math.Sqrt(distanceSquared));
            if (maxSteps <= 0) {
                return false;
            }
            return CanReachTarget(getCellValue, fromX, fromY, fromZ, toX, toY, toZ, maxSteps);
        }

        static bool CanReachTarget(
            Func<int, int, int, int> getCellValue,
            int x,
            int y,
            int z,
            int targetX,
            int targetY,
            int targetZ,
            int stepsLeft
        ) {
            if (x == targetX
                && y == targetY
                && z == targetZ) {
                return true;
            }
            if (stepsLeft <= 0) {
                return false;
            }
            for (int i = 0; i < 6; i++) {
                Point3 point = CellFace.FaceToPoint3(i);
                int nx = x + point.X;
                int ny = y + point.Y;
                int nz = z + point.Z;
                if (nx == targetX
                    && ny == targetY
                    && nz == targetZ) {
                    return true;
                }
                if (!IsFirePassableCell(getCellValue, nx, ny, nz)) {
                    continue;
                }
                if (CanReachTarget(getCellValue, nx, ny, nz, targetX, targetY, targetZ, stepsLeft - 1)) {
                    return true;
                }
            }
            return false;
        }

        static bool IsFirePassableCell(Func<int, int, int, int> getCellValue, int x, int y, int z) {
            int contents = Terrain.ExtractContents(getCellValue(x, y, z));
            return contents == 0
                || contents == 104
                || contents == 61;
        }
    }
}
