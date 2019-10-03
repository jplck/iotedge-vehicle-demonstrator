using System;

namespace locationmodule
{
    class Helper
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
