using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace DBCopier
{
    public partial class Form1 : Form
    {
        public SQLiteHelper originalHelper;
        public SQLiteHelper newHelper;
        private string path;
        public DateTime UserDate = new DateTime(2021, 1, 1); 
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = Path.GetDirectoryName(openFileDialog1.FileName);
                CopyDB();
                Log("бд скопирована");
                Connection();
                Log("подключение к бд завершено");
                DeleteTables();
                Log("таблицы oru и img очищены");
                newHelper.Vacuum();
                Log("вакуум завершен");
                Transfering();
            }
        }
        private void CopyDB()
        {
            File.Copy(path + @"\\SynDB.sqlite",
                path + @"\\ShortSynDB.sqlite", true);
        }
        private void Connection()
        {
            originalHelper = new SQLiteHelper(path + @"\\SynDB.sqlite");
            newHelper = new SQLiteHelper(path + @"\\ShortSynDB.sqlite");
            originalHelper.CreateConnection();
            newHelper.CreateConnection();
        }
        private void DeleteTables()
        {
            newHelper.Delete<oru>("");
            newHelper.Delete<img>("");
        }
        private async void Transfering()
        {
            var oruList = originalHelper.Select<oru>();
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            Log("начинается перебор в oru");
            await Task.Run(() =>
            {
                foreach (oru oruitem in oruList)
                {
                    if (IsLater(oruitem.DataIspolneniya_oru, UserDate))
                    {         
                        newHelper.InsertObjectIntoTable(oruitem, "oru");
                        dictionary.Add(oruitem.ID, (int)SQLiteHelper.GetLastInsertID(newHelper.con));
                    }
                }
            });
            var imgList = originalHelper.Select<img>();
            Log("начинается перебор в img");
            await Task.Run(() =>
            {
                int id;
                foreach (img imgitem in imgList)
                {
                    id = GetIdFromImg(imgitem.Info_img);
                    if (dictionary.ContainsKey(id))
                    {
                        img newImg = imgitem;
                        int newId = 0;
                        dictionary.TryGetValue(id, out newId);
                        if (newId != 0)
                            ChangeIdInImg(newImg, newId);
                        newHelper.InsertObjectIntoTable(newImg, "img");
                    }
                }
            });
            Log("завершено!");
        }
        private bool IsLater(string value, DateTime date)
        {
            DateTime valuedate = SQLiteHelper.ToDateTime(value);
            if (valuedate.CompareTo(date) >= 0)
                return true;
            else
                return false;
        }
        private int GetIdFromImg(string info)
        {
            int result = Convert.ToInt32(info.Split('|')[2]);
            return result;
        }
        private img ChangeIdInImg(img imgitem, int newid)
        {
            string info = imgitem.Info_img;
            int oldid = GetIdFromImg(info);
            string newinfo = info.Replace(oldid.ToString(), newid.ToString());
            imgitem.Info_img = newinfo;
            return imgitem;
        }
        public void Log(string msg)
        {
            log_textbox.Text += msg + " " + DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine;
        }
    }
}

