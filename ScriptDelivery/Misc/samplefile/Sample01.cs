using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptDelivery.Maps;
using ScriptDelivery.Maps.Works;
using ScriptDelivery.Maps.Requires;

namespace ScriptDelivery.Misc.samplefile
{
    public class Sample01
    {
        public static void Create01()
        {
            var mapping = new Mapping();
            mapping.Require = new Require();
            mapping.Require.Mode = "and";
            mapping.Require.Rules = new RequireRule[]
            {
                new RequireRule()
                {
                    Target = "HostName",
                    Match = "Equal",
                    Param = new Dictionary<string, string>()
                    {
                        { "name", "HOSTNAME01" }
                    },
                },
                new RequireRule()
                {
                    Target = "IPAddress",
                    Param = new Dictionary<string, string>()
                    {
                        { "Address", "192.168.10.51" },
                        { "Interface", "Ethernet*" }
                    },
                }
            };
            mapping.Work = new Work();
            mapping.Work.Downloads = new Download[]
            {
                new Download()
                {
                    Source = "example001.txt",
                    Destination = @"C:\App\Sample\Example001.txt",
                    Keep = "true",
                },
                new Download()
                {
                    Source = "example002.txt",
                    Destination = @"C:\App\Sample\Example002.txt",
                },
            };

            var list = new List<Mapping>() { mapping };
            MappingGenerator.Serialize(list, @"bin\sample01.yml");
            MappingGenerator.Serialize(list, @"bin\sample01.csv");
            MappingGenerator.Serialize(list, @"bin\sample01.txt");
        }
    }
}
