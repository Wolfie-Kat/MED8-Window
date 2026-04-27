using UnityEngine;
using System.Diagnostics;
using System.IO;

public class LaunchExternal : MonoBehaviour
{
    private Process pythonProcess;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        string exePath = Path.Combine(Application.streamingAssetsPath, "face_gesture_server.exe");

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true,     // IMPORTANT
            CreateNoWindow = false       // keep CMD visible
        };

        pythonProcess = Process.Start(startInfo);
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

    void OnDestroy()
    {
        Cleanup();
    }

    void Cleanup()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            try
            {
                // Best case: kill full process tree (Unity 2020+)
                // pythonProcess.Kill();
                KillProcessTree(pythonProcess.Id);
            }
            catch
            {
                // Fallback if Kill(true) fails
                KillProcessTree(pythonProcess.Id);
            }

            pythonProcess.Dispose();
        }
    }

    // Fallback method (Windows only)
    void KillProcessTree(int pid)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "taskkill",
            Arguments = $"/PID {pid} /T /F",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        Process.Start(psi);
    }
}