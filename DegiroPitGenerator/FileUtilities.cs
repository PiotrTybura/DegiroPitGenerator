using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DegiroPitGenerator
{
    internal static class FileUtilities
    {
        internal static string SaveToJsonFile(object jsonName, object obj)
        {
            var json = JsonConvert.SerializeObject(obj, new StringEnumConverter());
            var fileName = $"{jsonName}_{DateTime.Now.ToString("yyyy_dd_M_HH_mm_ss")}.json";
            File.WriteAllText(fileName, json);
            return fileName;
        }

        internal static void StartProcess(string fileName)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(fileName)
            {
                UseShellExecute = true
            };
            p.Start();
        }

    }
}
