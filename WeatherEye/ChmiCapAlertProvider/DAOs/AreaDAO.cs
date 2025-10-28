using CAP;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ChmiCapAlertProvider.DAOs
{
    public class AreaDAO
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string AreaDesc { get; set; }
        public List<string> Polygons { get; set; }
        public List<string> Circles { get; set; }
        public Dictionary<string, string> Geocodes { get; set; }
        public decimal? Altitude { get; set; }
        public decimal? Ceiling { get; set; }

        public AreaDAO()
        {

        }
        public AreaDAO(AlertInfoArea area)
        {
            AreaDesc = area.AreaDesc;
            Polygons = new List<string>();
            Polygons.AddRange(area.Polygon);
            Circles = new List<string>();
            Circles.AddRange(area.Circle);
            Geocodes = new Dictionary<string, string>();
            foreach (var g in area.Geocode)
            {
                Geocodes.Add(g.Value, g.ValueName);
            }
            Altitude = area.Altitude;
            Ceiling = area.Ceiling;
        }
    }
}
