using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Small.Tools.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //571,223
            //Win32Mouse.SetCursorPos(569+5,695+121);
            Point p;
            Win32Mouse.GetCursorPos(out p);
            System.Console.ReadLine();
        }
    }
}
