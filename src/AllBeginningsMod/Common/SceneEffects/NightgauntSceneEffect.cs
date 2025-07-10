using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Content.NPCs.Enemies.Bosses.Nightgaunt;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace AllBeginningsMod.Common.SceneEffects; 
[Autoload(Side = ModSide.Client)]
internal class NightgauntSceneEffect : ModSceneEffect {
    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
    private NightgauntNPC nightgaunt;

    /*[Effect("WaterFilter")]
    private static Effect waterEffect;*/
    public override void SpecialVisuals(Player player, bool isActive) {
        Effect effect = EffectLoader.GetEffect("Filter::Nightgaunt");
        effect.Parameters["smooth"].SetValue(0.4f);
        effect.Parameters["radius"].SetValue(0.85f);
        player.ManageSpecialBiomeVisuals("AllBeginningsMod::Nightgaunt", isActive);

        // Water effect for the shielding attack
        /*Effect waterEffect = EffectLoader.GetEffect("Filter::Screen");
        waterEffect.Parameters["noise"].SetValue(
            Mod.Assets.Request<Texture2D>("Assets/Images/Sample/Noise3", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value
        );

        waterEffect.Parameters["strength"].SetValue(
            0.1f
        );
        player.ManageSpecialBiomeVisuals(
            "AllBeginningsMod:WaterFilter",
            isActive && nightgaunt is not null && nightgaunt.Attack == NightgauntNPC.Attacks.Shield
        );*/
    }

    public override bool IsSceneEffectActive(Player player) {
        NPC npc = Main.npc.FirstOrDefault(npc => npc.type == ModContent.NPCType<NightgauntNPC>());
        if(npc is null) {
            return false;
        }

        nightgaunt = (NightgauntNPC)npc.ModNPC;
        return npc.active && npc.life > 0;
    }
}