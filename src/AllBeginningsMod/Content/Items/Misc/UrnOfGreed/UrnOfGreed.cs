using AllBeginningsMod.Content.Items.Misc;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Items.Misc;

internal sealed class UrnOfGreedItem : ModItem {
    public override string Texture => Textures.Items.Misc.UrnOfGreed.KEY_UrnOfGreedItem;

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = 1;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(gold: 1);
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<UrnOfGreedPlayer>().UrnOfGreedEquipped = true;
    }
}

internal sealed class PotGraveTile : ModTile {
    public override string Texture => Textures.Items.Misc.UrnOfGreed.KEY_UrnOfGreedTile;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        TileID.Sets.TileInteractRead[Type] = true;
        Main.tileSign[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateHeights = [16, 16];

        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<PotGraveTileEntity>().Generic_HookPostPlaceMyPlayer; 
        
        TileObjectData.addTile(Type);
        
        AddMapEntry(Color.Gray, Keys.Items.UrnOfGreedItem.DisplayName.GetText());

        DustType = DustID.Pot;
        HitSound = SoundID.Shatter;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        Sign.KillSign(i, j);
        
        int tileEntityX = i - (frameX / 18) % 2;
        int tileEntityY = j - (frameY / 18) % 2;

        Point16 tileEntityOrigin = new Point16(tileEntityX, tileEntityY);
        
        if (TileEntity.ByPosition.TryGetValue(tileEntityOrigin, out TileEntity entity) && entity is PotGraveTileEntity potGraveEntity) {
            potGraveEntity.DropCoins();
        }
        
        ModContent.GetInstance<PotGraveTileEntity>().Kill(i, j);
    }
    
    public override void NearbyEffects(int i, int j, bool closer) {
        if ((!Main.gamePaused && Main.instance.IsActive) && Main.rand.NextBool(240)) {
            ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.PaladinsHammer, new ParticleOrchestraSettings {
                PositionInWorld = new Vector2(i * 16 + 8, j * 16 - 8)
            });
        }
    }
}

internal sealed class PotGraveTileEntity : ModTileEntity {
    public long StoredCoins { get; set; } = 0;
    
    public override bool IsTileValidForEntity(int x, int y) {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<PotGraveTile>();
    }

    public override void SaveData(TagCompound tag) {
    }

    public override void LoadData(TagCompound tag) {
    }

    public override void NetSend(BinaryWriter writer) {
    }

    public override void NetReceive(BinaryReader reader) {
    }

    public override void Update() {
    }
    
    public void DropCoins() {
        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }

        if (StoredCoins <= 0) {
            return;
        }

        Vector2 worldPosition = new Vector2(Position.X * 16 + 16, Position.Y * 16 + 16);
        
        long tempCoins = StoredCoins;

        int platinum = (int)(tempCoins / 1000000);
        tempCoins %= 1000000;
        int gold = (int)(tempCoins / 10000);
        tempCoins %= 10000;
        int silver = (int)(tempCoins / 100);
        tempCoins %= 100;
        int copper = (int)tempCoins;

        if (platinum > 0)
            Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), (int)worldPosition.X, (int)worldPosition.Y, 0, 0, ItemID.PlatinumCoin, platinum);
        if (gold > 0)
            Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), (int)worldPosition.X, (int)worldPosition.Y, 0, 0, ItemID.GoldCoin, gold);
        if (silver > 0)
            Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), (int)worldPosition.X, (int)worldPosition.Y, 0, 0, ItemID.SilverCoin, silver);
        if (copper > 0)
            Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), (int)worldPosition.X, (int)worldPosition.Y, 0, 0, ItemID.CopperCoin, copper);
        

        StoredCoins = 0;
    }
}

internal sealed class UrnOfGreedProjectile : ModProjectile {
    public override string Texture => Textures.Items.Misc.UrnOfGreed.KEY_UrnOfGreedTile;

    public long storedCoins;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.IsAGravestone[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.knockBack = 12f;
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.aiStyle = 17;
        Projectile.penetrate = -1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.timeLeft = 3600;
    }

