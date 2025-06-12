using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harpan
{
    public class CellList
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public ObservableCollection<Spelkort> Cards { get; set; } = new ObservableCollection<Spelkort>();
    }
}
