Basically this app is meant to help me convert Q-See videos into a readable format for youtube.  It will also help me rename and move the files.


Eventually I would like to have some sort of display showing how many videos remain, and how many videos have been converted, prefereably by channel.

Also, to speed up the conversion, i would like to have multiple different conversions going on at once, so have threading of some sort.

Maybe have a youtube API going on, if it is possible to do so, haven't looked yet.
	Yes it is possible : https://github.com/youtube/api-samples


Design Choice Decisions
	If the user has a custom avi that they want to use, that doesn't follow the default naming convnetion of the QSEE system, then it's up to them to handle it.

	The processTheFiles() function will keep track of when a file needs to be processed, don't need to do it anywhere else.


So we have three directory types :

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


Youtube Stuff :

	Using this as a guidence for setting things up : https://developers.google.com/api-client-library/dotnet/get_started
	Using this as a guidence for coding things : https://github.com/youtube/api-samples/tree/master/dotnet