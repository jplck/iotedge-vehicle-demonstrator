using System.Xml;
using System.Xml.Serialization;

namespace GPXParserLib
{
    public static class GPXReader
    {
        public static GPX Read(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GPX));
            XmlReader reader = XmlReader.Create(filename);
            GPX gpxObject = (GPX)serializer.Deserialize(reader);
            return gpxObject;
        }
    }
}
