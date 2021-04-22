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
        public DateTime UserDate = new DateTime(2020, 1, 1);
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
                Connection();
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
            DeleteTables();
            Transfering();
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
            int count = 1;
            await Task.Run(() =>
            {
                foreach (oru oruitem in oruList)
                {
                    if (IsLater(oruitem.DataIspolneniya_oru, UserDate))
                    {
                        dictionary.Add(oruitem.ID, count);
                        newHelper.InsertObjectIntoTable(oruitem, "oru");
                        count++;
                    }
                }
            });
            var imgList = originalHelper.Select<img>();
            await Task.Run(() =>
            {
                int id;
                foreach (img imgitem in imgList)
                {
                    id = GetIdFromImg(imgitem.Info);
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
            label1.Visible = true;
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
            int result = Convert.ToInt32(info.Split('|')[3]);
            return result;
        }
        private img ChangeIdInImg(img imgitem, int newid)
        {
            string info = imgitem.Info;
            int oldid = GetIdFromImg(info);
            info.Replace(oldid.ToString(), newid.ToString());
            imgitem.Info = info;
            return imgitem;
        }
    }
}

