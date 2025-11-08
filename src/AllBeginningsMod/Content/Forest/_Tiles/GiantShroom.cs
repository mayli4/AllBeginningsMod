using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Forest;

//okk so, if were gonna have lots of tiles that have stuff like this i should probably come up with a dummy system or something

public sealed class GiantShroom : ModTile {
    public override string Texture => Assets.Textures.Tiles.Forest.GiantShroomStem.KEY;

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = false;
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);
    }

    //lol sorry for all the magic numbers
    public override void NearbyEffects(int i, int j, bool closer) {
        var tile = Main.tile[i, j];

        if(tile.TileFrameX == 0 && tile.TileFrameY == 0) {
            bool capExists = Main.projectile.Any(
                p =>
                    p.active &&
                    p.type == ModContent.ProjectileType<GiantShroomCapDummy>() &&
                    (p.ModProjectile as GiantShroomCapDummy)?.ParentPosition ==
                    new Point(i, j)
            );

            if(!capExists) {
                int projIndex = Projectile.NewProjectile(
                    new EntitySource_TileUpdate(i, j),
                    new Vector2(i * 16 + 15, j * 16 - 14),
                    Vector2.Zero,
                    ModContent.ProjectileType<GiantShroomCapDummy>(),
                    0,
                    0f,
                    Main.myPlayer
                );

                if(Main.projectile[projIndex].ModProjectile is GiantShroomCapDummy capDummy) {
                    capDummy.ParentPosition = new Point(i, j);
                }
            }
        }
    }

    internal sealed class GiantShroomCapDummy : ModProjectile {
        public Point ParentPosition { get; set; }

        public override string Texture => Assets.Textures.Tiles.Forest.GiantShroomCap.KEY;

        private int _squishTimer;
        private float _bounceState;
        private float _leanDirection;

        private const int squish_animation_duration = 60;
        private const float max_lean_radians = 0.3f;

        private const float idle_wobble_amount = 0.02f;
        private const float idle_wobble_speed = 0.8f;

        private const int heart_spawn_cooldown = 60 * 15;
        private int _heartSpawnTimer = heart_spawn_cooldown;

        public override void SetDefaults() {
            Projectile.width = 70;
            Projectile.height = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 10;
        }

        public override void AI() {
            Tile parentTile = Main.tile[ParentPosition.X, ParentPosition.Y];
            if(
                parentTile.HasTile &&
                parentTile.TileType == ModContent.TileType<GiantShroom>()
            ) {
                Projectile.timeLeft = 10;
            }
            else {
                Projectile.Kill();
                return;
            }

            if(_heartSpawnTimer > 0) {
                _heartSpawnTimer--;
            }

            if(_bounceState == 0 && Main.LocalPlayer.velocity.Y > 0 && Main.LocalPlayer.Hitbox.Intersects(Projectile.Hitbox)) {
                _bounceState = 1;
                _leanDirection = Math.Sign(Main.LocalPlayer.velocity.X);

                for(int i = 0; i < 15; i++) {
                    Dust.NewDust(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height / 2,
                        DustID.Water,
                        Main.LocalPlayer.velocity.X * 0.2f,
                        -2f,
                        80,
                        default,
                        1.2f
                    );
                }

                if(Main.LocalPlayer.velocity.Y > -10) {
                    Main.LocalPlayer.velocity.Y = -10;
                }

                if(Main.LocalPlayer.velocity.X != 0) {
                    Main.LocalPlayer.velocity.X +=
                        (Main.LocalPlayer.velocity.X > 0 ? 1 : -1) * 3f;
                }

                if(_heartSpawnTimer <= 0) {
                    Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ItemID.Heart, 3);
                    _heartSpawnTimer = heart_spawn_cooldown;
                }
            }

            if(_bounceState == 1) {
                _squishTimer++;
            }

            if(_squishTimer >= 90) {
                _bounceState = 0;
                _squishTimer = 0;
                _leanDirection = 0;
            }
        }

        public override bool PreDraw(ref Color lightColor) {
            var tex = TextureAssets.Projectile[Projectile.type].Value;
            var pos = Projectile.position + new Vector2(36, 40) - Main.screenPosition;
            var origin = new Vector2(tex.Width / 2f, tex.Height);
            Vector2 scale = Vector2.One;

            float rotation;
            if(_bounceState == 1 && _squishTimer <= squish_animation_duration) {
                float animationProgress =
                    (float)_squishTimer / squish_animation_duration;

                float decay = 5f;
                float frequency = 15f;
                float jiggleFactor = (float)Math.Pow(Math.E, -decay * animationProgress) * (float)Math.Sin(frequency * animationProgress);

                scale.Y = 1f + jiggleFactor * 0.4f;
                scale.X = 1f - jiggleFactor * 0.4f;

                float leanProgress = Math.Clamp(animationProgress, 0f, 1f);
                float currentLeanFactor = 1f - leanProgress;
                rotation = _leanDirection * max_lean_radians * currentLeanFactor;
            }
            else {
                float time = Main.GameUpdateCount * 0.05f;

                float wobbleX = (float)Math.Sin(time * idle_wobble_speed);
                scale.X = 1 + wobbleX * idle_wobble_amount;

                float wobbleY = (float)Math.Sin(time * idle_wobble_speed + 1);
                scale.Y = 1 + wobbleY * idle_wobble_amount;

                rotation = 0f;
            }

            Main.EntitySpriteDraw(
                tex,
                pos,
                null,
                lightColor,
                rotation,
                origin,
                scale,
                SpriteEffects.None
            );

            return false;
        }
    }
}