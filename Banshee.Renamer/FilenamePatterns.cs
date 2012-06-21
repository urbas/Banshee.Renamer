// 
// FilenameCreatorFactory.cs
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
using System.Collections.Generic;
using Mono.Unix;
using Banshee.Collection.Database;

namespace Banshee.Renamer
{
    /// <summary>
    /// Contains a registry of all known filename creators. Each filename creator
    /// may support a different kind of pattern.
    /// </summary>
    public static class FilenamePatterns
    {
        #region Registry Container
        private static readonly Dictionary<string, IFilenamePatternCompiler> registry = new Dictionary<string, IFilenamePatternCompiler> ();
        #endregion

        #region Registry Initialisation
        static FilenamePatterns ()
        {
            registry.Add (SimplePatternCompiler.CompilerName, new SimplePatternCompiler ());
            registry.Add (DummyPatternCompiler.CompilerName, new DummyPatternCompiler ());
        }
        #endregion

        #region Public Registry Methods
        /// <summary>
        /// Compiles the given pattern using the compiler with the given name.
        /// The returned object can be used to construct filenames for songs
        /// in a multithreaded fashion.
        /// </summary>
        /// <exception cref="PatternCompilationException">if the compilation failed
        /// for any reason. The message of this exception can be displayed to the
        /// user.</exception>
        public static ICompiledFilenamePattern CompilePattern (string compilerName, string pattern)
        {
            var compiler = GetPatternCompiler (compilerName);
            if (compiler == null) {
                throw new PatternCompilationException (string.Format (Catalog.GetString ("Could not compile the pattern '{0}' with the pattern compiler '{1}'."), pattern, compilerName));
            } else {
                return compiler.CompilePattern(pattern, TrackInfoParameterMap.GetParameterMap);
            }
        }
        /// <summary>
        /// Gets a filename pattern compiler for the given name.
        /// </summary>
        /// <returns>
        /// The filename pattern compiler. May be <c>null</c> if no such creator is registered.
        /// </returns>
        /// <param name='name'>
        /// The name of the filename pattern compiler to fetch.
        /// </param>
        public static IFilenamePatternCompiler GetPatternCompiler (string name)
        {
            IFilenamePatternCompiler creator;
            return registry.TryGetValue (name, out creator) ? creator : null;
        }

        /// <summary>
        /// Returns the names of all registered creators.
        /// </summary>
        public static ICollection<string> RegisteredCreatorNames {
            get {
                return registry.Keys;
            }
        }
        #endregion
    }

    internal class DummyPatternCompiler : IFilenamePatternCompiler {
        public const string CompilerName = "Dummy Pattern v1";
        public string Name {
            get {
                return CompilerName;
            }
        }
        public ICompiledFilenamePattern CompilePattern (string pattern, Func<string, Func<DatabaseTrackInfo, object>> parameterMap)
        {
            return new SimplePattern(pattern, this);
        }
        public string Usage {
            get {
                return Catalog.GetString("Patterns of this type produce filenames literally the same as the input pattern.");
            }
        }
    }
}

