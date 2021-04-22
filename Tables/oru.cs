using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBCopier
{
    public class oru
    {
        public int ID { get; set; }
        public string NTE1ListHL7_oru { get; set; }
        public string OBXListHL7_oru { get; set; }
        public string PIDinHL7_oru { get; set; }
        public string NomerZayavki_oru { get; set; }
        public string Karta_oru { get; set; }
        public string TestType_oru { get; set; }
        public string DataZaprosa_oru { get; set; }
        public string DataIspolneniya_oru { get; set; }
        public string ResultStatus_oru { get; set; }
        public string Staff_oru { get; set; }
        public string Probirka_oru { get; set; }
        
        /*public oru(string n, string o, string p, string no, string k, string t,
            string d, string da, string r, string s, string pr)
        {
            NTE1ListHL7 = n;
            OBXListHL7 = o;
            PIDinHL7 = p;
            NomerZayavki = no;
            Karta = k;
            TestType = t;
            DataZaprosa = d;
            DataIspolneniya = da;
            ResultStatus = r;
            Staff = s;
            Probirka = pr;
        }*/
    }
}
