using System;
using System.Collections.Generic;

namespace Menu
{
    class Menu
    {
        public static List<Option> options;
        public static void WriteMenu(Option selected, List<Option> options, string title = "", string end = "")
        {
            Console.Clear();
            if (title != "") Console.WriteLine(title);
            foreach (Option option in options)
            {
                if (option == selected)
                    Console.Write(" > ");
                else
                    Console.Write("   ");
                Console.WriteLine(option.Name);
            }
            if (end != "") Console.WriteLine(end);
        }

        public static void RunMenu(List<Option> options, string title = "", string end = "") 
        {
            int index = 0;
            WriteMenu(options[index], options, title, end);
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    options[index].Selected();
                    return;
                }
                Console.Clear();
                if (key.Key == ConsoleKey.UpArrow)
                {
                    index--;
                    if (index == -1)
                    {
                        index = options.Count - 1;
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    index++;
                    if (index > options.Count - 1)
                    {
                        index = 0;
                    }
                }
                WriteMenu(options[index], options, title, end);
            }
        }

        public static readonly Option Cancel = new(CustomCMD.GetValue("cancel"), () => { });

        public static readonly Dictionary<string, string> colorsFore = new()
        {
            { "black", "\u001b[30m" },
            { "red", "\u001b[31m" },
            { "green", "\u001b[32m" },
            { "yellow", "\u001b[33m" },
            { "blue", "\u001b[34m" },
            { "magenta", "\u001b[35m" },
            { "cyan", "\u001b[36m" },
            { "white", "\u001b[37m" }
        };

        public static readonly Dictionary<string, string> colorsBack = new()
        {
            { "black", "\u001b[40m" },
            { "red", "\u001b[41m" },
            { "green", "\u001b[42m" },
            { "yellow", "\u001b[43m" },
            { "blue", "\u001b[44m" },
            { "magenta", "\u001b[45m" },
            { "cyan", "\u001b[46m" },
            { "white", "\u001b[47m" }
        };

        public static readonly Dictionary<string, string> styles = new()
        {
            { "bright", "\u001b[1m" },
            { "dim", "\u001b[2m" },
            { "normal", "\u001b[22m" }
        };

        public static readonly Dictionary<string, string> reset = new()
        {
            { "reset", "\u001b[0m" }
        };
    }
    class Option
    {
        public string Name { get; }
        public Action Selected { get; }

        public Option(string name, Action selected)
        {
            Name = name;
            Selected = selected;
        }
    }
}