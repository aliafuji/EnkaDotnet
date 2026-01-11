using System;
using System.Collections.Generic;
using System.Text.Json;
using EnkaDotNet.Enums;
using EnkaDotNet.Models.EnkaProfile;
using EnkaDotNet.Components.EnkaProfile;

namespace EnkaDotNet.Utils.Enka
{
    /// <summary>
    /// Maps raw Enka.Network API response models to component objects.
    /// Handles profile data, hoyo accounts, and game type identification.
    /// </summary>
    public class EnkaDataMapper
    {
        private readonly EnkaClientOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnkaDataMapper"/> class.
        /// </summary>
        /// <param name="options">The client options for configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
        public EnkaDataMapper(EnkaClientOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Maps an Enka.Network profile API response to an EnkaUserProfile component.
        /// </summary>
        /// <param name="model">The raw API response model.</param>
        /// <returns>An EnkaUserProfile component, or null if the model is null.</returns>
        public EnkaUserProfile MapEnkaUserProfile(EnkaProfileResponse model)
        {
            if (model == null) return null;

            var userProfile = new EnkaUserProfile
            {
                Username = model.Username,
                UserId = model.Id,
            };

            if (model.Profile != null)
            {
                userProfile.Bio = model.Profile.Bio;
                userProfile.Level = model.Profile.Level;
                userProfile.AvatarUrl = model.Profile.Avatar;
                userProfile.ProfileImageUrl = model.Profile.ImageUrl;
            }

            // Map hoyos to HoyoAccount components
            if (model.Hoyos != null)
            {
                userProfile.HoyoAccounts = MapHoyoAccounts(model.Hoyos);
            }

            return userProfile;
        }

        /// <summary>
        /// Maps raw hoyo data to HoyoAccount components with game type identification.
        /// </summary>
        /// <param name="hoyos">Dictionary of raw hoyo account models keyed by hash</param>
        /// <returns>List of HoyoAccount components</returns>
        public List<HoyoAccount> MapHoyoAccounts(Dictionary<string, HoyoAccountModel> hoyos)
        {
            var result = new List<HoyoAccount>();

            if (hoyos == null)
            {
                return result;
            }

            foreach (var kvp in hoyos)
            {
                var hoyoAccount = MapHoyoAccount(kvp.Key, kvp.Value);
                if (hoyoAccount != null)
                {
                    result.Add(hoyoAccount);
                }
            }

            return result;
        }

        /// <summary>
        /// Maps a single raw hoyo account model to a HoyoAccount component.
        /// </summary>
        /// <param name="hash">The hash identifier for the hoyo</param>
        /// <param name="model">The raw hoyo account model</param>
        /// <returns>A HoyoAccount component with game type identified</returns>
        public HoyoAccount MapHoyoAccount(string hash, HoyoAccountModel model)
        {
            if (model == null)
            {
                return null;
            }

            var hoyoAccount = new HoyoAccount
            {
                Hash = hash,
                Uid = model.Uid,
                Region = model.Region,
                IsVerified = model.Verified,
                IsPublic = model.Public,
                Order = model.Order,
                GameType = IdentifyGameType(model.PlayerInfo),
            };

            // Extract nickname and level from player_info if available
            ExtractPlayerInfo(model.PlayerInfo, hoyoAccount);

            return hoyoAccount;
        }

        /// <summary>
        /// Identifies the game type from the player_info JSON structure.
        /// Each game has distinct fields in its player_info structure.
        /// </summary>
        /// <param name="playerInfo">The player_info JSON element</param>
        /// <returns>The identified GameType</returns>
        private GameType IdentifyGameType(JsonElement? playerInfo)
        {
            if (playerInfo == null || playerInfo.Value.ValueKind == JsonValueKind.Null)
            {
                // Default to Genshin if no player_info available
                return GameType.Genshin;
            }

            var element = playerInfo.Value;

            // ZZZ has unique fields: ShowcaseDetail, SocialDetail
            if (TryGetProperty(element, "ShowcaseDetail") || TryGetProperty(element, "SocialDetail"))
            {
                return GameType.ZZZ;
            }

            // HSR has unique fields: detailInfo (when nested) or headIcon, recordInfo at root
            if (TryGetProperty(element, "detailInfo") || 
                TryGetProperty(element, "headIcon") || 
                TryGetProperty(element, "recordInfo") ||
                TryGetProperty(element, "avatarDetailList"))
            {
                return GameType.HSR;
            }

            // Genshin has unique fields: worldLevel, towerFloorIndex, showAvatarInfoList, profilePicture
            if (TryGetProperty(element, "worldLevel") || 
                TryGetProperty(element, "towerFloorIndex") || 
                TryGetProperty(element, "showAvatarInfoList") ||
                TryGetProperty(element, "profilePicture"))
            {
                return GameType.Genshin;
            }

            // Default to Genshin if unable to determine
            return GameType.Genshin;
        }

        /// <summary>
        /// Extracts nickname and level from the player_info JSON based on game type.
        /// </summary>
        /// <param name="playerInfo">The player_info JSON element</param>
        /// <param name="hoyoAccount">The HoyoAccount to populate</param>
        private void ExtractPlayerInfo(JsonElement? playerInfo, HoyoAccount hoyoAccount)
        {
            if (playerInfo == null || playerInfo.Value.ValueKind == JsonValueKind.Null)
            {
                return;
            }

            var element = playerInfo.Value;

            switch (hoyoAccount.GameType)
            {
                case GameType.Genshin:
                    ExtractGenshinPlayerInfo(element, hoyoAccount);
                    break;
                case GameType.HSR:
                    ExtractHSRPlayerInfo(element, hoyoAccount);
                    break;
                case GameType.ZZZ:
                    ExtractZZZPlayerInfo(element, hoyoAccount);
                    break;
            }
        }

        private void ExtractGenshinPlayerInfo(JsonElement element, HoyoAccount hoyoAccount)
        {
            // Genshin player_info has nickname and level at root
            if (element.TryGetProperty("nickname", out var nickname))
            {
                hoyoAccount.Nickname = nickname.GetString();
            }
            if (element.TryGetProperty("level", out var level) && level.TryGetInt32(out var levelValue))
            {
                hoyoAccount.Level = levelValue;
            }
        }

        private void ExtractHSRPlayerInfo(JsonElement element, HoyoAccount hoyoAccount)
        {
            // HSR player_info may have detailInfo wrapper or be at root
            var infoElement = element;
            if (element.TryGetProperty("detailInfo", out var detailInfo))
            {
                infoElement = detailInfo;
            }

            if (infoElement.TryGetProperty("nickname", out var nickname))
            {
                hoyoAccount.Nickname = nickname.GetString();
            }
            if (infoElement.TryGetProperty("level", out var level) && level.TryGetInt32(out var levelValue))
            {
                hoyoAccount.Level = levelValue;
            }
        }

        private void ExtractZZZPlayerInfo(JsonElement element, HoyoAccount hoyoAccount)
        {
            // ZZZ player_info has SocialDetail.ProfileDetail with nickname and level
            if (element.TryGetProperty("SocialDetail", out var socialDetail))
            {
                if (socialDetail.TryGetProperty("ProfileDetail", out var profileDetail))
                {
                    if (profileDetail.TryGetProperty("Nickname", out var nickname))
                    {
                        hoyoAccount.Nickname = nickname.GetString();
                    }
                    if (profileDetail.TryGetProperty("Level", out var level) && level.TryGetInt32(out var levelValue))
                    {
                        hoyoAccount.Level = levelValue;
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to check if a JSON element has a specific property.
        /// </summary>
        private bool TryGetProperty(JsonElement element, string propertyName)
        {
            return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out _);
        }
    }
}
