using System;
using System.Xml.Serialization;

namespace GPXParserLib
{
    public class GPXGenericItem
    {
        [XmlAttribute(AttributeName = "lat")]
        public double Lat { get; set; }

        [XmlAttribute(AttributeName = "lon")]
        public double Lon { get; set; }

        [XmlElement(ElementName = "ele")]
        public string Elevation { get; set; }

        [XmlElement(ElementName = "time")]
        public DateTime Time { get; set; }

        [XmlElement(ElementName = "magvar")]
        public double Declination { get; set; }

        [XmlElement(ElementName = "geoidheight")]
        public double Geoidheight { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "cmt")]
        public string Comment { get; set; }

        [XmlElement(ElementName = "desc")]
        public string Description { get; set; }

        [XmlElement(ElementName = "src")]
        public string DataSource { get; set; }

        [XmlElement(ElementName = "link")]
        public string Link { get; set; }

        [XmlElement(ElementName = "sym")]
        public string Symbol { get; set; }

        [XmlElement(ElementName = "type")]
        public string Type { get; set; }

        [XmlElement(ElementName = "fix")]
        public string FixType { get; set; }

        [XmlElement(ElementName = "sat")]
        public byte NumberOfSatellites { get; set; }

        [XmlElement(ElementName = "hdop")]
        public double HDOP { get; set; }

        [XmlElement(ElementName = "vdop")]
        public double VDOP { get; set; }

        [XmlElement(ElementName = "pdop")]
        public double PDOP { get; set; }

        [XmlElement(ElementName = "ageofdgpsdata")]
        public long AgeOfDGPSData { get; set; }

        [XmlElement(ElementName = "dgpsid")]
        public int DGPSID { get; set; }
    }
}
