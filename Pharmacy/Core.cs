using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy
{
    internal class Core
    {
        public static PharmacyEntities Context = new PharmacyEntities();
        public static Users CurrentUser {  get; set; }

    }

}
