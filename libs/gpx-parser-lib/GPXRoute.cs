using System.Collections.Generic;
using System.Xml.Serialization;

namespace GPXParserLib
{
    [XmlType("rte")]
    public class GPXRoute
    {
        [XmlElement("rtept")]
        public List<GPXRoutePoint> RoutePoints;
    }
}
