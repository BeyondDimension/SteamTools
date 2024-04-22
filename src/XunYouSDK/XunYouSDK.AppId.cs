#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1601 // Partial elements should be documented
#pragma warning disable SA1205 // Partial elements should declare access
#pragma warning disable SA1303 // Const field names should begin with upper-case letter
#pragma warning disable SA1400 // Access modifier should be declared
#pragma warning disable SA1629 // Documentation text should end with a period

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Mobius;

static partial class XunYouSDK
{
    /// <summary>
    /// 合作 id，由迅游给出明确值
    /// </summary>
    const int appId = 0;

    /// <summary>
    /// 账号类型，由迅游给出明确值
    /// </summary>
    const int userType = 0;

    /// <summary>
    /// 渠道版本，由迅游给出明确值
    /// </summary>
    const string channel_no = "";

    const string webapi_host = "";

    const string webapi_vip_endtime = "";

    static string CalcWebApiSign(XunYouBaseRequest body)
    {
        return "";
    }
}