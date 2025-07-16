using AllBeginningsMod.Common.PrimitiveDrawing;
using AllBeginningsMod.Content.Biomes;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public sealed class CursedSpiritNPC : ModNPC {
    byte _spiritType;
    PrimitiveTrail _ghostTrail;
    Vector2 _directionToTarget;

    Player Target => Main.player[NPC.target];

    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Effigy.KEY_CursedSpiritMasks;

    static Color _ghostColor1 = new(214, 237, 5);
    static Color _ghostColor2 = new(181, 200, 4);

    public override void SetDefaults() {
        NPC.width = 32;
        NPC.height = 32;
        NPC.lifeMax = 640;
        NPC.value = 250f;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void OnSpawn(IEntitySource source) {
        _spiritType = (byte)Main.rand.Next(0, 3);
    }

    public override void AI() {
        const float TrailSize = 55;
        _ghostTrail ??= new(
            Enumerable.Repeat(NPC.Center, 14).ToArray(),
            static t => TrailSize,
            static t => Color.Lerp(_ghostColor1, _ghostColor2, t + 0.7f)
        );

        NPC.TargetClosest();
        if(Target != null) _directionToTarget = NPC.Center.DirectionTo(Target.Center);

        NPC.velocity += _directionToTarget * 0.2f;
        switch(_spiritType) {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
        }

        var i = _ghostTrail.Positions.Length - 1;
        while(i > 0) {
            _ghostTrail.Positions[i] = _ghostTrail.Positions[i - 1];
            i -= 1;
        }
        _ghostTrail.Positions[0] = NPC.Center;

        if(!Main.dedServ) {
            if(Main.rand.NextBool(7)) Dust.NewDust(
                NPC.position,
                NPC.width,
                NPC.height,
                DustID.Pixie,
                newColor: Main.rand.NextFromList(_ghostColor1, _ghostColor2)
            );

            Lighting.AddLight(NPC.Center, _ghostColor1.ToVector3() * 0.75f);
        }

        var maxRotation = 0.25f;
        NPC.rotation = Math.Clamp(NPC.velocity.X * 0.025f, -maxRotation, maxRotation);

        // var moveDirection = NPC.velocity.SafeNormalize(Vector2.UnitX);
        // var movedCenter = NPC.Center + NPC.velocity;
        // _ghostTrail.Positions[0] = movedCenter + moveDirection * TrailSize / 2;
        // _ghostTrail.Positions[1] = movedCenter - moveDirection * TrailSize / 2;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if(!NPC.IsABestiaryIconDummy) {
            var trailEffect = Assets.Assets.Effects.Compiled.Trail.CursedSpiritFire.Value;
            trailEffect.Parameters["time"].SetValue(0.025f * Main.GameUpdateCount);
            trailEffect.Parameters["mat"].SetValue(MathUtilities.WorldTransformationMatrix);
            trailEffect.Parameters["stepY"].SetValue(0.25f);
            trailEffect.Parameters["texture1"].SetValue(Assets.Assets.Textures.Sample.Pebbles.Value);
            trailEffect.Parameters["texture2"].SetValue(Assets.Assets.Textures.Sample.Noise2.Value);
            _ghostTrail.Draw(trailEffect);
        }

        var snapshot = spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        var glowTexture = Assets.Assets.Textures.Sample.Glow1.Value;
        var t = (MathF.Sin(0.1f * Main.GameUpdateCount + 23.2f * NPC.whoAmI) + MathF.Cos(0.06f * Main.GameUpdateCount) + 2f) / 4f;
        spriteBatch.Draw(
            glowTexture,
            NPC.Center - screenPos,
            null,
            _ghostColor2 * (0.3f + 0.3f * t),
            0f,
            glowTexture.Size() * 0.5f,
            0.45f,
            SpriteEffects.None,
            0
        );

        spriteBatch.Draw(
            glowTexture,
            NPC.Center - screenPos,
            null,
            _ghostColor1,
            0f,
            glowTexture.Size() * 0.5f,
            0.2f,
            SpriteEffects.None,
            0
        );

        spriteBatch.EndBegin(snapshot);

        var maskTexture = TextureAssets.Npc[Type].Value;
        var maskSource = new Rectangle(
            _spiritType switch
            {
                0 => 0,
                1 => 44,
                _ => 100,
            },
            0,
            _spiritType switch
            {
                0 => 44,
                1 => 54,
                _ => 54,
            },
            44
        );

        var originOffset = _spiritType switch
        {
            1 => Vector2.UnitY * 3,
            _ => Vector2.Zero,
        };

        Main.EntitySpriteDraw(
            maskTexture,
            NPC.Center - screenPos,
            maskSource,
            drawColor,
            NPC.rotation,
            maskSource.Size() / 2f + originOffset,
            NPC.scale,
            NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
        );

        return false;
    }
}
