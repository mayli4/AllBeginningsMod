using AllBeginningsMod.Common;
using AllBeginningsMod.Common.Graphics;
using AllBeginningsMod.Common.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Utilities;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace AllBeginningsMod.Content.Jungle;

//todo:
//shaking
//finish branches
//polish framing
//rotting trunks
//fix breaking (high prio)

internal sealed class GiantMahoganyTree : ModTile, ICustomLayerTile {
    public override string Texture => Textures.Tiles.Jungle.KEY_GiantJungleTreeTile;

    private const int big_right_branch_frame = 220;
    private const int big_left_branch_frame = 210;
    private const int small_right_branch_frame = 246;
    private const int small_left_branch_frame = 192;
    
    internal static IEnumerable<TreetopPlatformNPC> Platforms {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => from plat 
            in Main.npc 
            where plat.active 
            where plat.type == ModContent.NPCType<TreetopPlatformNPC>() 
            select plat.ModNPC as TreetopPlatformNPC;
    }
    
    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileSolid[Type] = false;
        
        Main.tileFrameImportant[Type] = true;
        Main.tileAxe[Type] = true;
        MineResist = 2f;
        
        TileID.Sets.IsATreeTrunk[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

        DustType = DustID.RichMahogany;
        
        AddMapEntry(new Color(86, 38, 55), Language.GetText("MapObject.Tree"));
        
        KillTileHooks.OverrideKillTileEvent += PreventTileBreakIfOnTopOfIt;
    }

    private bool? PreventTileBreakIfOnTopOfIt(int i, int j, int type) {
        if (type == Type) {
            int leftmostX = i;
            while (WorldGen.InWorld(leftmostX - 1, j) && Main.tile[leftmostX - 1, j].HasTile 
                                                      && Main.tile[leftmostX - 1, j].TileType == Type) {
                leftmostX--;
            }
            
            var segmentsToMurder = new Queue<Point16>();
            segmentsToMurder.Enqueue(new Point16(leftmostX, j));

            while (segmentsToMurder.Any()) {
                var currentSegmentStart = segmentsToMurder.Dequeue();
                int currentX = currentSegmentStart.X;
                int currentY = currentSegmentStart.Y;

                for (int x = currentX; x < currentX + 5; x++) {
                    if (WorldGen.InWorld(x, currentY) && Main.tile[x, currentY].HasTile && Main.tile[x, currentY].TileType == Type) {
                        WorldGen.KillTile(x, currentY); 
                    }
                }

                int nextY = currentY - 1;
                bool hasTreeAbove = false;
                for (int x = currentX; x < currentX + 3; x++) {
                    if (WorldGen.InWorld(x, nextY) 
                        && Main.tile[x, nextY].HasTile 
                        && Main.tile[x, nextY].TileType == Type) {
                        hasTreeAbove = true;
                        break;
                    }
                }

                if (hasTreeAbove) {
                    segmentsToMurder.Enqueue(new Point16(currentX, nextY));
                }
            }

            return true;
        }
        return null;
    }

    private bool IsTreeTile(int x, int y) {
        if (!WorldGen.InWorld(x, y)) {
            return false;
        }
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == Type;
    }
    
    internal static bool IsSuitableForPlatform(int x, int y) {
        if (!WorldGen.InWorld(x, y)) {
            return false;
        }
        
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<GiantMahoganyTree>() && tile.TileFrameY == 200 || tile.TileFrameY == 210 || tile.TileFrameY == 228;
    }

    public static bool ValidTileToGrowOn(Tile t) => t.TileType == TileID.JungleGrass || t.TileType == TileID.Mud;

