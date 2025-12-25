using UnityEngine;

namespace Division.SpreadSheets
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Networking;

    public class SheetLoader : MonoBehaviour
    {
        private string sheetName = "Job"; // 시트 탭 이름 (여러 개이므로 나중에 바꿈)
        private string[] sheetNames = { "기본", "역할군", "강화 능력", "스토리" };
        
        void Start()
        {
            StartCoroutine(DownloadSheet());
        }

        IEnumerator DownloadSheet()
        {
            string cleanSheetName = sheetName.Trim();

            string escapedSheetName = UnityWebRequest.EscapeURL(cleanSheetName);
            string url = $"https://docs.google.com/spreadsheets/d/e/2PACX-1vSsJOpAJOlwfE0Kj2mUJoUCw0Vz3KB24s_fNJ8AaC3_eU_i5_-ng1upNu-MevjuIZb_J9ekCxCULiw5/pub?output=csv&sheet={escapedSheetName}";
            Debug.Log("최종 요청 URL: " + url);
//https://docs.google.com/spreadsheets/d/e/2PACX-1vSsJOpAJOlwfE0Kj2mUJoUCw0Vz3KB24s_fNJ8AaC3_eU_i5_-ng1upNu-MevjuIZb_J9ekCxCULiw5/pub?output=csv
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + www.error);
                }
                else
                {
                    // UTF-8
                    string csvData = www.downloadHandler.text;
                    Debug.Log("데이터 로드 완료:");
                    Debug.Log(csvData);

                    ProcessData(csvData);
                }
            }
        }

        void ProcessData(string data)
        {
            // 간단한 CSV 파싱 (한 줄씩 읽기)
            string[] lines = data.Split('\n');
        
            foreach (string line in lines)
            {
                // 쉼표(,)로 데이터 구분
                string[] row = line.Split(',');

                try
                {
                    if (row.Length > 0)
                    {
                        // 한글 데이터가 포함된 로그 출력
                        Debug.Log($"첫 번째 열: {row[0]}, 두 번째 열: {row[1]}");
                    }

                }
                catch
                {
                }
            }
        }
    }
}
