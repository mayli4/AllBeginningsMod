using Terraria.ModLoader;

namespace AllBeginningsMod.Core.World;

/// <summary>
///     Declares a <see cref="ModBiome"/> to have a custom lava style./>
/// </summary>
public interface IHasCustomLavaBiome {
    /// <summary>
    ///     The custom lava style of this biome.
    /// </summary>
    public ModLavaStyle ModLavaStyle { get; } 
}