using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxTest
{
    enum ElementType { pdf, img, dir, exe, file, back }
    class ItemInformation
    {
        public ElementType Type{ get; set; }
        public string Name{ get; set; }
        public string Fullpath{ get; set; }
        public ulong Size { get; set; }
        public string Share_url { get; set; }
        public DateTime EditDate { get; set; }
        public string GetSize()
        {
            return Size.ToString();
        }
        public string GetFormatedDate()
        {
            //https://msdn.microsoft.com/ru-ru/library/8kb3ddd4(v=vs.110).aspx
            return EditDate.ToString("dd MMMM yyyy; H:mm");
        }
    }
}
