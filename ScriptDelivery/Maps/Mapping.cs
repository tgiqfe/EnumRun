
namespace ScriptDelivery.Maps
{
    internal class Mapping
    {
        public string Name { get; set; }

        public ScriptDelivery.Maps.Requires.Require Require { get; set; }

        public ScriptDelivery.Maps.Works.Work Work { get; set; }
    }
}
