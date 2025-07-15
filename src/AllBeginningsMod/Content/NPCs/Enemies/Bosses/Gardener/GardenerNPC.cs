using AllBeginningsMod.Common.PrimitiveDrawing;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Bosses.Gardener;

internal class GardenerNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Bosses.Gardener.KEY_GardenerNPC_Body;

    private PrimitiveTrail[] intestineTrails;
    private Vector2 twitchOffset;
    private int twitchTimer;
    public override void SetDefaults() {
        NPC.width = 90;
        NPC.height = 140;
        NPC.knockBackResist = 0f;

        NPC.defense = 100;
        NPC.damage = 80;
        NPC.lifeMax = 9999999;

        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;

        NPC.alpha = 0;
        NPC.aiStyle = -1;

        NPC.npcSlots = 40f;

        NPC.HitSound = SoundID.NPCHit2;

        /*if (!Main.dedServ)
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music");*/

        intestineTrails = new PrimitiveTrail[4];
        for(int i = 0; i < intestineTrails.Length; i++) {
            intestineTrails[i] = new(new Vector2[8], factor => 26);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        twitchOffset *= 0.85f;
        Vector2 bodyDrawPosition = NPC.Center - Main.screenPosition + twitchOffset - Vector2.UnitY * 10;
        Color mainDrawColor = drawColor * (1f - (Math.Clamp(NPC.alpha, 0, 255) / 255f));
        if(twitchTimer == 0) {
            twitchOffset = Main.rand.NextVector2Unit() * 5f;
            twitchTimer = Main.rand.Next(30, 80);
        }
        else {
            twitchTimer--;
        }

        Texture2D armLeftTexture = ModContent.Request<Texture2D>(Texture.Replace("Body", "ArmLeft"), AssetRequestMode.ImmediateLoad).Value;
        Main.spriteBatch.Draw(
            armLeftTexture,
            bodyDrawPosition + new Vector2(46, -60).RotatedBy(NPC.rotation),
            null,
            mainDrawColor,
            NPC.rotation + MathF.Sin(Main.GameUpdateCount * 0.035f + 0.5f) * MathHelper.PiOver4 * 0.05f,
            Vector2.Zero,
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        Texture2D armRightTexture = ModContent.Request<Texture2D>(Texture.Replace("Body", "ArmRight"), AssetRequestMode.ImmediateLoad).Value;
        Main.spriteBatch.Draw(
            armRightTexture,
            bodyDrawPosition + new Vector2(-46, -35).RotatedBy(NPC.rotation),
            null,
            mainDrawColor,
            NPC.rotation - MathF.Sin(Main.GameUpdateCount * 0.0275f) * MathHelper.PiOver4 * 0.1f,
            new Vector2(armLeftTexture.Width, 0f),
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        Texture2D mainTexture = TextureAssets.Npc[Type].Value;
        Main.spriteBatch.Draw(
            mainTexture,
            bodyDrawPosition,
            null,
            mainDrawColor,
            NPC.rotation,
            mainTexture.Size() * 0.5f,
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        SpriteBatchSnapshot capture = spriteBatch.Capture();
        spriteBatch.End();
        spriteBatch.Begin(capture);

        DrawTrails(bodyDrawPosition, mainDrawColor);

        return false;
    }

    private readonly Vector2[] initialIntestinePositions = {
        new Vector2(-11, 12),
        new Vector2(-4, 21),
        new Vector2(19, 11),
        new Vector2(14, 17)
    };

    private void DrawTrails(Vector2 bodyDrawPosition, Color drawColor) {
        static float IntestineEquation(float x) {
            return 0.2f * MathF.Sin(x) + 0.8f * MathF.Cos(x + MathHelper.PiOver4);
        }

        for(int i = 0; i < intestineTrails.Length; i++) {
            Vector2[] positions = new Vector2[intestineTrails[i].MaxTrailPositions];
            positions[0] = initialIntestinePositions[i];
            Vector2 moveDirection = positions[0].SafeNormalize(Vector2.Zero);
            for(int j = 1; j < intestineTrails[i].MaxTrailPositions; j++) {
                float factor = j / (intestineTrails[i].MaxTrailPositions - 1f);
                positions[j] = positions[0]
                    + moveDirection * MathHelper.Lerp(110, 130, MathF.Sin(Main.GameUpdateCount * (0.02f + i * 0.003f) + i * 0.6f)) * factor
                    + moveDirection.RotatedBy(MathHelper.PiOver2) * IntestineEquation(Main.GameUpdateCount * (0.04f + i * 0.005f) + factor * 4f + factor + i * 0.4f) * 20f;
            }

            intestineTrails[i].Positions = positions.Select(position => position + bodyDrawPosition).ToArray();
        }

        Texture2D intestineTexture = ModContent.Request<Texture2D>(Texture.Replace("Body", "Intestine"), AssetRequestMode.ImmediateLoad).Value;
        Matrix transformationMatrix = Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
        for(int i = 0; i < intestineTrails.Length; i++) {
            intestineTrails[i].Draw(intestineTexture, drawColor, transformationMatrix);
        }
    }
}