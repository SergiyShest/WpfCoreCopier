using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;

public class Watcher
{

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public void Run(string path)
    {
       

        // Create a new FileSystemWatcher and set its properties.
        using (FileSystemWatcher watcher = new FileSystemWatcher())
        {
            watcher.Path = path;

            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            // Only watch text files.
            watcher.Filter = "*.txt";

            // Add event handlers.
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
          //  Debug.WriteLine("Press 'q' to quit the sample.");
        //    while (Debug.Read() != 'q') ;
        }
    }

    // Define the event handlers.
    private static void OnChanged(object source, FileSystemEventArgs e) =>
        // Specify what is done when a file is changed, created, or deleted.
        Debug.WriteLine($"File: {e.FullPath} {e.ChangeType}");

    private static void OnRenamed(object source, RenamedEventArgs e) =>
        // Specify what is done when a file is renamed.
        Debug.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
}