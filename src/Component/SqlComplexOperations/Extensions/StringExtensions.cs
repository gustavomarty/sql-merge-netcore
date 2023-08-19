using System.Text;
using System.Text.RegularExpressions;

namespace SqlComplexOperations.Extensions
{
    internal static class StringExtensions
    {
        internal static string ToSnakeCase(this string text)
        {
            if(text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if(text.Length < 2)
            {
                return text;
            }

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));

            for(int i = 1; i < text.Length; ++i)
            {
                char c = text[i];
                if(char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        internal static string ToPascalCase(this string original)
        {
            Regex invalidCharsRgx = new ("[^_a-zA-Z0-9]");
            Regex whiteSpace = new (@"(?<=\s)");
            Regex startsWithLowerCaseChar = new ("^[a-z]");
            Regex firstCharFollowedByUpperCasesOnly = new ("(?<=[A-Z])[A-Z0-9]+$");
            Regex lowerCaseNextToNumber = new ("(?<=[0-9])[a-z]");
            Regex upperCaseInside = new ("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

            var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(original, "_"), string.Empty)
                .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

            return string.Concat(pascalCase);
        }
    }
}
