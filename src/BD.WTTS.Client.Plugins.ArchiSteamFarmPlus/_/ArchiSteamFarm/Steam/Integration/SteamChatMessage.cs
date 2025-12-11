// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.Steam.Integration;

static class SteamChatMessage
{
    internal const char ContinuationCharacter = '…'; // A character used for indicating that the next newline part is a continuation of the previous line
    internal const byte ContinuationCharacterBytes = 3; // The continuation character specified above uses 3 bytes in UTF-8
    internal const ushort MaxMessageBytesForLimitedAccounts = 1945; // This is a limitation enforced by Steam
    internal const ushort MaxMessageBytesForUnlimitedAccounts = 6340; // This is a limitation enforced by Steam
    internal const ushort MaxMessagePrefixBytes = MaxMessageBytesForLimitedAccounts - ReservedContinuationMessageBytes - ReservedEscapeMessageBytes; // Simplified calculation, nobody should be using prefixes even close to that anyway
    internal const byte NewlineWeight = 61; // This defines how much weight a newline character is adding to the output, limitation enforced by Steam
    internal const char ParagraphCharacter = '¶'; // A character used for indicating that this is not the last part of message (2 bytes, so it fits in ContinuationCharacterBytes)
    internal const byte ReservedContinuationMessageBytes = ContinuationCharacterBytes * 2; // Up to 2 optional continuation characters
    internal const byte ReservedEscapeMessageBytes = 5; // 2 characters total, escape one '\' of 1 byte and real one of up to 4 bytes
}
