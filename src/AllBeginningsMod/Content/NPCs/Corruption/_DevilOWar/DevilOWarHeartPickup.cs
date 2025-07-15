using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public class DevilOWarHeartPickup : ModItem {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.DevilOWar.KEY_DevilOWarHeartPickup;

    public override void SetDefaults() {        
        (Item.width, Item.height) = (22, 22);
        
        Item.maxStack = 1;
        Item.noGrabDelay = 0;
    }
    
    public override void GrabRange(Player player, ref int grabRange) {
        if (player.lifeMagnet)
            grabRange += Item.lifeGrabRange;
    }
    
    public override bool ItemSpace(Player Player) => true;
    
    public override bool OnPickup(Player Player) {
        SoundEngine.PlaySound(SoundID.Grab, Player.Center);

        int healthToHeal = Item.damage; 
        if (healthToHeal > 0) {
            Player.statLife += healthToHeal;
            Player.HealEffect(healthToHeal);
        }
        
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        var tex = TextureAssets.Item[Type].Value;
        
        var snapshot = spriteBatch.Capture();
        spriteBatch.End();
        spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive });
        
        for(int i = 0; i < 4; i++) {
            spriteBatch.Draw(
                tex,
                Item.position - Main.screenPosition + Main.rand.NextVector2Unit() * 2f,
                null,
                Color.GreenYellow,
                rotation,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0
            );
        }
        
        spriteBatch.End();
        spriteBatch.Begin(snapshot);
        
        spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, lightColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
        
        return false;
    }
}