using AllBeginningsMod.Common.Rendering;
using AllBeginningsMod.Common.World;
using AllBeginningsMod.Core;
using AllBeginningsMod.Utilities;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace AllBeginningsMod.Content.AbandonedShack;

//todo reduce magic numbers

internal sealed class AbandonedShackSystem : ModSystem {
    private float _overlayAlpha = 1f;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));
        if(ShiniesIndex != -1) {
            tasks.Insert(ShiniesIndex + 1, new PassLegacy("aa", CarvePit));
        }
    }

    private void CarvePit(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = "p";

        int numberOfCircles = 1;
        int maxRadius = 25;

        for(int i = 0; i < numberOfCircles; i++) {
            bool placed = false;
            int attempts = 0;
            const int maxAttempts = 5000;

            while(!placed && attempts < maxAttempts) {
                attempts++;

                int x = WorldGen.genRand.Next(Main.maxTilesX / 10, Main.maxTilesX - (Main.maxTilesX / 10));

                int y = 0;
                for(int scanY = 100; scanY < Main.worldSurface; scanY++) {
                    Tile tile = Framing.GetTileSafely(x, scanY);
                    if(tile.HasTile && tile.TileType == TileID.Grass) {
                        y = scanY;
                        break;
                    }
                }

                const int cloudSearchRadius = 15;
                bool cloudNearby = false;
                for(int checkX = x - cloudSearchRadius; checkX <= x + cloudSearchRadius; checkX++) {
                    for(int checkY = y - cloudSearchRadius; checkY <= y + cloudSearchRadius; checkY++) {
                        if(checkX >= 0 && checkX < Main.maxTilesX && checkY >= 0 && checkY < Main.maxTilesY) {
                            Tile tile = Framing.GetTileSafely(checkX, checkY);
                            if(tile.HasTile && (tile.TileType == TileID.Cloud || tile.TileType == TileID.RainCloud || tile.LiquidAmount > 0)) {
                                cloudNearby = true;
                                break;
                            }
                        }
                    }
                    if(cloudNearby) break;
                }
                if(cloudNearby) continue;

                if(y == 0) {
                    continue;
                }

                if(!Helper.AirScanUp(new Point16(x, y - 1), maxRadius + 5)) {
                    continue;
                }

                var biome = GenVars.configuration.CreateBiome<AbandonedShackMicrobiome>();

                biome.Place(new Point(x, y), GenVars.structures);

                placed = true;
                progress.Value += 1f / numberOfCircles;
            }
        }
    }

    public override void Load() {
        CommonHooks.DrawThingsBehindNonSolidSolidTilesEvent += DrawInsideContainer;
        CommonHooks.DrawThingsAbovePlayersEvent += DrawOverlay;
    }

    private void DrawOverlay(bool overPlayers) {
        if(!overPlayers) return;

        if(PointsOfInterestSystem.ShackPosition == Point.Zero) {
            return;
        }

        var rect1 = new Rectangle(PointsOfInterestSystem.ShackPosition.X + 11, PointsOfInterestSystem.ShackPosition.Y - 1, 18, 12);
        var rect2 = new Rectangle(PointsOfInterestSystem.ShackPosition.X, PointsOfInterestSystem.ShackPosition.Y - 1, 11, 9);

        var totalRenderArea = Rectangle.Union(rect1, rect2);

        float targetAlpha = PointsOfInterestSystem.LocalPlayerInShack ? 0f : 1f;
        _overlayAlpha = MathHelper.Lerp(_overlayAlpha, targetAlpha, 0.125f);

        if(_overlayAlpha <= 0.01f && !PointsOfInterestSystem.LocalPlayerInShack) {
            return;
        }

        var renderColor = Color.White * _overlayAlpha;

        var overArea = LightingBuffer
            .Prepare(totalRenderArea)
            .WithColor(renderColor);

        overArea.Draw(Textures.Tiles.AbandonedShack.AbandonedShackOver.Value);
    }

    private void DrawInsideContainer() {
        var rect1 = new Rectangle(PointsOfInterestSystem.ShackPosition.X, PointsOfInterestSystem.ShackPosition.Y - 2, 29, 13);

        var baseArea = LightingBuffer
            .Prepare(rect1)
            .WithColor(Color.White);

        Main.spriteBatch.End(out var snapshot);

        baseArea.Draw(Textures.Tiles.AbandonedShack.AbandonedShackBack.Value);

        Main.spriteBatch.Begin(snapshot);
    }

    #region rendering
    public override void PostDrawTiles() {
        if(PointsOfInterestSystem.ShackPosition == Point.Zero) {
            return;
        }

        var rect1 = new Rectangle(PointsOfInterestSystem.ShackPosition.X + 11, PointsOfInterestSystem.ShackPosition.Y - 1, 18, 12);
        var rect2 = new Rectangle(PointsOfInterestSystem.ShackPosition.X, PointsOfInterestSystem.ShackPosition.Y - 1, 11, 9);

        float targetAlpha = PointsOfInterestSystem.LocalPlayerInShack ? 0f : 1f;
        _overlayAlpha = MathHelper.Lerp(_overlayAlpha, targetAlpha, 0.125f);

        if(_overlayAlpha <= 0.01f && !PointsOfInterestSystem.LocalPlayerInShack) {
            return;
        }

        var renderColor = Color.White * _overlayAlpha;

        var totalRenderArea = Rectangle.Union(rect1, rect2);

        var baseArea = LightingBuffer
            .Prepare(totalRenderArea)
            .WithColor(Color.White);

        baseArea.Draw(Textures.Tiles.AbandonedShack.AbandonedShackBase.Value);

        if(Main.rand.NextBool(50)) {
            Gore.NewGore(
                new AEntitySource_Tile(PointsOfInterestSystem.ShackPosition.X, PointsOfInterestSystem.ShackPosition.Y, null),
                PointsOfInterestSystem.ShackPosition.ToVector2() * 16 + new Vector2(395, -50),
                Main.rand.NextVector2Circular(0, 2),
                GoreID.Smoke2
            );
        }


        // dograys

        var godrayColor = new Color();
        float godrayRot = 0;

        if(Main.dayTime) {
            godrayColor = Color.Yellow * 0.7f;
            godrayColor *= (float)Math.Pow(Math.Sin(Main.time / 54000f * 3.14f), 1);
            godrayRot = -0.5f * 1.57f + (float)Main.time / 54000f * 3.14f;
        }
        else {
            godrayColor = Color.Goldenrod * 0.7f;
            godrayColor *= (float)Math.Pow(Math.Sin(Main.time / 24000f * 3.14f), 3) * 0.25f;
            godrayRot = -0.5f * 1.57f + (float)Main.time / 24000f * 3.14f;
        }

        Main.spriteBatch.Begin(new SpriteBatchSnapshot() with { BlendState = BlendState.Additive });
        var ray = Textures.Sample.Godray2.Value;
        var pos = PointsOfInterestSystem.ShackPosition.ToVector2() * 16 + new Vector2(200, -400);

        Main.spriteBatch.Draw(ray, pos - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, 1f, 0, 0);
        Main.spriteBatch.Draw(ray, pos - Main.screenPosition, null, godrayColor, godrayRot + 0.2f, Vector2.Zero, 0.85f, 0, 0);
        Main.spriteBatch.Draw(ray, pos - Main.screenPosition, null, godrayColor, godrayRot - 0.2f, Vector2.Zero, 0.85f, 0, 0);

        Main.spriteBatch.End();

    }
    #endregion
}

