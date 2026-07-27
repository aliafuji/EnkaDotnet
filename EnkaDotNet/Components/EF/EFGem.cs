using System.Collections.Generic;

namespace EnkaDotNet.Components.EF
{
    public class EFGem
    {
        public int Id { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;
        public string OverlayIconUrl { get; internal set; } = string.Empty;
        public int DomainId { get; internal set; }
        public int TotalCost { get; internal set; }
        public IReadOnlyList<EFGemTerm> Terms { get; internal set; } = new List<EFGemTerm>();
    }

    public class EFGemTerm
    {
        public int TermNumId { get; internal set; }
        public int Cost { get; internal set; }
        public int TermType { get; internal set; }
        public string TagId { get; internal set; } = string.Empty;
        public string TagIconUrl { get; internal set; } = string.Empty;
        public string TagName { get; internal set; } = string.Empty;
    }
}
