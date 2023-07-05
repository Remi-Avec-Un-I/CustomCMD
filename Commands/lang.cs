using System.CodeDom.Compiler;
using System.Text.Json;
using System.IO;
using Menu;


namespace lang;

public class LangCommand : Command
{
    public override string Name => "lang";
    public override string[] Aliases => new string[] { "changelang", "editlang", "language" };
    private Dictionary<string, Command> commands;

    public LangCommand(Dictionary<string, Command> commands)
    {
        this.commands = commands;
    }

    static List<string> GetLangs()
    {
        List<string> langs = new List<string>();
        string[] files = Directory.GetFiles("Lang");
        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            langs.Add(name.Replace("Lang", ""));
        }
        return langs;
    }

    static void SetLang(string lang, bool show = true)
    {
        string? text = File.ReadAllText("Settings/Settings.json");
        // indent it
        Settings settings = JsonSerializer.Deserialize<Settings>(text);
        settings.lang = lang;
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText("Settings/Settings.json", json);
        if (show)
        {
            Console.WriteLine(CustomCMD.GetValue("lang.change") + lang);
        }
    }

    public override void Execute(string[] args)
    {
        base.Execute(args);
        string current = CustomCMD.GetLang();
        List<string> langs = GetLangs();
        List<string> edit = new List<string>() { "edit", "change" };
        if (args.Length == 0)
        {
            Console.WriteLine(CustomCMD.GetValue("lang.current") + current);
        }
        else if (args[0] == "list" & args.Length == 1)
        {
            string[] files = Directory.GetFiles("Lang");
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine(name.Replace("Lang", " - "));
            }
        }
        else if (args.Length == 1 & edit.Contains(args[0]))
        {
            //Console.WriteLine(CustomCMD.GetValue("lang.current") + current);
            List<Option> options = new List<Option>();
            foreach (string name in langs)
            {
                options.Add(new Option(name, () => SetLang(name)));
            }
            Menu.Menu.RunMenu(options, CustomCMD.GetValue("lang.current") + current);
        }
        else if (args.Length == 1)
        {
            string newLang = args[0];
            string[] files = Directory.GetFiles("Lang");
            foreach (string name in langs)
            {
                if (name.ToLower() == newLang.ToLower())
                {
                    SetLang(newLang);
                    CustomCMD.CMD(newLang, commands);
                }
            }
        }
    }
}
