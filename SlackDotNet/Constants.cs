using System;
using System.Collections.Generic;
using System.Text;

namespace SlackDotNet
{
    /// <summary>
    /// Stores constants.
    /// </summary>
    public static class StringConstants
    {
        /// <summary>
        /// Gets the string 'true'.
        /// </summary>
        public const string True = "true";

        /// <summary>
        /// Gets the string 'false'.
        /// </summary>
        public const string False = "false";

        /// <summary>
        /// Gets the string '0'.
        /// </summary>
        public const string Zero = "0";

        /// <summary>
        /// Gets the string '1'.
        /// </summary>
        public const string One = "1";

        /// <summary>
        /// Gets the string 'full'.
        /// </summary>
        public const string Full = "full";

        /// <summary>
        /// Gets the string 'client'.
        /// </summary>
        public const string Client = "client";

        /// <summary>
        /// Gets the string 'none'.
        /// </summary>
        public const string None = "none";

        /// <summary>
        /// Returns the constant string 'true' or 'false' based on the input boolean.
        /// </summary>
        /// <param name="value">Input boolean value to convert to string.</param>
        /// <returns>Returns the input boolean value as a lower case string.</returns>
        public static string FromBoolean(bool value)
        {
            if (value)
                return True;
            return False;
        }
    }
}
