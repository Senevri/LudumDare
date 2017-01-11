using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class Location
    {
        //public static int lastID;
        public int ID { get; set; } 
        public float X { get; set; }
        public float Y { get; set; }
        public string Type { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public Location() { 
        
        }
        
        // get size etc. Whatevs.
    }
}
