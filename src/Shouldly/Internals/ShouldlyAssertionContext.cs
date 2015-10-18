using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Shouldly.Internals;

namespace Shouldly
{
    internal class ShouldlyAssertionContext : IShouldlyAssertionContext
    {
        public bool DeterminedOriginatingFrame { get; set; }
        public string ShouldMethod { get; set; }
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public string CodePart { get; set; }
        public StackFrame OriginatingFrame  { get; set; }
        public MethodBase UnderlyingShouldMethod { get; set; }

        public object Key { get; set; }

        private object _expected;
        public object Expected
        {
            get { return GetLineEndingInsensitiveStringIfNecessary(_expected); }
            set { _expected = value; }
        }
        private object _actual;
        public object Actual
        {
            get { return GetLineEndingInsensitiveStringIfNecessary(_actual); }
            set { _actual = value; }
        }
        private object GetLineEndingInsensitiveStringIfNecessary(object item)
        {
            if (item is string && Options.HasFlag(ShouldBeStringOptions.IgnoreLineEndings))
            {
                return new LineEndingInsensitiveString(item.As<string>());
            }
            return item;
        }

        public object Tolerance { get; set; }
        private Case? _caseSensitivity;
        public Case? CaseSensitivity
        {
            get { return _caseSensitivity; }
            set
            {
                if (value.HasValue && value.Value == Case.Insensitive)
                {
                    _options |= ShouldBeStringOptions.IgnoreCase;
                }
                else
                {
                    _options &= ~ShouldBeStringOptions.IgnoreCase;
                }
                _caseSensitivity = value;
            }
        }
        private ShouldBeStringOptions _options = ShouldBeStringOptions.None;
        public ShouldBeStringOptions Options
        {
            get { return _options; }
            set
            {
                _caseSensitivity = value.ToCase();
                _options = value;
            }
        }
        public TimeSpan? Timeout { get; set; }

        public bool IgnoreOrder { get; set; }

        // For now, this property cannot just check to see if "Actual != null". The term is overloaded. 
        // In some cases it means the "Actual" value is not relevant (eg: "dictionary.ContainsKey(key)") and in some
        // cases it means that the value is relevant, but during execution we got a null. (eg: Foo.ShouldBe(bar) where 
        // Foo is null). So for now, it is a flag needs to be set externally to determine whether or not the "Actual" value
        // is relevant.
        public bool HasRelevantActual { get; set; }
        public bool HasRelevantKey { get; set; }

        public bool IsNegatedAssertion { get { return ShouldMethod.Contains("Not"); } }
        public string CustomMessage { get; set; }

        /// <summary>
        /// Manually specify the parts of the context, default format is {codePart} {shouldMethod}....
        /// </summary>
        protected ShouldlyAssertionContext(string codePart, string shouldMethod, object expected, object actual = null)
        {
            CodePart = codePart;
            ShouldMethod = shouldMethod;
            Expected = expected;
            Actual = actual;
        }

