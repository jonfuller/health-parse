namespace HealthParse.Standard.Settings
{
    public class Setting
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public object DefaultValue { get; set; }
        public string Description { get; set; }
        public SerializationBehavior JsonSerialization { get; set; }
        public SerializationBehavior ExcelSerialization { get; set; }
    }
}