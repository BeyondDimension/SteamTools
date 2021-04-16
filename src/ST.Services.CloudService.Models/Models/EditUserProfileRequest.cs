using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
    [MPObject]
    public class EditUserProfileRequest
    {
        [MPKey(0)]
        public string NickName { get; set; } = string.Empty;

        [MPKey(1)]
        public Guid? Avatar { get; set; }

        [MPKey(2)]
        public Gender Gender { get; set; }

        [MPKey(3)]
        public DateTime? BirthDate { get; set; }

        [MPKey(4)]
        public sbyte BirthDateTimeZone { get; set; }

        [MPKey(5)]
        public int? AreaId { get; set; }
    }
}