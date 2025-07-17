using AllBeginningsMod.Common.Bases.NPCs;
using AllBeginningsMod.Content.CameraModifiers;
using AllBeginningsMod.Content.Dusts;
using AllBeginningsMod.Content.Items.Materials;
using AllBeginningsMod.Content.Projectiles;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Nighttime;

internal class DevilVampireNPC : VampireNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Nighttime.KEY_DevilVampireNPC;
    
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 2;
    }

    protected override float FollowRange => 6000;
    protected override float ExplosionRange => 160;
    protected override int MaxExplodingTime => 60;
    protected override void PostSetDefaults() {
        NPC.width = NPC.height = 75;
        NPC.life = 250;
    }

    protected override void FollowBehaviour(Player target) {
        if(NPC.velocity.LengthSquared() > 1.2f) {
            return;
        }

        NPC.velocity += 0.05f * NPC.Center.DirectionTo(target.Center);
        NPC.direction = MathF.Sign(target.Center.X - NPC.Center.X);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExothermicSoulItem>(), 1, 2, 4));
    }

    public override void FindFrame(int frameHeight) {
        if(IsExploding) {
            NPC.frame = new(0, 0, 84, frameHeight);
            return;
        }

        int frameTime = 20;
        if(NPC.frameCounter++ > frameTime * 2) {
            NPC.frameCounter = 0d;
        }

        NPC.frame = new(0, ((int)NPC.frameCounter / frameTime % 2) * frameHeight, 84, frameHeight);
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.ZoneOverworldHeight && !Main.dayTime ? 0.04f : 0f;
    }

    protected override void OnExplode() {
        IEntitySource source = NPC.GetSource_Death();
        if(Main.netMode != NetmodeID.MultiplayerClient) {
            Projectile.NewProjectile(
                source,
                NPC.Center,
                Vector2.Zero,
                ModContent.ProjectileType<DevilVampireAuraProjectile>(),
                NPC.damage,
                0f
            );
        }

        if(Main.dedServ) {
            return;
        }

        Gore gore1 = Gore.NewGoreDirect(
            source,
            NPC.Center,
            (NPC.direction == -1 ? MathHelper.Pi * 0.75f : MathHelper.PiOver2 * 0.5f).ToRotationVector2() * Main.rand.NextFloat(3f, 5f),
            Mod.Find<ModGore>("DevilVampireGore0").Type
        );

        gore1.position -= new Vector2(gore1.Width, gore1.Height) * 0.5f;

        for(int i = 0; i < 4; i++) {
            Vector2 direction = (
                NPC.direction == -1 ? Main.rand.NextFloat(-MathHelper.PiOver2, 0f) : Main.rand.NextFloat(-MathHelper.Pi, -MathHelper.PiOver2)
            ).ToRotationVector2();

            Gore spikeGore = Gore.NewGoreDirect(
                source,
                NPC.Center + direction * 16f,
                direction * Main.rand.NextFloat(4f, 10f),
                Mod.Find<ModGore>("DevilVampireGore1").Type
            );

            spikeGore.position -= new Vector2(spikeGore.Width, spikeGore.Height) * 0.5f;
        }

        Vector2[] dustPositions = NPC.Center.PositionsAround(18, _ => Main.rand.NextFloat(20f, 50f), out Vector2[] dustDirections, Main.rand.NextFloat());
        for(int i = 0; i < dustPositions.Length; i++) {
            Dust.NewDustPerfect(
                dustPositions[i],
                Main.rand.NextFromList(DustID.Smoke, DustID.TreasureSparkle, DustID.YellowTorch),
                dustDirections[i] * Main.rand.NextFloat(3f, 7f)
            );

            if(i % 3 == 0) {
                Dust.NewDustPerfect(
                    dustPositions[i],
                    ModContent.DustType<DevilVampireExplosionDust>(),
                    dustDirections[i] * Main.rand.NextFloat(0.6f, 7f),
                    Main.rand.Next(30, 50),
                    Color.White,
                    Main.rand.NextFloat(0.6f, 1.0f)
                );
            }
        }



        ExplosionVFXProjectile.Spawn(
            source,
            NPC.Center,
            Color.Yellow,
            Color.OrangeRed,
            progress => Color.Lerp(Color.OrangeRed, Color.Black, -MathF.Pow(progress - 1f, 2) + 1f),
            300,
            130
        );

        Lighting.AddLight(NPC.Center, new Vector3(1.86f, 1.22f, 0.69f) * 5f);
        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, NPC.Center);

        Main.instance.CameraModifiers.Add(
            new ExplosionShakeCameraModifier(70f, 0.88f, NPC.Center, 5000, FullName)
        );
    }

    protected override void Draw(SpriteBatch spriteBatch, Color drawColor, float explodingProgress) {
        Texture2D texture = TextureAssets.Npc[Type].Value;

        float shake = MathF.Max(explodingProgress * explodingProgress - 0.66f, 0f);
        Vector2 position = NPC.Center - Main.screenPosition + Main.rand.NextVector2Unit() * shake * 16f;
        Vector2 scale = Vector2.One * (1f + 0.25f * explodingProgress);
        float rotation = NPC.rotation + MathF.Sin(Main.GameUpdateCount * 0.3f) * 0.4f * shake;

        for(int i = 0; i < 4; i++) {
            spriteBatch.Draw(
                texture,
                position + Main.rand.NextVector2Unit() * 4f,
                NPC.frame,
                Color.Black * 0.1f,
                rotation,
                NPC.frame.Size() * 0.5f,
                scale,
                NPC.direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );
        }

        Effect fishEyeEffect = Assets.Assets.Effects.Compiled.Pixel.FishEye.Value;
        fishEyeEffect.Parameters["strength"].SetValue(explodingProgress * 1.8f);
        fishEyeEffect.Parameters["uImageSize0"].SetValue(texture.Size());
        fishEyeEffect.Parameters["uSourceRect"].SetValue(new Vector4(NPC.frame.X, NPC.frame.Y, NPC.frame.Width, NPC.frame.Height));
        fishEyeEffect.Parameters["center"].SetValue(new Vector2(0.5f, 0.5f));

        SpriteBatchSnapshot snapshot = spriteBatch.Capture();
        spriteBatch.End();
        spriteBatch.Begin(snapshot with { CustomEffect = fishEyeEffect });

        spriteBatch.Draw(
            texture,
            position,
            NPC.frame,
            drawColor,
            rotation,
            NPC.frame.Size() * 0.5f,
            scale,
            NPC.direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0
        );

        spriteBatch.End();
        spriteBatch.Begin(snapshot);
    }
}