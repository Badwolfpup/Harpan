using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Harpan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region Private Fields
        private Spelkort _klickatkort;
        private bool _forceraUIUppdatering;
        private ObservableCollection<Spelkort> _spelplan;
        private bool _manuellförflyttning = true;
        private bool _automatiskförflyttning = false;
        private bool _automatisklösning = false;
        private bool _aktiverad = false;
        DispatcherTimer _timer;
        #endregion

        //Det kort man redan har klickat på, det som har en ram runt sig
        public Spelkort KlickatKort
        {
            get { return _klickatkort; }
            set
            {
                if (_klickatkort != value)
                {
                    _klickatkort = value;
                    OnPropertyChanged(nameof(KlickatKort));
                }
            }
        }

        // Lista med typer av spelhögar som används i UI
        public ObservableCollection<string> Spelhögstyper { get; set; }

        // Flagga för att tvinga UI-uppdatering, används för att uppdatera UI när kort flyttas
        public bool ForceraUIUppdatering
        {
            get { return _forceraUIUppdatering; }
            set
            {
                if (_forceraUIUppdatering != value)
                {
                    _forceraUIUppdatering = value;
                    OnPropertyChanged(nameof(ForceraUIUppdatering));
                }
            }
        }

        // Spelplan som innehåller alla kort i spelet, inklusive tomma högar
        public ObservableCollection<Spelkort> Spelplan
        {
            get { return _spelplan; }
            set
            {
                if (_spelplan != value)
                {
                    _spelplan = value;
                    OnPropertyChanged(nameof(Spelplan));
                }
            }
        }



        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Spelhögstyper = new ObservableCollection<string> { "Tahög", "Kasthög", "", "Hjärterhög", "Spaderhög", "Ruterhög", "Klöverhög",
             "Spelhög1", "Spelhög2", "Spelhög3", "Spelhög4", "Spelhög5", "Spelhög6", "Spelhög7" };
            InitieraOmgång();
        }

        // Initiera Spelplan med kort för tomma högar
        private void InitieraOmgång()
        {
            Spelplan = new ObservableCollection<Spelkort>()
            {
                new Spelkort("nocard") { Högtyp = Högtyp.Tahög},
                new Spelkort("nocard") { Högtyp = Högtyp.Kasthög },
                new Spelkort("hjarter") { Högtyp = Högtyp.Hjärterhög, KortVärde = 0 },
                new Spelkort("spader") { Högtyp = Högtyp.Spaderhög, KortVärde = 0 },
                new Spelkort("ruter") { Högtyp = Högtyp.Ruterhög, KortVärde = 0 },
                new Spelkort("klover") { Högtyp = Högtyp.Klöverhög, KortVärde = 0 }
            };

            var Kortlek = HanteraKortlek.BlandaKortlek();

            // Lägg till kort i Spelplanen för varje rad i spelhögarna
            for (int i = 1; i < 8; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (Kortlek.Count > 0)
                    {
                        var kort = Kortlek[0];
                        kort.Högtyp = (Högtyp)Enum.Parse(typeof(Högtyp), "Spelhög" + i); // Sätt Högtyp för varje rad i spelplanen
                        Spelplan.Add(kort);
                        if (j == i - 1) kort.ÄrVisad = true; // Sätt ÄrVisad för det sista kortet i raden
                        Kortlek.RemoveAt(0);
                    }
                }
            }

            // Lägg till de återstående korten i Tahög
            foreach (var item in Kortlek)
            {
                item.Högtyp = Högtyp.Tahög;
                Spelplan.Add(item); // Lägg till de återstående korten i Spelplan
            }

            ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
        }

        // Rensa ram för alla kort i Spelplanen och nollställ KlickatKort om det är samma som det klickade kortet
        private void RensaBorders(Spelkort kort)
        {
            foreach (var item in Spelplan)
            {
                item.Visaram = false;
            }
            if (KlickatKort != null) KlickatKort.Visaram = false; // Rensa ram för det klickade kortet
            if (KlickatKort == kort) KlickatKort = null; // Nollställ KlickatKort om det är samma som det klickade kortet
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (sender is Border b && b.DataContext is Spelkort kort)
            {



                // Om kortet är i Tahög, ta ett nytt kort
                if (kort.Högtyp == Högtyp.Tahög)
                {
                    TaNyttKort(kort);
                    return;
                }

                //Om kortet kan flyttas till en annan hög, returnera
                if (!_manuellförflyttning && _automatiskförflyttning && !ÄrFärghög(kort) && HittaKortAttFlyttaTill(kort)) return;

                // Om KlickatKort är satt och flyttning lyckas, returnera
                if (KlickatKort != null && FlyttaKort(kort))
                {
                    RensaBorders(kort); // Rensa tidigare valda borders
                    return;
                }

                if (kort != null && !kort.ÄrKort) return; // Om kortet inte är ett giltigt kort, returnera

                // Om kortet är i Kasthög eller Färghög, sätt ram och KlickatKort
                if (kort.Högtyp == Högtyp.Kasthög || ÄrFärghög(kort))
                {
                    RensaBorders(kort); // Rensa tidigare valda borders
                    kort.Visaram = true; // Sätt ram för det klickade kortet
                    KlickatKort = kort; // Sätt det klickade kortet som KlickatKort
                }

                else
                {

                    var hög = Spelplan.Where(x => x.Högtyp == kort.Högtyp && x.ÄrKort).ToList();
                    var index = hög?.IndexOf(kort) ?? -1;
                    // Om kortet är det sista i raden, sätt ram och KlickatKort
                    if (index == hög.Count - 1)
                    {
                        RensaBorders(kort);
                        KlickatKort = kort;
                        kort.Visaram = true;
                    }
                    // Om kortet är i en spelhög och alla korten i raden följer reglerna, sätt ram för alla kort i raden och KlickatKort
                    else if (hög.Skip(index).Zip(hög.Skip(index + 1), (prev, current) => prev.ÄrRöd != current.ÄrRöd && prev.KortVärde - 1 == current.KortVärde).ToList().All(x => x))
                    {
                        RensaBorders(kort);
                        for (int i = index; i < hög.Count; i++)
                        {
                            var spelkort = hög[i];
                            spelkort.Visaram = true; // Sätt ram för alla kort i den valda raden
                        }
                        KlickatKort = kort; // Sätt det klickade kortet som KlickatKort
                    }

                    //Om spelet är vunnet, visa meddelande och starta om spelet
                    if (KollaVinst())
                    {
                        MessageBox.Show("Grattis! Du har vunnit spelet!", "Vinst", MessageBoxButton.OK, MessageBoxImage.Information);
                        InitieraOmgång(); // Starta om spelet
                    }
                }
            }
        }

        private bool HittaKortAttFlyttaTill(Spelkort kort)
        {
            bool resultat = false;
            RensaBorders(kort); // Rensa tidigare valda borders
            if (Spelplan.Where(x => x.Högtyp == HittaFärg(kort)).LastOrDefault().KortVärde == kort.KortVärde - 1)
            {
                var gammaltyp = kort.Högtyp; // Spara den gamla högtypen för kortet
                kort.Högtyp = HittaFärg(kort); // Sätt Högtyp till den färghög som kortet tillhör
                Spelplan.Remove(kort); // Ta bort kortet från den tidigare högen
                Spelplan.Add(kort); // Lägg till kortet i spelhögen
                resultat = true; // Flyttning lyckades
                KlickatKort = null; // Nollställ KlickatKort efter flyttning
                VisaNästaKort(gammaltyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                return true;
            }
            else
            {
                for (int i = 1; i < 8; i++)
                {
                    var högtyp = (Högtyp)Enum.Parse(typeof(Högtyp), "Spelhög" + i);
                    if (kort.Högtyp == högtyp) continue; // Hoppa över den spelhögen som kortet redan är i
                    var spelhög = Spelplan.Where(x => x.Högtyp == högtyp).ToList();

                    if (spelhög.Count > 0)
                    {
                        var gammaltyp = kort.Högtyp; // Spara den gamla högtypen för kortet
                        var sistakort = spelhög.LastOrDefault();
                        // Kolla om det klickade kortet kan flyttas till den aktuella spelhögen
                        if ((kort.KortVärde == sistakort.KortVärde - 1 && kort.ÄrRöd != sistakort.ÄrRöd) || (!sistakort.ÄrKort && kort.KortVärde == 13))
                        {
                            if (Spelplan.Where(x => x.Högtyp == kort.Högtyp).LastOrDefault() == kort)
                            {
                                if (!sistakort.ÄrKort) Spelplan.Remove(sistakort);
                                kort.Högtyp = (Högtyp)Enum.Parse(typeof(Högtyp), "Spelhög" + i); // Sätt Högtyp för kortet till den aktuella spelhögen
                                Spelplan.Remove(kort); // Ta bort kortet från den tidigare högen
                                Spelplan.Add(kort); // Lägg till kortet i spelhögen
                                resultat = true; // Flyttning lyckades
                                KlickatKort = null; // Nollställ KlickatKort efter flyttning
                                VisaNästaKort(gammaltyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                                ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                                break;
                            } 
                            else
                            {
                                if (!sistakort.ÄrKort) Spelplan.Remove(sistakort);
                                var index = Spelplan.Where(x => x.Högtyp == kort.Högtyp).ToList().IndexOf(kort);
                                var sekvens = Spelplan.Where(x => x.Högtyp == kort.Högtyp).ToList().Skip(index).ToList();
                                if (!kort.ÄrKort) Spelplan.Remove(kort);
                                sekvens.ForEach(k =>
                                {
                                    k.Högtyp = kort.Högtyp;
                                    Spelplan.Remove(k); // Ta bort korten i sekvensen från den tidigare högen
                                    Spelplan.Add(k); // Lägg till korten i spelhögen
                                });
                                VisaNästaKort(gammaltyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                                resultat = true;
                                ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                            }
                        }
                    }
                }
            }


                return resultat;
        }

        // Flytta kortet till rätt hög baserat på reglerna
        private bool FlyttaKort(Spelkort? kort)
        {
            bool resultat = false; // Flagga för att indikera om flyttning lyckades
            var klickadtyp = KlickatKort.Högtyp; // Spara den högtypen för KlickatKort


            if (ÄrFärghög(kort))
            {
                //Om det redan klickade kortet är i en färghög och det inte är det sista kortet i den högen, returnera false
                if (ÄrSpelhög(KlickatKort) && Spelplan.Where(x => x.Högtyp == KlickatKort.Högtyp && x.ÄrKort).ToList().IndexOf(KlickatKort) < Spelplan.Where(x => x.Högtyp == KlickatKort.Högtyp && x.ÄrKort).ToList().Count - 1) return false;

                //Kollar vilken färghög kortet tillhör
                var färghög = Spelplan.Where(x => x.Högtyp == kort.Högtyp).ToList();

                //Om det inte redan finns några kort i färghögen och det klickade kortet är ett ess, lägg till det i färghögen
                if (färghög.Last().KortVärde == 0 && KlickatKort.KortVärde == 1)
                {
                    KlickatKort.Högtyp = HittaFärg(KlickatKort);
                    Spelplan.Remove(KlickatKort); // Ta bort KlickatKort från den tidigare högen
                    Spelplan.Add(KlickatKort); // Lägg till KlickatKort i spelhögen
                    resultat = true; // Flyttning lyckades
                    VisaNästaKort(klickadtyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                    ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                }
                //Kollar om det klickade kortet är av samma färg och en valör högre
                else if (färghög.Last().KortVärde + 1 == KlickatKort.KortVärde && färghög.Last().ÄrRöd == KlickatKort.ÄrRöd)
                {
                    KlickatKort.Högtyp = kort.Högtyp;
                    Spelplan.Remove(KlickatKort); // Ta bort KlickatKort från den tidigare högen
                    Spelplan.Add(KlickatKort); // Lägg till KlickatKort i spelhögen
                    resultat = true; // Flyttning lyckades
                    VisaNästaKort(klickadtyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                    ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                }
            }
            if (ÄrSpelhög(kort))
            {
                //Kollar vilken spelhög kortet tillhör
                var spelhög = Spelplan.Where(x => x.Högtyp == kort.Högtyp).ToList();
                //Kollar vilken spelhög det klickade kortet tillhör
                var klickadhög = Spelplan.Where(x => x.Högtyp == KlickatKort.Högtyp).ToList();

                //Kollar om det klickade kortet är i kasthögen eller en färghög
                if (ÄrFärghög(KlickatKort) || KlickatKort.Högtyp == Högtyp.Kasthög)
                {
                    var sistakort = spelhög.LastOrDefault();
                    if ((KlickatKort.ÄrRöd != sistakort.ÄrRöd && KlickatKort.KortVärde + 1 == sistakort.KortVärde)
                        || (spelhög.LastOrDefault() != null && !spelhög.LastOrDefault().ÄrKort && KlickatKort != null && KlickatKort.KortVärde == 13))
                    {
                        if (!kort.ÄrKort) Spelplan.Remove(kort);
                        KlickatKort.Högtyp = kort.Högtyp;
                        Spelplan.Remove(KlickatKort); // Ta bort KlickatKort från den tidigare högen
                        Spelplan.Add(KlickatKort); // Lägg till KlickatKort i spelhögen
                        VisaNästaKort(klickadtyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                        resultat = true; // Flyttning lyckades
                        ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                    }
                }
                //Kollar att kortet är det sista i spelhögen
                else if (KlickatKort != null && kort != null && spelhög.LastOrDefault() == kort)
                {
                    var sistakort = spelhög.LastOrDefault();

                    //Kollar om det klickade kortet är av motsatt färg och en valör högre än det sista kortet i spelhögen eller en kung om det klickade kortet är en tom spelhög
                    if ((KlickatKort != null && KlickatKort.KortVärde == kort.KortVärde - 1 && KlickatKort.ÄrRöd != kort.ÄrRöd)
                        || (sistakort != null && !sistakort.ÄrKort && KlickatKort != null && KlickatKort.KortVärde == 13))
                    {
                        //Om det klickade kortet är det sista i spelhögen, flytta det till den nya högen
                        if (KlickatKort == klickadhög.LastOrDefault())
                        {
                            if (!kort.ÄrKort) Spelplan.Remove(kort);
                            KlickatKort.Högtyp = kort.Högtyp;
                            Spelplan.Remove(KlickatKort);
                            Spelplan.Add(KlickatKort);
                            VisaNästaKort(klickadtyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                            resultat = true; // Flyttning lyckades
                            ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                        }
                        //Om det klickade kortet inte är det sista i spelhögen, flytta alla kort i sekvensen till den nya högen
                        else
                        {
                            var index = klickadhög.IndexOf(KlickatKort);
                            var sekvens = klickadhög.Skip(index).ToList();
                            if (!kort.ÄrKort) Spelplan.Remove(kort);
                            sekvens.ForEach(k =>
                            {
                                k.Högtyp = kort.Högtyp;
                                Spelplan.Remove(k); // Ta bort korten i sekvensen från den tidigare högen
                                Spelplan.Add(k); // Lägg till korten i spelhögen
                            });
                            VisaNästaKort(klickadtyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                            resultat = true;
                            ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                        }
                    }

                }
            }
            if (resultat)
            {
                KlickatKort = null; // Nollställ KlickatKort om flyttning lyckades
                if (KollaVinst())
                {
                    MessageBox.Show("Grattis! Du har vunnit spelet!", "Vinst", MessageBoxButton.OK, MessageBoxImage.Information);
                    InitieraOmgång(); // Starta om spelet
                }
            }
            RensaBorders(kort);
            return resultat; //Returnera false om det inte gick att flytta kortet
        }

        // Returnera Högtyp baserat på färgen på kortet
        private Högtyp HittaFärg(Spelkort kort)
        {
            switch (kort.Färg)
            {
                case "H":
                    return Högtyp.Hjärterhög;
                case "R":
                    return Högtyp.Ruterhög;
                case "S":
                    return Högtyp.Spaderhög;
                case "K":
                    return Högtyp.Klöverhög;
                default:
                    return Högtyp.Hjärterhög;
            }
        }

        // Visa nästa kort i den valda spelhögen, om det finns några kort kvar
        private void VisaNästaKort(Högtyp typ)
        {
            var hög = Spelplan.Where(x => x.Högtyp == typ && x.ÄrKort).ToList();
            if (hög.Count > 0) hög.LastOrDefault().ÄrVisad = true; // Sätt ÄrVisad för det sista kortet i den valda högen, om det finns några kort kvar
            else if (!ÄrFärghög(typ)) // Om det inte är en färghög, lägg till ett tomt kort i spelplanen
            {
                var kort = new Spelkort("nocard") { Högtyp = typ };
                Spelplan.Add(kort);
            }
        }

        // Kolla om kortet är i en färghög
        private bool ÄrFärghög(Spelkort kort)
        {
            return kort.Högtyp == Högtyp.Hjärterhög ||
                   kort.Högtyp == Högtyp.Spaderhög ||
                   kort.Högtyp == Högtyp.Ruterhög ||
                   kort.Högtyp == Högtyp.Klöverhög;
        } // Kolla om kortet är i en färghög

        // Kolla om kortet är i en färghög
        private bool ÄrFärghög(Högtyp typ)
        {
            return typ == Högtyp.Hjärterhög ||
                   typ == Högtyp.Spaderhög ||
                   typ == Högtyp.Ruterhög ||
                   typ == Högtyp.Klöverhög;
        }


        // Kolla om kortet är i en spelhög
        private bool ÄrSpelhög(Spelkort kort)
        {
            return kort.Högtyp == Högtyp.Spelhög1 ||
                   kort.Högtyp == Högtyp.Spelhög2 ||
                   kort.Högtyp == Högtyp.Spelhög3 ||
                   kort.Högtyp == Högtyp.Spelhög4 ||
                   kort.Högtyp == Högtyp.Spelhög5 ||
                   kort.Högtyp == Högtyp.Spelhög6 ||
                   kort.Högtyp == Högtyp.Spelhög7;
        }

        // Kolla om spelet är vunnet genom att kontrollera om det finns minst 13 kort i varje färghög
        private bool KollaVinst()
        {
            return Spelplan.Where(x => x.Högtyp == Högtyp.Hjärterhög && x.ÄrKort).Count() >= 13 || Spelplan.Where(x => x.Högtyp == Högtyp.Spaderhög && x.ÄrKort).Count() >= 13
                || Spelplan.Where(x => x.Högtyp == Högtyp.Ruterhög && x.ÄrKort).Count() >= 13 || Spelplan.Where(x => x.Högtyp == Högtyp.Klöverhög && x.ÄrKort).Count() >= 13 || ÄrAllaKortUppvända();
        }

        private bool ÄrAllaKortUppvända()
        {
            return Spelplan.Where(x => x.Högtyp == Högtyp.Tahög && x.ÄrKort).Count() == 0 && Spelplan.Where(x => ÄrSpelhög(x) && x.ÄrKort).All(x => x.ÄrVisad);
        }

        // Hantera klick på kort i Tahög för att ta ett nytt kort
        private void TaNyttKort(Spelkort kort)
        {
            // Om det finns kort i Tahög, flytta det klickade kortet till Kasthög
            if (Spelplan.Where(x => x.Högtyp == Högtyp.Tahög && x.ÄrKort).Count() > 0)
            {
                kort.Högtyp = Högtyp.Kasthög;
                kort.ÄrVisad = true;

                //Ta bort och lägg tillbaka kortet så att det hamnar i rätt position i listan
                Spelplan.Remove(kort);
                Spelplan.Add(kort);
                RensaBorders(kort);
            }
            // Om det inte finns kort i Tahög, flytta alla kort från Kasthög till Tahög
            else
            {
                var templista = Spelplan.Where(x => x.Högtyp == Högtyp.Kasthög && x.ÄrKort).ToList();
                while (templista.Count > 0)
                {
                    var kastkort = templista.LastOrDefault(x => x.Högtyp == Högtyp.Kasthög);
                    if (kastkort != null || kastkort != default)
                    {
                        kastkort.Högtyp = Högtyp.Tahög;
                        kastkort.ÄrVisad = false;

                        //Ta bort och lägg tillbaka kortet så att det hamnar i rätt position i listan
                        Spelplan.Remove(kastkort);
                        Spelplan.Add(kastkort);

                        templista.Remove(kastkort);
                    }

                }

            }
            ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering

        }

        // Om användaren trycker på OemPlus-tangenten, starta om spelet
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPlus)
            {
                InitieraOmgång();
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton r && r.Tag is string tag)
            {
                _manuellförflyttning = tag == "1" ? true : false;
                _automatiskförflyttning = tag == "2" ? true : false;
                _automatisklösning = tag == "3" ? true : false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            return;
            if (!_automatisklösning) return; // Om automatisk lösning inte är aktiverad, returnera

            if (!_aktiverad)
            {
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(500) // Justera tidsintervallet för att passa din lösning
                };
                _timer.Tick += FlyttaAuto;
                _timer.Start();
                _aktiverad = true; // Sätt flagga för att indikera att automatisk lösning är aktiverad
            } else
            {
                _aktiverad = false; // Nollställ flagga för att indikera att automatisk lösning är inaktiverad
                _timer.Stop(); // Stoppa timern om den är aktiv
            }
        }

        private void FlyttaAuto(object sender, EventArgs e)
        {
            if (Spelplan.Where(x => x.Högtyp == Högtyp.Kasthög && x.ÄrKort).Count() > 0)
            {
                var kort = Spelplan.Where(x => x.Högtyp == Högtyp.Kasthög).LastOrDefault();
                foreach (var item in Spelhögstyper)
                {
                    if (item == "") continue; // Hoppa över tomma högar
                    var högtyp = (Högtyp)Enum.Parse(typeof(Högtyp), item);
                    if (högtyp == Högtyp.Tahög || högtyp == Högtyp.Kasthög) continue; // Hoppa över Tahög och Kasthög
                    var sistakort = Spelplan.Where(x => x.Högtyp == högtyp).ToList().LastOrDefault();

                    if (ÄrFärghög(sistakort) && sistakort.KortVärde == kort.KortVärde - 1) { }

                    if (sistakort.KortVärde == kort.KortVärde + 1 && sistakort.ÄrRöd != kort.ÄrRöd || !sistakort.ÄrKort && kort.KortVärde == 13)
                    {
                        if (kort.Högtyp != högtyp)
                        {
                            kort.Högtyp = högtyp; // Sätt Högtyp för kortet till den aktuella spelhögen
                            Spelplan.Remove(kort); // Ta bort kortet från den tidigare högen
                            Spelplan.Add(kort); // Lägg till kortet i spelhögen
                            ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                            return;
                        }
                    }
                }
            }
        }
    }
    // Konverterare för att visa ram på kort baserat på Visaram-egenskapen
    public class VisaRamConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool visaram)
            {
                return visaram ? Brushes.Red : Brushes.Transparent;
            }
            return Brushes.Blue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    // Konverterare för att visa baksidan av kortet om det inte är visat eller inte är ett kort
    public class BaksidaConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return null; 
            }

            var kort = values[0] as Spelkort;
            if (kort == null) return "pack://application:,,,/Spelkort/baksida.png"; // Default baksida om inget kort är satt

            string imagePath = kort.ÄrVisad || !kort.ÄrKort ? kort.ImageGenväg : $"pack://application:,,,/Spelkort/baksida.png";

            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(imagePath, UriKind.Absolute);
                image.EndInit();

                
                return image;
            }
            catch (Exception)
            {
                
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Konverterare för att filtrera spelhögar baserat på vald högtyp
    public class FiltreraSpelhögarConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return null; // Or a default fallback image
            }
            var filter = values[0] as string;
            Högtyp högtyp;
            Enum.TryParse(filter, out högtyp);
            var lista = values[1] as ObservableCollection<Spelkort>;
            return lista.Where(x => x.Högtyp == högtyp).ToList();
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Konverterare för att sätta marginal på kort baserat på Högtyp för att justera positionen i UI
    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var kort = value as Spelkort;
            if (kort == null) return new Thickness(0);
            if (kort.Högtyp == Högtyp.Tahög || kort.Högtyp == Högtyp.Kasthög || kort.Högtyp == Högtyp.Hjärterhög 
                || kort.Högtyp == Högtyp.Spaderhög || kort.Högtyp == Högtyp.Ruterhög || kort.Högtyp == Högtyp.Klöverhög) return new Thickness(0, -150, 0, 0); // Ingen marginal för dessa högar
            return new Thickness(0, -110, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 
}
