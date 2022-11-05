using System.Linq;
using System.Net.NetworkInformation;

namespace Scorpian.Helper;

public static class TcpStatus
{
    public static ConnectionStatus GetState(int localPort, int remotePort)
    {
        var ipGlobProp = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnInfos = ipGlobProp.GetActiveTcpConnections();
        var tcpConnInfo = tcpConnInfos.FirstOrDefault(t =>
            t.LocalEndPoint.Port == localPort && t.RemoteEndPoint.Port == remotePort);

        if (tcpConnInfo == null)
            return ConnectionStatus.Idle;

        var tcpState = tcpConnInfo.State;
        switch (tcpState)
        {
            case TcpState.Listen:
            case TcpState.SynSent:
            case TcpState.SynReceived:
                return ConnectionStatus.Connecting;

            case TcpState.Established:
                return ConnectionStatus.Connected;

            case TcpState.FinWait1:
            case TcpState.FinWait2:
            case TcpState.CloseWait:
            case TcpState.Closing:
            case TcpState.LastAck:
                return ConnectionStatus.Disconnecting;

            default:
                return ConnectionStatus.NotReady;
        }
    }
}

public enum ConnectionStatus
{
    NotInitialized,
    NotReady,
    Idle,
    Connecting,
    Connected,
    Disconnecting
}