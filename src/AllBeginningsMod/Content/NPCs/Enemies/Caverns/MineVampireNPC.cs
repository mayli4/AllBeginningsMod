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

namespace AllBeginningsMod.Content.NPCs.Enemies.Caverns; 

internal class MineVampireNPC : VampireNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Caverns.KEY_MineVampireNPC;
    
    protected override float ExplosionRange => 120;
    protected override int MaxExplodingTime => 60;
    protected override void PostSetDefaults() {
        NPC.width = NPC.height = 45;
    }

    protected override void FollowBehaviour(Player target) {
        if(NPC.velocity.LengthSquared() > 1.8f) {
            return;
        }

        NPC.velocity += 0.08f * NPC.Center.DirectionTo(target.Center);
        NPC.direction = MathF.Sign(target.Center.X - NPC.Center.X);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExothermicSoulItem>(), 1, 2, 4));
    }

    protected override void OnExplode() {
        IEntitySource source = NPC.GetSource_Death();
        Gore gore1 = Gore.NewGoreDirect(
            source,
            NPC.Center - Vector2.UnitY * 20f,
            -Vector2.UnitY * Main.rand.NextFloat(5f, 7f),
            Mod.Find<ModGore>("MineVampireGore0").Type
        );

        gore1.position -= new Vector2(gore1.Width, gore1.Height) * 0.5f;

        Gore gore2 = Gore.NewGoreDirect(
            source,
            NPC.Center + Vector2.UnitY * 20f,
            Vector2.UnitY * Main.rand.NextFloat(3f, 5f),
            Mod.Find<ModGore>("MineVampireGore1").Type
        );

        gore2.position -= new Vector2(gore1.Width, gore1.Height) * 0.5f;


        Vector2[] dustPositions = NPC.Center.PositionsAround(14, _ => Main.rand.NextFloat(20f, 50f), out Vector2[] dustDirections, Main.rand.NextFloat());
        for(int i = 0; i < dustPositions.Length; i++) {
            Dust.NewDustPerfect(
                dustPositions[i],
                Main.rand.NextFromList(DustID.Smoke, DustID.TreasureSparkle, DustID.YellowTorch),
                dustDirections[i] * Main.rand.NextFloat(3f, 7f)
            );

            if(i % 3 == 0) {
                Dust.NewDustPerfect(
                    dustPositions[i],
                    ModContent.DustType<MineVampireExplosionDust>(),
                    dustDirections[i] * Main.rand.NextFloat(0.4f, 8f),
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
            progress => Color.Lerp(Color.OrangeRed, Color.DarkGray, -MathF.Pow(progress - 1f, 2) + 1f),
            250,
            120
        );
        Lighting.AddLight(NPC.Center, new Vector3(1.86f, 1.22f, 0.69f) * 3.5f);
        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, NPC.Center);

        Main.instance.CameraModifiers.Add(
            new ExplosionShakeCameraModifier(70f, 0.88f, NPC.Center, 5000, FullName)
        );
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.ZoneRockLayerHeight ? 0.05f : 0f;
    }

    protected override void Draw(SpriteBatch spriteBatch, Color drawColor, float explodingProgress) {
        Texture2D texture = TextureAssets.Npc[Type].Value;

        explodingProgress *= explodingProgress;

        float shake = MathF.Max(explodingProgress * explodingProgress - 0.6f, 0f);
        Vector2 position = NPC.Center - Main.screenPosition + Main.rand.NextVector2Unit() * shake * 16f;
        Vector2 scale = Vector2.One * (1f + 0.25f * explodingProgress);
        float rotation = NPC.rotation + MathF.Sin(Main.GameUpdateCount * 0.3f) * 0.5f * shake;

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
            NPC.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0
        );

        spriteBatch.End();
        spriteBatch.Begin(snapshot);
    }
}