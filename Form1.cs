using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Media;
using Payloads;
using System.IO;
using System.Threading;
using System.Net;
using NAudio;
using NAudio.Wave;
using System.Windows.Media.Imaging;

namespace RemotePresentationManager
{
    public partial class Form1 : Form
    {
        SerialPort Port;
        bool Connected = false;
        SoundPlayer Player;
        WaveOut WaveOutDevice = new WaveOut();
        AudioFileReader AudioFileReader;
        CoreAudioDevice DefaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
        int window = 0;//Window mode. used to avoid Invoke()
        string clip = "";//Clipboard text
        bool pass = false;//Password protection
        int remaining = 5;//remaining tries
        string title = "";//title of all messageboxes
        bool loop = false;//audio loop
        public Form1(string[] args)
        {
            InitializeComponent();
            WaveOutDevice.PlaybackStopped += (s, e) =>
            {
                if (loop && e.Exception == null)
                {
                    WaveOutDevice.Play();
                }
            };
            if (args.Length > 0)
            {
                window = 1;
                Port = new SerialPort(args[0], 9600, Parity.None, 8, StopBits.One);
                Port.Open();
                Port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
                if (Port.IsOpen)
                {
                    Port.Write("Insert the password");
                    button1.Enabled = false;
                    listBox1.Items.Clear();
                    listBox1.Items.Add("Status: " + Port.PortName + " online");
                    Connected = true;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (string s in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(s);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            try
            {
                Port = new SerialPort(comboBox1.SelectedItem.ToString(), 9600, Parity.None, 8, StopBits.One);
                Port.Open();
                Port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
                if (Port.IsOpen)
                {
                    Port.Write("Insert the password");
                    button1.Enabled = false;
                    listBox1.Items.Clear();
                    listBox1.Items.Add("Status: " + Port.PortName + " online");
                    Connected = true;
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Connected)
            {
                //MessageBox.Show("You cannot change the current Port. Restart the program");
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                CheckFunctions(Port.ReadExisting());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Port.Write(ex.GetType() + ": " + ex.Message);
            }
        }

        private void CheckFunctions(string data)
        {
            if (pass)
            {
                if (data.Equals("AT"))
                {
                    Console.WriteLine("Sending current status...");
                    Port.Write("Online!");
                    Port.Write(Port.NewLine + "PORT INFO: Current Port: " + Port.PortName + Port.NewLine + "Baud rate: " + Port.BaudRate + Port.NewLine + "Using RPM " + Application.ProductVersion + " By Adryzz\n(Adryzz#7264)");

                }
                else if (data.Equals("HELP"))
                {
                    Help();
                }
                else if (data.Equals("SHOW"))
                {
                    Port.Write("Command received!");
                    window = 2;
                    Console.WriteLine("Show window");
                }
                else if (data.Contains("PLAYSOUND "))
                {
                    Port.Write("Command received!");
                    PlaySound(data);
                    Console.WriteLine("Play sound file");
                }
                else if (data.Contains("PLAYURL "))
                {
                    Port.Write("Command received!");
                    PlayMp3FromUrl(data);
                    Console.WriteLine("Play url");
                }
                else if (data.Contains("PLAY "))
                {
                    Port.Write("Command received!");
                    Play(data);
                    Console.WriteLine("Play sound");
                }
                else if (data.Contains("URLIMAGE "))
                {
                    Port.Write("Command received!");
                    UrlImage(data);
                    Console.WriteLine("Url image");
                }
                else if (data.Equals("MUTE"))
                {
                    Port.Write("Command received!");
                    Mute();
                    Console.WriteLine("Mute");
                }
                else if (data.Equals("STOP"))
                {
                    Port.Write("Command received!");
                    Stop();
                    Console.WriteLine("Stop sound file player");
                }
                else if (data.Equals("PAUSE"))
                {
                    Port.Write("Command received!");
                    Pause();
                    Console.WriteLine("Pause sound file player");
                }
                else if (data.Equals("RESUME"))
                {
                    Port.Write("Command received!");
                    Resume();
                    Console.WriteLine("Resume sound file player");
                }
                else if (data.Equals("UNMUTE"))
                {
                    Port.Write("Command received!");
                    UnMute();
                    Console.WriteLine("Unmute");
                }
                else if (data.Contains("VOLUME "))
                {
                    Port.Write("Command received!");
                    AdjustVolume(data);
                    Console.WriteLine("Adjust volume");
                }
                else if (data.Contains("CLIP "))
                {
                    Port.Write("Command received!");
                    ClipBoard(data);
                    Console.WriteLine("Edit clipboard");
                }
                else if (data.Contains("KEY "))
                {
                    Port.Write("Command received!");
                    Key(data);
                    Console.WriteLine("Send custom key");
                }
                else if (data.Contains("LOOP "))
                {
                    Port.Write("Command received!");
                    Loop(data);
                    Console.WriteLine("Set loop mode");
                }
                else if (data.Contains("SAY "))
                {
                    Port.Write("Command received!");
                    Say(data);
                    Console.WriteLine("Say text");
                }
                else if (data.Contains("IMG "))
                {
                    Port.Write("Command received!");
                    Draw(data);
                    Console.WriteLine("Draw bitmap");
                }
                else if (data.Contains("ROTATE "))
                {
                    Port.Write("Command received!");
                    Rotate(data);
                    Console.WriteLine("Rotate screen");
                }
                else if (data.Contains("QMSG "))
                {
                    Port.Write("Command received!");
                    QMsg(data);
                    Console.WriteLine("Question MessageBox");
                }
                else if (data.Contains("MSG "))
                {
                    Port.Write("Command received!");
                    Msg(data);
                    Console.WriteLine("MessageBox");
                }
                else if (data.Contains("TITLE "))
                {
                    Port.Write("Command received!");
                    Title(data);
                    Console.WriteLine("Set Title");
                }
                else if (data.Equals("MSGLOOP"))
                {
                    Port.Write("Command received!");
                    MsgLoop();
                    Console.WriteLine("Loop MessageBox");
                }
                else if (data.Equals("HOOK"))
                {
                    Port.Write("Command received!");
                    Hook();
                    Console.WriteLine("Hook keys");
                }
                else if (data.Equals("UNHOOK"))
                {
                    Port.Write("Command received!");
                    UnHook();
                    Console.WriteLine("Unhook keys");
                }
                else if (data.Equals("HIDE"))
                {
                    Port.Write("Command received!");
                    window = 1;
                    Console.WriteLine("Hide window");
                }
                else if (data.Equals("CLOSE"))
                {
                    Port.Write("Command received! Exiting...");
                    window = 3;
                    Console.WriteLine("Exit");
                }
                else if (data.Equals("ESC"))
                {
                    Port.Write("Command received!");
                    Esc();
                    Console.WriteLine("ESC key");
                }
                else if (data.Equals("F5"))
                {
                    Port.Write("Command received!");
                    F5();
                    Console.WriteLine("F5 key");
                }
                else if (data.Equals("LEFT"))
                {
                    Port.Write("Command received!");
                    LeftArrow();
                    Console.WriteLine("LEFT ARROW key");
                }
                else if (data.Equals("RIGHT"))
                {
                    Port.Write("Command received!");
                    RightArrow();
                    Console.WriteLine("RIGHT ARROW key");
                }
                else if (data.Equals("UP"))
                {
                    Port.Write("Command received!");
                    UpArrow();
                    Console.WriteLine("UP ARROW key");
                }
                else if (data.Equals("DOWN"))
                {
                    Port.Write("Command received!");
                    DownArrow();
                    Console.WriteLine("Down ARROW key");
                }
                else if (data.Equals("ALTF4"))
                {
                    Port.Write("Command received!");
                    Altf4();
                    Console.WriteLine("ALT+F4 KEYSTROKE");
                }
                else if (data.Equals("SHUTDOWN"))
                {
                    Port.Write("Command received!");
                    Shutdown();
                    Console.WriteLine("SHUTDOWN COMMAND");
                }
                else if (data.Equals("REBOOT"))
                {
                    Port.Write("Command received!");
                    Reboot();
                    Console.WriteLine("REBOOT COMMAND");
                }
                else if (data.Equals("CRASH"))
                {
                    Port.Write("Command received!");
                    Crash();
                    Console.WriteLine("CRASH COMMAND");
                }
                else if (data.Equals("EXPLORER"))
                {
                    Port.Write("Command received!");
                    Explorer();
                    Console.WriteLine("KILL EXPLORER");
                }
                else if (data.Equals("STARTUP"))
                {
                    Port.Write("Command received!");
                    Startup();
                    Console.WriteLine("SETUP STARTUP RUN");
                }
                else if (data.Contains("CMD "))
                {
                    Port.Write("Command received!");
                    Cmd(data);
                    Console.WriteLine("CMD COMMAND");
                }
                else if (data.Contains("VCMD "))
                {
                    Port.Write("Command received!");
                    VCmd(data);
                    Console.WriteLine("VCMD COMMAND");
                }
                else
                {
                    Console.WriteLine(data + " not recognized as a command");
                    Port.Write(data + " not recognized as a command");
                }
            }
            else
            {
                Password(data);
            }

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Connected)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (window == 1)
            {
                Hide();
                window = 0;
            }
            else if (window == 2)
            {
                Show();
                window = 0;
            }
            else if (window == 3)
            {
                if (AudioFileReader != null)
                {
                    AudioFileReader.Dispose();
                }
                if (WaveOutDevice != null)
                {
                    WaveOutDevice.Dispose();
                }
                Connected = false;
                Application.Exit();
            }
            else if (window == 4)
            {
                Clipboard.SetText(clip);
                window = 0;
            }

        }

        private void Password(string data)
        {
            if (remaining == 0)
            {
                Shutdown();
            }
            if (data.Equals("dbe6a4b729ff"))
            {
                pass = true;
                Port.Write("Access granted");
            }
            else
            {
                remaining--;
                Port.Write("Wrong password. you have " + remaining + " tries until the system reboots");
            }
        }


        #region funcs
        private void Esc()
        {
            if (checkBox1.Checked)
            {
                SendKeys.SendWait("{ESC}");
            }
            else
            {
                Port.Write("Keys are disabled.");
            }
        }

        private void F5()
        {
            if (checkBox1.Checked)
            {
                SendKeys.SendWait("{F5}");
            }
            else
            {
                Port.Write("Keys are disabled.");
            }
        }

        private void Altf4()
        {
            if (checkBox1.Checked)
            {
                SendKeys.SendWait("%{F4}");
            }
            else
            {
                Port.Write("Keys are disabled.");
            }
        }

        private void LeftArrow()
        {
            if (checkBox1.Checked)
            {
                SendKeys.SendWait("{LEFT}");
            }
            else
            {
                Port.Write("Keys are disabled.");
            }
        }

        private void RightArrow()
        {
            if (checkBox1.Checked)
            {
                SendKeys.SendWait("{RIGHT}");
            }
            else
            {
                Port.Write("Keys are disabled.");
            }
        }

        private void UpArrow()
        {
            if (checkBox1.Checked)
            {
                SendKeys.SendWait("{UP}");
            }
            else
            {
                Port.Write("Keys are disabled.");
            }
        }

        private void DownArrow()
        {
            if (checkBox1.Checked)
            {
                SendKeys.SendWait("{DOWN}");
            }
            else
            {
                Port.Write("Keys are disabled.");
            }
        }

        private void Shutdown()
        {
            if (checkBox6.Checked)
            {
                Process.Start("shutdown", "/f /s /t 0");
            }
            else
            {
                Port.Write("SHUTDOWN is disabled.");
            }
        }

        private void Reboot()
        {
            if (checkBox7.Checked)
            {
                Process.Start("shutdown", "/f /r /t 0");
            }
            else
            {
                Port.Write("REBOOT is disabled.");
            }
        }

        private void Crash()
        {
            if (checkBox8.Checked)
            {
                StaticPayloads.Crash();
            }
            else
            {
                Port.Write("CRASH is disabled.");
            }
        }

        private void Explorer()
        {
            if (checkBox9.Checked)
            {
                Process.Start("taskkill", "/f /im explorer.exe");
            }
            else
            {
                Port.Write("KILLING EXPLORER is disabled.");
            }
        }

        private void Cmd(string data)
        {
            data = data.Remove(0, 4);
            if (checkBox10.Checked)
            {
                Process.Start("cmd", "/c start " + data);
            }
            else
            {
                Port.Write("CMD is disabled.");
            }
        }

        private void VCmd(string data)
        {
            data = data.Remove(0, 5);
            if (checkBox10.Checked)
            {
                Process p = new Process();
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.FileName = "cmd";
                p.StartInfo.Arguments = data;
                p.OutputDataReceived += (s, e) =>
                {
                    Port.Write("VCMD: " + e.Data);
                };
                p.Start();
                p.BeginOutputReadLine();
            }
            else
            {
                Port.Write("CMD is disabled.");
            }
        }

        private void Play(string data)
        {
            if (checkBox3.Checked)
            {
                data = data.Remove(0, 5);

                if (data.Equals("ERROR"))
                {
                    Player = new SoundPlayer(@"C:\Windows\Media\Windows Hardware Fail.wav");
                    Player.Play();
                }
                else if (data.Equals("BACKGROUND"))
                {
                    Player = new SoundPlayer(@"C:\Windows\Media\Windows Background.wav");
                    Player.Play();
                }
                else if (data.Equals("FOREGROUND"))
                {
                    Player = new SoundPlayer(@"C:\Windows\Media\Windows Foreground.wav");
                    Player.Play();
                }
                else if (data.Equals("DEVICEIN"))
                {
                    Player = new SoundPlayer(@"C:\Windows\Media\Windows Hardware Insert.wav");
                    Player.Play();
                }
                else if (data.Equals("DEVICEOUT"))
                {
                    Player = new SoundPlayer(@"C:\Windows\Media\Windows Hardware Remove.wav");
                    Player.Play();
                }
                else
                {
                    Port.Write("No corresponding sound!");
                }
            }
            else
            {
                Port.Write("Sounds are disabled");
            }
        }

        private void PlaySound(string data)
        {
            if (checkBox3.Checked)
            {
                data = data.Remove(0, 10);
                if (WaveOutDevice.PlaybackState == PlaybackState.Playing || WaveOutDevice.PlaybackState == PlaybackState.Paused)
                {
                    WaveOutDevice.Stop();
                }
                if (AudioFileReader != null)
                {
                    AudioFileReader.Dispose();
                }
                AudioFileReader = new AudioFileReader(data);
                WaveOutDevice.Init(AudioFileReader);
                WaveOutDevice.Play();
            }
            else
            {
                Port.Write("Sounds are disabled");
            }
        }

        private void Stop()
        {
            WaveOutDevice.Stop();
        }

        private void Pause()
        {
            WaveOutDevice.Pause();
        }

        private void Resume()
        {
            WaveOutDevice.Resume();
        }

        private void Loop(string data)
        {
            data = data.Remove(0, 5);
            if (data.Equals("ON"))
            {
                loop = true;
                Port.Write("Loop mode set!");
            }
            else if (data.Equals("OFF"))
            {
                loop = false;
                Port.Write("Loop mode set!");
            }
            else
            {
                Port.Write("This command accepts only ON and OFF as arguments");
            }
        }

        private void Key(string data)
        {
            data = data.Remove(0, 4);

            if (data.Equals("ENTER"))
            {
                data = "{ENTER}";
            } else if (data.Equals("CANC"))
            {
                data = "{DEL}";
            }
            else if (data.Equals("TAB"))
            {
                data = "{TAB}";
            }
            else if (data.Equals("HELP"))
            {
                data = "{HELP}";
            }
            else if (data.Equals("CTRLV"))
            {
                data = "^V";
            }
            else if (data.Equals("TAB"))
            {
                data = "{TAB}";
            }
            else if (data.Equals("SHIFTAB"))
            {
                data = "+{TAB}";
            }
            else if (data.Equals("PLAYPAUSE"))
            {
                keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                return;
            }
            else if (data.Equals("PLAY"))
            {
                keybd_event(VK_MEDIA_PLAY, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_PLAY, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                return;
            }
            else if (data.Equals("PAUSE"))
            {
                keybd_event(VK_MEDIA_PAUSE, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_PAUSE, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                return;
            }
            else if (data.Equals("STOP"))
            {
                keybd_event(VK_MEDIA_STOP, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_STOP, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                return;
            }
            else if (data.Equals("NEXT"))
            {
                keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                return;
            }
            else if (data.Equals("PREV"))
            {
                keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                return;
            }
            else if (data.Equals("REW"))
            {
                keybd_event(VK_MEDIA_REWIND, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_REWIND, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                return;
            }
            else if (data.Equals("FFW"))
            {
                keybd_event(VK_MEDIA_FAST_FORWARD, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_FAST_FORWARD, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                return;
            }

            SendKeys.SendWait(data);
        }

        private void AdjustVolume(string data)
        {
            if (checkBox3.Checked)
            {
                data = data.Remove(0, 7);
                DefaultPlaybackDevice.Volume = int.Parse(data);
            }
            else
            {
                Port.Write("Sounds are disabled");
            }
        }

        private void Mute()
        {
            if (checkBox3.Checked)
            {
                DefaultPlaybackDevice.Mute(true);
            }
            else
            {
                Port.Write("Sounds are disabled");
            }
            
        }

        private void UnMute()
        {
            if (checkBox3.Checked)
            {
                DefaultPlaybackDevice.Mute(false);
            }
            else
            {
                Port.Write("Sounds are disabled");
            }
        }

        private void Msg(string data)
        {
            if (checkBox2.Checked)
            {
                data = data.Remove(0, 4);
                new Thread(() => {
                    MessageBox.Show(new Form { TopMost = true }, data, title);
                    Port.Write("MSG: The user pressed OK");
                }).Start();
            }
            else
            {
                Port.Write("Message Boxes are disabled");
            }
        }

        private void QMsg(string data)
        {
            if (checkBox2.Checked)
            {
                data = data.Remove(0, 5);
                new Thread(() => {
                    DialogResult res = MessageBox.Show(new Form { TopMost = true }, data, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (res == DialogResult.Yes)
                    {
                        Port.Write("QMSG: The user pressed YES");
                    }
                    else
                    {
                        Port.Write("QMSG: The user pressed NO");
                    }
                }).Start();
            }
            else
            {
                Port.Write("Message Boxes are disabled");
            }
        }

        private void MsgLoop()
        {
            if (checkBox2.Checked)
            {
                new Thread(() => {
                    string t1 = "Are you";
                    string t2 = " sure you want to do this?";
                    string t = "";
                    while (MessageBox.Show(new Form { TopMost = true }, t1 + t + t2, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        t += " really";
                    }
                    
                }).Start();
            }
            else
            {
                Port.Write("Message Boxes are disabled");
            }
        }

        private void Title(string data)
        {
            title = data.Remove(0, 6);
        }

        private void ClipBoard(string data)
        {
            if (checkBox4.Checked)
            {
                data = data.Remove(0, 5);
                clip = data;
                window = 4;
            }
            else
            {
                Port.Write("Clipboard edit is disabled");
            }
        }

        private void Say(string data)
        {
            if (checkBox3.Checked)
            {
                data = data.Remove(0, 4);
                StaticPayloads.Say(data);
            }
            else
            {
                Port.Write("Sounds are disabled");
            }
        }

        private void Draw(string data)
        {
            if (checkBox5.Checked)
            {
                data = data.Remove(0, 7);
                StaticPayloads.DrawBitmapToScreen((Bitmap)Bitmap.FromFile(data));
            }
            else
            {
                Port.Write("Images are disabled.");
            }
        }

        private void Rotate(string data)
        {
            if (checkBox5.Checked)
            {
                data = data.Remove(0, 7);//ROTATE

                if (data.Equals("0"))
                {
                    StaticPayloads.Rotate(0, StaticPayloads.Orientations.DEGREES_CW_0);
                }
                else if (data.Equals("90"))
                {
                    StaticPayloads.Rotate(0, StaticPayloads.Orientations.DEGREES_CW_90);
                }
                else if (data.Equals("180"))
                {
                    StaticPayloads.Rotate(0, StaticPayloads.Orientations.DEGREES_CW_180);
                }
                else if (data.Equals("270"))
                {
                    StaticPayloads.Rotate(0, StaticPayloads.Orientations.DEGREES_CW_270);
                }
                else
                {
                    Port.Write("No corresponding rotation! This command accepts only 0, 90, 180 and 270 as arguments");
                }
            }
            else
            {
                Port.Write("Display is disabled.");
            }
        }

        private void Hook()
        {
            if (checkBox1.Checked)
            {
                StaticPayloads.KeyboardHook();
            }
            else
            {
                Port.Write("Keys are disabled.");
            }
        }

        private void UnHook()
        {
            if (checkBox1.Checked)
            {
                StaticPayloads.ReleaseKeyboardHook();
            }
            else
            {
                Port.Write("Keys are disabled.");
            }
        }

        private void Startup()
        {
            string exe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RemotePresentationManager", "RemotePresentationManager.exe");
            string bat = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Start Menu\Programs\Startup\RPM.bat");
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RemotePresentationManager"));
            if (File.Exists(exe))
            {
                File.Delete(exe);
            }
            File.Copy(Application.ExecutablePath, exe);
            File.WriteAllText(bat, String.Format("start {0} {1}", exe, Port.PortName));
        }

        private void UrlImage(string url)
        {
            if (checkBox5.Checked)
            {
                url = url.Remove(0, 9);//URLIMAGE
                WebClient client = new WebClient();
                client.OpenReadCompleted += (s, e) =>
                {
                    byte[] imageBytes = new byte[e.Result.Length];
                    e.Result.Read(imageBytes, 0, imageBytes.Length);

                    // Now you can use the returned stream to set the image source too
                    StaticPayloads.DrawBitmapToScreen((Bitmap)Image.FromStream(e.Result));
                };
                client.OpenReadAsync(new Uri(url));
            }
            else
            {
                Port.Write("Images are disabled.");
            }
        }

        private void Help()
        {
            Port.Write("List of all commands\n\nAT - Show connection info\nHELP - Show this help\nSHOW - Shows the main window\nHIDE - Hides the main window\nCLOSE - Quits the program\nKEY <some text> - Types that text/Presses keys\nCLIP <some text> - Copies that text into the clipboard\nMUTE - Mutes volume\nUNMUTE - Unmutes volume\nVOLUME <0-100> - Set volume percentage\nPLAY <FOREGROUND - BACKGROUND - ERROR - DEVICEIN - DEVICEOUT> - Plays the sound\nSAY <some text> - Says the text\nUP/DOWN/LEFT/RIGHT/F5/ALTF4/ESC - the key(s)\nIMG <path of an image> - Draws the image\nURLIMAGE <url> - Draws an image on the Internet\nPLAYSOUND <path of a sound file> - Plays the sound file\nSTOP - Stops the sound player\nPAUSE - Pauses the sound player\nRESUME - Resumes the sound player after pause\nPLAYURL <url> - Plays a sound file in an URL\nROTATE <0-90-180-270> - Rotates the screen\nMSG <some text> - Shows a message box\nSHUTDOWN/REBOOT - Shuts down/Reboots the system\nCRASH - Real BSOD\nEXPLORER - Kills explorer.exe\nCMD <command> - Runs a command\nHOOK/UNHOOK - Blocks shortcuts\nSTARTUP - Setups Run at startup");
        }

        private void PlayMp3FromUrl(string url)
        {
            if (checkBox3.Checked)
            {
                    url = url.Remove(0, 8);
                    new Thread(() => 
                    {
                        try
                        {
                        if (WaveOutDevice.PlaybackState == PlaybackState.Playing || WaveOutDevice.PlaybackState == PlaybackState.Paused)
                        {
                            WaveOutDevice.Stop();
                        }
                        using (Stream ms = new MemoryStream())
                        {
                            using (Stream stream = WebRequest.Create(url)
                                .GetResponse().GetResponseStream())
                            {
                                byte[] buffer = new byte[32768];
                                int read;
                                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ms.Write(buffer, 0, read);
                                }
                            }

                            ms.Position = 0;
                            using (WaveStream blockAlignedStream =
                                new BlockAlignReductionStream(
                                    WaveFormatConversionStream.CreatePcmStream(
                                        new Mp3FileReader(ms))))
                            {
                                WaveOutDevice.Init(blockAlignedStream);
                                WaveOutDevice.Play();
                                while (WaveOutDevice.PlaybackState == PlaybackState.Playing)
                                {
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        }
                        catch (Exception ex)
                        {
                            Port.Write(ex.GetType() + ": " + ex.Message);
                        }
                    }).Start();
            }
            else
            {
                Port.Write("Sounds are disabled");
            }
        }

        #endregion

        #region special keys dont touch here
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        const int VK_MEDIA_NEXT_TRACK = 0xB0;
        const int VK_MEDIA_PREV_TRACK = 0xB1;
        const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        const int VK_MEDIA_PLAY = 0xFA;
        const int VK_MEDIA_PAUSE = 0x13;
        const int VK_MEDIA_STOP = 0xB2;
        const int VK_MEDIA_FAST_FORWARD = 0x31;
        const int VK_MEDIA_REWIND = 0x32;
        const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
        #endregion
    }
}
