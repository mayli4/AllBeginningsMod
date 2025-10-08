using AllBeginningsMod.Utilities;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace AllBeginningsMod.Content.Jungle;

internal sealed class CorpseFlower : ModTile {
    public override string Texture => Textures.Tiles.Jungle.KEY_CorpseFlowerBase;
    
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = false;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 2;
        
        TileObjectData.newTile.Origin = new Point16(0, 0);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        
        TileObjectData.newTile.CoordinateHeights = new [] { 16, 16, 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.DrawYOffset = 2;
        
        TileObjectData.addTile(Type);
        
        AddMapEntry(Color.Brown);

        DustType = DustID.RichMahogany;
        HitSound = SoundID.Dig;
    }
    
    public override void NearbyEffects(int i, int j, bool closer) {
        var tile = Main.tile[i, j];
        if(tile.TileFrameX is not 18 || tile.TileFrameY is not 0) return;

        var bulbs = from bulb 
                    in Main.npc 
                    where bulb.active 
                    where bulb.type == ModContent.NPCType<CorpseFlowerBulb>() 
                    select bulb.ModNPC as CorpseFlowerBulb;

        var pos = new Point16(i + 1, j - 2);
        
        if (!bulbs.Any(p => p.ParentPosition == pos)) {
            int npcIndex = NPC.NewNPC(
                new EntitySource_SpawnNPC(),
                (i * 16),
                (j * 16),
                ModContent.NPCType<CorpseFlowerBulb>()
            );
        
            if (npcIndex != -1) {
                CorpseFlowerBulb newPlatform = (CorpseFlowerBulb)Main.npc[npcIndex].ModNPC;
                newPlatform.ParentPosition = pos;
                newPlatform.Bloomed = false;
                Main.npc[npcIndex].netUpdate = true;
            }
        }
    }
}

internal sealed class CorpseFlowerBulb : ModNPC {
    public override string Texture => Textures.Tiles.Jungle.KEY_CorpseFlowerBase;
    
    public Point16 ParentPosition { get; set; }

    public bool Bloomed { get; set; } = false;

    private bool _wasBloomed = false;
    private float _bloomAnimationTimer = -1f;
    public float BloomAnimationDuration = 65f;
    
    private float _postBloomSquashTimer = -1f;
    public float PostBloomSquashDuration = 15f;

    public override void SetDefaults() {
        NPC.width = 64;
        NPC.height = 60;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = int.MaxValue;
        NPC.knockBackResist = 0f;
        NPC.immortal = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = false;
        NPC.value = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = null;
        NPC.DeathSound = null;
        NPC.dontCountMe = true;
        NPC.ShowNameOnHover = false;
    }

