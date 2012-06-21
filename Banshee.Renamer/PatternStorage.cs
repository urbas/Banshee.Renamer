// 
// PatternStorage.cs
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
using Banshee.Configuration;
using System.Collections.Generic;

namespace Banshee.Renamer
{
    /// <summary>
    /// Stores user's patterns persistently (in Banshee's configuration).
    /// </summary>
    public static class PatternStorage
    {
        #region Configuration Keys
        private const string StoredPatternCompilerKey = "storedPatternCompiler";
        private const string StoredPatternKey = "storedPattern";
        #endregion

        #region Stored Patterns Methods (public)
        public static int StoredPatternsCount {
            get {
                return 0;
            }
        }

        /// <summary>
        /// Returns a list of all patterns found stored in the Banshee configuration.
        /// </summary>
        public static List<StoredPattern> LoadPatterns() {
                List<StoredPattern> storedPatterns = new List<StoredPattern>();
                int counter = 1;
                string pattern;
                string compiler;
                while (TryGetPattern(counter, out pattern) && TryGetCompiler(counter, out compiler) && !string.IsNullOrEmpty(compiler) && pattern != null) {
                    if (FilenamePatterns.GetPatternCompiler(compiler) == null) {
                        foreach (var c in FilenamePatterns.RegisteredCreatorNames) {
                            SetCompiler(counter, compiler = c);
                        }
                    }
                    storedPatterns.Add(new StoredPattern(pattern, compiler));
                    ++counter;
                }
                return storedPatterns;
        }

        /// <summary>
        /// Stores the patterns into persistent configuration.
        /// 'null' or invalid stored patterns will not be added to the configuration.
        /// </summary>
        public static void StorePatterns(List<StoredPattern> patterns) {
            int counter = 1;
            for (int i = 0; i < patterns.Count; i++) {
                StoredPattern sp = patterns[i];
                if (sp != null && sp.IsValid) {
                    SetPattern(counter, sp.Pattern);
                    SetCompiler(counter++, sp.Compiler);
                }
            }
            RemovePatternsFrom(counter);
        }
        #endregion

        #region Helper Methods (private)
        private static void RemovePatternsFrom(int index) {
            SetPattern(index);
            SetCompiler(index);
        }

        private static bool TryGetPattern(int index, out string pattern) {
            return ConfigurationClient.TryGet(RenamerService.ConfigurationNamespace, GetPatternKey(index), out pattern);
        }

        private static bool TryGetCompiler(int index, out string compiler) {
            return ConfigurationClient.TryGet(RenamerService.ConfigurationNamespace, GetCompilerKey(index), out compiler);
        }

        private static void SetPattern(int index, string pattern = null) {
            ConfigurationClient.Set(RenamerService.ConfigurationNamespace, GetPatternKey(index), pattern ?? string.Empty);
        }

        private static void SetCompiler(int index, string compiler = null) {
            ConfigurationClient.Set(RenamerService.ConfigurationNamespace, GetCompilerKey(index), compiler ?? string.Empty);
        }

        private static string GetPatternKey(int index) {
            return StoredPatternKey + index.ToString();
        }

        private static string GetCompilerKey(int index) {
            return StoredPatternCompilerKey + index.ToString();
        }
        #endregion
    }

    public sealed class StoredPattern {

        public StoredPattern(string pattern, string compiler) {
            this.Pattern = pattern;
            this.Compiler = compiler;
        }

        public string Pattern { get; set; }
        public string Compiler { get; set; }

        /// <summary>
        /// A stored pattern is deleted is its compiler is null or empty or if
        /// its pattern is null.
        /// </summary>
        public bool IsDeleted {
            get {
                return string.IsNullOrEmpty(Compiler) || Pattern == null;
            }
        }

        /// <summary>
        /// A stored pattern is valid if it is not deleted and if a compiler
        /// for it actually exists.
        /// </summary>
        public bool IsValid {
            get {
                return !IsDeleted && FilenamePatterns.GetPatternCompiler(Compiler) != null;
            }
        }
    }
}