    public override bool PreAI() {
        if (Projectile.velocity.Y == 0f)
            Projectile.velocity.X *= 0.98f;

        Projectile.rotation += Projectile.velocity.X * 0.1f;
        Projectile.velocity.Y += 0.2f;
        if (Projectile.owner != Main.myPlayer)
            return false;

        int potentialPlacementX = (int)((Projectile.position.X + (Projectile.width / 2)) / 16f);
        int potentialPlacementY = (int)((Projectile.position.Y + Projectile.height - 4f) / 16f);
        bool placementSuccessful = false;

        var objectData = default(TileObject);
        if (TileObject.CanPlace(potentialPlacementX, potentialPlacementY, ModContent.TileType<PotGraveTile>(), 0, Projectile.direction, out objectData)) {
            placementSuccessful = TileObject.Place(objectData);
        }

        if (placementSuccessful) {
            NetMessage.SendObjectPlacement(-1, potentialPlacementX, potentialPlacementY, objectData.type, objectData.style, objectData.alternate, objectData.random, Projectile.direction);
            TileEntity.PlaceEntityNet(potentialPlacementX, potentialPlacementY - 1, ModContent.TileEntityType<PotGraveTileEntity>());
            if (TileEntity.ByPosition.TryGetValue(new Point16(potentialPlacementX, potentialPlacementY - 1), out TileEntity entity) && entity is PotGraveTileEntity newTileEntity) {
                newTileEntity.StoredCoins = storedCoins;
                if (Main.netMode == NetmodeID.Server) {
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, newTileEntity.ID, potentialPlacementX, potentialPlacementY - 1);
                }
            }
            
            
            SoundEngine.PlaySound(SoundID.Dig, new Vector2(potentialPlacementX * 16, potentialPlacementY * 16));

            int signID = Sign.ReadSign(potentialPlacementX, potentialPlacementY);
            if (signID >= 0) {
                Sign.TextSign(signID, Projectile.miscText);
                NetMessage.SendData(MessageID.ReadSign, -1, -1, null, signID, 0f, (int)(byte)new BitsByte(b1: true));
            }

            Projectile.Kill();
        }
        return false;
    }
}

internal sealed class UrnOfGreedPlayer : ModPlayer {
    public bool UrnOfGreedEquipped { get; set; }

    public override void ResetEffects() {
        UrnOfGreedEquipped = false;
    }

    public override void Load() {
        On_Player.DropTombstone += On_Player_DropTombstone;
        On_Player.DropCoins += On_Player_DropCoins;
    }

    public override void Unload() {
        On_Player.DropTombstone -= On_Player_DropTombstone;
    }

    private void On_Player_DropTombstone(On_Player.orig_DropTombstone orig, Player self, long coinsOwned, NetworkText deathText, int hitDirection) {
        if (self.GetModPlayer<UrnOfGreedPlayer>().UrnOfGreedEquipped) {
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                var spawnPosition = self.Center;
                float graveHorizontalSpeed = Math.Min(Main.rand.NextFloat(3.5f), 2f);
                if (Main.rand.NextBool())
                    graveHorizontalSpeed *= -1;

                Vector2 graveVelocity = new Vector2(Main.rand.NextFloat(1f, 3f) * hitDirection + graveHorizontalSpeed, Main.rand.NextFloat(-4f, -2f));

                var source = self.GetSource_Misc("PlayerDeath_PotGrave");
                var projectileSpawned = Projectile.NewProjectile(source, spawnPosition, graveVelocity, ModContent.ProjectileType<UrnOfGreedProjectile>(), 0, 0f, Main.myPlayer);

                var now = DateTime.Now;
                string str = now.ToString("D");
                if (GameCulture.FromCultureName(GameCulture.CultureName.English).IsActive)
                    str = now.ToString("MMMM d, yyy");
                string miscText = deathText.ToString() + "\n" + str;
                Main.projectile[projectileSpawned].miscText = miscText;
                
                if (projectileSpawned >= 0 && projectileSpawned < Main.maxProjectiles && Main.projectile[projectileSpawned].ModProjectile is UrnOfGreedProjectile potGraveProj) {
                    potGraveProj.storedCoins = coinsOwned;

                    if (Main.netMode == NetmodeID.Server) {
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectileSpawned);
                    }
                }
                
                self.lostCoins = 0;
            }
        }
        else {
            orig(self, coinsOwned, deathText, hitDirection);
        }
    }
    
    private static long On_Player_DropCoins(On_Player.orig_DropCoins orig, Player self) {
        if (self.GetModPlayer<UrnOfGreedPlayer>().UrnOfGreedEquipped) {
            long totalCoinsRemoved = 0;
            for (int i = 0; i < 59; i++) {
                Item item = self.inventory[i];
                if (item.stack > 0 && item.IsACoin) {
                    totalCoinsRemoved += (long)item.stack * item.value;
                    item.TurnToAir();
                }
            }
            return totalCoinsRemoved; 
        }
        return orig(self);
    }
}