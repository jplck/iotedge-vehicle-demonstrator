using System.Collections.Generic;
using System.Xml.Serialization;
using VehicleDemonstrator.Shared.GPX.Route;
using VehicleDemonstrator.Shared.GPX.Track;
using VehicleDemonstrator.Shared.GPX.Waypoint;

namespace VehicleDemonstrator.Shared.GPX
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
