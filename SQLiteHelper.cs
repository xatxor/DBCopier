/// Переключатель режима соединения. Когда RECONNECT_MODE определён, система при взаимодействии с БД соединяется каждый раз заново.
#define RECONNECT_MODE

using System;
using System.Collections.Generic;
using System.Text;
//using MySql.Data.MySqlClient;
using System.Reflection;
using System.Data;
using System.Linq;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace DBCopier
{
    // Для русского Order by
    [SQLiteFunction(FuncType = FunctionType.Collation, Name = "RUSSIAN_COLLATION")]
    public class SQLiteRussianCollation : SQLiteFunction
    {
        /// <summary />
        /// CultureInfo for comparing strings in case insensitive manner 
        /// </summary />
        private static readonly CultureInfo _cultureInfo =
            CultureInfo.CreateSpecificCulture("ru-RU");

        public override int Compare(string x, string y)
        {
            return String.Compare(x, y, true, _cultureInfo);
        }
    }

    // Для русского Upper
    [SQLiteFunction(FuncType = FunctionType.Scalar, Name = "Upper_Ru", Arguments = 1)]
    public class SQLiteRussianUpper : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Convert.ToString(args[0]).ToUpper();
        }
    }

    // Для русского Lower
    [SQLiteFunction(FuncType = FunctionType.Scalar, Name = "Lower_Ru", Arguments = 1)]
    public class SQLiteRussianLower : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Convert.ToString(args[0]).ToLower();
        }
    }

    public partial class SQLiteHelper
    {
        public bool ReplaceRowid { get; set; } = true;
        public bool UsePostfix { get; set; } = true;

        static SQLiteHelper()
        {
            SQLiteFunction.RegisterFunction(typeof(SQLiteRussianCollation));
            SQLiteFunction.RegisterFunction(typeof(SQLiteRussianUpper));
            SQLiteFunction.RegisterFunction(typeof(SQLiteRussianLower));
        }

        public SQLiteHelper(string fileName = "SynDB.sqlite", bool replaceRowID=true, bool usePostfix=true)
        {
            ConnStr = $"Data Source={fileName};Version=3;";
            ReplaceRowid = replaceRowID;
            UsePostfix = usePostfix;
        }
        public class NoHelp : Attribute
        {
        }

        // Connection string is taken from Web.config, named "ConnString"
        string connStr = null;
        public string ConnStr
        {
            get
            {
                // Строка соединения для локальной версии используется если не удаётся загрузить её из WebConfig
                if(connStr==null)
                    connStr = "Data Source=SynDB.sqlite;Version=3;";
                return connStr;
            }
            set
            {
                connStr = value;
            }
        }


        static string SQLiteLastInsertIDcommand = "SELECT last_insert_rowid()";
        public static uint GetLastInsertID(SQLiteConnection con)
        {
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = SQLiteLastInsertIDcommand;
            object responce = cmd.ExecuteScalar();
            cmd.Dispose();
            return uint.Parse(responce.ToString());
        }

		/// <summary>
		/// Выполняет запрос SQL и не ожидает результатов (ExecuteNonQuery)
		/// </summary>
		/// <param name="cmdText"></param>
		public void Exec(string cmdText)
		{
			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = cmdText;
				cmd.ExecuteNonQuery();
			}
		}

        /// <summary>
        /// Выполняет запрос SQL и ожидает результатов (ExecuteScalar)
        /// </summary>
        /// <param name="cmdText"></param>
        public object ExecScalar(string cmdText)
        {
            object obj = null;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = cmdText;
                obj = cmd.ExecuteScalar();
            }
            return obj;
        }


        /// <summary>I
        /// Создание соединения с БД
        /// </summary>
        /// <returns>Соединение с БД</returns>
        public SQLiteConnection CreateConnection()
        {
            /*string connStr = string.Format(
                "datasource={0};username={1};password={2};database={3}",
                address, login, pass, dbName);/**/
            return new SQLiteConnection(ConnStr);
        }

        SQLiteConnection _con = null;
        public SQLiteConnection con { get
            {
                if (_con == null)
                {
                    _con = CreateConnection();
                    _con.Open();
                }
                return _con;
            } }
        public void CloseConnection()
        {
            con.Close();
            _con = null;
        }
        /// <summary>
        /// Создание новой команды в новом соединении
        /// </summary>
        /// <returns>Новая команда в новом соединении</returns>
        public SQLiteCommand CreateCommand()
        {
            while (true)
            {
                try
                {
#if RECONNECT_MODE
					if (con != null) CloseConnection();
#endif
					if (con.State != ConnectionState.Open)
						con.Open();
                    SQLiteCommand cmd = con.CreateCommand();
                    return cmd;
                }
                catch 
                {	//	хитрая попытка восстановить соединение с БД.
					try { _con.Close(); } catch { }
					try
					{

						_con = null;

						SQLiteCommand cmd = con.CreateCommand();
						return cmd;
					}
					catch(Exception ex) {
						return null;
					//	continue;
					}
                }
            }
        }
        /// <summary>
        /// Удаление команды вместе с соединением
        /// </summary>
        /// <param name="cmd">Команда, которую нужно удалить вместе с соединением</param>
        public void DisposeCommand(SQLiteCommand cmd)
        {
      //      cmd.Connection.Close();
      //      cmd.Connection.Dispose();
            cmd.Dispose();
        }

        public T FillObjectFromDataReader<T>(SQLiteDataReader r) where T : class
        {
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(new Type[] { });
            object o = constructor.Invoke(null);

            return FillObjectFromDataReader(o, r) as T;
        }

        public object FillObjectFromDataReader(object o, SQLiteDataReader r)
        {
            string  postfix = UsePostfix?"_" + GetTableForObject(o):"";

            foreach (PropertyInfo info in HelpedProperties(o))
            {
                if (!info.CanWrite) continue;
                string columnName = GetColumnForProperty(info);//  info.Name + (info.Name.Contains("_") ? "" : postfix);
                if (info.Name.Equals("ID") && ReplaceRowid) columnName = "rowid";
                object dbObj = r[columnName];

                if (info.PropertyType.BaseType.Name == "Enum")
                    dbObj = Enum.Parse(info.PropertyType, dbObj.ToString());
                else
                    switch (info.PropertyType.Name)
                    {
                        case "DbID":
                            dbObj = info.PropertyType.GetMethod("Parse").Invoke(null, new object[] { dbObj.ToString() });
                            break;
                        case "UInt32":
                            if (dbObj.GetType().Equals(typeof(Int64)))
                                dbObj = (UInt32)((Int64)dbObj);
                            else if (dbObj.GetType().Equals(typeof(UInt64)))
                                dbObj = (UInt32)((UInt64)dbObj);
                            else if (dbObj.GetType().Equals(typeof(Int32)))
                                dbObj = (UInt32)((Int32)dbObj);
                            break;
                        case "Byte[]":
                            //throw new Exception("Helper can't work with byte arrays yet.");
                            dbObj = (Byte[])dbObj;
                            break;
                        case "String":
                            dbObj=dbObj.ToString();
                            break;
                        case "Int32":
                            if (dbObj.GetType().Equals(typeof(Int64)))
                                dbObj=(Int32)((Int64)dbObj);
                            else if (dbObj.GetType().Equals(typeof(Decimal)))
                                dbObj = (Int32)((Decimal)dbObj);
                            else if (dbObj.GetType().Equals(typeof(UInt64)))
                                dbObj = (Int32)((UInt64)dbObj);
                            break;
                        case "Double":
                            if (dbObj.GetType().Equals(typeof(float)))
                                dbObj = (double)((float)dbObj);
                            else if (dbObj.GetType().Equals(typeof(Decimal)))
                                dbObj = (double)((Decimal)dbObj);
                            break;
                        case "Single":
                        case "Float":
                            if (dbObj.GetType().Equals(typeof(double)))
                                dbObj = (float)((double)dbObj);
                            else if (dbObj.GetType().Equals(typeof(Decimal)))
                                dbObj = (float)((Decimal)dbObj);
                            break;
                        case "Boolean":
                            if (dbObj is DBNull)
                                    dbObj = false;
                            if (dbObj is Byte)
                                dbObj = ((Byte)dbObj != 0);
                            if (dbObj is sbyte)
                                dbObj = ((sbyte)dbObj != 0);
                            break;
                        case "DateTime":
                            if (dbObj is DBNull)
                                dbObj = DateTime.MinValue;
                            else
                                dbObj = ToDateTime(dbObj.ToString());
                            break;
                        default:
                            Console.WriteLine("Loading type " + o.GetType().Name + " can't interpret property " + info.Name);
                            dbObj = null;
                            break;
                    }
                info.SetValue(o, dbObj);
            }
            return o;
        }

        public static DateTime ToDateTime(string value)
        {
            string format = "yyyy-MM-dd---HH-mm-ss-fff";

            return DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }

        private static IEnumerable<PropertyInfo> HelpedProperties(object objType)
        {
            return HelpedPropertiesForType(objType.GetType());
        }

        private static IEnumerable<PropertyInfo> HelpedPropertiesForType(Type objType)
        {
            return objType.GetProperties().Where(p => p.GetCustomAttribute(typeof(NoHelp)) == null);
        }

        public int UpdateObjectInTable(object obj, string tableName)
        {
            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "UPDATE " + tableName + " SET ";

            string idFieldName = "rowid";
            uint idFieldValue = 0;
            StringBuilder sets = new StringBuilder();
            foreach (PropertyInfo info in HelpedProperties(obj))
            {
                if (info.Name.ToUpper().StartsWith("ID_"))
                {   //  перехват поля идентификатора записи
                   // idFieldName = info.Name;
                    idFieldValue = (uint)info.GetValue(obj);
                }
                else
                {   // обработка остальных полей записи
                    if (sets.Length > 0) sets.Append(", "); else sets.Append(" ");
                    sets.Append(info.Name + " = @" + info.Name);
                    cmd.Parameters.AddWithValue("@" + info.Name, info.GetValue(obj));
                }
            }
            cmd.CommandText += sets.ToString();
            cmd.CommandText += " WHERE " + idFieldName + "=" + idFieldValue;
            int ret = cmd.ExecuteNonQuery();

            DisposeCommand(cmd);
            return ret;
        }

        /// <summary>
        /// Проверка наличия таблицы по типу в БД
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool TableExists(Type t)
        {
            string tableName = GetTableForType(t);
            SQLiteCommand cmd = CreateCommand();

            try
            {
                List(t, "");

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                DisposeCommand(cmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateTable(Type t)
        {
            string tableName = GetTableForType(t);
            SQLiteCommand cmd = CreateCommand();

            string addColumn = "";

            foreach (PropertyInfo pi in HelpedPropertiesForType(t))
            {
                if (pi.Name == "ID") continue;
                if (addColumn.Length != 0)
                    addColumn += ", " + pi.Name + "_" + tableName;
                else addColumn += pi.Name + "_" + tableName;

                if (pi.PropertyType.BaseType != null && pi.PropertyType.BaseType.Name == "Enum")
                {
                    addColumn += " TEXT COLLATE RUSSIAN_COLLATION";
                }
                else
                    switch (pi.PropertyType.Name)
                    {
                        case "DbID":
                        case "UInt32":
                        case "Int32":
                        case "Boolean":
                            addColumn += " NUMERIC";
                            break;
                        case "String":
                        case "DateTime":
                            addColumn += " TEXT COLLATE RUSSIAN_COLLATION";
                            break;
                        case "Double":
                        case "Float":
                            addColumn += " REAL";
                            break;
                        case "Byte[]":
                            addColumn += " BLOB";
                            break;
                        default:
                            throw new Exception("Helper can't work with this.");
                    }
            }
            cmd.CommandText = "CREATE TABLE " + tableName + " ({0}) ";
            cmd.CommandText = string.Format(cmd.CommandText, addColumn);

            cmd.ExecuteNonQuery();
            
            DisposeCommand(cmd);
        }

        /// <summary>
        /// Запись объекта в БД. Если объект с таким ID уже есть, то он обновляется. Если ID==0 или ID==null, то создаётся новый
        /// Возвращается ID в том типе, в котором он имеется в классе хранимого объекта.
        /// Также ID записывается в соответствующее поле самого объекта
        /// </summary>
        /// <param name="obj">Объект, который нужно сохранить</param>
        /// <returns>ID сохранённого объекта</returns>
        public object Save(object obj)
        {
            object idFieldValue = GetID(obj);

            if (idFieldValue == null || (idFieldValue != null && idFieldValue.ToString().Equals("0")))
                return Insert(obj);
            else Update(obj);
            return idFieldValue;
        }

        public int Update(object obj)
        {
            string tableName = GetTableForObject(obj);
            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "UPDATE " + tableName + " SET ";

            uint idFieldValue = 0;
            StringBuilder sets = new StringBuilder();
            foreach (PropertyInfo info in HelpedProperties(obj))
            {
                string columnName = GetColumnForProperty(info);
                if (columnName.Equals("ID_" + tableName))
                {   //  перехват поля идентификатора записи
                    idFieldValue = uint.Parse(info.GetValue(obj).ToString());
                    //idFieldValue = (uint)info.GetValue(obj);
                }
                else
                {   // обработка остальных полей записи
                    if (sets.Length > 0) sets.Append(", "); else sets.Append(" ");
                    sets.Append(columnName + " = @" + columnName);
                    if (info.PropertyType.Equals(typeof(DateTime)))
                        cmd.Parameters.AddWithValue("@" + columnName, ((DateTime)info.GetValue(obj)).ToString("yyyy-MM-dd---HH-mm-ss-fff"));
                    else cmd.Parameters.AddWithValue("@" + columnName, info.GetValue(obj));
                }
            }
            cmd.CommandText += sets.ToString();
            cmd.CommandText += " WHERE rowid=" + idFieldValue;
            int ret = cmd.ExecuteNonQuery();


            DisposeCommand(cmd);
            return ret;
        }

        /// <summary>
        /// Запись объекта в БД. После записи поле ID обновляется в соответствии с идентификатором новой записи.
        /// </summary>
        /// <param name="obj">Сохраняемый объект</param>
        /// <param name="tableName">имя таблицы</param>
        /// <returns>ID новой записи в БД</returns>
        public uint InsertObjectIntoTable(object obj, string tableName)
        {
            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "INSERT INTO " + tableName + "({0}) values ({1})";

            StringBuilder fieldNames = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (PropertyInfo info in HelpedProperties(obj))
            {
                if (info.Name.ToUpper().StartsWith("ID_"))
                    continue;   //  пропуск поля идентификатора записи
                else
                {   // обработка остальных полей записи
                    if (fieldNames.Length > 0) fieldNames.Append(", ");
                    fieldNames.Append(info.Name);
                    if (values.Length > 0) values.Append(", ");
                    values.Append("@" + info.Name);
                    cmd.Parameters.AddWithValue("@" + info.Name, info.GetValue(obj));
                }
            }
            cmd.CommandText = string.Format(cmd.CommandText, fieldNames.ToString(), values.ToString());
            cmd.ExecuteNonQuery();
            uint ret = SQLiteHelper.GetLastInsertID(cmd.Connection);
            DisposeCommand(cmd);
            return ret;
        }

        public int MarkObjectDeletedInTable(object obj, string tableName)
        {
            string tableName2 = GetTableForObject(obj);
            if (!tableName.Equals(tableName2)) throw new Exception("BAD TYPE NAME " + obj.GetType().Name);

            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "UPDATE " + tableName + " SET ";

            //string idFieldName = "";
            //uint idFieldValue = 0;

            var idFieldValue = GetID(obj);

            //foreach (PropertyInfo info in HelpedProperties(obj))
            //{
            //    if (info.Name.ToUpper().StartsWith("ID_"))
            //    {   //  перехват поля идентификатора записи
            //        idFieldName = info.Name;
            //        idFieldValue = (uint)info.GetValue(obj);
            //    }
            //}

            cmd.CommandText += "deleted_"+tableName2 + "=1 WHERE rowid=" + idFieldValue;
            int ret = cmd.ExecuteNonQuery();

            DisposeCommand(cmd);
            return ret;
        }



        /// <summary>
        /// Преобразование списка объектов из БД в словарь таких объектов, где ключ - ID объекта в БД
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Dictionary<object, T> ListToDictionary<T>(List<T> list) where T : class
        {
            Dictionary<object, T> ret = new Dictionary<object, T>();

            PropertyInfo idField = GetIDProperty<T>();

            if (idField == null) throw new Exception("Can't convert List of " + typeof(T).ToString() + " to Dictionary - ID field not found!");

            foreach (var o in list)
                ret.Add((object)idField.GetValue(o), o);

            return ret;
        }

        private static PropertyInfo GetIDProperty<T>() where T : class
        {
            return GetIDPropertyOfType(typeof(T));
        }

        private static PropertyInfo GetIDPropertyOfType(Type t)
        {
            return HelpedPropertiesForType(t).Where(p => p.Name.Equals("ID_" + GetTableForType(t))).Single();
        }

        /// <summary>
        /// Удаление объектов в БД
        /// </summary>
        /// <typeparam name="T">Тип объекта. Имя таблицы в БД должно совпадать с именем типа</typeparam>
        /// <param name="wheres">Строка ограничений для передачи в параметр запроса WHERE</param>
        /// <returns></returns>
        public void Delete<T>(string wheres, params object[] p) where T : class
        {
            DateTime t = DateTime.Now;
            List<T> ret = new List<T>();
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(new Type[] { });
            string table = GetTableForType(type);

            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "DELETE FROM " + table + (wheres.Length > 0 ? ("  WHERE " + wheres) : "");
            ApplyParamsToCmd(p, cmd);

            SQLiteDataReader r = cmd.ExecuteReader();
            double ms = DateTime.Now.Subtract(t).TotalMilliseconds;
            t = DateTime.Now;
            while (r.Read())
            {
                ret.Add(FillObjectFromDataReader(constructor.Invoke(null), r) as T);
                ms = DateTime.Now.Subtract(t).TotalMilliseconds;
                t = DateTime.Now;
            }
            r.Close();

            DisposeCommand(cmd);
        }

        /*
        /// <summary>
        /// Загрузка списка объектов из БД
        /// </summary>
        /// <typeparam name="T">Тип объекта. Имя таблицы в БД должно совпадать с именем типа маленькими буквами</typeparam>
        /// <param name="wheres">Строка ограничений для передачи в параметр запроса WHERE. По умолчанию ""</param>
        /// <returns>Список объектов, загруженных из БД</returns>
        public static T[] Load<T>(string wheres="") where T : class
        {
            string table = GetTableForType(typeof(T));
            var cmdtext = "SELECT rowid,* FROM " + table + (wheres.Length > 0 ? ("  WHERE " + wheres) : "");
            var ret = LoadByQuery<T>(cmdtext);

            return ret;
        }*/

        public  T [] LoadByQuery<T>(string cmdtext) where T : class
        {
            List<T> ret = new List<T>();
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(new Type[] { });

            SQLiteCommand cmd = CreateCommand();
            cmd.CommandText = cmdtext;

            SQLiteDataReader r = cmd.ExecuteReader();

            while (r.Read())
                ret.Add(FillObjectFromDataReader(constructor.Invoke(null), r) as T);

            r.Close();

            DisposeCommand(cmd);
            return ret.ToArray();
        }

        /// <summary>
        /// Загрузка списка идентификаторов таблицы из БД
        /// </summary>
        /// <param name="type">Тип объектов, хранимых в БД</param>
        /// <param name="wheres">Параметры выборки</param>
        /// <returns>Список ID объектов в БД</returns>
        public List<object> List(Type type, string wheres)
        {
            List<object> ret = new List<object>();
            var IdType = type.GetProperty("ID").PropertyType;
            var IdParser = IdType.GetMethod("Parse",new Type[]{typeof(string) });
            // Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(new Type[] { });
            string table = GetTableForType(type);

            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = $"SELECT {(ReplaceRowid?"rowid": "ID")} FROM " + table + (wheres.Length > 0 ? ("  WHERE " + wheres) : "");
            SQLiteDataReader r = cmd.ExecuteReader();
            while (r.Read())
                ret.Add(IdParser.Invoke(null,new object[] { r[0].ToString() }));
            r.Close();

            DisposeCommand(cmd);


            return ret;
        }


        /// <summary>
        /// Загрузка списка объектов из БД
        /// </summary>
        /// <typeparam name="T">Тип объекта. Имя таблицы в БД должно совпадать с именем типа маленькими буквами</typeparam>
        /// <param name="wheres">Строка ограничений для передачи в параметр запроса WHERE</param>
        /// <param name="p">Произвольное количество пар параметров ИМЯ-ЗНАЧЕНИЕ, имена должны совпадать с именами, указанными в WHERES</param>
        /// <returns></returns>
        public List<T> Select<T>(string columns = "", string wheres="", params object[] p) where T : class
        {
            List<T> ret = new List<T>();
            Type type = typeof(T);
            string table = GetTableForType(type);

            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "SELECT " /*+ (columns.Length > 0 ? columns : "rowid,*")*/ + "rowid,* FROM " + table + (wheres.Length > 0 ? ("  WHERE " + wheres) : "");

            ApplyParamsToCmd(p, cmd);

            SQLiteDataReader r = cmd.ExecuteReader();
            while (r.Read())
                ret.Add(FillObjectFromDataReader<T>(r));
            r.Close();

            DisposeCommand(cmd);

            return ret;
        }

        /// <summary>
        /// Вставка параметров в команду. Параметры передаются в виде последовательности "ИМЯ", ЗНАЧЕНИЕ, упакованной в массив
        /// </summary>
        /// <param name="p">имена и значения параметров</param>
        /// <param name="cmd">команда, в которую нужно добавить параметры</param>
        private static void ApplyParamsToCmd(object[] p, SQLiteCommand cmd)
        {
            for (int i = p.Length / 2; --i >= 0;)
                cmd.Parameters.AddWithValue(p[i * 2].ToString(), p[i * 2 + 1]);
        }


        /// <summary>
        /// Загрузка объектов из БД по ID
        /// </summary>
        /// <typeparam name="T">Тип объекта. Имя таблицы в БД должно совпадать с именем типа маленькими буквами</typeparam>
        /// <param name="ID">Идентификатор. Поле в БД должно иметь имя ID_ИмяТипа</param>
        /// <returns></returns>
        public T Load<T>(object ID) where T : class
        {
            return LoadByID<T>(ID);
        }

        private T LoadByID<T>(object ID) where T : class
        {
            Type type = typeof(T);
            return Load(ID, type) as T;
        }

        public object Load(object ID, Type type)
        {
            object ret = null;
            ConstructorInfo constructor = type.GetConstructor(new Type[] { });
            string table = GetTableForType(type);

            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = $"SELECT {(ReplaceRowid ? "rowid," : "")}* FROM {table}  WHERE {(ReplaceRowid ? "rowid" : "ID")}=" + ID;
            SQLiteDataReader r = cmd.ExecuteReader();
            if (r.Read())
                ret = FillObjectFromDataReader(constructor.Invoke(null), r);
            r.Close();

            DisposeCommand(cmd);

            return ret;
        }


        public object Insert(object obj)
        {
            string tableName = GetTableForObject(obj);
       //     tableName = tableName.ToLower();

            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "INSERT INTO " + tableName + "({0}) values ({1})";

            StringBuilder fieldNames = new StringBuilder();
            StringBuilder values = new StringBuilder();
            PropertyInfo idPropInfo = null;
            foreach (PropertyInfo info in HelpedProperties(obj))
            {
                string columnName = GetColumnForProperty(info);
                if (info.Name.ToUpper().Equals("ID"))
                {
                    idPropInfo=info;   //  пропуск поля идентификатора записи
                }
                else
                {   // обработка остальных полей записи
                    if (fieldNames.Length > 0) fieldNames.Append(", ");
                    fieldNames.Append(columnName);
                    if (values.Length > 0) values.Append(", ");
                    values.Append("@" + columnName);

                    if (info.PropertyType.Equals(typeof(DateTime)))
                        cmd.Parameters.AddWithValue("@" + columnName, ((DateTime)info.GetValue(obj)).ToString("yyyy-MM-dd---HH-mm-ss-fff"));
                    else cmd.Parameters.AddWithValue("@" + columnName, info.GetValue(obj));
                }
            }
            cmd.CommandText = string.Format(cmd.CommandText, fieldNames.ToString(), values.ToString());
            cmd.ExecuteNonQuery();
            uint ret = SQLiteHelper.GetLastInsertID(cmd.Connection);
            DisposeCommand(cmd);

            //var retObj = idPropInfo.PropertyType.GetMethod("Parse").Invoke(null, new object[] { ret.ToString() });
            var retObj = idPropInfo.PropertyType.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { ret.ToString() });

            idPropInfo.SetValue(obj, retObj);
            return retObj;
        }

        private static string GetTableForObject(object obj)
        {
            return GetTableForType(obj.GetType());
        }
        private static string GetTableForType(Type t)
        {
            return t.Name.ToLower();
        }

        private string GetColumnForProperty(PropertyInfo info)
        {
            string postfix = UsePostfix?"_" + GetTableForType(info.ReflectedType):"";
            return info.Name + (info.Name.Contains("_") ? "" : postfix);
        }

        public int MarkObjectDeletedInDB(object obj)
        {
            string tableName = GetTableForObject(obj);
            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "UPDATE " + tableName + " SET ";

            var idFieldValue = GetID(obj);

            cmd.CommandText += "deleted_" + tableName + "=1 WHERE rowid=" + idFieldValue;
            int ret = cmd.ExecuteNonQuery();

            DisposeCommand(cmd);
            return ret;
        }

        public static object GetID(object obj)
        {
            string idFieldName = "";
            object idFieldValue = null;
            foreach (PropertyInfo info in obj.GetType().GetProperties())
            {
                if (info.Name.ToUpper().StartsWith("ID_") || info.Name.ToUpper().Equals("ID"))
                {   //  перехват поля идентификатора записи
                    idFieldName = info.Name;
                    idFieldValue = info.GetValue(obj);
                    break;
                }
            }
            return idFieldValue;
        }
        public int MarkObjectUndeletedInDB(object obj)
        {
            string tableName = GetTableForObject(obj);
            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "UPDATE " + tableName + " SET ";

            string idFieldName = "";
            uint idFieldValue = 0;
            foreach (PropertyInfo info in HelpedProperties(obj))
            {
                if (info.Name.ToUpper().StartsWith("ID_") || info.Name.ToUpper().Equals("ID"))
                {   //  перехват поля идентификатора записи
                    idFieldName = info.Name;
                    idFieldValue = (uint)info.GetValue(obj);
                    break;
                }
            }

            cmd.CommandText += "deleted_" + tableName + "=0 WHERE rowid=" + idFieldValue;
            int ret = cmd.ExecuteNonQuery();

            DisposeCommand(cmd);
            return ret;
        }

        public int Delete(object obj)
        {
            string tableName = GetTableForObject(obj);
            SQLiteCommand cmd = CreateCommand();

            cmd.CommandText = "DELETE FROM " + tableName + " WHERE rowid=";

            cmd.CommandText += GetID(obj).ToString();
            int ret = cmd.ExecuteNonQuery();

            DisposeCommand(cmd);
            return ret;
        }


#region Поддержка хранилища пар-значений
        /*
            CREATE TABLE `czm`.`kvp` (
            `Key_Kvp` VARCHAR(255) NOT NULL COMMENT 'Ключ',
            `Value_Kvp` BLOB NULL COMMENT 'Значение',
            `Created_Kvp` DATETIME NULL COMMENT 'Момент создания пары ключ-значение',
            `Read_Kvp` DATETIME NULL COMMENT 'Момент последнего чтения пары ключ-значение',
            `Written_Kvp` DATETIME NULL COMMENT 'Момент последней записи пары ключ-значение',
            `ID_Kvp` INT NOT NULL AUTO_INCREMENT COMMENT 'идентификатор пары ключ-значение (для совместимости с основным движком)',
            UNIQUE INDEX `Key_Kvp_UNIQUE` (`Key_Kvp` ASC),
            PRIMARY KEY (`ID_Kvp`, `Key_Kvp`),
            UNIQUE INDEX `ID_Kvp_UNIQUE` (`ID_Kvp` ASC))
            COMMENT = 'Коллекция ключей-значений';
         */
#endregion

        public bool IsDeleted(object obj)
        {
            string tableName = GetTableForObject(obj);
            SQLiteCommand cmd = CreateCommand();
            string delColumnName = "deleted_" + tableName;
            cmd.CommandText = "SELECT " + delColumnName + " FROM " + tableName + " WHERE rowid=";
            string idVal = SQLiteHelper.GetID(obj).ToString();
            cmd.CommandText += idVal;
            var r = cmd.ExecuteReader();
            if (r.Read())
            {
                object o = r[delColumnName];
                return !o.Equals(false);
            }
            throw new Exception("Can't get deleted status for ID=" + idVal + " - ID not found in table " + tableName);
        }


        public void ResetBadCount(string p)
        {
#region Сброс счётчика плохих логинов
            var cmd = CreateCommand();
            cmd.CommandText = "UPDATE users SET FailedPasswordAttemptCount=0 WHERE Username='" + p + "'";
            cmd.ExecuteNonQuery();
            DisposeCommand(cmd);
#endregion
        }

        public int BadCount(string badLogin)
        {
            int badCount = 1;
            try
            {
                var cmd = CreateCommand();
                cmd.CommandText = "select FailedPasswordAttemptCount from users where Username='" + badLogin + "'";
                var r = cmd.ExecuteReader();
                if (r.Read())
                    badCount = int.Parse(r[0].ToString());
                DisposeCommand(cmd);
            }
            catch { }
            return badCount;
        }

        /// <summary>
        /// Загрузка из БД произвольного табличного результата произвольного запроса с параметрами
        /// </summary>
        /// <param name="p">Текст SQL запроса. В тексте запроса параметры перечисляются по именам,
        /// например SELECT * FROM tbl WHERE name=@param1 AND addr=@param2
        /// В этом запросе указаны два параметра: @param1 и @param2</param>
        /// <param name="queryParams">Словарь параметров, где ключи - имена параметров, а значения - значения</param>
        /// <param name="rowCount">Возвращаемое значение - количество строк результатов - актуально при использовании LIMIT</param>
        /// <returns>Двумерный массив, где первое измерение - строка, второе - колонка.
        /// Первая строка массива содержит имена колонок. Остальные строки - результаты запроса.</returns>
        public object[][] Load(string p, Dictionary<string, object> queryParams, out long rowCount)
        {
            bool needRowCount = false;
            rowCount = -1;
            /*  if (p.ToLower().StartsWith("select"))
              {
                  p = "select SQL_CALC_FOUND_ROWS " + p.Substring(6);
                  needRowCount = true;
              }/**/
            needRowCount = true;

            var cmd = CreateCommand();

            cmd.CommandText = p;

            foreach (var key in queryParams.Keys)
            {
                cmd.Parameters.AddWithValue(key, queryParams[key]);
            }

            var r = cmd.ExecuteReader();
            List<List<object>> got = new List<List<object>>();
            if (r.HasRows)
            {
                var schema = r.GetSchemaTable();


                List<object> columnsNames = new List<object>();
                foreach (DataRow columnDescription in schema.Rows)
                    columnsNames.Add("<b>" + columnDescription[0] + "</b>");

                got.Add(columnsNames);

                while (r.Read())
                {
                    List<object> rowsValues = new List<object>();
                    for (int i = 0; i < columnsNames.Count; i++)
                        rowsValues.Add(r[i]);
                    got.Add(rowsValues);
                }

                if (needRowCount)
                {
                    r.Close();
                    cmd.CommandText = "SELECT FOUND_ROWS()";
                    cmd.Parameters.Clear();
                    rowCount = (long)cmd.ExecuteScalar();
                }
            }

            DisposeCommand(cmd);

            object[][] ret = new object[got.Count][];

            for (int rI = 0; rI < got.Count; rI++)
                ret[rI] = got[rI].ToArray();

            return ret;
        }
    }
    /// <summary>
    /// Пара ключ-значение, хранимая в БД
    /// </summary>
    public class Kvp
    {
        public uint ID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        //SQLiteHelper helper = new SQLiteHelper();
        SQLiteHelper helper;
        public Kvp(SQLiteHelper help)
        {
            helper = help;
        }
        public Kvp() { }


        public DateTime Created = DateTime.Now;
        public DateTime Read = DateTime.Now;
        public DateTime Written = DateTime.Now;

        public string Load(string key, string defaultValue)
        {
            string ret = defaultValue;
            SQLiteCommand cmd = helper.CreateCommand();
            cmd.CommandText = "SELECT Value_kvp FROM Kvp WHERE Key_kvp=@key";
            cmd.Parameters.AddWithValue("@key", key);
            SQLiteDataReader r = cmd.ExecuteReader();
            if (r.Read())
                ret = r["Value_kvp"].ToString();
            r.Close();
            helper.DisposeCommand(cmd);

            return ret;
        }

        public Kvp[] Load(string like)
        {
            return helper.Select<Kvp>("", "Key_kvp LIKE '@like'",new string[] { "@like", like }).ToArray();
        }

        public void Save(string key, string value)
        {
            Kvp kvp = new Kvp(helper);
            var kvps = helper.Select<Kvp>("Key_kvp=@key", "@key", key);
            if (kvps.Count == 1)
                kvp = kvps[0];
            kvp.Key = key;

            kvp.Value = value;
            helper.Save(kvp);
        }
    }
}
