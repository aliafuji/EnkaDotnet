using System.Collections.Generic;
using EnkaDotNet.Enums.EF;
using EnkaDotNet.Utils.EF;

namespace EnkaDotNet.Components.EF
{
    public class EFEquip
    {
        private string _slotName;

        public EFEquipSlot Slot { get; internal set; }

        public string SlotName
        {
            get => !string.IsNullOrEmpty(_slotName) ? _slotName : EFStatsHelpers.GetEquipSlotName(Slot);
            internal set => _slotName = value;
        }

        public int Id { get; internal set; }
        public string SuitId { get; internal set; } = string.Empty;
        public string SuitName { get; internal set; } = string.Empty;
        public string SuitIconUrl { get; internal set; } = string.Empty;
        public int Rarity { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;
        public IReadOnlyList<EFStat> Attributes { get; internal set; } = new List<EFStat>();
    }
}
