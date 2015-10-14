using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Shouldly
{
    [DebuggerStepThrough]
    [ShouldlyMethods]
    public static partial class ShouldBeStringTestExtensions
    {
        /// <summary>
        /// Perform a case sensitive string comparison
        /// </summary>
        [Obsolete("Use the ShouldBeStringOptions instead of the Case enum")]
        public static void ShouldBe(this string actual, string expected)
        {
            ShouldBe(actual, expected, () => null, ShouldBeStringOptions.None);
        }

        /// <summary>
        /// Perform a string comparison, specifying the desired case sensitivity
        /// </summary>
        [Obsolete("Use the ShouldBeStringOptions instead of the Case enum")]
        public static void ShouldBe(this string actual, string expected, Case caseSensitivity)
        {
            ShouldBe(actual, expected, () => null, caseSensitivity.ToOptions());
        }

        [Obsolete("Use the ShouldBeStringOptions instead of the Case enum")]
        public static void ShouldBe(this string actual, string expected, Case caseSensitivity, string customMessage)
        {
            ShouldBe(actual, expected, () => customMessage, caseSensitivity.ToOptions());
        }

        [Obsolete("Use the ShouldBeStringOptions instead of the Case enum")]
        public static void ShouldBe(this string actual, string expected, Case caseSensitivity, [InstantHandle] Func<string> customMessage)
        {
            ShouldBe(actual, expected, customMessage, caseSensitivity.ToOptions());
        }

        public static void ShouldBe(
            this string actual,
            string expected,
            ShouldBeStringOptions options = ShouldBeStringOptions.None)
        {
            ShouldBe(actual, expected, () => null, options);
        }
        public static void ShouldBe(
            this string actual,
            string expected,
            string customMessage,
            ShouldBeStringOptions options = ShouldBeStringOptions.None)
        {
            ShouldBe(actual, expected, () => customMessage, options);
        }
        public static void ShouldBe(
            this string actual,
            string expected,
            Func<string> customMessage,
            ShouldBeStringOptions options = ShouldBeStringOptions.None)
        {
            actual.AssertAwesomelyWithOptions(
                v => Is.StringEqualWithOptions(v, expected, options),
                actual, expected, options, customMessage);
        }
    }
}