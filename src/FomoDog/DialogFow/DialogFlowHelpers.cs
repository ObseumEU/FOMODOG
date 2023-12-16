using System.Text.RegularExpressions;

namespace FomoDog.DialogFow
{
    public static class DialogFlowHelpers
    {
        public static string[] ExtractHttpsLinks(string inputText)
        {
            const string pattern = @"https://\S+";
            var matches = Regex.Matches(inputText, pattern);
            var links = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                links[i] = matches[i].Value;
            }

            return links;
        }
    }
}
