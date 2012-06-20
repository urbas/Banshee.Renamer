// 
// SimpleFilenameCreator.cs
// 
// Author:
//   Matej Urbas <matej.urbas@gmail.com>
// 
// Copyright 2012 Matej Urbas
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Text.RegularExpressions;
using Hyena;
using Banshee.Collection.Database;
using System.Collections.Generic;
using Mono.Unix;
using System.Text;

namespace Banshee.Renamer
{
    #region Compiler
    /// <summary>
    /// A simple `filename creator` implementation.
    ///
    /// <para>
    /// Here is an example pattern for this creator:
    /// <code>
    ///     "/some/directories/{artist}/{album}/{track number} - {artist} - {album} - {title}"
    /// </code>
    /// </para>
    /// </summary>
    public class SimplePatternCompiler : IFilenamePatternCompiler
    {
        public const string CompilerName = "Renamer Pattern v1";

        public ICompiledFilenamePattern CompilePattern (string pattern)
        {
            SfcPattern compiledPattern = new SfcPattern (pattern, this);
            return compiledPattern;
        }

        public string Name {
            get {
                return CompilerName;
            }
        }
    }
    #endregion

    #region Compiled Pattern
    /// <summary>
    /// This class contains the main part of the filename generation
    /// and pattern parsing and compilation.
    /// Sfc stands for 'simple filename creator'.
    /// </summary>
    internal sealed class SfcPattern : SimplePattern
    {
        #region Private Compiled State (instance)
        private List<SfcSegment> segments = new List<SfcSegment> ();
        #endregion

        #region Regex Stuff (Static)
        private const string ParameterStart = "[";
        private const string ParameterStartRegex = @"\[";
        private const string ParameterEnd = "]";
        private const string ParameterEndRegex = @"\]";
        private const string EscapedCharacters = ParameterStartRegex + ParameterEndRegex + @"\\";
        private const string RegexStringEscapedSequences = @"\\[" + EscapedCharacters + "]";

