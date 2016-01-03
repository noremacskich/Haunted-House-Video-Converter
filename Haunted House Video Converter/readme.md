# Haunted House Video Converter

The basic purpose of this app is to help me convert Q-See videos into a readable format for youtube, upload said videos to youtube, and organize them into playlists by channel name and day of the recorded videos.
Eventually I would like to have some sort of display showing how many videos remain, and how many videos have been converted, prefereably by channel.

##Current Status of Project
Currently, all the individual parts are working correctly.  We can sort videos, convert them, create playlists, upload videos and associate said videos to the the playlists.



##Youtube API
I'm using the youtube api for three things : creating playlists, uploading videos to users account, and associating the video to the playlist.

The creation of the playlists and the uploading of the videos in this application are directly based off of the samples provided here : https://github.com/youtube/api-samples

Here are a couple of more links that I used a lot to help me through the youtube api :
 - For setting things up : https://developers.google.com/api-client-library/dotnet/get_started
 - Coding things : https://github.com/youtube/api-samples/tree/master/dotnet

## Todo
Also, to speed up the conversion, i would like to have multiple different conversions going on at once, so have threading of some sort.

## Design Choice Decisions
	If the user has a custom avi that they want to use, that doesn't follow the default naming convnetion of the QSEE system, then it's up to them to handle it.

	The processTheFiles() function will keep track of when a file needs to be processed, don't need to do it anywhere else.


## So we have three directory types :

	pathToOriginal - 
		This is the path to the directory where all the videos were initially stored.
		Files in this immediate directory will be referred to as "Original".
	
	pathToOriginal + "Sorted_Videos" - 
		This is where the videos are moved to in the sorting process.  The videos are moved from the pathToOriginal dirctory to the respective channels in this directory
		Files in this immediate directory will be referred to as "Sorted".

	pathToOriginal + "Converted_Videos" -
		This is where the videos are stored once they have been converted.  This should have an identical structure to the Sorted_Videos directory.  
		Also the individual file size differences should be no greater than 2mb when compared with the original videos.
			If it is, then keep track of it in the lstConvertedFilesToCheck list.
		Files in this immediate directory will be referred to as "Converted"

## Basic outline of the flow ( incomplete )
Basic Flow Chart

 - Get folder with all vids (select path dialog)
 - Create playlists, if not already stored in user settings
   - For each playlist
     - Name it "HH<yy> - Thursday - <Channel Order> - <Channel Name>"
       - <yy> - is the last two digits of the video year
	 - <Channel Order> - is the correct sequence number that the vidoe should be watched in.
	 - <Channel Name> - is the name of the channel specified by the textboxes on the lower half of the form
     - store said name in the "PlaylistNames" user setting
     - Create playlist in youtube account
     - store playlist ID in "PlaylistId" user setting
     
 - Create "Sorted_Videos" folder
   - Create 16 folders inside this one, each named "CH##" eg "CH01", "CH10", "CH16"
   - Store these paths in a list called folder paths
 - Take list of videos
   - For each video
     - seperate out the information in the file name
       - Date, Time, Channel Number
     - move the file to the appropriate folder
     - rename the file to "CH## - dddd - hh-mm-ss.avi" eg "CH01 - Thursday - 9-32-16.avi"
 - Create "Sorted_Videos" folder
   - Create 16 folders inside this one, each named "CH##" eg "CH01", "CH10", "CH16"
   - Store these paths in a list called "sortedPaths"
 - Take list of Sorted Videos
   - For each video
     - convert the video, and move it to the appropriate folder
 - User fills out the names of the channels
 - User hits the upload videos button
   - for each video
     - Give it a title of "\[Channel Name\] - \[Video Date\]"
     - Give it a description of "This is a security camera footage from the Madison Terror Trial's Haunted House of 2015.  Primary purpose of video survalance is for the safety of the scarers, and we want the ability to see what happened if something did.  As a added side benefit, we can also enjoy the scares themselves at a later point."
     - Give it the following tags
       - "Haunted House"
	   - "\[year\]"
	   - "\[day of week\]"
	   - "\[name of channel\]"
	   - "\[room order\]"
	   - "Madison Terror Trial"
     - make it an unlisted video
     - Upload it, update video upload status bar while doing so
     - When finished, associate video with appropriate playlist
     