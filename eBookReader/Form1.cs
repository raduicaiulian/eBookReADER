using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.IO;
using WMPLib;
using Google.Cloud.TextToSpeech.V1;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        static private string fileName;
        static private int isPlaying = 0;
        static private int was_converted=0;
        static private int ia_paused = 0;
        private static WMPLib.WindowsMediaPlayer wplayer;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //TO DO: check if the input has a valid path
            //open file via chose button
            byte[] file;
            OpenFileDialog p = new OpenFileDialog();
            p.ShowDialog();
            this.textBox1.Text = p.FileName;

            if (!(p.FileName.EndsWith(".pdf") || p.FileName.EndsWith(".docx") || p.FileName.EndsWith(".txt")))
                System.Windows.Forms.MessageBox.Show("Please select a pdf, docx or a txt file!");

            if (!p.FileName.EndsWith(".txt")) {
                try
                {
                    file = File.ReadAllBytes(p.FileName);
                    //convert pdf/docx to txt file
                    IPAddress ip = IPAddress.Parse("192.168.1.243");
                    int port = 9999;
                    TcpClient client = new TcpClient();
                    client.Connect(ip, port);
                    Debug.WriteLine("client connected!!");
                    NetworkStream ns = client.GetStream();
                    Thread thread = new Thread(o => SendFile((TcpClient)o, file));//send file trough socket

                    thread.Start(client);

                    fileName = ReadFile(client, p.FileName);

                    client.Client.Shutdown(SocketShutdown.Send);
                    thread.Join();
                    ns.Close();
                    client.Close();
                    Debug.WriteLine("disconnect from server!!");
                }
                catch (SocketException ex) {
                    System.Windows.Forms.MessageBox.Show("Connection to server failed!!!");
                }catch(IOException ex)
                {
                    System.Windows.Forms.MessageBox.Show("Failed to open the file!!!");
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Something unexpected happened, next pop-up contains all known details about this exception!!!");
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
            
            // convert the txt file gained from convert_server.py into a mp3 file
            //read file content as a string
            string text_to_be_read = InternalReadAllText(fileName, Encoding.UTF8);
            this.richTextBox1.Text = text_to_be_read;
            // Instantiate tts client
            TextToSpeechClient ttsclient = TextToSpeechClient.Create();

            // Set the text input to be synthesized.
            SynthesisInput input = new SynthesisInput
            {
                Text = text_to_be_read
            };

            // Build the voice request, select the language code ("en-US"),
            // and the SSML voice gender ("neutral").
            VoiceSelectionParams voice = new VoiceSelectionParams
            {
                LanguageCode = "ro-RO",
                SsmlGender = SsmlVoiceGender.Neutral
            };

            // Select the type of audio file you want returned.
            AudioConfig config = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the Text-to-Speech request, passing the text input
            // with the selected voice parameters and audio file type
            var response = ttsclient.SynthesizeSpeech(new SynthesizeSpeechRequest
            {
                Input = input,
                Voice = voice,
                AudioConfig = config
            });

            // Write the binary AudioContent of the response to an MP3 file.
            fileName = fileName.Remove(fileName.Length - 3);
            using (Stream output = File.Create(fileName + "mp3"))
            {
                response.AudioContent.WriteTo(output);
            }
            was_converted = 1;
            //---------------------------end convertion  to mp3

        }

        private static void PlayTheBook() {
            if(ia_paused == 0){ 
                //play the audio file(mp3)
                wplayer = new WMPLib.WindowsMediaPlayer();

                wplayer.URL = fileName + "mp3";
            }
            ia_paused = 1;
            wplayer.controls.play();
        }

        private static void PauseBokReading()
        {
            wplayer.controls.pause();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {     

        }

        static void SendFile(TcpClient client, byte[] file)
        {
            NetworkStream ns = client.GetStream();

            //write size of file
            ns.Write(BitConverter.GetBytes(file.Length), 0, 4);

            //write file
            ns.Write(file, 0, file.Length);
        }

        static string ReadFile(TcpClient client, string fileName) {
            NetworkStream ns = client.GetStream();

            int n;
            byte[] b = new byte[4];
            ns.Read(b, 0, 4);
            n = BitConverter.ToInt32(b);
            byte[] receivedBytes = new byte[n];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            }

            //rename received file based on old file name
            if (fileName.EndsWith(".docx"))
                fileName=fileName.Remove(fileName.Length - 4);
            else
                fileName=fileName.Remove(fileName.Length - 3);
            fileName += "txt";

            ByteArrayToFile(fileName, receivedBytes);

            return fileName;
        }

        static public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

        private static string InternalReadAllText(string path, Encoding encoding)
        {
            string result;
            using (StreamReader streamReader = new StreamReader(path, encoding))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (was_converted == 1)
            {
                if (isPlaying == 0) { 
                    PlayTheBook();
                    isPlaying = 1;
                }
                else {
                    PauseBokReading();
                    isPlaying = 0;
                }
            }
            else 
            { 
                System.Windows.Forms.MessageBox.Show("First you need to chose apdf/docx/txt file in erder to extract text to be read!");
            }
        }

        //reset button handler
        private void button3_Click(object sender, EventArgs e)
        {
            ia_paused = 0;
            PlayTheBook();
            PauseBokReading();
        }
    }
}
