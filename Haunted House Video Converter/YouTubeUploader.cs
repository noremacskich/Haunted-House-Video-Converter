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

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using System.Linq;
using Haunted_House_Video_Converter.Properties;

namespace Haunted_House_Video_Converter
{
    public delegate void FileUploadUpdate(int uploadedBytesSum);
    public delegate void FileUploadFailure(string reason);
    public delegate void UpdateFilePath(string path);
    public delegate void UpdateOverallStatus(string numberVidsRemaining);
    public delegate void UpdateOverallProgress(int percentComplete);

    /// <summary>
    /// YouTube Data API v3 sample: upload a video.
    /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
    /// See https://code.google.com/p/google-api-dotnet-client/wiki/GettingStarted
    /// </summary>
    internal class UploadVideo
    {

        public event FileUploadUpdate videoUpdate;
        public event FileUploadFailure uploadFailure;
        public event UpdateFilePath updatePath;
        public event UpdateOverallStatus updatedOverallStatus;
        public event UpdateOverallProgress updatedOverallProgress;


        Settings set = Settings.Default;


        public long totalNumberOfBytes { get; set; }

        public string videoTitle{ get; set; }

        public string videoSource { get; set; }

        public List<string> lstConvertedFiles { get; set; }

        private YouTubeService youtubeService { get; set; }

        /// <summary>
        /// Returns the list of strings that have yet to be uploaded to youtube.
        /// </summary>
        public List<string> lstFilesToUpload
        {
            get
            {
                return lstConvertedFiles.Except(set.UploadedVideos.Cast<string>().ToList()).ToList();
            }
        }

        /// <summary>
        ///   Get the string "CH01 - 09-31-16 PM - Thursday.avi"
        /// </summary>
        /// <param name="path">The full path to the file in question.</param>

        private string getFileName(string path)
        {
            return path.Split('\\').Last();
        }

        /// <summary>
        ///   Get the string "CH01 - 09-31-16 PM - Thursday"
        /// </summary>
        /// <param name="path">The full path to the file in question.</param>
        public string getVideoTitle(string path)
        {
            // Remove the directory path
            string fullFileName = getFileName(path);

            // Remove the .avi
            return fullFileName.Substring(0, fullFileName.Length - 4); ;

        }

        /// <summary>
        ///   Get the string "CH01"
        /// </summary>
        /// <param name="path">The full path to the file in question.</param>
        public string getChannelId(string path)
        {
            return path.Split('\\').Last().Split('-').First().Trim();
        }

        /// <summary>
        ///   Get the string "09-31-16 PM - Thursday"
        /// </summary>
        /// <param name="path">The full path to the file in question.</param>
        public string getDateTime(string path)
        {
            string fullDateTime = getVideoTitle(path);
            return fullDateTime.Substring(7);
        }

        /// <summary>
        ///   Get the string "Thursday" from "09-31-16 PM - Thursday"
        /// </summary>
        /// <param name="path">The full path to the file in question.</param>
        public string getDayOfWeek(string path)
        {
            return getVideoTitle(path).Split('-').Last().Trim();
        }

        /// <summary>
        ///   Get the string "1" from "CH01"
        /// </summary>
        /// <param name="path">The full path to the file in question.</param>
        public int getChannelNumber(string path)
        {
            string channelID = getChannelId(path);
            string numbersFromString = new String(channelID.Where(x => x >= '0' && x <= '9').ToArray());
            return Int32.Parse(numbersFromString);
        }




        [STAThread]
        public async void upload()
        {


            foreach (string thisFile in lstFilesToUpload)
            {
                // Reset the progress bar
                videoUpdate(0);


                string videoDate = getDateTime(thisFile);

                videoTitle = set.ChannelNames[getChannelNumber(thisFile) - 1] + " - " + videoDate;
                videoSource = thisFile;

                updatePath(thisFile);

                // Get the size of the file being uploaded for the progress bar.
                var fileStream = new FileStream(thisFile, FileMode.Open);
                totalNumberOfBytes = fileStream.Length;
                fileStream.Close();


                int totalUploaded = set.UploadedVideos.Count;

                updatedOverallStatus("A total of " + totalUploaded.ToString() + " videos out of " + set.numberOfVideos + " have been uploaded.");
                updatedOverallProgress((int)(((float)totalUploaded / (float)set.numberOfVideos) * 100));


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


                // Be sure to denote that the video was successfully uploaded, as well as the upload name, we will be needing those
                // later to make sure that we get the videos into the right playlist
                set.UploadedVideos.Add(thisFile);
                set.UploadedVideoNames.Add(videoTitle);
                set.Save();

            }

            Console.WriteLine("YouTube Data API: Upload Video");
            Console.WriteLine("==============================");


            Console.WriteLine("Press any key to continue...");
        }



        private async Task Run()
        {


            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the
                    // authenticated user's YouTube channel, but doesn't allow other types of access.
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }


            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString() //Assembly.GetExecutingAssembly().GetName().Name
            });

            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = videoTitle;
            video.Snippet.Description = "This is a security camera footage from the Madison Terror Trial's Haunted House of " + "2015" + 
                ".  Primary purpose of video survalance is for the safety of the scarers, and we want the ability to see what happened if something did.  " + 
                "As a added side benefit, we can also enjoy the scares themselves at a later point.";
            // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            // to get list, see http://stackoverflow.com/a/28539752/3271665
            video.Snippet.CategoryId = "29"; // Non profits and Activism 

            video.Snippet.Tags = new string[] {
                "Haunted House", // Just a generic Haunted House Tag
                "2015", // The current year
                getDayOfWeek(videoSource), // The day of the week
                set.ChannelNames[getChannelNumber(videoSource) - 1].Split('-').Last().Trim(), // The Name of the channel
                set.ChannelNames[getChannelNumber(videoSource) - 1].Split('-').First().Trim(), // The order of the room
                "Madison Terror Trial"
            };


            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "unlisted"; // or "private" or "public"
            //video.Snippet.Thumbnails.Standard.Url = "http://img.youtube.com/vi/" + video.Id + "/0.jpg";

            using (var fileStream = new FileStream(videoSource, FileMode.Open))
            {
                const int KB = 0x400;
                var minimumChunkSize = 256 * KB;

                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ChunkSize = minimumChunkSize * 5;
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

                await videosInsertRequest.UploadAsync();
            }

            return;

        }

        void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine("{0} bytes sent.", progress.BytesSent);

                    // Get the sum of all the bytes
                    int percentComplete = (int)(((double)progress.BytesSent / (double)totalNumberOfBytes)*100);

                    if (percentComplete > 100) percentComplete = 100;

                    // Update the UI while we are at it.
                    videoUpdate(percentComplete);


                    break;

                case UploadStatus.Failed:
                    Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
                    uploadFailure(progress.Exception.ToString());
                    break;
            }
        }

        async void videosInsertRequest_ResponseReceived(Video video)
        {
            Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);

            // Add a video to the newly created playlist.
            var newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = set.PlayListIds[getChannelNumber(video.Snippet.Title) - 1];
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
            newPlaylistItem.Snippet.ResourceId.VideoId = video.Id;
            newPlaylistItem = await youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync();

        }
    }
}
