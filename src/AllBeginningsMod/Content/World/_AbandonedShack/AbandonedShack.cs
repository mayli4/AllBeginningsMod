using AllBeginningsMod.Common.World;
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
    public static Point16? ShackPos; 
    
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Pyramids"));
        if(ShiniesIndex != -1) {
            tasks.Insert(ShiniesIndex + 1, new PassLegacy("aa", CarvePit));
        }
    }

    private void CarvePit(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = "p";

        int numberOfCircles = 1;
        int maxRadius = 25;
        
        for (int i = 0; i < numberOfCircles; i++) {
            bool placed = false;
            int attempts = 0;
            const int maxAttempts = 5000;
        
            while (!placed && attempts < maxAttempts) {
                attempts++;
        
                int x = WorldGen.genRand.Next(Main.maxTilesX / 10, Main.maxTilesX - (Main.maxTilesX / 10));
        
                int y = 0;
                for (int scanY = 100; scanY < Main.worldSurface; scanY++) {
                    Tile tile = Framing.GetTileSafely(x, scanY);
                    if (tile.HasTile && tile.TileType == TileID.Grass) {
                        y = scanY;
                        break;
                    }
                }
        
                if (y == 0) {
                    continue;
                }
        
                if (!Helper.AirScanUp(new Point16(x, y - 1), maxRadius + 5)) {
                    continue;
                }

                var biome = GenVars.configuration.CreateBiome<AbandonedShackMicrobiome>();

                biome.Place(new Point(x, y), GenVars.structures);
                
                placed = true;
                progress.Value += 1f / numberOfCircles;
            }
        }
    }
}

