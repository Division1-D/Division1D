using Mirror;
using UnityEngine;

/// <summary>
/// Client Bootstrap
/// </summary>
public sealed class ClientBootstrap : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    [Header("Server Configuration")]
    [SerializeField] private ServerConfig serverConfig;

    private void Start()
    {
#if UNITY_SERVER
        // 서버 빌드에서는 클라 부트스트랩 자체가 의미 없으니 즉시 종료
        return;
#else
        if (networkManager == null)
            networkManager = FindFirstObjectByType<NetworkManager>();

        // ServerConfig에서 서버 주소 로드
        if (serverConfig != null)
        {
            networkManager.networkAddress = serverConfig.serverAddress;
            Debug.Log($"[ClientBootstrap] Connecting to server: {serverConfig.serverAddress}");
        }
        else
        {
            Debug.LogError("[ClientBootstrap] ServerConfig is null! Please assign it in the Inspector.");
            return;
        }

        networkManager.StartClient(); // 접속 시작
#endif
    }
}