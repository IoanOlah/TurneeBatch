using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace TurneeBatch
{
    public partial class frmMain : Form
    {
        private static string fileName;
        private static string logfileName;
        private parametriRute pr = new parametriRute();
        private listaDistanteOR DistanteOR = new listaDistanteOR();
        private listaClienti Clienti = new listaClienti();
        private listaSavings Savings = new listaSavings();
        private listaRute Rute = new listaRute();
        private enum stadiu { S0_Null = 0, S1_Clienti = 1, S2_Precalcul = 2, S3_Calcul = 3, S4_Postcalcul = 4 };
        stadiu StadiuCalcul = new stadiu();
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCP = base.CreateParams;
                myCP.ClassStyle = myCP.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCP;
            }
        }

        public frmMain()
        {
            InitializeComponent();
            StadiuCalcul = stadiu.S0_Null;
            txVolCR.DataBindings.Add("Text", pr, "CR.Volum");
            txVolMega.DataBindings.Add("Text", pr, "Mega.Volum");
            txVolNormal.DataBindings.Add("Text", pr, "Normal.Volum");
            txVolCamionetaMare.DataBindings.Add("Text", pr, "CamiMare.Volum");
            txVolCamionetaMica.DataBindings.Add("Text", pr, "CamiMica.Volum");

            txOprCR.DataBindings.Add("Text", pr, "CR.Opriri");
            txOprMega.DataBindings.Add("Text", pr, "Mega.Opriri");
            txOprNormal.DataBindings.Add("Text", pr, "Normal.Opriri");
            txOprCamionetaMare.DataBindings.Add("Text", pr, "CamiMare.Opriri");
            txOprCamionetaMica.DataBindings.Add("Text", pr, "CamiMica.Opriri");

            txMjCR.DataBindings.Add("Text", pr, "CR.Marja");
            txMjMega.DataBindings.Add("Text", pr, "Mega.Marja");
            txMjNormal.DataBindings.Add("Text", pr, "Normal.Marja");
            txMjCamionetaMare.DataBindings.Add("Text", pr, "CamiMare.Marja");
            txMjCamionetaMica.DataBindings.Add("Text", pr, "CamiMica.Marja");

            txMaxCamCR.DataBindings.Add("Text", pr, "CR.LimitaCamioane");
            txMaxCamMega.DataBindings.Add("Text", pr, "Mega.LimitaCamioane");
            txMaxCamNormal.DataBindings.Add("Text", pr, "Normal.LimitaCamioane");
            txMaxCamCamionetaMare.DataBindings.Add("Text", pr, "CamiMare.LimitaCamioane");
            txMaxCamCamionetaMica.DataBindings.Add("Text", pr, "CamiMica.LimitaCamioane");

            txCostPerKm.DataBindings.Add("Text", pr, "CostPerKm");
            txCostPerMc.DataBindings.Add("Text", pr, "CostPerMc");
            txCostPerDescarcare.DataBindings.Add("Text", pr, "CostPerDescarcare");
            txEroareDist.DataBindings.Add("Text", pr, "MarjaEroareDistanta");
            ckVolMaxUltimStop.DataBindings.Add("Checked", pr, "VolMaximLaUltimStop");
            txCentruResponsabilitate.DataBindings.Add("Text", pr, "CentruResponsabilitate");

            WriteLogLine("START ROUTE BUILDER");
        }

        private void LoadClientiFile()
        {
            bool toLoad = true;
            if (StadiuCalcul >= stadiu.S1_Clienti)
                if (MessageBox.Show("Exista clienti incarcati. Doriti sa continuati?", "incarcare clienti", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    toLoad = false;
            if (toLoad)
            {
                //DistanteOR.LoadFile();
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.FileName = "Document";
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Fisiere CSV |*.csv|Fisiere text |*.txt|Toate fisierele |*.*";
                DialogResult drs = dlg.ShowDialog();
                if (drs == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                    if (File.Exists(fileName))
                        try
                        {
                            Rute.Reset();
                            Savings.Reset();
                            Clienti.Reset();
                            pr.Reset();
                            using (StreamReader sr = new StreamReader(fileName))
                            {
                                sr.ReadLine(); sr.ReadLine();
                                while (!sr.EndOfStream)
                                {
                                    var line = sr.ReadLine();
                                    var lineWords = line.Split(';');
                                    double dLon, dLat, dVolPGS, dVolGRC, dVolART, dVolTOT, dDist;
                                    double.TryParse(lineWords[4], out dLat);
                                    double.TryParse(lineWords[5], out dLon);
                                    double.TryParse(lineWords[6], out dVolPGS);
                                    double.TryParse(lineWords[7], out dVolGRC);
                                    double.TryParse(lineWords[8], out dVolART);
                                    double.TryParse(lineWords[9], out dVolTOT);
                                    double.TryParse(lineWords[10], out dDist);
                                    Clienti.Add(new client(lineWords[0], lineWords[1], lineWords[2], lineWords[3], dLat, dLon,
                                        dVolPGS, dVolGRC, dVolART, dVolTOT, dDist, pr.MarjaEroareDistanta));
                                }
                            }
                            StadiuCalcul = stadiu.S1_Clienti;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Eroare in fisierul de date. Verificati formatul delimitatorului precum si continutul fisierului." +
                                Environment.NewLine +
                                Environment.NewLine + "<" + ex.Message + ">", "Fisier", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    else
                        MessageBox.Show("Fisierul nu exista.");
                }
            }
        }
        private bool LoadClientiFile(string fileName)
        {
            //un singur rand va fi ignorat ca fiind capul de tabel
            //DistanteOR.LoadFile();
            if (File.Exists(fileName))
                try
                {
                    Rute.Reset();
                    Savings.Reset();
                    Clienti.Reset();
                    pr.Reset();
                    using (StreamReader sr = new StreamReader(fileName))
                    {
                        sr.ReadLine(); 
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();
                            var lineWords = line.Split(';');
                            double dLon, dLat, dVolPGS, dVolGRC, dVolART, dVolTOT, dDist;
                            double.TryParse(lineWords[4], out dLat);
                            double.TryParse(lineWords[5], out dLon);
                            double.TryParse(lineWords[6], out dVolPGS);
                            double.TryParse(lineWords[7], out dVolGRC);
                            double.TryParse(lineWords[8], out dVolART);
                            double.TryParse(lineWords[9], out dVolTOT);
                            double.TryParse(lineWords[10], out dDist);
                            Clienti.Add(new client(lineWords[0], lineWords[1], lineWords[2], lineWords[3], dLat, dLon,
                                dVolPGS, dVolGRC, dVolART, dVolTOT, dDist, pr.MarjaEroareDistanta));
                        }
                    }
                    StadiuCalcul = stadiu.S1_Clienti;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Eroare in fisierul de date. Verificati formatul delimitatorului precum si continutul fisierului." +
                        Environment.NewLine +
                        Environment.NewLine + "<" + ex.Message + ">", "Fisier", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            else
                MessageBox.Show("Fisierul nu exista.");
            return true;
        }

        private void PrecalculRute()
        {
            int NrCamioaneSD, newRTnumber;
            double volSD, volRout;
            string tipCamSD;
            bool start = true;
            if (StadiuCalcul >= stadiu.S2_Precalcul)
                if (MessageBox.Show("Exista rute precalculate. Doriti sa continuati?", "Precalcul rute", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    start = false;
            if (start)
            {
                //WriteLogLine(" -- START PRECALCUL RUTE --");
                Rute.Reset();
                Savings.Reset();
                pr.Reset();
                for (int i = 0; i <= Clienti.Lista.Count - 1; i++)
                    Clienti.ResetSD(Clienti.Lista[i].ClCode);

                for (int i = 0; i <= Clienti.Lista.Count - 1; i++)
                {
                    NrCamioaneSD = 0;
                    volSD = pr.VolumPosibilSD(Clienti.Lista[i].VolumTotal);
                    while (volSD > 0)
                    {
                        NrCamioaneSD++;
                        volRout = Clienti.Lista[i].VolumPentruRute - volSD;
                        newRTnumber = Rute.AdaugaRutaSD(Clienti.Lista[i].ClCode, volSD);
                        tipCamSD = pr.TipCamion(volSD);
                        Rute.ModificaTipCamion(newRTnumber, tipCamSD);
                        pr.InregistreazaCamion("", tipCamSD);
                        Clienti.ModificaVolumeSD(Clienti.Lista[i].ClCode, volSD, volRout, NrCamioaneSD);
                        volSD = pr.VolumPosibilSD(Clienti.Lista[i].VolumPentruRute);
                        //WriteLogLine(" Add ruta SD - " + newRTnumber + " - " + Clienti.Lista[i].ClCode);
                    }
                }
                //WriteLogLine(" -- END PRECALCUL RUTE --");
                StadiuCalcul = stadiu.S2_Precalcul;
            }
        }
        private void CalculSavings()
        {
            //WriteLogLine(" -- START CALCUL SAVINGS --");
            for (int i = 0; i <= Clienti.Lista.Count - 1; i++)
                if ((Clienti.Lista[i].VolumPentruRute > 0) && (Clienti.Lista[i].ExclusDinRute == false))
                    for (int j = 0; j <= Clienti.Lista.Count - 1; j++)
                        if ((Clienti.Lista[j].VolumPentruRute > 0) && (Clienti.Lista[j].ExclusDinRute == false))
                            if (Clienti.Lista[i].ClCode != Clienti.Lista[j].ClCode)
                                Savings.Add(new saving(Clienti.Lista[i].ClCode, Clienti.Lista[j].ClCode,
                                    Clienti.Lista[i].VolumPentruRute, Clienti.Lista[j].VolumPentruRute,
                                    Clienti.Lista[i].Distanta(Clienti.Lista[j], pr.MarjaEroareDistanta),
                                    Clienti.Lista[j].DistantaOR - Clienti.Lista[j].Distanta(Clienti.Lista[i], pr.MarjaEroareDistanta)));
            //WriteLogLine(" -- END CALCUL SAVINGS --");
        }
        private void CalculRute()
        {
            double volDePrins = 0, volto, volfr;
            string tipPosibilCamion = "";
            int numarStopuri;
            if (StadiuCalcul >= stadiu.S3_Calcul)
                MessageBox.Show("Exista rute calculate. Nu puteti continua!", "Calcul rute", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            else
            {
                //WriteLogLine(" -- START CALCUL RUTE --");
                CalculSavings();
                for (int i = 0; i <= Savings.Lista.Count - 1; i++)
                    if (Savings.Lista[i].SavingValue > 0)
                    {
                        client from = Clienti.Client(Savings.Lista[i].FromClient);
                        client to = Clienti.Client(Savings.Lista[i].ToClient);
                        if ((from.PozitiaInRuta == "nealocat") && (to.PozitiaInRuta == "nealocat"))
                        #region FROM si TO nu sunt in Rute -> Ruta noua
                        {
                            numarStopuri = 2;
                            volDePrins = from.VolumPentruRute - from.VolumInRute + to.VolumPentruRute - to.VolumInRute;
                            tipPosibilCamion = pr.CamionPosibilRuta(volDePrins, numarStopuri);
                            if (tipPosibilCamion != "")
                            {
                                volto = to.VolumPentruRute - to.VolumInRute;
                                volfr = from.VolumPentruRute - from.VolumInRute;
                                Rute.AdaugaRutaRT(from.ClCode, to.ClCode, volfr, volto, Savings.Lista[i].Distanta);
                                Rute.ModificaTipCamion(Rute.RutaCuStartCode(from.ClCode, "RT").NumarRuta, tipPosibilCamion);
                                pr.InregistreazaCamion("", tipPosibilCamion);
                                Clienti.ModificaDupaAdaugareRT(from.ClCode, Rute.RutaCuStartCode(from.ClCode, "RT").NumarRuta, "First", volfr);
                                Clienti.ModificaDupaAdaugareRT(to.ClCode, Rute.RutaCuStartCode(from.ClCode, "RT").NumarRuta, "Last", volto);
                                Savings.Lista[i].Utilizare = "RT noua";
                                //WriteLogLine("Add ruta RT - " + Rute.Lista.Count.ToString() + ": " + from.ClCode + " - " + to.ClCode);
                            }
                        }
                        #endregion
                        else if ((from.PozitiaInRuta == "Last") && (to.PozitiaInRuta == "nealocat"))
                        #region FROM este ultimul in Ruta, TO nu este inRute -> adauga To la sfarsit
                        {
                            rutaObject rtPosibila = Rute.RutaCuEndCode(from.ClCode, "RT");
                            numarStopuri = rtPosibila.NumarStopuri + 1;
                            volDePrins = rtPosibila.Volum + to.VolumPentruRute - to.VolumInRute;
                            tipPosibilCamion = pr.CamionPosibilRuta(volDePrins, numarStopuri);
                            if (tipPosibilCamion != "")
                            {
                                volto = to.VolumPentruRute - to.VolumInRute;
                                Rute.AddNewEnd(rtPosibila.NumarRuta, to.ClCode, volto, Savings.Lista[i].Distanta);
                                if (rtPosibila.TipCamion != tipPosibilCamion)
                                {
                                    pr.InregistreazaCamion(rtPosibila.TipCamion, tipPosibilCamion);
                                    Rute.ModificaTipCamion(rtPosibila.NumarRuta, tipPosibilCamion);
                                }
                                Clienti.ModificaDupaAdaugareRT(from.ClCode, from.NumarRuta, "Mid", 0);
                                Clienti.ModificaDupaAdaugareRT(to.ClCode, from.NumarRuta, "Last", volto);
                                //WriteLogLine("Add new END la ruta - " + from.NumarRuta + ": " + from.ClCode + " - " + to.ClCode);
                                Savings.Lista[i].Utilizare = "RT new END";
                            }
                        }
                        #endregion
                        else if ((from.PozitiaInRuta == "nealocat") && (to.PozitiaInRuta == "First"))
                        #region TO este primul in Ruta, FROM nu este inRute -> adauga FROM la inceput
                        {
                            rutaObject rtPosibila = Rute.RutaCuStartCode(to.ClCode, "RT");
                            numarStopuri = rtPosibila.NumarStopuri + 1;
                            volDePrins = rtPosibila.Volum + from.VolumPentruRute - from.VolumInRute;
                            tipPosibilCamion = pr.CamionPosibilRuta(volDePrins, numarStopuri);
                            if (tipPosibilCamion != "")
                            {
                                volfr = from.VolumPentruRute - from.VolumInRute;
                                Rute.AddNewStart(rtPosibila.NumarRuta, from.ClCode, volfr, Savings.Lista[i].Distanta);
                                if (rtPosibila.TipCamion != tipPosibilCamion)
                                {
                                    pr.InregistreazaCamion(rtPosibila.TipCamion, tipPosibilCamion);
                                    Rute.ModificaTipCamion(rtPosibila.NumarRuta, tipPosibilCamion);
                                }
                                Clienti.ModificaDupaAdaugareRT(from.ClCode, to.NumarRuta, "First", volfr);
                                Clienti.ModificaDupaAdaugareRT(to.ClCode, to.NumarRuta, "Mid", 0);
                                //WriteLogLine("Add new START la ruta - " + to.NumarRuta + ": " + from.ClCode + " - " + to.ClCode);
                                Savings.Lista[i].Utilizare = "RT new START";
                            }
                        }
                        #endregion
                        else if ((from.PozitiaInRuta == "Last") && (to.PozitiaInRuta == "First"))
                        #region FROM este ultimul intr-o ruta, TO este primul in alta ruta -> link rute
                        {
                            rutaObject rtPosibila1 = Rute.RutaCuEndCode(from.ClCode, "RT");
                            rutaObject rtPosibila2 = Rute.RutaCuStartCode(to.ClCode, "RT");
                            if ((rtPosibila1.TipRuta != "SD") && (rtPosibila2.TipRuta != "SD"))
                            {
                                numarStopuri = rtPosibila1.NumarStopuri + rtPosibila2.NumarStopuri;
                                volDePrins = rtPosibila1.Volum + rtPosibila2.Volum;
                                tipPosibilCamion = pr.CamionPosibilRuta(volDePrins, numarStopuri);
                                if ((tipPosibilCamion != "") && (from.NumarRuta != to.NumarRuta))
                                {
                                    Rute.LinkRute(from.NumarRuta, to.NumarRuta, Savings.Lista[i].Distanta);
                                    Clienti.ModificaDupaAdaugareRT(from.ClCode, from.NumarRuta, "Mid", 0);
                                    Clienti.ModificaDupaAdaugareRT(to.ClCode, from.NumarRuta, "Mid", 0);
                                    foreach (stopRuta sr in rtPosibila2.ListaStopuri)
                                    {
                                        Clienti.ModificaDupaAdaugareRT(sr.Stop, from.NumarRuta, "", 0);
                                    }
                                    if (rtPosibila1.TipCamion != tipPosibilCamion)
                                    {
                                        pr.InregistreazaCamion(rtPosibila1.TipCamion, tipPosibilCamion);
                                        Rute.ModificaTipCamion(rtPosibila1.NumarRuta, tipPosibilCamion);
                                    }
                                    pr.InregistreazaCamion(rtPosibila2.TipCamion, "");
                                    //WriteLogLine("LINK Rute - " + rtPosibila1.NumarRuta + " <- " + rtPosibila2.NumarRuta + ": " + from.ClCode + " - " + to.ClCode);
                                    Savings.Lista[i].Utilizare = "RT LINK";
                                }
                            }
                        }
                        #endregion
                    }
                WriteLogLine(" -- Adauga distante depozit --");
                foreach (rutaObject ro in Rute.Lista)
                {
                    Rute.AddSecventaDepozit(ro.NumarRuta, Clienti.Client(ro.FirstStop).DistantaOR);
                }
                PostcalculRute();
                WriteLogLine(" -- END CALCUL RUTE --");
                StadiuCalcul = stadiu.S4_Postcalcul;
            }
        }
        private void PostcalculRute()
        {
            //alocare camionete la volume mici cu o singura destinatie

            //redefinire coloana Pozitie in Ruta
            for (int i = 0; i <= Clienti.Lista.Count - 1; i++)
            {
                if ((Clienti.Lista[i].VolumTotal == Clienti.Lista[i].VolumSingleDest))
                    Clienti.Lista[i].PozitiaInRuta = Clienti.Lista[i].NrRuteSD.ToString() + "SD";
                else if ((Clienti.Lista[i].VolumSingleDest > 0) && (Clienti.Lista[i].ExclusDinRute == true))
                    Clienti.Lista[i].PozitiaInRuta = Clienti.Lista[i].NrRuteSD.ToString() + "SD";
                else if (Clienti.Lista[i].VolumSingleDest > 0)
                    Clienti.Lista[i].PozitiaInRuta = Clienti.Lista[i].NrRuteSD.ToString() + "SD + " + Clienti.Lista[i].PozitiaInRuta;
            }
            StadiuCalcul = stadiu.S4_Postcalcul;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.CR_volum = pr.CR.Volum;
            Properties.Settings.Default.CR_opriri = pr.CR.Opriri;
            Properties.Settings.Default.CR_marja = pr.CR.Marja;
            Properties.Settings.Default.CR_nrmax = pr.CR.LimitaCamioane;

            Properties.Settings.Default.Mega_volum = pr.Mega.Volum;
            Properties.Settings.Default.Mega_opriri = pr.Mega.Opriri;
            Properties.Settings.Default.Mega_marja = pr.Mega.Marja;
            Properties.Settings.Default.Mega_nrmax = pr.Mega.LimitaCamioane;

            Properties.Settings.Default.Normal_volum = pr.Normal.Volum;
            Properties.Settings.Default.Normal_orpiri = pr.Normal.Opriri;
            Properties.Settings.Default.Normal_marja = pr.Normal.Marja;
            Properties.Settings.Default.Normal_nrmax = pr.Normal.LimitaCamioane;

            Properties.Settings.Default.Camima_volum = pr.CamiMare.Volum;
            Properties.Settings.Default.Camima_opriri = pr.CamiMare.Opriri;
            Properties.Settings.Default.Camima_marja = pr.CamiMare.Marja;
            Properties.Settings.Default.Camima_nrmax = pr.CamiMare.LimitaCamioane;

            Properties.Settings.Default.Camimi_volum = pr.CamiMica.Volum;
            Properties.Settings.Default.Camimi_opriri = pr.CamiMica.Opriri;
            Properties.Settings.Default.Camimi_marja = pr.CamiMica.Marja;
            Properties.Settings.Default.Camimi_nrmax = pr.CamiMica.LimitaCamioane;

            Properties.Settings.Default.MjErrDist = pr.MarjaEroareDistanta;

            Properties.Settings.Default.Save();
        }
        public void WriteLogLine(string logtext)
        {
            StreamWriter log;
            if (!File.Exists(@"D:\TourBuilderLog.txt"))
            {
                log = new StreamWriter(@"D:\TourBuilderLog.txt");
            }
            else
            {
                log = File.AppendText(@"D:\TourBuilderLog.txt");
            }

            // Write to the file:
            log.WriteLine(DateTime.Now + " - " + logtext);

            // Close the stream:
            log.Close();
        }
        public void WriteLogLine(string filename, string logtext)
        {
            StreamWriter log;
            if (!File.Exists(filename))
            {
                log = new StreamWriter(filename);
            }
            else
            {
                log = File.AppendText(filename);
            }

            // Write to the file:
            log.WriteLine(DateTime.Now + " - " + logtext);

            // Close the stream:
            log.Close();
        }
        private string right(string instr, int numcar)
        {
            instr = "0000000" + instr;
            return instr.Substring(instr.Length - numcar);
        }

        private void btLoadFile_Click(object sender, EventArgs e)
        {
            LoadClientiFile();
            dgvClienti.DataSource = Clienti.Lista;
            dgvSavings.DataSource = Savings.Lista;
            dgvRute.DataSource = Rute.Lista;
        }
        private void btnPrecalcul_Click(object sender, EventArgs e)
        {
            PrecalculRute();
            dgvClienti.DataSource = Clienti.Lista;
            dgvSavings.DataSource = Savings.Lista;
            dgvRute.DataSource = Rute.Lista;
        }
        private void btRoute_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            CalculRute();
            dgvClienti.DataSource = Clienti.Lista;
            dgvSavings.DataSource = Savings.Lista;
            dgvRute.DataSource = Rute.Lista;
            Cursor.Current = Cursors.Default;
        }
        private void btExportFile_Click(object sender, EventArgs e)
        {
            if (StadiuCalcul >= stadiu.S2_Precalcul)
            {
                Cursor.Current = Cursors.WaitCursor;
                int stopno;
                StreamWriter log;
                string line, rtno;
                string infoRulare, fileName;
                infoRulare = DateTime.Now.Year.ToString() + "-" +
                    right(DateTime.Now.Month.ToString(), 2) + "-" +
                    right(DateTime.Now.Day.ToString(), 2) + " " +
                    right(DateTime.Now.Hour.ToString(), 2) +
                    right(DateTime.Now.Minute.ToString(), 2);
                fileName = @"D:\ListaTurnee " + infoRulare + ".csv";
                if (!File.Exists(fileName))
                {
                    log = new StreamWriter(fileName);
                }
                else
                {
                    log = File.CreateText(fileName);
                }
                log.WriteLine("CentruResponsabilitate;InfoRutare;RouteNo;StopNo;IncarcareNo;ClientCode;ShipToAddrCode;ShipToAddrName;DeliveryZone;StopLatitude;StopLongitude;StopVolume");
                for (int i = 0; i <= Rute.Lista.Count - 1; i++)
                {
                    rtno = "000" + Rute.Lista[i].NumarRuta.ToString();
                    line = pr.CentruResponsabilitate + ";" + infoRulare + ";" +
                        "R" + rtno.Substring(rtno.Length - 3) + ";0;0;DOR;DOR;Depozit Oradea;none;47.080;21.890;0";
                    log.WriteLine(line);
                    stopno = 1;
                    for (int j = 0; j <= Rute.Lista[i].ListaStopuri.Count - 1; j++)
                    {
                        line = pr.CentruResponsabilitate + ";" + infoRulare + ";";
                        line += "R" + rtno.Substring(rtno.Length - 3) + ";";
                        line += stopno.ToString() + ";";
                        line += (Rute.Lista[i].ListaStopuri.Count - stopno + 1).ToString() + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).StCode + ";";
                        line += Rute.Lista[i].ListaStopuri[j].Stop + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).ClNume + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).StDlZone + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).Latitudine + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).Longitudine + ";";
                        line += Rute.Lista[i].ListaStopuri[j].Volum.ToString();
                        log.WriteLine(line);
                        stopno++;
                    }
                }
                log.Close();
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Fisierul a fost salvat!", "Export fisier turnee", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("Nu au fost calculate rute!");
        }
        private void btClose_Click(object sender, EventArgs e)
        {
            bool toClose = true;
            if (StadiuCalcul >= stadiu.S2_Precalcul)
                if (MessageBox.Show("Exista rute calculate. Doriti sa inchideti?", "Terminare program", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    toClose = false;
            if (toClose)
            {
                SaveSettings();
                this.Close();
            }
        }

        private void btSelSursa_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tbSursa.Text = folderBrowserDialog1.SelectedPath;
            }
            int FileCount = Directory.GetFiles(tbSursa.Text, "*.csv", SearchOption.AllDirectories).Length;
            tbNumarFisiere.Text = FileCount.ToString();
        }
        private void btSelDestinatie_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tbDestinatie.Text = folderBrowserDialog1.SelectedPath;
                logfileName = tbDestinatie.Text + @"\\BatchLog.txt";
            }
        }

        private bool BatchVerify(string sourceFolder)
        {
            foreach (string file in Directory.EnumerateFiles(sourceFolder, "*.csv"))
            {
                try
                {
                    StadiuCalcul = stadiu.S0_Null;
                    if (!(LoadClientiFile(file)))
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }
            return true;
        }
        private void BatchExportResult(string fileName, string sursa)
        {
            if (StadiuCalcul >= stadiu.S2_Precalcul)
            {
                int stopno;
                StreamWriter log;
                string line, rtno;
                string infoRulare;
                infoRulare = DateTime.Now.Year.ToString() + "-" +
                    right(DateTime.Now.Month.ToString(), 2) + "-" +
                    right(DateTime.Now.Day.ToString(), 2) + " " +
                    right(DateTime.Now.Hour.ToString(), 2) +
                    right(DateTime.Now.Minute.ToString(), 2);
                //fileName = @"D:\ListaTurnee " + infoRulare + ".csv";
                if (!File.Exists(fileName))
                {
                    log = new StreamWriter(fileName);
                }
                else
                {
                    log = File.CreateText(fileName);
                }
                log.WriteLine("FisierSursa;InfoRutare;RouteNo;StopNo;IncarcareNo;ClientCode;ShipToAddrCode;ShipToAddrName;DeliveryZone;StopLatitude;StopLongitude;StopVolume");
                for (int i = 0; i <= Rute.Lista.Count - 1; i++)
                {
                    rtno = "000" + Rute.Lista[i].NumarRuta.ToString();
                    line = sursa + ";" + infoRulare + ";" +
                        "R" + rtno.Substring(rtno.Length - 3) + ";0;0;DOR;DOR;Depozit Oradea;none;47.080;21.890;0";
                    log.WriteLine(line);
                    stopno = 1;
                    for (int j = 0; j <= Rute.Lista[i].ListaStopuri.Count - 1; j++)
                    {
                        line = sursa + ";" + infoRulare + ";";
                        line += "R" + rtno.Substring(rtno.Length - 3) + ";";
                        line += stopno.ToString() + ";";
                        line += (Rute.Lista[i].ListaStopuri.Count - stopno + 1).ToString() + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).StCode + ";";
                        line += Rute.Lista[i].ListaStopuri[j].Stop + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).ClNume + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).StDlZone + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).Latitudine + ";";
                        line += Clienti.Client(Rute.Lista[i].ListaStopuri[j].Stop).Longitudine + ";";
                        line += Rute.Lista[i].ListaStopuri[j].Volum.ToString();
                        log.WriteLine(line);
                        stopno++;
                    }
                }
                log.Close();
                Cursor.Current = Cursors.Default;
                //MessageBox.Show("Fisierul a fost salvat!", "Export fisier turnee", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                WriteLogLine(logfileName,"Nu au fost calculate rute!");
        }
        private void BatchCalcul(string sourceFolder)
        {
            WriteLogLine(logfileName, "Batch process start =================================");
            if (tbSursa.Text == tbDestinatie.Text)
            {
                MessageBox.Show("Sursa este identica cu destinatia. Programul nu poate continua.");
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            foreach (string file in Directory.EnumerateFiles(sourceFolder, "*.csv"))
            {
              try
                {
                    WriteLogLine(logfileName, "Process file -> " + Path.GetFileNameWithoutExtension(file));
                    StadiuCalcul = stadiu.S0_Null;
                    LoadClientiFile(file);
                    PrecalculRute();
                    CalculSavings();
                    CalculRute();
                    BatchExportResult(tbDestinatie.Text + @"\\" + Path.GetFileName(file), Path.GetFileNameWithoutExtension(file));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    WriteLogLine(logfileName, "Batch process error -> " + e.Message);
                }
            }
            Cursor.Current = Cursors.Default;
            WriteLogLine(logfileName, "Batch process end =================================");
            MessageBox.Show("Fisierele au fost salvate in directorul destinatie!", "Export fisier turnee", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void btCalcul_Click(object sender, EventArgs e)
        {
            if (tbDestinatie.Text == "")
            {
                MessageBox.Show("Directorul destinatie nu este setat.");
            }
            else
            {
                if (BatchVerify(tbSursa.Text))
                {
                    BatchCalcul(tbSursa.Text);
                }
            }
        }
    }
}