    public static bool IsTreeTop(int x, int y) => Main.tile[x, y].TileFrameY == 200;

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        int dropCount = 1;
        int closestPlayer = Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16);
        int axe = Main.player[closestPlayer].inventory[Main.player[closestPlayer].selectedItem].axe;
        if (WorldGen.genRand.Next(100) < axe || Main.rand.NextBool(3))
            dropCount = 2;

        yield return new Item(ItemID.RichMahogany, dropCount);
    }

    #region framing
    //maybe clean this up later? idk! idont want to touch this again i cant even lie aha
    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        Tile tile = Main.tile[i, j];
        
        if (!tile.HasTile || tile.TileType != Type) {
            return true;
        }
        
        if (tile.TileFrameY == 200 || tile.TileFrameY == 210 || tile.TileFrameY == 228 || tile.TileFrameY == small_right_branch_frame || tile.TileFrameY == small_left_branch_frame) {
            return true;
        }
        
        int originalFrameX = tile.TileFrameX;
        int originalFrameY = tile.TileFrameY;

        int bottomTileType = -1;
        if(Main.tile[i, j + 1].HasTile)
            bottomTileType = Main.tile[i, j + 1].TileType;

        bool ontopOfGround = bottomTileType == TileID.JungleGrass;

        int topTileType = -1;
        if(Main.tile[i, j - 1].HasTile)
            topTileType = Main.tile[i, j - 1].TileType;

        bool isLeftTrunkSegment = Main.tile[i - 1, j].TileType != Type  
                                  && Main.tile[i + 1, j].TileType == Type;
        
        bool isRightTrunkSegment = Main.tile[i + 1, j].TileType != Type 
                                   && Main.tile[i - 1, j].TileType == Type;
        
        bool isMiddleTrunkSegment = Main.tile[i + 1, j].HasTile 
                                    && Main.tile[i + 1, j].TileType == Type 
                                    && Main.tile[i - 1, j].HasTile 
                                    && Main.tile[i - 1, j].TileType == Type;

        bool isExposedToAirOnTop = Main.tile[i, j].TileType == Type && Main.tile[i, j - 1].TileType != Type;

        bool isLeftRoot = false;
        bool isRightRoot = false;
        
        if(isLeftTrunkSegment) {
            tile.TileFrameY = (short)(18 + WorldGen.genRand.Next(3) * 18);
        }
        if(isRightTrunkSegment) {
            tile.TileFrameY = (short)(18 + WorldGen.genRand.Next(3) * 18);
            tile.TileFrameX = 36;
        }

        if(isMiddleTrunkSegment) {
            tile.TileFrameY = (short)(18 + WorldGen.genRand.Next(3) * 18);
            tile.TileFrameX = 18;
        }

        if(isLeftTrunkSegment && ontopOfGround) {
            tile.TileFrameY = 72;
            isLeftRoot = true;
        }
        
        if(isRightTrunkSegment && ontopOfGround) {
            tile.TileFrameX = 72;
            tile.TileFrameY = 72;
            isRightRoot = true;
        }

        if(isExposedToAirOnTop && !isRightRoot && !isLeftRoot) {
            if(isLeftTrunkSegment) {
                tile.TileFrameY = 116;
            }
            if(isRightTrunkSegment) {
                tile.TileFrameY = 116;
                tile.TileFrameX = 36;
            }

            if(isMiddleTrunkSegment) {
                tile.TileFrameY = 116;
                tile.TileFrameX = 18;
            }
        }

        if(ontopOfGround) {
            if(isMiddleTrunkSegment) {
                tile.TileFrameY = 72;
                tile.TileFrameX = 36;
            }

            if(Main.tile[i - 1, j].TileFrameY == 72 && Main.tile[i - 1, j].TileFrameX == 0) {
                tile.TileFrameY = 72;
                tile.TileFrameX = 18;
            }
            
            if(Main.tile[i + 1, j].TileFrameY == 72 && Main.tile[i + 1, j].TileFrameX == 72) {
                tile.TileFrameY = 72;
                tile.TileFrameX = 54;
            }
        }

        if(isExposedToAirOnTop && ontopOfGround && !isRightRoot && !isLeftRoot) {
            tile.TileFrameY = 92;
        }
        
        return true;
    }
    
    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        if (tileFrameY == 72)
            height = 18;

        if(tileFrameY == 92) {
            height = 20;
            offsetY = -2;
        }
        
        int seed = i * 31 + j * 17;
        UnifiedRandom rand = new UnifiedRandom(seed);
        
        if (tileFrameY == 116 && tileFrameX == 0) {
            if (rand.NextBool(2)) {
                height = 20;
                offsetY = -4;
            }
        }
        
        if (tileFrameY == 116 && tileFrameX == 36) {
            if (rand.NextBool(2)) {
                height = 20;
                offsetY = 2;
            }
        }
    }
    
    #endregion

    #region rendering
    
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        Tile t = Main.tile[i, j];
        if(TileDrawing.IsVisible(t) && t.TileFrameY == 200) { //Treetop
            CustomTileRendering.AddSpecialDrawingPoint(i, j, TileDrawLayer.Foreground, true);
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        }

        if(TileDrawing.IsVisible(t) && t.TileFrameY == 210) { //Left big branch
            CustomTileRendering.AddSpecialDrawingPoint(i, j, TileDrawLayer.Foreground, true);
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        }

        if(TileDrawing.IsVisible(t) && t.TileFrameY == 228) { //Right big branch
            CustomTileRendering.AddSpecialDrawingPoint(i, j, TileDrawLayer.Foreground, true);
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        }
        
        if(TileDrawing.IsVisible(t) && t.TileFrameY == small_right_branch_frame) { //Right small branch
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        }
        
        if(TileDrawing.IsVisible(t) && t.TileFrameY == small_left_branch_frame) { //Left small branch
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        }
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
        bool rightBigBranch = tile.frameY == 228;
        bool rightSmallBranch = tile.frameY == small_right_branch_frame;
        bool leftSmallBranch = tile.frameY == small_left_branch_frame;

        if(top) {
            spriteBatch.Draw(texture, drawPos + new Vector2(4, 18), underLeavesRect, Lighting.GetColor(i, j), leavesRotation, origin, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(trunkTexture, drawPos + new Vector2(4, 18), rect, Lighting.GetColor(i, j), rotation, origin, 1f, SpriteEffects.None, 0f); 
        }

        var drawPosLeftBigBranch = new Vector2(i, j) * 16 - Main.screenPosition + new Vector2(5, -20);
        
        var leftBranchRect = new Rectangle(164, 356, 216, 90);
        var leftBranchLeavesRect = new Rectangle(164, 265, 216, 90);
        var leftBranchOrigin = new Vector2(leftBranchRect.Width, leftBranchRect.Height / 2);
        
        if(leftBigBranch) {
            float branchRot = GetSway(i, j) * 0.01f;
            spriteBatch.Draw(texture, drawPosLeftBigBranch, leftBranchRect, Lighting.GetColor(i, j), branchRot, leftBranchOrigin, 1f, SpriteEffects.None, 0f);
        }
        
        var rightBranchLeavesRect = new Rectangle(382, 356, 216, 90);
        var rightBranchOrigin = new Vector2(0, rightBranchLeavesRect.Height / 2);
        
        var drawPosRightBigBranch = new Vector2(i + 1, j) * 16 - Main.screenPosition - new Vector2(5, 20);
        
        if(rightBigBranch) {
            float branchRot = GetSway(i, j) * 0.01f;
            spriteBatch.Draw(texture, drawPosRightBigBranch, rightBranchLeavesRect, Lighting.GetColor(i, j), branchRot, rightBranchOrigin, 1f, SpriteEffects.None, 0f); 
        }
        
        if(rightSmallBranch) {
            float branchRot = GetSway(i, j) * 0.09f;
            spriteBatch.Draw(texture, drawPosRightBigBranch + new Vector2(0, 50), new Rectangle(42, 284, 40, 38), Lighting.GetColor(i, j), branchRot, rightBranchOrigin, 1f, SpriteEffects.None, 0f); 
        }
        
        var drawPosLeftSmallBranch = new Vector2(i, j) * 16 - Main.screenPosition + new Vector2(4, 3);
        
        if(leftSmallBranch) {
            float branchRot = GetSway(i, j) * 0.09f;
            var rect2 = new Rectangle(0, 284, 40, 38);
            var orig = new Vector2(rect2.Width, rect2.Height / 2);
            
            spriteBatch.Draw(texture, drawPosLeftSmallBranch, new Rectangle(0, 284, 40, 38), Lighting.GetColor(i, j), branchRot, orig, 1f, SpriteEffects.None, 0f); 
        }
    }

    public void DrawSpecialLayer(int i, int j, TileDrawLayer layer, SpriteBatch spriteBatch) {
        var tile = Main.tile[i, j];
        Texture2D texture = TextureAssets.Tile[Type].Value;
        if (tile.TileColor != PaintID.None) {
            Texture2D paintedTex = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(Type, 0, tile.TileColor);
            texture = paintedTex ?? texture;
        }
        
        var worldPos = new Vector2(i + 1, j) * 16;
        var drawPos = worldPos - Main.screenPosition;
        
        float leavesRotation = GetSway(i + 1, j) * .1f;

        var rect = new Rectangle(164, 36, 510, 226);
        var leavesRect = new Rectangle(676, 36, 510, 226);
        var origin = new Vector2(rect.Width / 2, rect.Height);

        bool top = tile.frameY == 200;
        bool leftBigBranch = tile.frameY == 210;
        bool rightBigBranch = tile.frameY == 228;

        var drawPosLeftBigBranch = new Vector2(i, j) * 16 - Main.screenPosition + new Vector2(5, -20);
        
        var leftBranchLeavesRect = new Rectangle(164, 265, 216, 90);
        var leftBranchOrigin = new Vector2(leftBranchLeavesRect.Width, leftBranchLeavesRect.Height / 2);
        
        var rightBranchLeavesRect = new Rectangle(382, 265, 216, 90);
        var rightBranchOrigin = new Vector2(0, rightBranchLeavesRect.Height / 2);
        var drawPosRightBigBranch = new Vector2(i + 1, j) * 16 - Main.screenPosition - new Vector2(5, 20);
        
        if(top) {
            DrawShade(drawPos - new Vector2(0, 90) + new Vector2(4, 18), leavesRotation, 110, 240, i, j);
            spriteBatch.Draw(texture, drawPos + new Vector2(4, 18), leavesRect, Lighting.GetColor(i, j), leavesRotation, origin, 1f, SpriteEffects.None, 0f);   
        }
        
        if(leftBigBranch) {
            float branchRot = GetSway(i, j) * 0.01f;
            spriteBatch.Draw(texture, drawPosLeftBigBranch, leftBranchLeavesRect, Lighting.GetColor(i, j), branchRot, leftBranchOrigin, 1f, SpriteEffects.None, 0f); 
        }
        
        if(rightBigBranch) {
            float branchRot = GetSway(i, j) * 0.01f;
            spriteBatch.Draw(texture, drawPosRightBigBranch, rightBranchLeavesRect, Lighting.GetColor(i, j), branchRot, rightBranchOrigin, 1f, SpriteEffects.None, 0f); 
        }
    }
    
    private void DrawShade(Vector2 position, float rotation, int leftWidth, int length, int tileX, int tileY) {
		const float startTime = 4.50f;
		const float endTime = 21f;

		float time = Utils.GetDayTimeAs24FloatStartingFromMidnight();
		
		if (time is < startTime or > endTime)
			return;
        
		float opacity = Math.Clamp((float)Math.Sin((time - startTime) / (endTime - startTime) * MathHelper.Pi) * 2, 0, 1);
		float x = MathHelper.Lerp(200, -140, (float)(Main.time / Main.dayLength));

		Vector3 topLeft = new Vector3(position, 0) + new Vector3(new Vector2(-240, 0).RotatedBy(rotation) - new Vector2(0, 106), 0);
		Vector3 topRight = new Vector3(position, 0) + new Vector3(new Vector2(220, 0).RotatedBy(rotation) - new Vector2(0, 106), 0);
		Vector3 botLeft = new Vector3(position, 0) + new Vector3(-160 + x, length, 0);
		Color color = Color.White;

		short[] indices = [0, 1, 2, 1, 3, 2];

		VertexPositionColorTexture[] vertices = [
			new(topLeft, color, new Vector2(0, 0)),
            new(topRight, color, new Vector2(1, 0)),
            new(botLeft, color, new Vector2(0, 1)),
            new(botLeft + new Vector3(410, 0, 0), color, new Vector2(1, 1)),
		];

		Effect effect = Shaders.Fragment.Shade.Value;

		var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
		Matrix view = Main.GameViewMatrix.TransformationMatrix;
		Matrix renderMatrix = view * projection;

        float positionalOffset = (float)(Math.Sin(tileX * 0.1234f + tileY * 0.5678f) * 50f);

        Graphics.BeginPipeline(0.5f)
            .DrawTexturedIndexedMesh(
                vertices,
                indices,
                PrimitiveType.TriangleList,
                primitiveCount: 2,
                effect,
                ("baseShadowColor", Color.Black.ToVector4() * 0.9f * opacity),
                ("adjustColor", Color.Goldenrod.ToVector4() * 1.4f * opacity),
                ("noiseScroll", Main.GameUpdateCount * 0.0005f + positionalOffset),
                ("noiseStretch", 2),
                ("uWorldViewProjection", renderMatrix),
                ("noiseTexture", Textures.Sample.Noise4.Value)
            )
            .Flush();
	}

    #endregion
    
    public static bool GrowTree(int x, int y, bool needsSapling = true, int generateOverrideHeight = 0) {
        var type = ModContent.TileType<GiantMahoganyTree>();

        void GenBaseOnlyOutermost(int i, int j) {
            WorldGen.PlaceTile(i - 1, j, type, mute: true, forced: true);
            WorldGen.PlaceTile(i + 3, j, type, mute: true, forced: true);
        }
        
        int minHeightSegments = 20;
        int maxHeightSegments = 40; 
        
        int treeHeightSegments = generateOverrideHeight > 0 ? generateOverrideHeight : WorldGen.genRand.Next(minHeightSegments, maxHeightSegments + 1);

        GenBaseOnlyOutermost(x, y);
        
        bool lastSegmentHadLeftBranch = false;
        bool lastSegmentHadRightBranch = false;
        
        bool lastSegmentHadLeftSmallBranch = false;
        bool lastSegmentHadRightSmallBranch = false;

        for (int h = 0; h < treeHeightSegments; h++) {
            int currentY = y - h;

            WorldGen.PlaceTile(x, currentY, type, mute: true, forced: true);
            WorldGen.PlaceTile(x + 1, currentY, type, mute: true, forced: true);
            WorldGen.PlaceTile(x + 2, currentY, type, mute: true, forced: true);

            if (h > 2 && h < treeHeightSegments - 3) {
                bool currentSegmentHasLeftBranch = false;
                bool currentSegmentHasRightBranch = false;
                bool currentSegmentHasLeftSmallBranch = false;
                bool currentSegmentHasRightSmallBranch = false;

                if (!lastSegmentHadLeftBranch && WorldGen.genRand.NextBool(15)) {
                    //set as left branch
                    Main.tile[x, currentY].TileFrameY = 210; 
                    currentSegmentHasLeftBranch = true;
                }

                if (!lastSegmentHadRightBranch && WorldGen.genRand.NextBool(15)) {
                    //set as right branch
                    Main.tile[x + 2, currentY].TileFrameY = 228; 
                    currentSegmentHasRightBranch = true;
                }
                
                if (!lastSegmentHadLeftSmallBranch && WorldGen.genRand.NextBool(20)) {
                    //set as left branch
                    Main.tile[x, currentY].TileFrameY = small_left_branch_frame; 
                    currentSegmentHasLeftSmallBranch = true;
                }

                if (!lastSegmentHadRightSmallBranch && WorldGen.genRand.NextBool(20)) {
                    //set as right branch
                    Main.tile[x + 2, currentY].TileFrameY = 246; 
                    currentSegmentHasRightSmallBranch = true;
                }

                lastSegmentHadLeftBranch = currentSegmentHasLeftBranch;
                lastSegmentHadRightBranch = currentSegmentHasRightBranch;
                lastSegmentHadLeftSmallBranch = currentSegmentHasLeftSmallBranch;
                lastSegmentHadRightSmallBranch = currentSegmentHasRightSmallBranch;
            }
        }

        int frameStartX = x - 1; 
        int frameWidth = 5;      
        for (int h = 0; h < treeHeightSegments; h++) {
            for (int iOffset = 0; iOffset < frameWidth; iOffset++) {
                 WorldGen.SquareTileFrame(frameStartX + iOffset, y - h, true);
            }
            
            if (h == 0) { 
                frameStartX = x;
                frameWidth = 3;
            }
        }

        int topY = y - (treeHeightSegments - 1);
        if (WorldGen.InWorld(x + 1, topY)) {
            Main.tile[x + 1, topY].TileFrameY = 200;
            WorldGen.SquareTileFrame(x + 1, topY, true);
        }
        
        return true;
    }
    
    
    public override void NearbyEffects(int i, int j, bool closer) {
        if (closer)
            return;
        
        var tile = Main.tile[i, j];
        
        Point16? platformSpawnPoint = tile.TileFrameY switch {
            200 => new Point16(i, j - 12),
            210 => new Point16(i - 8, j - 3),
            228 => new Point16(i + 8, j - 3),
            _ => default
        };

        Point16 pt = platformSpawnPoint.Value;
        Point16 windSourceTile = new Point16(i, j);

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
                newPlatform.WindSourceTile = windSourceTile;
                Main.npc[npcIndex].netUpdate = true;
            }
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

        if(Main.keyState.IsKeyDown(Keys.H) && !Main.oldKeyState.IsKeyDown(Keys.H)) {
            //WorldGen.TileFrame(tileX, tileY, true);
            
            GiantMahoganyTree.GrowTree(tileX, tileY);
        }
    }
}