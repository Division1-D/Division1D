using System;
using kcp2k;
using Mirror;
using Test;
using UnityEngine;

/// <summary>
/// Server Bootstrap
/// </summary>
public sealed class DedicatedServerBootstrap : MonoBehaviour
{
    [SerializeField] private CustomNetworkManager networkManager;
    [SerializeField] private KcpTransport kcpTransport;
    [SerializeField] private int port = 7777;

    private void Start()
    {
        if (networkManager == null) networkManager = FindFirstObjectByType<CustomNetworkManager>();
        if (kcpTransport == null) kcpTransport = FindFirstObjectByType<KcpTransport>();

        if (kcpTransport != null)
            kcpTransport.Port = (ushort)port;

#if UNITY_SERVER
        Debug.Log($"[DedicatedServerBootstrap] Starting Mirror server on UDP {port}...");
        networkManager.StartServer();
#endif
    }
}