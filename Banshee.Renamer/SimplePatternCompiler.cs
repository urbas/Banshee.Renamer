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
    ///     "/some/directories/[artist]/[album]/[FC<{0:00} - >track number][artist] - [album] ([S<00>year]) - [title]"
    /// </code>
    /// </para>
    /// </summary>
    public class SimplePatternCompiler : IFilenamePatternCompiler
    {
        public const string CompilerName = "Renamer Pattern v1";
        private const string ExamplePattern3 = @"[directory]/[FC<{0:00} - >track number][artist] - [album] - [title]";
        private const string ExamplePattern2 = @"[directory]/[S<00>track number] - [artist] - [album] - [title]";
        private const string ExamplePattern1 = @"[home]/Music/[artist] - [album] - [title]";

        public ICompiledFilenamePattern CompilePattern (string pattern, Func<string, Func<DatabaseTrackInfo, object>> parameterMap)
        {
            SfcPattern compiledPattern = new SfcPattern (pattern, this, parameterMap);
            return compiledPattern;
        }

        public string Name {
            get {
                return CompilerName;
            }
        }

        public string Usage {
            get {
                return string.Format (Catalog.GetString (
                    "Pattern engine: `{0}`\n" +
                    "\n" +
                    "The pattern may include template placeholders with the following syntax:\n" +
                    "\n" +
                    "-   {1}: simple parameter (it's interpolated with track's info directly)\n" +
                    "-   {2}: formatted parameter (the track's is formatted using the .NET string formatting syntax),\n" +
                    "-   {3}: formatted multi-parameter (also using the .NET format syntax) where the `FC` means that an empty string will be produced if the first parameter is `null`.\n" +
                    "\n" +
                    "Some examples:\n" +
                    "\n" +
                    "-   {4}\n" +
                    "-   {5}\n" +
                    "-   {6}"
                    ), Name, SfcParameter.SyntaxOutline, SfcFormattedParameter.SyntaxOutline, SfcMultiFormattedParameter.SyntaxOutline, ExamplePattern1, ExamplePattern2, ExamplePattern3);
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

        #region Constructors
        internal SfcPattern (string sourcePattern, SimplePatternCompiler owner, Func<string, Func<DatabaseTrackInfo, object>> parameterMap) : base(sourcePattern, owner)
        {
            segments = ExtractPatternSegments (sourcePattern, parameterMap);
        }
        #endregion

        #region IFilenamePatternCompiler implementation
        public override void CreateFilename (System.Text.StringBuilder output, DatabaseTrackInfo song)
        {
            int segmentsCount = segments.Count;
            for (int i = 0; i < segmentsCount; i++) {
                output.Append (segments [i].ToString (song));
            }
        }
        #endregion

        #region Parsing and Compilation Methods (static)
        private const char EscapeCharacter = '\\';
        private const char ParamStartCharacter = '[';
        private const char ParamEndCharacter = ']';

        /// <summary>
        /// Just collecting text...
        /// </summary>
        private static void processState0 (string sourcePattern, int index, StringBuilder sb, ref int state)
        {
            char curChar = sourcePattern [index];
            switch (curChar) {
            case EscapeCharacter:
                state = 1;
                break;
            case ParamStartCharacter:
                state = 2;
                break;
            default:
                sb.Append (curChar);
                break;
            }
        }

        /// <summary>
        /// The previous character was the escape character and we are in text collection mode.
        /// </summary>
        private static void processState1 (string sourcePattern, int index, StringBuilder sb, ref int state)
        {
            char curChar = sourcePattern [index];
            switch (curChar) {
            case EscapeCharacter:
            case ParamStartCharacter:
                sb.Append (curChar);
                state = 0;
                break;
            default:
                throw new PatternCompilationException (Catalog.GetString ("An illegal escape sequence."), sourcePattern, index);
            }
        }

        /// <summary>
        /// We are in a parameter!
        /// </summary>
        private static void processState2 (string sourcePattern, int index, StringBuilder sb, ref int state)
        {
            char curChar = sourcePattern [index];
            switch (curChar) {
            case EscapeCharacter:
                state = 3;
                break;
            case ParamEndCharacter:
                state = 0;
                break;
            default:
                sb.Append (curChar);
                break;
            }
        }

        /// <summary>
        /// The previous character was the escape character and we are in a parameter.
        /// </summary>
        private static void processState3 (string sourcePattern, int index, StringBuilder sb, ref int state)
        {
            char curChar = sourcePattern [index];
            switch (curChar) {
            case EscapeCharacter:
            case ParamEndCharacter:
                sb.Append (curChar);
                state = 2;
                break;
            default:
                throw new PatternCompilationException (Catalog.GetString ("An illegal escape sequence."), sourcePattern, index);
            }
        }

        /// <summary>
        /// Extracts the pattern segments (the compiled version of the `sourcePattern`).
        /// This method uses a finite state automaton parser.
        /// TODO: The same should be done for parameter parsing.
        /// </summary>
        private static List<SfcSegment> ExtractPatternSegments (string sourcePattern, Func<string, Func<DatabaseTrackInfo, object>> parameterMap)
        {
            var segments = new List<SfcSegment> ();

            // If the source pattern is empty, don't do anything...
            if (string.IsNullOrEmpty (sourcePattern)) {
                return segments;
            }

            // Start consuming characters:
            int patternLength = sourcePattern.Length;
            StringBuilder sb = new StringBuilder (patternLength);
            int state = 0;
            int textStartIndex = 0;
            for (int curIndex = 0; curIndex < patternLength; curIndex++) {
                switch (state) {
                case 0: // We are accepting text...
                    processState0 (sourcePattern, curIndex, sb, ref state);
                    if (state == 2) {
                        // We have transitioned to a parameter. Put the text segment in:
                        if (sb.Length > 0) {
                            segments.Add (CreateLiteralSegment (sourcePattern, textStartIndex, sb.ToString ()));
                            sb.Clear ();
                        }
                        // Take a not of where the parameter started
                        textStartIndex = curIndex;
                    }
                    break;
                case 1: // We are accepting text and the previous character was an escape character!
                    processState1 (sourcePattern, curIndex, sb, ref state);
                    break;
                case 2: // We are in a parameter...
                    processState2 (sourcePattern, curIndex, sb, ref state);
                    if (state == 0) {
                        // We have transitioned back to text again. Put the parameter segment in:
                        segments.Add (CreateParameterSegment (sourcePattern, textStartIndex, sb.ToString ()));
                        sb.Clear ();
                        textStartIndex = curIndex + 1;
                    }
                    break;
                case 3: // We are in a parameter and the previous character was an escape character!
                    processState3 (sourcePattern, curIndex, sb, ref state);
                    break;
                }
            }

            // We have come to the end. Act according to which state we were last in:
            switch (state) {
            case 0: // We were collecting text. Just add whatever is in the SB...
                if (sb.Length > 0) {
                    segments.Add (CreateLiteralSegment (sourcePattern, textStartIndex, sb.ToString ()));
                }
                break;
            case 1:
            case 3:
                throw new PatternCompilationException (Catalog.GetString ("The pattern contains a dangling escape sequence."), sourcePattern, patternLength);
            default:
                throw new PatternCompilationException (Catalog.GetString ("The parameter was opened but never closed."), sourcePattern, textStartIndex + 1);
            }

            // All the segments have been collected. We only have to compile them now...
            for (int i = segments.Count - 1; i >= 0; --i) {
                segments [i].Compile (parameterMap);
            }

            return segments;
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
            Content = content;
        }
        #endregion

        #region Data
        public readonly int IndexInSource;

        public string SourcePattern { get; private set; }

        public string Content { get; private set; }
        #endregion

        #region Customisable Behaviour
        public virtual string ToString (DatabaseTrackInfo song)
        {
            return Content;
        }

        internal virtual void Compile (Func<string, Func<DatabaseTrackInfo, object>> parameterMap)
        {
        }

        public override string ToString ()
        {
            return Content;
        }
        #endregion
    }

    internal class SfcParameter : SfcSegment
    {
        internal const string SyntaxOutline = @"[parameter name]";

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

        public Func<DatabaseTrackInfo, object> ParameterLookup { get; private set; }

        internal override void Compile (Func<string, Func<DatabaseTrackInfo, object>> parameterMap)
        {
            ParameterLookup = parameterMap (Content);
            if (ParameterLookup == null) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("The parameter '{0}' is unknown."), Content), SourcePattern, IndexInSource);
            }
        }

        public override string ToString (DatabaseTrackInfo song)
        {
            return (ParameterLookup (song) ?? "").ToString ();
        }

        public override string ToString ()
        {
            return "[" + Content + "]";
        }
    }

    internal class SfcFormattedParameter : SfcSegment
    {
        internal const char DelimiterStart = '<';
        internal const char DelimiterEnd = '>';
        internal const string Header = "S<";
        // TODO: Introduce 'SC<'
        internal const char AlignmentFormatStringDelimiter = ':';
        internal const string SyntaxOutline = "[S<format>parameter name] or [S<alignment:format>parameter name]";

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

        public Func<DatabaseTrackInfo, object> ParameterLookup { get; private set; }

        internal override void Compile (Func<string, Func<DatabaseTrackInfo, object>> parameterMap)
        {
            if (!Content.StartsWith (Header) || Content.LastIndexOf (DelimiterEnd) < Header.Length) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("This 'S-type' format parameter has an invalid syntax. Expected syntax: {0}"), SyntaxOutline), SourcePattern, IndexInSource);
            }

            string formatPart;
            string parameters;
            if (!SfcFormattedParameter.ExtractFormatAndParameters (DelimiterStart, DelimiterEnd, Content, out formatPart, out parameters)) {
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
            ParameterLookup = parameterMap (ParameterName);
            if (ParameterLookup == null) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("The parameter '{0}' is unknown."), Content), SourcePattern, IndexInSource);
            }
        }

        public override string ToString (DatabaseTrackInfo song)
        {
            return string.Format (Format, ParameterLookup (song));
        }

        public override string ToString ()
        {
            return "[S<" + Format + ">" + ParameterName + "]";
        }

        /// <summary>
        /// Extracts the format and parameters if the string is of the following format:
        ///     HEADER'startDelimiter'format'endDelimiter'parameters
        /// </summary>
        public static bool ExtractFormatAndParameters (char startDelimiter, char endDelimiter, string content, out string format, out string parameters)
        {
            int idxFormatEnd = content.LastIndexOf (endDelimiter);
            int idxFormatStart = content.IndexOf (startDelimiter);
            if (idxFormatEnd > idxFormatStart) {
                format = content.Substring (idxFormatStart + 1, idxFormatEnd - idxFormatStart - 1);
                parameters = content.Substring (idxFormatEnd + 1);
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

        public string ParametersString { get; private set; }

        public string[] Parameters { get; private set; }

        public Func<DatabaseTrackInfo, object>[] ParameterLookups { get; private set; }

        internal override void Compile (Func<string, Func<DatabaseTrackInfo, object>> parameterMap)
        {
            if (Content.StartsWith (Header1)) {
                IsConditional = false;
            } else if (Content.StartsWith (Header2)) {
                IsConditional = true;
            } else {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("This 'F-type' format parameter has an invalid syntax. Expected syntax: {0}"), SyntaxOutline), SourcePattern, IndexInSource);
            }

            string format;
            string parameters;
            if (!SfcFormattedParameter.ExtractFormatAndParameters (DelimiterStart, DelimiterEnd, Content, out format, out parameters)) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("This 'F-type' format parameter has an invalid syntax. Expected syntax: {0}"), SyntaxOutline), SourcePattern, IndexInSource);
            }
            // Extract the format string:
            Format = format;
            ParametersString = parameters;
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
            ParameterLookups = new Func<DatabaseTrackInfo, object>[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++) {
                ParameterLookups [i] = parameterMap (Parameters [i]);
                if (ParameterLookups [i] == null) {
                    throw new PatternCompilationException (string.Format (Catalog.GetString ("The parameter '{0}' is unknown."), Parameters [i]), SourcePattern, IndexInSource);
                }
            }
        }

        public override string ToString (DatabaseTrackInfo song)
        {
            object firstParameter = ParameterLookups [0] (song);
            if (IsConditional && firstParameter == null) {
                return string.Empty;
            }

            // A small optimisation if there is a small number of parameters:
            int len = Parameters.Length;
            if (len == 1) {
                return string.Format (Format, firstParameter);
            } else if (len == 2) {
                return string.Format (Format, firstParameter, ParameterLookups [1] (song));
            } else if (len == 3) {
                return string.Format (Format, firstParameter, ParameterLookups [1] (song), ParameterLookups [2] (song));
            } else {
                // NOTE: We cannot reuse a pre-allocated array as this method may be
                // called from multiple threads.
                object[] values = new object[Parameters.Length];
                for (int i = Parameters.Length - 1; i > 0; --i) {
                    values [i] = ParameterLookups [i] (song);
                }
                values[0] = firstParameter;
                return string.Format (Format, values);
            }
        }

        public override string ToString ()
        {
            if (IsConditional) {
                return "[FC<" + Format + ">" + ParametersString + "]";
            } else {
                return "[F<" + Format + ">" + ParametersString + "]";
            }
        }
    }
    #endregion
}

