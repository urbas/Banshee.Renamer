// 
// TemplateStorage.cs
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
    /// Stores user's templates persistently (in Banshee's configuration).
    /// </summary>
    public static class TemplateStorage
    {
        #region Configuration Keys
        private const string StoredTemplateEngineKey = "storedTemplateEngine";
        private const string StoredTemplateKey = "storedTemplate";
        #endregion

        #region Stored Templates Methods (public)
        public static int StoredTemplatesCount {
            get {
                return 0;
            }
        }

        /// <summary>
        /// Returns a list of all templates found stored in the Banshee configuration.
        /// </summary>
        public static List<StoredTemplate> LoadTemplates() {
                List<StoredTemplate> storedTemplates = new List<StoredTemplate>();
                int counter = 1;
                string template;
                string engine;
                while (TryGetTemplate(counter, out template) && TryGetEngine(counter, out engine) && !string.IsNullOrEmpty(engine) && template != null) {
                    if (SongFilenameTemplates.GetTemplateEngine(engine) == null) {
                        foreach (var c in SongFilenameTemplates.KnownTemplateEngines) {
                            SetEngine(counter, engine = c);
                        }
                    }
                    storedTemplates.Add(new StoredTemplate(template, engine));
                    ++counter;
                }
                return storedTemplates;
        }

        /// <summary>
        /// Stores the templates into persistent configuration.
        /// 'null' or invalid stored templates will not be added to the configuration.
        /// </summary>
        public static void StoreTemplates(List<StoredTemplate> templates) {
            int counter = 1;
            for (int i = 0; i < templates.Count; i++) {
                StoredTemplate sp = templates[i];
                if (sp != null && sp.IsValid) {
                    SetTemplate(counter, sp.Template);
                    SetEngine(counter++, sp.Engine);
                }
            }
            RemoveTemplatesFrom(counter);
        }
        #endregion

        #region Helper Methods (private)
        private static void RemoveTemplatesFrom(int index) {
            SetTemplate(index);
            SetEngine(index);
        }

        private static bool TryGetTemplate(int index, out string template) {
            return ConfigurationClient.TryGet(RenamerService.ConfigurationNamespace, GetTemplateKey(index), out template);
        }

        private static bool TryGetEngine(int index, out string engine) {
            return ConfigurationClient.TryGet(RenamerService.ConfigurationNamespace, GetEngineKey(index), out engine);
        }

        private static void SetTemplate(int index, string template = null) {
            ConfigurationClient.Set(RenamerService.ConfigurationNamespace, GetTemplateKey(index), template ?? string.Empty);
        }

        private static void SetEngine(int index, string engine = null) {
            ConfigurationClient.Set(RenamerService.ConfigurationNamespace, GetEngineKey(index), engine ?? string.Empty);
        }

        private static string GetTemplateKey(int index) {
            return StoredTemplateKey + index.ToString();
        }

        private static string GetEngineKey(int index) {
            return StoredTemplateEngineKey + index.ToString();
        }
        #endregion
    }

    public sealed class StoredTemplate {

        public StoredTemplate(string template, string engine) {
            this.Template = template;
            this.Engine = engine;
        }

        public string Template { get; set; }
        public string Engine { get; set; }

        /// <summary>
        /// A stored template is deleted is its engine is null or empty or if
        /// its template is null.
        /// </summary>
        public bool IsDeleted {
            get {
                return string.IsNullOrEmpty(Engine) || Template == null;
            }
        }

        /// <summary>
        /// A stored template is valid if it is not deleted and if a engine
        /// for it actually exists.
        /// </summary>
        public bool IsValid {
            get {
                return !IsDeleted && SongFilenameTemplates.GetTemplateEngine(Engine) != null;
            }
        }
    }
}

