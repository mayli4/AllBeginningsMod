using AllBeginningsMod.Common;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;

namespace AllBeginningsMod.Content.NPCs.Bosses;

//had a solver but removed since it sucks, new one tbd

public class NightgauntNPC : ModNPC {
    public override string Texture => Helper.PlaceholderTextureKey;

    IKSkeleton _rightArm;
    IKSkeleton _leftArm;

    public override void SetDefaults() {
        NPC.width = 30;
        NPC.height = 40;
        NPC.damage = 15;
        NPC.defense = 8;
        NPC.lifeMax = 150;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.value = 100f;
        NPC.aiStyle = -1;
        NPC.noGravity = true;

        _rightArm = new(new(40f), new(40f));
        _leftArm = new(new(40f), new(40f));
    }

    public override void AI() {
        _rightArm.Update(Main.LocalPlayer.Center, Main.MouseWorld);
        _leftArm.Update(Main.LocalPlayer.Center, Main.MouseWorld);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if(NPC.IsABestiaryIconDummy) return false;

#if DEBUG
        spriteBatch.DrawLine(
            _rightArm.Position(0) - Main.screenPosition,
            _rightArm.Position(1) - Main.screenPosition,
            drawColor,
            1,
            TextureAssets.BlackTile.Value
        );

        spriteBatch.DrawLine(
            _rightArm.Position(1) - Main.screenPosition,
            _rightArm.Position(1) + _rightArm.Offset(1) - Main.screenPosition,
            drawColor,
            1,
            TextureAssets.BlackTile.Value
        );

        spriteBatch.DrawLine(
            _leftArm.Position(0) - Main.screenPosition,
            _leftArm.Position(1) - Main.screenPosition,
            drawColor,
            1,
            TextureAssets.BlackTile.Value
        );

        spriteBatch.DrawLine(
            _leftArm.Position(1) - Main.screenPosition,
            _leftArm.Position(1) + _leftArm.Offset(1) - Main.screenPosition,
            drawColor,
            1,
            TextureAssets.BlackTile.Value
        );
#endif

        return true;
    }
}