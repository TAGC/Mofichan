namespace Mofichan.Core.Utility
{
    /// <summary>
    /// Provides globally-available constant values.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// A regex pattern that matches Mofichan's name.
        /// <para></para>
        /// This pattern should be used with
        /// <see cref="System.Text.RegularExpressions.RegexOptions.IgnoreCase"/> 
        /// </summary>
        public const string IdentityMatch = @"(mofi|mofichan)";
    }
}
