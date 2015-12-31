/*
 * Copyright 2015 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Haunted_House_Video_Converter.Properties;

namespace Haunted_House_Video_Converter
{
    /// <summary>
    /// YouTube Data API v3 sample: create a playlist.
    /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
    /// See https://code.google.com/p/google-api-dotnet-client/wiki/GettingStarted
    /// </summary>
    internal class PlaylistUpdates
    {
        Settings set = Settings.Default;

        /// <summary>This needs to be populated with the list of names before createPlaylists() is called.</summary>
        public List<string> lstPlaylistNames { get; set; }

        [STAThread]
        public async void createPlaylists()
        {
            Console.WriteLine("YouTube Data API: Playlist Updates");
            Console.WriteLine("==================================");

            try
            {
                await Run();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
        }

        private async Task Run()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });


            if (set.PlayListIds.Count != 16) {


                for(int i=0; i<16; i++)
                {


                    // Create a new, private playlist in the authorized user's channel.
                    var newPlaylist = new Playlist();
                    newPlaylist.Snippet = new PlaylistSnippet();
                    newPlaylist.Snippet.Title = lstPlaylistNames[i];
                    newPlaylist.Snippet.Description = "A playlist created with the YouTube API v3";
                    newPlaylist.Status = new PlaylistStatus();
                    newPlaylist.Status.PrivacyStatus = "public";
                    newPlaylist = await youtubeService.Playlists.Insert(newPlaylist, "snippet,status").ExecuteAsync();

                    // Add this ID to the playlist
                    set.PlayListIds.Add(newPlaylist.Id);


                }
                
                
                // either way, we should now have an updated value, so we need to save it.
                set.Save();

            }

        }
    }
}
