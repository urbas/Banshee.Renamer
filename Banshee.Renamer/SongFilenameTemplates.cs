// 
// SongFilenameTemplates.cs
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
using System.Text;

namespace Banshee.Renamer
{
    /// <summary>
    /// Contains a registry of all known filename creators. Each filename creator
    /// may support a different kind of pattern.
    /// </summary>
    public static class SongFilenameTemplates
    {
        #region Registry Container
        private static readonly Dictionary<string, SongFilenameEngine> registry = new Dictionary<string, SongFilenameEngine> ();
        #endregion

        #region Registry Initialisation
        static SongFilenameTemplates ()
        {
            registry.Add (TemplateEngineV1.CompilerName, new TemplateEngineV1 ());
            registry.Add (DummyPatternCompiler.CompilerName, new DummyPatternCompiler ());
        }
        #endregion

        #region Public Registry Methods
        /// <summary>
        /// Compiles the given pattern using the compiler with the given name.
        /// The returned object can be used to construct filenames for songs
        /// in a multithreaded fashion.
        /// </summary>
        /// <exception cref="TemplateCompilationException">if the compilation failed
        /// for any reason. The message of this exception can be displayed to the
        /// user.</exception>
        public static ISongFilenameTemplate CompileTemplate (string compilerName, string pattern)
        {
            var compiler = GetTemplateEngine (compilerName);
            if (compiler == null) {
                throw new TemplateCompilationException (string.Format (Catalog.GetString ("Could not compile the pattern '{0}' with the pattern compiler '{1}'."), pattern, compilerName));
            } else {
                return compiler.CompileTemplate(pattern, TrackInfoParameterMap.GetParameterMap);
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
        public static SongFilenameEngine GetTemplateEngine (string name)
        {
            SongFilenameEngine creator;
            return registry.TryGetValue (name, out creator) ? creator : null;
        }

        /// <summary>
        /// Returns the names of all registered creators.
        /// </summary>
        public static ICollection<string> KnownTemplateEngines {
            get {
                return registry.Keys;
            }
        }
        #endregion
    }

    #region Song Filename Specialised Classes (public)
    /// <summary>
    /// Template engines of this type specialise for the construction of
    /// filenames for songs.
    /// </summary>
    public interface ISongFilenameEngine : ITemplateEngine<DatabaseTrackInfo>
    {
        new ISongFilenameTemplate CompileTemplate(string template, LookupMap<DatabaseTrackInfo> parameterMap);
    }

    /// <summary>
    /// A compiled template that specialises in song filenames.
    /// </summary>
    public interface ISongFilenameTemplate : ICompiledTemplate<DatabaseTrackInfo>
    {
        new ISongFilenameEngine Owner { get; }
    }

    public abstract class SongFilenameEngine : ISongFilenameEngine
    {
        public abstract ISongFilenameTemplate CompileTemplate (string template, LookupMap<DatabaseTrackInfo> parameterMap);

        public abstract string Name { get; }
        public abstract string Usage { get; }

        ICompiledTemplate<DatabaseTrackInfo> ITemplateEngine<DatabaseTrackInfo>.CompileTemplate (string template, LookupMap<DatabaseTrackInfo> parameterMap)
        {
            return this.CompileTemplate(template, parameterMap);
        }
    }

    /// <summary>
    /// A simple implementation of the <see cref="ICompiledTemplate"/>
    /// interface. Intended to be used for providing specific implementations.
    /// </summary>
    public class SongFilenameTemplate : ISongFilenameTemplate
    {
        public SongFilenameTemplate (string templateString, ISongFilenameEngine owner)
        {
            Source = templateString;
            Owner = owner;
        }

        public string Source {
            get;
            private set;
        }

        ITemplateEngine<DatabaseTrackInfo> ICompiledTemplate<DatabaseTrackInfo>.Owner {
            get {
                return this.Owner;
            }
        }

        public ISongFilenameEngine Owner {
            get;
            private set;
        }

        public virtual void CreateString (StringBuilder output, DatabaseTrackInfo song)
        {
            output.Append(Source);
        }
    }
    #endregion

    #region Dummy Template Engine (should be removed soon)
    /// <summary>
    /// Dummy pattern compiler. Should be removed soon.
    /// </summary>
    internal class DummyPatternCompiler : SongFilenameEngine {

        public const string CompilerName = "Dummy Template v1";

        public override string Name {
            get {
                return CompilerName;
            }
        }

        public override ISongFilenameTemplate CompileTemplate (string template, LookupMap<DatabaseTrackInfo> parameterMap)
        {
            return new SongFilenameTemplate(template, this);
        }

        public override string Usage {
            get {
                return Catalog.GetString("Patterns of this type produce filenames literally the same as the input pattern.");
            }
        }
    }
    #endregion
}

