using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Read-only IP sub-net list
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly record struct IpSubNets : IEnumerable<IpSubNet>
    {
        /// <summary>
        /// IP sub-nets
        /// </summary>
        private readonly IpSubNet[] SubNets;
        /// <summary>
        /// Number of IP sub-nets
        /// </summary>
        public readonly int Count;
        /// <summary>
        /// Are all contained IP sub-nets loopback sub-nets?
        /// </summary>
        public readonly bool IsLoopback;
        /// <summary>
        /// Are all contained IP sub-nets LAN sub-nets?
        /// </summary>
        public readonly bool IsLan;
        /// <summary>
        /// Are all contained IP sub-nets WAN sub-nets?
        /// </summary>
        public readonly bool IsWan;
        /// <summary>
        /// IP sub-nets address family (<see cref="AddressFamily.Unknown"/>, if empty, or <see cref="AddressFamily.Unspecified"/> if mixed)
        /// </summary>
        public readonly AddressFamily AddressFamily;
        /// <summary>
        /// Contained IP network kinds
        /// </summary>
        public readonly IpNetworkKind NetworkKind;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="subNets">IP sub-nets</param>
        public IpSubNets(in IpSubNet[] subNets) : this(subNets, isLoopBack: null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="subNets">IP sub-nets</param>
        public IpSubNets(params IpSubNet[] subNets) : this(subNets, isLoopBack: null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="subNets">IP sub-nets</param>
        /// <param name="isLoopBack">Are all contained IP sub-nets loopback sub-nets?</param>
        /// <param name="isLan">Are all contained IP sub-nets LAN sub-nets?</param>
        internal IpSubNets(in IpSubNet[] subNets, bool? isLoopBack, bool? isLan = null)
        {
            if (subNets.LongLength > int.MaxValue) throw new OutOfMemoryException();
            SubNets = subNets;
            Count = subNets.Length;
            // Determine the address family
            if (Count == 0)
            {
                AddressFamily = AddressFamily.Unknown;
            }
            else
            {
                bool v4 = false,
                    v6 = false;
                for (int i = 0; i < Count && (!v4 || !v6); v4 |= subNets[i].IsIPv4, v6 |= !subNets[i].IsIPv4, i++) ;
                AddressFamily = v4 && v6
                    ? AddressFamily.Unspecified
                    : v4
                        ? AddressFamily.InterNetwork
                        : AddressFamily.InterNetworkV6;
            }
            // Determine the overall network kinds
            IpNetworkKind kind = IpNetworkKind.None;
            if (!isLoopBack.HasValue && !isLan.HasValue)
            {
                for (int i = 0; i < Count && kind != IpNetworkKind.ALL; kind |= subNets[i].NetworkKind, i++) ;
            }
            else
            {
                kind = isLoopBack.HasValue && isLoopBack.Value ? IpNetworkKind.Loopback : IpNetworkKind.LAN;
            }
            NetworkKind = kind;
            // Determine if all contained IP sub-nets are loopback sub-nets
            if (isLoopBack.HasValue) IsLoopback = isLoopBack.Value;
            else if (IsLoopback = Count != 0) for (int i = 0; i < Count && IsLoopback; IsLoopback &= NetworkHelper.LoopBack.Matches(subNets[i]), i++) ;
            // Determine if all contained IP sub-nets are LAN sub-nets
            if(isLan.HasValue) IsLan = isLan.Value;
            else if (IsLan = Count != 0) for (int i = 0; i < Count && IsLan; IsLan &= NetworkHelper.LAN.Matches(subNets[i]), i++) ;
            // Determine if all contained IP sub-nets are WAN sub-nets
            IsWan = Count != 0 && !IsLoopback && !IsLan;
        }

        /// <summary>
        /// Get an IP sub-net
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns><see cref="IpSubNet"/></returns>
        public IpSubNet this[in int index] => SubNets[index];

        /// <summary>
        /// Get an IP sub-net
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns><see cref="IpSubNet"/></returns>
        public IpSubNet this[in Index index] => SubNets[index];

        /// <summary>
        /// Determine if an IP sub-net is contained
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>If contained</returns>
        public bool Contains(in IpSubNet subNet)
        {
            if (Count == 0) return false;
            for (int i = 0; i < Count; i++) if (SubNets[i] == subNet) return true;
            return false;
        }

        /// <summary>
        /// Get the index of an IP sub-net
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>Index or <c>-1</c>, if not contained</returns>
        public int IndexOf(in IpSubNet subNet)
        {
            if (Count == 0) return -1;
            for (int i = 0; i < Count; i++) if (SubNets[i] == subNet) return i;
            return -1;
        }

        /// <summary>
        /// Determine if an IP address is included in any IP sub-net
        /// </summary>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <returns>If included in any IP sub-net</returns>
        public bool Includes(in IPAddress ip)
        {
            if (Count == 0) return false;
            for (int i = 0; i < Count; i++) if (SubNets[i] == ip) return true;
            return false;
        }

        /// <summary>
        /// Get the IP sub-net which includes an IP address
        /// </summary>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <returns>The including IP sub-net</returns>
        public IpSubNet? Including(in IPAddress ip)
        {
            if (Count == 0) return null;
            for (int i = 0; i < Count; i++) if (SubNets[i] == ip) return SubNets[i];
            return null;
        }

        /// <summary>
        /// Determine if an IP sub-net is intersecting with any IP sub-net
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>If included in any IP sub-net</returns>
        public bool IntersectsWith(in IpSubNet subNet)
        {
            if (Count == 0) return false;
            for (int i = 0; i < Count; i++) if ((subNet & SubNets[i]) != IpSubNet.ZeroV4) return true;
            return false;
        }

        /// <summary>
        /// Determine if the given IP sub-nets intersect any contained sub-nets
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <returns>If it intersects</returns>
        public bool IntersectsWith(in IpSubNets subNets)
        {
            if (Count == 0 || subNets.Count == 0) return false;
            for (int i = 0; i < Count; i++) if (subNets.IntersectsWith(SubNets[i])) return true;
            return false;
        }

        /// <summary>
        /// Get the intersecting IP sub-net
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>The intersecting IP sub-net</returns>
        public IpSubNet? GetIntersecting(in IpSubNet subNet)
        {
            if (Count == 0) return null;
            for (int i = 0; i < Count; i++) if ((subNet & SubNets[i]) != IpSubNet.ZeroV4) return SubNets[i];
            return null;
        }

        /// <summary>
        /// Get all intersecting IP sub-nets
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>The intersecting IP sub-nets</returns>
        public IEnumerable<IpSubNet> GetAllIntersecting(IpSubNet subNet)
        {
            if (Count == 0) yield break;
            for (int i = 0; i < Count; i++) if ((subNet & SubNets[i]) != IpSubNet.ZeroV4) yield return SubNets[i];
        }

        /// <summary>
        /// Determine if an IP sub-net is matching into any IP sub-net
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>If matching into any IP sub-net</returns>
        public bool Matches(in IpSubNet subNet)
        {
            if (Count == 0) return false;
            for (int i = 0; i < Count; i++) if ((subNet & SubNets[i]) == subNet) return true;
            return false;
        }

        /// <summary>
        /// Get the matching IP sub-net
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>The matching IP sub-net</returns>
        public IpSubNet? GetMatching(in IpSubNet subNet)
        {
            if (Count == 0) return null;
            for (int i = 0; i < Count; i++) if ((subNet & SubNets[i]) == subNet) return SubNets[i];
            return null;
        }

        /// <summary>
        /// Get all matching IP sub-nets
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>The matching IP sub-nets</returns>
        public IEnumerable<IpSubNet> GetAllMatching(IpSubNet subNet)
        {
            if (Count == 0) yield break;
            for (int i = 0; i < Count; i++) if ((subNet & SubNets[i]) == subNet) yield return SubNets[i];
        }

        /// <summary>
        /// Determine if an IP sub-net covers all contained sub-nets
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>If it covers</returns>
        public bool IsCoveredBy(in IpSubNet subNet)
        {
            if (Count == 0) return true;
            AddressFamily addressFamily = subNet.AddressFamily;
            if (AddressFamily != addressFamily) return false;
            for (int i = 0; i < Count; i++) if (SubNets[i].AddressFamily != addressFamily || !SubNets[i].IsWithin(subNet)) return false;
            return true;
        }

        /// <summary>
        /// Determine if an IP sub-net covers all contained sub-nets
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>If it covers</returns>
        public bool IsAnyCoveredBy(in IpSubNet subNet)
        {
            if (Count == 0) return false;
            AddressFamily addressFamily = subNet.AddressFamily;
            for (int i = 0; i < Count; i++) if (SubNets[i].AddressFamily == addressFamily && SubNets[i].IsWithin(subNet)) return true;
            return false;
        }

        /// <summary>
        /// Determine if any of the given IP sub-nets covers any of the contained sub-nets
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <returns>If any covers</returns>
        public bool IsAnyCoveredByAnyOf(in IpSubNets subNets)
        {
            if (Count == 0 || subNets.Count == 0) return false;
            for (int i = 0; i < Count; i++) if (subNets.Matches(SubNets[i])) return true;
            return false;
        }

        /// <summary>
        /// Determine if all contained IP sub-nets are covered by any of the given sub-nets
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <returns>If all are covered</returns>
        public bool AreAllCoveredByAnyOf(in IpSubNets subNets)
        {
            if (Count == 0 || subNets.Count == 0) return false;
            for (int i = 0; i < Count; i++) if (!subNets.Matches(SubNets[i])) return false;
            return true;
        }

        /// <summary>
        /// Get this as array
        /// </summary>
        /// <returns>IP sub-net array</returns>
        public IpSubNet[] ToArray() => (IpSubNet[])SubNets.Clone();

        /// <summary>
        /// Get an IP sub-net enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<IpSubNet> GetEnumerator() => ((IEnumerable<IpSubNet>)SubNets).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => SubNets.GetEnumerator();

        /// <summary>
        /// Cast as IP sub-net count
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        public static implicit operator int(in IpSubNets subNets) => subNets.Count;

        /// <summary>
        /// Cast as IP network kind
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        public static implicit operator IpNetworkKind(in IpSubNets subNets) => subNets.NetworkKind;

        /// <summary>
        /// Cast as IP address family
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        public static implicit operator AddressFamily(in IpSubNets subNets) => subNets.AddressFamily;

        /// <summary>
        /// Cast as combined IP sub-net
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        public static explicit operator IpSubNet(in IpSubNets subNets)
        {
            if (subNets.Count == 0) return IpSubNet.ZeroV4;
            if (subNets.AddressFamily == AddressFamily.Unspecified) throw new InvalidCastException("Can't mix IPv4 and IPv6 sub-nets");
            IpSubNet res = subNets.SubNets[0];
            for (int i = 1; i < subNets.Count; res = res.CombineWith(subNets.SubNets[i]), i++) ;
            return res;
        }

        /// <summary>
        /// Determine if an IP address is included in any IP sub-net
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <returns>If included in any IP sub-net</returns>
        public static bool operator ==(in IpSubNets subNets, in IPAddress ip) => subNets.Includes(ip);

        /// <summary>
        /// Determine if an IP address is not included in any IP sub-net
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <returns>If not included in any IP sub-net</returns>
        public static bool operator !=(in IpSubNets subNets, in IPAddress ip) => !subNets.Includes(ip);

        /// <summary>
        /// Determine if an IP address is included in any IP sub-net
        /// </summary>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <returns>If included in any IP sub-net</returns>
        public static bool operator ==(in IPAddress ip, in IpSubNets subNets) => subNets.Includes(ip);

        /// <summary>
        /// Determine if an IP address is not included in any IP sub-net
        /// </summary>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <returns>If not included in any IP sub-net</returns>
        public static bool operator !=(in IPAddress ip, in IpSubNets subNets) => !subNets.Includes(ip);

        /// <summary>
        /// Count is lower
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If lower</returns>
        public static bool operator <(in IpSubNets a, in IpSubNets b) => a.SubNets.Length < b.SubNets.Length;

        /// <summary>
        /// Count is greater
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If greater</returns>
        public static bool operator >(in IpSubNets a, in IpSubNets b) => a.SubNets.Length > b.SubNets.Length;

        /// <summary>
        /// Count is lower or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If lower or equal</returns>
        public static bool operator <=(in IpSubNets a, in IpSubNets b) => a.SubNets.Length <= b.SubNets.Length;

        /// <summary>
        /// Count is greater or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If greater or equal</returns>
        public static bool operator >=(in IpSubNets a, in IpSubNets b) => a.SubNets.Length >= b.SubNets.Length;

        /// <summary>
        /// Get the IP sub-net which includes an IP address
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <returns>The including IP sub-net or <see cref="IpSubNet.ZeroV4"/></returns>
        public static IpSubNet operator &(in IpSubNets subNets, in IPAddress ip) => subNets.Including(ip) ?? IpSubNet.ZeroV4;

        /// <summary>
        /// Get the IP sub-net which includes an IP address
        /// </summary>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <returns>The including IP sub-net or <see cref="IpSubNet.ZeroV4"/></returns>
        public static IpSubNet operator &(in IPAddress ip, in IpSubNets subNets) => subNets.Including(ip) ?? IpSubNet.ZeroV4;

        /// <summary>
        /// Get the matching or intersecting IP sub-net
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>The matching or intersecting IP sub-net, or <see cref="IpSubNet.ZeroV4"/>, if not intersecting at al</returns>
        public static IpSubNet operator &(in IpSubNets subNets, in IpSubNet subNet) => subNets.GetMatching(subNet) ?? subNets.GetIntersecting(subNet) ?? IpSubNet.ZeroV4;

        /// <summary>
        /// Get the matching or intersecting IP sub-net
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <returns>The matching or intersecting IP sub-net, or <see cref="IpSubNet.ZeroV4"/>, if not intersecting at al</returns>
        public static IpSubNet operator &(in IpSubNet subNet, in IpSubNets subNets) => subNets.GetMatching(subNet) ?? subNets.GetIntersecting(subNet) ?? IpSubNet.ZeroV4;

        /// <summary>
        /// Add an IP sub-net (if it doesn't exist!)
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>New <see cref="IpSubNets"/></returns>
        public static IpSubNets operator |(in IpSubNets subNets, in IpSubNet subNet)
        {
            if (subNets.Contains(subNet)) return subNets;
            IpSubNet[] newSubNets = new IpSubNet[subNets.Count + 1];
            Array.Copy(subNets.SubNets, 0, newSubNets, 0, subNets.Count);
            newSubNets[^1] = subNet;
            return new(newSubNets);
        }

        /// <summary>
        /// Add an IP sub-net (if it doesn't exist!)
        /// </summary>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <returns><see cref="IpSubNets"/></returns>
        public static IpSubNets operator |(in IpSubNet subNet, in IpSubNets subNets)
        {
            if (subNets.Contains(subNet)) return subNets;
            IpSubNet[] newSubNets = new IpSubNet[subNets.Count + 1];
            Array.Copy(subNets.SubNets, 0, newSubNets, 0, subNets.Count);
            newSubNets[^1] = subNet;
            return new(newSubNets);
        }

        /// <summary>
        /// Merge two IP sub-net lists
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="other"><see cref="IpSubNets"/></param>
        /// <returns><see cref="IpSubNets"/></returns>
        public static IpSubNets operator |(in IpSubNets subNets, in IpSubNets other)
        {
            if (other.Count == 0) return subNets;
            List<int> unknown = new();
            for (int i = 0; i < other.Count; i++)
                if (!subNets.Contains(other.SubNets[i]))
                    unknown.Add(i);
            if (unknown.Count == 0) return subNets;
            IpSubNet[] newSubNets = new IpSubNet[subNets.Count + unknown.Count];
            Array.Copy(subNets.SubNets, 0, newSubNets, 0, subNets.Count);
            for (int start = subNets.Count, i = start, len = start + unknown.Count; i < len; i++)
                newSubNets[i] = other[unknown[i - start]];
            return new(newSubNets);
        }

        /// <summary>
        /// Add an IP sub-net
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns>New <see cref="IpSubNets"/></returns>
        public static IpSubNets operator +(in IpSubNets subNets, in IpSubNet subNet)
        {
            IpSubNet[] newSubNets = new IpSubNet[subNets.Count + 1];
            Array.Copy(subNets.SubNets, 0, newSubNets, 0, subNets.Count);
            newSubNets[^1] = subNet;
            return new(newSubNets);
        }

        /// <summary>
        /// Add IP sub-nets
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="other"><see cref="IpSubNets"/></param>
        /// <returns><see cref="IpSubNets"/></returns>
        public static IpSubNets operator +(in IpSubNets subNets, in IpSubNets other)
        {
            if (other.SubNets.Length == 0) return subNets;
            IpSubNet[] newSubNets = new IpSubNet[subNets.Count + other.Count];
            Array.Copy(subNets.SubNets, 0, newSubNets, 0, subNets.Count);
            Array.Copy(other.SubNets, 0, newSubNets, subNets.Count, other.Count);
            return new(newSubNets);
        }

        /// <summary>
        /// Remove an IP sub-net
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="subNet"><see cref="IpSubNet"/></param>
        /// <returns><see cref="IpSubNets"/></returns>
        public static IpSubNets operator -(in IpSubNets subNets, in IpSubNet subNet)
        {
            if (!subNets.Contains(subNet)) return subNets;
            if (subNets.Count == 1) return new(Array.Empty<IpSubNet>());
            IpSubNet[] newSubNets = new IpSubNet[subNets.Count - 1];
            for (int i = 0, j = 0; i < subNets.Count; i++)
            {
                if (subNets.SubNets[i] == subNet) continue;
                newSubNets[j] = subNets.SubNets[i];
                j++;
            }
            return new(newSubNets);
        }

        /// <summary>
        /// Remove IP sub-nets
        /// </summary>
        /// <param name="subNets"><see cref="IpSubNets"/></param>
        /// <param name="other"><see cref="IpSubNets"/></param>
        /// <returns><see cref="IpSubNets"/></returns>
        public static IpSubNets operator -(in IpSubNets subNets, in IpSubNets other)
        {
            if (subNets == other) return new(Array.Empty<IpSubNet>());
            List<int> remove = new();
            for (int i = 0, len = subNets.Count; i < len; i++)
                if (other.Contains(subNets.SubNets[i]))
                    remove.Add(i);
            if (remove.Count == 0) return subNets;
            if (remove.Count == subNets.Count) return new(Array.Empty<IpSubNet>());
            IpSubNet[] newSubNets = new IpSubNet[subNets.Count - remove.Count];
            for (int i = 0, j = 0; i < subNets.Count; i++)
            {
                if (remove.Contains(i)) continue;
                newSubNets[j] = subNets.SubNets[i];
                j++;
            }
            return new(newSubNets);
        }
    }
}
