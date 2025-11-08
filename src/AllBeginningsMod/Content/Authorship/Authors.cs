using Daybreak.Common.Features.Authorship;
using JetBrains.Annotations;

namespace AllBeginningsMod.Content.Authorship;

public abstract class CommonAuthorTag : AuthorTag {
    private const string suffix = "Tag";
    public override string Name => base.Name.EndsWith(suffix) ? base.Name[..^suffix.Length] : base.Name;
    public override string Texture => string.Join('/', Assets.Textures.UI.Tobias.KEY.Split('/')[..^1]) + '/' + Name;
}

[UsedImplicitly] public class MathicaTag : CommonAuthorTag;
[UsedImplicitly] public class TobiasTag : CommonAuthorTag;
[UsedImplicitly] public class WaffTag : CommonAuthorTag;
[UsedImplicitly] public class ScssTag : CommonAuthorTag;