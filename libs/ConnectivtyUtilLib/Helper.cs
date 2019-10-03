using System;

namespace VehicleDemonstrator.Shared.Util
{
    public class Helper
    {
        public static void WriteLine(string value, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(value);
            Console.ResetColor();
        }
    }
}
