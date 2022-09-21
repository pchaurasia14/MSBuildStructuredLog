using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Build.Framework;
using System.Xml;

namespace Microsoft.Build.Logging.StructuredLogger
{
    /// <summary>
    /// Provides a method to get the difference of two binary log files (*.binlog).
    /// </summary>
    public sealed class BinLogDiffer
    {
        public static IEnumerable<(ResultType ResultType, string AItem, string BItem)> ComputeDiff(string logFile1, string logFile2)
        {
            string[] file1 = File.ReadAllLines(logFile1);
            string[] file2 = File.ReadAllLines(logFile2);

            MyersDiff<string> diff = new MyersDiff<string>(file1, file2);
            return diff.GetResult();
        }   
    }
}
