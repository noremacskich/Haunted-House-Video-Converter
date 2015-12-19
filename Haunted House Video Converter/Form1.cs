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
        }

        List<string> originalFileNames = new List<string>();


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
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if(result == DialogResult.OK)
            {
                txtLocationOfOriginal.Text = folderBrowserDialog1.SelectedPath;
            }

        }
    }
}
