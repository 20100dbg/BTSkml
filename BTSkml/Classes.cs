﻿using System;
using System.Collections.Generic;

namespace BTSkml
{
    class BTS
    {
        public List<Cell> Antennes { get; set; }
        public Coord Coord { get; set; }
        public int PyloneId { get; set; }
    }

    class Cell
    {
        public int CellId { get; set; }
        public float Azm { get; set; }
    }

    class Coord
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    class Style
    {
        public string linecolor { get; set; }
        public string areacolor { get; set; }
    }
}
