using AllBeginningsMod.Common.PrimitiveDrawing;
using AllBeginningsMod.Content.Biomes;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public sealed class CursedSpiritNPC : ModNPC {
    byte SpiritType {
        get => (byte)NPC.ai[0];
        set => NPC.ai[0] = value;
    }
    PrimitiveTrail _ghostTrail;
    Vector2 _directionToTarget;

    Player Target => Main.player[NPC.target];

    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Effigy.KEY_CursedSpiritMasks;

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
        SpiritType = (byte)Main.rand.Next(0, 3);
    }

    public override void AI() {
        const float TrailSize = 25;
        _ghostTrail ??= new(Enumerable.Repeat(NPC.Center, 5).ToArray(), static _ => TrailSize);

        NPC.TargetClosest();
        if(Target != null) _directionToTarget = NPC.Center.DirectionTo(Target.Center);

        NPC.velocity += _directionToTarget * 0.2f;
        switch(SpiritType) {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
        }

        var i = _ghostTrail.Positions.Length - 1;
        while(i > 1) {
            _ghostTrail.Positions[i] = _ghostTrail.Positions[i - 1];
            i -= 1;
        }

        var moveDirection = NPC.velocity.SafeNormalize(Vector2.UnitX);
        var movedCenter = NPC.Center + NPC.velocity;
        _ghostTrail.Positions[0] = movedCenter + moveDirection * TrailSize / 2;
        _ghostTrail.Positions[1] = movedCenter - moveDirection * TrailSize / 2;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var maskTexture = TextureAssets.Npc[Type].Value;
        var maskSource = new Rectangle(
            SpiritType switch
            {
                0 => 0,
                1 => 44,
                _ => 100,
            },
            0,
            SpiritType switch
            {
                0 => 44,
                1 => 54,
                _ => 54,
            },
            44
        );

        var originOffset = SpiritType switch
        {
            1 => Vector2.UnitY * 3,
            _ => Vector2.Zero,
        };

        _ghostTrail.Draw(TextureAssets.MagicPixel.Value, Color.White, MathUtilities.WorldTransformationMatrix);
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
