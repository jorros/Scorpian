using System;
using System.Collections.Generic;
using Scorpian.Network;
using Scorpian.Network.Packets;

namespace Scorpian;

public record EngineSettings
{
    public bool NetworkEnabled { get; set; }
    
    public NetworkMode NetworkMode { get; set; }
    
    public Func<string, IServiceProvider, LoginResponsePacket> Authentication { get; set; }

    public int Port { get; set; } = 1992;
    
    public bool Headless { get; set; }
    
    public Action HeadlessLoopAction { get; set; }
    
    public string Name { get; set; }
    
    public string DisplayName { get; set; }

    public int TicksPerSecond { get; set; } = 1;

    public List<Type> NetworkedNodes { get; } = new();

    internal Dictionary<ushort, Type> NetworkPackets { get; } = new();
};