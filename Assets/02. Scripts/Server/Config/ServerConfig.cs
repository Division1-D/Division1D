using UnityEngine;

namespace Division.Server.Config
{
    /// <summary>
    /// 서버 연결 설정
    /// </summary>
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "Game/Server Config", order = 0)]
    public class ServerConfig : ScriptableObject
    {
        [Header("Server Connection")]
        [Tooltip("접속할 서버의 IP 주소 또는 도메인")]
        public string serverAddress = "127.0.0.1";

        [Tooltip("서버 포트")]
        public ushort serverPort = 7777;

        [Header("Development")]
        [Tooltip("개발용 설정 여부")]
        public bool isDevelopment = true;
    }
}
