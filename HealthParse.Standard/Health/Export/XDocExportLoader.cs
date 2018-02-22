using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HealthParse.Standard.Health.Sheets;

namespace HealthParse.Standard.Health.Export
{
    public class XDocExportLoader : IExportLoader
    {
        public XDocExportLoader(Stream stream)
        {
            var export = XDocument.Load(stream);
            Records = export.Descendants("Record").Select(RecordParser.FromXElement).ToList();
            Workouts = export.Descendants("Workout").Select(WorkoutParser.X.ParseWorkout).ToList();

        }
        public IList<Workout> Workouts { get; }
        public IList<Record> Records { get; }
    }
}
