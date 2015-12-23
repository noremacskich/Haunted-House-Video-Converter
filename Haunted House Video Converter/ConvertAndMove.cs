using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace Haunted_House_Video_Converter
{
    class ConvertAndMove
    {

        List<string> originalFileNames = new List<string>();

        /// <summary>Contains the list of folders that the vidoes can be sorted into.  At first
        /// it assumes that the camera order is 1-16.</summary>
        List<string> ChannelFolderPaths = new List<string>();

        List<string> lstNewFileNames = new List<string>();

        /// <summary>
        ///  A list of files that should be checked to make sure that they are the same.
        ///  In order to get into this list, the difference between the source and destination videos
        ///  must be greater than 2mb.
        /// </summary>
        List<string> lstConvertedFilesToCheck = new List<string>();

        /// <summary>This is the path to the directory where all the videos were initially stored.</summary>
        public string pathToOriginal { get; set; }

        /// <summary>
        ///  This is where the videos are moved to in the sorting process.  The videos are moved from the 
        ///  pathToOriginal dirctory to the respective channels in this directory.
        /// </summary>
        private string pathToSortedVideos
        {
            get
            {
                return pathToOriginal + "Sorted_Videos\\";
            }
        }

        /// <summary>
        ///  This is where the videos are stored once they have been converted.  This should have an identical structure to the Sorted_Videos directory.  
        ///  Also the individual file size differences should be no greater than 2mb when compared with the original videos.
        ///  If it is, then keep track of it in the lstConvertedFilesToCheck list.
        /// </summary>
        private string pathToConvertedVideos
        {
            get
            {
                return pathToOriginal + "Converted_Videos\\";
            }
        }

        public List<string> lstFilesToUpload
        {
            get
            {
                // Stub, need to figure out way of figuring out which ones have been uploaded already.
                // Most likely a text file of some sort keeping track of that.
                return Directory.EnumerateFiles(pathToConvertedVideos, "*.avi", SearchOption.AllDirectories).ToList();
            }
       }

        public bool hasInitialized = false;

        private string getFileName(string path)
        {
            return path.Split('\\').Last();
        }



        /// <summary>
        /// This will create 16 folders with names of "CH01", "CH02", etc
        /// Then it will move the appropriate files over to their respective folders.
        /// This will not take into consideration the users channel names.
        /// This will also not check to see if the user has already customized the names.
        /// This will also not check to see if the folders have already been filled.
        /// </summary>
        public void createChannelFolders()
        {
            // check to see if we have already sorted the videos, if so, we are done.
            if (!Directory.Exists(pathToSortedVideos))
            {

                Directory.CreateDirectory(pathToSortedVideos);

                //Console.WriteLine(pathToSortedVideos);

            }

            // Be sure that we have a list of default directories.
            for (int i = 1; i <= 16; i++)
            {

                string currentChannelPath = pathToSortedVideos + "CH" + i.ToString("D2") + "\\";

                ChannelFolderPaths.Add(currentChannelPath);

                if (!Directory.Exists(currentChannelPath))
                {
                    Directory.CreateDirectory(currentChannelPath);
                }


            }
        }

        /// <summary>
        /// This will move the default files to the folders for each camera.  It will also rename the file to somthing similar to : 
        /// "CH01 - HH-MM-SS PM - Thursday"
        /// Aiming to have this start automatically on startup.
        /// </summary>
        public void moveOriginalFilesToSortedFolders()
        {

            // First lets check to see if there are any files that have yet to be moved :
            List<string> lstOriginalFiles = Directory.EnumerateFiles(pathToOriginal, "*.avi").ToList();

            if (lstOriginalFiles.Count() > 0)
            {
                // Lookes like we have some files to move, but first, lets make sure that each of these files are 
                // formatted with the default file names.  Any custom file must be handled by the user manually.

                foreach (string originalFile in lstOriginalFiles)
                {
                    string fileName = getFileName(originalFile);

                    string defaultFileFormat = "CH\\d\\d-\\d\\d\\d\\d-\\d\\d-\\d\\d-\\d\\d-\\d\\d-\\d\\d\\.avi";

                    if (Regex.Match(fileName, defaultFileFormat).Success)
                    {
                        // Move and rename the file to the sorted directory.
                        moveFileToSortedDirectory(originalFile);

                    }

                }

            }

            // Otherwise, just ignore this file.
            
        }

        /// <summary>
        /// Move the given file to the Sorted Directory, while renameing it to the following : "CH01 - HH-MM-SS PM - Thursday"
        /// Keeping track of which files have been converted is the responsibility of the processTheseFiles function.
        /// </summary>
        private void moveFileToSortedDirectory(string pathToFile)
        {
            // Create the Sorted Directory if it doesn't exist
            createChannelFolders();

            try
            {
                // Get the filename for this path
                string fileName = getFileName(pathToFile);

                // Get the Channel number for this file.  It should be the first thing in the file name
                string channelNumber = fileName.Split('-').First();
                //Console.WriteLine("Channel Number " + channelNumber);


                // Get the actual number from the Channel Number string
                // Pull out only the numbers from the string using LINQ

                string numbersFromString = new String(channelNumber.Where(x => x >= '0' && x <= '9').ToArray());
                int intChannelNumber = Int32.Parse(numbersFromString);

                // Default file name layout - CH01-2015-10-22-17-51-05.avi

                // Remove the "CH01-" from the string
                string fileDate = fileName.Substring(5);

                // Remove the ".avi" from the string
                fileDate = fileDate.Substring(0, fileDate.Length - 4);
                //Console.WriteLine(fileDate);

                DateTime fileDateTime;

                DateTime.TryParseExact(fileDate, "yyyy-MM-dd-HH-mm-ss", null, System.Globalization.DateTimeStyles.None, out fileDateTime);

                string newFileName = channelNumber + fileDateTime.ToString(" - hh-mm-ss tt - dddd") + ".avi";

                // move this file to the appropriate directoy in the sorted folder.  Need to have the file name as well
                // List is a zero based index, so we need to subtract one form the number.
                Console.WriteLine("Moving File \"" + pathToFile + " to " + ChannelFolderPaths[intChannelNumber - 1] + newFileName);

                Directory.Move(pathToFile, ChannelFolderPaths[intChannelNumber - 1] + newFileName);

                // Keep track of all the files.
                lstNewFileNames.Add(ChannelFolderPaths[intChannelNumber - 1] + newFileName);

            }
            catch (UnauthorizedAccessException UAEx)
            {
                Console.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                Console.WriteLine(PathEx.Message);
            }

        }

        /// <summary>At this point we know that we have a Sorted_Videos folder.  
        /// We now just need to get the files from it.</summary>
        public void getExistingVidoes()
        {
            string channelDirectory = pathToSortedVideos;

            // Get the list of files in the directory
            lstNewFileNames = Directory.EnumerateFiles(pathToOriginal, "*.avi", SearchOption.AllDirectories).ToList();

            // Debug
            //foreach(string file in lstNewFileNames)
            //{

            //    Console.WriteLine(file);

            //}

        }

        /// <summary>
        ///  Citing my sources : http://stackoverflow.com/a/240610
        /// </summary>
        private void convertFiles(string source, string destination)
        {

            // Make sure that the source exists
            if (!File.Exists(source))
            {
                throw new Exception("The given source \"" + source + "\" doesn't exist.");
            }
            if (File.Exists(destination))
            {
                throw new Exception("The destination file \"" + destination + "\" exists already.");
            }


            // Prepare the process to run
            ProcessStartInfo start = new ProcessStartInfo();

            // Enter in the command line arguments, everything you would enter after the executable name itself
            start.Arguments = " -i \"" + source + "\" -o \"" + destination + "\" -f 29.97";

            // Enter the executable to run, including the complete path
            start.FileName = "\"" + Directory.GetCurrentDirectory() + "\\avctoavi converter\\avc2avi.exe\"";

            // Do you want to show a console window?
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;

            // Cause any errors to be handled locally, not by the operating system
            // Inspiration : http://stackoverflow.com/questions/5210950/how-to-detect-an-application-executed-by-a-process-whether-the-application-stops
            start.RedirectStandardOutput = true;
            start.UseShellExecute = false;

            int exitCode;

            Console.WriteLine("Waiting for {" + start.FileName + start.Arguments + "} to stop.");


            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start))
            {
                Console.WriteLine("Waiting for stop signal");
                proc.WaitForExit();

                // Retrieve the app's exit code
                exitCode = proc.ExitCode;

                if (exitCode != 0)
                {

                    Console.WriteLine("avc2avi.exe did crash, checking to see if file size difference between the source and destination is bigger than 2mb.");

                    // Make sure that the source and destination files are the same size, if so, it was converted successfully
                    FileInfo sourceAttributes = new FileInfo(source);
                    FileInfo destinationAttributes = new FileInfo(destination);

                    long sourceSize = sourceAttributes.Length;
                    long destinationSize = destinationAttributes.Length;

                    // If the difference in size between the two files is greater than 2mb, then note it, otherwise continue on.
                    // Yes 2mb is kinda arbituary, and an educated guess.
                    if (Math.Abs(sourceSize - destinationSize) > 2000000)
                    {
                        Console.WriteLine("Difference between source and destination files was bigger than 2mb.");
                        Console.WriteLine("Affected File : " + source);
                        lstConvertedFilesToCheck.Add(source);
                    }
                    else
                    {
                        Console.WriteLine("Nope, assuming that this was a successful conversion.  Moving on to the next file.");
                    }

                }

                Console.WriteLine("All Done : exit code of " + exitCode.ToString());
            }

        }

        /// <summary>
        /// This will process any files that have yet to be converted over to the youtube friendly version.
        /// It will also create the "Converted_Videos" folder and any sub directories needed for each channel.
        /// </summary>
        public void processTheFiles()
        {

            List<string> lstConvertedFileNames;

            lstNewFileNames = Directory.EnumerateFiles(pathToSortedVideos, "*.avi", SearchOption.AllDirectories).ToList();

            if (!Directory.Exists(pathToConvertedVideos))
            {
                Directory.CreateDirectory(pathToConvertedVideos);
            }

            lstConvertedFileNames = Directory.EnumerateFiles(pathToConvertedVideos, "*.avi", SearchOption.AllDirectories).ToList();


            List<string> convertedFileNames = lstConvertedFileNames.Select(x => x.Split('\\').Last()).ToList();

            List<string> sortedFileNames = lstNewFileNames.Select(x => x.Split('\\').Last()).ToList();



            List<string> filesToConvert = sortedFileNames.Except(convertedFileNames).ToList();


            foreach (string fileName in filesToConvert)
            {

                // Get the Channel number for this file.  It should be the first thing in the file name
                string channelNumber = fileName.Split('-').First().Trim();


                // If channelNumber doesn't exist, create it
                if (!Directory.Exists(pathToConvertedVideos + channelNumber))
                {
                    Directory.CreateDirectory(pathToConvertedVideos + channelNumber);
                }

                convertFiles(pathToSortedVideos + channelNumber + "\\" + fileName, pathToConvertedVideos + channelNumber + "\\" + fileName);
            }


        }

    }
}
