using SynEngine.General;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static SynEngine.MySQLHelper;

namespace EL2Maket.HL7
{
    /// <summary>
    /// Пакет направления вместе с данными пациента и результатами
    /// </summary>
    public class oru
    {
        public override string ToString()
        {
            return $"[oru] {PID.FIO}->{OBR.TestType}#{OBR.Probirka}";
        }
        public uint ID { get; set; }
        public static oru FromResults(string[] oBXResultsSa)
        {
            throw new Exception();
            return new oru();
        }
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
		public uint SecondID { get => PID.SecondID; set => PID.SecondID = value; }
        /*        public PID PID { get; set; }
                public OBR OBR { get; set; }*/
        public OBX[] OBX = new HL7.OBX[0];

        /// <summary>
        /// OrderGroup->NTE-1 - примечания к заявке на исследования.
        /// В одной из строк примечания может храниться информация об использовании пробирок гелевой карты и о статусах валидации.
        /// Это свойство не пробрасывается в БД напрямую. Используется свойство NTE1ListHL7
        /// </summary>
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
		public List<string> NTE1 = new List<string>();

		/// <summary>
		/// Проброс в БД примечаний к заявке на исследования (NTE1 напрямую не пробрасывается т.к. это массив строк)
		/// </summary>
		public string NTE1ListHL7
		{
			get
			{
				string ret = "";								//	Возвращаемое значение накапливается сложением
				foreach (var o in NTE1)							//	всех строк примечаний из массива примечаний NTE1
					ret += (ret.Length > 0 ? "\n" : "") + o;	//	итоговая длинная строка
				return ret;										//	выдаётся из функции
			}
			set
			{
				NTE1 = new List<string>();					//	При получении строки с новым списком примечаний сбрасываем старый список	
				foreach (var s in value.Split(				//	и разделяем полученную строку на подстроки по символу перевода строки.
					new char[] { '\n' },					//
					StringSplitOptions.RemoveEmptyEntries))	//	Пустые комменты отбрасываются.
						NTE1.Add(s);						//	Заполненные комменты собираются в список.
			}
		}

        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public string Rezultat
        {
            get
            {
                string ret = "";
                if (OBX != null)
                    foreach (var obx in OBX)
                    {
                        var d = obx.Rezultat;
                        var parts = d.Split(new char[] { '^' });
                        if (parts.Length > 1)
                            d = parts[1];
                        ret = ret+(ret.Length>0?" ":"")+d;
                    }
                return ret;
            }
        }
        /// <summary>
        /// Изображения, полученные в ходе видеорегистрации анализа и обработки полученных данных.
        /// </summary>
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public Dictionary<string, Bitmap> Images = new Dictionary<string, Bitmap>();

