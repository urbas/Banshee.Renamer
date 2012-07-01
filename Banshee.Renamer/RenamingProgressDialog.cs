// 
// RenamingProgressDialog.cs
// 
// Author:
//   matej <${AuthorEmail}>
// 
// Copyright 2012 matej
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
using System.Threading.Tasks;
using Banshee.Collection.Database;
using Template.Text;
using Hyena;
using Mono.Unix;
using System.Text;
using Banshee.IO;

namespace Banshee.Renamer
{
    /// <summary>
    /// Starts renaming the currently selected files and shows the progress
    /// as it goes along.
    /// </summary>
    public partial class RenamingProgressDialog : Gtk.Dialog
    {
        public ICompiledTemplate<DatabaseTrackInfo> template;
        public Task renamerTask;
        public volatile bool cancel = false;

        public RenamingProgressDialog (ICompiledTemplate<DatabaseTrackInfo> template)
        {
            if (template == null) {
                throw new ArgumentNullException ("template");
            }
            this.template = template;
            this.Build ();
            buttonCancel.Clicked += OnButtonCancelClicked;
            buttonOk.Clicked += OnButtonOkClicked;
        }

        private void OnButtonOkClicked (object sender, EventArgs e)
        {
            renamerTask.Wait ();
            Destroy ();
        }

        private void OnButtonCancelClicked (object sender, EventArgs e)
        {
            cancel = true;
            renamerTask.Wait ();
//            Destroy ();
        }

        protected override void OnDestroyed ()
        {
            cancel = true;
            renamerTask.Wait ();
            base.OnDestroyed ();
        }

        protected override void OnRealized ()
        {
            base.OnRealized ();
            renamerTask = Task.Factory.StartNew (() => {
                try {
                    RenameSongs ();
                } catch (Exception ex) {
                    AppendMessage ("Error while renaming files: " + ex.ToString ());
                } finally {
                    // Finished:
                    Gtk.Application.Invoke ((s, a) => {
                        buttonOk.Sensitive = true;
                        buttonCancel.Sensitive = false;
                    });
                }
            });
        }

        private void AppendMessage(string message) {
            Gtk.Application.Invoke ((s, a) => {
                var atEnd = tvMessages.Buffer.EndIter;
                tvMessages.Buffer.Insert (ref atEnd, message);
                expndrMessages.Expanded = true;
            });
        }

        private void RenameSongs ()
        {
            if (template == null) {
                Log.Error ("Will not perform renaming. No filename template found.");
                return;
            }
            DatabaseTrackListModel model = null;
            var selection = RenamerService.GetSongsSelection (out model);
            if (selection != null) {
//                lock (model) {
                int songCounter = 0;
                int totalSongs = selection.Count;
                StringBuilder sb = new StringBuilder ();
                foreach (var songModelIndex in selection.RangeCollection) {
                    if (cancel) {
                        break;
                    }
                    var trackInfo = model [songModelIndex] as DatabaseTrackInfo;
                    if (trackInfo == null) {
                        AppendMessage(string.Format (Catalog.GetString (@"Skipped the song '{0}' with selection index {1}."), model [songModelIndex].DisplayTrackTitle, songModelIndex));
                        ++songCounter;
                    } else {
                        sb.Clear ();
                        template.CreateString (sb, trackInfo);
                        string newFilename = sb.ToString ();
                        ++songCounter;
                        Gtk.Application.Invoke ((s, a) => {
                            pbRenaming.Text = string.Format (Catalog.GetString ("{0}/{1}"), songCounter, totalSongs);
                            pbRenaming.Fraction = (double)songCounter / (double)totalSongs;
                            lblOriginalFilename.Text = trackInfo.Uri.AbsolutePath;
                            lblNewFilename.Text = newFilename;
                        });
                        try {
                            Directory.Create(System.IO.Path.GetDirectoryName(newFilename));
                            File.Move (trackInfo.Uri, new SafeUri (newFilename));
                            try {
                                Directory.Delete(System.IO.Path.GetDirectoryName(trackInfo.LocalPath));
                            } catch (Exception) {}
                        } catch (Exception ex) {
                            AppendMessage (string.Format (Catalog.GetString ("Could not rename song '{0}' to '{1}'. Error message: {2}\n"), trackInfo.Uri.AbsolutePath, newFilename, ex.Message));
                        }
                    }
                }
//                }
            }
        }
    }
}

