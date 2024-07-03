using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if UNITY_STANDALONE_WIN
using System.Management;
using System.Diagnostics.PerformanceCounter;
#elif UNITY_STANDALONE_OSX
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
#elif UNITY_STANDALONE_LINUX
using System.Text.RegularExpressions;
#endif
using UnityEngine.Profiling;

public class PerformanceProfiler : MonoBehaviour
{
    private int frameCount = 0;
    private float totalTime = 0f;
    private float averageFrameRate = 0f;
    private List<string> logData = new List<string>();
    private string filePath;
    private string sessionID;
#if UNITY_STANDALONE_WIN
    private PerformanceCounter cpuCounter;
#endif

    void Start()
    {
        try
        {
            InitializeLogging();
            LogHardwareInfo();
            StartCoroutine(LogPerformanceData());
        }
        catch (System.Exception e)
        {
            //UnityEngine.Debug.LogError("Error initializing PerformanceProfiler: " + e.Message);
        }
    }

    void Update()
    {
        frameCount++;
        totalTime += Time.deltaTime;
    }

    private void InitializeLogging()
    {
        filePath = Path.Combine(Application.persistentDataPath, "PerformanceData.txt");
        //UnityEngine.Debug.Log("Logging performance data to: " + filePath);

        sessionID = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        logData.Add($"Session: {sessionID}");
        logData.Add("Platform,Time,FrameCount,AverageFrameRate,TotalAllocatedMemory(MB),TotalReservedMemory(MB),TotalUnusedReservedMemory(MB),CPUUsage(%),GPUUsage(%),FrameTime(ms),GCAlloc(MB),DrawCalls,VerticesCount,PhysicsTime(ms),RenderTime(ms),Below30FPS,CPU,GPU,RAM");
#if UNITY_STANDALONE_WIN
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
#endif
    }

    private IEnumerator LogPerformanceData()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Log data every 1 second

            if (totalTime > 0)
            {
                averageFrameRate = frameCount / totalTime;
            }

            float totalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
            float totalReservedMemory = Profiler.GetTotalReservedMemoryLong() / (1024 * 1024);
            float totalUnusedReservedMemory = Profiler.GetTotalUnusedReservedMemoryLong() / (1024 * 1024);
            float cpuUsage = GetCPUUsage();
            float gpuUsage = GetGPUUsage();
            float frameTime = 1000.0f / averageFrameRate;
            float gcAlloc = Profiler.GetMonoUsedSizeLong() / (1024 * 1024);
            int drawCalls = UnityEngine.Rendering.OnDemandRendering.renderFrameInterval; // Adjust accordingly
            int verticesCount = 0; // Implement vertices count calculation
            float physicsTime = 0; // Implement physics calculation time
            float renderTime = 0; // Implement render time calculation
            bool below30FPS = averageFrameRate < 30.0f;

            string logEntry = $"{Application.platform},{Time.time},{frameCount},{averageFrameRate},{totalAllocatedMemory},{totalReservedMemory},{totalUnusedReservedMemory},{cpuUsage},{gpuUsage},{frameTime},{gcAlloc},{drawCalls},{verticesCount},{physicsTime},{renderTime},{below30FPS},{SystemInfo.processorType},{SystemInfo.graphicsDeviceName},{SystemInfo.systemMemorySize}";
            logData.Add(logEntry);
        }
    }

    void OnDisable()
    {
        //UnityEngine.Debug.Log("Game is stopping, writing data to file.");
        WriteDataToFile();
    }

    private void WriteDataToFile()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                foreach (string logEntry in logData)
                {
                    writer.WriteLine(logEntry);
                }
            }
            ////UnityEngine.Debug.Log("Performance data saved to: " + filePath);
        }
        catch (System.Exception e)
        {
            ////UnityEngine.Debug.LogError("Error writing performance data to file: " + e.Message);
        }
    }

    private void LogHardwareInfo()
    {
        logData.Add("Hardware Info:");
        logData.Add($"OS: {SystemInfo.operatingSystem}");
        logData.Add($"CPU: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)");
        logData.Add($"GPU: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB)");
        logData.Add($"RAM: {SystemInfo.systemMemorySize} MB");
        logData.Add($"Screen Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
        logData.Add($"DirectX Version: {SystemInfo.graphicsDeviceVersion}");
        logData.Add($"System Disk Space: {GetDiskSpace()} GB");
    }

    private float GetCPUUsage()
    {
        try
        {
#if UNITY_STANDALONE_WIN
            return cpuCounter.NextValue();
#elif UNITY_STANDALONE_OSX
            return GetCPUUsageMacOS();
#elif UNITY_STANDALONE_LINUX
            return GetCPUUsageLinux();
#else
            return 0;
#endif
        }
        catch (System.Exception e)
        {
            //UnityEngine.Debug.LogError("Error getting CPU usage: " + e.Message);
            return 0;
        }
    }

    private float GetGPUUsage()
    {
        try
        {
#if UNITY_STANDALONE_WIN
            return GetGPUUsageWindows();
#elif UNITY_STANDALONE_OSX
            return GetGPUUsageMacOS();
#elif UNITY_STANDALONE_LINUX
            return GetGPUUsageLinux();
#else
            return 0;
#endif
        }
        catch (System.Exception e)
        {
            //UnityEngine.Debug.LogError("Error getting GPU usage: " + e.Message);
            return 0;
        }
    }

#if UNITY_STANDALONE_WIN
    private float GetGPUUsageWindows()
    {
        float gpuUsage = 0.0f;
        try
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine");
            foreach (ManagementObject obj in searcher.Get())
            {
                gpuUsage += float.Parse(obj["UtilizationPercentage"].ToString());
            }
        }
        catch (System.Exception e)
        {
           ////UnityEngine.Debug.LogError("Error querying GPU usage on Windows: " + e.Message);
        }
        return gpuUsage;
    }
