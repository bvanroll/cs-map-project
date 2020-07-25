using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;


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
                    case "MultiPolygon":
                        _polygons.Add(new PolygonPunten(JsonConvert.DeserializeObject<Polygon>(feature["geometry"]
                        .ToString()), feature["properties"]["name"].ToString()));
                        break;
                    case "Polygon":
                        _multiPolygons.Add(new MultiPolygonPunten(JsonConvert.DeserializeObject<MultiPolygon>
                        (feature["geometry"].ToString()), feature["properties"]["name"].ToString()));
                        break;
                }
            }
        }
        
        
        
    }
}