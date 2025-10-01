using AllBeginningsMod.Common;
using AllBeginningsMod.Common.Graphics;
using AllBeginningsMod.Common.World;
using AllBeginningsMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace AllBeginningsMod.Content.Tiles.Jungle;

internal sealed class GiantMahoganyTree : ModTile, ICustomLayerTile {
    public override string Texture => Textures.Tiles.Jungle.KEY_GiantJungleTreeTile;
    
    internal static IEnumerable<TreetopPlatformNPC> Platforms {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Main.npc.Where(npc => npc.active && npc.type == ModContent.NPCType<TreetopPlatformNPC>())
            .Select(npc => npc.ModNPC as TreetopPlatformNPC)
            .Where(modNpc => modNpc != null);
    }
    
    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = false;
        
        Main.tileFrameImportant[Type] = true;
        Main.tileAxe[Type] = true;
        
        TileID.Sets.IsATreeTrunk[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

        DustType = DustID.RichMahogany;
        
        AddMapEntry(new Color(86, 38, 55), Language.GetText("MapObject.Tree"));
        
        KillTileHooks.OverrideKillTileEvent += PreventTileBreakIfOnTopOfIt;
    }

    private bool? PreventTileBreakIfOnTopOfIt(int i, int j, int type) {
        if (type == Type)
            return true;

        return null;
    }

    private bool IsTreeTile(int x, int y) {
        if (!WorldGen.InWorld(x, y)) {
            return false;
        }
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == Type;
    }
    
    public static bool ValidTileToGrowOn(Tile t) => t.HasUnactuatedTile && !t.IsHalfBlock && t.Slope == SlopeType.Solid && 
                                                    (t.TileType == TileID.JungleGrass);

    public static bool IsTreeTop(int x, int y) => Main.tile[x, y].TileFrameY >= 200;

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        Tile tile = Main.tile[i, j];
        int originalFrameX = tile.TileFrameX;
        int originalFrameY = tile.TileFrameY;
        
        int bottomTileType = -1;
        if (Main.tile[i, j + 1].HasTile)
            bottomTileType = Main.tile[i, j + 1].TileType;

        int topTileType = -1;
        if (Main.tile[i, j - 1].HasTile)
            topTileType = Main.tile[i, j - 1].TileType;
 
        tile.TileFrameY = 200;
        
