// 
// RenamerWindow.cs
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
using Mono.Unix;
using System.Collections.Generic;
using System.Text;
using Template.Text;

namespace Banshee.Renamer
{
    public partial class RenamerWindow : Gtk.Window
    {
        #region Fields
        private Gtk.NodeStore storedTemplates = new Gtk.NodeStore (typeof(StoredTemplateNode));
        private List<StoredTemplate> sps;
        private List<string> compilers = new List<string> ();
        private int defCompilerIndex = 0;
        private StoredTemplateNode currentPattern;
        private string currentCompiler;
        #endregion

        #region Constructor
        public RenamerWindow () :
             base(Gtk.WindowType.Toplevel)
        {
            this.Build ();

            // Initialise the list of pattern compilers:
            Gtk.ListStore compilersModel = new Gtk.ListStore (typeof(string));
            foreach (var compiler in SongFilenameTemplates.KnownTemplateEngines) {
                compilersModel.AppendValues (compiler);
                compilers.Add (compiler);
            }
            cbCompiler.Model = compilersModel;

            // Set a monotype and small font for the guide:
            tvGuide.ModifyFont (Pango.FontDescription.FromString ("monospace 8"));

            // Load all the stored patterns (from the persistent configuration):
            RefillStoredTemplatesStore ();

            // Initialise the table of stored patterns:
            tableStoredTemplates.NodeStore = storedTemplates;
            tableStoredTemplates.AppendColumn (Catalog.GetString ("Template"), new Gtk.CellRendererText (), "text", 0);
            tableStoredTemplates.AppendColumn (Catalog.GetString ("Type"), new Gtk.CellRendererText (), "text", 1);
            tableStoredTemplates.NodeSelection.Changed += OnSelectionChanged;
            tableStoredTemplates.Selection.Mode = Gtk.SelectionMode.Single;

            // Create a new stored pattern and make it the currently edited one:
            CurrentPattern = GetNewPattern ();

            // Register handlers for all the buttons:
            btnAdd.Clicked += OnPatternAdded;
            btnDelete.Clicked += OnPatternDeleted;
            cbCompiler.Changed += OnCompilerComboBoxChanged;
        }
        #endregion

        #region Properties (private)
        private StoredTemplateNode CurrentPattern {
            get { return currentPattern; }
            set {
                if (currentPattern != value) {
                    if (currentPattern != null) {
                        currentPattern.Changed -= OnCurrentPatternModified;
                    }
                    currentPattern = value;
                    if (currentPattern != null) {
                        currentPattern.Changed += OnCurrentPatternModified;
                    }
                    OnCurrentPatternPropertyChanged ();
                }
            }
        }

        private string CurrentCompiler {
            get { return currentCompiler; }
            set {
                if (!string.Equals (value, currentCompiler)) {
                    currentCompiler = value;
                    OnCurrentCompilerPropertyChanged ();
                }
            }
        }
        #endregion

        #region Event Handlers (private)
        /// <summary>
        /// When the cbCompiler current selection changes...
        /// </summary>
        private void OnCompilerComboBoxChanged (object source, EventArgs eargs)
        {
            UpdateCurrentCompilerFromComboBox ();
        }

        /// <summary>
        /// Invoked when the CurrentCompiler property actually changes.
        /// </summary>
        private void OnCurrentCompilerPropertyChanged ()
        {
            UpdateGuide ();
            if (CurrentPattern != null) {
                CurrentPattern.Engine = CurrentCompiler;
            }
            int idx = compilers.IndexOf (CurrentCompiler);
            if (idx != cbCompiler.Active) {
                cbCompiler.Active = idx;
            }
        }

        /// <summary>
        /// This method is invoked when the StoredTemplateNode fires its changed event.
        /// </summary>
        private void OnCurrentPatternModified (object source, EventArgs eargs)
        {
            if (CurrentPattern != null) {
                CurrentCompiler = CurrentPattern.Engine;
            }

            // Update the text entry with the content of the new pattern.
            UpdatePatternEntry ();

            // Update the currently selected compiler:
            UpdateCompilerComboBox ();

            // Okay, let's try to compile the pattern.
            TryCompileTemplate ();
        }