internal sealed class AbandonedShackMicrobiome : MicroBiome {
    public override bool Place(Point origin, StructureMap structures) {
        if (Framing.GetTileSafely(origin.X, origin.Y).TileType != TileID.Grass) {
            return false;
        }

        int x = origin.X;
        int y = origin.Y;

        y += 16;
        ShapeData airData = new();
        ShapeData stoneData = new();
        ShapeData dirtData = new();
        
        #region dirt outer region data
        WorldUtils.Gen(
            new Point(x, y), 
            new Shapes.Slime(30, 1.3, 1),
            new Actions.Blank().Output(dirtData)
        );
        
        WorldUtils.Gen(
            new Point(x, y), 
            new Shapes.Circle(15, 12),
            Actions.Chain(
                new [] {
                    new Modifiers.Offset(0, -20),
                    new Actions.Blank().Output(dirtData)
                }
            )
        );
        
        WorldUtils.Gen(
            new Point(x, y), 
            new Shapes.Rectangle(45, 2),
            Actions.Chain(
                new [] {
                    new Modifiers.Offset(-20, -28),
                    new Actions.Blank().Output(dirtData)
                }
            )
        );
        
        #endregion
        
        #region stone inner region data
        WorldUtils.Gen(
            new Point(x, y), 
            new Shapes.Slime(22, 1.2, 1.2),
            new Actions.Blank().Output(stoneData)
        );
        
        WorldUtils.Gen(
            new Point(x, y), 
            new Shapes.Mound(15, 12),
            Actions.Chain(
                new [] {
                    new Modifiers.Offset(0, -15),
                    new Actions.Blank().Output(stoneData)
                }
            )
        );
        
        int count = WorldGen.genRand.Next(3, 5);

        for (int i = 1; i < count + 1; ++i) {
            var o = new Point(x + WorldGen.genRand.Next(-5 * i, 5 * i + 1), y + WorldGen.genRand.Next(2, 5 * i));
            bool circle = WorldGen.genRand.NextBool();
            GenShape shape = circle ? new Shapes.Circle(12 + 3 * i, 3 + i) : new Shapes.Rectangle(12 + 3 * i, 4 + i);
            var blotches = new Modifiers.Blotches(circle ? 3 : 7, circle ? 2 : 4, circle ? 0.9f : 0.6f);
            WorldUtils.Gen(o, shape, Actions.Chain(blotches, new Actions.ClearTile().Output(stoneData)));
        }
        #endregion
        
        #region carved air region data
        WorldUtils.Gen(
            new Point(x, y), 
            new Shapes.Slime(14, 1.3, 1),
            Actions.Chain(
                new Modifiers.Blotches(2, 5, 10.6f), 
                new Actions.Blank().Output(airData)
            )
        );
        
        WorldUtils.Gen(
            new Point(x, y), 
            new Shapes.Mound(10, 30),
            Actions.Chain(
                new [] {
                    new Modifiers.Blotches(2, 2, 10.6f), 
                    new Modifiers.Offset(0, -10),
                    new Actions.Blank().Output(airData)
                }
            )
        );
        
        #endregion
        
        WorldUtils.Gen(
            new Point(x, y), 
            new ModShapes.All(dirtData),
            Actions.Chain(
                new Modifiers.Blotches(3, 2, 0.6f), 
                new Actions.PlaceTile(TileID.Dirt),
                new GrassAction()
            )
        );
        
        WorldUtils.Gen(
            new Point(x, y), 
            new ModShapes.All(stoneData),
            Actions.Chain(
                new Modifiers.Blotches(2, 2, 10.6f), 
                new Actions.ClearTile(),
                new Actions.PlaceTile(TileID.Stone)
            )
        );
        
        WorldUtils.Gen(
            new Point(x, y), 
            new ModShapes.All(airData),
            Actions.Chain(
                new Modifiers.Blotches(2, 2, 0.6f), 
                new Actions.ClearTile(true),
                new Actions.ClearWall(true),
                new Actions.Clear(),
                new Actions.SetFrames(true)
            )
        );
        
        WorldUtils.Gen(
            new Point(x, y), 
            new ModShapes.OuterOutline(airData),
            new Actions.Smooth()
        );

        WorldUtils.Gen(
            new Point(x, y),
            new Shapes.Slime(20, 1.2, 1.2),
            Actions.Chain(
                new Modifiers.Blotches(2, 0.4),
                new Actions.PlaceWall(WallID.Rocks1Echo),
                new Modifiers.Blotches(1, 0.1),
                new Modifiers.Dither(0.8),
                new Actions.PlaceWall(WallID.Stone)

            )
        );
        
        WorldUtils.Gen(
            new Point(x, y - 15),
            new Shapes.Circle(15),
            Actions.Chain(
                new Modifiers.Blotches(2, 0.4),
                new Actions.ClearWall(true)

            )
        );
        
        var structureCenter = new Point(origin.X, origin.Y + 20);
        if (!WorldUtils.Find(structureCenter, Searches.Chain(new Searches.Down(50), new Conditions.IsSolid()), out Point floorPoint)) {
            return false;
        }

        var mod = ModContent.GetInstance<AllBeginningsModImpl>(); 
        string structurePath = "Assets/Structures/AbandonedShack.shstruct";
        Point16 structureDimensions = StructureHelper.API.Generator.GetStructureDimensions(structurePath, mod);

        Point16 structureOrigin = new(
            floorPoint.X - structureDimensions.X / 2,
            floorPoint.Y - structureDimensions.Y
        );

        AbandonedShackSystem.ShackPos = structureOrigin;
        StructureHelper.API.Generator.GenerateStructure(structurePath, structureOrigin, mod);
        
        structures.AddProtectedStructure(new Rectangle(origin.X - 40, origin.Y - 40, 80, 80), 10);

        return true;
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
                        new Actions.SetFrames(true)
                    )
                );
            }
            return true;
        }
        
        if (player.altFunctionUse == 2) {
            var biome = GenVars.configuration.CreateBiome<AbandonedShackMicrobiome>();

            biome.Place(spawnPoint, GenVars.structures);
            return true;
        }
        return false;
    }
}
