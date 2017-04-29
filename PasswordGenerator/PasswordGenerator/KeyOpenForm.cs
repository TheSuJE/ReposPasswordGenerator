﻿using System;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PasswordGenerator
{
    public partial class KeyOpenForm : Form
    {
        string path = "";
        byte[] file;
        public KeyOpenForm(string path, byte[] file)
        {
            this.path = path;
            this.file = file;
            InitializeComponent();
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            try { InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new System.Globalization.CultureInfo("en-US")); }
            catch (Exception) { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                RCC4 decrypter = new RCC4(Encoding.ASCII.GetBytes(textBox1.Text));
                FileStream openfile = File.OpenRead(path);
                byte[] dfile = new byte[openfile.Length];
                openfile.Read(dfile, 0, dfile.Length);
                openfile.Close();
                dfile = decrypter.Decode(dfile);
                string text = Encoding.ASCII.GetString(dfile);
                if(checkBox1.Checked)Clipboard.SetText(text);
                MessageBox.Show((checkBox2.Checked ? "Password: " + text + "\n" : "") + (checkBox1.Checked ? "Password been copied to the clipboard." : ""), "Decrypted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(null, null);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked && !checkBox2.Checked)
            {
                checkBox1.Checked = true;
                MessageBox.Show("You can't turn off all the checkboxes", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked && !checkBox2.Checked)
            {
                checkBox2.Checked = true;
                MessageBox.Show("You can't turn off all the checkboxes", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}