using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxTest
{
    public class SpaceInfo
    {
        public ulong FullSpace { get; set; }
        public ulong UsedSpace { get; set; }
        public double Procent { get; private set; }
        public SpaceInfo(ulong fullSpace, ulong usedSpace)
        {
            FullSpace = fullSpace;
            UsedSpace = usedSpace;
            Procent = ((double)UsedSpace / (double)FullSpace)*100;
        }
        public string GetInfoString()
        {
           return UsedSpace + "/" + FullSpace;
        }

    }
}
