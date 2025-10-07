using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AllBeginningsMod.Common.Config;

internal partial class ClientConfig {
    [Header("Camera")]
    [DefaultValue(true)]
    public bool EnableScreenshake { get; set; } = true;
}