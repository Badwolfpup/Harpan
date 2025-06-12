using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
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
        #endregion

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
        public ObservableCollection<string> Spelhögstyper { get; set; }

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

            for (int i = 1; i < 8; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (Kortlek.Count > 0)
                    {
                        var kort = Kortlek[0];
                        kort.Högtyp = (Högtyp)Enum.Parse(typeof(Högtyp), "Spelhög" + i); // Sätt Högtyp för varje rad i spelplanen
                        Spelplan.Add(kort);
                        if (j == i-1) kort.ÄrVisad = true; // Sätt ÄrVisad för det sista kortet i raden
                        Kortlek.RemoveAt(0);
                    }
                }
            }

            foreach (var item in Kortlek)
            {
                item.Högtyp = Högtyp.Tahög;
                Spelplan.Add(item); // Lägg till de återstående korten i Spelplan
            }
            ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
        }

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
                if (kort.Högtyp == Högtyp.Tahög)
                {
                    TaNyttKort(kort); // Om kortet är i Tahög, ta ett nytt kort
                    return;
                }
                if (KlickatKort != null && FlyttaKort(kort)) 
                { 
                    RensaBorders(kort); 
                    return; 
                } // Om kortet flyttas, returnera
                if (kort != null && !kort.ÄrKort) return; // Om kortet inte är ett giltigt kort, returnera
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
                    if (index == hög.Count-1) 
                    {
                        RensaBorders(kort);
                        KlickatKort = kort; 
                        kort.Visaram = true; 
                    }
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
                }
                if (KollaVinst())
                {
                    MessageBox.Show("Grattis! Du har vunnit spelet!", "Vinst", MessageBoxButton.OK, MessageBoxImage.Information);
                    InitieraOmgång(); // Starta om spelet
                }
            }
        }

        private bool FlyttaKort(Spelkort? kort) //Fixa flytta till färghög
        {
            bool resultat = false;
            var klickadtyp = KlickatKort.Högtyp;
            if (ÄrFärghög(kort))
            {
                if (ÄrSpelhög(KlickatKort) && Spelplan.Where(x => x.Högtyp == KlickatKort.Högtyp && x.ÄrKort).ToList().IndexOf(KlickatKort) < Spelplan.Where(x => x.Högtyp == KlickatKort.Högtyp && x.ÄrKort).ToList().Count - 1) return false;
                var färghög = Spelplan.Where(x => x.Högtyp == kort.Högtyp).ToList();
                if (färghög.Last().KortVärde == 0 && KlickatKort.KortVärde == 1) 
                { 
                    KlickatKort.Högtyp = HittaFärg(KlickatKort);
                    Spelplan.Remove(KlickatKort); // Ta bort KlickatKort från den tidigare högen
                    Spelplan.Add(KlickatKort); // Lägg till KlickatKort i spelhögen
                    resultat = true; // Flyttning lyckades
                    VisaNästaKort(klickadtyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                    ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                }
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
                var spelhög = Spelplan.Where(x => x.Högtyp == kort.Högtyp).ToList();
                var klickadhög = Spelplan.Where(x => x.Högtyp == KlickatKort.Högtyp).ToList();
                if (ÄrFärghög(KlickatKort) || KlickatKort.Högtyp == Högtyp.Kasthög)
                {
                    var sistakort = spelhög.LastOrDefault();
                    if ((KlickatKort.ÄrRöd != sistakort.ÄrRöd && KlickatKort.KortVärde +1 == sistakort.KortVärde)
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
                else if (KlickatKort != null && kort != null && spelhög.LastOrDefault() == kort)
                {
                    var sistakort = spelhög.LastOrDefault();
                    if ((KlickatKort != null && KlickatKort.KortVärde == kort.KortVärde - 1 && KlickatKort.ÄrRöd != kort.ÄrRöd)
                        || (sistakort != null && !sistakort.ÄrKort && KlickatKort != null && KlickatKort.KortVärde == 13))
                    {
                        if (KlickatKort == klickadhög.LastOrDefault())
                        {
                            //Spelplan.LastOrDefault(x => x.Högtyp == KlickatKort.Högtyp).ÄrVisad = Spelplan.LastOrDefault(x => x.Högtyp == KlickatKort.Högtyp).ÄrKort ? true : false; // Sätt ÄrVisad för det sista kortet i den tidigare högen
                            if (!kort.ÄrKort) Spelplan.Remove(kort);
                            KlickatKort.Högtyp = kort.Högtyp;
                            Spelplan.Remove(KlickatKort); // Ta bort KlickatKort från den tidigare högen
                            Spelplan.Add(KlickatKort); // Lägg till KlickatKort i spelhögen
                            VisaNästaKort(klickadtyp); // Visa nästa kort i den tidigare högen, om det är en spelhög
                            resultat = true; // Flyttning lyckades
                            ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                        }
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
            return resultat; // Om kortet inte är i Kasthög eller Färghög, returnera false
        }

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
                    return Högtyp.Hjärterhög; // Default till Tahög om ingen färg matchar
            }
        }

        private void VisaNästaKort(Högtyp typ)
        {
            var hög = Spelplan.Where(x => x.Högtyp == typ && x.ÄrKort).ToList();
            if (hög.Count > 0) hög.LastOrDefault().ÄrVisad = true; // Sätt ÄrVisad för det sista kortet i den valda högen
            else if (!ÄrFärghög(typ)) // Om det inte är en färghög, lägg till ett tomt kort i spelplanen
            {
                var kort = new Spelkort("nocard") { Högtyp = typ};
                Spelplan.Add(kort);
            }
        }

        private bool ÄrFärghög(Spelkort kort)
        {
            return kort.Högtyp == Högtyp.Hjärterhög ||
                   kort.Högtyp == Högtyp.Spaderhög ||
                   kort.Högtyp == Högtyp.Ruterhög ||
                   kort.Högtyp == Högtyp.Klöverhög;
        }

        private bool ÄrFärghög(Högtyp typ)
        {
            return typ == Högtyp.Hjärterhög ||
                   typ == Högtyp.Spaderhög ||
                   typ == Högtyp.Ruterhög ||
                   typ == Högtyp.Klöverhög;
        }

        

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


        private bool KollaVinst()
        {
            return Spelplan.Where(x => x.Högtyp == Högtyp.Hjärterhög && x.ÄrKort).Count() > 2 || Spelplan.Where(x => x.Högtyp == Högtyp.Spaderhög && x.ÄrKort).Count() > 2
                || Spelplan.Where(x => x.Högtyp == Högtyp.Ruterhög && x.ÄrKort).Count() > 2 || Spelplan.Where(x => x.Högtyp == Högtyp.Klöverhög && x.ÄrKort).Count() > 2;
        }

        private void TaNyttKort(Spelkort kort) //Fixa att den flytta nocard till först
        {
            if (Spelplan.Where(x => x.Högtyp == Högtyp.Tahög && x.ÄrKort).Count() > 0)
            {
                kort.Högtyp = Högtyp.Kasthög; // Sätt Högtyp till Kasthög
                kort.ÄrVisad = true;
                Spelplan.Remove(kort); // Ta bort kortet från Tahög
                Spelplan.Add(kort); // Lägg till kortet i Spelplan så att det liger sist i listan
                RensaBorders(kort); // Rensa tidigare valda borders
            }
            else
            {
                var templista = Spelplan.Where(x => x.Högtyp == Högtyp.Kasthög && x.ÄrKort).ToList();
                while (templista.Count > 0)
                {
                    var kastkort = templista.LastOrDefault(x => x.Högtyp == Högtyp.Kasthög);
                    if (kastkort != null || kastkort != default)
                    {
                        kastkort.Högtyp = Högtyp.Tahög; // Sätt Högtyp till Tahög
                        kastkort.ÄrVisad = false;
                        Spelplan.Remove(kastkort); // Ta bort kortet från Kasthög
                        Spelplan.Add(kastkort); // Lägg till kortet i Spelplan så att det liger sist i listan
                        templista.Remove(kastkort); // Ta bort kortet från temporär lista
                    }

                }

            }
            ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPlus)
            {
                InitieraOmgång();
            }
        }
    }

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

    public class BaksidaConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return null; // Or a default fallback image
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

                // Return the fully formed BitmapImage object, not the string.
                return image;
            }
            catch (Exception)
            {
                // If the URI is bad or the image doesn't exist, return null 
                // or a default "image not found" image.
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


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
