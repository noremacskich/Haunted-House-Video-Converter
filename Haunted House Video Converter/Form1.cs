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

using System.Threading;
using System.Threading.Tasks;


namespace Haunted_House_Video_Converter
{

    public partial class Form1 : Form
    {
        private ConvertAndMove preppingFilesForUpload = new ConvertAndMove();
        private UploadVideo testUpload = new UploadVideo();
        private PlaylistUpdates thesePlaylists = new PlaylistUpdates();

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

            testUpload.uploadFailure += uploadFailure;
            testUpload.updatePath += updateCurrentPath;
            testUpload.updatedOverallStatus += updateFilesStatus;
            testUpload.updatedOverallProgress += UpdateOverallProgressBar;
            // set max on progress bar
            pgb_UploadStatus.Maximum = 100;


            // If this is the first time running this, then initialize the channel names
            if (set.ChannelNames == null) { 
                set.ChannelNames = new System.Collections.Specialized.StringCollection();

                // If there are no ChannelNames yet, lets set the default of "CH##"
                for (int i = 1; i<=16; i++)
                {
                    set.ChannelNames.Add("CH" + i.ToString("D2"));
                }
            }

            // Also check to make sure that the Uploaded videos are intialized as well
            if (set.UploadedVideos == null) set.UploadedVideos = new System.Collections.Specialized.StringCollection();
            if (set.UploadedVideoNames == null) set.UploadedVideoNames = new System.Collections.Specialized.StringCollection();
            if (set.PlayListIds == null) set.PlayListIds = new System.Collections.Specialized.StringCollection();

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

            // Get the information needed to display the upload count
            testUpload.lstConvertedFiles = preppingFilesForUpload.lstConvertedFiles;
            int totalNumberToUpload = testUpload.lstFilesToUpload.Count();
            int totalUploaded = set.UploadedVideos.Count;
            updateFilesStatus("A total of " + totalUploaded.ToString() + " videos out of " + totalNumberToUpload + " have been uploaded.");


            // Get the playlists
            List<string> lstPlaylistNames = new List<string>();

            string dayOfWeek = testUpload.getDayOfWeek(preppingFilesForUpload.lstConvertedFiles.First());

            foreach (string name in set.ChannelNames)
            {
                lstPlaylistNames.Add("HH" + DateTime.Now.ToString("yy") + " - " + dayOfWeek + " - " + name);
            }

            // Be sure to pass over the names of the playlists
            thesePlaylists.lstPlaylistNames = lstPlaylistNames;

            thesePlaylists.createPlaylists();


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

            testUpload.lstConvertedFiles = preppingFilesForUpload.lstConvertedFiles;

            testUpload.upload();

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

        // https://msdn.microsoft.com/en-us/library/ms171728(v=vs.110).aspx
        private void updateCurrentPath(string path)
        {

            if (this.lbl_CurrentFileUpload.InvokeRequired)
            {
                UpdateFilePath d = new UpdateFilePath(updateCurrentPath);
                this.Invoke(d, new object[] { path });
            }
            else
            {
                this.lbl_CurrentFileUpload.Text = path;
            }

        }

        // https://msdn.microsoft.com/en-us/library/ms171728(v=vs.110).aspx
        private void updateFilesStatus(string path)
        {

            if (this.lblOveralProgress.InvokeRequired)
            {
                UpdateOverallStatus d = new UpdateOverallStatus(updateFilesStatus);
                this.Invoke(d, new object[] { path });
            }
            else
            {
                this.lblOveralProgress.Text = path;
            }

        }

        // https://msdn.microsoft.com/en-us/library/ms171728(v=vs.110).aspx
        private void UpdateOverallProgressBar(int percentComplete)
        {

            if (this.pgbOverallPercent.InvokeRequired)
            {
                UpdateOverallProgress d = new UpdateOverallProgress(UpdateOverallProgressBar);
                this.Invoke(d, new object[] { percentComplete });
            }
            else
            {
                this.pgbOverallPercent.Value = percentComplete;
            }

        }


        public void uploadFailure(string reason)
        {
            MessageBox.Show("Upload a Failure for this reason:" + reason);
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
        private void btnClearUploadList_Click(object sender, EventArgs e)
        {
            set.UploadedVideos.Clear();
            set.UploadedVideoNames.Clear();
            set.Save();

            // Update the total uploaded string
            int totalNumberToUpload = testUpload.lstFilesToUpload.Count();
            int totalUploaded = set.UploadedVideos.Count;
            updateFilesStatus("A total of " + totalUploaded.ToString() + " videos out of " + totalNumberToUpload + " have been uploaded.");

        }

    }
}
