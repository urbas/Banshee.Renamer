// 
// IPattern.cs
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
using System.Text;
using Banshee.Collection.Database;

namespace Banshee.Renamer
{
    /// <summary>
    /// Represents a compiled pattern pattern of a filename creator.
    /// Implementations may extend the <see cref="Banshee.Renamer.SimplePattern"/> instead of this
    /// interface directly.
    /// </summary>
    public interface ICompiledFilenamePattern
    {
        /// <summary>
        /// Gets the source string for this pattern (the string that was passed to the
        /// filename creator when this pattern was constructed).
        /// </summary>
        string Source { get; }

        /// <summary>
        /// The filename creator which created this pattern.
        /// </summary>
        IFilenamePatternCompiler Owner { get; }

        /// <summary>
        /// Creates a string using the set pattern, a given object, and a given map. The map returns
        /// a string for the object and the given parameter (the parameters appear in the pattern).
        /// This method will be called a lot of times (and quite possibly from multiple threads).
        /// </summary>
        void CreateFilename(StringBuilder output, DatabaseTrackInfo song);

    }
}