    public override void AI() {
        if (ParentPosition.X == 0 && ParentPosition.Y == 0 && NPC.position != Vector2.Zero) {
            ParentPosition = NPC.Center.ToPoint16();
        }
        
        NPC.Center = ParentPosition.ToVector2() * 16; 

        NPC.dontTakeDamage = Bloomed; 
        
        if (Bloomed && !_wasBloomed) {
            SoundEngine.PlaySound(Sounds.Tile.Jungle.CorpseFlowerHit with { Volume = 0.8f, PitchVariance = 0.2f }, NPC.Center);
            _bloomAnimationTimer = 0f;
            _postBloomSquashTimer = -1f;
        }

        if (_bloomAnimationTimer >= 0f && _bloomAnimationTimer < BloomAnimationDuration) {
            _bloomAnimationTimer++;
            if (_bloomAnimationTimer >= BloomAnimationDuration) {
                _bloomAnimationTimer = BloomAnimationDuration;
                _postBloomSquashTimer = 0f;
                SoundEngine.PlaySound(Sounds.Tile.Jungle.CorpseFlowerOpen with { Volume = 0.8f, PitchVariance = 0.2f }, NPC.Center);
                for(int i = 0; i < Main.rand.NextFloat(10, 20); i++) {
                    Dust.NewDust(NPC.Center - new Vector2(30, 30), 70, 70, DustID.RedMoss);
                    Dust.NewDust(NPC.Center - new Vector2(30, 30), 70, 70, DustID.Water_Jungle);
                }
            }
        }

        if (_postBloomSquashTimer >= 0f && _postBloomSquashTimer < PostBloomSquashDuration) {
            _postBloomSquashTimer++;
            if (_postBloomSquashTimer >= PostBloomSquashDuration) {
                _postBloomSquashTimer = PostBloomSquashDuration;
            }
        }
        
        _wasBloomed = Bloomed;

        if(Main.rand.NextBool(9)) {
            Dust d = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(55f, 55f), DustID.ShimmerSpark, (NPC.rotation * 0.1f - MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(0.6f, 1.6f));
            d.noGravity = true;
            d.noLight = true;
            d.noLightEmittence = true;
        }
    }
    
    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) => modifiers.HideCombatText();
    
    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
        if (!Bloomed) {
            Bloomed = true;
            NPC.netUpdate = true;
        }
    }
    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
        if (!Bloomed) {
            Bloomed = true;
            NPC.netUpdate = true;
        }
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Bloomed) return;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var tile = Main.tile[ParentPosition.X, ParentPosition.Y]; 
        var tex = TextureAssets.Npc[Type].Value;
        
        if (tile.HasTile && tile.TileColor != PaintID.None) {
            var paintedTex = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(ModContent.TileType<CorpseFlower>(), 0, tile.TileColor);
            tex = paintedTex ?? tex;
        }
        
        var unbloomedRect = new Rectangle(66, 0, 122, 94);
        var bloomedRect = new Rectangle(190, 0, 122, 94);

        Rectangle currentRect = unbloomedRect;
        Vector2 drawOrigin = new Vector2(currentRect.Width / 2f, currentRect.Height);
        Vector2 drawPosition = this.NPC.position + new Vector2(25, 88) - Main.screenPosition;
        Vector2 drawScale = Vector2.One;
        float drawRotation = 0f;

        if (_bloomAnimationTimer >= 0f && _bloomAnimationTimer < BloomAnimationDuration) {
            float progress = _bloomAnimationTimer / BloomAnimationDuration;
            
            if (progress < 0.33f) {
                float shakeProgress = MathHelper.Clamp(progress / 0.33f, 0f, 1f);
                float shakeStrength = MathF.Sin(shakeProgress * MathHelper.Pi * 4) * (1f - shakeProgress) * 4f;
                
                //drawPosition.X += Main.rand.NextFloat(-1f, 1f) * shakeStrength;
                //drawPosition.Y += MathF.Abs(MathF.Sin(shakeProgress * MathHelper.Pi * 8)) * shakeStrength * 0.5f;
                drawRotation = MathF.Sin(shakeProgress * MathHelper.Pi * 3) * 0.07f;
            }
            else {
                float phaseProgress = (progress - 0.33f) / 0.62f;
                float easedProgress = Easings.SineBumpEasing(phaseProgress, 3f);
                float squishAmount = MathF.Sin(easedProgress * MathHelper.Pi * 0.4f);

                drawScale.X = 1f - squishAmount * 0.3f;
                drawScale.Y = 1f + squishAmount * 0.7f;
            }
        } 
        else if (_postBloomSquashTimer >= 0f && _postBloomSquashTimer < PostBloomSquashDuration) {
            currentRect = bloomedRect;
            drawOrigin = new Vector2(currentRect.Width / 2f, currentRect.Height);

            float progress = _postBloomSquashTimer / PostBloomSquashDuration;
            float easedProgress = Easings.SineInEasing(progress, 2f);

            float squashAmount = MathF.Sin(easedProgress * MathHelper.Pi);
            
            drawScale.X = 1f + squashAmount * 0.25f;
            drawScale.Y = 1f - squashAmount * 0.15f;
            
            //drawRotation = MathF.Sin(easedProgress * 3) * 0.07f;
        }
        
        else if (Bloomed) {
            currentRect = bloomedRect;
            drawScale = new Vector2(1f + 0.1f * (float)(0.2 * Math.Sin(Main.GlobalTimeWrappedHourly * 3 * NPC.whoAmI * 0.2f) + 0.5));
            drawScale.Y = 2 - drawScale.X;
            
            drawRotation = MathF.Sin(Main.GlobalTimeWrappedHourly * 3 * NPC.whoAmI * 0.2f) * 0.05f;
        }

        if (!Bloomed) {
            drawScale = new Vector2(1f + 0.1f * (float)(0.2 * Math.Sin(Main.GlobalTimeWrappedHourly * 3 * NPC.whoAmI * 0.2f) + 0.5));
            drawScale.Y = 2 - drawScale.X;
        
            drawRotation = MathF.Sin(Main.GlobalTimeWrappedHourly * 3 * NPC.whoAmI * 0.2f) * 0.05f;
        }
        
        Main.EntitySpriteDraw(
            tex,
            drawPosition,
            currentRect,
            drawColor,
            drawRotation,
            drawOrigin, 
            drawScale,
            SpriteEffects.None
        );
        
        return false;
    }
}