		/// <summary>
		/// Проброс в БД результатов исследований (OBX напрямую не пробрасывается т.к. это список сложных объектов)
		/// </summary>
		public string OBXListHL7
        {
            get
            {
				var строки = OBX;
				string ret = "";
				foreach (var o in строки)
                    ret += (ret.Length > 0 ? "\n" : "") + o.ToHL7String();
                return ret;
            }
            set
            {
                var obxs = new List<OBX>();
                foreach (var s in value.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    obxs.Add(HL7.OBX.FromHL7String(s));
                OBX = obxs.ToArray();
            }
        }
        /* }
         /// <summary>
         /// Пакет запроса анализов (направление)
         /// </summary>
         public class ORM
         {*/
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public pid PID = new pid();
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public OBR OBR = new OBR();
        public string PIDinHL7 { get => PID.ToHL7String(); set => PID = pid.FromHL7String(value); }
        public string NomerZayavki { get; set; }
        public string Karta { get => PID.Karta; set => PID.Karta = value; }
        public string TestType { get => OBR.TestType; set => OBR.TestType = value; }
        public DateTime DataZaprosa { get => OBR.DataZaprosa; set => OBR.DataZaprosa=value; }
        public DateTime DataIspolneniya { get => OBR.DataIspolneniya; set => OBR.DataIspolneniya = value; }
        public string ResultStatus { get => OBR.ResultStatus; set => OBR.ResultStatus=value; }
		/// <summary>
		/// Какой сотрудник выполнил это исследование (должно пробрасываться из PV1, но пока PV1 у нас не обслуживается)
		/// </summary>
		public string Staff { get; set; }

        /// <summary>
        /// Буферизированные имена гелевых карт
        /// </summary>
        public static Dictionary<string, string> namesOfTestTypes;
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public string TestName { get
            {
                if(namesOfTestTypes==null)
                {
                    namesOfTestTypes = new Dictionary<string, string>();
                    foreach(var t in
                            DB.GetMethods().Select
                            (m => Tuple.Create<string,string>(m.TestType, m.TestName)))
                        namesOfTestTypes.Add(t.Item1, t.Item2);
                }
                if (!namesOfTestTypes.ContainsKey(TestType)) return "ОШИБКА БД";
                return namesOfTestTypes[TestType];
            } }

        /// <summary>
        /// Буферизированные описания тестов
        /// </summary>
        public static Dictionary<string, string> descriptionsOfTestTypes;
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public string TestDescription
        {
            get
            {
                if (descriptionsOfTestTypes == null)
                {
                    descriptionsOfTestTypes = new Dictionary<string, string>();
                    foreach (var t in
                            DB.GetMethods().Select
                            (m => Tuple.Create<string, string>(m.TestType, m.Description)))
                        descriptionsOfTestTypes.Add(t.Item1, t.Item2);
                }
                if (!descriptionsOfTestTypes.ContainsKey(TestType)) return "ОШИБКА БД";
                return descriptionsOfTestTypes[TestType];
            }
        }

		public static string memoNoteTag = "MEMO^";
		public static string pipesNoteTag = "PIPES^";


		public string Probirka { get => OBR.Probirka; set => OBR.Probirka = value; }

        /// <summary>
        /// Примечание к исследованию хранится в NTE1 среди других записей (note).
        /// Примечание к исследованию отличается тем, что начинается с префикса, заданного в memoNoteTag
        /// </summary>
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
		public string ПримечаниеКИсследованию
		{
			get
			{
				var поискПримечанийКИсследованию = NTE1.Where(п => п.Contains(memoNoteTag));
				if (поискПримечанийКИсследованию.Count() == 0)
					return "";
				return поискПримечанийКИсследованию.First().Substring(memoNoteTag.Length);
			}
			set
			{
				NTE1.RemoveAll(p => p.Contains(memoNoteTag));
				NTE1.Add(memoNoteTag + value);
			}
		}

		public static oru FromHL7String(string OBRString)
        {
            var parts = OBRString.Split(new char[] { '|' });
            var ret=new oru()
            {
                NomerZayavki=parts[2]
            };
            ret.OBR.TestType = parts[3];
            return ret;
        }

    }


    public class OBX
    {
        /// <summary>
        /// Date/time of the observation F.14
        /// </summary>
        public DateTime DataIspolneniya { get; set; }
        /// <summary>
        /// Observation value F.5
        /// </summary>
        public string Rezultat { get; set; }
        /// <summary>
        /// Units F.6
        /// </summary>
        public string Edinica { get; set; }
        /// <summary>
        /// Observation Method F.17
        /// </summary>
        public string TestType { get; set; }
        /// <summary>
        /// Set ID OBX F.1
        /// </summary>
        public int ObxIndexInOru { get; set; }
        /// <summary>
        /// Observation result status F.11
        /// </summary>
        public string ResultStatus { get; set; }
        /// <summary>
        /// Observation identifier F.3
        /// </summary>
        public string ResultType { get; set; }
        /// <summary>
        /// Responsable observer F.16
        /// </summary>
        public string Observer { get; set; }
        /// <summary>
        /// Equipment instance identifier
        /// </summary>
        public string DeviceID { get; set; }

        public override string ToString()
        {
            return $"RESULT {ResultType} = {Rezultat} {Edinica}";
        }

        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public string ReadableResult { get
            {
                var parts = Rezultat.Split(new char[] { '^' }, StringSplitOptions.None);
                if (parts.Length > 1)
                    return (TestType.Length>0?ReadableTestType+": ":"")+parts[1];
                else
                    return Rezultat;
            } }

        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public string ReadableTestType {
            get
            {
                var parts = TestType.Split(new char[] { '^' }, StringSplitOptions.None);
                if (parts.Length > 1)
                    return parts[1];
                else
                    return TestType;
            }
        }


