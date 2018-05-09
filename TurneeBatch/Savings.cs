using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurneeBatch
{
    class descComparer<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            return Comparer<T>.Default.Compare(y, x);
        }
    }
    class saving
    {
        public string FromClient { get; set; }
        public string ToClient { get; set; }
        public double FromVolum { get; set; }
        public double ToVolum { get; set; }
        public double Distanta { get; set; }
        public double SavingValue { get; set; }
        public string Utilizare { get; set; }
        public saving(string from, string to, double volumFrom, double volumTo, double distanta, double saving)
        {
            FromClient = from;
            ToClient = to;
            FromVolum = volumFrom;
            ToVolum = volumTo;
            SavingValue = saving;
            Distanta = distanta;
            Utilizare = "";
        }
    }
    class listaSavings
    {
        private SortedList<double, saving> _savings;
        public listaSavings()
        {
            _savings = new SortedList<double, saving>(new descComparer<double>());
        }
        public void Add(saving saving)
        {
            double savVal = saving.SavingValue;
            bool contains = _savings.ContainsKey(savVal);
            while (contains == true)
            {
                savVal += 0.0001;
                contains = _savings.ContainsKey(savVal);
            }
            _savings.Add(savVal, saving);
        }
        public List<saving> Lista
        {
            get
            {
                return _savings.Values.ToList<saving>();
            }
        }
        public void Reset()
        {
            _savings.Clear();
        }
    }
}
