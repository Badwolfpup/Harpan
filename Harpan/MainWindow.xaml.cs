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
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private Spelkort _klickatkort;
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


        private string klickadhög;

        private bool _forceraUIUppdatering;
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

        public ObservableCollection<string> Spelhögstyper { get; set; }

        private ObservableCollection<Spelkort> _spelplan;
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


        private ObservableCollection<Spelkort> _tahög;
        public ObservableCollection<Spelkort> Tahög
        {
            get { return _tahög; }
            set
            {
                if (_tahög != value)
                {
                    _tahög = value;
                    OnPropertyChanged(nameof(Tahög));
                }
            }
        }

        private ObservableCollection<Spelkort> _kasthög;
        public ObservableCollection<Spelkort> Kasthög
        {
            get { return _kasthög; }
            set
            {
                if (_kasthög != value)
                {
                    _kasthög = value;
                    OnPropertyChanged(nameof(Kasthög));
                }
            }
        }

        private ObservableCollection<ObservableCollection<Spelkort>> _färghögar;
        public ObservableCollection<ObservableCollection<Spelkort>> Färghögar
        {
            get { return _färghögar; }
            set
            {
                if (_färghögar != value)
                {
                    _färghögar = value;
                    OnPropertyChanged(nameof(Färghögar));
                }
            }
        }

        private ObservableCollection<ObservableCollection<Spelkort>> _spelhögar;
        public ObservableCollection<ObservableCollection<Spelkort>> Spelhögar
        {
            get { return _spelhögar; }
            set
            {
                if (_spelhögar != value)
                {
                    _spelhögar = value;
                    OnPropertyChanged(nameof(Spelhögar));
                }
            }
        }



        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Tahög = new ObservableCollection<Spelkort>();
            Kasthög = new ObservableCollection<Spelkort>();
            Färghögar = new ObservableCollection<ObservableCollection<Spelkort>>();
           
            Spelhögar = new ObservableCollection<ObservableCollection<Spelkort>>();
            InitieraOmgång();

        }


        private void InitieraOmgång()
        {
            Tahög.Clear();
            Kasthög.Clear();
            Färghögar.Clear();
            Spelhögar.Clear();
            Tahög.Add(new Spelkort("nocard") { Högtyp = Högtyp.Tahög }); // Lägg till en tom kort i Tahög
            Kasthög.Add(new Spelkort("nocard") { Högtyp = Högtyp.Kasthög }); // Lägg till en tom kort i Kasthög
            Färghögar.Add(new ObservableCollection<Spelkort>() { new Spelkort("hjarter") { Högtyp = Högtyp.Färghög, KortVärde = 0 } }); // Lägg till tomma färghögar
            Färghögar.Add(new ObservableCollection<Spelkort>() { new Spelkort("spader") { Högtyp = Högtyp.Färghög, KortVärde = 0 } });
            Färghögar.Add(new ObservableCollection<Spelkort>() { new Spelkort("ruter") { Högtyp = Högtyp.Färghög, KortVärde = 0 } });
            Färghögar.Add(new ObservableCollection<Spelkort>() { new Spelkort("klover") { Högtyp = Högtyp.Färghög, KortVärde = 0 } });
            Spelplan = new ObservableCollection<Spelkort>()
            {
                new Spelkort("nocard") { Högtyp = Högtyp.Tahög}, // Lägg till en tom kort i spelplanen
                new Spelkort("nocard") { Högtyp = Högtyp.Kasthög },
                new Spelkort("nocard") { Högtyp = Högtyp.Hjärterhög, KortVärde = 0 }, // Lägg till tomma färghögar i spelplanen
                new Spelkort("nocard") { Högtyp = Högtyp.Spaderhög, KortVärde = 0 },
                new Spelkort("nocard") { Högtyp = Högtyp.Ruterhög, KortVärde = 0 },
                new Spelkort("nocard") { Högtyp = Högtyp.Klöverhög, KortVärde = 0 }

               };
            Spelhögstyper = new ObservableCollection<string> { "Tahög", "Kasthög", "", "Hjärterhög", "Spaderhög", "Ruterhög", "Klöverhög",
             "Spelhög1", "Spelhög2", "Spelhög3", "Spelhög4", "Spelhög5", "Spelhög6", "Spelhög7" };
            var Kortlek = HanteraKortlek.BlandaKortlek();

            for (int i = 1; i < 8; i++)
            {
                //Spelhögar.Add(new ObservableCollection<Spelkort>());
                for (int j = 0; j < i; j++)
                {
                    if (Kortlek.Count > 0)
                    {
                        var kort = Kortlek[0];
                        kort.Högtyp = (Högtyp)Enum.Parse(typeof(Högtyp), "Spelhög" + i); // Sätt Högtyp för varje rad i spelplanen
                        Spelplan.Add(kort);
                        //Spelhögar[i].Add(kort);
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
            Kortlek.ForEach(kort => Tahög.Add(kort)); // Lägg till de återstående korten i TaHög

        }






        private void RensaBorders(Spelkort kort)
        {
            foreach (var list in Spelhögar)
            {
                foreach (var item in list)
                {
                    item.Visaram = false;
                }
            }
            foreach (var list in Färghögar)
            {
                foreach (var item in list)
                {
                    item.Visaram = false;
                }
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
                if (kort.Högtyp == Högtyp.Kasthög || kort.Högtyp == Högtyp.Färghög)
                {
                    RensaBorders(kort); // Rensa tidigare valda borders
                    kort.Visaram = true; // Sätt ram för det klickade kortet
                    KlickatKort = kort; // Sätt det klickade kortet som KlickatKort
                }
                else
                {
                    
                    var hög = Spelhögar.FirstOrDefault(h => h.Contains(kort));
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

        private bool FlyttaKort(Spelkort? kort)
        {
            bool resultat = false;
            if (kort.Högtyp == Högtyp.Färghög)
            {
                if (KlickatKort.Högtyp == Högtyp.Spelhög && Spelhögar.FirstOrDefault(x => x.Contains(KlickatKort)).IndexOf(KlickatKort) < Spelhögar.FirstOrDefault(x => x.Contains(KlickatKort)).Count - 1) return false;
                var färghögar = Färghögar.FirstOrDefault(x => x.Contains(kort));
                if (färghögar.Last().KortVärde == 0 && KlickatKort.KortVärde == 1) 
                { 
                    TaBortKlickatkort(); // Ta bort kortet från den tidigare högen
                    färghögar.Add(KlickatKort);
                    KlickatKort.Högtyp = kort.Högtyp;
                    resultat = true; // Flyttning lyckades
                    ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                }
                else if (färghögar.Last().KortVärde + 1 == KlickatKort.KortVärde && färghögar.Last().ÄrRöd == KlickatKort.ÄrRöd)
                {
                    TaBortKlickatkort(); // Ta bort kortet från den tidigare högen
                    färghögar.Add(KlickatKort); // Lägg till kortet i färghögen
                    KlickatKort.Högtyp = kort.Högtyp;
                    resultat = true; // Flyttning lyckades
                    ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                }
            }
            if (kort.Högtyp == Högtyp.Spelhög)
            {
                var spelhög = Spelhögar.FirstOrDefault(x => x.Contains(kort));

                if (KlickatKort.Högtyp == Högtyp.Färghög || KlickatKort.Högtyp == Högtyp.Kasthög)
                {
                    var sistakort = spelhög.LastOrDefault();
                    if ((KlickatKort.ÄrRöd != sistakort.ÄrRöd && KlickatKort.KortVärde +1 == sistakort.KortVärde)
                        || (spelhög.LastOrDefault() != null && !spelhög.LastOrDefault().ÄrKort && KlickatKort != null && KlickatKort.KortVärde == 13))
                    {
                        TaBortKlickatkort(); // Ta bort kortet från den tidigare högen
                        if (!kort.ÄrKort) spelhög.Remove(kort);
                        spelhög.Add(KlickatKort); // Lägg till kortet i spelhögen
                        KlickatKort.Högtyp = kort.Högtyp;
                        resultat = true; // Flyttning lyckades
                        ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                    }
                }
                else if (KlickatKort != null)
                {
                    var klickatHög = Spelhögar.FirstOrDefault(x => x.Contains(KlickatKort));
                    var sistakort = klickatHög != null ? klickatHög.LastOrDefault() : null;
                    if ((sistakort != null && KlickatKort.KortVärde == kort.KortVärde - 1 && KlickatKort.ÄrRöd != kort.ÄrRöd)
                        || (spelhög.LastOrDefault() != null && !spelhög.LastOrDefault().ÄrKort && KlickatKort != null && KlickatKort.KortVärde == 13))
                    {
                        if (KlickatKort == sistakort)
                        {
                            TaBortKlickatkort(); // Ta bort kortet från den tidigare högen
                            klickatHög.LastOrDefault().ÄrVisad = klickatHög.LastOrDefault().ÄrKort ? true : false; // Sätt ÄrVisad för det sista kortet i den tidigare högen
                            if (!kort.ÄrKort) spelhög.Remove(kort);
                            spelhög.Add(KlickatKort); // Lägg till kortet i spelhögen
                            KlickatKort.Högtyp = kort.Högtyp;
                            resultat = true; // Flyttning lyckades
                            ForceraUIUppdatering = !ForceraUIUppdatering; // Tvinga UI-uppdatering
                        }
                        else
                        { 
                            var index = klickatHög.IndexOf(KlickatKort);
                            var sekvens = klickatHög.Skip(index).ToList();
                            if (!kort.ÄrKort) spelhög.Remove(kort);
                            sekvens.ForEach(k =>
                            {
                                klickatHög.Remove(k); // Ta bort korten i sekvensen från den tidigare högen
                                spelhög.Add(k); // Lägg till korten i spelhögen
                            });
                            klickatHög.LastOrDefault().ÄrVisad = klickatHög.LastOrDefault().ÄrKort ? true : false; // Sätt ÄrVisad för det sista kortet i den tidigare högen
                            KlickatKort.Högtyp = kort.Högtyp;
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
            return resultat; // Om kortet inte är i Kasthög eller Färghög, returnera false
        }

        private void TaBortKlickatkort()
        {
            if (Kasthög.Contains(KlickatKort))
            {
                Kasthög.Remove(KlickatKort); // Ta bort kortet från Kasthög
            }
            else if (Färghögar.FirstOrDefault(x => x.Contains(KlickatKort)) is ObservableCollection<Spelkort> färghög)
            {
               färghög.Remove(KlickatKort); // Ta bort kortet från färghögen

            }
            else if (Spelhögar.FirstOrDefault(x => x.Contains(KlickatKort)) is ObservableCollection<Spelkort> spelhög)
            {
                spelhög.Remove(KlickatKort);
                if (spelhög.Count == 0) spelhög.Add(new Spelkort("nocard") { Högtyp = Högtyp.Spelhög }); // Lägg till en tom kort i spelhögen om den är tom
                else spelhög.Last().ÄrVisad = true; // Sätt ÄrVisad för det sista kortet i spelhögen1
            }
        }



        private bool KollaVinst()
        {
            return Färghögar.Any(x => x.Count > 2);
        }

        private void TaNyttKort(Spelkort kort) //Fixa att den flytta nocard till först
        {
            if (Spelplan.Where(x => x.Högtyp == Högtyp.Tahög).Count() > 1)
            {
                kort.Högtyp = Högtyp.Kasthög; // Sätt Högtyp till Kasthög
                kort.ÄrVisad = true;
                Spelplan.Remove(kort); // Ta bort kortet från Tahög
                Spelplan.Add(kort); // Lägg till kortet i Spelplan så att det liger sist i listan
                RensaBorders(kort); // Rensa tidigare valda borders
            }
            else
            {
                var templista = Spelplan.Where(x => x.Högtyp == Högtyp.Kasthög).ToList();
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

        private void TaKort_Click(object sender, RoutedEventArgs e)
        {
            if (Tahög.Count > 1)
            {
                var kort = Tahög.Last();
                kort.Högtyp = Högtyp.Kasthög; // Sätt Högtyp till Kasthög
                Tahög.Remove(kort);
                Kasthög.Add(kort);
                kort.ÄrVisad = true;
                RensaBorders(kort); // Rensa tidigare valda borders
            }
            else
            {
                for (int i = 1; i < Kasthög.Count; i++)
                {
                    var kort = Kasthög[i];
                    kort.Högtyp = Högtyp.Tahög; // Sätt Högtyp till Tahög
                    Tahög.Add(kort);
                    kort.ÄrVisad = false;
                }
                for (int i = Kasthög.Count - 1; i > 0; i--)
                {
                    Kasthög.RemoveAt(i);
                }
            }
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

    public class SpelPlanBaksidaConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return null; // Or a default fallback image
            }
            
            var kort = values[0] as Spelkort;
            if (kort == null) return "pack://application:,,,/Spelkort/baksida.png"; // Default baksida om inget kort är satt
            var spelhögar = values[1] as ObservableCollection<ObservableCollection<Spelkort>>;
            var hög = spelhögar?.FirstOrDefault(h => h.Contains(kort));
            if (hög == null) return "pack://application:,,,/Spelkort/baksida.png"; // Default baksida om ingen hög hittas
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
        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    var lista = value as ObservableCollection<Spelkort>;
        //    var typ = parameter as string;
        //    Högtyp högtyp = (Högtyp)Enum.Parse(typeof(Högtyp), typ);
        //    if (lista == null) return null;

        //    return lista.Where(x => x.Högtyp == högtyp);
        //}

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
            var testlista = lista.Where(x => x.Högtyp == högtyp).ToList(); 
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
