using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SynEngine.Recognition;
using SynEngine.General;
using SynEngine;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Windows.Media;
//using static SynEngine.MySQLHelper;
using Microsoft.Win32;
using System.ComponentModel;
using ELII.UI;
using ELII;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace EL2Maket
{
    public static class Tools
    {
		/// <summary>
		/// Должно бибикать
		/// </summary>
		public static void Beep()
		{
			System.Media.SystemSounds.Beep.Play();
		}

		public static string TrimPre(this string s, int i=3)
		{
			//i = int.Parse(DB.Settings.ProbirkaLen);
			if (s.Length <= i) return s;
			s = s.Substring(s.Length - i);
			s = "…" + s;
			return s;
		}

		public static int ПодстрокаПоПрефиксу(this List<string> NTE1, string pipesNoteTag)
		{
			var noteIndex = -1;
			string val = null;
			if (NTE1.Where(s => s.StartsWith(pipesNoteTag)).Count() == 0)
				NTE1.Add(pipesNoteTag);
			val = NTE1.Where(s => s.StartsWith(pipesNoteTag)).First();
			noteIndex = NTE1.IndexOf(val);
			return noteIndex;
		}

		static string logPath = null;
        /// <summary>
        /// Счетчик неудач при записи журнала в БД
        /// </summary>
        static int DbLogErrorReported = 0;
        /// <summary>
        /// Запись сообщения в журнал (в файл и в базу если получится - в таблицу log)
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string LogLine(this string msg, string procedure=null, string module=null)
        {
            if (procedure == null)
            {   //  Автоматическое определение имени вызывавшей функции и класса
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1).GetMethod();
                procedure = callingMethod.DeclaringType.Name+"."+ callingMethod.Name;
            }
            if (module == null)
            {   //  Автоматическое определение имени вызывавшего модуля
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1).GetMethod();
                module = callingMethod.DeclaringType.Assembly.GetName().Name;
            }

            var msg2 = DateTime.Now.ToString("dd.MM.yy HH:mm:ss ffff") + ": " + msg;
            (msg2 +"\r\n").Log();

            //if (DbLogErrorReported<10)    //  если не известно о большом количестве ошибок при работе с журналом в бд, попробуем скинуть лог в базу
            //    try
            //        { App.sqlHelp.Save(new SynDB.Log() { Message = msg,Module=module,Procedure=procedure });
            //        DbLogErrorReported = 0; //  восстановим счетчик ошибок сохранения журнала если сохранение произошло без ошибок
            //    } 
            //    catch(Exception ex)
            //        { ++DbLogErrorReported; if(DbLogErrorReported==1)
            //            ("Ошибка сохранения журнала в БД: " + ex.Message).LogLine(); }    //  В случае ошибки при записи журнала в бд отмечаем этот факт чтобы больше в бд не пытаться писать
            
            return msg;
        }
        /// <summary>
        /// Запись в журнал без записи в БД.
        /// </summary>
        /// <param name="msg">Сообщение</param>
        /// <param name="element">Необязательный параметр - Control, который пишет в лог.
        /// Используется для отсечки ошибок в режиме дизайнера.</param>
        public static string Log(this string msg, System.Windows.DependencyObject element = null)
        {
            if (element != null && DesignerProperties.GetIsInDesignMode(element)) return msg;
            if (logPath == null)
                logPath = Path.Combine(App.DataFolder, "log.txt");

            if (File.Exists(logPath))
            {
                File.AppendAllText(logPath, msg);
            }

            return msg;
        }
        public static Exception Log(this Exception ex)
        {
            "!!! EXCEPTION !!!\r\n".LogLine();
            $"Message: {ex.Message}\r\nStack: {ex.StackTrace}".LogLine();
            return ex;
        }
        public static void ВЖурнал(this string сообщение)
        {
            сообщение.LogLine();
        }


        /// <summary>
        /// Место хранения правильного кода активации (чтобы каждый раз заново не генерить с винта)
        /// </summary>
        //static string goodCode = null;
        ///// <summary>
        ///// по умолчанию генерирует хороший код активации для данного компа,
        ///// а если дать в параметрах код запроса, то генерирует активацию под него
        ///// </summary>
        ///// <param name="hddID"></param>
        ///// <returns></returns>
        //public static string GetGoodCode(string hddID="", bool isHash=false)   //  
        //{
        //    if (goodCode == null)
        //    {
        //        int hash = 0;
        //        if (hddID.Length == 0)
        //        {
        //            hddID = MachineID.MachineID.GetMachineID(); 
        //        }
        //        if (isHash)
        //            hash = int.Parse(hddID);
        //        else
        //            hash = hddID.GetHashCode();
        //        var goodValue = Math.Abs((int)(hash * 2 ^ 0xffffffff));
        //        var good = (goodValue).ToString("000-###-###-###");
        //        goodCode="EL2-" + good;
        //    }
        //    return goodCode;
        //}
        //public static string GetActivationRequest()
        //{
        //    var hddID = MachineID.MachineID.GetMachineID();
        //    return "EL2AR" + hddID.GetHashCode().ToString("####-000-###-###-###");
        //}

        public static string TableConvert(this string value, string[] from, string[] to)
        {
            string[][] t = new string[2][] { to, from };
            var v = t[0][t[1].ToList().IndexOf(value)];
            return v;
        }

        public static Bitmap Scan(string par="")
        {
            $"Начинается сканирование. Параметры: [{par}]".LogLine();
            string startTag = "=====IMAGE START=====";
            string endTag = "=====IMAGE END=====";

            Bitmap img = null;
            try
            {
                try
                {
                    System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
                    pProcess.StartInfo.FileName = @"PictureTaker.exe";
                    pProcess.StartInfo.Arguments = $"{par}|550|WIA"; //argument
                    pProcess.StartInfo.UseShellExecute = false;
                    pProcess.StartInfo.RedirectStandardOutput = true;
                    pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                    pProcess.Start();
                    string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                    pProcess.WaitForExit();

                    int startp = output.IndexOf(startTag) + startTag.Length;
                    var part = output.Substring(startp, output.IndexOf(endTag) - startp);

                    var data = Convert.FromBase64String(part);
                    var ms = new MemoryStream(data);
                    img = Bitmap.FromStream(ms) as Bitmap;
                    ms.Dispose();
                    //image.Source = bmp.ToImageSource();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ОШИБКА ВНЕШНЕГО ЗАПУСКА", ex.Message);
                }




                //img = Synteco.Module.PictureTaker.Scan(par,"550","WIA");


			}
			catch (Exception ex)
            {
                $"Ошибка сканирования!".LogLine();
                ex.Log();

				MessageBoxF.Show("Проверьте подключение сканера.", "Сканирование не удалось", MessageBoxButton.OK);
			}

            return img;
        }

		/// <summary>
		/// Конвертирует картинку в JPEG Base64 и создаёт тэг IMG со встроенными данными изображения.
		/// Позволяет вставлять в HTML картинки без приложения файлов.
		/// </summary>
		/// <param name="img">Изображение, которое нужно конвертировать в тэг HTML</param>
		/// <returns></returns>
		public static string ImageToHTML(Bitmap img)
        {
            var ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var img64 = Convert.ToBase64String(ms.GetBuffer());
            string imageInHTML = $"data:image/png;base64,{img64}";
            return imageInHTML;
        }


        /// <summary>
        /// Заполнение шаблона значениями из свойств объекта.
        /// Используется для заполнения полей HTML данными пациентов, результатами тестов и т.п.
        /// В шаблоне производится замена строк вида [префикс][имя поля]_[имя класса][постфикс]
        /// Например при source типа Point и префиксом с постфиксом по умолчанию,
        /// из объекта будут извлекаться значения ####X_Point### и ###Y_Point###
        /// </summary>
        /// <param name="html">шаблон</param>
        /// <param name="source">объект, из свойств которого будут взяты значения</param>
        /// <param name="prefix">префикс</param>
        /// <param name="postfix">постфикс</param>
        /// <returns></returns>
        public static string FillTemplate(string html, object source, string prefix="###", string postfix="###")
        {
            var ret = html;
            var t = source.GetType();
            foreach(var p in t.GetProperties())
            {
                if(p.CanRead)
                {
                    var v = p.GetValue(source).ToString();
                    var name = prefix+ p.Name + "_" + t.Name+postfix;
                    ret = ret.Replace(name, v);
                }
            }
            return ret;
        }
    }
    /// <summary>
    /// Изображение, хранимое в БД.
    /// Для отражения в БД есть свойство Data, выдающее файл картинки в виде массива байтов.
    /// </summary>
    public class img
    {
        /// <summary>
        /// Формат сохранения новых картинок в БД
        /// </summary>
        public static System.Drawing.Imaging.ImageFormat Format = System.Drawing.Imaging.ImageFormat.Jpeg;
        /// <summary>
        /// Сама картинка
        /// </summary>
        [SynEngine.SQLiteHelper.NoHelp]
        public Bitmap Bitmap { get; set; }
        public uint ID { get; set; }
        /// <summary>
        /// Искусственное свойство для конвертации картинки в форматированный массив байтов и обратно (например JPG).
        /// </summary>
        public byte[] Data {
            get
            {
                var ms = new MemoryStream();
                this.Bitmap.Save(ms, Format);
                /*
                var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                var encParams = new EncoderParameters() { Param = new[] { new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L) } };

                this.Bitmap.Save(ms, encoder, encParams);
                */

                return ms.GetBuffer();
            }
            set
            {
                var ms = new MemoryStream(value);
                Bitmap = Bitmap.FromStream(ms) as Bitmap;
            }
        }
        /// <summary>
        /// Описание картинки. Например формализованный идентификатор анализа вида 
        /// ANALIZ|374|1^A
        /// может кодировать изображение колонки первой колонки (А) в анализе с ID=374
        /// </summary>
        public string Info { get; set; }
    }
}
