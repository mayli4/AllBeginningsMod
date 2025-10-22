using AllBeginningsMod.Common.World;
using AllBeginningsMod.Utilities;
using System;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace AllBeginningsMod.Content.Jungle;

internal class JungleBushes : GenPass {
    public JungleBushes(string name, double loadWeight) : base(name, loadWeight) { }

    private static bool ValidTileForBushGround(Tile tile) {
        return tile.HasTile && (tile.TileType == TileID.JungleGrass || tile.TileType == TileID.Mud);
    }

    private static bool Valid(int x) {
        for(int y = (int)(Main.worldSurface * 0.35f); y < Main.rockLayer; y++) {
            if(Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.JungleGrass)
                return true;
        }
        return false;
    }

    public override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        progress.Set(0f);

        var genNoise = new FastNoiseLite();
        genNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        genNoise.SetSeed(WorldGen.genRand.Next());

        for(int k = 60; k < Main.maxTilesX - 60; k++) {
            progress.Value = (float)(k - 60) / (Main.maxTilesX - 120);

            if(WorldGen.genRand.NextBool(10) && Valid(k)) {
                int size = WorldGen.genRand.Next(6, 12);

                for(int xOffsetWithinBush = 0; xOffsetWithinBush < size; xOffsetWithinBush++) {
                    int currentColumnX = k + xOffsetWithinBush;

                    int currentSurfaceY = -1;
                    for(int j = (int)(Main.worldSurface * 0.35f); j < Main.rockLayer; j++) {
                        if(ValidTileForBushGround(Main.tile[currentColumnX, j])) {
                            currentSurfaceY = j;
                            break;
                        }
                    }

                    if(currentSurfaceY == -1) {
                        continue;
                    }

                    int xOff = xOffsetWithinBush > size / 2 ? size - xOffsetWithinBush : xOffsetWithinBush;

                    float noisePre = genNoise.GetNoise(k * 10, currentColumnX * 10);
                    int noise = (int)(noisePre * 15);

                    int topYOfBushSegment = currentSurfaceY - Math.Min(xOff / 2 + noise + 5, 9);

                    int bushBottomY = topYOfBushSegment;
                    int maxDepthBelowSurface = 20;

                    for(int ySim = topYOfBushSegment; ; ySim++) {
                        if(ySim - topYOfBushSegment > maxDepthBelowSurface) {
                            break;
                        }
                        if(!WorldGen.InWorld(currentColumnX, ySim + 1) || (Main.tile[currentColumnX, ySim + 1].WallType != 0 && Main.tile[currentColumnX, ySim + 1].WallType != WallID.Jungle)) {
                            bushBottomY = ySim;
                            break;
                        }
                        bushBottomY = ySim;
                    }

                    if(bushBottomY < topYOfBushSegment) {
                        continue;
                    }

                    int actualBushHeight = bushBottomY - topYOfBushSegment + 1;

                    if(actualBushHeight > 0) {
                        WorldUtils.Gen(
                            new Point(currentColumnX, topYOfBushSegment),
                            new Shapes.Rectangle(1, actualBushHeight),
                            Actions.Chain(
                                new Actions.RemoveWall(),
                                new Actions.PlaceWall(WallID.Jungle, true),
                                new Actions.SetFrames(true)
                            )
                        );
                    }
                }
            }
        }
    }
}