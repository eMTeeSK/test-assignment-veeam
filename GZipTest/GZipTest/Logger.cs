using GZipTest.Core;
using System;
using System.Collections.Generic;

namespace GZipTest
{
    class Logger : ILogger
    {
        private readonly int _left;
        private readonly int _top;
        private readonly List<string> _messages;
        private readonly List<string> _errors;
        private int _lastTop;

        public Logger()
        {
            this._left = Console.CursorLeft;
            this._top = Console.CursorTop;
            this._messages = new List<string>(10);
            this._errors = new List<string>(10);
        }

        public void ErrorLog(string msg)
        {
            this._errors.Add(msg);
            WriteError(msg);
        }

        private static void WriteError(string msg)
        {
            using (new SaveConsoleSetting(ConsoleColor.DarkRed, ConsoleColor.White))
            {
                Console.WriteLine(msg);
            }
        }

        public void Log(string msg)
        {
            this._messages.Add(msg);
            Console.WriteLine(msg);
        }


        public void ProgressInfo(string message)
        {
            Console.SetCursorPosition(this._left, this._top);
            Console.WriteLine(message);
            foreach (var msg in this._messages)
            {
                Console.WriteLine(msg);
            }
            foreach (var err in this._errors)
            {
                WriteError(err);
            }
            int currTop = Console.CursorTop;
            int currLeft = Console.CursorLeft;
            if (currTop < this._lastTop)
            {
                for (int i = currTop; i <= _lastTop; i++)
                {
                    Console.WriteLine();
                }
                Console.SetCursorPosition(currLeft, currTop);
            }
            this._lastTop = currTop;
        }

        private class SaveConsoleSetting : IDisposable
        {
            private readonly ConsoleColor bgColor;
            private readonly ConsoleColor fgColor;

            public SaveConsoleSetting()
            {
                this.bgColor = Console.BackgroundColor;
                this.fgColor = Console.ForegroundColor;
            }

            public SaveConsoleSetting(ConsoleColor background, ConsoleColor foreground) : this()
            {
                Console.BackgroundColor = background;
                Console.ForegroundColor = foreground;
            }

            public void Dispose()
            {
                Console.BackgroundColor = this.bgColor;
                Console.ForegroundColor = this.fgColor;
            }
        }
    }

}