        return true;
    }

    #region rendering
    
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        Tile t = Main.tile[i, j];
        if (TileDrawing.IsVisible(t) && t.TileFrameY == 200) //Treetop
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        
        if (TileDrawing.IsVisible(t) && t.TileFrameY == 210) //Left big branch
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        
        if (TileDrawing.IsVisible(t) && t.TileFrameY == 220) //Right big branch
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
    }
    
    public static float GetSway(int i, int j, double factor = 0) {
        if (factor == 0)
            factor = TileSwaying.Instance.TreeWindCounter;

        return Main.instance.TilesRenderer.GetWindCycle(i, j, factor) * .4f;
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
        var tile = Main.tile[i, j];
        Texture2D texture = TextureAssets.Tile[Type].Value;
        if (tile.TileColor != PaintID.None) {
            Texture2D paintedTex = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(Type, 0, tile.TileColor);
            texture = paintedTex ?? texture;
        }
        
        var trunkTexture = TextureAssets.Tile[Type].Value;
        
        var worldPos = new Vector2(i + 1, j) * 16;
        var drawPos = worldPos - Main.screenPosition;
        
        float rotation = GetSway(i + 1, j) * .04f;
        float leavesRotation = GetSway(i + 1, j) * .1f;

        var rect = new Rectangle(164, 36, 510, 226);
        var leavesRect = new Rectangle(676, 36, 510, 226);
        var underLeavesRect = new Rectangle(1188, 36, 510, 226);
        var origin = new Vector2(rect.Width / 2, rect.Height);

        bool top = tile.frameY == 200;
        bool leftBigBranch = tile.frameY == 210;
        bool rightBigBranch = tile.frameY == 220;

        if(top) {
            spriteBatch.Draw(texture, drawPos, underLeavesRect, Lighting.GetColor(i, j), leavesRotation, origin, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(trunkTexture, drawPos, rect, Lighting.GetColor(i, j), rotation, origin, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos + new Vector2(0, 0), leavesRect, Lighting.GetColor(i, j), leavesRotation, origin, 1f, SpriteEffects.None, 0f);   
        }

        var drawPosLeftBigBranch = new Vector2(i, j) * 16 - Main.screenPosition;
        
        var leftBranchRect = new Rectangle(164, 356, 216, 90);
        var leftBranchLeavesRect = new Rectangle(164, 265, 216, 90);
        var leftBranchOrigin = new Vector2(leftBranchRect.Width, leftBranchRect.Height / 2);
        
        if(leftBigBranch) {
            float branchRot = GetSway(i, j) * 0.01f;
            spriteBatch.Draw(texture, drawPosLeftBigBranch, leftBranchRect, Lighting.GetColor(i, j), branchRot, leftBranchOrigin, 1f, SpriteEffects.None, 0f); 
            spriteBatch.Draw(texture, drawPosLeftBigBranch, leftBranchLeavesRect, Lighting.GetColor(i, j), branchRot, leftBranchOrigin, 1f, SpriteEffects.None, 0f); 
        }
    }

    public static bool GrowTree(int x, int y, bool needsSapling = true, int generateOverrideHeight = 0) {
        return false;
    }
    
    #endregion
    
    public override void NearbyEffects(int i, int j, bool closer) {
        if (closer || !IsTreeTop(i, j))
            return;
        
        var tile  = Main.tile[i, j];
        
        bool leftBigBranch =  tile.frameY == 210;

        if(leftBigBranch) {
            var pt = new Point16(i - 8, j - 2);
            if (!Platforms.Any(p => p.TreePosition == pt)) {
                int npcIndex = NPC.NewNPC(
                    new EntitySource_SpawnNPC(),
                    (i * 16),
                    (j * 16),
                    ModContent.NPCType<TreetopPlatformNPC>()
                );
        
                if (npcIndex != -1) {
                    TreetopPlatformNPC newPlatform = (TreetopPlatformNPC)Main.npc[npcIndex].ModNPC;
                    newPlatform.TreePosition = pt;
                    newPlatform.WindSourceTile = new Point16(i, j);
                    Main.npc[npcIndex].netUpdate = true;
                }
            }
        }
    }
    
            public void RenderWithSway(Effect effect, Matrix worldViewProjection, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, Point tileCoords, float sway, int divisions, float swayBoostPerDivision, bool flipped = false, bool fullBright = false)
        {
            if (flipped)
                origin.X = frame.Width - origin.X;

            Vector3 topLeft = (position - origin).Vec3();
            Vector3 botLeft = topLeft + Vector3.UnitY * frame.Height;
            float leftUv = flipped ? 1 : 0;
            float rightUv = flipped ? 0 : 1;

            short[] indices = new short[6 + 6 * divisions];
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4 + divisions * 2];

            Color color = fullBright ? Color.White : Lighting.GetColor(tileCoords);
            float worldPosHeight = tileCoords.Y * 16;

            vertices[0] = new(botLeft, color, new Vector2(leftUv, 1));
            vertices[1] = new(botLeft + Vector3.UnitX * frame.Width, color, new Vector2(rightUv, 1));

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 2;

            int vertexIndex = 2;
            int indexIndex = 6;
            float sideSway = 0;
            float divisionHeight = 1 / (float)(divisions + 1);


            for (int i = 0; i <= divisions; i++)
            {
                float uvY = 1 - (i + 1) / (float)(divisions + 1);
                Vector3 divisionLeft = botLeft - Vector3.UnitY * frame.Height * divisionHeight * (i + 1);
                divisionLeft.X += sideSway;

                Color segmentColor = fullBright ? Color.White : Lighting.GetColor(new Point(tileCoords.X, (int)((worldPosHeight - frame.Height * divisionHeight * (i + 1)) / 16)));
                vertices[vertexIndex] = new(divisionLeft, segmentColor, new Vector2(leftUv, uvY));
                vertices[vertexIndex + 1] = new(divisionLeft + Vector3.UnitX * frame.Width, segmentColor, new Vector2(rightUv, uvY));

                //Rotational tilt
                if (sideSway > 0)
                {
                    vertices[vertexIndex + 1].Position.Y += sideSway * 2;
                    vertices[vertexIndex].Position.Y -= sideSway * 1.2f;
                }
                else
                {
                    vertices[vertexIndex].Position.Y -= sideSway * 2;
                    vertices[vertexIndex + 1].Position.Y += sideSway * 1.2f;
                }

                if (i < divisions)
                {
                    indices[indexIndex] = (short)(vertexIndex);
                    indices[indexIndex + 1] = (short)(vertexIndex + 1);
                    indices[indexIndex + 2] = (short)(vertexIndex + 2);
                    indices[indexIndex + 3] = (short)(vertexIndex + 1);
                    indices[indexIndex + 4] = (short)(vertexIndex + 3);
                    indices[indexIndex + 5] = (short)(vertexIndex + 2);
                }

                vertexIndex += 2;
                indexIndex += 6;


                sideSway = (float)Math.Pow((i + 1) / (float)divisions, swayBoostPerDivision) * sway;
            }


            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                effect.Parameters["textureResolution"].SetValue(texture.Size());
                effect.Parameters["sampleTexture"].SetValue(texture);
                effect.Parameters["frame"].SetValue(new Vector4(frame.X, frame.Y, frame.Width, frame.Height));
                effect.Parameters["uWorldViewProjection"].SetValue(worldViewProjection);
                pass.Apply();


                RasterizerState cache = Main.instance.GraphicsDevice.RasterizerState;
                Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

                Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4 + 2 * divisions, indices, 0, 2 + 2 * divisions);

                Main.instance.GraphicsDevice.RasterizerState = cache;
            }
        }
}

internal class FrameTest : ModSystem {
    private bool IsTreeTile(int x, int y) {
        if (!WorldGen.InWorld(x, y)) {
            return false;
        }
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<GiantMahoganyTree>();
    }
    
    public override void PostUpdateEverything() {
        Vector2 mouseWorldPosition = Main.MouseWorld;
        int tileX = (int)(mouseWorldPosition.X / 16f);
        int tileY = (int)(mouseWorldPosition.Y / 16f);

        if(Main.keyState.IsKeyDown(Keys.H) && IsTreeTile(tileX, tileY)) {
            WorldGen.TileFrame(tileX, tileY, true);
        }
    }
}