using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bev.IO.PerkinElmerReader
{
    public class PEReader
    {
        private string[] lines;
        private List<Tupel> tupels = new List<Tupel>();


        public void LoadFile(string filename)
        {
            try
            {
                string allText = File.ReadAllText(filename);
                if (string.IsNullOrWhiteSpace(allText))
                {
                    return;
                }
                lines = Regex.Split(allText, "\r\n|\r|\n");
            }
            catch (Exception)
            {
                return;
            }
        }

        public string ExtractOwner() => ExtractLine(7);
        public string ExtractSampleName() => ExtractLine(8);
        public string ExtractInstrumentType() => ExtractLine(11);
        public string ExtractInstrumentSerialNumber() => ExtractLine(12);
        public string ExtractInstrumentSoftware() => ExtractLine(13);
        public string ExtractXUnit() => ExtractLine(GetIndexOfUnits() + 1);
        public string ExtractYUnit() => ExtractLine(GetIndexOfUnits() + 2);
        public DateTime MeasurementDate()
        {
            string s = $"{ExtractLine(3)} {ExtractLine(4)}";
            string format = "yy/MM/dd HH:mm:ss.ff";
            try
            {
                return DateTime.ParseExact(s, format, null);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public bool ValidSyntax()
        {
            int minFileLength = 10;
            string magicLine = "PE UV       SUBTECH     SPECTRUM    ASCII       PEDS        4.00        "; //TODO 4.00 might be a version number
            if (lines == null) 
                return false;
            if (lines.Length < minFileLength)
                return false;
            if (!lines[0].Contains(magicLine))
                return false;
            if (GetIndexOfKey("#DATA") <= 0)
                return false;
            if (GetIndexOfKey("#HDR") <= 0)
                return false;
            if (GetIndexOfKey("#GR") <= 0)
                return false;
            return true;
        }

        private string ExtractLine(int lineNumber)
        {
            if (lines == null) return string.Empty;
            if (lineNumber >= lines.Length) return string.Empty;
            return lines[lineNumber].Trim();
        }

        public void ExtractSpectrum()
        {
            int startIndex = GetIndexOfSpectrum() + 1;
            if (startIndex >= lines.Length) 
                return;
            for (int i = startIndex; i < lines.Length; i++)
            {
                Tupel tupel = ParseToTupel(lines[i]);
                if (tupel.IsValid) tupels.Add(tupel);
            }
        }

        private int GetIndexOfSpectrum() => GetIndexOfKey("#DATA");
        
        private int GetIndexOfUnits() => GetIndexOfKey("#GR");

        private int GetIndexOfKey(string keyword)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(keyword))
                    return i;
            }
            return -1; //TODO
        }

        private Tupel ParseToTupel(string dataLine)
        {
            string[] tokens = dataLine.Split(new[] { ' ', '=', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 2) 
                return new Tupel(double.NaN, double.NaN);
            double x = ParseToDouble(tokens[0]);
            double y = ParseToDouble(tokens[1]);
            return new Tupel(x, y);
        }

        private double ParseToDouble(string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                return result;
            return double.NaN;
        }

        private int ParseToInt(string value)
        {
            if (int.TryParse(value, out int result))
                return result;
            return -1; //TODO -1 is valid in some cases
        }

    }

    public enum FileType
    {
        Unknown,
        Asc,
        JcampDx,
        Dat
    }

}
