using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace AllBeginningsMod.Content.Jungle;

internal sealed class JungleWorld : ModSystem {
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int surfaceIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));

        if(surfaceIndex != -1) {
            tasks.Insert(surfaceIndex + 1, new JungleBushes("Jungle Bushes", 1.0f));
        }
    }
}