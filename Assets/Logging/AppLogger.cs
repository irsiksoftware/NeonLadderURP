//using Serilog;
using System;
using UnityEngine;

public interface ILogger
{
}   
public static class AppLogger
{
    public static ILogger Logger { get; private set; }

    static AppLogger()
    {
        // Configure Serilog
        //Logger = new LoggerConfiguration()
        //    .MinimumLevel.Debug() // Set the minimum log level
        //    .WriteTo.File(path: "Logs/log-.txt",
        //                  rollingInterval: RollingInterval.Hour, // Log file will rotate every hour
        //                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        //                  flushToDiskInterval: TimeSpan.FromMinutes(1)) // Adjusted for more frequent flushing to disk
        //    .CreateLogger();

        //// Example of how to enrich logs with properties (optional)
        //Logger = Logger.ForContext("GameVersion", Application.version);
    }

    public static void Initialize()
    {
        // Method to explicitly initialize the logger from the game's entry point
    }
}
