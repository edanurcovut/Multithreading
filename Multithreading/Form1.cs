using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BigDataMultiThreading
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<string> record = new List<string>();
        public List<string> dg2 = new List<string>();

        
        public List<string> SimilarityCalculator(int i, float rate, string recordString, int searchingColumnIndex)
        {
           
            for (int j = 1; j < dataGridView1.Rows.Count; j++)
            {
                
                if (j != i && dataGridView1.Rows[j].Cells[searchingColumnIndex].Value != null)
                {
                    
                    var comparingRecord = dataGridView1.Rows[j].Cells[searchingColumnIndex].Value.ToString().Split(new[] { ' ' }).ToArray();
                    
                    string comparingRecordString = "";
                    foreach (var item in comparingRecord)
                    {
                        comparingRecordString = comparingRecordString + " " + item + " ";
                    }
                    comparingRecordString = comparingRecordString.Trim();

                    
                    int length = 0;
                    if (comparingRecord.Length >= record.Count)
                    {
                        length = comparingRecord.Length;
                    }
                    else
                    {
                        length = record.Count;
                    }

                    
                    int counter = 0;
                    for (int k = 0; k < record.Count; k++)
                    {
                        for (int l = 0; l < comparingRecord.Length; l++)
                        {
                            string comparingRecordValue = comparingRecord[l];
                            string recordValue = record[k];
                            
                            if (comparingRecordValue.Contains(recordValue))
                            {
                                counter++;
                            }
                        }
                    }

                    
                    rate = (100 * counter) / length;

                      
                    dg2.Add(recordString + "," + comparingRecordString + "," + rate.ToString());
                }
            }
            
            return dg2;
        }

        
        public void GetAllRecords(string[] lines)
        {
            
            foreach (var cellValues in lines.Skip(1))
            {
                
                var cellArray = cellValues
                    .Split(new[] { ',' }, StringSplitOptions.None).Select(x => x.Trim(' ', '?', '"', '.')).ToArray();

                
                var isNull = false;

                
                if (cellArray.Length == 18)
                {
                    for (int i = 0; i < cellArray.Length; i++)
                    {
                        
                        if (cellArray[1] == "" || cellArray[3] == "" || cellArray[7] == "" || cellArray[8] == "" || cellArray[9] == "" || cellArray[17] == "")
                        {
                            isNull = true;
                            break;
                        }
                    }
                   
                    if (!isNull)
                    {
                        dataGridView1.Rows.Add(cellArray[1], cellArray[3], cellArray[7], cellArray[8], cellArray[9], cellArray[17]);

                    }
                }
            }
        }

        
        public int cntr = 0;


        
        private void btnKayitGetir_Click(object sender, EventArgs e)
        {
            
            timer1.Start();

            
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            string FilePath = file.FileName;

            
            var lines = File.ReadAllLines(FilePath);

            
            int[] columnIndex = { 1, 3, 7, 8, 9, 17 };



            
            if (lines.Count() > 0)
            {
                
                dataGridView1.ColumnCount = 6;
                dataGridView1.Columns[0].Name = "Product";
                dataGridView1.Columns[1].Name = "Issue";
                dataGridView1.Columns[2].Name = "Company";
                dataGridView1.Columns[3].Name = "State";
                dataGridView1.Columns[4].Name = "Zip Code";
                dataGridView1.Columns[5].Name = "Complaint Id";

               
                int threadCount = int.Parse(textBox1.Text);
                
                Thread[] ts = new Thread[threadCount];
                int value = lines.ToList().Count / threadCount;

                for (int i = 0; i < ts.Length; i++)
                {
                    
                    ts[i] = new Thread(() => GetAllRecords(lines.Skip(i * value).Take(value).ToArray()));
                    
                    ts[i].Start();
                    
                    ts[i].Join();
                }
                
                timer1.Stop();
                MessageBox.Show("Geçen Süre : 1 saniye");

                textBox1.Enabled = true;
                comboBox2.Enabled = true;
                btnBenzerlikGetir.Enabled = true;
                txtOran.Enabled = true;
                txtColumnValue.Enabled = true;
                textBox1.Enabled = false;
                btnKayitGetir.Enabled = false;
            }
        }
        
        private void btnBenzerlikGetir_Click(object sender, EventArgs e)
        {
           
            int searchingColumnIndex = comboBox2.SelectedIndex;

            
            float rate = 0;

            
            List<Similarity> list2 = new List<Similarity>();

            
            for (int i = 1; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[searchingColumnIndex].Value != null)
                {
                    
                    record = dataGridView1.Rows[i].Cells[searchingColumnIndex].Value.ToString().Split(new[] { ' ' }).ToList();
                    string recordString = "";

                    
                    foreach (var item in record)
                    {
                        recordString = recordString + " " + item + " ";
                    }
                    recordString = recordString.Trim();

                    
                    var similarList = SimilarityCalculator(i, rate, recordString, searchingColumnIndex);


                    
                    int threadCount = int.Parse(textBox1.Text);
                    Thread[] ts = new Thread[threadCount];
                    int value = similarList.ToList().Count / threadCount;

                    for (int j = 0; j < ts.Length; j++)
                    {
                        ts[j] = new Thread(() => SendSimilarity(similarList.Skip(j * value).Take(value).ToList(), list2));
                        ts[j].Start();
                        ts[j].Join();
                    }

                    
                    dataGridView2.DataSource = list2;

                }
            }
        }

        
        public void SendSimilarity(List<string> similarList, List<Similarity> list2)
        {
            
            List<string> list1 = new List<string>();

            
            int enteredRate = int.Parse(txtOran.Text);

            
            string value = txtColumnValue.Text;

            
            for (int x = 0; x < similarList.Count; x++)
            {
                
                foreach (var item in similarList)
                {
                    
                    list1 = item.Split(',').ToList();

                    
                    for (int z = 0; z < list1.Count; z++)
                    {
                        Similarity sm = new Similarity();
                        sm.Record1 = list1[0];
                        sm.Record2 = list1[1];
                        sm.SimilarityRate = list1[2];

                       


                        
                        if (!String.IsNullOrEmpty(value))
                        {
                            
                            if (int.Parse(sm.SimilarityRate) >= enteredRate && sm.Record1.ToLower().Trim().Contains(value.ToLower().Trim()))
                            {
                                list2.Add(sm);
                            }
                        }
                        
                        else
                        {
                            
                            if (int.Parse(sm.SimilarityRate) >= enteredRate)
                            {
                                list2.Add(sm);
                            }
                        }
                        
                        
                    }
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        
        private void timer1_Tick(object sender, EventArgs e)
        {
            cntr++;
        }
    }
}
