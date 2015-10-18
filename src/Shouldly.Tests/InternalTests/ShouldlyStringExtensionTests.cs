using NUnit.Framework;

namespace Shouldly.Tests.InternalTests
{
    [TestFixture]
    public class ShouldlyStringExtensionTests
    {
        [Test]
        public void Clip_ShouldNotReduceTheSizeOfAStringSmallerThanTheMaximumLength()
        {
            "small".ShouldMatch("small".Clip(10));
        }

        [Test]
        public void Clip_ShouldReduceTheSizeOfAStringLongerThanTheMaximumLength()
        {
            "largestrin".ShouldMatch("largestringtoclip".Clip(10));
        }

        [Test]
        public void Clip_ShouldHandleEmptyStrings()
        {
            string.Empty.ShouldMatch(string.Empty.Clip(10));
        }

        [Test]
        public void ClipWithEllipsis_ShouldNotReduceTheSizeOfAStringSmallerThanTheMaximumLength()
        {
            "small".ShouldMatch("small".Clip(10, "..."));
        }

        [Test]
        public void ClipWithEllipsis_ShouldReduceTheSizeOfAStringLongerThanTheMaximumLength()
        {
            "largestrin...".ShouldMatch("largestringtoclip".Clip(10, "..."));
        }

        [Test]
        public void ClipWithEllipsis_ShouldHandleEmptyStrings()
        {
            string.Empty.ShouldMatch(string.Empty.Clip(10, "..."));
        }

        [Test]
        public void NormalizeLineEndings_ShouldReturnNullWhenGivenNull()
        {
            string s = null;
            s.NormalizeLineEndings().ShouldBe(null);
        }

        [Test]
        public void NormalizeLineEndings_ShouldReturnSameStringIfNoLineEndings()
        {
            "oneline".NormalizeLineEndings().ShouldBe("oneline");
        }

        [Test]
        public void NormalizeLineEndings_ShouldReturnStringWithNormalizedLineEndings()
        {
            "line1\nline2".NormalizeLineEndings().ShouldBe("line1\nline2");
            "line1\r\nline2".NormalizeLineEndings().ShouldBe("line1\nline2");
            "line1\rline2".NormalizeLineEndings().ShouldBe("line1\nline2");
        }

        [Test]
        [TestCase("() => result")]
        [TestCase("( ) => result")]
        [TestCase("( ) =>result")]
        [TestCase("( )=> result")]
        [TestCase("( )=>result")]
        [TestCase("()=>result")]
        [TestCase(@"()
                        =>result")]
        [TestCase(@"()
                        => result")]
        [TestCase(@"()
                        =>
                        result")]

        [TestCase(@" =>         
                        result")] // The () might be on a different line. Shouldly context will only give us what is on the assertion line.
        public void StripLambdaExpressionSyntax_ShouldRemoveLambda(string input)
        {
            var inputWithLambdaExpressionStripped = input.StripLambdaExpressionSyntax();
            inputWithLambdaExpressionStripped.ShouldBe("result");
        }
    }
}
