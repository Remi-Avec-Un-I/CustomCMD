using Menu;

namespace Reco;

class Reco
{

    public static string RecoReadLine()
    {
        List<string> commands = new();
        foreach (string command in CustomCMD.commands.Keys)
        {
            commands.Add(command);
            foreach (string alias in CustomCMD.commands[command].Aliases)
            {
                commands.Add(alias);
            }
        }
        string result = "";
        int suggestion;
        bool isSug = false;
        string currentSug = "";
        while (true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("");
                    return result;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (result.Length > 0)
                    {
                        result = result.Remove(result.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    Console.Write(currentSug.Substring(result.Trim().Length));
                    result += currentSug.Substring(result.Trim().Length);
                    isSug = false;
                }
                else
                {
                    Console.Write(key.KeyChar);
                    result += key.KeyChar;
                }
                foreach (string command in commands)
                {
                    if (command.StartsWith(result.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        suggestion = command.Substring(result.Trim().Length).Length;
                        Console.Write(command.Substring(result.Trim().Length));
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(Console.CursorLeft - suggestion, Console.CursorTop);
                        isSug = true;
                        currentSug = command;
                        break;
                    }
                }
                if (!isSug)
                {
                    ClearRight();
                    isSug = false;
                    currentSug = "";
                }
                else isSug = false;

                if (result.Trim() == "") ClearRight();

            }
        }
    }

    private static void ClearRight()
    {
        int ToClear = Console.WindowWidth - Console.CursorLeft - 1;
        for (int i = 0; i < ToClear; i++)
        {
            Console.Write(" ");
        }
        Console.SetCursorPosition(Console.CursorLeft - ToClear, Console.CursorTop);
    }
}
