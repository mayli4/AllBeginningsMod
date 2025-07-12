using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace AllBeginningsMod.Common._NPCs;

public interface ISpawnsInSwarm {
    int SwarmSize();
}

internal sealed class SpawnsInSwarmImpl : GlobalNPC {
    public override void OnSpawn(NPC npc, IEntitySource source) {
        foreach(var activeNPC in Main.ActiveNPCs) {
            if(activeNPC is ISpawnsInSwarm swarmNPC) {
                //tbd
            }
        }
    }
}