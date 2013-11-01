using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Ionic.Zip;

/*Changelog*/

namespace Updater
{
    public partial class Form1 : Form
    {
        String version = "";
        Stopwatch sw;
        WebClient wc;
        XElement xmlServeur;
        String[] arg;
        string currentVersion = "1.0";

        public Form1()
        {
            InitializeComponent();

            wc = new WebClient();
            sw = new Stopwatch();

            string versionUpdate = (string)(xmlServeur = XElement.Load("http://www.konosprod.fr/Updates/updater.xml").Element("version"));

            if (versionUpdate != currentVersion)
            {
                MessageBox.Show("Une nouvelle version de l'updater est disponible, rendez-vous sur http://www.konosprod.fr/");
                Environment.Exit(0);
            }

            if ((arg = Environment.GetCommandLineArgs()).Length != 3)
            {
                MessageBox.Show("Veuillez lancer la mise à jour via le programme que vous voulez mettre à jour !");
                Environment.Exit(0);
            }

            loadVersion();
            getVersionServer();

            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

            string test = (string)xmlServeur.Element("version");

            if (test == version)
            {
                MessageBox.Show("Aucune mise à jour requise.", "Information");
                Environment.Exit(0);
            }
            else
            {
                loadChangeLog();
                update();
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void loadChangeLog()
        {
            richTextBox1.Text += (string)xmlServeur.Element("changelog");        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.IO.File.Delete("maj.zip");
            Environment.Exit(0);
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            labelPerc.Text = e.ProgressPercentage.ToString() + "%";
            labelSpeed.Text = (Convert.ToDouble(e.BytesReceived) / 1024 / sw.Elapsed.TotalSeconds).ToString("0.00") + " kb/s";
            labelDownloaded.Text = (Convert.ToDouble(e.BytesReceived) / 1024 / 1024).ToString("0.00") + " Mo" + "  /  " + (Convert.ToDouble(e.TotalBytesToReceive) / 1024 / 1024).ToString("0.00") + " Mo";
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                System.IO.File.Delete("maj.zip");
                Environment.Exit(0);
            }
            labelSpeed.Text = "";
            sw.Stop();
            extractFiles();
            cancelButton.Enabled = false;
            button1.Enabled = true;
        }

        private void extractFiles()
        {
            labelPerc.Text = "Extracting files...";
            ZipFile zf = ZipFile.Read("maj.zip");
            System.IO.File.Move(arg[2], arg[2] + ".old");

            foreach (ZipEntry ze in zf)
            {
                ze.Extract(".", ExtractExistingFileAction.OverwriteSilently);
            }
            zf.Dispose();
            System.IO.File.Move(arg[1] + ".exe", arg[2]);
            System.IO.File.Delete(arg[2] + ".old");
            File.WriteAllText("version.txt", (string)xmlServeur.Element("version"));
            labelPerc.Text = "Done";
        }

        private void update()
        {
            sw.Start();
            wc.DownloadFileAsync(new Uri((string)xmlServeur.Element("url")), "maj.zip");
        }

        private void loadVersion()
        {
            try
            {
                StreamReader sr = new StreamReader("version.txt");
                version = sr.ReadLine();
                sr.Close();
            }
            catch (Exception e)
            {
                if (e is System.IO.FileNotFoundException)
                {
                    MessageBox.Show("Le fichier version.txt n'existe pas.", "Erreur");
                }

                Environment.Exit(-1);
            }
        }

        private void getVersionServer()
        {
            xmlServeur = XElement.Load("http://www.konosprod.fr/Updates/"+ arg[1] +".xml");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            wc.CancelAsync();
        }
    }
}
