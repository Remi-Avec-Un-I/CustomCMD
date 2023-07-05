using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;

namespace prompt;
public class PromptCommand : Command
{
    public override string Name => "prompt";
    public override string[] Aliases => new string[] { "prompt" };
    private List<string> edit = new List<string>() { "change", "edit" };
    readonly Dictionary<string, Command> commands;

    public PromptCommand(Dictionary<string, Command> commands)
    {
        this.commands = commands;
    }

    public override void Execute(string[] args)
    {
        base.Execute(args);

        if (args.Length == 0)
        {
            Console.WriteLine(CustomCMD.GetValue("prompt.show") + ReadPrompt.Read());
        }
        else if (args.Length == 1)
        {
            if (args[0] == "show")
            {
                Console.WriteLine(CustomCMD.GetValue("prompt.show") + ReadPrompt.Read());
            }
            else if (edit.Contains(args[0])) 
            {
                ReadPrompt.Editing();
            }
        }
    }
}

public class ReadPrompt
{

    public static string Read()
    {
        List<string> elements = GetPrompt();
        if (elements.Count == 0) return "";
        string result = "";
        foreach (string element in elements)
        {
            if (element == "%path")
            {
                result += Environment.CurrentDirectory;
            }
            else if (element.StartsWith("%path:"))
            {
                result += Environment.CurrentDirectory.Replace("\\", element.Replace("%path:", ""));
            }
            else if (element == "%user")
            {
                result += Environment.UserName;
            }
            else if (element == "%os")
            {
                result += Environment.OSVersion;
            }
            else if (element == "%workingset")
            {
                result += Environment.WorkingSet;
            }
            else if (element == "%time")
            {
                result += DateTime.Now.ToString("HH:mm:ss");
            }
            else if (element == "%date")
            {
                result += DateTime.Now.ToString("dd/MM/yyyy");
            }
            else if (element == "%newline")
            {
                result += "\n";
            }
            else if (element == "%reset")
            {
                result += Menu.Menu.reset["reset"];
            }
            else if (element.StartsWith("%fore:"))
            {
                result += Menu.Menu.colorsFore[element.Replace("%fore:", "").Trim()];
            }
            else if (element.StartsWith("%back:"))
            {
                result += Menu.Menu.colorsBack[element.Replace("%back:", "").Trim()];
            }
            else if (element.StartsWith("%style:"))
            {
                result += Menu.Menu.styles[element.Replace("%style:", "").Trim()];
            }
            else
            {
                result += element;
            }
        }
        return result;
    }

