using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurneeBatch
{
    class stopRuta
    {
        public string Stop { get; set; }
        public double Volum { get; set; }
        public stopRuta(string stop, double volum)
        {
            Stop = stop;
            Volum = volum;
        }
    }

    class rutaObject
    {
        private List<stopRuta> _listaStopuri;
        public rutaObject(int numarRuta, string tipRuta)
        {
            _listaStopuri = new List<stopRuta>();
            NumarRuta = numarRuta;
            TipRuta = tipRuta;
        }
        public int NumarRuta { get; set; }
        public string TipRuta { get; set; }
        public string LabelRuta
        {
            get
            {
                string str = "000" + NumarRuta.ToString();
                return TipRuta + str.Substring(str.Length - 3, 3);
            }
        }
        public double Volum { get; set; }
        public double DistantaInterna { get; set; }
        public double DistantaDepozit { get; set; }
        public int NumarStopuri
        {
            get
            {
                return _listaStopuri.Count;
            }
        }
        public string FirstStop
        {
            get
            {
                return _listaStopuri[0].Stop;
            }
        }
        public string LastStop
        {
            get
            {
                return _listaStopuri[_listaStopuri.Count - 1].Stop;
            }
        }
        public string Eticheta
        {
            get
            {
                string et = "";
                foreach (stopRuta rs in _listaStopuri)
                    et += ".." + rs.Stop;
                return et;
            }
        }
        public string TipCamion { get; set; }
        public List<stopRuta> ListaStopuri
        {
            get
            {
                return _listaStopuri;
            }
        }
        public void AddNewStart(string stop, double volum, double distanta)
        {
            _listaStopuri.Insert(0, new stopRuta(stop, volum));
            Volum += volum;
            DistantaInterna += distanta;
        }
        public void AddNewEnd(string stop, double volum, double distanta)
        {
            _listaStopuri.Add(new stopRuta(stop, volum));
            Volum += volum;
            DistantaInterna += distanta;
        }
        public void AddDepozit(double distanta)
        {
            _listaStopuri.Insert(0, new stopRuta("DOR", 0));
            DistantaDepozit = distanta;
        }
        public double CostPerMc(double costperkm, double marjaerrdist, double costperdesc)
        {
            return (costperkm * ((DistantaInterna + DistantaDepozit) * (1 + marjaerrdist / 100)) + costperdesc * (NumarStopuri - 1)) / Volum;
        }
        public double CostTurneu(double costperkm, double marjaerrdist, double costperdesc)
        {
            return costperkm * ((DistantaInterna + DistantaDepozit) * (1 + marjaerrdist / 100)) + costperdesc * (NumarStopuri - 1);
        }
    }
    class listaRute
    {
        private int _contorRute = 0;
        private Dictionary<int, rutaObject> _dictRute;
        public listaRute()
        {
            _dictRute = new Dictionary<int, rutaObject>();
        }
        public List<rutaObject> Lista
        {
            get
            {
                return _dictRute.Values.ToList();
            }
        }
        public rutaObject Ruta(int numarRuta)
        {
            return _dictRute[numarRuta];
        }
        public void Reset()
        {
            this._dictRute.Clear();
            _contorRute = 0;
        }
        public int AdaugaRutaSD(string codDest, double volum)
        {
            _contorRute++;
            _dictRute.Add(_contorRute, new rutaObject(_contorRute, "SD"));
            _dictRute[_contorRute].AddNewStart(codDest, volum, 0);
            return _contorRute;
        }
        public void AdaugaRutaRT(string codStart, string codEnd, double volStart, double volEnd, double distanta)
        {
            _contorRute++;
            _dictRute.Add(_contorRute, new rutaObject(_contorRute, "RT"));
            _dictRute[_contorRute].AddNewStart(codStart, volStart, 0);
            _dictRute[_contorRute].AddNewEnd(codEnd, volEnd, distanta);
        }
        public void AddNewStart(int numarRuta, string newStart, double volum, double distanta)
        {
            _dictRute[numarRuta].AddNewStart(newStart, volum, distanta);
        }
        public void AddNewEnd(int numarRuta, string newEnd, double volum, double distanta)
        {
            _dictRute[numarRuta].AddNewEnd(newEnd, volum, distanta);
        }
        public void LinkRute(int rutaDestinatie, int rutaSursa, double distanta)
        {
            for (int i = 0; i <= _dictRute[rutaSursa].ListaStopuri.Count - 1; i++)
            {
                _dictRute[rutaDestinatie].AddNewEnd(_dictRute[rutaSursa].ListaStopuri[i].Stop, _dictRute[rutaSursa].ListaStopuri[i].Volum, 0);
            }
            _dictRute[rutaDestinatie].DistantaInterna = _dictRute[rutaSursa].DistantaInterna + distanta;
            _dictRute.Remove(rutaSursa);
        }
        public void AddSecventaDepozit(int numarRuta, double distanta)
        {
            _dictRute[numarRuta].DistantaDepozit = distanta;
        }
        public void ModificaTipCamion(int numarRuta, string tipCamion)
        {
            _dictRute[numarRuta].TipCamion = tipCamion;
        }
        public rutaObject RutaCuStartCode(string codStart, string tipRuta)
        {
            foreach (var ro in _dictRute)
                if ((ro.Value.FirstStop == codStart) && (ro.Value.TipRuta == tipRuta))
                    return ro.Value;
            return null;
        }
        public rutaObject RutaCuEndCode(string endCode, string tipRuta)
        {
            foreach (var ro in _dictRute)
                if ((ro.Value.LastStop == endCode) && (ro.Value.TipRuta == tipRuta))
                    return ro.Value;
            return null;
        }
    }
}
