using AllBeginningsMod.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace AllBeginningsMod.Content.World;

//todo reduce magic numbers

internal sealed class AbandonedShackSystem : ModSystem {
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int surfacePassIndex = tasks.FindIndex(pass => pass.Name == "Vines");
        if (surfacePassIndex != -1) {
            tasks.Add(new PassLegacy("CarvePit", CarvePit));
            totalWeight += 200f;
        }
    }

    private void CarvePit(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = "p";

        // int numberOfCircles = 1;
        // int minRadius = 10;
        // int maxRadius = 25;
        //
        // for (int i = 0; i < numberOfCircles; i++) {
        //     bool placed = false;
        //     int attempts = 0;
        //     const int maxAttempts = 5000;
        //
        //     while (!placed && attempts < maxAttempts) {
        //         attempts++;
        //
        //         int x = WorldGen.genRand.Next(Main.maxTilesX / 10, Main.maxTilesX - (Main.maxTilesX / 10));
        //
        //         int y = 0;
        //         for (int scanY = 100; scanY < Main.worldSurface; scanY++) {
        //             Tile tile = Framing.GetTileSafely(x, scanY);
        //             if (tile.HasTile && tile.TileType == TileID.Grass) {
        //                 y = scanY;
        //                 break;
        //             }
        //         }
        //
        //         if (y == 0) {
        //             continue;
        //         }
        //
        //         if (!Helper.AirScanUp(new Point16(x, y - 1), maxRadius + 5)) {
        //             continue;
        //         }
        //
        //         // WorldUtils.Gen(
        //         //     new Point(x, y),
        //         //     new Shapes.Circle(135),
        //         //     Actions.Chain(
        //         //         new Actions.ClearTile()
        //         //     )
        //         //     );
        //         
        //         placed = true;
        //         progress.Value += 1f / numberOfCircles;
        //     }
        // }
    }
}

public class Debug : ModItem {
    public override string Texture => Textures.Items.Misc.UrnOfGreed.KEY_ClayUrnProj;

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.rare = ItemRarityID.Green;
        Item.useAnimation = 20;
        Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = false;
        Item.autoReuse = false;
    }

    public override bool AltFunctionUse(Player player) {
        return true;
    }

    public override bool CanUseItem(Player player) {
        return true;
    }

    public override bool? UseItem(Player player) {
        Vector2 mouseWorldPosition = Main.MouseWorld;
        int tileX = (int)(mouseWorldPosition.X / 16f);
        int tileY = (int)(mouseWorldPosition.Y / 16f);
        Point spawnPoint = new Point(tileX, tileY);
        
        if (player.altFunctionUse != 2) {
            if(player.altFunctionUse != 2) {
                WorldUtils.Gen(
                    spawnPoint,
                    new Shapes.Rectangle(90, 90),
                    Actions.Chain(
                        new Actions.RemoveWall(),
                        new Actions.ClearTile(),
                       // new Actions.PlaceTile(TileID.Dirt),
                        new Actions.SetFrames(true)
                    )
                );
            }
            return true;
        }
        
        if (player.altFunctionUse == 2) {
            GeneratePitEntrance(spawnPoint.X, spawnPoint.Y);
            return true;
        }
        return false;
    }

    public static void GeneratePitEntrance(int x, int y) {
        y += 16;
        ShapeData data = new();
        ShapeData stoneData = new();
        ShapeData dirtData = new();
        
        #region dirt outer region data
        WorldUtils.Gen(
            new Point(x, y), 
            new Shapes.Slime(24),
            new Actions.Blank().Output(dirtData)
        );
        
        WorldUtils.Gen(
            new Point(x, y), 
            new Shapes.Mound(15, 12),
            Actions.Chain(
                new [] {
                    new Modifiers.Offset(0, -20),
                    new Actions.Blank().Output(dirtData)
                }
            )
        );
        
        #endregion
        
        WorldUtils.Gen(
            new Point(x, y), 
            new ModShapes.All(dirtData),
            Actions.Chain(
                new Modifiers.Blotches(3, 2, 0.6f), 
                new Actions.PlaceTile(TileID.Dirt)
            )
        );
        WorldUtils.Gen(
            new Point(x, y), 
            new ModShapes.OuterOutline(dirtData),
            Actions.Chain(
                new Actions.Smooth()
            )
        );
    }
}