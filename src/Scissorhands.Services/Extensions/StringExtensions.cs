namespace Aliencube.Scissorhands.Services.Extensions
{
    public static class StringExtensions
    {
        public static bool StartsWith(this string value, char check)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var chars = value.ToCharArray();
            var result = chars[0].Equals(check);
            return result;
        }
    }
}