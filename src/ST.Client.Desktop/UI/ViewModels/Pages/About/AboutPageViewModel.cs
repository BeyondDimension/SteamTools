namespace System.Application.UI.ViewModels
{
    partial class AboutPageViewModel : TabItemViewModel
    {
        public static AboutPageViewModel Instance { get; } = new();

        public override string Name
        {
            get => TitleName;
            protected set { throw new NotSupportedException(); }
        }

        public static string RmbadminSteamLink => SteamApiUrls.MY_PROFILE_URL;

        public static string RmbadminLink => UrlConstants.GitHub_User_Rmbadmin;

        public static string AigioLLink => UrlConstants.GitHub_User_AigioL;

        public static string MossimosLink => UrlConstants.GitHub_User_Mossimos;

        public static string CliencerLink => UrlConstants.BILI_User_Cliencer;

        public static string PrivacyLink => UrlConstants.OfficialWebsite_Privacy;

        public static string AgreementLink => UrlConstants.OfficialWebsite_Agreement;

        public static string OfficialLink => UrlConstants.OfficialWebsite;

        public static string SourceCodeLink => UrlConstants.GitHub_Repository;

        public static string UserSupportLink => UrlConstants.OfficialWebsite_Contact;

        public static string BugReportLink => UrlConstants.GitHub_Issues;

        public static string FAQLink => UrlConstants.OfficialWebsite_Faq;

        public static string LicenseLink => UrlConstants.License_GPLv3;
    }
}