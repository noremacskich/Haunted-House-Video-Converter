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

            string channelID = preppingFilesForUpload.getChannelId(thisFile);

            string videoTitle = preppingFilesForUpload.getVideoTitle(thisFile);

            string RoomIdentification;

            switch (channelID)
            {
                case "CH01" :
                    RoomIdentification = txtCH01.Text;
                    break;
                case "CH02":
                    RoomIdentification = txtCH02.Text;
                    break;
                case "CH03":
                    RoomIdentification = txtCH03.Text;
                    break;
                case "CH04":
                    RoomIdentification = txtCH04.Text;
                    break;
                case "CH05":
                    RoomIdentification = txtCH05.Text;
                    break;
                case "CH06":
                    RoomIdentification = txtCH06.Text;
                    break;
                case "CH07":
                    RoomIdentification = txtCH07.Text;
                    break;
                case "CH08":
                    RoomIdentification = txtCH08.Text;
                    break;
                case "CH09":
                    RoomIdentification = txtCH09.Text;
                    break;
                case "CH10":
                    RoomIdentification = txtCH10.Text;
                    break;
                case "CH11":
                    RoomIdentification = txtCH11.Text;
                    break;
                case "CH12":
                    RoomIdentification = txtCH12.Text;
                    break;
                case "CH13":
                    RoomIdentification = txtCH13.Text;
                    break;
                case "CH14":
                    RoomIdentification = txtCH14.Text;
                    break;
                case "CH15":
                    RoomIdentification = txtCH15.Text;
                    break;
                case "CH16":
                    RoomIdentification = txtCH16.Text;
                    break;
                default:
                    break;
            }

            testUpload.videoTitle = "Test 456";
            testUpload.videoSource = thisFile;

            lbl_CurrentFileUpload.Text = thisFile;

            FileInfo destinationAttributes = new FileInfo(thisFile);
            testUpload.totalNumberOfBytes = destinationAttributes.Length;

            pgb_UploadStatus.Maximum = 100;

            testUpload.upload();

        }

        // https://msdn.microsoft.com/en-us/library/ms171728(v=vs.110).aspx
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
