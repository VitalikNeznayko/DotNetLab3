﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FileEncryption
{
    public partial class Form1 : Form
    {
        private BackgroundWorker bgWorker = new BackgroundWorker();

        private string currentFilePath;
        private string currentKey;

        private bool isEncoding;

        private DateTime startTime;
        private Timer timer = new Timer();
        private TimeSpan elapsedTime;

        public Form1()
        {
            InitializeComponent();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        private void btn_encoding_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tb_4FileEncoding.Text) || string.IsNullOrEmpty(tb_keyEncoding.Text))
            {
                MessageBox.Show("Будь ласка, виберіть файл і введіть ключ.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            currentFilePath = tb_4FileEncoding.Text;
            currentKey = tb_keyEncoding.Text;
            isEncoding = true;

            progressBar.Value = 0;

            startTime = DateTime.Now;
            timer.Start();

            bgWorker.RunWorkerAsync();
        }

        private void btn_decoding_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tb_4FileDecoding.Text) || string.IsNullOrEmpty(tb_keyDecoding.Text))
            {
                MessageBox.Show("Будь ласка, виберіть файл та введіть ключ.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            currentFilePath = tb_4FileDecoding.Text;
            currentKey = tb_keyDecoding.Text;
            isEncoding = false;

            progressBar.Value = 0;

            startTime = DateTime.Now;
            timer.Start();

            bgWorker.RunWorkerAsync();
        }
        private void EncryptionFile(string filePath, string key, BackgroundWorker worker)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            string tempFile = Path.GetTempFileName(); 

            using (FileStream inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (FileStream tempStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[8192];
                long totalBytes = inputStream.Length;
                long processedBytes = 0;
                int bytesRead;

                while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        buffer[i] ^= keyBytes[i % keyBytes.Length];
                    }
                    tempStream.Write(buffer, 0, bytesRead);

                    processedBytes += bytesRead;
                    int progressPercentage = (int)((processedBytes * 100) / totalBytes);
                    worker.ReportProgress(progressPercentage);
                }
            }

            File.Copy(tempFile, filePath, overwrite: true); 
            File.Delete(tempFile);
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            EncryptionFile(currentFilePath, currentKey, (BackgroundWorker)sender);
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer.Stop();
            elapsedTime = DateTime.Now - startTime;
            string operation = isEncoding ? "зашифровано" : "дешифровано";
            MessageBox.Show($"Файл успішно {operation}: {currentFilePath}\nЧас виконання: {elapsedTime.TotalSeconds:F2} секунд.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_ChooseFIleEncoding_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Text Files|*.txt;*.doc;*.docx;*.xls;*.xlsx|All Files|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tb_4FileEncoding.Text = dlg.FileName;
            }
        }

        private void btn_ChooseFIleDecoding_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Text Files|*.txt;*.doc;*.docx;*.xls;*.xlsx|All Files|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tb_4FileDecoding.Text = dlg.FileName;
            }
        }
    }
}
