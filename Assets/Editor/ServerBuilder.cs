using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Division.Editor
{
    /// <summary>
    /// GitHub Actions에서 사용할 서버 빌드 스크립트
    /// </summary>
    public static class ServerBuilder
    {
        // 빌드 출력 경로
        private static readonly string BuildPath = "build/StandaloneLinux64";
        private static readonly string BuildName = "Division1D";

        // 서버 빌드에 포함할 씬 목록
        private static readonly string[] ServerScenes = new[]
        {
            // "Assets/01. Scenes/Lobby.unity",
            // "Assets/01. Scenes/WaitingRoom.unity",
            // "Assets/01. Scenes/TempGame.unity"
            "Assets/01. Scenes/Test/TestNet_Server.unity"
        };

        /// <summary>
        /// GitHub Actions에서 호출할 메인 빌드 메서드
        /// </summary>
        [MenuItem("Build/Build Linux ARM64 Server")]
        public static void BuildLinuxARM64Server()
        {
            Debug.Log("════════════════════════════════════════");
            Debug.Log("Starting Linux ARM64 Dedicated Server Build");
            Debug.Log("════════════════════════════════════════");

            try
            {
                // 빌드 옵션 설정
                BuildPlayerOptions buildOptions = new BuildPlayerOptions
                {
                    // 빌드할 씬들 (서버 전용 씬 목록)
                    scenes = ServerScenes,

                    // 출력 경로
                    locationPathName = Path.Combine(BuildPath, BuildName),

                    // 타겟 플랫폼: Linux Dedicated Server
                    target = BuildTarget.StandaloneLinux64,

                    // Dedicated Server 전용 설정
                    subtarget = (int)StandaloneBuildSubtarget.Server,

                    // 빌드 옵션
                    options = BuildOptions.None
                };

                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Server, ScriptingImplementation.IL2CPP);
                
                PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone, 1);
                PlayerSettings.SetArchitecture(NamedBuildTarget.Server, 1);
                
                Debug.Log($"Arch Standalone = {PlayerSettings.GetArchitecture(NamedBuildTarget.Standalone)}");
                Debug.Log($"Arch Server     = {PlayerSettings.GetArchitecture(NamedBuildTarget.Server)}");

                Debug.Log($"Build Target: {buildOptions.target}");
                Debug.Log($"Build Subtarget: Server (ARM64)");
                Debug.Log($"Build Path: {buildOptions.locationPathName}");
                Debug.Log($"Server Scenes ({buildOptions.scenes.Length}):");
                foreach (var scene in buildOptions.scenes)
                {
                    Debug.Log($"  - {scene}");
                }

                // 빌드 실행
                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                BuildSummary summary = report.summary;

                // 빌드 결과 출력
                Debug.Log("════════════════════════════════════════");
                Debug.Log($"Build Result: {summary.result}");
                Debug.Log($"Total Time: {summary.totalTime.TotalSeconds:F2} seconds");
                Debug.Log($"Total Size: {FormatBytes((long)summary.totalSize)}");
                Debug.Log($"Output Path: {summary.outputPath}");
                Debug.Log("════════════════════════════════════════");

                // 빌드 성공/실패 확인
                if (summary.result == BuildResult.Succeeded)
                {
                    Debug.Log("✅ Build SUCCEEDED!");

                    // 빌드 파일 검증
                    ValidateBuildOutput(summary.outputPath);

                    // CI 환경에서는 정상 종료
                    EditorApplication.Exit(0);
                }
                else
                {
                    Debug.LogError("❌ Build FAILED!");
                    Debug.LogError($"Total Errors: {summary.totalErrors}");
                    Debug.LogError($"Total Warnings: {summary.totalWarnings}");

                    // 에러 메시지 출력
                    foreach (var step in report.steps)
                    {
                        foreach (var message in step.messages)
                        {
                            if (message.type == LogType.Error || message.type == LogType.Exception)
                            {
                                Debug.LogError($"[{message.type}] {message.content}");
                            }
                        }
                    }

                    // CI 환경에서는 에러 코드와 함께 종료
                    EditorApplication.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("════════════════════════════════════════");
                Debug.LogError("❌ BUILD EXCEPTION");
                Debug.LogError(ex.ToString());
                Debug.LogError("════════════════════════════════════════");

                // CI 환경에서는 에러 코드와 함께 종료
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Build Settings에 포함된 활성화된 씬들을 가져옴
        /// </summary>
        private static string[] GetEnabledScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }

        /// <summary>
        /// 빌드 결과물 검증
        /// </summary>
        private static void ValidateBuildOutput(string outputPath)
        {
            Debug.Log("🔍 Validating build output...");

            if (!File.Exists(outputPath))
            {
                Debug.LogError($"❌ Build executable not found: {outputPath}");
                EditorApplication.Exit(1);
                return;
            }

            FileInfo fileInfo = new FileInfo(outputPath);
            Debug.Log($"✅ Build executable found: {outputPath}");
            Debug.Log($"   Size: {FormatBytes(fileInfo.Length)}");

            // Data 폴더 확인
            string dataFolder = outputPath.Replace(BuildName, $"{BuildName}_Data");
            if (Directory.Exists(dataFolder))
            {
                Debug.Log($"✅ Data folder found: {dataFolder}");
            }
            else
            {
                Debug.LogWarning($"⚠️  Data folder not found: {dataFolder}");
            }
        }

        /// <summary>
        /// 바이트를 읽기 쉬운 형식으로 변환
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
