using Terraria.ModLoader.Config;

namespace AllBeginningsMod.Common.Config;

internal partial class ClientConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static ClientConfig Instance => ModContent.GetInstance<ClientConfig>();
}