        /// <summary>
        /// This regular expression matches parameters of the form "[...]", where the escape
        /// character is '\'. See <see cref="EscapedCharacters"/> for a list
        /// of escape sequences.
        /// </summary>
        private static readonly Regex ParameterMatcher = new Regex (
        // The start of the parameter must be an unescaped '[' character.
            @"(^|[^\\])" + ParameterStartRegex +
        // Now collect the actual parameter (which can be any string that does not contain an unescaped ']').
            @"(?<parameter>([^" + EscapedCharacters + "]|" + RegexStringEscapedSequences + @")*)" +
            ParameterEndRegex,
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// This regex finds only valid escape sequences.
        /// </summary>
        private static readonly Regex ValidEscapeSequences = new Regex (RegexStringEscapedSequences, RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// This regex finds only invalid escape sequences.
        /// </summary>
        private static readonly Regex InvalidEscapeSequences = new Regex (@"\\[^" + EscapedCharacters + "]", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// This regex finds valid as well as invalid escape sequences. I.e.: all strings of the form: '\[character]'.
        /// </summary>
        private static readonly Regex AllEscapeSequences = new Regex (@"\\.", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        #endregion

        #region Constructors
        internal SfcPattern (string sourcePattern, SimplePatternCompiler owner) : base(sourcePattern, owner)
        {
            segments = ExtractPatternSegments (sourcePattern);
        }
        #endregion

        #region IFilenamePatternCompiler implementation
        public override void CreateFilename (System.Text.StringBuilder output, DatabaseTrackInfo song, Func<DatabaseTrackInfo, string, object> parameterMap)
        {
            int segmentsCount = segments.Count;
            for (int i = 0; i < segmentsCount; i++) {
                output.Append (segments [i].ToString (song, parameterMap));
            }
        }
        #endregion

        #region Helper Methods (static)
        /// <summary>
        /// Extracts the pattern segments (the compiled version of the `sourcePattern`).
        /// </summary>
        private static List<SfcSegment> ExtractPatternSegments (string sourcePattern)
        {
            CheckSyntax (sourcePattern);

            var segments = new List<SfcSegment> ();

            // Parse the pattern string and extract all of its segments:
            // TODO: Solve this without regular expressions (do a linear scan).
            var matcher = ParameterMatcher.Match (sourcePattern);
            // The index of the character right after the last found parameter:
            int nonParameterIndex = 0;
            while (matcher.Success) {

                var parameter = matcher.Groups [1];
                // Is there some text between the start of this parameter and the last index?
                // parameter.Index is the index of the character right after '[', this is why we look at 'parameter.Index'.
                if (nonParameterIndex < parameter.Index - 1) {
                    // Yes, there is some text. Add it as a segment:
                    segments.Add (CreateLiteralSegment (sourcePattern, nonParameterIndex, sourcePattern.Substring (nonParameterIndex, parameter.Index - nonParameterIndex - 1)));
                }
                nonParameterIndex = matcher.Index + matcher.Length;
                // Now add the parameter:
                segments.Add (CreateParameterSegment (sourcePattern, parameter.Index - 1, parameter.Value));
                matcher = matcher.NextMatch ();
            }

            if (nonParameterIndex < sourcePattern.Length) {
                segments.Add (CreateLiteralSegment (sourcePattern, nonParameterIndex, sourcePattern.Substring (nonParameterIndex)));
            }

            // Go through all segments and compile them completely:
            for (int i = segments.Count - 1; i >= 0; --i) {
                segments [i].Compile ();
            }

            return segments;
        }

        /// <summary>
        /// Checks the syntax of the raw pattern (as typed by the user)
        /// and throws a <see cref="PatternCompilationException"/> with
        /// additional information if the pattern has wrong escape sequences
        /// anywhere.
        /// </summary>
        public static void CheckSyntax (string rawPattern)
        {
            if (rawPattern == null) {
                throw new PatternCompilationException (Catalog.GetString ("The given pattern is 'null'. This pattern compiler cannot work with null patterns."));
            }

            // Go through all escape sequences and check that they are correct:
            //  1.) There may not be any invalid escape sequences:
            var m = AllEscapeSequences.Match (rawPattern);
            while (m.Success) {
                if (InvalidEscapeSequences.IsMatch (m.Value)) {
                    throw new PatternCompilationException (string.Format (Catalog.GetString ("The pattern contains an invalid escape sequence '{0}'."), m.Value), rawPattern, m.Index + 1);
                }
                m = AllEscapeSequences.Match (rawPattern, m.Index + m.Length);
            }
        }

        /// <summary>
        /// Unescapes the pattern source string.
        /// </summary>
        public static string UnescapeString (string escaped)
        {
            string unescaped = ValidEscapeSequences.Replace (escaped, m => {
                return m.Value [1].ToString ();
            });
            return unescaped;
        }
        #endregion

        #region Factory Methods
        public static SfcSegment CreateLiteralSegment (string sourcePattern, int indexInSource, string content)
        {
            return new SfcSegment (sourcePattern, indexInSource, content);
        }

        /// <summary>
        /// Creates the parameter segment.
        /// </summary>
        /// <param name='content'>
        /// This should be the unescaped content of the parameter notation. E.g.: '[parameter content]'.
        /// Depending on the `parameter content`, this method chooses a specific parameter segment.
        /// For example, if the content starts with 'S<' then this method will create and return an SfcFormattedParameter.
        /// </param>
        public static SfcSegment CreateParameterSegment (string sourcePattern, int indexInSource, string content)
        {
            return SfcMultiFormattedParameter.ConstructParameter (sourcePattern, indexInSource, content) ??
                SfcFormattedParameter.ConstructParameter (sourcePattern, indexInSource, content) ??
                new SfcParameter (sourcePattern, indexInSource, content);
        }
        #endregion
    }
    #endregion

    #region Segments
    internal class SfcSegment
    {
        #region Constructor
        public SfcSegment (string sourcePattern, int indexInSource, string content)
        {
            IndexInSource = indexInSource;
            SourcePattern = sourcePattern;
            RawContent = content;
            RawContentUnescaped = SfcPattern.UnescapeString (content);
        }
        #endregion

        #region Data
        public readonly int IndexInSource;

        public string SourcePattern { get; private set; }

        public string RawContent { get; private set; }

        public string RawContentUnescaped { get; private set; }
        #endregion

        #region Customisable Behaviour
        public virtual string ToString (DatabaseTrackInfo song, Func<DatabaseTrackInfo, string, object> parameterMap)
        {
            return RawContentUnescaped;
        }

        internal virtual void Compile ()
        {
        }
        #endregion
    }

    internal class SfcParameter : SfcSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Banshee.Renamer.SfcParameter"/> class.
        /// </summary>
        /// <param name='content'>
        /// Content.
        /// </param>
        /// <param name='indexInSource'>
        /// Index in the original raw (escaped) source pattern (the index of the '{' character).
        /// </param>.
        public SfcParameter (string sourcePattern, int indexInSource, string content) : base(sourcePattern, indexInSource, content)
        {
        }

        public override string ToString (DatabaseTrackInfo song, Func<DatabaseTrackInfo, string, object> parameterMap)
        {
            return (parameterMap (song, RawContentUnescaped) ?? "").ToString ();
        }
    }

    internal class SfcFormattedParameter : SfcSegment
    {
        internal const char DelimiterStart = '<';
        internal const char DelimiterEnd = '>';
        internal const string Header = "S<";
        internal const char AlignmentFormatStringDelimiter = ':';
        internal const string SyntaxOutline = "[S<format>parameter] or [S<alignment:format>parameter]";

        internal static SfcSegment ConstructParameter (string wholePattern, int indexInPattern, string parameterContent)
        {
            if (parameterContent.StartsWith (Header)) {
                return new SfcFormattedParameter (wholePattern, indexInPattern, parameterContent);
            } else {
                return null;
            }
        }

        public SfcFormattedParameter (string sourcePattern, int indexInSource, string content) : base(sourcePattern, indexInSource, content)
        {
        }

        public string Alignment { get; private set; }

        public string FormatString { get; private set; }
        /// <summary>
        /// The final format string. Can be used directly in string.Format(this.Format, singleObj).
        /// </summary>
        public string Format { get; private set; }

        public string ParameterName { get; private set; }

        internal override void Compile ()
        {
            if (!RawContentUnescaped.StartsWith (Header) || RawContentUnescaped.LastIndexOf (DelimiterEnd) < Header.Length) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("This 'S-type' format parameter has an invalid syntax. Expected syntax: {0}"), SyntaxOutline), SourcePattern, IndexInSource);
            }

            string formatPart;
            string parameters;
            if (!SfcFormattedParameter.ExtractFormatAndParameters(DelimiterStart, DelimiterEnd, RawContentUnescaped, out formatPart, out parameters)) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("This 'S-type' format parameter has an invalid syntax. Expected syntax: {0}"), SyntaxOutline), SourcePattern, IndexInSource);
            }

            // Extract the format string and the optional alignment.
            int indexOfFormatDelimiter = formatPart.IndexOf (AlignmentFormatStringDelimiter);
            if (indexOfFormatDelimiter < 0) {
                FormatString = formatPart;
            } else {
                Alignment = formatPart.Substring (0, indexOfFormatDelimiter);
                FormatString = formatPart.Substring (indexOfFormatDelimiter + 1);
            }

            // Compile the final format string. Can be used directly in string.Format().
            if (string.IsNullOrEmpty (Alignment)) {
                if (string.IsNullOrEmpty (FormatString)) {
                    Format = "{0}";
                } else {
                    Format = "{0:" + FormatString + "}";
                }
            } else {
                if (string.IsNullOrEmpty (FormatString)) {
                    Format = "{0," + Alignment + "}";
                } else {
                    Format = "{0," + Alignment + ":" + FormatString + "}";
                }
            }

            // Now store the name of the parameter:
            ParameterName = parameters;
        }

        public override string ToString (DatabaseTrackInfo song, Func<DatabaseTrackInfo, string, object> parameterMap)
        {
            return string.Format (Format, parameterMap (song, ParameterName));
        }

        /// <summary>
        /// Extracts the format and parameters if the string is of the following format:
        ///     HEADER'startDelimiter'format'endDelimiter'parameters
        /// </summary>
        public static bool ExtractFormatAndParameters(char startDelimiter, char endDelimiter, string content, out string format, out string parameters) {
            int idxFormatEnd = content.LastIndexOf(endDelimiter);
            int idxFormatStart = content.IndexOf(startDelimiter);
            if (idxFormatEnd > idxFormatStart) {
                format = content.Substring(idxFormatStart + 1, idxFormatEnd - idxFormatStart - 1);
                parameters = content.Substring(idxFormatEnd + 1);
                return true;
            } else {
                format = null;
                parameters = null;
                return false;
            }
        }
    }

    internal class SfcMultiFormattedParameter : SfcSegment
    {
        internal const char DelimiterStart = '<';
        internal const char DelimiterEnd = '>';
        internal static readonly char[] ParameterDelimiter = new char[]{','};
        internal const string Header1 = "F<";
        internal const string Header2 = "FC<";
        internal const string SyntaxOutline = "[F<format>P1,P2,...] or [FC<format>P1,P2,...]";

        internal static SfcSegment ConstructParameter (string wholePattern, int indexInPattern, string parameterContent)
        {
            if (parameterContent.StartsWith (Header1) || parameterContent.StartsWith (Header2)) {
                return new SfcMultiFormattedParameter (wholePattern, indexInPattern, parameterContent);
            } else {
                return null;
            }
        }

        public SfcMultiFormattedParameter (string sourcePattern, int indexInSource, string content) : base(sourcePattern, indexInSource, content)
        {
        }

        public bool IsConditional { get; private set; }

        public string Format { get; private set; }

        public string[] Parameters { get; private set; }

        internal override void Compile ()
        {
            if (RawContentUnescaped.StartsWith (Header1)) {
                IsConditional = false;
            } else if (RawContentUnescaped.StartsWith (Header2)) {
                IsConditional = true;
            } else {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("This 'F-type' format parameter has an invalid syntax. Expected syntax: {0}"), SyntaxOutline), SourcePattern, IndexInSource);
            }

            string format;
            string parameters;
            if (!SfcFormattedParameter.ExtractFormatAndParameters(DelimiterStart, DelimiterEnd, RawContentUnescaped, out format, out parameters)) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("This 'F-type' format parameter has an invalid syntax. Expected syntax: {0}"), SyntaxOutline), SourcePattern, IndexInSource);
            }
            // Extract the format string:
            Format = format;
            // Now extract the parameter names (delimited with commas).
            Parameters = parameters.Split (ParameterDelimiter, StringSplitOptions.RemoveEmptyEntries);

            if (Parameters == null || Parameters.Length < 1) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("The 'F-type' format requires at least one parameter. Expected syntax: {0}"), SyntaxOutline), SourcePattern, IndexInSource);
            }

            // Test out the format:
            object[] dummyValues = new object[Parameters.Length];
            try {
                string.Format (Format, dummyValues);
            } catch (FormatException fex) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("The format '{0}' is not a valid .NET string format. Format error: '{1}'"), Format, fex.Message), SourcePattern, IndexInSource);
            }
        }

        public override string ToString (DatabaseTrackInfo song, Func<DatabaseTrackInfo, string, object> parameterMap)
        {
            object firstParameter = parameterMap (song, Parameters [0]);
            if (IsConditional && firstParameter == null) {
                return string.Empty;
            }
            // NOTE: We cannot reuse a pre-allocated array as this method may be
            // called from multiple threads.
            object[] values = new object[Parameters.Length];
            values [0] = firstParameter;
            for (int i = Parameters.Length - 1; i > 0; --i) {
                values [i] = parameterMap (song, Parameters [i]);
            }
            return string.Format (Format, values);
        }
    }
    #endregion
}

