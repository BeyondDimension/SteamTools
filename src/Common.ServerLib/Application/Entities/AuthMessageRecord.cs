using System.Application.Columns;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace System.Application.Entities
{
    public class AuthMessageRecord<TSendSmsRequestType, TAuthMessageType> : IEntity<Guid>, ICreationTime
        where TSendSmsRequestType : notnull, Enum
        where TAuthMessageType : notnull, Enum
    {
        [Key] // EF 主键
        public Guid Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// 手机号(如果为短信验证码此字段必填)
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 邮箱地址(如果为邮箱验证码此字段必填)
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 调用方IP地址
        /// </summary>
        [Required] // EF not null
        [NotNull, DisallowNull] // C# 8 not null
        public string? IPAddress { get; set; }

        #region 提供商内容

        /// <summary>
        /// 第三方下发渠道的显示名称
        /// 比如网易/阿里云
        /// </summary>
        [Required] // EF not null
        [NotNull, DisallowNull] // C# 8 not null
        public string? Channel { get; set; }

        /// <summary>
        /// 第三方提供商返回的内容
        /// </summary>
        public string? SendResultRecord { get; set; }

        /// <summary>
        /// 第三方提供商返回的HTTP状态码
        /// </summary>
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// 第三方提供商发送是否成功
        /// 通常从调用第三方接口返回的JSON中获取状态码判定是否成功
        /// </summary>
        public bool SendIsSuccess { get; set; }

        #endregion

        /// <summary>
        /// 内容（短信验证码或邮箱验证码的内容）
        /// 短信内容通常仅含有验证码数字，不包括其他的提示语文字
        /// </summary>
        [Required] // EF not null
        [NotNull, DisallowNull] // C# 8 not null
        public string? Content { get; set; }

        /// <summary>
        /// 是否校验过
        /// </summary>
        public bool EverCheck { get; set; }

        /// <summary>
        /// 是否校验成功
        /// </summary>
        public bool CheckSuccess { get; set; }

        /// <summary>
        /// 是否被废弃
        /// </summary>
        public bool Abandoned { get; set; }

        /// <summary>
        /// 校验失败次数
        /// </summary>
        public int CheckFailuresCount { get; set; }

        /// <summary>
        /// 类型 是属于邮箱验证还是短信验证
        /// </summary>
        public TAuthMessageType Type { get; set; }

        /// <summary>
        /// 发送验证码用途
        /// </summary>
        public TSendSmsRequestType RequestType { get; set; }

        /// <summary>
        /// 所属用户Id
        /// </summary>
        public Guid? UserId { get; set; }
    }
}