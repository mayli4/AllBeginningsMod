using Microsoft.Xna.Framework.Graphics;

namespace AllBeginningsMod.Core;

public class ManagedShader(Effect effect) {
    public Effect Effect { get; } = effect;
    
    public static implicit operator Effect(ManagedShader shader) => shader.Effect;
}