    static List<string> GetPrompt()
    {
        string settingsPath = "Settings/Settings.json";
        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            using JsonDocument document = JsonDocument.Parse(json);
            // check if there is a prompt key
            if (!document.RootElement.TryGetProperty("prompt", out JsonElement prompt))
            {
                CustomCMD.ChangeValue<List<string>>(settingsPath, "prompt", new List<string>() { " > " });
                return new List<string>() { " > " };
            }
            return document.RootElement.GetProperty("prompt").EnumerateArray().Select(x => x.GetString()).ToList();
        }
        return new List<string>();
    }

    public static void Write()
    {
        List<Menu.Option> options = new List<Menu.Option>
        {
            new Menu.Option(CustomCMD.GetValue("edit"), Editing),
            new Menu.Option(CustomCMD.GetValue("reset"), Reset),
            Menu.Menu.Cancel
        };
        Menu.Menu.RunMenu(options);
    }

    public static void Editing()
    {
        int x = 0;
        int y = 0;
        List<Option> x_options = new List<Option>();
        List<string> y_options = GetPrompt();
        List<string> special = new List<string>() { "%path", "%user", "%os", "%workingset", "%time", "%date", "%newline" };
        List<string> colors = new List<string>() { "%fore:", "%back:", "%style:" };
        Console.CursorVisible = false;
        while (true)
        {
            Console.Clear();
            if (special.Contains(y_options[y]) || colors.Contains(y_options[y]))
            {
                x_options.Add(new Option(CustomCMD.GetValue("tips"), () => { }));
            }
            else if (x > 4) x = 4;

            x_options.Add(new Option(CustomCMD.GetValue("edit"), () => EditMenu(y)));
            x_options.Add(new Option(CustomCMD.GetValue("add_right"), () => AddEmpty(y, 1)));
            x_options.Add(new Option(CustomCMD.GetValue("add_left"), () => AddEmpty(y, 0)));
            x_options.Add(new Option(CustomCMD.GetValue("delete"), () => Delete(y)));
            x_options.Add(new Option(CustomCMD.GetValue("cancel"), () => { }));
            WriteComplexMenu(x_options, y_options, x_options[x], y);
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.LeftArrow)
            {
                y--;
                if (y < 0) y = y_options.Count - 1;
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                y++;
                if (y >= y_options.Count) y = 0;
            }
            else if (key.Key == ConsoleKey.UpArrow)
            {
                x--;
                if (x < 0) x = x_options.Count - 1;
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                x++;
                if (x >= x_options.Count) x = 0;
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                if (x == x_options.Count - 1) break;
                x_options[x].Selected();
                Console.CursorVisible = true;
            }
            x_options.Clear();
        }
        Console.WriteLine("");
    }

    private static void Reset()
    {
        List<Menu.Option> options = new List<Menu.Option>
        {
            new Menu.Option(CustomCMD.GetValue("yes"), () => Edit(new List<string>() { " > " })),
            new Menu.Option(CustomCMD.GetValue("no"), () => { })
        };
        Menu.Menu.RunMenu(options, CustomCMD.GetValue("verification"));
    }

    private static void Edit(List<string> value)
    {
        CustomCMD.ChangeValue("Settings/Settings.json", "prompt", value);
    }

    private static void EditMenu(int index)
    {
        Console.Clear();
        List<Menu.Option> options = new List<Menu.Option>
        {
            new Menu.Option(CustomCMD.GetValue("prompt.custom"), () => EditCustom(index)),
            new Menu.Option(CustomCMD.GetValue("prompt.path"), () => EditIndex(index, "%path")),
            new Menu.Option(CustomCMD.GetValue("prompt.colors"), () => EditColors(index)),
            new Menu.Option(CustomCMD.GetValue("prompt.user"), () => EditIndex(index, "%user")),
            new Menu.Option(CustomCMD.GetValue("prompt.os"), () => EditIndex(index, "%os")),
            new Menu.Option(CustomCMD.GetValue("prompt.workingset"), () => EditIndex(index, "%workingset")),
            new Menu.Option(CustomCMD.GetValue("prompt.time"), () => EditIndex(index, "%time")),
            new Menu.Option(CustomCMD.GetValue("prompt.date"), () => EditIndex(index, "%date")),
            new Menu.Option(CustomCMD.GetValue("prompt.newline"), () => EditIndex(index, "%newline")),
            Menu.Menu.Cancel
        };
        Menu.Menu.RunMenu(options);
    }

    private static void EditIndex(int index, string value)
    {
        Console.Clear();
        List<string> prompt = GetPrompt();
        prompt[index] = value;
        Edit(prompt);
    }

    private static void EditCustom(int index)
    {
        Console.Clear();
        Console.Write(CustomCMD.GetValue("prompt.custom") + " : ");
        string value = Console.ReadLine();
        EditIndex(index, value);
    }

    private static void EditColors(int index)
    {
        Console.Clear();
        List<string> prompt = GetPrompt();
        List<Menu.Option> options = new List<Menu.Option>
        {
            new Menu.Option(CustomCMD.GetValue("prompt.foreground"), () => MenuColors(index, Menu.Menu.colorsFore)),
            new Menu.Option(CustomCMD.GetValue("prompt.background"), () => MenuColors(index, Menu.Menu.colorsBack)),
            new Menu.Option(CustomCMD.GetValue("prompt.style"), () => MenuColors(index, Menu.Menu.styles)),
            Menu.Menu.Cancel
        };
        Menu.Menu.RunMenu(options);
    }

    private static void MenuColors(int index, Dictionary<string, string> colors)
    {
        Console.Clear();
        List<Menu.Option> options = new List<Menu.Option>();
        foreach (string color in colors.Keys)
        {
            options.Add(new Menu.Option(color + " " + colors[color] + CustomCMD.GetValue("prompt.exemple") + Menu.Menu.reset["reset"], 
                () => EditIndex(index, colors[color])
                )
            );
        }
        options.Add(new Menu.Option(CustomCMD.GetValue("prompt.reset"), () => EditIndex(index, Menu.Menu.reset["reset"])));
        options.Add(Menu.Menu.Cancel);
        Menu.Menu.RunMenu(options);
    }

    private static void AddEmpty(int index, int padd = 0)
    {
        List<string> prompt = GetPrompt();
        prompt.Insert(index + padd, "---");
        Edit(prompt);
    }

    private static void Delete(int index)
    {
        List<string> prompt = GetPrompt();
        prompt.RemoveAt(index);
        Edit(prompt);
    }

    private static void WriteComplexMenu(List<Option> x_options, List<string> y_options, Option x_selected, int y_selected, string title="", string end="")
    {
        if (title != "") Console.WriteLine(title);
        foreach (Option option in x_options)
        {
            if (option == x_selected)
            {
                Console.Write(" > ");
            }
            else
            {
                Console.Write("   ");
            }
            Console.WriteLine(option.Name);
        }
        Console.WriteLine(end);
        int tab = 0;
        bool found = false;
        int index = -1;
        int length = 0;
        foreach (string option in y_options)
        {
            index++;
            if (Menu.Menu.colorsBack.Values.Contains(option) 
                || Menu.Menu.colorsFore.Values.Contains(option) 
                || Menu.Menu.styles.Values.Contains(option)
                || option == "\u001b[0m")
            {
                length = 0;
            }
            else
            {
                length = option.Length;
            }
            if (index == y_selected)
            {
                found = true;
                tab += Math.Abs(length / 2);
            }
            else if (!found)
            {
                tab += length + 1;
            }
            Console.Write(option + " ");
        }
        Console.WriteLine();
        for (int i = 0; i < tab; i++)
        {
            Console.Write(" ");
        }
        Console.Write("^");
    }

    class Option
    {
        public string Name { get; }
        public Action Selected { get; }
        public int index { get; set; }
        public Option(string name, Action selected)
        {
            Name = name;
            Selected = selected;
        }
    }
}
