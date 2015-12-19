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

                string currentChannelPath = pathToOriginal + "Sorted_Videos\\" + "CH" + i.ToString("D2");

                ChannelFolderPaths.Add(currentChannelPath);

                if (!Directory.Exists(currentChannelPath))
                {
                    Directory.CreateDirectory(currentChannelPath);
                }


            }

        }

        private void btnRenameMove_Click(object sender, EventArgs e)
        {

            try
            {
                if (txtLocationOfOriginal.Text == string.Empty)
                {
                    return;
                }

                originalFileNames = Directory.EnumerateFiles(txtLocationOfOriginal.Text, "*.avi").ToList();

                foreach(string file in originalFileNames)
                {
                    Console.WriteLine(file);

                    // Split up the string based on the directory seperator
                    string[] pathSplit = file.Split('\\');

                    foreach (string split in pathSplit)
                    {
                        Console.WriteLine(split);
                    }


                    // Get the last one, as that is the file name.
                    string fileName = pathSplit.Last();

                    // Now need to split this up even further, based on the -
                    string[] fileSplit = fileName.Split('-');
                    foreach(string nameSplit in fileSplit)
                    {
                        Console.WriteLine(nameSplit);
                    }

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
