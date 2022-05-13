namespace System.Globalization
{
    public static class CultureInfo2
    {
        public static bool IsMatch(CultureInfo cultureInfo, string cultureName)
        {
            if (String2.IsNullOrWhiteSpace(cultureInfo.Name))
            {
                return false;
            }
            if (cultureInfo.Name == cultureName)
            {
                return true;
            }
            else
            {
                return IsMatch(cultureInfo.Parent, cultureName);
            }
        }
    }
}
