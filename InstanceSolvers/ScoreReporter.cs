﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class ScoreReporter : IReporter
    {
        private DateTime _start { get; set; }
        private List<ReportEntry> _entries { get; set; } = new List<ReportEntry>();
        
        public void Start()
        {
            if (_start != default(DateTime));
                _start = DateTime.Now;
        }

        public void AddEntry(ReportEntry entry)
        {
            entry.SecondsFromStart = (entry.Time - _start).TotalSeconds;
            lock (_entries)
            {
                _entries.Add(entry);
            }
        }

        public void Save(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                int i = 0;
                foreach(var entry in _entries)
                {
                    i += 1;
                    writer.WriteLine($"{i},{entry.ToCsv()}");
                }
            }
        }
    }
}
