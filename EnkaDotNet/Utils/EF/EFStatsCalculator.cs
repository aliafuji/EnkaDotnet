using System;
using System.Collections.Generic;
using System.Linq;
using EnkaDotNet.Assets.EF;
using EnkaDotNet.Assets.EF.Models;
using EnkaDotNet.Components.EF;
using EnkaDotNet.Enums.EF;
using EnkaDotNet.Models.EF;

namespace EnkaDotNet.Utils.EF
{
    public class EFOperatorStatResult
    {
        public Dictionary<int, double> Attrs { get; set; } = new Dictionary<int, double>();
        public double Hp { get; set; }
        public double Atk { get; set; }
        public double WeaponAtk { get; set; }
    }

    public static class EFStatsCalculator
    {
        public static EFOperatorStatResult CalculateOperatorStats(EFOperator op, IEFAssets assets)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (assets == null) throw new ArgumentNullException(nameof(assets));

            var avatar = assets.GetAvatarInfo(op.Id);
            if (avatar == null)
            {
                return new EFOperatorStatResult();
            }

            return Calculate(
                avatar,
                op.Level,
                op.PotentialLevel,
                op.AttrNodes,
                op.Equips,
                op.Weapon,
                assets);
        }

        public static EFOperatorStatResult CalculateFromModel(EFCharDataModel model, IEFAssets assets)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (assets == null) throw new ArgumentNullException(nameof(assets));

            var avatar = assets.GetAvatarInfo(model.TemplateId);
            if (avatar == null)
            {
                return new EFOperatorStatResult();
            }

            var equips = new List<EFEquip>();
            if (model.Equip != null)
            {
                foreach (var entry in model.Equip)
                {
                    if (entry?.Value == null) continue;
                    equips.Add(BuildTempEquip(entry, assets));
                }
            }

            EFWeapon weapon = null;
            if (model.Weapon != null)
            {
                weapon = BuildTempWeapon(model.Weapon, assets);
            }

