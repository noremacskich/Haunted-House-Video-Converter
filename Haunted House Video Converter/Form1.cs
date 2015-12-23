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
using System.Text.RegularExpressions;

namespace Haunted_House_Video_Converter
{
    public partial class Form1 : Form
    {
        private ConvertAndMove preppingFilesForUpload = new ConvertAndMove();
        private UploadVideo testUpload = new UploadVideo();

        public Form1()
        {
            InitializeComponent();

            // Do this now, so we don't have to worry about a null pathToOriginal variable.
            getSourceFolderPath();

            // Do this now, so we have a list of channel folders to choose from.
            preppingFilesForUpload.createChannelFolders();

            // This is needed to make sure that the first popup will properly exit out of the whole application.
            preppingFilesForUpload.hasInitialized = true;

            testUpload.videoUpdate += updateProgressBar;
        }

        private void getSourceFolderPath()
        {

            // Say what the dialog is about
            folderBrowserDialog1.Description = "Please select the folder where all the Haunted House videos are.";

            // Get the source path right away
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                // Make sure that the string ends with a \
                preppingFilesForUpload.pathToOriginal = folderBrowserDialog1.SelectedPath + '\\';
            }
            else if (!preppingFilesForUpload.hasInitialized)
            {
                // Close out of the program
                Application.Exit();
            }

            txtLocationOfOriginal.Text = preppingFilesForUpload.pathToOriginal;

        }


        private void btnBrowserDialog_Click(object sender, EventArgs e)
        {
            getSourceFolderPath();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            preppingFilesForUpload.getExistingVidoes();

            preppingFilesForUpload.processTheFiles();
        }

        private void btnMoveDefaultAndRename_Click(object sender, EventArgs e)
        {
            preppingFilesForUpload.moveOriginalFilesToSortedFolders();

            btnMoveDefaultAndRename.BackColor = Color.Green;
        }

        private void btnUploadToYouTube_Click(object sender, EventArgs e)
        {

            // For now, just wanting to see if the uploading will actually work.
            // Need to work on getting the list of files uploaded already

            string thisFile = preppingFilesForUpload.lstFilesToUpload.First();

            testUpload.videoTitle = "Test 456";
            testUpload.videoSource = thisFile;

            lbl_CurrentFileUpload.Text = thisFile;

            FileInfo destinationAttributes = new FileInfo(thisFile);
            testUpload.totalNumberOfBytes = destinationAttributes.Length;

            pgb_UploadStatus.Maximum = 100;

            testUpload.upload();


            // Now that we know that the video is all done, lets keep track of the fact it was done



        }


        private void updateProgressBar(int percentComplete)
        {

            if (this.pgb_UploadStatus.InvokeRequired)
            {
                FileUploadUpdate d = new FileUploadUpdate(updateProgressBar);
                this.Invoke(d, new object[] { percentComplete });
            }
            else
            {
                this.pgb_UploadStatus.Value = percentComplete;
            }

        }

    }
}
