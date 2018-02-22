using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using HealthParse.Standard.Health.Sheets;

namespace HealthParse.Standard.Health.Export
{
    public class XmlReaderExportLoader :IExportLoader
    {
        public IList<Workout> Workouts { get; }
        public IList<Record> Records { get; }

        public XmlReaderExportLoader(Stream data)
        {
            var elements = Read(data)
                .Select(e =>
                {
                    var isRecord = e.Item1 == "Record";
                    var record = isRecord ? RecordParser.ParseRecord(e.Item2) : null;
                    var workout = isRecord ? null : WorkoutParser.Dictionary.ParseWorkout(e.Item2);
                    return new {isRecord, record, workout};
                })
                .Aggregate(Tuple.Create(new List<Record>(), new List<Workout>()), (acc, item) =>
                    {
                        if (item.isRecord)
                        {
                            acc.Item1.Add(item.record);
                        }
                        else
                        {
                            acc.Item2.Add(item.workout);
                        }
                        return acc;
                    });

            Records = elements.Item1;
            Workouts = elements.Item2;
        }

        private IEnumerable<Tuple<string, Dictionary<string, string>>> Read(Stream data)
        {
            using (var reader = XmlReader.Create(data,
                new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    CloseInput = false,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true,
                }))
            {
                reader.MoveToContent();
                while (!reader.EOF)
                {
                    if (!SkipToElement(reader, "Record", "Workout"))
                    {
                        break;
                    }

                    var element = Tuple.Create(reader.Name, new Dictionary<string, string>());
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToAttribute(i);
                        var name = reader.Name;
                        reader.ReadAttributeValue();
                        var value = reader.Value;
                        element.Item2.Add(name, value);
                    }

                    yield return element;
                }
            }
        }
        private static bool SkipToElement(XmlReader xmlReader, params string[] elementNames)
        {
            if (!xmlReader.Read())
                return false;

            while (!xmlReader.EOF)
            {
                if (
                    xmlReader.NodeType == XmlNodeType.Element &&
                    elementNames.Any(e => e == xmlReader.Name))
                    return true;

                xmlReader.Skip();
            }

            return false;
        }
    }
}
