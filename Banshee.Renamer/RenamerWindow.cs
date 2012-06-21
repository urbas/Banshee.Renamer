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

namespace Banshee.Renamer
{
    public partial class RenamerWindow : Gtk.Window
    {
        #region Fields
        private Gtk.NodeStore storedPatterns = new Gtk.NodeStore (typeof(StoredPatternNode));
        private List<StoredPattern> sps;
        private List<string> compilers = new List<string> ();
        private int defCompilerIndex = 0;
        private StoredPatternNode currentPattern;
        #endregion

        #region Constructor
        public RenamerWindow () :
             base(Gtk.WindowType.Toplevel)
        {
            this.Build ();

            // Initialise the list of pattern compilers:
            Gtk.ListStore compilersModel = new Gtk.ListStore (typeof(string));
            foreach (var compiler in FilenamePatterns.RegisteredCreatorNames) {
                compilersModel.AppendValues (compiler);
                compilers.Add (compiler);
            }
            cbCompiler.Model = compilersModel;

            // Load all the stored patterns (from the persistent configuration):
            RefillStoredPatternsStore ();

            // Initialise the table of stored patterns:
            tableStoredPatterns.NodeStore = storedPatterns;
            tableStoredPatterns.AppendColumn (Catalog.GetString ("Pattern"), new Gtk.CellRendererText (), "text", 0);
            tableStoredPatterns.AppendColumn (Catalog.GetString ("Type"), new Gtk.CellRendererText (), "text", 1);
            tableStoredPatterns.NodeSelection.Changed += OnSelectionChanged;
            tableStoredPatterns.Selection.Mode = Gtk.SelectionMode.Single;

            // Create a new stored pattern and make it the currently edited one:
            CurrentPattern = GetNewPattern ();

            // Register handlers for all the buttons:
            btnAdd.Clicked += OnPatternAdded;
            btnDelete.Clicked += OnPatternDeleted;
            cbCompiler.Changed += OnCompilerChanged;
        }
        #endregion

        #region Properties (private)
        private StoredPatternNode CurrentPattern {
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
                    OnCurrentPatternChanged ();
                }
            }
        }
        #endregion

        #region Event Handlers (private)
        /// <summary>
        /// When the cbCompiler current selection changes...
        /// </summary>
        private void OnCompilerChanged (object source, EventArgs eargs)
        {
            UpdatePatternCompilerFromComboBox ();
        }

        /// <summary>
        /// This method is invoked when the StoredPatternNode fires its changed event.
        /// </summary>
        private void OnCurrentPatternModified (object source, EventArgs eargs)
        {
            // This can be called only when the user has changed the text entry, therefore don't update the text entry.
            // Update the sample created filename:
            // TODO:
        }

        /// <summary>
        /// When the property CurrentPattern changes (due to current selection changes or similar).
        /// </summary>
        private void OnCurrentPatternChanged ()
        {
            // Update the text entry with the content of the new pattern.
            if (currentPattern == null) {
                entryPattern.Text = string.Empty;
            } else {
                entryPattern.Text = currentPattern.Pattern ?? string.Empty;
            }

            // Update the currently selected compiler:
            if (currentPattern == null) {
                cbCompiler.Active = defCompilerIndex;
            } else {
                cbCompiler.Active = compilers.IndexOf (currentPattern.Compiler);
            }

            // Focus on the entry field:
            entryPattern.GrabFocus();
        }

        /// <summary>
        /// The currently selected stored pattern changed...
        /// </summary>
        void OnSelectionChanged (object sender, EventArgs e)
        {
            StoredPatternNode sn = tableStoredPatterns.NodeSelection.SelectedNode as StoredPatternNode;
            if (sn != null) {
                CurrentPattern = sn;
            }
        }

        /// <summary>
        /// Raised when the user types something into the entry field.
        /// </summary>
        private void OnPatternChanged (object sender, System.EventArgs e)
        {
            UpdatePatternFromEntry ();
        }

        /// <summary>
        /// Raised when the Add button is pressed.
        /// </summary>
        private void OnPatternAdded (object sender, System.EventArgs ea)
        {
            StoredPattern sp = new StoredPattern (entryPattern.Text, compilers [cbCompiler.Active]);
            sps.Add (sp);
            storedPatterns.AddNode (new StoredPatternNode (sp));
            // Select the newly added one.
            tableStoredPatterns.NodeSelection.SelectPath (new Gtk.TreePath (new int[] { sps.Count - 1 }));
        }

        /// <summary>
        /// Raised when the Delete button is pressed.
        /// </summary>
        private void OnPatternDeleted (object sender, System.EventArgs e)
        {
            StoredPatternNode sn = tableStoredPatterns.NodeSelection.SelectedNode as StoredPatternNode;
            if (sn != null) {
                int selectionIndex = sps.IndexOf (sn.StoredPattern);
                if (selectionIndex >= 0) {
                    sps.RemoveAt (selectionIndex);
                    storedPatterns.RemoveNode (sn);
                    // Select the next one in the list:
                    if (sps.Count > 0) {
                        tableStoredPatterns.NodeSelection.SelectPath (new Gtk.TreePath (new int[] { selectionIndex < sps.Count ? selectionIndex : selectionIndex - 1 }));
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
            StoreStoredPatterns ();
            base.OnDestroyed ();
        }
        #endregion

        #region Update Model From UI (private)
        private void UpdatePatternCompilerFromComboBox ()
        {
            if (CurrentPattern != null && !string.Equals (CurrentPattern.Compiler, compilers [cbCompiler.Active])) {
                CurrentPattern.Compiler = compilers [cbCompiler.Active];
            }
        }

        private void UpdatePatternFromEntry ()
        {
            if (CurrentPattern != null && !string.Equals (CurrentPattern.Pattern, entryPattern.Text)) {
                CurrentPattern.Pattern = entryPattern.Text;
            }
        }
        #endregion

        #region Helper Methods (private)
        private StoredPatternNode GetNewPattern ()
        {
            int compilerIndex = defCompilerIndex;
            if (cbCompiler.Active >= 0 && cbCompiler.Active < compilers.Count) {
                compilerIndex = cbCompiler.Active;
            }
            return new StoredPatternNode (new StoredPattern (string.Empty, compilers [compilerIndex]));
        }

        private void RefillStoredPatternsStore ()
        {
            storedPatterns.Clear ();
            sps = PatternStorage.LoadPatterns ();
            foreach (StoredPattern sp in sps) {
                if (sp != null && sp.IsValid) {
                    storedPatterns.AddNode (new StoredPatternNode (sp));
                }
            }
        }

        private void StoreStoredPatterns ()
        {
            PatternStorage.StorePatterns (sps);
        }
        #endregion
    }

    [Gtk.TreeNode (ListOnly=true)]
    public class StoredPatternNode : Gtk.TreeNode
    {

        public readonly StoredPattern StoredPattern;

        public StoredPatternNode (StoredPattern sp)
        {
            StoredPattern = sp;
        }

        [Gtk.TreeNodeValue (Column=0)]
        public string Pattern {
            get {
                return StoredPattern.Pattern ?? string.Empty;
            }
            set {
                StoredPattern.Pattern = value;
                this.OnChanged ();
            }
        }

        [Gtk.TreeNodeValue (Column=1)]
        public string Compiler {
            get {
                return StoredPattern.Compiler ?? string.Empty;
            }
            set {
                StoredPattern.Compiler = value;
                this.OnChanged ();
            }
        }
    }
}

