using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mastersign.Tools;

namespace Mastersign.WinJockey
{
    // https://stackoverflow.com/a/58043033

    public static class WakeOnLan
    {
        public static Task SendMagicPacket(string macAddress, string subnet)
        {
            if (string.IsNullOrEmpty(subnet)) return SendMagicPacket(macAddress, subnetAddress: null);
            var subnetParts = subnet.Split(new[] { '/' }, 2);
            if (subnetParts.Length != 2)
                throw new ArgumentException("Invalid syntax for subnet. Expected CIDR format.", nameof(subnet));
            var subnetAddress = IPAddress.Parse(subnetParts[0]);
            var prefixLength = int.Parse(subnetParts[1]);
            return SendMagicPacket(macAddress, subnetAddress, prefixLength);
        }

        public static async Task SendMagicPacket(string macAddress, IPAddress subnetAddress = null, int prefixLength = 24)
        {
            var magicPacket = BuildMagicPacket(macAddress);
            foreach (var networkInterface in GetActiveInterfaces())
            {
                var ipInterfaceProps = networkInterface.GetIPProperties();
                if (subnetAddress != null
                    && !ipInterfaceProps.UnicastAddresses.Any(a => InSubnet(a.Address, subnetAddress, prefixLength)))
                {
                    continue;
                }
                foreach (var multicastIPAddressInformation in ipInterfaceProps.MulticastAddresses)
                {
                    var multicastIpAddress = multicastIPAddressInformation.Address;
                    if (multicastIpAddress.ToString().StartsWith("ff02::1%", StringComparison.OrdinalIgnoreCase)) // Ipv6: All hosts on LAN (with zone index)
                    {
                        var unicastIPAddressInformation = ipInterfaceProps.UnicastAddresses.Where((u) =>
                            u.Address.AddressFamily == AddressFamily.InterNetworkV6 && !u.Address.IsIPv6LinkLocal).FirstOrDefault();
                        if (unicastIPAddressInformation != null)
                        {
                            await SendMagicPacket(unicastIPAddressInformation.Address, multicastIpAddress, magicPacket);
                        }
                    }
                    else if (multicastIpAddress.ToString().Equals("224.0.0.1")) // Ipv4: All hosts on LAN
                    {
                        var unicastIPAddressInformation = ipInterfaceProps.UnicastAddresses
                            .Where((u) =>
                                u.Address.AddressFamily == AddressFamily.InterNetwork && 
                                !ipInterfaceProps.GetIPv4Properties().IsAutomaticPrivateAddressingActive)
                            .FirstOrDefault();
                        if (unicastIPAddressInformation != null)
                        {
                            await SendMagicPacket(unicastIPAddressInformation.Address, multicastIpAddress, magicPacket);
                        }
                    }
                }
            }
        }

        private static IEnumerable<NetworkInterface> GetActiveInterfaces() 
            => NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => 
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    n.OperationalStatus == OperationalStatus.Up);

        private static bool InSubnet(IPAddress address, IPAddress subnet, int prefixLength)
        {
            var addressBytes = address.GetAddressBytes();
            var subnetBytes = subnet.GetAddressBytes();

            for (var i = 0; i < prefixLength / 8; i++)
            {
                if (addressBytes[i] != subnetBytes[i]) return false;
            }

            var remainingBits = prefixLength % 8;
            if (remainingBits > 0)
            {
                var addressByte = addressBytes[prefixLength / 8];
                var subnetAddressByte = subnetBytes[prefixLength / 8];

                var mask = (byte)~((1 << (8 - remainingBits)) - 1);
                if ((subnetAddressByte & mask) != (addressByte & mask)) return false;
            }
            return true;
        }

        private static byte[] BuildMagicPacket(string macAddress) // MacAddress in any standard HEX format
        {
            macAddress = Regex.Replace(macAddress, "[: -]", "");
            var macBytes = macAddress.ParseHex();

            var header = Enumerable.Repeat((byte)0xff, 6); //First 6 times 0xff
            var data = Enumerable.Repeat(macBytes, 16).SelectMany(m => m); // then 16 times MacAddress
            return header.Concat(data).ToArray();
        }

        private static async Task SendMagicPacket(IPAddress localIpAddress, IPAddress multicastIpAddress, byte[] magicPacket)
        {
            using (var client = new UdpClient(new IPEndPoint(localIpAddress, 0)))
            {
                await client.SendAsync(magicPacket, magicPacket.Length, new IPEndPoint(multicastIpAddress, 9));
            }
        }
    }
}