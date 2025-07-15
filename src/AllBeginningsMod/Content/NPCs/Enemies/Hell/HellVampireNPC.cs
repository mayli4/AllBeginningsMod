using AllBeginningsMod.Common.Bases.NPCs;
using AllBeginningsMod.Content.CameraModifiers;
using AllBeginningsMod.Content.Dusts;
using AllBeginningsMod.Content.Items.Materials;
using AllBeginningsMod.Content.Projectiles;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Hell;

public sealed class HellVampireNPC : VampireNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Hell.KEY_HellVampireNPC;
    
    protected override float ExplosionRange => 120;
    protected override int MaxExplodingTime => 60;

    protected override void PostSetDefaults() {
        NPC.lavaImmune = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.ZoneUnderworldHeight ? 0.15f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExothermicSoulItem>(), 1, 2, 4));
    }

    protected override void OnExplode() {
        IEntitySource source = NPC.GetSource_Death();
        Gore gore1 = Gore.NewGoreDirect(
            source,
            NPC.Center - Vector2.One * 20f,
            -Vector2.One * Main.rand.NextFloat(3f, 5f),
            Mod.Find<ModGore>("HellVampireGore0").Type
        );

        gore1.position -= new Vector2(gore1.Width, gore1.Height) * 0.5f;

        Gore gore2 = Gore.NewGoreDirect(
            source,
            NPC.Center + Vector2.One * 20f,
            Vector2.One * Main.rand.NextFloat(3f, 5f),
            Mod.Find<ModGore>("HellVampireGore1").Type
        );

        gore2.position -= new Vector2(gore2.Width, gore2.Height) * 0.5f;

        Vector2[] dustPositions = NPC.Center.PositionsAround(13, _ => Main.rand.NextFloat(20f, 30f), out Vector2[] dustDirections, Main.rand.NextFloat());
        for(int i = 0; i < dustPositions.Length; i++) {
            Dust.NewDustPerfect(
                dustPositions[i],
                Main.rand.NextFromList(DustID.Smoke, DustID.TreasureSparkle, DustID.YellowTorch),
                dustDirections[i] * Main.rand.NextFloat(3f, 6f)
            );

            if(i % 3 == 0) {
                Dust.NewDustPerfect(
                    dustPositions[i],
                    ModContent.DustType<HellVampireExplosionDust>(),
                    dustDirections[i] * Main.rand.NextFloat(0.6f, 7f),
                    Main.rand.Next(30, 50),
                    Color.White,
                    Main.rand.NextFloat(0.4f, 0.9f)
                );
            }
        }

        ExplosionVFXProjectile.Spawn(
            source,
            NPC.Center,
            Color.Yellow,
            Color.OrangeRed, progress => Color.Lerp(Color.Orange, Color.Gray, progress * progress),
            200,
            120
        );

        Lighting.AddLight(NPC.Center, new Vector3(1.86f, 1.22f, 0.69f) * 3.5f);
        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, NPC.Center);

        Main.instance.CameraModifiers.Add(
            new ExplosionShakeCameraModifier(70f, 0.88f, NPC.Center, 5000, FullName)
        );
    }

    protected override void Draw(SpriteBatch spriteBatch, Color drawColor, float explodingProgress) {
        Texture2D texture = TextureAssets.Npc[Type].Value;
        explodingProgress *= explodingProgress;

        float shake = MathF.Max(explodingProgress * explodingProgress - 0.66f, 0f);
        Vector2 position = NPC.Center - Main.screenPosition + Main.rand.NextVector2Unit() * shake * 16f;
        Vector2 scale = Vector2.One * (1f + 0.25f * explodingProgress);
        float rotation = NPC.rotation + MathF.Sin(Main.GameUpdateCount * 0.3f) * 0.4f * shake;

        Effect fishEyeEffect = Assets.Assets.Effects.Compiled.Pixel.FishEye.Value;
        fishEyeEffect.Parameters["strength"].SetValue(explodingProgress * 2f);
        fishEyeEffect.Parameters["uImageSize0"].SetValue(texture.Size());
        fishEyeEffect.Parameters["uSourceRect"].SetValue(new Vector4(0f, 0f, texture.Width, texture.Height));
        fishEyeEffect.Parameters["center"].SetValue(Vector2.One * 0.5f);

        SpriteBatchSnapshot snapshot = spriteBatch.Capture();
        spriteBatch.End();
        spriteBatch.Begin(snapshot with { CustomEffect = fishEyeEffect });

        spriteBatch.Draw(
            texture,
            position,
            null,
            drawColor,
            rotation,
            texture.Size() * 0.5f,
            scale,
            SpriteEffects.None,
            0
        );

        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Additive,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            Main.Rasterizer,
            fishEyeEffect,
            Main.GameViewMatrix.TransformationMatrix
        );

        float bloomScaling = 0.2f;
        Texture2D texBloom0 = ModContent.Request<Texture2D>(Texture + "_Glow", AssetRequestMode.ImmediateLoad).Value;
        spriteBatch.Draw(
            texBloom0,
            position,
            null,
            Color.White * explodingProgress * 1.1f,
            rotation,
            texBloom0.Size() * 0.5f,
            scale,
            SpriteEffects.None,
            0
        );

        Texture2D texBloom1 = ModContent.Request<Texture2D>(Texture + "_Bloom", AssetRequestMode.ImmediateLoad).Value;
        spriteBatch.Draw(
            texBloom1,
            position,
            null,
            Color.White * (explodingProgress * 2f - 1f) * 1.1f,
            rotation,
            texBloom1.Size() * 0.5f,
            scale * bloomScaling,
            SpriteEffects.None,
            0
        );

        spriteBatch.End();
        spriteBatch.Begin(snapshot);
    }
}