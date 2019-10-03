using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace TelemetryLib
{
    public class TelemetrySegmentFactory<T>
    {
        public static T FromJsonString(string json)
        {
            var deserializedObj = JsonConvert.DeserializeObject<T>(json, 
                new JsonSerializerSettings
                {
                    Error = delegate(object sender, ErrorEventArgs args)
                    {
                        Console.WriteLine(args.ToString());
                    }
                }
            );
            return deserializedObj;
        }

    }
}
