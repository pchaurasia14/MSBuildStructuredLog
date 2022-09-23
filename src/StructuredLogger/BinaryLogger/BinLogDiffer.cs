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

        public static MyersDiff<string> stringDiff = null;

        public static IEnumerable<(ResultType ResultType, string AItem, string BItem)> GetDiffResult(string logFile1, string logFile2)
        {
            string[] lines1 = File.ReadAllLines(logFile1);
            string[] lines2 = File.ReadAllLines(logFile2);
            return GetDiffResult(lines1, lines2);
        }

        public static IEnumerable<(ResultType ResultType, string AItem, string BItem)> GetDiffResult(Stream stream1, Stream stream2)
        {
            List<string> lines1 = new List<string>();
            List<string> lines2 = new List<string>();

            using (StreamReader sr = new StreamReader(stream1))
            {
                while(!sr.EndOfStream)
                {
                    lines1.Add(sr.ReadLine());
                }
            }

            using (StreamReader sr = new StreamReader(stream2))
            {
                while (!sr.EndOfStream)
                {
                    lines2.Add(sr.ReadLine());
                }
            }
            return GetDiffResult(lines1.ToArray(), lines2.ToArray());
        }

        public static IEnumerable<(ResultType ResultType, string AItem, string BItem)> GetDiffResult(string[] lines1, string[] lines2)
        {
            stringDiff = new MyersDiff<string>(lines1, lines2);
            return stringDiff.GetResult();
        }

        public static IEnumerable<(int LineA, int LineB, int CountA, int CountB)> GetEditScript(string logFile1, string logFile2)
        {
            string[] lines1 = File.ReadAllLines(logFile1);
            string[] lines2 = File.ReadAllLines(logFile2);
            return GetEditScript(lines1, lines2);
        }

        public static IEnumerable<(int LineA, int LineB, int CountA, int CountB)> GetEditScript(Stream stream1, Stream stream2)
        {
            List<string> lines1 = new List<string>();
            List<string> lines2 = new List<string>();

            using (StreamReader sr = new StreamReader(stream1))
            {
                while (!sr.EndOfStream)
                {
                    lines1.Add(sr.ReadLine());
                }
            }

            using (StreamReader sr = new StreamReader(stream2))
            {
                while (!sr.EndOfStream)
                {
                    lines2.Add(sr.ReadLine());
                }
            }
            return GetEditScript(lines1.ToArray(), lines2.ToArray());
        }

        public static IEnumerable<(int LineA, int LineB, int CountA, int CountB)> GetEditScript(string[] lines1, string[] lines2)
        {
            stringDiff = new MyersDiff<string>(lines1, lines2);
            return stringDiff.GetEditScript();
        }
    }
}
