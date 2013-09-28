//==============================================================
// Forex Strategy Builder
// Copyright � Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;
using System.Diagnostics;
using System.IO;
using FSB_Launcher.Interfaces;

namespace FSB_Launcher.Helpers
{
    public class IoManager : IIoManager
    {
        public string CurrentDirectory
        {
            get { return Environment.CurrentDirectory; }
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void RunFile(string path, string arguments)
        {
            try
            {
                var process = new Process
                    {
                        StartInfo =
                            {
                                FileName = path,
                                Arguments = arguments,
                                // ReSharper disable AssignNullToNotNullAttribute
                                WorkingDirectory = Path.GetDirectoryName(path)
                                // ReSharper restore AssignNullToNotNullAttribute
                            }
                    };
                process.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}