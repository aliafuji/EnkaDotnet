using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EnkaDotNet.Assets.EF.Models;
using EnkaDotNet.Utils;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Assets.EF
{
    public class EFAssets : BaseAssets, IEFAssets
    {
        private readonly ConcurrentDictionary<string, EFAvatarAssetInfo> _avatars = new ConcurrentDictionary<string, EFAvatarAssetInfo>();
        private readonly ConcurrentDictionary<string, EFWeaponAssetInfo> _weapons = new ConcurrentDictionary<string, EFWeaponAssetInfo>();
        private readonly ConcurrentDictionary<string, EFEquipItemInfo> _equipItems = new ConcurrentDictionary<string, EFEquipItemInfo>();
        private readonly ConcurrentDictionary<string, EFEquipSuitInfo> _equipSuits = new ConcurrentDictionary<string, EFEquipSuitInfo>();
        private readonly ConcurrentDictionary<string, EFGemTermInfo> _gemTerms = new ConcurrentDictionary<string, EFGemTermInfo>();
        private readonly ConcurrentDictionary<string, EFGemTemplateInfo> _gemTemplates = new ConcurrentDictionary<string, EFGemTemplateInfo>();
        private readonly ConcurrentDictionary<string, EFSkillAssetInfo> _skills = new ConcurrentDictionary<string, EFSkillAssetInfo>();
        private readonly ConcurrentDictionary<string, EFPfpAssetInfo> _pfps = new ConcurrentDictionary<string, EFPfpAssetInfo>();
        private readonly ConcurrentDictionary<string, EFNameCardAssetInfo> _namecards = new ConcurrentDictionary<string, EFNameCardAssetInfo>();
        private readonly ConcurrentDictionary<string, EFMedalAssetInfo> _medals = new ConcurrentDictionary<string, EFMedalAssetInfo>();
        private readonly ConcurrentDictionary<string, int> _strIdToTemplateId = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        private EFWeaponMetaData _weaponMeta;

        public EFAssets(string language, HttpClient httpClient, ILogger<EFAssets> logger, string fallbackDirectory = null)
            : base(language, "ef", httpClient, logger, fallbackDirectory)
        {
        }

        protected override IReadOnlyDictionary<string, string> GetAssetFileUrls()
        {
            return Constants.EFAssetFileUrls;
        }

        protected override async Task LoadAssetsInternalAsync()
        {
            var tasks = new List<Task>
            {
                RunLoaderAsync(LoadAvatars),
                RunLoaderAsync(LoadWeapons),
                RunLoaderAsync(LoadEquips),
                RunLoaderAsync(LoadGems),
                RunLoaderAsync(LoadSkills),
                RunLoaderAsync(LoadWeaponMeta),
                RunLoaderAsync(LoadPfps),
                RunLoaderAsync(LoadNamecards),
                RunLoaderAsync(LoadMedals)
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task LoadAvatars()
        {
            _avatars.Clear();
            _strIdToTemplateId.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, EFAvatarAssetInfo>>("avatars.json").ConfigureAwait(false);
                if (deserializedMap == null)
                {
                    throw new InvalidOperationException("EF avatars.json data is null after deserialization.");
                }

                foreach (var kvp in deserializedMap)
                {
                    _avatars[kvp.Key] = kvp.Value;
                    if (!string.IsNullOrEmpty(kvp.Value?.StrId) && int.TryParse(kvp.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out int templateId))
                    {
                        _strIdToTemplateId[kvp.Value.StrId] = templateId;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Endfield avatars.json asset.");
                throw new InvalidOperationException("Failed to load essential Endfield avatar data.", ex);
            }
        }

        private async Task LoadWeapons()
        {
            _weapons.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, EFWeaponAssetInfo>>("weapons.json").ConfigureAwait(false);
                if (deserializedMap == null)
                {
                    throw new InvalidOperationException("EF weapons.json data is null after deserialization.");
                }

                foreach (var kvp in deserializedMap) _weapons[kvp.Key] = kvp.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Endfield weapons.json asset.");
                throw new InvalidOperationException("Failed to load essential Endfield weapon data.", ex);
            }
        }

        private async Task LoadEquips()
        {
            _equipItems.Clear();
            _equipSuits.Clear();
            try
            {
                var deserializedData = await FetchAndDeserializeAssetAsync<EFEquipData>("equips.json").ConfigureAwait(false);
                if (deserializedData?.Items == null)
                {
                    throw new InvalidOperationException("EF equips.json Items data is null after deserialization.");
                }

                foreach (var kvp in deserializedData.Items) _equipItems[kvp.Key] = kvp.Value;

                if (deserializedData.Suits != null)
                {
                    foreach (var kvp in deserializedData.Suits) _equipSuits[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Endfield equips.json asset.");
                throw new InvalidOperationException("Failed to load essential Endfield equip data.", ex);
            }
        }

        private async Task LoadGems()
        {
            _gemTerms.Clear();
            _gemTemplates.Clear();
            try
            {
                var deserializedData = await FetchAndDeserializeAssetAsync<EFGemData>("gems.json").ConfigureAwait(false);
                if (deserializedData == null)
                {
                    throw new InvalidOperationException("EF gems.json data is null after deserialization.");
                }

                if (deserializedData.TermNums != null)
                {
                    foreach (var kvp in deserializedData.TermNums) _gemTerms[kvp.Key] = kvp.Value;
                }

                if (deserializedData.TemplateItems != null)
                {
                    foreach (var kvp in deserializedData.TemplateItems) _gemTemplates[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Endfield gems.json asset.");
                throw new InvalidOperationException("Failed to load essential Endfield gem data.", ex);
            }
        }

        private async Task LoadSkills()
        {
            _skills.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, EFSkillAssetInfo>>("skills.json").ConfigureAwait(false);
                if (deserializedMap == null)
                {
                    throw new InvalidOperationException("EF skills.json data is null after deserialization.");
                }

                foreach (var kvp in deserializedMap) _skills[kvp.Key] = kvp.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Endfield skills.json asset.");
                throw new InvalidOperationException("Failed to load essential Endfield skill data.", ex);
            }
        }

        private async Task LoadWeaponMeta()
        {
            try
            {
                _weaponMeta = await FetchAndDeserializeAssetAsync<EFWeaponMetaData>("weapon_meta.json").ConfigureAwait(false);
                if (_weaponMeta == null)
                {
                    throw new InvalidOperationException("EF weapon_meta.json data is null after deserialization.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Endfield weapon_meta.json asset.");
                throw new InvalidOperationException("Failed to load essential Endfield weapon meta data.", ex);
            }
        }

        private async Task LoadPfps()
        {
            _pfps.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, EFPfpAssetInfo>>("pfps.json").ConfigureAwait(false);
                if (deserializedMap == null)
                {
                    throw new InvalidOperationException("EF pfps.json data is null after deserialization.");
                }

                foreach (var kvp in deserializedMap) _pfps[kvp.Key] = kvp.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Endfield pfps.json asset.");
                throw new InvalidOperationException("Failed to load essential Endfield profile picture data.", ex);
            }
        }

        private async Task LoadNamecards()
        {
            _namecards.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, EFNameCardAssetInfo>>("namecards.json").ConfigureAwait(false);
                if (deserializedMap == null)
                {
                    throw new InvalidOperationException("EF namecards.json data is null after deserialization.");
                }

                foreach (var kvp in deserializedMap) _namecards[kvp.Key] = kvp.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Endfield namecards.json asset.");
                throw new InvalidOperationException("Failed to load essential Endfield namecard data.", ex);
            }
        }

        private async Task LoadMedals()
        {
            _medals.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, EFMedalAssetInfo>>("medals.json").ConfigureAwait(false);
                if (deserializedMap == null)
                {
                    throw new InvalidOperationException("EF medals.json data is null after deserialization.");
                }

                foreach (var kvp in deserializedMap) _medals[kvp.Key] = kvp.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Endfield medals.json asset.");
                throw new InvalidOperationException("Failed to load essential Endfield medal data.", ex);
            }
        }

        private static string BuildCdnUrl(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return path;
            return Constants.DefaultEFAssetCdnUrl + path;
        }

        public string GetLocalizedText(string key) => GetText(key);

        public EFAvatarAssetInfo GetAvatarInfo(int operatorId)
        {
            return GetAvatarInfo(operatorId.ToString(CultureInfo.InvariantCulture));
        }

        public EFAvatarAssetInfo GetAvatarInfo(string operatorId)
        {
            if (operatorId != null && _avatars.TryGetValue(operatorId, out var info)) return info;
            return null;
        }

        public string GetOperatorName(int operatorId)
        {
            var avatar = GetAvatarInfo(operatorId);
            if (avatar != null && !string.IsNullOrEmpty(avatar.NameHash))
            {
                string localized = GetText(avatar.NameHash);
                if (!string.IsNullOrEmpty(localized) && localized != avatar.NameHash) return localized;
            }

            if (avatar != null && !string.IsNullOrEmpty(avatar.StrId))
            {
                string[] parts = avatar.StrId.Split('_');
                if (parts.Length > 0)
                {
                    string last = parts[parts.Length - 1];
                    if (!string.IsNullOrEmpty(last))
                    {
                        return char.ToUpperInvariant(last[0]) + last.Substring(1);
                    }
                }
            }

            return $"Operator_{operatorId}";
        }

        public string GetOperatorIconUrl(int operatorId)
        {
            var avatar = GetAvatarInfo(operatorId);
            if (avatar != null && !string.IsNullOrEmpty(avatar.StrId))
            {
                return GetOperatorIconUrl(avatar.StrId);
            }
            return string.Empty;
        }

        public string GetOperatorIconUrl(string strId)
        {
            if (string.IsNullOrEmpty(strId)) return string.Empty;
            return BuildCdnUrl($"/ui/ef/charremoteicon/icon_{strId}.png");
        }

        public string GetOperatorRoundIconUrl(int operatorId)
        {
            var avatar = GetAvatarInfo(operatorId);
            if (avatar == null || string.IsNullOrEmpty(avatar.StrId)) return string.Empty;
            return BuildCdnUrl($"/ui/ef/charroundicon/icon_round_{avatar.StrId}.png");
        }

        public string GetOperatorSplashArtUrl(int operatorId)
        {
            var avatar = GetAvatarInfo(operatorId);
            if (avatar == null || string.IsNullOrEmpty(avatar.StrId)) return string.Empty;
            return BuildCdnUrl($"/ui/ef/splash/{avatar.StrId}.webp");
        }

        public string GetOperatorSilhouetteUrl(int operatorId)
        {
            var avatar = GetAvatarInfo(operatorId);
            if (avatar == null || string.IsNullOrEmpty(avatar.StrId)) return string.Empty;
            return BuildCdnUrl($"/ui/ef/charinfo/bg_charinfo_{avatar.StrId}.png");
        }

        public string GetProfessionIconUrl(string profession)
        {
            if (string.IsNullOrEmpty(profession)) return string.Empty;
            return BuildCdnUrl($"/ui/ef/profession/{profession}.png");
        }

        public string GetWeaponName(int weaponId)
        {
            if (_weapons.TryGetValue(weaponId.ToString(CultureInfo.InvariantCulture), out var weapon) && !string.IsNullOrEmpty(weapon.NameHash))
            {
                return GetText(weapon.NameHash);
            }
            return $"Weapon_{weaponId}";
        }

        public string GetWeaponIconUrl(int weaponId)
        {
            if (_weapons.TryGetValue(weaponId.ToString(CultureInfo.InvariantCulture), out var weapon) && !string.IsNullOrEmpty(weapon.Icon))
            {
                return BuildCdnUrl(NormalizeItemIconPath(weapon.Icon));
            }
            return string.Empty;
        }

        private static string NormalizeItemIconPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (path.IndexOf("/itemicon/", StringComparison.OrdinalIgnoreCase) >= 0) return path;

            int slash = path.LastIndexOf('/');
            string fileName = slash >= 0 ? path.Substring(slash + 1) : path;
            if (string.IsNullOrEmpty(fileName)) return path;
            return "/ui/ef/itemicon/" + fileName;
        }

        public EFWeaponAssetInfo GetWeaponInfo(int weaponId)
        {
            _weapons.TryGetValue(weaponId.ToString(CultureInfo.InvariantCulture), out var info);
            return info;
        }

        public string GetEquipIconUrl(int equipId)
        {
            if (_equipItems.TryGetValue(equipId.ToString(CultureInfo.InvariantCulture), out var item) && !string.IsNullOrEmpty(item.Icon))
            {
                return BuildCdnUrl(item.Icon);
            }
            return string.Empty;
        }

        public EFEquipItemInfo GetEquipItem(int equipId)
        {
            _equipItems.TryGetValue(equipId.ToString(CultureInfo.InvariantCulture), out var item);
            return item;
        }

        public string GetSuitName(string suitId)
        {
            if (!string.IsNullOrEmpty(suitId) && _equipSuits.TryGetValue(suitId, out var suit) && !string.IsNullOrEmpty(suit.NameHash))
            {
                return GetText(suit.NameHash);
            }
            return suitId ?? string.Empty;
        }

        public string GetSuitIconUrl(string suitId)
        {
            if (!string.IsNullOrEmpty(suitId) && _equipSuits.TryGetValue(suitId, out var suit) && !string.IsNullOrEmpty(suit.Icon))
            {
                return BuildCdnUrl(suit.Icon);
            }
            return string.Empty;
        }

        public EFEquipSuitInfo GetSuitInfo(string suitId)
        {
            if (!string.IsNullOrEmpty(suitId) && _equipSuits.TryGetValue(suitId, out var suit)) return suit;
            return null;
        }

        public EFSkillAssetInfo GetSkillProp(int skillId)
        {
            _skills.TryGetValue(skillId.ToString(CultureInfo.InvariantCulture), out var skill);
            return skill;
        }

        public double GetWeaponAtk(string levelTemplateId, int weaponLevel)
        {
            if (string.IsNullOrEmpty(levelTemplateId) || _weaponMeta?.LevelCurves == null) return 0;
            if (!_weaponMeta.LevelCurves.TryGetValue(levelTemplateId, out var curve) || curve == null || curve.Count == 0) return 0;
            int index = weaponLevel - 1;
            if (index < 0) index = 0;
            if (index >= curve.Count) index = curve.Count - 1;
            return curve[index];
        }

        public IReadOnlyList<EFSkillLevelBound> GetBreakBounds(string breakthroughTemplateId, int breakthroughLevel)
        {
            if (string.IsNullOrEmpty(breakthroughTemplateId) || _weaponMeta?.BreakSkillLevelBounds == null)
            {
                return Array.Empty<EFSkillLevelBound>();
            }

            if (!_weaponMeta.BreakSkillLevelBounds.TryGetValue(breakthroughTemplateId, out var byLevel) || byLevel == null)
            {
                return Array.Empty<EFSkillLevelBound>();
            }

            string key = breakthroughLevel.ToString(CultureInfo.InvariantCulture);
            if (byLevel.TryGetValue(key, out var bounds) && bounds != null)
            {
                return bounds;
            }

            return Array.Empty<EFSkillLevelBound>();
        }

        public EFGemTermInfo GetGemTermTag(int termNumId)
        {
            _gemTerms.TryGetValue(termNumId.ToString(CultureInfo.InvariantCulture), out var term);
            return term;
        }

        public string GetGemIconUrl(int gemTemplateId)
        {
            if (_gemTemplates.TryGetValue(gemTemplateId.ToString(CultureInfo.InvariantCulture), out var template) && !string.IsNullOrEmpty(template.Icon))
            {
                return BuildCdnUrl(NormalizeItemIconPath(template.Icon));
            }
            return string.Empty;
        }

        public string GetGemTermIconUrl(string tagIconPath)
        {
            if (string.IsNullOrEmpty(tagIconPath)) return string.Empty;
            return BuildCdnUrl(NormalizeItemIconPath(tagIconPath));
        }

        public string GetProfilePictureIconUrl(int profilePictureId)
        {
            if (_pfps.TryGetValue(profilePictureId.ToString(CultureInfo.InvariantCulture), out var pfp) && !string.IsNullOrEmpty(pfp.Icon))
            {
                return BuildCdnUrl(pfp.Icon);
            }
            return string.Empty;
        }

        public string GetNameCardIconUrl(int nameCardId)
        {
            if (_namecards.TryGetValue(nameCardId.ToString(CultureInfo.InvariantCulture), out var card) && !string.IsNullOrEmpty(card.Icon))
            {
                return BuildCdnUrl(card.Icon);
            }
            return string.Empty;
        }

        public string GetMedalName(int medalId)
        {
            if (_medals.TryGetValue(medalId.ToString(CultureInfo.InvariantCulture), out var medal))
            {
                if (!string.IsNullOrEmpty(medal.NameHash))
                {
                    string localized = TryGetText(medal.NameHash);
                    if (!string.IsNullOrEmpty(localized))
                    {
                        return localized;
                    }
                }

                string fromIcon = HumanizeMedalIconName(medal.IconByLevel);
                if (!string.IsNullOrEmpty(fromIcon))
                {
                    return fromIcon;
                }
            }
            return $"Medal_{medalId}";
        }

        private static string HumanizeMedalIconName(Dictionary<string, string> iconByLevel)
        {
            string path = FirstNonEmptyValue(iconByLevel);
            if (string.IsNullOrEmpty(path)) return null;

            string file = StripMedalIconFileName(path);
            string[] parts = file.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = HumanizeMedalToken(parts[i]);
            }
            return string.Join(" ", parts);
        }

        private static string FirstNonEmptyValue(Dictionary<string, string> values)
        {
            if (values == null || values.Count == 0) return null;
            return values.Values.FirstOrDefault(v => !string.IsNullOrEmpty(v));
        }

        private static string StripMedalIconFileName(string path)
        {
            int slash = path.LastIndexOf('/');
            string file = slash >= 0 ? path.Substring(slash + 1) : path;
            if (file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                file = file.Substring(0, file.Length - 4);
            }
            if (file.EndsWith("_plating", StringComparison.OrdinalIgnoreCase))
            {
                file = file.Substring(0, file.Length - 8);
            }

            int lv = file.LastIndexOf("_lv", StringComparison.OrdinalIgnoreCase);
            if (lv > 0)
            {
                file = file.Substring(0, lv);
            }
            if (file.StartsWith("achv_", StringComparison.OrdinalIgnoreCase))
            {
                file = file.Substring(5);
            }
            return file;
        }

        private static string HumanizeMedalToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return token;
            if (token.Length <= 2 && IsAllUpperOrDigit(token))
            {
                return token.ToUpperInvariant();
            }
            return char.ToUpperInvariant(token[0]) + token.Substring(1).ToLowerInvariant();
        }

        private static bool IsAllUpperOrDigit(string value)
        {
            return value.All(c => char.IsDigit(c) || char.IsUpper(c));
        }

        public string GetMedalIconUrl(int medalId, int level, bool isPlated = false)
        {
            if (!_medals.TryGetValue(medalId.ToString(CultureInfo.InvariantCulture), out var medal) || medal.IconByLevel == null)
            {
                return string.Empty;
            }

            string key = level.ToString(CultureInfo.InvariantCulture);
            string icon = medal.IconByLevel.TryGetValue(key, out var byLevel) && !string.IsNullOrEmpty(byLevel)
                ? byLevel
                : FirstNonEmptyValue(medal.IconByLevel);

            if (string.IsNullOrEmpty(icon))
            {
                return string.Empty;
            }

            if (isPlated)
            {
                icon = ApplyPlatingSuffix(icon);
            }
            return BuildCdnUrl(icon);
        }

        private static string ApplyPlatingSuffix(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath)) return iconPath;
            if (iconPath.EndsWith("_plating.png", StringComparison.OrdinalIgnoreCase)) return iconPath;
            if (iconPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return iconPath.Substring(0, iconPath.Length - 4) + "_plating.png";
            }
            return iconPath;
        }

        public string GetSkillIconUrl(int operatorId, string skillId)
        {
            var avatar = GetAvatarInfo(operatorId);
            string mapped = ResolveMappedSkillIcon(avatar, skillId);
            if (!string.IsNullOrEmpty(mapped))
            {
                return BuildCdnUrl(NormalizeSkillIconPath(mapped));
            }

            return GuessTalentSkillIconUrl(avatar, skillId);
        }

        private static string ResolveMappedSkillIcon(EFAvatarAssetInfo avatar, string skillId)
        {
            if (avatar == null || string.IsNullOrEmpty(skillId)) return null;

            if (avatar.SkillInfoMap != null
                && avatar.SkillInfoMap.TryGetValue(skillId, out var skillInfo)
                && !string.IsNullOrEmpty(skillInfo.Icon))
            {
                return skillInfo.Icon;
            }

            if (avatar.NodeSkillMap != null
                && avatar.NodeSkillMap.TryGetValue(skillId, out var nodeInfo)
                && !string.IsNullOrEmpty(nodeInfo.Icon))
            {
                return nodeInfo.Icon;
            }

            return null;
        }

        private static string GuessTalentSkillIconUrl(EFAvatarAssetInfo avatar, string skillId)
        {
            if (string.IsNullOrEmpty(skillId)
                || skillId.IndexOf("_talent_", StringComparison.Ordinal) < 0
                || avatar == null
                || string.IsNullOrEmpty(avatar.StrId))
            {
                return string.Empty;
            }

            int talentIndex = ParseTalentIndex(skillId);
            if (talentIndex < 0) return string.Empty;

            string shortName = ExtractOperatorShortName(avatar.StrId);
            if (string.IsNullOrEmpty(shortName)) return string.Empty;

            return BuildCdnUrl($"/ui/ef/skillicon/icon_talent_{shortName}_{talentIndex + 1:00}.png");
        }

        public string GetNodeSkillIconUrl(int operatorId, string nodeId)
        {
            var node = GetNodeSkillInfo(operatorId, nodeId);
            if (node == null || string.IsNullOrEmpty(node.Icon)) return string.Empty;
            return BuildCdnUrl(NormalizeSkillIconPath(node.Icon));
        }

        public EFAvatarNodeSkillInfo GetNodeSkillInfo(int operatorId, string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            var avatar = GetAvatarInfo(operatorId);
            if (avatar?.NodeSkillMap != null && avatar.NodeSkillMap.TryGetValue(nodeId, out var node))
            {
                return node;
            }
            return null;
        }

        private static string NormalizeSkillIconPath(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath)) return iconPath;
            if (iconPath.IndexOf("/skillicon/", StringComparison.OrdinalIgnoreCase) >= 0
                || iconPath.IndexOf("/spaceshipskillicon/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return iconPath;
            }

            int slash = iconPath.LastIndexOf('/');
            string file = slash >= 0 ? iconPath.Substring(slash + 1) : iconPath;
            if (file.StartsWith("icon_talent_", StringComparison.OrdinalIgnoreCase)
                || file.StartsWith("icon_skill_", StringComparison.OrdinalIgnoreCase)
                || file.StartsWith("icon_combo_", StringComparison.OrdinalIgnoreCase)
                || file.StartsWith("icon_ultimate_", StringComparison.OrdinalIgnoreCase)
                || file.StartsWith("icon_attack_", StringComparison.OrdinalIgnoreCase))
            {
                return "/ui/ef/skillicon/" + file;
            }

            return iconPath;
        }

        private static int ParseTalentIndex(string skillId)
        {
            int idx = skillId.LastIndexOf("_talent_", StringComparison.Ordinal);
            if (idx < 0) return -1;
            string tail = skillId.Substring(idx + "_talent_".Length);
            return int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : -1;
        }

        private static string ExtractOperatorShortName(string strId)
        {
            if (string.IsNullOrEmpty(strId)) return null;
            int last = strId.LastIndexOf('_');
            if (last < 0 || last >= strId.Length - 1) return null;
            return strId.Substring(last + 1);
        }

        public int? ResolveStrIdToTemplateId(string strId)
        {
            if (string.IsNullOrEmpty(strId)) return null;
            if (_strIdToTemplateId.TryGetValue(strId, out int templateId)) return templateId;
            return null;
        }

        public EFWeaponMetaData GetWeaponMeta() => _weaponMeta;
    }
}
