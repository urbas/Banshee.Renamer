// 
// IRenamingPattern.cs
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
using Banshee.Collection;
using System.Text;
using Banshee.Collection.Database;

namespace Banshee.Renamer
{
    /// <summary>
    /// Implementations of this interface provide the following:
    ///
    ///     1.) Loading (parsing and compiling) a string pattern, which is in a human-readable and human-editable form.
    ///     2.) Creating a string (filename) for a song based on the compiled pattern and actual parameters.
    ///     3.) Methods of this class should be callable from multiple threads.
    ///     4.) They should register themselves in the <see cref="Banshee.Renamer.FilenamePatterns"/> registry (TODO: provide a way for dynamic registration of additional filename creators).
    /// </summary>
    public interface IFilenamePatternCompiler
    {
        /// <summary>
        /// Returns a compiled pattern for the given string. The returned object will
        /// be used in string creation. This method is called seldomly.
        /// </summary>
        /// <exception cref="Banshee.Renamer.PatternCompilationException">thrown if the
        /// compilation failed for any reason.</exception>
        ICompiledFilenamePattern CompilePattern(string pattern, Func<string, Func<DatabaseTrackInfo, object>> parameterMap);

        /// <summary>
        /// Gets the name of this compiler.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a human-readable (ASCII-art) string that describes the usage
        /// of this pattern type.
        /// </summary>
        string Usage { get; }
    }
}

