using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnkaDotNet.Assets.EF;
using EnkaDotNet.Components.EF;
using EnkaDotNet.Enums.EF;
using EnkaDotNet.Models.EF;
using EnkaDotNet.Utils;

namespace EnkaDotNet.Utils.EF
{
    public class EFDataMapper
    {
        private readonly IEFAssets _assets;
        private readonly EnkaClientOptions _options;

        public EFDataMapper(IEFAssets assets, EnkaClientOptions options)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public EFPlayerInfo MapPlayerInfo(EFApiResponse response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (response.PlayerInfo == null) throw new ArgumentException("PlayerInfo is null", nameof(response));

            var card = response.PlayerInfo.BusinessCard;
            long uid = 0;
            if (!string.IsNullOrEmpty(response.Uid))
            {
                long.TryParse(response.Uid, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid);
            }
            if (uid == 0 && !string.IsNullOrEmpty(card?.PlatformRoleId))
            {
                long.TryParse(card.PlatformRoleId, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid);
            }

            var showcaseOperators = new List<EFOperator>();
            if (response.PlayerInfo.CharData != null)
            {
                foreach (var charData in response.PlayerInfo.CharData)
                {
                    if (charData == null) continue;
                    var op = MapOperator(charData);
                    if (op != null) showcaseOperators.Add(op);
                }
            }

            var charList = new List<EFCharListEntry>();
            if (card?.CharList != null)
            {
                foreach (var entry in card.CharList)
                {
                    if (entry == null) continue;
                    int? resolved = _assets.ResolveStrIdToTemplateId(entry.TemplateId);
                    string name = resolved.HasValue ? _assets.GetOperatorName(resolved.Value) : entry.TemplateId ?? string.Empty;
                    string icon = resolved.HasValue
                        ? _assets.GetOperatorIconUrl(resolved.Value)
                        : _assets.GetOperatorIconUrl(entry.TemplateId);

                    charList.Add(new EFCharListEntry
                    {
                        TemplateId = entry.TemplateId ?? string.Empty,
                        ResolvedTemplateId = resolved,
                        Name = name,
                        Level = entry.Level,
                        PotentialLevel = entry.PotentialLevel,
                        IconUrl = icon
                    });
                }
            }

            var medals = MapMedals(card?.Achievement);

            return new EFPlayerInfo
            {
                Nickname = card?.Name ?? string.Empty,
                AdminLevel = card?.AdventureLevel ?? 0,
                EndfieldLevel = card?.WorldLevel ?? 0,
                Signature = card?.Signature ?? string.Empty,
                Uid = uid,
                Region = response.Region ?? string.Empty,
                Ttl = response.Ttl,
                ProfilePictureId = card?.UserAvatarId ?? 0,
                ProfilePictureIcon = _assets.GetProfilePictureIconUrl(card?.UserAvatarId ?? 0),
                NameCardId = card?.BusinessCardTopicId ?? 0,
                NameCardIcon = _assets.GetNameCardIconUrl(card?.BusinessCardTopicId ?? 0),
                CharCount = card?.Statistic?.CharNum ?? 0,
                WeaponCount = card?.Statistic?.WeaponNum ?? 0,
                DocCount = card?.Statistic?.DocNum ?? 0,
                Medals = medals,
                ShowcaseOperators = showcaseOperators,
                CharList = charList
            };
        }

        private List<EFMedal> MapMedals(EFAchievementModel achievement)
        {
            var result = new List<EFMedal>();
            if (achievement?.InfoList == null || achievement.InfoList.Count == 0)
            {
                return result;
            }

            var infoById = new Dictionary<int, EFAchievementInfoModel>();
            foreach (var info in achievement.InfoList)
            {
                if (info == null) continue;
                infoById[info.AchieveNumId] = info;
            }

            if (achievement.Display != null && achievement.Display.Count > 0)
            {
                foreach (var slot in achievement.Display.OrderBy(d => d.Key))
                {
                    if (slot == null) continue;
                    if (!infoById.TryGetValue(slot.Value, out var info)) continue;
                    result.Add(CreateMedal(info, slot.Key));
                    infoById.Remove(slot.Value);
                }
            }

            foreach (var info in infoById.Values.OrderBy(i => i.AchieveNumId))
            {
                result.Add(CreateMedal(info, 0));
            }

            return result;
        }

        private EFMedal CreateMedal(EFAchievementInfoModel info, int displaySlot)
        {
            return new EFMedal
            {
                Id = info.AchieveNumId,
                Name = _assets.GetMedalName(info.AchieveNumId),
                Level = info.Level,
                IsPlated = info.IsPlated,
                DisplaySlot = displaySlot,
                IconUrl = _assets.GetMedalIconUrl(info.AchieveNumId, info.Level, info.IsPlated)
            };
        }

