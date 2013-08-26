using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class Identifiable
    {
        public static int lastID;
        public int ID { get; set; }

        Identifiable() {
            ID = lastID + 1;
            lastID = ID;
        }
    }
}
