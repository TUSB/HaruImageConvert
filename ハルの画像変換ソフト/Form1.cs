using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SilverNBTLibrary;
using System.IO;

namespace ハルの画像変換ソフト
{
    public partial class Form1 : Form
    {
        private string[] fileName;
        private bool IsSchematic;
        private bool IsFloydSteinberg;
        private bool IsComplete = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetColorFiles();
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            if (comboBox2.Items.Count > 0)
                comboBox2.SelectedIndex = 0;

            numericUpDown1.Maximum = int.MaxValue;
        }

        private void GetColorFiles()
        {
            string[] files = System.IO.Directory.GetDirectories(@"data\color", "*", System.IO.SearchOption.AllDirectories);
            foreach(var file in files)
            {
                if (File.Exists(file + @"\BlockColor.csv"))
                    comboBox1.Items.Add(Path.GetFileName(file));
                if (File.Exists(file + @"\MapColor.csv"))
                    comboBox2.Items.Add(Path.GetFileName(file));
            }
        }

        private void LoadBlockColors()
        {
            Convert.Colors.blockpalette.Clear();
            TextFieldParser parser = new TextFieldParser(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\data\color\" + 
                comboBox1.SelectedItem.ToString() + @"\BlockColor.csv", Encoding.GetEncoding("Shift_JIS"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            parser.ReadFields();
            while (parser.EndOfData == false)
            {
                var block = new Convert.Block();
                string[] column = parser.ReadFields();

                block.ID = int.Parse(column[0]);
                block.Meta = int.Parse(column[1]);
                block.R = int.Parse(column[2]);
                block.G = int.Parse(column[3]);
                block.B = int.Parse(column[4]);

                Convert.Colors.blockpalette.Add(block);
            }

            parser.Close();
        }

        private void LoadMapColors()
        {
            Convert.Colors.mappalette.Clear();
            TextFieldParser parser = new TextFieldParser(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\data\color\" + 
                comboBox2.SelectedItem.ToString() + @"\MapColor.csv", Encoding.GetEncoding("Shift_JIS"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            parser.ReadFields();
            while (parser.EndOfData == false)
            {
                var block = new Convert.Block();
                string[] column = parser.ReadFields();

                block.ID = int.Parse(column[0]);
                block.R = int.Parse(column[1]);
                block.G = int.Parse(column[2]);
                block.B = int.Parse(column[3]);

                Convert.Colors.mappalette.Add(block);
            }

            parser.Close();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            IsComplete = false;
            var gen = new Convert.Generate();
            var bw = (BackgroundWorker)sender;
            int i = radioButton2.Checked ? (int)numericUpDown1.Value : 0;
            foreach (var file in fileName)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    progressBar1.Value = 0;
                });
                Bitmap bitmap;
                try
                {
                    bitmap = new Bitmap(file);
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        progressBar1.Maximum = bitmap.Height;
                    });
                }
                catch (Exception)
                {
                    LogAppend("非対応形式の画像のため処理をスキップします。");
                    continue;
                }


                if (IsFloydSteinberg)
                {
                    LogAppend("誤差拡散をしています...");
                    bitmap = gen.FloydSteinberg(bitmap,ref bw,IsSchematic);
                    LogAppend("誤差拡散完了");
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        progressBar1.Value = 0;
                    });
                }

                
                var compound = new NBTTagCompound();
                

                if (IsSchematic)
                {
                    LogAppend("schematic形式に変換しています...");
                    compound = gen.ImageConvert(bitmap, IsSchematic, ref bw);
                    SilverNBTLibrary.NBTFile.SaveToFile(Path.GetDirectoryName(file) + @"\" + Path.GetFileNameWithoutExtension(file) + ".schematic", compound, true);
                    bitmap.Dispose();
                    LogAppend(file + "を変換しました");
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        progressBar1.Maximum = (bitmap.Height / 128) * (bitmap.Width / 128) * 128;
                    });
                    for (int y = 0; y < (bitmap.Height / 128) + 1; y++)
                    {
                        for (int x = 0; x < (bitmap.Width / 128) + 1; x++)
                        {
                            int ry = 0;
                            int rx = 0;
                            int nowx = x * 128;
                            int nowy = y * 128;

                            if (bitmap.Height - nowy >= 128)
                            {
                                ry = 128;
                            }
                            else
                            {
                                ry = bitmap.Height - nowy;
                                continue;
                            }

                            if (bitmap.Width - nowx >= 128)
                            {
                                rx = 128;
                            }
                            else
                            {
                                rx = bitmap.Width - nowx;
                                continue;
                            }
                            if (rx >= 0 && ry >= 0)
                            {
                                Rectangle rect = new Rectangle(nowx, nowy, rx, ry);
                                Bitmap bmpNew = bitmap.Clone(rect, bitmap.PixelFormat);
                                compound = gen.ImageConvert(bmpNew, IsSchematic, ref bw);
                                SilverNBTLibrary.NBTFile.SaveToFile(Path.GetDirectoryName(file) + @"\map_" + i.ToString() + ".dat", compound, true);
                                bmpNew.Dispose();
                            }

                            LogAppend(Path.GetDirectoryName(file) + @"\map_" + i.ToString() + "を変換しました");
                            i++;
                        }
                    }
                }
                bitmap.Dispose();
            }
        }

        private void LogAppend(string text)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                richTextBox1.AppendText(text + "\n");
            });
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsComplete = true;
            progressBar1.Value = 0;
            LogAppend("変換がすべて終了しました。");
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            else
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (IsComplete)
            {
                //コントロール内にドロップされたとき実行される
                //ドロップされたすべてのファイル名を取得する
                fileName =
                    (string[])e.Data.GetData(DataFormats.FileDrop, false);
                LoadBlockColors();
                LoadMapColors();

                IsSchematic = radioButton1.Checked;
                IsFloydSteinberg = radioButton4.Checked;

                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value++;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = radioButton2.Checked ? true : false;
        }
    }
}
