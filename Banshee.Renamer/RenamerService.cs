// 
// RenamerService.cs
// 
// Author:
//   Matej Urbas <matej.urbas@gmail.com>
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
using Banshee.ServiceStack;
using Banshee.Gui;
using Gtk;
using Banshee.Sources;
using Banshee.Collection.Database;
using Mono.Unix;
using Banshee.IO;
using Banshee.Collection;
using Banshee.Configuration;
using System.Text;
using Hyena;

namespace Banshee.Renamer
{
    public class RenamerService : IExtensionService
    {
        #region Fields
        private ActionGroup menuActions;
        private uint menuActionsUiId;
        #endregion

        #region Constants and Static Fields
        public const string Name = "Renamer";
        /// <summary>
        /// The namespace used by this plugin for all its configuration.
        /// This value may be used in <see cref="ConfigurationClient"/>
        /// </summary>
        internal const string ConfigurationNamespace = "plugins.renamer";
        #endregion

        #region Constructors
        public RenamerService ()
        {
        }
        #endregion

        #region Action Handlers
        /// <summary>
        /// This method is invoked when the user clicks on the `Tools -> Rename...`
        /// menu item.
        /// </summary>
        /// <param name='source'>
        /// The source of the click event (the menu item).
        /// </param>
        /// <param name='args'>
        /// Event arguments...
        /// </param>
        protected virtual void OnOpenMassRenamerAction (object source, EventArgs args)
        {
            Console.WriteLine ("=========== Currently registered services: ===========");
            foreach (var s in ServiceManager.RegisteredServices) {
                Console.WriteLine (s.ServiceName);
            }

            TemplateEngineV1 sfc = new TemplateEngineV1 ();

            try {
                LookupMap<DatabaseTrackInfo> parameterMap = parameter => {
                    switch (parameter) {
                    case "artist":
                        return song => song.DisplayArtistName;
                    case "album":
                        return song => song.DisplayAlbumTitle;
                    case "title":
                        return song => song.DisplayTrackTitle;
                    case "track number":
                        return song => song.TrackNumber > 0 ? (object)song.TrackNumber : null;
                    default:
                        return null;
                    }
                };

                var pattern = sfc.CompileTemplate (@"[CC<{0:00} - {1} - >track number,artist][C<{0:00} - {1} - >track number,artist][artist] - [album] - [title] - [F<0000>track number][FC<0000>track number]", parameterMap);

                Hyena.Log.Information ("================ Traversing songs =================");
                StringBuilder sb = new StringBuilder ();
                ForAllSongs (s => {
                    sb.Clear ();
                    pattern.CreateString (sb, s);
                    Hyena.Log.Information (sb.ToString ());
                    //Hyena.Log.Information(string.Format("Song: {0}", s.Uri.AbsoluteUri));
                },
                s => {
                    Hyena.Log.Information (string.Format ("Not the right type of song: {0}", s.DisplayTrackTitle));
                });
            } catch (TemplateCompilationException pcex) {
                Log.Error (pcex.FullMessage);
            }

            RenamerWindow window = new RenamerWindow ();
            window.ShowAll ();
        }
        #endregion

        #region Song Selection Utility Methods
        /// <summary>
        /// Performs the given action on each song in the current user's selection in the current model.
        /// </summary>
        /// <param name='action'>
        /// The action to be performed on each song in the current user's selection in the current model.
        /// </param>
        public static void ForAllSongs (Action<DatabaseTrackInfo> action, Action<TrackInfo> actionForNonDbTracks = null)
        {
            DatabaseTrackListModel model = null;
            var selection = GetSongsSelection (out model);
            if (selection != null) {
                lock (model) {
                    //int selectionCount = selection.RangeCollection.Count;
                    //int songSelectionIndex = 0;
                    foreach (var songModelIndex in selection.RangeCollection) {
                        var trackInfo = model [songModelIndex] as DatabaseTrackInfo;
                        if (trackInfo == null) {
                            if (actionForNonDbTracks == null) {
                                Hyena.Log.Information (string.Format (Catalog.GetString (@"Skipped the song '{0}' with selection index {1}."), model [songModelIndex].DisplayTrackTitle, songModelIndex));
                            } else {
                                actionForNonDbTracks (model [songModelIndex]);
                            }
                        } else {
                            action (trackInfo);
                        }
                        //++songSelectionIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the database track list model which contains all the songs
        /// listed in the current playlist.
        /// </summary>
        public static DatabaseTrackListModel GetSongsModel ()
        {
            var activeSource = ServiceManager.SourceManager.ActiveSource as DatabaseSource;
            if (activeSource == null) {
                return null;
            } else {
                return activeSource.DatabaseTrackModel;
            }
        }

        /// <summary>
        /// Gets the user's selection of songs together with the model. The model contains the actual track infos while
        /// the selection is a collection of indices of songs in the model that are selected.
        /// </summary>
        /// <param name='model'>
        /// The model from which to extract the user's selection of songs.
        /// </param>
        public static Hyena.Collections.Selection GetSongsSelection (out DatabaseTrackListModel model)
        {
            model = GetSongsModel ();
            return model == null ? null : model.Selection;
        }
        #endregion

        #region Interface Initialisation and Disposal
        /// <summary>
        /// Initialises the interface of Banshee. It inserts the `rename` menu item
        /// in the `Tools` menu.
        /// </summary>
        private void InitializeUi ()
        {
            // Add the `Rename` menu item to the `Tools` menu.
            menuActions = new ActionGroup ("Renamer");

            menuActions.Add (new ActionEntry [] {
                new ActionEntry ("OpenMassRenamerAction", null,
                    Catalog.GetString ("_Rename..."), null,
                    Catalog.GetString ("Rename selected files."), OnOpenMassRenamerAction)
            });

            BansheeActionService.UIManager.InsertActionGroup (menuActions, 0);
            menuActionsUiId = BansheeActionService.UIManager.AddUiFromResource ("RenamerActionsUI.xml");
        }

        /// <summary>
        /// Disposes of whatever the `InitializeUi` method did.
        /// </summary>
        private void DisposeUi ()
        {
            BansheeActionService.UIManager.RemoveUi (menuActionsUiId);
            BansheeActionService.UIManager.RemoveActionGroup (menuActions);
        }
        #endregion

        #region IExtensionService Implementation
        public string ServiceName {
            get {
                return Name;
            }
        }

        void IExtensionService.Initialize ()
        {
            InitializeUi ();
        }

        void IDisposable.Dispose ()
        {
            DisposeUi ();
        }
        #endregion

        #region Static Private Helper Properties
        private static InterfaceActionService BansheeActionService {
            get {
                return ServiceManager.Get<InterfaceActionService> ();
            }
        }
        #endregion
    }
}

