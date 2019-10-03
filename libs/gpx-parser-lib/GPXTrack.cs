using System.Collections.Generic;
using System.Xml.Serialization;

namespace GPXParserLib
{
    [XmlType("trk")]
    public class GPXTrack
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("trkseg")]
        public List<GPXTrackSegment> TrackSegments { get; set; }
    }
}
