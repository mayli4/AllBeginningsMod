using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Tiles.Corruption;

public class OvergrownCorruptAshFoliage : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_OvergrownCorruptAshFoliage;
    
    public override void SetStaticDefaults() {
        Main.tileCut[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileObsidianKill[Type] = true;
        
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<OvergrownCorruptAsh>()];

        DustType = DustID.Corruption;
        HitSound = SoundID.Grass;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(69, 68, 114));
    }
    
    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (Main.player[Player.FindClosest(new Vector2(i, j).ToWorldCoordinates(0, 0), 16, 16)].HeldItem.type == ItemID.Sickle)
            yield return new Item(ItemID.Hay, Main.rand.Next(1, 3));
    }
}