// 
// TrackInfoParameterMap.cs
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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Banshee.Collection.Database;
using Mono.Unix;
using Template.Text;

namespace Banshee.Renamer
{
    /// <summary>
    /// Contains a library of parameters that can be extracted from a song and
    /// used in a filename template.
    /// </summary>
    public static class TrackInfoParameterMap
    {
        public static string GetDescription(string parameter) {
            return parameterDescriptions[parameter];
        }

        public static ReadOnlyCollection<string> Parameters {
            get {
                return new ReadOnlyCollection<string>(parameters);
            }
        }

        public static Lookup<DatabaseTrackInfo> GetParameterMap(string parameter) {
            Lookup<DatabaseTrackInfo> lookup;
            if (parameterMaps.TryGetValue(parameter, out lookup))
                return lookup;
            return Template.Text.SimpleDataLookup.SimpleLookupMap<DatabaseTrackInfo>(parameter);
        }

        private static List<string> parameters = new List<string>();
        private static Dictionary<string, string> parameterDescriptions = new Dictionary<string, string>();
        private static Dictionary<string, Lookup<DatabaseTrackInfo>> parameterMaps = new Dictionary<string, Lookup<DatabaseTrackInfo>>();

        static TrackInfoParameterMap() {
            // Add all parameters:
            AddParameter("artist", Catalog.GetString("The artist of the song."), s => s.DisplayArtistName);
            AddParameter("title", Catalog.GetString("The song's title."), s => s.DisplayTrackTitle);
            AddParameter("album", Catalog.GetString("The album of the song."), s => s.DisplayAlbumTitle);
            AddParameter("track number", Catalog.GetString("The song's track number (position in the album)."), s => s.TrackNumber > 0 ? (object)s.TrackNumber : null);

            parameters.Sort ();
        }

        private static void AddParameter(string parameter, string description, Lookup<DatabaseTrackInfo> map) {
            parameters.Add(parameter);
            parameterDescriptions.Add(parameter, description);
            parameterMaps.Add(parameter, map);
        }
    }
}

