using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
    
namespace AllBeginningsMod.Core.Loaders; 

internal class EffectLoader : ILoadable {
    private readonly static Dictionary<string, Effect> Effects = new();

    /// <param name="name">ex. Nightgaunt</param>
    public static Filter GetFilter(string name) {
        return Filters.Scene[nameof(AllBeginningsMod) + "::" + name] ?? throw new ArgumentException($"No such filter \"{name}\"");
    }

    /// <param name="path">ex. "Filter::Nightgaunt" or "Pixel::FishEye"</param>
    public static Effect GetEffect(string path) {
        return Effects[path] ?? throw new ArgumentException($"No such effect \"{path}\"");
    }

    public void Load(Mod mod) {
        foreach(string filePath in mod.GetFileNames()) {
            if(filePath.EndsWith(".fxc")) {
                Effect effect = mod.Assets.Request<Effect>(filePath.Replace(".fxc", string.Empty), AssetRequestMode.ImmediateLoad).Value;

                string name = Path.GetFileNameWithoutExtension(filePath);
                string directory = Path.GetFileName(Path.GetDirectoryName(filePath));
                if(directory == "Filter") {
                    string sceneShaderName = nameof(AllBeginningsMod) + "::" + name;
                    Filters.Scene[sceneShaderName] = new Filter(new ScreenShaderData(
                        new Ref<Effect>(effect),
                        name + "Pass"
                    ));

                    Filters.Scene[sceneShaderName].Load();
                }

                Effects[$"{directory}::{name}"] = effect;
                mod.Logger.Info($"Loaded effect: {directory}::{name}.");
            }
        }
    }

    public void Unload() { }
}