        public string ToHL7String(int n=-1)
        {


            //            s += $"OBX|{obxI + 1}||||{dir.OBX[obxI].Rezultat}||||||{dir.ResultStatus}|||{dir.OBX[obxI].DataIspolneniya.ToString("yyyyMMddHHmmss")}\n";
            int index = ObxIndexInOru;
            if (index == 0) index = n;

            return $"OBX|{(n>=0?n.ToString(): "")}"+
                    $"|CE|{ResultType}||{Rezultat}|{Edinica}|||||{ResultStatus}|||{DataIspolneniya.ToString("yyyyMMddHHmmss")}||{Observer}|{TestType}|{DeviceID}";
        }
        public static OBX FromHL7String(string OBRString)
        {
            var parts = OBRString.Split(new char[] { '|' }).ToList();
            while (parts.Count < 19)    //  Дополним подстроки пустышками так чтобы парсинг не упал
                parts.Add("");          //  если в строке окажется маловато частей

            if (parts[14].Length == 0) parts[14] = DateTime.MinValue.ToHL7String();

            return new OBX()
            {
                ObxIndexInOru = parts[1].Length > 0 ? int.Parse(parts[1]) : 0,
                ResultType = parts[3],
                Rezultat = parts[5],
                Edinica = parts[6],
                ResultStatus = parts[11],
                DataIspolneniya = parts[14].HL7DateToDateTime(),
                Observer = parts[16],
                DeviceID = parts[18],
                TestType = parts[17]
            };
        }

    }


    public class OBR
    {
        public string TestType { get; set; }
        public DateTime DataZaprosa { get; set; }
        public DateTime DataIspolneniya { get; set; }
        /// <summary>
        /// OBR.25 - Result Status (https://phinvads.cdc.gov/vads/ViewCodeSystem.action?id=2.16.840.1.113883.12.85)
        /// Статусы, в том числе:
        /// O - результатов пока нет (назначено)
        /// R - результаты не проверены (валидация)
        /// I - образец в лаборатории, ожидаются результаты (в работе)
        /// F - окончательный результат (валидирован)
        /// X - результат получить не удалось (ошибка)
        /// </summary>
        public string ResultStatus { get; set; }
        public string Probirka { get; set; }

        public string ToHL7String()
        {
            //            s += $"OBR||{dir.Probirka}||{dir.OBR.TestType}^{method.TestName}\n";    //  Экспорт OBR

            return $"OBR||{Probirka}||{TestType}";//||{DataZaprosa.ToHL7String()}|{DataIspolneniya.ToHL7String()}|||||||||||||||||{ResultStatus}";
        }

        public OBR ()
        {
            ResultStatus = "O";
        }

        public static OBR FromHL7String(string OBRString)
        {
            var parts = OBRString.Split(new char[] { '|' }).ToList();
            while (parts.Count < 25)    //  Дополним подстроки пустышками так чтобы парсинг не упал
                parts.Add("");          //  если в строке окажется маловато частей

            return new OBR()
            {
                /*                DataZaprosa = parts[6].HL7DateToDateTime(),
                                DataIspolneniya = parts[7].HL7DateToDateTime(),
                                ResultStatus = parts[24]*/
                TestType = parts[4],
                Probirka = parts[2]
            };
        }

    }

    public class pid
    {
        public override string ToString()
        {
            return $"[pid] {Karta} {FIO}";
        }
        public uint ID { get; set; }
		/// <summary>
		/// Дополнительный невидимый идентификатор пациента
		/// </summary>
		public uint SecondID { get; set; }

		public string Karta { get; set; }
        public string Familiya { get; set; }
        public string Imya { get; set; }
        public string Otchestvo { get; set; }
        public string Sex { get; set; }

        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public string FIO { get { return $"{Familiya} {Imya} {Otchestvo}"; } }
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public string FIOinHL7 { get { return $"{Familiya}^{Imya}^{Otchestvo}"; } }
        public DateTime Birthday { get; set; }
        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public string Vozrast { get { return ((int)(DateTime.Now.Subtract(Birthday).TotalDays / 364.25)).ToString(); } }

        public string HL7 { get { return ToHL7String(); } set {
                var newPidFromHL7 = FromHL7String(value);
                newPidFromHL7.CopyTo(this,new string[] { "HL7" });
            } }

