namespace ReadySunValley.Helpers
{
    internal class Strings
    {
        public static readonly string TweetIntent = "https://twitter.com/intent/tweet?text=" + Locales.Locale.assetTweet;

        public static class Uri
        {
            public const string GitRepo = "https://github.com/builtbybel/ReadySunValley";
            public const string GitUpdateRepo = "https://github.com/builtbybel/readysunvalley/releases/tag/";
            public const string GitVersionCheck = "https://raw.githubusercontent.com/builtbybel/readysunvalley/master/appversion.txt";
            public const string GitVersionHint = "https://raw.githubusercontent.com/builtbybel/ReadySunValley/main/changes.txt";
            public const string GitTestingRelease = "https://github.com/builtbybel/ReadySunValley/issues/20";

            public const string CompareMS = "https://github.com/builtbybel/ReadySunValley/blob/main/assets/rsv-microsoft-requirements.png?raw=true";
            public const string CompareUtil = "https://github.com/rcmaehl/WhyNotWin11/releases/download/";
            public const string UtilVersionCheck = "https://raw.githubusercontent.com/builtbybel/ReadySunValley/main/utilversion.txt";

            public const string VotePage = "https://www.builtbybel.com/blog/19-apps/41-check-with-the-readysunvalley-app-if-your-device-works-with-windows11-sun-valley-update";

            public const string MSSystemRequirements = "https://aka.ms/WindowsSysReq";
        }
    }
}