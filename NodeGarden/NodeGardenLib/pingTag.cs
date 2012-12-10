using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace NodeGardenLib
{
    public static class PinTagExtensions
    {
        public static PingTag Deserialize(this string source)
        {
            try
            {
                return JsonConvert.DeserializeObject<PingTag>(source);
            }
            catch
            {
                return new PingTag();
            }
        }

        public static string Serialize(this PingTag source)
        {
            return JsonConvert.SerializeObject(source);
        }
    }

    public class PingTag
    {
        public PingTag()
        {
            AccentColour = Colors.White;
        }

        [JsonProperty("ac")]
        public Color AccentColour { get; set; }
        [JsonProperty("p")]
        public bool Ping { get; set; }
    }
}
