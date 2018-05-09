using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurneeBatch
{
    class tipCamion
    {
        public string Denumire { get; set; }
        public Single Volum { get; set; }
        public Single Opriri { get; set; }
        public Single Marja { get; set; }
        public Single VolumMax
        {
            get
            {
                return Volum + Marja;
            }
        }
        public bool ExistaCapacitate(int numarStopuri)
        {
            {
                if ((numarStopuri <= Opriri) && (CamioaneUtilizate < LimitaCamioane))
                    return true;
                return false;
            }
        }
        public int LimitaCamioane { get; set; }
        public int CamioaneUtilizate { get; set; }
        public tipCamion(string denumire, Single volum, Single opriri, Single marja, int maxCam, int numarCam)
        {
            Denumire = denumire;
            Volum = volum;
            Opriri = opriri;
            Marja = marja;
            LimitaCamioane = maxCam;
            CamioaneUtilizate = numarCam;
        }
    }
    class parametriRute
    {
        public tipCamion CR { get; set; }
        public tipCamion Mega { get; set; }
        public tipCamion Normal { get; set; }
        public tipCamion CamiMare { get; set; }
        public tipCamion CamiMica { get; set; }
        public Single CostPerKm { get; set; }
        public Single CostPerMc { get; set; }
        public Single CostPerDescarcare { get; set; }
        public Single MarjaEroareDistanta { get; set; }
        public bool VolMaximLaUltimStop { get; set; }
        public string CentruResponsabilitate { get; set; }

        public parametriRute()
        {
            CR = new tipCamion("CR",
                Properties.Settings.Default.CR_volum,
                Properties.Settings.Default.CR_opriri,
                Properties.Settings.Default.CR_marja,
                Properties.Settings.Default.CR_nrmax, 0);
            Mega = new tipCamion("MEGA",
                Properties.Settings.Default.Mega_volum,
                Properties.Settings.Default.Mega_opriri,
                Properties.Settings.Default.Mega_marja,
                Properties.Settings.Default.Mega_nrmax, 0);
            Normal = new tipCamion("NORMAL",
                Properties.Settings.Default.Normal_volum,
                Properties.Settings.Default.Normal_orpiri,
                Properties.Settings.Default.Normal_marja,
                Properties.Settings.Default.Normal_nrmax, 0);
            CamiMare = new tipCamion("CAMIONETA MARE",
                Properties.Settings.Default.Camima_volum,
                Properties.Settings.Default.Camima_opriri,
                Properties.Settings.Default.Camima_marja,
                Properties.Settings.Default.Camima_nrmax, 0);
            CamiMica = new tipCamion("CAMIONETA MICA",
                Properties.Settings.Default.Camimi_volum,
                Properties.Settings.Default.Camimi_opriri,
                Properties.Settings.Default.Camimi_marja,
                Properties.Settings.Default.Camimi_nrmax, 0);
            CostPerKm = 0.95F;
            CostPerMc = 28.0F;
            CostPerDescarcare = 30.0F;
            MarjaEroareDistanta = Properties.Settings.Default.MjErrDist;
            VolMaximLaUltimStop = true;
            CentruResponsabilitate = Properties.Settings.Default.CentruResponsabilitate;
        }
        public string TipCamion(double volum)
        {
            if ((volum >= CR.Volum - CR.Marja) && (volum <= CR.VolumMax))
                return CR.Denumire;
            if ((volum >= Mega.Volum - Mega.Marja) && (volum <= Mega.VolumMax))
                return Mega.Denumire;
            if ((volum >= Normal.Volum - Normal.Marja) && (volum <= Normal.VolumMax))
                return Normal.Denumire;
            if ((volum >= CamiMare.Volum - CamiMare.Marja) && (volum <= CamiMare.VolumMax))
                return CamiMare.Denumire;
            if ((volum >= CamiMica.Volum - CamiMica.Marja) && (volum <= CamiMica.VolumMax))
                return CamiMica.Denumire;
            return "";
        }
        public string CamionPosibilRuta(double volum, int numarStopuri)
        {
            if ((volum <= CamiMica.VolumMax) && (CamiMica.ExistaCapacitate(numarStopuri)))
                return CamiMica.Denumire;
            if ((volum <= CamiMare.VolumMax) && (CamiMare.ExistaCapacitate(numarStopuri)))
                return CamiMare.Denumire;
            if ((volum <= Normal.VolumMax) && (Normal.ExistaCapacitate(numarStopuri)))
                return Normal.Denumire;
            if ((volum <= Mega.VolumMax) && (Mega.ExistaCapacitate(numarStopuri)))
                return Mega.Denumire;
            if ((volum <= CR.VolumMax) && (CR.ExistaCapacitate(numarStopuri)))
                return CR.Denumire;
            return "";
        }
        public double VolumPosibilSD(double volum)
        {
            if ((volum >= CR.Volum - CR.Marja) && (CR.ExistaCapacitate(1)))
                //daca volum e intre 94-2 si 94+2 return volum, daca e mai mare de 94 return 94
                if (volum > CR.VolumMax)
                    return CR.Volum;
                else
                    return volum;
            if ((volum >= Mega.Volum - Mega.Marja) && (Mega.ExistaCapacitate(1)))
                if (volum > Mega.VolumMax)
                    return Mega.Volum;
                else
                    return volum;
            if ((volum >= Normal.Volum - Normal.Marja) && (Normal.ExistaCapacitate(1)))
                if (volum > Normal.VolumMax)
                    return Normal.Volum;
                else
                    return volum;
            return 0;
        }
        public void InregistreazaCamion(string oldTip, string newTip)
        {
            switch (oldTip)
            {
                case "NORMAL":
                    Normal.CamioaneUtilizate--;
                    break;
                case "MEGA":
                    Mega.CamioaneUtilizate--;
                    break;
                case "CR":
                    CR.CamioaneUtilizate--;
                    break;
                case "CAMIONETA MARE":
                    CamiMare.CamioaneUtilizate--;
                    break;
                case "CAMIONETA MICA":
                    CamiMica.CamioaneUtilizate--;
                    break;
            }
            switch (newTip)
            {
                case "NORMAL":
                    Normal.CamioaneUtilizate++;
                    break;
                case "MEGA":
                    Mega.CamioaneUtilizate++;
                    break;
                case "CR":
                    CR.CamioaneUtilizate++;
                    break;
                case "CAMIONETA MARE":
                    CamiMare.CamioaneUtilizate++;
                    break;
                case "CAMIONETA MICA":
                    CamiMica.CamioaneUtilizate++;
                    break;
            }
        }
        public void Reset()
        {
            CR.CamioaneUtilizate = 0;
            Mega.CamioaneUtilizate = 0;
            Normal.CamioaneUtilizate = 0;
            CamiMare.CamioaneUtilizate = 0;
            CamiMica.CamioaneUtilizate = 0;
        }
    }
}
