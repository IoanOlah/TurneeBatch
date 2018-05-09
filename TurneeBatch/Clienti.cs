using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TurneeBatch
{
    class client
    {
        double _distOR = 0;
        public string StCode { get; set; }
        public string ClCode { get; set; }
        public string ClNume { get; set; }
        public string StDlZone { get; set; }
        public double Latitudine { get; set; }
        public double Longitudine { get; set; }
        public double DistantaOR
        {
            get
            {
                return _distOR;
            }
        }
        public double VolumTotal { get; set; } //volumul total pentru destinatie
        public double VolumPGS { get; set; } //volum PGS
        public double VolumGRC { get; set; } //volum GREENCORP
        public double VolumART { get; set; } //volum ARTEMOB
        public double VolumSingleDest { get; set; } //volumul prins ininitial in rute SD
        public double VolumPentruRute { get; set; } //volumul ramas pentru rute dupa precaclcul SDD:\Documents\Visual Studio 2010\Projects\Turnee v2\Turnee v2\ListaDistante.cs
        public double VolumInRute { get; set; } //volumul prins in rute
        public bool ExclusDinRute { get; set; }
        public int NrRuteSD { get; set; }
        public int NumarRuta { get; set; }
        public string PozitiaInRuta { get; set; }
        public client(string CodeSellTo, string CodeCl, string NumeCl, string DeliveryZone, double LatitudineCl, double LongitudineCl,
            double VolumClPGS, double VolumClGRC, double VolumClART, double VolumCl, double distOR, double coefMajDist)
        {
            StCode = CodeSellTo;
            ClCode = CodeCl;
            ClNume = NumeCl;
            StDlZone = DeliveryZone;
            Longitudine = LongitudineCl;
            Latitudine = LatitudineCl;
            VolumTotal = VolumCl;
            VolumPGS = VolumClPGS;
            VolumGRC = VolumClGRC;
            VolumART = VolumClART;
            VolumPentruRute = VolumCl;
            VolumSingleDest = 0;
            VolumInRute = 0;
            ExclusDinRute = false;
            PozitiaInRuta = "nealocat";
            if (distOR != 0)
                _distOR = distOR;
            else
                _distOR = distORdinCoord * (1 + coefMajDist / 100);
        }
        public double Distanta(client client, double coefMajDist)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(client.Latitudine - Latitudine);
            var dLon = deg2rad(client.Longitudine - Longitudine);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(deg2rad(Latitudine)) * Math.Cos(deg2rad(client.Latitudine)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            //var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var c = 2 * Math.Atan(Math.Sqrt(a) / Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d * (1 + coefMajDist / 100);
        }
        private double distORdinCoord
        {
            get
            {
                var lonOR = 21.890;
                var latOR = 47.080;
                var R = 6371; // Radius of the earth in km
                var dLat = deg2rad(latOR - Latitudine);
                var dLon = deg2rad(lonOR - Longitudine);
                var a =
                    Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(deg2rad(Latitudine)) * Math.Cos(deg2rad(latOR)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var d = R * c; // Distance in km
                return d;
            }
        }
        double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
    class listaClienti
    {
        protected Dictionary<string, client> _dictClienti;
        public listaClienti()
        {
            _dictClienti = new Dictionary<string, client>();
        }
        public void Add(client client)
        {
            _dictClienti.Add(client.ClCode, client);
        }
        public client Client(string CodCl)
        {
            if (_dictClienti.ContainsKey(CodCl))
                return _dictClienti[CodCl];
            return null;
        }
        public List<client> Lista
        {
            get
            {
                return _dictClienti.Values.ToList();
            }
        }
        public void ModificaVolumeSD(string code, double volSD, double volRout, int nrRuteSD)
        {
            if (_dictClienti.ContainsKey(code))
            {
                _dictClienti[code].VolumSingleDest += volSD;
                _dictClienti[code].VolumPentruRute = volRout;
                _dictClienti[code].NrRuteSD = nrRuteSD;
            }
        }
        public void ModificaDupaAdaugareRT(string code, int numarRuta, string pozitia, double volInRute)
        {
            if (_dictClienti.ContainsKey(code))
            {
                _dictClienti[code].NumarRuta = numarRuta;
                if (pozitia != "")
                    _dictClienti[code].PozitiaInRuta = pozitia;
                _dictClienti[code].VolumInRute += volInRute;
                _dictClienti[code].VolumPentruRute -= volInRute;
            }
        }
        public void Reset()
        {
            _dictClienti.Clear();
        }
        public void ResetSD(string code)
        {
            foreach (KeyValuePair<string, client> cl in _dictClienti)
            {
                _dictClienti[cl.Key].NrRuteSD = 0;
                _dictClienti[cl.Key].VolumSingleDest = 0;
                _dictClienti[cl.Key].VolumPentruRute = _dictClienti[cl.Key].VolumTotal;
                _dictClienti[cl.Key].PozitiaInRuta = "nealocat";
            }
        }
    }
    class listaDistanteOR
    {
        private static string fileName;
        protected Dictionary<string, double> _dictDistOR;
        public listaDistanteOR()
        {
            _dictDistOR = new Dictionary<string, double>();
        }
        public void Add(string destinatie, double distanta)
        {
            _dictDistOR.Add(destinatie, distanta);
        }
        public double Distanta(string destinatie)
        {
            if (_dictDistOR.ContainsKey(destinatie))
                return _dictDistOR[destinatie];
            return 0;
        }
        public void LoadFile()
        {
            fileName = "D:\\Turnee\\DistanteDepozit.txt";
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    sr.ReadLine(); sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        var lineWords = line.Split(';');
                        double dist;
                        double.TryParse(lineWords[1], out dist);
                        Add(lineWords[0], dist);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Reset()
        {
            _dictDistOR.Clear();
        }
    }
}
