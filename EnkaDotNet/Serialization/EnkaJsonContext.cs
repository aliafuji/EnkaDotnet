#if NET8_0_OR_GREATER
using System.Collections.Generic;
using System.Text.Json.Serialization;
using EnkaDotNet.Models.EnkaProfile;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Models.ZZZ;

namespace EnkaDotNet.Serialization
{
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(ZZZApiResponse))]
    [JsonSerializable(typeof(ZZZPlayerInfoModel))]
    [JsonSerializable(typeof(ZZZShowcaseDetailModel))]
    [JsonSerializable(typeof(ZZZSocialDetailModel))]
    [JsonSerializable(typeof(ZZZTitleInfoModel))]
    [JsonSerializable(typeof(ZZZMedalModel))]
    [JsonSerializable(typeof(ZZZAvatarModel))]
    [JsonSerializable(typeof(ZZZSkillLevelModel))]
    [JsonSerializable(typeof(ZZZEquippedItemModel))]
    [JsonSerializable(typeof(ZZZEquipmentModel))]
    [JsonSerializable(typeof(ZZZPropertyModel))]
    [JsonSerializable(typeof(ZZZWeaponModel))]
    [JsonSerializable(typeof(ZZZProfileDetailModel))]
    [JsonSerializable(typeof(HSRApiResponse))]
    [JsonSerializable(typeof(HSRDetailInfo))]
    [JsonSerializable(typeof(HSRRecordInfoModel))]
    [JsonSerializable(typeof(HSRAvatarDetail))]
    [JsonSerializable(typeof(HSRSkillTreeModel))]
    [JsonSerializable(typeof(HSREquipment))]
    [JsonSerializable(typeof(HSREquipmentFlat))]
    [JsonSerializable(typeof(HSRPropertyInfo))]
    [JsonSerializable(typeof(HSRRelicModel))]
    [JsonSerializable(typeof(HSRSubAffix))]
    [JsonSerializable(typeof(HSRRelicFlat))]
    [JsonSerializable(typeof(ApiResponse))]
    [JsonSerializable(typeof(PlayerInfoModel))]
    [JsonSerializable(typeof(ShowAvatarInfoModel))]
    [JsonSerializable(typeof(ProfilePictureModel))]
    [JsonSerializable(typeof(AvatarInfoModel))]
    [JsonSerializable(typeof(PropValueModel))]
    [JsonSerializable(typeof(FetterInfoModel))]
    [JsonSerializable(typeof(EquipModel))]
    [JsonSerializable(typeof(WeaponModel))]
    [JsonSerializable(typeof(ReliquaryModel))]
    [JsonSerializable(typeof(FlatDataModel))]
    [JsonSerializable(typeof(StatPropertyModel))]
    [JsonSerializable(typeof(EnkaProfileResponse))]
    [JsonSerializable(typeof(EnkaProfileDetail))]
    [JsonSerializable(typeof(HoyoAccountModel))]
    [JsonSerializable(typeof(RawBuildModel))]
    [JsonSerializable(typeof(Dictionary<string, HoyoAccountModel>))]
    [JsonSerializable(typeof(Dictionary<string, List<RawBuildModel>>))]
    [JsonSerializable(typeof(List<RawBuildModel>))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(Dictionary<string, Dictionary<string, string>>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(List<ZZZAvatarModel>))]
    [JsonSerializable(typeof(List<ZZZMedalModel>))]
    [JsonSerializable(typeof(List<HSRAvatarDetail>))]
    [JsonSerializable(typeof(List<HSRRelicModel>))]
    [JsonSerializable(typeof(List<AvatarInfoModel>))]
    [JsonSerializable(typeof(List<EquipModel>))]
    public partial class EnkaJsonContext : JsonSerializerContext
    {
    }
}
#endif
