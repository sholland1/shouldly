using NUnit.Framework;
using Shouldly.DifferenceHighlighting;
using Shouldly.Internals;
using Shouldly.Tests.TestHelpers;

namespace Shouldly.Tests.InternalTests
{
    [TestFixture]
    public class LineEndingInsensitiveStringDifferenceHighlighterTests
    {
        private readonly LineEndingInsensitiveStringDifferenceHighlighter _highlighter = 
            new LineEndingInsensitiveStringDifferenceHighlighter();

        [Test]
        public void CanProcessTwoNotNullLineEndingInsensitiveStringsAndAShouldBeAssertionMethod()
        {
            _highlighter
                .CanProcess(new TestShouldlyAssertionContext(
                    new LineEndingInsensitiveString("string1"),
                    new LineEndingInsensitiveString("string2"))
                { ShouldMethod = "ShouldBe", Options = ShouldBeStringOptions.IgnoreLineEndings })
                .ShouldBe(true);
        }

        [Test]
        public void CanProcessTwoNotNullLineEndingInsensitiveStringsAndAShouldBeAssertionMethodAndIgnoreCaseOption()
        {
            _highlighter
                .CanProcess(new TestShouldlyAssertionContext(
                    new LineEndingInsensitiveString("string1"),
                    new LineEndingInsensitiveString("string2"))
                { ShouldMethod = "ShouldBe", Options = ShouldBeStringOptions.IgnoreLineEndings | ShouldBeStringOptions.IgnoreCase })
                .ShouldBe(true);
        }

        [Test]
        public void CannotProcessTwoNotNullStringsAndAShouldBeAssertionMethod()
        {
            _highlighter
                .CanProcess(new TestShouldlyAssertionContext("string1", "string2")
                { ShouldMethod = "ShouldBe", Options = ShouldBeStringOptions.IgnoreCase })
                .ShouldBe(false);
        }

        [Test]
        public void CannotProcessTwoNotNullLineEndingInsensitiveStringsAndANotShouldBeAssertionMethod()
        {
            _highlighter
                .CanProcess(new TestShouldlyAssertionContext(
                    new LineEndingInsensitiveString("string1"),
                    new LineEndingInsensitiveString("string2"))
                { ShouldMethod = "ShouldContain", Options = ShouldBeStringOptions.IgnoreLineEndings })
                .ShouldBe(false);
        }

        [Test]
        public void CannotProcessNullAndLineEndingInsensitiveString()
        {
            _highlighter
                .CanProcess(new TestShouldlyAssertionContext(
                    null, new LineEndingInsensitiveString("string1"))
                { Options = ShouldBeStringOptions.IgnoreLineEndings })
                .ShouldBe(false);
        }

        [Test]
        public void CannotProcessLineEndingInsensitiveStringAndNull()
        {
            _highlighter
                .CanProcess(new TestShouldlyAssertionContext(
                    new LineEndingInsensitiveString("string1"), null)
                { Options = ShouldBeStringOptions.IgnoreLineEndings })
                .ShouldBe(false);
        }
    }
}