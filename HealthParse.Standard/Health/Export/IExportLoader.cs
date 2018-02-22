using System.Collections.Generic;

namespace HealthParse.Standard.Health.Export
{
    public interface IExportLoader
    {
        IList<Workout> Workouts { get; }
        IList<Record> Records { get; }
    }
}
