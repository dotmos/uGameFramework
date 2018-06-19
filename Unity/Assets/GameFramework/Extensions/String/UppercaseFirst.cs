namespace CustomExtensions
{
    //Extension methods must be defined in a static class
    public static partial class StringExtension
    {
        /// <summary>
        /// Uppercase the first character of this string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string UppercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