        /// <summary>
        /// When the property CurrentPattern changes (due to current selection changes or similar).
        /// </summary>
        private void OnCurrentPatternPropertyChanged ()
        {
            // Update the text entry with the content of the new pattern.
            UpdatePatternEntry ();

            // Update the currently selected compiler:
            UpdateCompilerComboBox ();

            TryCompileTemplate ();

            // Focus on the entry field:
            entryPattern.GrabFocus ();
        }

        /// <summary>
        /// The currently selected stored pattern changed...
        /// </summary>
        void OnSelectionChanged (object sender, EventArgs e)
        {
            StoredTemplateNode sn = tableStoredTemplates.NodeSelection.SelectedNode as StoredTemplateNode;
            if (sn != null) {
                CurrentPattern = sn;
            }
        }

        /// <summary>
        /// Raised when the user types something into the entry field.
        /// </summary>
        private void OnPatternChanged (object sender, System.EventArgs e)
        {
            UpdateCurrentPatternFromEntry ();
        }

        /// <summary>
        /// Raised when the Add button is pressed.
        /// </summary>
        private void OnPatternAdded (object sender, System.EventArgs ea)
        {
            StoredTemplateNode sp = GetNewPattern ();
            sps.Add (sp.StoredTemplate);
            storedTemplates.AddNode (sp);
            // Select the newly added one.
            tableStoredTemplates.NodeSelection.SelectPath (new Gtk.TreePath (new int[] { sps.Count - 1 }));
        }

        /// <summary>
        /// Raised when the Delete button is pressed.
        /// </summary>
        private void OnPatternDeleted (object sender, System.EventArgs e)
        {
            StoredTemplateNode sn = tableStoredTemplates.NodeSelection.SelectedNode as StoredTemplateNode;
            if (sn != null) {
                int selectionIndex = sps.IndexOf (sn.StoredTemplate);
                if (selectionIndex >= 0) {
                    sps.RemoveAt (selectionIndex);
                    storedTemplates.RemoveNode (sn);
                    // Select the next one in the list:
                    if (sps.Count > 0) {
                        tableStoredTemplates.NodeSelection.SelectPath (new Gtk.TreePath (new int[] { selectionIndex < sps.Count ? selectionIndex : selectionIndex - 1 }));
                    } else {
                        // There are none in the list. Create a new one:
                        CurrentPattern = GetNewPattern ();
                    }
                }
            }
        }
        #endregion

        #region Overriden Methods
        protected override void OnDestroyed ()
        {
            CommitStoredTemplates ();
            base.OnDestroyed ();
        }
        #endregion

        #region Update Model From UI (private)
        private void UpdateCurrentCompilerFromComboBox ()
        {
            CurrentCompiler = compilers [cbCompiler.Active];
        }

        private void UpdateCurrentPatternFromEntry ()
        {
            if (CurrentPattern == null) {
                CurrentPattern = GetNewPattern ();
            } else if (!string.Equals (CurrentPattern.Template, entryPattern.Text)) {
                CurrentPattern.Template = entryPattern.Text;
            }
        }
        #endregion

        #region UI Update Methods (private)
        private void UpdateGuide ()
        {
            StringBuilder sb = new StringBuilder ();
            sb.Append (Catalog.GetString ("=========== List of parameters ===========\n\n"));
            // TODO: Add the list of parameters:
            var knownParams = TrackInfoParameterMap.Parameters;
            foreach (var param in knownParams) {
                sb.Append ("-   ").Append (param).Append (": ").Append (TrackInfoParameterMap.GetDescription (param)).Append ('\n');
            }

            // Now append the guide for the currently chosen compiler:
            if (CurrentCompiler != null) {
                var compiler = SongFilenameTemplates.GetTemplateEngine (CurrentCompiler);
                if (compiler != null) {
                    sb.Append (Catalog.GetString ("\n=========== Template usage ===========\n\n"));
                    sb.Append (compiler.Usage);
                }
            }
            tvGuide.Buffer.Text = sb.ToString ();
        }