#endif

#if UNITY_STANDALONE_OSX
    [DllImport("libc")]
    static extern int sysctlbyname(string name, IntPtr oldp, ref ulong oldlenp, IntPtr newp, uint newlen);

    [DllImport("libc")]
    static extern int host_processor_info(IntPtr host, int flavor, out int processor_count, out IntPtr processor_info, out int processor_info_count);

    private float GetCPUUsageMacOS()
    {
        IntPtr host = IntPtr.Zero;
        int processorCount = 0;
        IntPtr processorInfoPtr = IntPtr.Zero;
        int processorInfoCount = 0;
        
        const int PROCESSOR_CPU_LOAD_INFO = 2;

        try
        {
            host = mach_host_self();
            int result = host_processor_info(host, PROCESSOR_CPU_LOAD_INFO, out processorCount, out processorInfoPtr, out processorInfoCount);

            if (result != 0)
            {
                ////UnityEngine.Debug.LogError("Error getting processor info: " + result);
                return 0;
            }

            int[] cpuInfo = new int[processorInfoCount];
            Marshal.Copy(processorInfoPtr, cpuInfo, 0, processorInfoCount);

            int userTime = 0, systemTime = 0, idleTime = 0, niceTime = 0;
            for (int i = 0; i < processorCount; i++)
            {
                userTime += cpuInfo[i * 4 + 0];
                systemTime += cpuInfo[i * 4 + 1];
                idleTime += cpuInfo[i * 4 + 2];
                niceTime += cpuInfo[i * 4 + 3];
            }

            int totalTime = userTime + systemTime + idleTime + niceTime;
            int usedTime = totalTime - idleTime;

            return (float)usedTime / totalTime * 100;
        }
        catch (System.Exception e)
        {
            ////UnityEngine.Debug.LogError("Error getting CPU usage on macOS: " + e.Message);
            return 0;
        }
        finally
        {
            if (processorInfoPtr != IntPtr.Zero)
            {
                vm_deallocate(host, processorInfoPtr, (ulong)(processorInfoCount * sizeof(int)));
            }
        }
    }

    private float GetGPUUsageMacOS()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "sh",
                Arguments = "-c \"ioreg -l | grep 'IntelAccelerator' -A10 | grep -i 'GpuBusy' | awk '{print $NF}'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    using (var reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        if (float.TryParse(result, out float gpuUsage))
                        {
                            return gpuUsage;
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            ////UnityEngine.Debug.LogError("Error getting GPU usage on macOS: " + e.Message);
        }
        return 0;
    }

    [DllImport("libc")]
    private static extern IntPtr mach_host_self();

    [DllImport("libc")]
    private static extern int vm_deallocate(IntPtr task, IntPtr address, ulong size);
#endif

#if UNITY_STANDALONE_LINUX
    private float GetCPUUsageLinux()
    {
        var cpuUsage = File.ReadLines("/proc/stat").First().Split(' ').Skip(2).Select(float.Parse).ToArray();
        var idleTime = cpuUsage[3];
        var totalTime = cpuUsage.Sum();
        var usedTime = totalTime - idleTime;

        return (usedTime / totalTime) * 100;
    }

    private float GetGPUUsageLinux()
    {
        // Try NVIDIA first
        string gpuUsageStr = "";
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"nvidia-smi --query-gpu=utilization.gpu --format=csv,noheader,nounits\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = Process.Start(psi))
            {
                gpuUsageStr = process.StandardOutput.ReadLine();
                process.WaitForExit();
            }
        }
        catch (System.Exception e)
        {
            //UnityEngine.Debug.LogError("Error querying NVIDIA GPU usage on Linux: " + e.Message);
        }
        if (float.TryParse(gpuUsageStr, out float gpuUsage))
        {
            return gpuUsage;
        }

        // Try AMD if NVIDIA failed
        gpuUsageStr = "";
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"rocm-smi --showgpuuse --csv | tail -n 1 | cut -d ',' -f 2\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = Process.Start(psi))
            {
                gpuUsageStr = process.StandardOutput.ReadLine();
                process.WaitForExit();
            }
        }
        catch (System.Exception e)
        {
            //UnityEngine.Debug.LogError("Error querying AMD GPU usage on Linux: " + e.Message);
        }
        if (float.TryParse(gpuUsageStr, out gpuUsage))
        {
            return gpuUsage;
        }

        return 0;
    }
#endif

    private float GetDiskSpace()
    {
#if UNITY_STANDALONE_WIN
        DriveInfo drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
        return (float)drive.TotalFreeSpace / (1024 * 1024 * 1024);
#elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        var startInfo = new ProcessStartInfo
        {
            FileName = "sh",
            Arguments = "-c \"df -h / | grep / | awk '{print $4}'\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(startInfo))
        {
            if (process != null)
            {
                using (var reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd().Trim();
                    if (float.TryParse(Regex.Replace(result, "[^0-9.]", ""), out float freeSpace))
                    {
                        return freeSpace;
                    }
                }
            }
        }
        return 0;
#else
        return 0;
#endif
    }
}
