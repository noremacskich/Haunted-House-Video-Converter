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

namespace Haunted_House_Video_Converter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            getSourceFolderPath();

        }

        List<string> originalFileNames = new List<string>();

        List<string> ChannelFolderPaths = new List<string>();


        string pathToOriginal;

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

        private void moveDefaultFilesToFolders()
        {



        }

        private void btnRenameMove_Click(object sender, EventArgs e)
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
                    

                    // move this file to the appropriate directoy in the sorted folder.  Need to have the file name as well
                    // List is a zero based index, so we need to subtract one form the number.
                    Console.WriteLine("Moving File \"" + file + " to " + ChannelFolderPaths[intChannelNumber - 1] + fileName);
                    //Directory.Move(file, );

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

        private void btnBrowserDialog_Click(object sender, EventArgs e)
        {
            getSourceFolderPath();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            createChannelFolders();
        }
    }
}
