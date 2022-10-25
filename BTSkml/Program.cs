using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BTSkml
{
    internal class Program
    {
        static List<Style> listStyle = new List<Style>();
        static float distance = 2;

        static void Main(string[] args)
        {
            
            listStyle.Add(new Style { areacolor = "66DD0000", linecolor = "FF000000" });
            listStyle.Add(new Style { areacolor = "6600DD00", linecolor = "FF000000" });
            listStyle.Add(new Style { areacolor = "660000DD", linecolor = "FF000000" });

            List<BTS> listBTS = readCSV("data.csv");

            DrawKML(listBTS);

            Console.Read();
        }


        static List<BTS> readCSV(string filename)
        {
            List<BTS> listBTS = new List<BTS>();

            using (StreamReader sr = new StreamReader(filename))
            {
                string line;
                int lastId = -1;
                bool firstLine = true;

                while ((line = sr.ReadLine()) != null)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                        continue;
                    }

                    string[] tabLine = line.Split(new char[] { ';' });
                    double x = double.Parse(FixDecSeparator(tabLine[0]));
                    double y = double.Parse(FixDecSeparator(tabLine[1]));
                    int pyloneId = int.Parse(tabLine[2]);
                    int cellId = int.Parse(tabLine[3]);
                    float azm = float.Parse(FixDecSeparator(tabLine[4]));

                    if (lastId != pyloneId)
                    {
                        listBTS.Add(new BTS { 
                            Coord = new Coord { X = x, Y = y},
                            Antennes = new List<Cell>(),
                            PyloneId = pyloneId
                        });
                        lastId = pyloneId;
                    }

                    listBTS[listBTS.Count - 1].Antennes.Add(new Cell
                    {
                        CellId = cellId,
                        Azm = azm
                    });

                }

                return listBTS;
            }
        }


        static void DrawKML(List<BTS> listBTS)
        {
            string tplStyle = "<Style id=\"area{idstyle}\"><LineStyle><color>{linecolor}</color><width>1</width></LineStyle>" +
                                "<PolyStyle><color>{areacolor}</color><fill>1</fill><outline>1</outline></PolyStyle></Style>";
            
            string tplArea = "<Placemark><description></description>{attributs}<styleUrl>#area{idstyle}</styleUrl><Polygon><outerBoundaryIs><LinearRing><coordinates>{coord}</coordinates></LinearRing></outerBoundaryIs></Polygon></Placemark>";

            string tplAttributs = "<pyloneId>{pyloneId}</pyloneId><cellId>{cellId}</cellId><azm>{azm}</azm>";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml><Document>");

            for (int i = 0; i < listStyle.Count; i++)
            {
                sb.AppendLine(tplStyle.Replace("{idstyle}", i.ToString()).Replace("{linecolor}", listStyle[i].linecolor).Replace("{areacolor}", listStyle[i].areacolor));
            }

            sb.AppendLine("<Folder><name>Area Features</name><description>Area Features</description>");

            for (int i = 0; i < listBTS.Count; i++)
            {
                if (listBTS[i].Antennes.Count == 0) continue;


                for (int j = 0; j < listBTS[i].Antennes.Count; j++)
                {
                    float azmDeb = (360 + listBTS[i].Antennes[j].Azm - 60) % 360;
                    float azmFin = (listBTS[i].Antennes[j].Azm + 60) % 360;

                    string coord = listBTS[i].Coord.X.ToString().Replace(",", ".") + "," + listBTS[i].Coord.Y.ToString().Replace(",", ".") + ",0" + Environment.NewLine;

                    Coord c = GetCible(listBTS[i].Coord, azmDeb, distance);
                    coord += c.X.ToString().Replace(",", ".") + "," + c.Y.ToString().Replace(",", ".") + ",0" + Environment.NewLine;

                    c = GetCible(listBTS[i].Coord, azmFin, distance);
                    coord += c.X.ToString().Replace(",", ".") + "," + c.Y.ToString().Replace(",", ".") + ",0" + Environment.NewLine;

                    string attr = tplAttributs.Replace("{pyloneId}", listBTS[i].PyloneId.ToString()).Replace("{cellId}", listBTS[i].Antennes[j].CellId.ToString()).Replace("{azm}", listBTS[i].Antennes[j].Azm.ToString());

                    sb.AppendLine(tplArea.Replace("{idstyle}", i.ToString()).Replace("{coord}", coord).Replace("{attributs}", attr));
                }
                
            }

            sb.AppendLine("</Folder></Document></kml>");



            using (StreamWriter sw = new StreamWriter("zonesBTS.kml", false))//, Encoding.GetEncoding(1252)))
            {
                sw.Write(sb.ToString());
            }
        }

        static string FixDecSeparator(string s)
        {
            string currentSep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            string oldSeparator = (currentSep == ",") ? "." : ",";
            s = s.Replace(oldSeparator, currentSep);
            return s;
        }

        static Coord GetCible(Coord capteur, float azm, float distance)
        {
            double cibleY = Math.Asin(Math.Sin(capteur.Y * Math.PI / 180) * Math.Cos(distance / 6371) + Math.Cos(capteur.Y * Math.PI / 180) * Math.Sin(distance / 6371) * Math.Cos(azm * Math.PI / 180)) * 180 / Math.PI;
            double cibleX = (capteur.X * Math.PI / 180 + Math.Atan2(Math.Sin(azm * Math.PI / 180) * Math.Sin(distance / 6371) * Math.Cos(cibleY * Math.PI / 180), Math.Cos(distance / 6371) - Math.Sin(cibleY * Math.PI / 180) * Math.Sin(cibleY * Math.PI / 180))) * 180 / Math.PI;

            return new Coord { X = cibleX, Y = cibleY };
        }

    }
}
