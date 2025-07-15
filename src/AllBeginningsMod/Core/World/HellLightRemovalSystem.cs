using AllBeginningsMod.Content.Biomes;
using AllBeginningsMod.Content.Tiles.Corruption;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace AllBeginningsMod.Core.World;

public class HellLightRemovalSystem : ModSystem {

    public override void Load() {
        On_TileLightScanner.ApplyHellLight += TileLightScanner_ApplyHellLight;
    }
    
    private void TileLightScanner_ApplyHellLight(On_TileLightScanner.orig_ApplyHellLight orig, TileLightScanner self, Tile tile, int x, int y, ref Vector3 lightColor) {
        orig.Invoke(self, tile, x, y, ref lightColor);

        var underworldCorruptLightColor = new Vector3(0.5f, 0.5f, 0.08f);
        var underworldCrimsonLightColor = new Vector3(0.5f, 0.5f, 0.0f);

        if(Main.LocalPlayer.InModBiome<UnderworldCorruptionBiome>()) {
            if ((!tile.HasTile || !Main.tileNoSunLight[tile.TileType] || ((tile.Slope != 0 || tile.IsHalfBlock) && Main.tile[x, y - 1].LiquidAmount == 0 && Main.tile[x, y + 1].LiquidAmount == 0 && Main.tile[x - 1, y].LiquidAmount == 0 && Main.tile[x + 1, y].LiquidAmount == 0)) && (Main.wallLight[tile.WallType] || tile.WallType == 73 || tile.WallType == 227) && tile.LiquidAmount < 200 && (!tile.IsHalfBlock || Main.tile[x, y - 1].LiquidAmount < 200))
            {
                lightColor = underworldCorruptLightColor;
            }
            if ((!tile.HasTile || tile.IsHalfBlock || !Main.tileNoSunLight[tile.TileType]) && tile.LiquidAmount < byte.MaxValue)
            {
                lightColor = underworldCorruptLightColor;
            }

            if(!tile.HasTile) {
                lightColor = underworldCorruptLightColor;
            }
        }
        
        if(Main.LocalPlayer.InModBiome<UnderworldCrimsonBiome>()) {
            if ((!tile.HasTile || !Main.tileNoSunLight[tile.TileType] || ((tile.Slope != 0 || tile.IsHalfBlock) && Main.tile[x, y - 1].LiquidAmount == 0 && Main.tile[x, y + 1].LiquidAmount == 0 && Main.tile[x - 1, y].LiquidAmount == 0 && Main.tile[x + 1, y].LiquidAmount == 0)) && (Main.wallLight[tile.WallType] || tile.WallType == 73 || tile.WallType == 227) && tile.LiquidAmount < 200 && (!tile.IsHalfBlock || Main.tile[x, y - 1].LiquidAmount < 200))
            {
                lightColor = underworldCrimsonLightColor;
            }
            if ((!tile.HasTile || tile.IsHalfBlock || !Main.tileNoSunLight[tile.TileType]) && tile.LiquidAmount < byte.MaxValue)
            {
                lightColor = underworldCrimsonLightColor;
            }

            if(!tile.HasTile) {
                lightColor = underworldCrimsonLightColor;
            }
        }
    }
}