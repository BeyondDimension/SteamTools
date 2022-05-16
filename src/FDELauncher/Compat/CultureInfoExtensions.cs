namespace System.Globalization
{
    public static class CultureInfoExtensions
    {
        public static bool IsMatch(this CultureInfo cultureInfo, string cultureName)
        {
            if (String2.IsNullOrWhiteSpace(cultureInfo.Name))
            {
                return false;
            }
            if (string.Equals(cultureInfo.Name, cultureName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return cultureInfo.Parent.IsMatch(cultureName);
            }
        }
    }
}
