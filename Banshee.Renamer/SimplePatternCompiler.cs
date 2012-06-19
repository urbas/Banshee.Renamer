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

namespace Banshee.Renamer
{
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
            SFCPattern compiledPattern = new SFCPattern (pattern, this);
            return compiledPattern;
        }

        public string Name {
            get {
                return CompilerName;
            }
        }
    }

    /// <summary>
    /// This class contains the main part of the filename generation
    /// and pattern parsing and compilation.
    /// </summary>
    internal sealed class SFCPattern : SimplePattern
    {
        private List<SFCPatternSegment> segments = new List<SFCPatternSegment> ();
        private static readonly Regex ParameterMatcher = new Regex (@"(^|[^\\])\{(?<parameter>.+?[^\\])\}", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        internal SFCPattern (string sourcePattern, SimplePatternCompiler owner) : base(sourcePattern, owner)
        {
            segments = ExtractPatternSegments (sourcePattern);
        }

        private static List<SFCPatternSegment> ExtractPatternSegments (string sourcePattern)
        {
            if (sourcePattern == null) {
                throw new PatternCompilationException (Catalog.GetString ("Could not compile a 'null' pattern."));
            }

            var segments = new List<SFCPatternSegment> ();

            // Parse the pattern string and extract all of its segments:
            var matcher = ParameterMatcher.Match (sourcePattern);
            // The index of the character right after the last found parameter:
            int nonParameterIndex = 0;
            while (matcher.Success) {

                var parameter = matcher.Groups [1];
                // Is there some text between the start of this parameter and the last index?
                // parameter.Index is the index of the character right after '{', this is why we look at 'parameter.Index'.
                if (nonParameterIndex < parameter.Index - 1) {
                    // Yes, there is some text. Add it as a segment:
                    segments.Add (new SFCPatternSegment (sourcePattern.Substring (nonParameterIndex, parameter.Index - nonParameterIndex - 1)));
                }
                nonParameterIndex = matcher.Index + matcher.Length;
                // Now add the parameter:
                segments.Add(new SFCPatternParameter(parameter.Value));
                matcher = matcher.NextMatch ();
            }

            if (nonParameterIndex < sourcePattern.Length) {
                segments.Add (new SFCPatternSegment (sourcePattern.Substring (nonParameterIndex)));
            }

            return segments;
        }

        public override void CreateFilename (System.Text.StringBuilder output, DatabaseTrackInfo song, Func<DatabaseTrackInfo, string, string> parameterMap)
        {
            int segmentsCount = segments.Count;
            for (int i = 0; i < segmentsCount; i++) {
                output.Append(segments[i].ToString(song, parameterMap));
            }
        }
    }

    internal class SFCPatternSegment
    {
        public SFCPatternSegment (string content)
        {
            Content = content;
        }

        public virtual string Content { get; protected set; }

        public virtual string ToString (DatabaseTrackInfo song, Func<DatabaseTrackInfo, string, string> parameterMap)
        {
            return Content;
        }

        public override string ToString ()
        {
            return Content;
        }
    }

    internal class SFCPatternParameter : SFCPatternSegment
    {
        public SFCPatternParameter (string content) : base(content)
        {
        }

        public override string ToString (DatabaseTrackInfo song, Func<DatabaseTrackInfo, string, string> parameterMap)
        {
            return parameterMap (song, Content);
        }

        public override string ToString ()
        {
            return "{" + Content + "}";
        }
    }
}

