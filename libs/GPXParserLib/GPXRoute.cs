using System.Collections.Generic;
using System.Xml.Serialization;

namespace VehicleDemonstrator.Shared.GPX.Route
{
    [XmlType("rte")]
    public class GPXRoute
    {
        [XmlElement("rtept")]
        public List<GPXRoutePoint> RoutePoints;
    }
}