        public EFOperator MapOperator(EFCharDataModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var avatar = _assets.GetAvatarInfo(model.TemplateId);
            var equips = MapEquips(model.Equip);
            var weapon = model.Weapon != null ? MapWeapon(model.Weapon) : null;
            var skills = MapSkills(model.SkillInfo, model.TemplateId);
            var talents = MapTalents(model.Talent?.LatestPassiveSkillNodes, model.TemplateId, avatar?.StrId);

            var tempOp = new EFOperator
            {
                Id = model.TemplateId,
                Level = model.Level,
                PotentialLevel = model.PotentialLevel,
                AttrNodes = model.Talent?.AttrNodes ?? new List<string>(),
                Equips = equips,
                Weapon = weapon
            };

            var calculated = EFStatsCalculator.CalculateOperatorStats(tempOp, _assets);
            var stats = new ConcurrentDictionary<EFAttrType, double>();
            var rawAttrs = new ConcurrentDictionary<int, double>();
            foreach (var pair in calculated.Attrs)
            {
                rawAttrs[pair.Key] = pair.Value;
                var mapped = EFStatsCalculator.MapAttrType(pair.Key);
                if (mapped != EFAttrType.Unknown)
                {
                    stats[mapped] = pair.Value;
                }
            }

            return new EFOperator
            {
                Id = model.TemplateId,
                StrId = avatar?.StrId ?? string.Empty,
                Name = _assets.GetOperatorName(model.TemplateId),
                Level = model.Level,
                PotentialLevel = model.PotentialLevel,
                Rarity = avatar?.Rarity ?? 0,
                Element = avatar?.Element ?? string.Empty,
                Profession = avatar?.Profession ?? string.Empty,
                WeaponType = avatar?.WeaponType ?? string.Empty,
                IconUrl = _assets.GetOperatorIconUrl(model.TemplateId),
                RoundIconUrl = _assets.GetOperatorRoundIconUrl(model.TemplateId),
                SplashArtUrl = _assets.GetOperatorSplashArtUrl(model.TemplateId),
                SilhouetteUrl = _assets.GetOperatorSilhouetteUrl(model.TemplateId),
                ProfessionIconUrl = _assets.GetProfessionIconUrl(avatar?.Profession),
                Stats = stats,
                RawAttrs = rawAttrs,
                Hp = calculated.Hp,
                Atk = calculated.Atk,
                Skills = skills,
                Talents = talents,
                Weapon = weapon,
                Equips = equips,
                AttrNodes = model.Talent?.AttrNodes ?? new List<string>(),
                PassiveSkillNodes = model.Talent?.LatestPassiveSkillNodes ?? new List<string>(),
                FactorySkillNodes = model.Talent?.LatestFactorySkillNodes ?? new List<string>(),
                LatestBreakNode = model.Talent?.LatestBreakNode ?? string.Empty,
                EquipMedicineId = model.EquipMedicineId,
                Options = _options,
                Assets = _assets
            };
        }

