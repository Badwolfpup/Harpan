using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harpan
{
    public static class HanteraKortlek
    {
        private static Random _rnd = new Random();

        private static List<Spelkort> SkapaKortlek()
        {
            List<Spelkort> kortlek = new List<Spelkort>();
            List<string> färger = new List<string> { "H", "R", "S", "K" };
            foreach (var färg in färger)
            {
                for (int i = 1; i <= 13; i++)
                {
                    kortlek.Add(new Spelkort(färg, i) { Högtyp = Högtyp.Tahög });
                }
            }
            return kortlek;
        }

        public static List<Spelkort> BlandaKortlek()
        {
            var kortlek = SkapaKortlek();
            int n = kortlek.Count;
            while (n > 1)
            {
                n--;
                int k = _rnd.Next(n + 1);
                Spelkort temp = kortlek[k];
                kortlek[k] = kortlek[n];
                kortlek[n] = temp;
            }
            return kortlek;
        }
    }
}
