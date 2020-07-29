using System.Collections.Generic;
using System.IO;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Globals;


namespace Datalaag
{
    public class JsonReader
    {
        public List<PolygonPunten> _polygons;
        public List<MultiPolygonPunten> _multiPolygons;
        public JsonReader(string path)
        {
            _polygons = new List<PolygonPunten>();
            _multiPolygons = new List<MultiPolygonPunten>();
            foreach (JObject feature in JObject.Parse(File.ReadAllText(path))["features"])
            {
                switch (feature["geometry"]["type"].ToString())
                {
                    case "Polygon":
                        _polygons.Add(new PolygonPunten(JsonConvert.DeserializeObject<Polygon>(feature["geometry"]
                            .ToString()), feature["properties"]["name"].ToString()));
                        break;
                    case "MultiPolygon":
                        _multiPolygons.Add(new MultiPolygonPunten(JsonConvert.DeserializeObject<MultiPolygon>
                            (feature["geometry"].ToString()), feature["properties"]["name"].ToString()));
                        break;
                }
            }
        }
    }
}