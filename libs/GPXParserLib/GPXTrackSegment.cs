using System.Collections.Generic;
using System.Xml.Serialization;

namespace VehicleDemonstrator.Shared.GPX.Track
{
    [XmlType("trkseg")]
    public class GPXTrackSegment
    {
        [XmlElement("trkpt")]
        public List<GPXTrackPoint> TrackPoints;
    }
}
