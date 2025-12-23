using System;
using System.Linq;
using kcp2k;
using Mirror;
using UnityEngine;

namespace Division.Server.Core
{
    public class CustomNetworkManager : NetworkManager
    {
        #if UNITY_SERVER
        private static string Now()
        {
            return DateTime.UtcNow.ToString("O");
        }

        private void LogServer(string msg, NetworkConnectionToClient conn = null)
        {
            var connInfo = conn == null
                ? ""
                : $" | connId={conn.connectionId} addr={conn.address} ready={conn.isReady} auth={conn.isAuthenticated}"; // [web:952]

            Debug.Log($"[{Now()}] [SERVER] {msg}{connInfo}");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            ushort port = 0;
            var transportName = Transport.active != null ? Transport.active.GetType().Name : "null";

            if (Transport.active is KcpTransport kcp)
                port = kcp.Port;

            var args = string.Join(" ", Environment.GetCommandLineArgs().Select(a => a.Contains(' ') ? $"\"{a}\"" : a));
            LogServer($"OnStartServer | transport={transportName} port={port} maxConn={maxConnections} | args={args}");
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            LogServer($"OnServerSceneChanged | scene={sceneName}");
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            LogServer("OnServerConnect", conn);
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            LogServer("OnServerReady", conn);
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            LogServer("OnServerAddPlayer (before base)", conn);

            base.OnServerAddPlayer(conn);

            var netId = conn.identity != null ? conn.identity.netId : 0;
            LogServer($"OnServerAddPlayer (after base) | playerNetId={netId} numPlayers={numPlayers}", conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            LogServer("OnServerDisconnect", conn);
            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer()
        {
            LogServer("OnStopServer");
            base.OnStopServer();
        }
        #endif
    }
}