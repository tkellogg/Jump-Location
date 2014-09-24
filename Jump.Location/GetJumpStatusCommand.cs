﻿using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Jump.Location
{
    using System;
    using System.Configuration;
    using System.IO;

    [Cmdlet(VerbsCommon.Get, "JumpStatus", DefaultParameterSetName = "Query")]
    public class GetJumpStatusCommand : PSCmdlet
    {
        private static readonly CommandController Controller = CommandController.DefaultInstance;

        [Parameter(ParameterSetName = "Query", ValueFromRemainingArguments = true)]
        public string[] Query { get; set; }

        [Parameter(ParameterSetName = "Query", HelpMessage = "Only retrieve the first record matching the query. Same as `getj`")]
        public SwitchParameter First { get; set; }

        [Parameter(ParameterSetName = "Save", HelpMessage = "Saves any pending changes from setting weights on records explicitly.")]
        public SwitchParameter Save { get; set; }

        [Parameter(ParameterSetName = "Query", HelpMessage = "Includes all results, even if the weight is negative.")]
        public SwitchParameter All { get; set; }

        [Parameter(ParameterSetName = "Cleanup", HelpMessage = "Remove obsolete(not existing on the file system) records from DB.")]
        public SwitchParameter Cleanup { get; set; }

        [Parameter(ParameterSetName = "Scan", HelpMessage = "Scan and discover new directories from known directories.")]
        public SwitchParameter Scan { get; set; }

        protected override void ProcessRecord()
        {
            
            if (Cleanup.IsPresent)
            {
                DoCleanup();
                return;    
            }

            if (Scan.IsPresent)
            {
                DoScan();
                return;
            }

            if (All || Query == null || Query.Length == 0) 
                ProcessSearch(Controller.GetOrderedRecords(All));
            else 
                ProcessSearch(Controller.GetMatchesForSearchTerm(Query));
        }

        private const string FileSystemProvider = @"Microsoft.PowerShell.Core\FileSystem";
        private void DoCleanup()
        {
            int recordsRemoved = 0;
            foreach (IRecord record in Controller.GetOrderedRecords(true))
            {
                if (record.Provider == FileSystemProvider && !Directory.Exists(record.Path))
                {
                    Controller.RemoveRecord(record);
                    recordsRemoved++;
                }
            }
            if (recordsRemoved > 0)
            {
                Controller.Save();
                Console.WriteLine("Number of records cleaned: " + recordsRemoved + ".");
            }
        }

        private void DoScan()
        {
            var home = Environment.GetEnvironmentVariable("USERPROFILE");
            home = home ?? Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE"), Environment.GetEnvironmentVariable("HOMEPATH"));
            var dirs = new Dictionary<string, int>();
            
            Console.WriteLine("Discovering new folders.");
            var currentDirs = new Dictionary<string, int>();
            foreach (IRecord record in Controller.GetOrderedRecords(true))
            {
                if (record.Provider == FileSystemProvider && Directory.Exists(record.Path))
                {
                    if (record.Path == home) continue; // Skip home folder - it's going to be in the list by default, and subfolders such as photos, music, etc. are probably not useful.
                    currentDirs.Add(record.Path, 1);
                    GetChildFolders(record.Path, dirs);
                }
            }

            int numDirsAdded = 0;
            Console.WriteLine("Adding directories:");
            foreach (string dir in dirs.Keys)
            {
                if (!currentDirs.ContainsKey(dir))
                {
                    Console.WriteLine(dir);
                    IRecord record = new Record(FileSystemProvider + "::" + dir, 0);
                    Controller.AddRecord(record);
                    numDirsAdded++;
                }
            }

            Controller.Save();

            Console.WriteLine(string.Format("Number of directories added: {0}.", numDirsAdded));
        }

        private void GetChildFolders(string dir, Dictionary<string, int> folders)
        {
            try
            {
                string[] subDirs = Directory.GetDirectories(dir);

                int maxSubFolders = int.Parse(ConfigurationManager.AppSettings["Jump.Location.Scan.MaxSubFolders"] ?? "50");
                // Skip scanning directories with more than maxSubFolders... probably not useful and also very large folders can slow scanning/queries down considerably.
                if (subDirs.Length <= maxSubFolders)
                {
                    foreach (string subDir in subDirs)
                    {
                        Console.WriteLine(subDir);
                        if (!folders.ContainsKey(subDir))
                        {
                            folders.Add(subDir, 1);
                            GetChildFolders(subDir, folders);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(string.Format("Skipped folder {0}. More than {1} subdirs.", dir, maxSubFolders));
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private void ProcessSearch(IEnumerable<IRecord> records)
        {
            if (Save)
            {
                Controller.Save();
                return;
            }

            if (First)
            {
                var record = records.FirstOrDefault();
                if (record != null) 
                    WriteObject(record);
                return;
            }

            foreach (var record in records)
                WriteObject(record);

            Console.WriteLine("Number of records: " + records.Count() + ".");
        }
    }
}
