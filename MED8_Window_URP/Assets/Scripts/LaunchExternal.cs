using UnityEngine;
using System.Diagnostics;
using System.IO;

public class LaunchExternal : MonoBehaviour
{
    public static LaunchExternal Instance { get; private set; }

    private Process pythonProcess;

    void Awake()
    {
        // Singleton check
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        string exePath = Path.Combine(Application.streamingAssetsPath, "LaunchScript.exe");

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true,
            CreateNoWindow = false
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