        private void UpdatePatternEntry ()
        {
            if (CurrentPattern == null) {
                entryPattern.Text = string.Empty;
            } else {
                entryPattern.Text = CurrentPattern.Template ?? string.Empty;
            }
        }

        private void UpdateCompilerComboBox ()
        {
            if (currentPattern == null) {
                CurrentCompiler = compilers [defCompilerIndex];
            } else {
                CurrentCompiler = currentPattern.Engine;
            }
        }

        /// <summary>
        /// Loads the list of stored templates from the persistent configuraion
        /// and refreshes the UI list.
        /// </summary>
        private void RefillStoredTemplatesStore ()
        {
            storedTemplates.Clear ();
            sps = TemplateStorage.LoadTemplates ();
            foreach (StoredTemplate sp in sps) {
                if (sp != null && sp.IsValid) {
                    storedTemplates.AddNode (new StoredTemplateNode (sp));
                }
            }
        }
        #endregion

        #region Helper Methods (private)
        /// <summary>
        /// Creates a new stored template, sets its template string and
        /// compiler and returns it without setting it as the currently
        /// edited one or adding it to
        /// the list of stored templates.
        /// </summary>
        private StoredTemplateNode GetNewPattern ()
        {
            int compilerIndex = defCompilerIndex;
            if (cbCompiler.Active >= 0 && cbCompiler.Active < compilers.Count) {
                compilerIndex = cbCompiler.Active;
            }
            return new StoredTemplateNode (new StoredTemplate (entryPattern.Text ?? string.Empty, compilers [compilerIndex]));
        }

        /// <summary>
        /// Commits the current list of stored templates into the  persistent
        /// configuration.
        /// </summary>
        private void CommitStoredTemplates ()
        {
            TemplateStorage.StoreTemplates (sps);
        }

        private void TryCompileTemplate ()
        {
            if (CurrentPattern != null) {
                try {
                    var compiledTemplate = SongFilenameTemplates.CompileTemplate (CurrentPattern.Engine ?? compilers [defCompilerIndex], CurrentPattern.Template);
                    // Print some examples (first five or so):
                    StringBuilder sb = new StringBuilder (Catalog.GetString ("Some filename examples:"));
                    RenamerService.ForAllSongs (song => {return compiledTemplate.CreateString (sb.Append('\n'), song);}, maxSongs: 5);
                    tvMessages.Buffer.Text = sb.ToString();
                } catch (TemplateCompilationException tce) {
                    tvMessages.Buffer.Text = tce.FullMessage;
                }
            }
        }
        #endregion
    }

    [Gtk.TreeNode (ListOnly=true)]
    public class StoredTemplateNode : Gtk.TreeNode
    {

        public readonly StoredTemplate StoredTemplate;

        public StoredTemplateNode (StoredTemplate sp)
        {
            if (sp == null) {
                throw new ArgumentNullException ("sp");
            }
            StoredTemplate = sp;
        }

        [Gtk.TreeNodeValue (Column=0)]
        public string Template {
            get {
                return StoredTemplate.Template ?? string.Empty;
            }
            set {
                if (!string.Equals (StoredTemplate.Template, value)) {
                    StoredTemplate.Template = value;
                    this.OnChanged ();
                }
            }
        }

        [Gtk.TreeNodeValue (Column=1)]
        public string Engine {
            get {
                return StoredTemplate.Engine ?? string.Empty;
            }
            set {
                if (!string.Equals (StoredTemplate.Engine, value)) {
                    StoredTemplate.Engine = value;
                    this.OnChanged ();
                }
            }
        }
    }
}

