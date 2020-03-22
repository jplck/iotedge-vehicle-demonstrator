using AzureMapsToolkit.Common;
using AzureMapsToolkit.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace LocationModule
{
    class Maps
    {
        private string mapsKey = "";
        const string CoordinateDigitFormat = "0.00";
        const string SpeedLimitSuffix = "KPH";
        private readonly AzureMapsToolkit.AzureMapsServices toolkit;

        public Maps(string key)
        {
            mapsKey = key;
            toolkit = new AzureMapsToolkit.AzureMapsServices(mapsKey);
        }

        public async Task<RouteDirectionsResult?> GetRoute(string from, string to)
        {

            var start = GetLocationFromString(from);
            var end = GetLocationFromString(to);

            if (start != null && end != null)
            {
                var lat1 = start.Result.Value.lat;
                var lon1 = start.Result.Value.lon;
                var lat2 = end.Result.Value.lat;
                var lon2 = end.Result.Value.lon;

                RouteRequestDirections directions = new RouteRequestDirections();
                directions.Query = $"{lat1},{lon1}:{lat2},{lon2}";
                directions.SectionType = SectionType.traffic;

                var route = await toolkit.GetRouteDirections(directions);
                return route.Result.Routes.Length > 0 ? route.Result.Routes[0] : null;
            }

            return null;
        }

        private async Task<(string lat, string lon)?> GetLocationFromString(string query)
        {
            SearchAddressRequest locRequest = new SearchAddressRequest
            {
                Query = query
            };

            var loc = await toolkit.GetSearchAddress(locRequest);
            if (loc.Result.Results.Length > 0)
            {
                var position = loc.Result.Results[0].Position;
                var lat = ConvertCoordinateToString(position.Lat);
                var lon = ConvertCoordinateToString(position.Lon);

                await GetSpeedLimit(position.Lat, position.Lon);

                return (lat, lon); 
            }

            return null;
        }

        private string ConvertCoordinateToString(double coord)
        {
            return coord.ToString(CoordinateDigitFormat, new CultureInfo("en-us", false));
        }

        public async Task<double?> GetSpeedLimit(double lat, double lon) 
        {
            var req = new SearchAddressReverseRequest
            {
                Query = $"{ConvertCoordinateToString(lat)},{ConvertCoordinateToString(lon)}",
                ReturnSpeedLimit = true
            };

            var searchResults = await toolkit.GetSearchAddressReverse(req);
            if (searchResults.Error == null)
            {
                var addresses = searchResults.Result.Addresses;
                if (addresses.Length > 0)
                {
                    var speedLimit = addresses[0].Address.SpeedLimit;
                    if (speedLimit != null && speedLimit != String.Empty)
                    {
                        try
                        {
                            var value = speedLimit.Replace(SpeedLimitSuffix, String.Empty);
                            return double.Parse(value);
                        } catch (FormatException formatException)
                        {
                            return null;
                        }
                        
                    }
                }
            }

            return null;
        }
    }
}
