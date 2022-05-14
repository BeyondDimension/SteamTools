namespace System.Diagnostics.Contracts
{
    public static class Contract
    {
        [Conditional("DEBUG")]
        [Conditional("CONTRACTS_FULL")]
        public static void Assert(bool condition, string? userMessage)
        {

        }
    }
}