        internal ShouldlyAssertionContext(object expected, object actual = null, StackTrace stackTrace = null)
        {
            stackTrace = stackTrace ?? new StackTrace(true);
            var i = 0;
            var currentFrame = stackTrace.GetFrame(i);

            if (currentFrame == null) throw new Exception("Unable to find test method");

            var shouldlyFrame = default(StackFrame);
            while (shouldlyFrame == null || IsShouldlyMethod(currentFrame.GetMethod()))
            {
                if (IsShouldlyMethod(currentFrame.GetMethod()))
                    shouldlyFrame = currentFrame;

                currentFrame = stackTrace.GetFrame(++i);

                // Required to support the DynamicShould.HaveProperty method that takes in a dynamic as a parameter.
                // Having a method that takes a dynamic really stuffs up the stack trace because the runtime binder
                // has to inject a whole heap of methods. Our normal way of just taking the next frame doesn't work.
                // The following two lines seem to work for now, but this feels like a hack. The conditions to be able to 
                // walk up stack trace until we get to the calling method might have to be updated regularly as we find more
                // scanarios. Alternately, it could be replaced with a more robust implementation.
                while ( currentFrame.GetMethod().DeclaringType == null ||
                        currentFrame.GetMethod().DeclaringType.FullName.StartsWith("System.Dynamic"))
                {
                    currentFrame = stackTrace.GetFrame(++i);
                }
            }

            var originatingFrame = currentFrame;

            var fileName = originatingFrame.GetFileName();

           DeterminedOriginatingFrame = fileName != null && File.Exists(fileName);
           ShouldMethod = shouldlyFrame.GetMethod().Name;
           UnderlyingShouldMethod = shouldlyFrame.GetMethod();
           FileName = fileName;
           LineNumber = originatingFrame.GetFileLineNumber() - 1;
           OriginatingFrame = originatingFrame;
           Expected = expected;
           Actual = actual;
           CodePart = GetCodePart();
        }

        private bool IsShouldlyMethod(MethodBase method)
        {
            if (method.DeclaringType == null)
                return false;
           
            return method.DeclaringType.GetCustomAttributes(typeof(ShouldlyMethodsAttribute), true).Any()
               || (method.DeclaringType.DeclaringType !=null && method.DeclaringType.DeclaringType.GetCustomAttributes(typeof(ShouldlyMethodsAttribute), true).Any());
        }

        private string GetCodePart()
        {
            var codePart = "Shouldly uses your source code to generate it's great error messages, build your test project with full debug information to get better error messages" +
                           "\nThe provided expression";

            if (DeterminedOriginatingFrame)
            {
                var codeLines = string.Join("\n", File.ReadAllLines(FileName).Skip(LineNumber).ToArray());

                var indexOf = codeLines.IndexOf(ShouldMethod);
                if (indexOf > 0)
                    codePart = codeLines.Substring(0, indexOf - 1).Trim();

                // When the static method is used instead of the extension method,
                // the code part will be "Should".
                // Using Endswith to cater for being inside a lambda
                if (codePart.EndsWith("Should"))
                {
                    codePart = GetCodePartFromParameter(indexOf, codeLines, codePart);
                }
                else
                {
                    codePart = codePart.RemoveVariableAssignment().RemoveBlock();
                }
            }
            return codePart;
        }

        private string GetCodePartFromParameter(int indexOfMethod, string codeLines, string codePart)
        {
            var indexOfParameters =
                indexOfMethod +
                ShouldMethod.Length;

            var parameterString = codeLines.Substring(indexOfParameters);
            // Remove generic parameter if need be
            parameterString = parameterString.StartsWith("<") 
                ? parameterString.Substring(parameterString.IndexOf(">", StringComparison.Ordinal) + 2)
                : parameterString.Substring(1);

            var parantheses = new Dictionary<char, char>
            {
                {'{', '}'},
                {'(', ')'},
                {'[', ']'}
            };

            var parameterFinishedKeys = new[] {',', ')'};

            var openParentheses = new List<char>();

            var found = false;
            var i = 0;
            while (!found && parameterString.Length > i)
            {
                var currentChar = parameterString[i];

                if (openParentheses.Count == 0 && parameterFinishedKeys.Contains(currentChar))
                {
                    found = true;
                    continue;
                }

                if (parantheses.ContainsKey(currentChar))
                {
                    openParentheses.Add(parantheses[currentChar]);
                }
                else if (openParentheses.Count > 0 && openParentheses.Last() == currentChar)
                {
                    openParentheses.RemoveAt(openParentheses.Count - 1);
                }

                i++;
            }

            if (found)
            {
                codePart = parameterString.Substring(0, i);
            }
            return codePart
                .StripLambdaExpressionSyntax()
                .CollapseWhitespace()
                .RemoveBlock()
                .Trim();
        }
    }
}