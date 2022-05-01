using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.NetworkInformation;
using System.Management;

namespace EnumRun.Log
{
    internal class NicCollection : List<Nic>
    {
        private JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public NicCollection()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces().Where(x =>
                x.OperationalStatus == OperationalStatus.Up &&
                x.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                x.NetworkInterfaceType != NetworkInterfaceType.Loopback))
            {
                this.Add(new Nic(
                    ni,
                    new ManagementClass("Win32_NetworkAdapterConfiguration").
                        GetInstances().
                        OfType<ManagementObject>(),
                    new ManagementClass("Win32_NetworkAdapter").
                        GetInstances().
                        OfType<ManagementObject>()));
            }
        }

        public string GetJson()
        {
            return JsonSerializer.Serialize(this, _options);
        }
    }
}
