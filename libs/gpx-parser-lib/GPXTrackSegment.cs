using System.Collections.Generic;
using System.Xml.Serialization;

namespace GPXParserLib
{
    [XmlType("trkseg")]
    public class GPXTrackSegment
    {
        [XmlElement("trkpt")]
        public List<GPXTrackPoint> TrackPoints;
    }
}