internal sealed class AbandonedShackMicrobiome : MicroBiome {
    public override bool Place(Point origin, StructureMap structures) {
        // if (Framing.GetTileSafely(origin.X, origin.Y).TileType != TileID.Grass || Framing.GetTileSafely(origin.X, origin.Y).LiquidAmount != 0) {
        //     return false;
        // }

        int x = origin.X;
        int y = origin.Y;

        y += 32;

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
                new[] {
                    new Modifiers.Offset(0, -20),
                    new Actions.Blank().Output(dirtData)
                }
            )
        );

        WorldUtils.Gen(
            new Point(x, y),
            new Shapes.Rectangle(45, 2),
            Actions.Chain(
                new[] {
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
                new[] {
                    new Modifiers.Offset(0, -15),
                    new Actions.Blank().Output(stoneData)
                }
            )
        );

        int count = WorldGen.genRand.Next(3, 5);

        for(int i = 1; i < count + 1; ++i) {
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
                new[] {
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

        Point floorGenOrigin = new Point(x - 19, y + 10);

        WorldUtils.Gen(
            floorGenOrigin,
            new Shapes.Rectangle(43, 8),
            Actions.Chain(
                new Modifiers.Blotches(2, 0.7),
                new Actions.PlaceTile(TileID.Dirt),
                new GrassAction()
            )
        );

        int numSmallDirtPatches = WorldGen.genRand.Next(10, 15);
        for(int i = 0; i < numSmallDirtPatches; i++) {
            Point patchOrigin = new Point(
                x + WorldGen.genRand.Next(-25, 26),
                floorGenOrigin.Y - WorldGen.genRand.Next(0, 21)
            );

            GenShape patchShape = new Shapes.Circle(WorldGen.genRand.Next(2, 4));

            WorldUtils.Gen(
                patchOrigin,
                patchShape,
                Actions.Chain(
                    new Modifiers.OnlyTiles(TileID.Stone),
                    new Modifiers.Blotches(1, 0.5f),
                    new Actions.PlaceTile(TileID.Dirt),
                    new GrassAction()
                )
            );
        }

        WorldUtils.Gen(
            new Point(x, y),
            new ModShapes.OuterOutline(airData),
            new Actions.Smooth()
        );

        WorldUtils.Gen(
            new Point(x, y),
            new Shapes.Slime(20, 1.2, 1.4),
            Actions.Chain(
                new Modifiers.Blotches(2, 0.4),
                new Actions.PlaceWall((ushort)ModContent.WallType<SmoothStoneWall>())
            )
        );

        WorldUtils.Gen(
            new Point(x, y - 17),
            new Shapes.Circle(11),
            Actions.Chain(
                new Modifiers.Blotches(2, 0.4),
                new Actions.ClearWall(true)
            )
        );

        WorldUtils.Gen(
            new Point(x, y - 12),
            new Shapes.Circle(50),
            Actions.Chain(
                new Actions.SetFrames(true),
                new Actions.Smooth()
            )
        );

        var structureCenter = new Point(origin.X, origin.Y + 20);
        if(!WorldUtils.Find(structureCenter, Searches.Chain(new Searches.Down(50), new Conditions.IsSolid()), out Point floorPoint)) {
            return false;
        }

        var mod = ModContent.GetInstance<AllBeginningsModImpl>();
        string structurePath = "Assets/Structures/AbandonedShack.shstruct";
        Point16 structureDimensions = StructureHelper.API.Generator.GetStructureDimensions(structurePath, mod);

        Point16 structureOrigin = new(
            floorPoint.X - structureDimensions.X / 2,
            floorPoint.Y - structureDimensions.Y
        );

        PointsOfInterestSystem.ShackPosition = structureOrigin.ToPoint();
        StructureHelper.API.Generator.GenerateStructure(structurePath, structureOrigin, mod);

        Rectangle shackBounds = new Rectangle(
            structureOrigin.X,
            structureOrigin.Y,
            structureDimensions.X,
            structureDimensions.Y
        );
        PointsOfInterestSystem.ShackBounds = shackBounds;

        structures.AddProtectedStructure(new Rectangle(origin.X - 40, origin.Y - 40, 80, 80), 10);
        structures.AddProtectedStructure(shackBounds);

        //ProtectedAreaSystem.AddProtectedRegion(PointsOfInterestSystem.ShackBounds);

        WorldUtils.Gen(
            new Point(x, y - 12),
            new Shapes.Circle(50),
            Actions.Chain(
                new Actions.SetFrames(true),
                new Actions.Smooth()
            )
        );

        return true;
    }
}

public class AbandonedShackMapIcon : ModMapLayer {
    public static bool MouseOver;

    public override void Draw(ref MapOverlayDrawContext context, ref string text) {
        if(!PointsOfInterestSystem.FoundOldbotShack)
            return;

        var icon = Textures.UI.AbandonedShackIcon.Value;

        bool hasRecallPot = false;
        bool hasUnityPot = false;

        if(Main.LocalPlayer.HasItem(ItemID.RecallPotion))
            hasRecallPot = true;
        else if(Main.LocalPlayer.HasItem(ItemID.WormholePotion))
            hasUnityPot = true;

        float scaleIfNotSelected = 1f;
        float scaleIfSelected = (hasRecallPot || hasUnityPot) ? 1.15f : 1f;

        if(context.Draw(icon, PointsOfInterestSystem.ShackBounds.Center(), Color.White, new(1, 1, 0, 0), scaleIfNotSelected, scaleIfSelected, Alignment.Center).IsMouseOver) {
            text = Keys.MapIcons.OldbotShack.GetTextValue();

            if(hasRecallPot || hasUnityPot) {
                if(!MouseOver) {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    MouseOver = true;
                }

                text = Language.GetTextValue("Game.TeleportTo", text);
                if(Main.mouseLeft && Main.mouseLeftRelease) {
                    Main.mouseLeftRelease = false;
                    Main.mapFullscreen = false;

                    int consumedPotionID = hasRecallPot ? ItemID.RecallPotion : ItemID.WormholePotion;
                    Main.LocalPlayer.ConsumeItem(consumedPotionID);
                }
            }
        }

        else
            MouseOver = false;
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
            return true;
        }
        // WorldUtils.Gen(
        //     spawnPoint,
        //     new Splatter(8, 10, 20),
        //     Actions.Chain(
        //         new Actions.PlaceTile(TileID.Mud)
        //     )
        // );


        var biome = GenVars.configuration.CreateBiome<AbandonedShackMicrobiome>();

        biome.Place(spawnPoint, GenVars.structures);

        return true;
    }
}