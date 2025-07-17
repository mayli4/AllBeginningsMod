using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Tiles.Corruption;

public class UnderworldCorruptLavaDropletSource : ModTile {
    public override string Texture => "Terraria/Images/Item_0";

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
        TileObjectData.newTile.Height = 1;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);
        TileID.Sets.DisableSmartCursor[Type] = true;
        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new Color(215, 227, 0), name);
        DustType = 0;
        
        TileLoader.RegisterConversion(TileID.LavaDrip, BiomeConversionID.Corruption, ConvertToCorruption);
    }
    
    public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
        WorldGen.ConvertTile(i, j, Type);
        return false;
    }

    public override void Convert(int i, int j, int conversionType) {
        switch (conversionType) {
            case BiomeConversionID.Chlorophyte:
            case BiomeConversionID.Purity:
                WorldGen.ConvertTile(i, j, TileID.LavaDrip);
                return;
            case BiomeConversionID.Sand:
            case BiomeConversionID.Corruption:
                WorldGen.ConvertTile(i, j, ModContent.TileType<UnderworldCorruptLavaDropletSource>());
                return;
            
        }
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        num = 0;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        EmitLiquidDrops(i, j);

        return base.PreDraw(i, j, spriteBatch);
    }

    public void EmitLiquidDrops(int i, int j) {
        Tile tile = Main.tile[i, j];
        int x = i - tile.TileFrameX / 18;
        int y = j - tile.TileFrameY / 18;
        int spawnX = x * 16;
        int spawnY = y * 16;

        //Below is vanilla code for spawning droplets
        int chanceDenominator = 60;
        if (tile.LiquidAmount != 0 || !Main.rand.NextBool(chanceDenominator)) {
            return;
        }

        Rectangle thisGoreAtParticularFrame = new Rectangle(x * 16, y * 16, 16, 16);
        thisGoreAtParticularFrame.X -= 34;
        thisGoreAtParticularFrame.Width += 68;
        thisGoreAtParticularFrame.Y -= 100;
        thisGoreAtParticularFrame.Height = 400;
        int goreType = Mod.Find<ModGore>("UnderworldCorruptLavaDroplet").Type;
        for (int k = 0; k < Main.maxGore; k++) {
            Gore otherGore = Main.gore[k];
            if (otherGore.active && otherGore.type == goreType) {
                Rectangle otherGoreRect = new Rectangle((int)otherGore.position.X, (int)otherGore.position.Y, 16, 16);
                if (thisGoreAtParticularFrame.Intersects(otherGoreRect)) {
                    //Check if no other droplets exist in the same tile
                    return;
                }
            }
        }

        var source = WorldGen.GetItemSource_FromTileBreak(x, y);
        Gore.NewGoreDirect(source, new Vector2(spawnX, spawnY), Vector2.Zero, goreType, 1f).velocity *= 0f;
    }

    public override bool CanDrop(int i, int j)
    {
        return false;
    }
}

public class UnderworldCorruptLavaDroplet : ModGore {
    public override string Texture => Assets.Assets.Textures.Lavas.KEY_UnderworldCorruptLavaDroplet;

    public override void OnSpawn(Gore gore, IEntitySource source)
    {
        gore.numFrames = 15;
        gore.behindTiles = true;
        gore.timeLeft = Gore.goreTime * 3;
    }

        public override bool Update(Gore gore)
        {
            gore.alpha = gore.position.Y < (Main.worldSurface * 16.0) + 8.0
                ? 0
                : 100;

            int frameDuration = 4;
            gore.frameCounter += 1;
            if (gore.frame <= 4)
            {
                int tileX = (int)(gore.position.X / 16f);
                int tileY = (int)(gore.position.Y / 16f) - 1;
                if (WorldGen.InWorld(tileX, tileY) && !Main.tile[tileX, tileY].HasTile)
                {
                    gore.active = false;
                }

                if (gore.frame == 0 || gore.frame == 1 || gore.frame == 2)
                {
                    frameDuration = 24 + Main.rand.Next(256);
                }

                if (gore.frame == 3)
                {
                    frameDuration = 24 + Main.rand.Next(96);
                }

                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                    if (gore.frame == 5)
                    {
                        int droplet = Gore.NewGore(new EntitySource_Misc(nameof(UnderworldCorruptLavaDroplet)), gore.position, gore.velocity, gore.type);
                        Main.gore[droplet].frame = 9;
                        Main.gore[droplet].velocity *= 0f;
                    }
                }
            }
            else if (gore.frame <= 6)
            {
                frameDuration = 8;
                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                    if (gore.frame == 7)
                    {
                        gore.active = false;
                    }
                }
            }
            else if (gore.frame <= 9)
            {
                frameDuration = 6;
                gore.velocity.Y += 0.2f;
                if (gore.velocity.Y < 0.5f)
                {
                    gore.velocity.Y = 0.5f;
                }

                if (gore.velocity.Y > 12f)
                {
                    gore.velocity.Y = 12f;
                }

                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                }

                if (gore.frame > 9)
                {
                    gore.frame = 7;
                }
            }
            else
            {
                gore.velocity.Y += 0.1f;
                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                }

                gore.velocity *= 0f;
                if (gore.frame > 14)
                {
                    gore.active = false;
                }
            }

            Vector2 oldVelocity = gore.velocity;
            gore.velocity = Collision.TileCollision(gore.position, gore.velocity, 16, 14);
            if (gore.velocity != oldVelocity)
            {
                if (gore.frame < 10)
                {
                    gore.frame = 10;
                    gore.frameCounter = 0;
                    SoundEngine.PlaySound(SoundID.Drip, gore.position + new Vector2(8, 8));
                }
            }
            else if (Collision.WetCollision(gore.position + gore.velocity, 16, 14))
            {
                if (gore.frame < 10)
                {
                    gore.frame = 10;
                    gore.frameCounter = 0;
                    SoundEngine.PlaySound(SoundID.Drip, gore.position + new Vector2(8, 8));
                }

                int tileX = (int)(gore.position.X + 8f) / 16;
                int tileY = (int)(gore.position.Y + 14f) / 16;
                if (Main.tile[tileX, tileY] != null && Main.tile[tileX, tileY].LiquidAmount > 0)
                {
                    gore.velocity *= 0f;
                    gore.position.Y = (tileY * 16) - (Main.tile[tileX, tileY].LiquidAmount / 16);
                }
            }

            gore.position += gore.velocity;
            return false;
        }
    }