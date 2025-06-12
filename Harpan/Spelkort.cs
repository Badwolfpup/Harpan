using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harpan
{
    public enum Högtyp
    {
        Tomhög,
        Tahög,
        Kasthög,
        Hjärterhög,
        Spaderhög,
        Ruterhög,
        Klöverhög,
        Spelhög1,
        Spelhög2,
        Spelhög3,
        Spelhög4,
        Spelhög5,
        Spelhög6,
        Spelhög7

    }
    public class Spelkort: INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Constructors

        public Spelkort() { }
    
        public Spelkort(string färg, int kortvärde)
        {
            Valör = BeräknaValör(kortvärde);
            Färg = färg;
            KortVärde = kortvärde;
            ÄrRöd = (färg == "H" || färg == "R");
            ÄrKort = true;
            ImageGenväg = Genväg("");
        }

        public Spelkort(string genväg)
        {
            ImageGenväg = Genväg(genväg);
        }
        #endregion

        #region Private Fields
            private bool _visaram;
            private Högtyp _högtyp;
            private string _imagegenväg;
        #endregion

        #region Public Properties
        public bool Visaram
        {
            get { return _visaram; }
            set
            {
                if (_visaram != value)
                {
                    _visaram = value;
                    OnPropertyChanged(nameof(Visaram));
                }
            }
        }

        public Högtyp Högtyp
        {
            get { return _högtyp; }
            set
            {
                if (_högtyp != value)
                {
                    _högtyp = value;
                    OnPropertyChanged(nameof(Högtyp));
                }
            }
        }

        public int KortVärde { get; set; }

        public bool ÄrRöd { get; set; }
        public bool ÄrVisad { get; set; }
        public bool ÄrKort { get; set; }

        public string Färg { get; set; }
        public string Valör { get; set; }
        public string ImageGenväg
        {
            get { return _imagegenväg; }
            set
            {
                if (_imagegenväg != value)
                {
                    _imagegenväg = value;
                    OnPropertyChanged(nameof(ImageGenväg));
                }
            }
        }
        #endregion


        private string Genväg(string genvag) => ÄrKort ? $"pack://application:,,,/Spelkort/{Färg}{Valör}.png" : $"pack://application:,,,/Spelkort/{genvag}.png";

        private string BeräknaValör(int kort)
        {
            if (kort >1 && kort < 10) return kort.ToString();
            switch (kort)
            {
                case 10:
                    return "T";
                case 11:
                    return "J";
                case 12:
                    return "Q";
                case 13:
                    return "K";
                case 1:
                    return "A";
                default:
                    throw new ArgumentOutOfRangeException(nameof(kort), "Kortvärdet är utanför det förväntade intervallet.");
            }
        }
    }
}