        [SynEngine.SQLiteHelper.NoHelp]
        //[NoHelp]
        public string ToHL7String()
        {
            return $"PID|||{Karta}|{(SecondID>0?SecondID.ToString():"")}|{FIOinHL7}||{Birthday.ToString("yyyyMMdd")}|{Sex}";
        }
        public static pid FromHL7String(string PIDString)
        {
            var parts = PIDString.Split(new char[] { '|' }).ToList();

            while (parts.Count() < 10) parts.Add("");
            if (parts[7].Trim().Length == 0)
                parts[7] = new DateTime().ToString("yyyyMMdd");

                var fioparts = parts[5].Split(new char[] { '^' });
                string datePart = parts[7];
				return new pid()
				{
					Familiya = fioparts.Length == 3 ? fioparts[0] : parts[5],
					Imya = fioparts.Length == 3 ? fioparts[1] : "",
					Otchestvo = fioparts.Length == 3 ? fioparts[2] : "",
					Birthday = datePart.HL7DateToDateTime(),
					Karta = parts[3],
                    Sex=parts[8],
					SecondID = parts[4].Length > 0 ? uint.Parse(parts[4]) : 0
                };
        }


    }

    public static class Extensions
    {
		public static void УдалитьИзображения(this oru [] oo)
		{
			oo.ToList().ForEach(o => o.Images.Clear());
		}
		public static void ДобавитьУказаниеРеагентаККолонке(this oru dir, int номерКолонкиПоГК, string реагент)
		{
			dir.NTE1[dir.NTE1.ПодстрокаПоПрефиксу(oru.pipesNoteTag)] += $"{номерКолонкиПоГК}:{реагент}^";
		}

		public static void УдалитьИнформациюОСоответствииКолонок(this oru [] Направления)
		{
			/// префикс строки NTE-1, хранящей информацию о соответствии колонок и реагентов для данного исследования
			foreach (var n in Направления)                          //	Из всех направлений фотопаратика
				n.NTE1.RemoveAll(s => s.StartsWith(oru.pipesNoteTag));  //	удалим информацию о соответствии колонок (далее впишем заново)
		}

		public static string [] Пробирки(this oru [] oo)
		{
			return oo.Select(o => o.Probirka).ToArray();
		}
		public static string[] Примечания(this oru[] oo)
		{
			return oo.Select(o => o.ПримечаниеКИсследованию).ToArray();
		}
		public static DateTime HL7DateToDateTime(this string datePart)
        {
            var dt = DateTime.MinValue;
            if (datePart.Trim().Length==0 || DateTime.TryParse(datePart, out dt))
                return dt;

            var parts = datePart.Split(new char[] { '/', ' ', ':' });
            if (parts.Length == 7) return new DateTime(int.Parse(parts[2]), int.Parse(parts[0]), int.Parse(parts[1]));

            if(datePart.Contains(" "))
                datePart = datePart.Substring(0, datePart.IndexOf(" "));
            if(@datePart[2]=='.')
                return new DateTime(int.Parse(datePart.Substring(6, 4)), int.Parse(datePart.Substring(3, 2)), int.Parse(datePart.Substring(0, 2)));
            return new DateTime(int.Parse(datePart.Substring(0, 4)), int.Parse(datePart.Substring(4, 2)), int.Parse(datePart.Substring(6, 2)));
        }
        public static string[] StartingWith(this string [] lines, string prefix)
        {
            return lines.Select((r => r.ToString())).ToArray().Where(rS => rS.StartsWith(prefix)).ToArray();
        }

        public static string ToHL7String(this DateTime dt)
        {
            return dt.ToString("yyyyMMdd");
        }

        /// <summary>
        /// Определяет методику, указанную в списке исследований.
        /// Exception выбрасывается если в списке оказываются исследования на разные методики или если список пустой.
        /// </summary>
        /// <param name="dirs">Направления, по которым нужно выяснить методику</param>
        /// <returns>Полная информация о методике</returns>
        public static DB.Method GetSingleMethod(this List<oru> dirs)
        {
            if (dirs.Count == 0) return null;
            var methods = dirs.Select(d => d.TestType).Distinct();
            if (methods.Count() != 1) throw new Exception("Неверное количество разных методов на сканирование: " + methods.Count());
            var method = DB.GetMethod(methods.Single());
            return method;
        }
    }
}
