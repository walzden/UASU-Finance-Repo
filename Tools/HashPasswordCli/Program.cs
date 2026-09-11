// Standalone tool - separate from the main web app, no dependency on it.
// Usage:
//   dotnet run                     -> prompts for a password (input hidden)
//   dotnet run -- "the-password"   -> hashes the given password directly
//
// Paste the printed hash into Users.PasswordHash - never store the
// plaintext password anywhere, including in shell history if you can
// avoid it (prefer the interactive prompt over the command-line argument
// for that reason).

string password;

if (args.Length > 0)
{
    password = args[0];
}
else
{
    Console.Write("Password to hash: ");
    password = ReadHidden();
}

if (string.IsNullOrEmpty(password))
{
    Console.Error.WriteLine("No password entered.");
    return;
}

var hash = BCrypt.Net.BCrypt.HashPassword(password);

Console.WriteLine();
Console.WriteLine("BCrypt hash (paste this into Users.PasswordHash):");
Console.WriteLine(hash);
Console.WriteLine($"Length: {hash.Length} characters (should always be exactly 60).");

// Also written to a file: a narrow terminal window wraps a 60-character
// line onto two visual lines, and a triple-click or drag-select often
// grabs only one of them - silently copying a truncated hash with no
// error until you try to log in with it. Selecting the whole content
// of a plain text file doesn't have that problem.
var outputPath = Path.Combine(AppContext.BaseDirectory, "hash_output.txt");
File.WriteAllText(outputPath, hash);
Console.WriteLine($"Also saved to: {outputPath}");
Console.WriteLine("(Delete that file once you've copied the hash - it's not sensitive on its own, but no reason to leave it lying around.)");

static string ReadHidden()
{
    var input = new System.Text.StringBuilder();
    ConsoleKeyInfo key;

    while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
    {
        if (key.Key == ConsoleKey.Backspace && input.Length > 0)
        {
            input.Remove(input.Length - 1, 1);
            Console.Write("\b \b");
        }
        else if (!char.IsControl(key.KeyChar))
        {
            input.Append(key.KeyChar);
            Console.Write("*");
        }
    }

    Console.WriteLine();
    return input.ToString();
}
