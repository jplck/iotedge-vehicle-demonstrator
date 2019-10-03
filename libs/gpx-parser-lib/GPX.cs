using System.Collections.Generic;
using System.Xml.Serialization;

namespace GPXParserLib
{
    [XmlRoot(ElementName = "gpx", Namespace = "http://www.topografix.com/GPX/1/1")]
    public class GPX
    {
        [XmlElement("rte")]
        public List<GPXRoute> Routes { get; set; }

        [XmlElement("trk")]
        public List<GPXTrack> Tracks { get; set; }

        [XmlElement("wpt")]
        public List<GPXWaypoint> Waypoints { get; set; }
    }
}
