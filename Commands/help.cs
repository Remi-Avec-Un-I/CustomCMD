namespace help;

public class HelpCommand : Command
{
    public override string Name => "help";
    public override string[] Aliases => new string[] { "h" };

    private Dictionary<string, Command> commands;

    public HelpCommand(Dictionary<string, Command> commands)
    {
        this.commands = commands;
    }

    public override void Execute(string[] args)
    {
        CustomCMD.ExecuteCMD("help");
        Console.WriteLine(CustomCMD.GetValue("help.call"));

        foreach (Command cmd in commands.Values)
        {
            Console.WriteLine(cmd.Name.PadRight(15) + CustomCMD.GetValue(cmd.Name));
        }
    }
}