using AllBeginningsMod.Core.World;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Tiles.Corruption;

public class UnderworldCorruptVines : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_UnderworldCorruptVines;
    
    public override void SetStaticDefaults() {
        Main.tileBlockLight[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.IsVine[Type] = true;
        TileID.Sets.VineThreads[Type] = true;
        TileID.Sets.ReplaceTileBreakDown[Type] = true;

        HitSound = SoundID.Grass;
        DustType = DustID.Corruption;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, 1, 0);
        TileObjectData.newTile.AnchorAlternateTiles = [Type];
        
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<OvergrownCorruptAsh>()];

        DustType = DustID.Corruption;

        TileObjectData.addTile(Type);
        
        AddMapEntry(new Color(128, 123, 88));
    }
    
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Main.instance.TilesRenderer.CrawlToTopOfVineAndAddSpecialPoint(j, i);
        return false;
    }

    public override void RandomUpdate(int i, int j) {
        int deltaY = 0;
        while (Main.tile[i, j - 1 - deltaY].TileType == Type) {
            deltaY++;
            if (deltaY > j - 1) {
                break;
            }
        }
        if (deltaY > 15 + Math.Sin(i + j) * 3) {
            return;
        }
        if (Main.rand.NextBool(Math.Max(1, deltaY * deltaY - 40))) {
            var tileBelow = Main.tile[i, j + 1];
            
            if (!tileBelow.HasTile) {
                tileBelow.TileType = Type;
                tileBelow.HasTile = true;
                tileBelow.CopyPaintAndCoating(Main.tile[i, j]);
                WorldGen.SquareTileFrame(i, j + 1);
                if (Main.netMode is NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, i, j + 1);
            }
        }
    }
}