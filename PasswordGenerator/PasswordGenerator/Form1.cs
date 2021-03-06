﻿using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;

namespace PasswordGenerator
{
    public partial class Form1 : Form
    {
        char[] letters = "abcdefghijklmnopqrstuvwxyz".ToCharArray(); //Symbols of alphabet
        char[] symbols = @"!@#$%&-_?".ToCharArray();
        GoogleDrive drive;
        public string workpath;
        public static string Profile;

        public Form1(string profile)
        {
            bool contains = false;
            foreach (string prof in IniFile.GetSectionNames()) { if (prof.Equals(profile)) { contains = true; break; } }
            if (!contains) { MessageBox.Show("Профиль не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); Application.Exit(); }
            workpath = IniFile.ReadINI(profile, "workpath");
            if (!Directory.Exists(workpath))
            {
                MessageBox.Show("Указанного пути не существует!\nВыбере новый", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                while (true)
                {
                    Settings settings = new Settings(workpath);
                    if (settings.ShowDialog() == DialogResult.OK) { workpath = settings.workpath; IniFile.Write(profile, "workpath", workpath); break; }
                }
            }
            InitializeComponent();
            label3.Text = profile;
            Profile = profile;
            ValidConfig();
            Console.WriteLine("Программа запущена. Рабочая папка: " + workpath + ". Имя пользователя: " + Profile + ".");
            CheckForUpdates();
        }
        public void ValidConfig()
        {
            if (!IniFile.KeyExists("workpath", Profile))
            {
                IniFile.Write(Profile, "workpath", "");
            }
        }
        private string newgenPassword(bool upper, bool numbers, bool symbol, int amount)
        {
            Random rand = new Random();
            string candidates = charArray2string(letters) ;
            candidates += upper ? changelength(charArray2string(letters).ToUpper(), 10) : "";
            candidates += symbol ? changelength(charArray2string(symbols).ToString(), 5) : "";
            candidates += numbers ? changelength("1234567890", 5) : "";
            string password = "";
            for (int i = 0; i < amount; i++) { password += candidates.ToCharArray()[rand.Next(0, candidates.Length)].ToString(); }
           
            return password;
        }
        private string changelength(string text, int lenght)
        {
            string newtext = "";
            Random rand = new Random();
            for (int i = 0; i < lenght; i++)
            {
                int r = rand.Next(0, text.Length);
                newtext += text.ToCharArray()[r].ToString();
                text.Remove(r, 1);
            }
            return newtext;
        }
        private string charArray2string(char[] a)
        {
            string b = "";
            foreach(char c in a){ b += c.ToString(); }
            return b;
        }
         private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = newgenPassword(checkBox1.Checked, checkBox2.Checked, checkBox3.Checked, trackBar1.Value);
            
        }
        private string getNumFileName(string filename, int number)
        {
            return Path.GetFileNameWithoutExtension(filename) + " (" + number + ")" + Path.GetExtension(filename);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Text box is empty");
              
                return;
            }
            Clipboard.SetText(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Text box is empty");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = workpath;
            string filename = "password.password";
            if (File.Exists(workpath + @"\" + filename))
            {
                for (int i = 1; i < int.MaxValue; i++)
                {
                    if (File.Exists(workpath + @"\" + getNumFileName(filename, i)))
                    {
                        continue;
                    }
                    else
                    {
                        sfd.FileName = getNumFileName(filename, i);
                        break;
                    }
                }
            }
            else
            {
                sfd.FileName = filename;
            }
            sfd.Filter = "Password files | *.password";
            sfd.DefaultExt = "password";

            while(true)
            {
                sfd.FileName = filename;
                DialogResult result = sfd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (!Path.GetDirectoryName(sfd.FileName).Equals(workpath))
                    {
                        DialogResult res = MessageBox.Show("Вы уверены, что хотите сохранить файл вне рабочей папки?\nВ этом случае вы не сможете синхронизировать пароли с гугл-диском!", "Хотите продолжить?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                        if (res == DialogResult.Yes) { new KeyForm(sfd.FileName, Encoding.ASCII.GetBytes(textBox1.Text), true).ShowDialog(); return; }
                        else if (res == DialogResult.No) continue;
                        else return; 

                   }
                    string path = sfd.FileName;
                    new KeyForm(path, Encoding.ASCII.GetBytes(textBox1.Text), true).ShowDialog();
                    return;
                }
                else
                {
                    return;
                }
            }

        }
   
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label2.Text = trackBar1.Value.ToString();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string symbols = "";
            foreach (char symbol in this.symbols)
            {
                symbols += symbol.ToString();
            }
            SettingsForm sf = new SettingsForm(symbols);
            DialogResult dr = sf.ShowDialog();
            if (dr == DialogResult.OK)
            {
              
                this.symbols = sf.textBox1.Text.ToCharArray();
            }
        }
        private void CheckForUpdates()
        {
            try
            {
                Console.WriteLine("Проверка обновлений...");
                Console.WriteLine("Создание запроса...");
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://sumjest.ru/programsinfo/programs.txt");
                Console.WriteLine("Запрос отправлен. Ожидание ответа...");
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                Console.WriteLine("Ответ получен.");
                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new StreamReader(res.GetResponseStream(), encoding))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] linea = line.Split(';');

                        if (line.Split(';')[0].Contains("PasswordGenerator"))
                        {
                            Version v;
                            if (Version.TryParse(line.Split(';')[1], out v)) { if (v.CompareTo(Version.Parse(Application.ProductVersion)) > 0) { Console.Write("Обнаружена более новая версия - {0}.\n", v.ToString()); menuStrip1.Items.Add("Вышла новая версия программы!", null, onNewClick); } }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if(ex is WebException)
                {
                    Console.WriteLine("Ошибка подключения. Следующая попытка будет совершена при запуске программы.");
                }
                else
                {
                    MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
              
            }

        }

        private void onNewClick(object sender, EventArgs e)
        {
            GetChangeLog();
        }
        private void GetChangeLog()
        {
            HttpWebRequest proxy_request = (HttpWebRequest)WebRequest.Create("http://sumjest.ru/index/pg/0-8");
            proxy_request.Method = "GET";
            proxy_request.Timeout = 5000;
            HttpWebResponse resp = proxy_request.GetResponse() as HttpWebResponse;
            string html = "";
            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                html = sr.ReadToEnd();
            string a = Regex.Match(html, @"<!--Dangerous--><p>Change log:([\s\S]*)\<!--Dangerous-->").ToString();
            a = a.Replace("<!--Dangerous-->", "");
            a = a.Replace("<p>", "");
            a = a.Replace("</p>", "");
            a = a.Replace("<br />", "");
            a = a.Replace("&nbsp;", "  ");
            MessageBox.Show(a);

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new OpenForm().ShowDialog(this);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
           drive = new GoogleDrive(Profile, this);
            uploadToolStripMenuItem.Enabled = true;
            downloadToolStripMenuItem.Enabled = true;
            syncToolStripMenuItem.Enabled = true;
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res =MessageBox.Show("Вы уверены, что хотите скачать все пароли с диска?\nВ этом случае, пароли с одинаковыми именами будут заменяться скаченными!", "Осторожно!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (res == DialogResult.Yes)
            {
                drive.Download(workpath);
                MessageBox.Show("Файлы успешно скачены!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Вы уверены, что хотите загрузить все пароли на диск?\nВ этом случае, пароли с одинаковыми именами будут заменяться вашими!", "Осторожно!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (res == DialogResult.Yes)
            {
                drive.Upload(workpath);
                MessageBox.Show("Файлы успешно закачены!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void syncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Вы уверены, что хотите синхронизировать все пароли?", "Осторожно!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (res == DialogResult.Yes)
            {
                drive.Download(workpath);
                drive.Upload(workpath);
                MessageBox.Show("Файлы успешно синхронизированы!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void workpathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings(workpath);
            if (settings.ShowDialog() == DialogResult.OK)
            {
                workpath = settings.workpath; IniFile.Write(Profile, "workpath", workpath);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
        }
    }
}