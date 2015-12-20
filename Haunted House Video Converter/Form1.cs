using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Haunted_House_Video_Converter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Do this now, so we don't have to worry about a null pathToOriginal variable.
            getSourceFolderPath();

            // Do this now, so we have a list of channel folders to choose from.
            createChannelFolders();

            // This is needed to make sure that the first popup will properly exit out of the whole application.
            hasInitialized = true;
        }

        List<string> originalFileNames = new List<string>();

        /// <summary>Contains the list of folders that the vidoes can be sorted into.  At first
        /// it assumes that the camera order is 1-16.</summary>
        List<string> ChannelFolderPaths = new List<string>();

        List<string> lstNewFileNames = new List<string>();


        string pathToOriginal;
        bool hasInitialized = false;

        private void getSourceFolderPath()
        {

            // Say what the dialog is about
            folderBrowserDialog1.Description = "Please select the folder where all the Haunted House videos are.";

            // Get the source path right away
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                // Make sure that the string ends with a \
                pathToOriginal = folderBrowserDialog1.SelectedPath + '\\';
            }
            else if(!hasInitialized)
            {
                // Close out of the program
                Application.Exit();
            }

            txtLocationOfOriginal.Text = pathToOriginal;

        }

        /// <summary>
        /// This will create 16 folders with names of "CH01", "CH02", etc
        /// Then it will move the appropriate files over to their respective folders.
        /// This will not take into consideration the users channel names.
        /// This will also not check to see if the user has already customized the names.
        /// This will also not check to see if the folders have already been filled.
        /// </summary>
        private void createChannelFolders()
        {
            // check to see if we have already sorted the videos, if so, we are done.
            if(!Directory.Exists(pathToOriginal + "Sorted_Videos"))
            {

                Directory.CreateDirectory(pathToOriginal + "Sorted_Videos");

                Console.WriteLine(pathToOriginal + "Sorted_Videos");
                
            }
            
            // Be sure that we have a list of default directories.
            for (int i = 1; i <= 16; i++)
            {

                string currentChannelPath = pathToOriginal + "Sorted_Videos\\" + "CH" + i.ToString("D2") + "\\";

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
        /// </summary>
        private void moveDefaultFilesToFolders()
        {

            try
            {
                // Get the list of files in the directory
                originalFileNames = Directory.EnumerateFiles(pathToOriginal, "*.avi").ToList();

                // Go through each, and get them into the correct directory.
                foreach(string file in originalFileNames)
                {
                    //Console.WriteLine(file);

                    // Get the filename for this path
                    string fileName = file.Split('\\').Last();

                    //Console.WriteLine("File Name " + fileName);

                    // Get the Channel number for this file.  It should be the first thing in the file name
                    string channelNumber = fileName.Split('-').First();
                    //Console.WriteLine("Channel Number " + channelNumber);

                    // Get the actual number from the Channel Number string
                    // Pull out only the numbers from the string using LINQ

                    string numbersFromString = new String(channelNumber.Where(x => x >= '0' && x <= '9').ToArray());

                    int intChannelNumber= Int32.Parse(numbersFromString);

                    // Default file name layout - CH01-2015-10-22-17-51-05.avi
                    
                    // Remove the "CH01-" from the string
                    string fileDate = fileName.Substring(5);

                    // Remove the ".avi" from the string
                    fileDate = fileDate.Substring(0, fileDate.Length - 4);
                    Console.WriteLine(fileDate);

                    DateTime fileDateTime;

                    DateTime.TryParseExact(fileDate, "yyyy-MM-dd-HH-mm-ss", null, System.Globalization.DateTimeStyles.None, out fileDateTime);


                    string newFileName = channelNumber + fileDateTime.ToString(" - hh-mm-ss tt - dddd") + ".avi"; 

                    // move this file to the appropriate directoy in the sorted folder.  Need to have the file name as well
                    // List is a zero based index, so we need to subtract one form the number.
                    Console.WriteLine("Moving File \"" + file + " to " + ChannelFolderPaths[intChannelNumber - 1] + newFileName);

                    Directory.Move(file, ChannelFolderPaths[intChannelNumber - 1] + newFileName);

                    // Keep track of all the files.
                    lstNewFileNames.Add(ChannelFolderPaths[intChannelNumber - 1] + newFileName);

                }

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
        private void getExistingVidoes()
        {
            string channelDirectory = pathToOriginal + "Sorted_Videos\\";

            // Get the list of files in the directory
            lstNewFileNames = Directory.EnumerateFiles(pathToOriginal, "*.avi", SearchOption.AllDirectories).ToList();

            // Debug
            //foreach(string file in lstNewFileNames)
            //{

            //    Console.WriteLine(file);

            //}

        }

        private void convertFiles(string source, string destination)
        {
            source = lstNewFileNames.First();
            destination = pathToOriginal + "TestAVIRun.avi";

            // Prepare the process to run
            ProcessStartInfo start = new ProcessStartInfo();
            // Enter in the command line arguments, everything you would enter after the executable name itself
            start.Arguments = " -i \"" + source + "\" -o \"" + destination + "\" -f 29.97";
            // Enter the executable to run, including the complete path
            start.FileName = "\"" + Directory.GetCurrentDirectory() + "\\avctoavi converter\\avc2avi.exe\"";
            // Do you want to show a console window?
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            int exitCode;

            Console.WriteLine("Waiting for {" + start.FileName + start.Arguments + "} to stop.");

            
            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start))
            {
                Console.WriteLine("Waiting for stop signal");
                proc.WaitForExit();

                // Retrieve the app's exit code
                exitCode = proc.ExitCode;

                if(exitCode != 0)
                {
                    Console.WriteLine("Error : " + proc.StandardError);
                }

                Console.WriteLine("All Done : exit code of " + exitCode.ToString());
            }


        }

        private void btnRenameMove_Click(object sender, EventArgs e)
        {

            moveDefaultFilesToFolders();

        }

        private void btnBrowserDialog_Click(object sender, EventArgs e)
        {
            getSourceFolderPath();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            getExistingVidoes();

            convertFiles(lstNewFileNames.First(), pathToOriginal + "TestAVIRun.avi");
        }
    }
}