            return Calculate(
                avatar,
                model.Level,
                model.PotentialLevel,
                model.Talent?.AttrNodes,
                equips,
                weapon,
                assets);
        }

        private static EFEquip BuildTempEquip(EFEquipEntryModel entry, IEFAssets assets)
        {
            var item = assets.GetEquipItem(entry.Value.TemplateId);
            var attributes = new List<EFStat>();
            var enhanceMap = new Dictionary<int, int>();
            if (entry.Value.Enhance != null)
            {
                foreach (var enh in entry.Value.Enhance)
                {
                    enhanceMap[enh.Key] = enh.Value;
                }
            }

            if (item?.AttrModifiers != null)
            {
                for (int i = 0; i < item.AttrModifiers.Count; i++)
                {
                    var mod = item.AttrModifiers[i];
                    int enhanceLevel = enhanceMap.TryGetValue(i, out int el) ? el : 0;
                    double value = GetValueAt(mod.Values, enhanceLevel);
                    attributes.Add(new EFStat
                    {
                        AttrId = mod.AttrType,
                        Type = MapAttrType(mod.AttrType),
                        Value = value,
                        EnhanceLevel = enhanceLevel
                    });
                }
            }

            return new EFEquip
            {
                Slot = (EFEquipSlot)entry.Key,
                Id = entry.Value.TemplateId,
                SuitId = item?.SuitId ?? string.Empty,
                Attributes = attributes
            };
        }

        private static EFWeapon BuildTempWeapon(EFWeaponModel model, IEFAssets assets)
        {
            var info = assets.GetWeaponInfo(model.TemplateId);
            double weaponAtk = 0;
            if (info != null)
            {
                weaponAtk = assets.GetWeaponAtk(info.LevelTemplateId, model.WeaponLv);
            }

            EFGem gem = null;
            if (model.AttachedGem != null)
            {
                var terms = new List<EFGemTerm>();
                if (model.AttachedGem.Terms != null)
                {
                    foreach (var term in model.AttachedGem.Terms)
                    {
                        var termInfo = assets.GetGemTermTag(term.TermNumId);
                        terms.Add(new EFGemTerm
                        {
                            TermNumId = term.TermNumId,
                            Cost = term.Cost,
                            TagId = termInfo?.TagId ?? string.Empty
                        });
                    }
                }

                gem = new EFGem
                {
                    Id = model.AttachedGem.TemplateId,
                    DomainId = model.AttachedGem.DomainId,
                    TotalCost = model.AttachedGem.TotalCost,
                    Terms = terms
                };
            }

            return new EFWeapon
            {
                Id = model.TemplateId,
                Level = model.WeaponLv,
                BreakthroughLevel = model.BreakthroughLv,
                RefineLevel = model.RefineLv,
                BaseAtk = weaponAtk,
                Gem = gem
            };
        }

        private static EFOperatorStatResult Calculate(
            EFAvatarAssetInfo avatar,
            int level,
            int potentialLevel,
            IReadOnlyList<string> attrNodes,
            IReadOnlyList<EFEquip> equips,
            EFWeapon weapon,
            IEFAssets assets)
        {
            var attrs = new Dictionary<int, double>();
            ApplyBaseAttributes(attrs, avatar, level);
            ApplyAttrNodes(attrs, avatar, attrNodes);
            ApplyPotentialAttributes(attrs, avatar, potentialLevel);
            var suitCounts = ApplyEquipAttributes(attrs, equips);

            double weaponAtk = weapon?.BaseAtk ?? 0;
            if (weapon != null)
            {
                ApplyWeaponSkills(attrs, weapon, assets);
            }

            ApplySuitBonuses(attrs, suitCounts, assets);

            double strength = Math.Floor(GetOrDefault(attrs, (int)EFAttrType.Strength));
            double baseHp = GetLevelValue(avatar.BaseHpByLevel, level);
            double hp = baseHp + strength * 5.0;

            double mainAttr = Math.Floor(GetOrDefault(attrs, avatar.MainAttrId));
            double subAttr = Math.Floor(GetOrDefault(attrs, avatar.SubAttrId));
            double baseAtk = GetLevelValue(avatar.BaseAtkByLevel, level);
            double atk = (baseAtk + weaponAtk) * (1.0 + 0.005 * mainAttr + 0.002 * subAttr);

            return new EFOperatorStatResult
            {
                Attrs = attrs,
                Hp = hp,
                Atk = atk,
                WeaponAtk = weaponAtk
            };
        }

        private static void ApplyBaseAttributes(Dictionary<int, double> attrs, EFAvatarAssetInfo avatar, int level)
        {
            if (avatar.BaseAttributes == null) return;
            foreach (var kvp in avatar.BaseAttributes)
            {
                if (!int.TryParse(kvp.Key, out int attrId) || kvp.Value == null) continue;
                attrs[attrId] = kvp.Value.BaseValue + kvp.Value.AddValue * (level - 1);
            }
        }

        private static void ApplyAttrNodes(Dictionary<int, double> attrs, EFAvatarAssetInfo avatar, IReadOnlyList<string> attrNodes)
        {
            if (attrNodes == null || avatar.AttributeNodes == null) return;
            foreach (var node in attrNodes.Where(n => !string.IsNullOrEmpty(n)))
            {
                if (!avatar.AttributeNodes.TryGetValue(node, out var nodeAttrs) || nodeAttrs == null) continue;
                foreach (var attr in nodeAttrs)
                {
                    if (!int.TryParse(attr.Key, out int attrId)) continue;
                    attrs[attrId] = GetOrDefault(attrs, attrId) + attr.Value;
                }
            }
        }

        private static void ApplyPotentialAttributes(Dictionary<int, double> attrs, EFAvatarAssetInfo avatar, int potentialLevel)
        {
            if (avatar.PotAttributes == null) return;
            foreach (var pot in avatar.PotAttributes.Where(p => p != null && potentialLevel >= p.Level && p.Attrs != null))
            {
                foreach (var attr in pot.Attrs)
                {
                    if (!int.TryParse(attr.Key, out int attrId) || attr.Value == null) continue;
                    attrs[attrId] = GetOrDefault(attrs, attrId) + attr.Value.Value;
                }
            }
        }

        private static Dictionary<string, int> ApplyEquipAttributes(Dictionary<int, double> attrs, IReadOnlyList<EFEquip> equips)
        {
            var suitCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            if (equips == null) return suitCounts;

            foreach (var equip in equips.Where(e => e != null))
            {
                if (!string.IsNullOrEmpty(equip.SuitId))
                {
                    suitCounts.TryGetValue(equip.SuitId, out int count);
                    suitCounts[equip.SuitId] = count + 1;
                }

                if (equip.Attributes == null) continue;
                foreach (var stat in equip.Attributes.Where(s => s != null))
                {
                    attrs[stat.AttrId] = GetOrDefault(attrs, stat.AttrId) + stat.Value;
                }
            }

            return suitCounts;
        }

        private static void ApplySuitBonuses(Dictionary<int, double> attrs, Dictionary<string, int> suitCounts, IEFAssets assets)
        {
            foreach (var suitPair in suitCounts.Where(pair => pair.Value >= 4))
            {
                var suit = assets.GetSuitInfo(suitPair.Key);
                if (suit == null || suit.SkillId == 0) continue;
                var skill = assets.GetSkillProp(suit.SkillId);
                if (skill?.PropMap == null) continue;
                foreach (var prop in skill.PropMap)
                {
                    if (!int.TryParse(prop.Key, out int attrId) || prop.Value?.Values == null || prop.Value.Values.Count == 0) continue;
                    attrs[attrId] = GetOrDefault(attrs, attrId) + prop.Value.Values[0];
                }
            }
        }

        private static void ApplyWeaponSkills(Dictionary<int, double> attrs, EFWeapon weapon, IEFAssets assets)
        {
            var weaponInfo = assets.GetWeaponInfo(weapon.Id);
            if (weaponInfo?.SkillList == null || weaponInfo.SkillList.Count == 0) return;

            var bounds = assets.GetBreakBounds(weaponInfo.BreakthroughTemplateId, weapon.BreakthroughLevel);
            var termsByTag = BuildTermsByTag(weapon);

            for (int i = 0; i < weaponInfo.SkillList.Count; i++)
            {
                var skill = assets.GetSkillProp(weaponInfo.SkillList[i]);
                if (skill?.PropMap == null) continue;

                int idx = ResolveWeaponSkillIndex(bounds, i, skill.TagId, termsByTag);
                foreach (var prop in skill.PropMap)
                {
                    if (!int.TryParse(prop.Key, out int attrId) || prop.Value?.Values == null || prop.Value.Values.Count == 0) continue;
                    attrs[attrId] = GetOrDefault(attrs, attrId) + GetValueAt(prop.Value.Values, idx);
                }
            }
        }

        private static Dictionary<string, int> BuildTermsByTag(EFWeapon weapon)
        {
            var termsByTag = new Dictionary<string, int>(StringComparer.Ordinal);
            if (weapon.Gem?.Terms == null) return termsByTag;

            foreach (var term in weapon.Gem.Terms.Where(t => t != null && !string.IsNullOrEmpty(t.TagId)))
            {
                termsByTag[term.TagId] = term.Cost;
            }
            return termsByTag;
        }

        private static int ResolveWeaponSkillIndex(
            IReadOnlyList<EFSkillLevelBound> bounds,
            int skillIndex,
            string tagId,
            Dictionary<string, int> termsByTag)
        {
            int lowerBound = 0;
            if (bounds != null && skillIndex < bounds.Count && bounds[skillIndex] != null)
            {
                lowerBound = bounds[skillIndex].LowerBound;
            }

            if (!string.IsNullOrEmpty(tagId) && termsByTag.TryGetValue(tagId, out int cost))
            {
                return lowerBound + cost - 1;
            }

            return lowerBound - 1;
        }

        private static double GetLevelValue(IReadOnlyList<double> values, int level)
        {
            if (values == null || values.Count == 0) return 0;
            int index = level - 1;
            if (index < 0) index = 0;
            if (index >= values.Count) index = values.Count - 1;
            return values[index];
        }

        private static double GetValueAt(IReadOnlyList<double> values, int index)
        {
            if (values == null || values.Count == 0) return 0;
            if (index < 0) index = 0;
            if (index >= values.Count) index = values.Count - 1;
            return values[index];
        }

        private static double GetOrDefault(Dictionary<int, double> attrs, int key)
        {
            return attrs.TryGetValue(key, out double value) ? value : 0;
        }

        public static EFAttrType MapAttrType(int attrId)
        {
            switch (attrId)
            {
                case (int)EFAttrType.HP:
                    return EFAttrType.HP;
                case (int)EFAttrType.Attack:
                    return EFAttrType.Attack;
                case (int)EFAttrType.Defense:
                    return EFAttrType.Defense;
                case (int)EFAttrType.CritRate:
                    return EFAttrType.CritRate;
                case (int)EFAttrType.ArtsIntensity:
                    return EFAttrType.ArtsIntensity;
                case (int)EFAttrType.TreatmentBonus:
                    return EFAttrType.TreatmentBonus;
                case (int)EFAttrType.Strength:
                    return EFAttrType.Strength;
                case (int)EFAttrType.Agility:
                    return EFAttrType.Agility;
                case (int)EFAttrType.Intellect:
                    return EFAttrType.Intellect;
                case (int)EFAttrType.Will:
                    return EFAttrType.Will;
                case (int)EFAttrType.PhysicalDMG:
                    return EFAttrType.PhysicalDMG;
                case (int)EFAttrType.HeatDMG:
                    return EFAttrType.HeatDMG;
                case (int)EFAttrType.ElectricDMG:
                    return EFAttrType.ElectricDMG;
                case (int)EFAttrType.CryoDMG:
                    return EFAttrType.CryoDMG;
                case (int)EFAttrType.NatureDMG:
                    return EFAttrType.NatureDMG;
                default:
                    return EFAttrType.Unknown;
            }
        }
    }
}
