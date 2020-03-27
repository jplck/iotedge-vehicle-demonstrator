using AzureMapsToolkit.Common;
using AzureMapsToolkit.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace LocationModule
{
    [Serializable]
    class UnexpectedResultException: Exception
    {
        public UnexpectedResultException(string variableName)
            : base($"A variable {variableName} contains an unexpected value or is null.")
        {}

        public UnexpectedResultException(string variableName, string message)
            : base($"A variable {variableName} contains an unexpected value or is null. ({message})")
        { }
    }

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
            try
            {
                var start = await GetLocationFromString(from);
                var end = await GetLocationFromString(to);

                var lat1 = start.lat;
                var lon1 = start.lon;
                var lat2 = end.lat;
                var lon2 = end.lon;

                RouteRequestDirections directions = new RouteRequestDirections();
                directions.Query = $"{lat1},{lon1}:{lat2},{lon2}";
                directions.SectionType = SectionType.traffic;

                var route = await toolkit.GetRouteDirections(directions);
                return route.Result.Routes.Length > 0 ? route.Result.Routes[0] : null;
            }
            catch (UnexpectedResultException ex)
            {
                Console.WriteLine($"Could not generate route. Due to error ({ex.Message})");
            }

            return null;
        }

        private async Task<(string lat, string lon)> GetLocationFromString(string query)
        {
            _ = query ?? throw new ArgumentNullException("query", "Query parameter is required.");

            SearchAddressRequest locRequest = new SearchAddressRequest
            {
                Query = query
            };

            var loc = await toolkit.GetSearchAddress(locRequest);

            var position = loc?.Result?.Results[0]?.Position;

            _ = position ?? throw new UnexpectedResultException("position");

            var lat = ConvertCoordinateToString(position.Lat);
            var lon = ConvertCoordinateToString(position.Lon);
            
            return (lat, lon);
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

            try
            {
                return double.Parse(searchResults?.Result?.Addresses[0]?.Address.SpeedLimit?.Replace(SpeedLimitSuffix, string.Empty));
            }
            catch (FormatException _)
            {
                return null;
            }
        }
    }
}