        private static string ResolveGemOverlayIconUrl(IReadOnlyList<EFGemTerm> terms)
        {
            if (terms == null || terms.Count == 0) return string.Empty;

            for (int i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                if (term.TermType == 3 && !string.IsNullOrEmpty(term.TagIconUrl))
                {
                    return term.TagIconUrl;
                }
            }

            for (int i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                if (string.IsNullOrEmpty(term.TagIconUrl)) continue;
                if (term.TagIconUrl.IndexOf("icon_wpngem_00", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                return term.TagIconUrl;
            }

            return string.Empty;
        }

        private IReadOnlyList<EFTalent> MapTalents(List<string> passiveNodes, int operatorId, string strId)
        {
            var result = new List<EFTalent>();
            if (passiveNodes == null || passiveNodes.Count == 0) return result;

            foreach (var nodeId in passiveNodes)
            {
                if (string.IsNullOrEmpty(nodeId)) continue;
                var node = _assets.GetNodeSkillInfo(operatorId, nodeId);
                int index = node?.Index ?? ParsePassiveSkillIndex(nodeId);
                int rank = node?.Level ?? ParsePassiveSkillRank(nodeId);
                if (rank <= 0) rank = 1;

                string skillId = string.Empty;
                if (!string.IsNullOrEmpty(strId) && index >= 0)
                {
                    skillId = $"{strId}_talent_{index}";
                }

                result.Add(new EFTalent
                {
                    Id = nodeId,
                    Index = index,
                    Rank = rank,
                    Name = index >= 0 ? $"Talent {index + 1}" : "Talent",
                    IconUrl = _assets.GetNodeSkillIconUrl(operatorId, nodeId),
                    SkillId = skillId
                });
            }

            result.Sort((a, b) => a.Index.CompareTo(b.Index));
            return result;
        }

        private static int ParsePassiveSkillIndex(string nodeId)
        {
            // chr_0017_yvonne_passive_skill_0_2
            const string marker = "_passive_skill_";
            int idx = nodeId.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return -1;
            string rest = nodeId.Substring(idx + marker.Length);
            int underscore = rest.IndexOf('_');
            string indexPart = underscore >= 0 ? rest.Substring(0, underscore) : rest;
            return int.TryParse(indexPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : -1;
        }

        private static int ParsePassiveSkillRank(string nodeId)
        {
            int last = nodeId.LastIndexOf('_');
            if (last < 0 || last >= nodeId.Length - 1) return 0;
            return int.TryParse(nodeId.Substring(last + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : 0;
        }

        private IReadOnlyList<EFEquip> MapEquips(List<EFEquipEntryModel> equipEntries)
        {
            var result = new List<EFEquip>();
            if (equipEntries == null) return result;

            foreach (var entry in equipEntries)
            {
                if (entry?.Value == null) continue;
                var item = _assets.GetEquipItem(entry.Value.TemplateId);
                var enhanceMap = new Dictionary<int, int>();
                if (entry.Value.Enhance != null)
                {
                    foreach (var enh in entry.Value.Enhance)
                    {
                        enhanceMap[enh.Key] = enh.Value;
                    }
                }

                var attributes = new List<EFStat>();
                if (item?.AttrModifiers != null)
                {
                    for (int i = 0; i < item.AttrModifiers.Count; i++)
                    {
                        var mod = item.AttrModifiers[i];
                        int enhanceLevel = enhanceMap.TryGetValue(i, out int el) ? el : 0;
                        double value = 0;
                        if (mod.Values != null && mod.Values.Count > 0)
                        {
                            int idx = enhanceLevel;
                            if (idx < 0) idx = 0;
                            if (idx >= mod.Values.Count) idx = mod.Values.Count - 1;
                            value = mod.Values[idx];
                        }

                        bool raw = _options.UseRawStatValues;
                        attributes.Add(new EFStat
                        {
                            AttrId = mod.AttrType,
                            Type = EFStatsCalculator.MapAttrType(mod.AttrType),
                            Name = EFStatsHelpers.GetAttrName(mod.AttrType, _assets),
                            Value = value,
                            FormattedValue = EFStatsHelpers.FormatAttrValue(mod.AttrType, value, raw),
                            EnhanceLevel = enhanceLevel
                        });
                    }
                }

                string suitId = item?.SuitId ?? string.Empty;
                var slot = (EFEquipSlot)entry.Key;
                result.Add(new EFEquip
                {
                    Slot = slot,
                    SlotName = EFStatsHelpers.GetEquipSlotName(slot),
                    Id = entry.Value.TemplateId,
                    SuitId = suitId,
                    SuitName = _assets.GetSuitName(suitId),
                    SuitIconUrl = _assets.GetSuitIconUrl(suitId),
                    Rarity = item?.Rarity ?? 0,
                    IconUrl = _assets.GetEquipIconUrl(entry.Value.TemplateId),
                    Attributes = attributes
                });
            }

            return result;
        }

        private EFWeapon MapWeapon(EFWeaponModel model)
        {
            var info = _assets.GetWeaponInfo(model.TemplateId);
            double weaponAtk = info != null ? _assets.GetWeaponAtk(info.LevelTemplateId, model.WeaponLv) : 0;

            EFGem gem = null;
            var subStats = new List<EFStat>();
            if (model.AttachedGem != null)
            {
                var terms = new List<EFGemTerm>();
                if (model.AttachedGem.Terms != null)
                {
                    foreach (var term in model.AttachedGem.Terms)
                    {
                        var termInfo = _assets.GetGemTermTag(term.TermNumId);
                        terms.Add(new EFGemTerm
                        {
                            TermNumId = term.TermNumId,
                            Cost = term.Cost,
                            TermType = termInfo?.TermType ?? 0,
                            TagId = termInfo?.TagId ?? string.Empty,
                            TagIconUrl = _assets.GetGemTermIconUrl(termInfo?.TagIcon),
                            TagName = !string.IsNullOrEmpty(termInfo?.TagNameHash)
                                ? _assets.GetLocalizedText(termInfo.TagNameHash)
                                : string.Empty
                        });
                    }
                }

                gem = new EFGem
                {
                    Id = model.AttachedGem.TemplateId,
                    IconUrl = _assets.GetGemIconUrl(model.AttachedGem.TemplateId),
                    OverlayIconUrl = ResolveGemOverlayIconUrl(terms),
                    DomainId = model.AttachedGem.DomainId,
                    TotalCost = model.AttachedGem.TotalCost,
                    Terms = terms
                };

                if (info?.SkillList != null)
                {
                    var bounds = _assets.GetBreakBounds(info.BreakthroughTemplateId, model.BreakthroughLv);
                    var termsByTag = new Dictionary<string, int>(StringComparer.Ordinal);
                    foreach (var term in terms)
                    {
                        if (!string.IsNullOrEmpty(term.TagId)) termsByTag[term.TagId] = term.Cost;
                    }

                    for (int i = 0; i < info.SkillList.Count; i++)
                    {
                        var skill = _assets.GetSkillProp(info.SkillList[i]);
                        if (skill?.PropMap == null) continue;
                        int lowerBound = (bounds != null && i < bounds.Count && bounds[i] != null) ? bounds[i].LowerBound : 0;
                        int idx = (!string.IsNullOrEmpty(skill.TagId) && termsByTag.TryGetValue(skill.TagId, out int cost))
                            ? lowerBound + cost - 1
                            : lowerBound - 1;

                        foreach (var prop in skill.PropMap)
                        {
                            if (!int.TryParse(prop.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out int attrId)
                                || prop.Value?.Values == null || prop.Value.Values.Count == 0)
                            {
                                continue;
                            }

                            int clampIdx = idx;
                            if (clampIdx < 0) clampIdx = 0;
                            if (clampIdx >= prop.Value.Values.Count) clampIdx = prop.Value.Values.Count - 1;
                            double subValue = prop.Value.Values[clampIdx];
                            bool raw = _options.UseRawStatValues;
                            subStats.Add(new EFStat
                            {
                                AttrId = attrId,
                                Type = EFStatsCalculator.MapAttrType(attrId),
                                Name = EFStatsHelpers.GetAttrName(attrId, _assets),
                                Value = subValue,
                                FormattedValue = EFStatsHelpers.FormatAttrValue(attrId, subValue, raw)
                            });
                        }
                    }
                }
            }

            int curveLength = 0;
            if (info != null)
            {
                var meta = _assets.GetWeaponMeta();
                if (meta?.LevelCurves != null && !string.IsNullOrEmpty(info.LevelTemplateId)
                    && meta.LevelCurves.TryGetValue(info.LevelTemplateId, out var curve) && curve != null)
                {
                    curveLength = curve.Count;
                }
            }

            int maxLevel = EFStatsHelpers.GetWeaponMaxLevel(model.BreakthroughLv, curveLength);

            return new EFWeapon
            {
                Id = model.TemplateId,
                Name = _assets.GetWeaponName(model.TemplateId),
                Level = model.WeaponLv,
                MaxLevel = maxLevel,
                Rarity = info?.Rarity ?? 0,
                BreakthroughLevel = model.BreakthroughLv,
                RefineLevel = model.RefineLv,
                BaseAtk = weaponAtk,
                WeaponType = info?.WeaponType ?? string.Empty,
                IconUrl = _assets.GetWeaponIconUrl(model.TemplateId),
                Gem = gem,
                SubStats = subStats
            };
        }

        private IReadOnlyList<EFSkill> MapSkills(EFSkillInfoModel skillInfo, int operatorId)
        {
            var result = new List<EFSkill>();
            if (skillInfo?.LevelInfo == null) return result;

            var avatar = _assets.GetAvatarInfo(operatorId);
            foreach (var level in skillInfo.LevelInfo)
            {
                if (level == null) continue;
                string element = string.Empty;
                if (avatar?.SkillInfoMap != null && !string.IsNullOrEmpty(level.SkillId)
                    && avatar.SkillInfoMap.TryGetValue(level.SkillId, out var skillAsset))
                {
                    element = skillAsset.Element ?? string.Empty;
                }

                string skillId = level.SkillId ?? string.Empty;
                bool isCombat = skillId.EndsWith("_NormalAttack", StringComparison.Ordinal)
                    || skillId.EndsWith("_NormalSkill", StringComparison.Ordinal)
                    || skillId.EndsWith("_UltimateSkill", StringComparison.Ordinal)
                    || skillId.EndsWith("_ComboSkill", StringComparison.Ordinal);

                result.Add(new EFSkill
                {
                    Id = skillId,
                    Name = EFStatsHelpers.GetSkillDisplayName(skillId),
                    Level = level.SkillLevel,
                    MaxLevel = level.SkillMaxLevel,
                    EnhancedLevel = level.SkillEnhancedLevel,
                    IconUrl = _assets.GetSkillIconUrl(operatorId, level.SkillId),
                    Element = element,
                    IsCombatSkill = isCombat
                });
            }

            return result;
        }
    }
}
