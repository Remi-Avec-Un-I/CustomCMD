using System.Globalization;
using System.Text.Json;
using System.Diagnostics;
using Menu;
using lang;
using help;
using prompt;

public class CustomCMD
{
    private static string entry_prompt = "> ";
    private static string? lang;
    private static Dictionary<string, Command> commands;
    private static string[] help;

    static void Main(string[] args)
    {
        help = new string[] { "help", "h", "-h" , "--h", "-help", "--help", "?", "/?" };
        lang = GetLang();
        commands = new Dictionary<string, Command>();
        // Add your custom commands here
        commands["help"] = new HelpCommand(commands);
        commands["lang"] = new LangCommand(commands);
        commands["prompt"] = new PromptCommand(commands); 
        CMD(lang, commands);
    }

    public static void Print(string text, bool NewLine = true)
    {
        if (NewLine) Console.WriteLine(text);
        else Console.Write(text);
        
    }

    public static string GetLang()
    {
        string? text = File.ReadAllText("Settings/Settings.json");
        string? current = JsonSerializer.Deserialize<Settings>(text).lang;
        return current;
    }

    public static object GetSetting(string key)
    {
        string settingsPath = "Settings/Settings.json";
        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty(key, out JsonElement valueElement))
            {
                return valueElement.GetString();
            }
        }
        return string.Empty;
    }

    public static string GetValue(string key)
    {
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        string langFilePath = "Lang/Lang" + textInfo.ToTitleCase(GetLang()) + ".json";

        if (File.Exists(langFilePath))
        {
            string json = File.ReadAllText(langFilePath);
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty(key, out JsonElement valueElement))
            {
                return valueElement.GetString();
            }
        }

        return string.Empty; // Return an empty string if the key is not found or the file doesn't exist
    }

    public static void ChangeValue<T>(string path, string key, T value)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
            File.WriteAllText(path, JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public static bool isHelp(string name, string[] args)
    {
        if (args.Length == 0) return false;
        if (help.Contains(args[0]))
        {
            Console.WriteLine(GetValue(name + ".help"));
            return true;
        }
        return false;
    }

    public static void ExecuteCMD(string command)
    {
        Process process = new Process();

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/c " + command; // "/c" flag tells CMD to execute the command and then terminate
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;

        process.StartInfo = startInfo;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        Console.WriteLine(output);

        process.WaitForExit();
    }

    public static void CMD(
        string lang,
        Dictionary<string, Command> commands
    )
    {
        while (true)
        {
            entry_prompt = ReadPrompt.Read();
            Print(entry_prompt, false);
            string? input = Console.ReadLine();
            if (input.Trim() == "") continue;

            string[] commandAndArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = commandAndArgs[0].ToLower();

            Command cmd;
            if (commands.TryGetValue(command, out cmd))
            {
                string[] args = new string[commandAndArgs.Length - 1];
                Array.Copy(commandAndArgs, 1, args, 0, commandAndArgs.Length - 1);
                args = args.Select(s => s.ToLower()).ToArray();
                cmd.Execute(args);
            }
            else
            {
                bool aliasMatched = false;
                foreach (var kvp in commands)
                {
                    if (kvp.Value.Aliases != null && kvp.Value.Aliases.Contains(command))
                    {
                        cmd = kvp.Value;
                        string[] args = new string[commandAndArgs.Length - 1];
                        Array.Copy(commandAndArgs, 1, args, 0, commandAndArgs.Length - 1);
                        cmd.Execute(args);
                        aliasMatched = true;
                        break;
                    }
                }

                if (!aliasMatched)
                {
                    ExecuteCMD(command);
                }
            }
        }
    }
}

public abstract class Command
{
    public abstract string Name
    {
        get;
    }
    public abstract string[] Aliases
    {
        get;
    }
    public virtual void Execute(string[] args)
    {
        CustomCMD.isHelp(Name, args);
    }
}

public class Settings
{
    public string lang { get; set; }
    public List<string> prompt { get; set; }
}
