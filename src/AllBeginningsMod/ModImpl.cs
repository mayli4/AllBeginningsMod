using Daybreak.Common.Features.Authorship;
using Daybreak.Common.Features.ModPanel;

namespace AllBeginningsMod;

partial class ModImpl : Mod, IHasCustomAuthorMessage {
    public string GetAuthorText() => AuthorText.GetAuthorTooltip(this, "Made by:");
}