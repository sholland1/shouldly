using System;

namespace Shouldly
{
    [Flags]
    public enum ShouldBeStringOptions
    {
        None = 0,
        IgnoreCase = 1,
        IgnoreLineEndings = 2
    }
    internal static class ShouldBeStringOptionsExtensions
    {
        public static Case ToCase(this ShouldBeStringOptions option)
        {
            return option.HasFlag(ShouldBeStringOptions.IgnoreCase)
                ? Case.Insensitive : Case.Sensitive;
        }
        public static Case? ToCase(this ShouldBeStringOptions? option)
        {
            if (!option.HasValue) return null;
            return option.Value.HasFlag(ShouldBeStringOptions.IgnoreCase)
                ? Case.Insensitive : Case.Sensitive;
        }
    }
}
