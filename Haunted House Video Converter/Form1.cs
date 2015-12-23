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
using Haunted_House_Video_Converter.Properties;

namespace Haunted_House_Video_Converter
{

    public partial class Form1 : Form
    {
        private ConvertAndMove preppingFilesForUpload = new ConvertAndMove();
        private UploadVideo testUpload = new UploadVideo();

        Settings set = Settings.Default;

        /// <summary>This array holds the values from the textboxes</summary>



        public Form1()
        {
            InitializeComponent();

            // Do this now, so we don't have to worry about a null pathToOriginal variable.
            getSourceFolderPath();

            // Do this now, so we have a list of channel folders to choose from.
            preppingFilesForUpload.createChannelFolders();

            // This is needed to make sure that the first popup will properly exit out of the whole application.
            preppingFilesForUpload.hasInitialized = true;

            // Update the progress bar
            testUpload.videoUpdate += updateProgressBar;

            // If this is the first time running this, then initialize the channel names
            if(set.ChannelNames == null) { 
                set.ChannelNames = new System.Collections.Specialized.StringCollection();

                // If there are no ChannelNames yet, lets set the default of "CH##"
                for (int i = 1; i<=16; i++)
                {
                    set.ChannelNames.Add("CH" + i.ToString("D2"));
                }
            }

            // Also check to make sure that the Uploaded videos are intialized as well
            if (set.UploadedVideos == null) set.UploadedVideos = new System.Collections.Specialized.StringCollection();


            // Give all the textboxes their values
            txtCH01.Text = set.ChannelNames[0];
            txtCH02.Text = set.ChannelNames[1];
            txtCH03.Text = set.ChannelNames[2];
            txtCH04.Text = set.ChannelNames[3];
            txtCH05.Text = set.ChannelNames[4];
            txtCH06.Text = set.ChannelNames[5];
            txtCH07.Text = set.ChannelNames[6];
            txtCH08.Text = set.ChannelNames[7];
            txtCH09.Text = set.ChannelNames[8];
            txtCH10.Text = set.ChannelNames[9];
            txtCH11.Text = set.ChannelNames[10];
            txtCH12.Text = set.ChannelNames[11];
            txtCH13.Text = set.ChannelNames[12];
            txtCH14.Text = set.ChannelNames[13];
            txtCH15.Text = set.ChannelNames[14];
            txtCH16.Text = set.ChannelNames[15];

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
            setTextBoxesEnabled(false);
            // For now, just wanting to see if the uploading will actually work.
            // Need to work on getting the list of files uploaded already

            string thisFile = preppingFilesForUpload.lstFilesToUpload.First();

            string channelID = preppingFilesForUpload.getChannelId(thisFile);

            string videoTitle = preppingFilesForUpload.getVideoTitle(thisFile);

            string videoDate = preppingFilesForUpload.getDateTime(thisFile);

            int channelNumber = Int32.Parse(channelID.Substring(1, 3));
            
            testUpload.videoTitle = set.ChannelNames[channelNumber-1] + " - " + videoDate;
            testUpload.videoSource = thisFile;

            lbl_CurrentFileUpload.Text = thisFile;

            FileInfo destinationAttributes = new FileInfo(thisFile);
            testUpload.totalNumberOfBytes = destinationAttributes.Length;

            pgb_UploadStatus.Maximum = 100;

            testUpload.upload();

            // Be sure to denote that the video was successfully uploaded.
            set.UploadedVideos.Add(thisFile);
            set.Save();

            setTextBoxesEnabled(true);
        }

        private void setTextBoxesEnabled(bool enableState)
        {
            txtCH01.Enabled = enableState;
            txtCH02.Enabled = enableState;
            txtCH03.Enabled = enableState;
            txtCH04.Enabled = enableState;
            txtCH05.Enabled = enableState;
            txtCH06.Enabled = enableState;
            txtCH07.Enabled = enableState;
            txtCH08.Enabled = enableState;
            txtCH09.Enabled = enableState;
            txtCH10.Enabled = enableState;
            txtCH11.Enabled = enableState;
            txtCH12.Enabled = enableState;
            txtCH13.Enabled = enableState;
            txtCH14.Enabled = enableState;
            txtCH15.Enabled = enableState;
            txtCH16.Enabled = enableState;
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

        /// <summary>
        /// This relies on the tab order to be set with 0 being channel's 1 textbox.
        /// </summary>
        private void txt_TextUpdate(object sender, EventArgs e)
        {
            TextBox thisText = (TextBox)sender;

            set.ChannelNames[thisText.TabIndex] = thisText.Text;
            set.Save();
        }
    }
}
