using System.Text;


// Speed settings (milliseconds)
const int TYPE_EFFECT_SPEED = 2;     // milliseconds per char
const int JUMBLE_SECONDS = 2;        // number of seconds for jumble effect
const int JUMBLE_LOOP_SPEED = 35;    // milliseconds between each jumble
const int REVEAL_LOOP_SPEED = 50;    // milliseconds between each reveal loop

// ANSI escape sequences
const string CURSOR_HIDE = "\x1b[?25l";
const string CURSOR_SHOW = "\x1b[?25h";
const string BOLD = "\x1b[1m";
const string CLEAR_ATTR = "\x1b[0m";
const string COLOR_BLUE = "\x1b[34m";

try
{
    // Check if input is piped
    if (Console.IsInputRedirected)
    {
        // Read all piped input
        string input;
        using (var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8))
        {
            input = reader.ReadToEnd();
        }

        if (string.IsNullOrEmpty(input))
        {
            Console.Error.WriteLine("Error: No input provided");
            Environment.Exit(1);
        }

        // Execute the effect
        ExecuteEffect(input);
    }
    else
    {
        Console.Error.WriteLine("Error: Input data from a piped or redirected source is required.");
        Console.Error.WriteLine("Usage: echo \"your text\" | dotnet run");
        Environment.Exit(1);
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

static void ExecuteEffect(string input)
{
    // Get terminal dimensions
    var maxCols = Console.WindowWidth;
    var maxRows = Console.WindowHeight;

    // Hide cursor
    Console.Write(CURSOR_HIDE);

    // Parse the input and build character list while outputting garbled text
    var characters = new List<CharacterInfo>();

    var startTop = Console.CursorTop;
    var startLeft = Console.CursorLeft;
    var currentTop = startTop;
    var currentLeft = startLeft;

    // Output garbled text first, tracking positions as we go
    foreach (char c in input)
    {
        if (c == '\n')
        {
            characters.Add(new CharacterInfo(c, currentTop, currentLeft));
            Console.WriteLine(); // Move to next line
            currentTop++;
            currentLeft = 0;

            // Handle scrolling - if we went past the bottom, everything shifts up
            if (currentTop >= maxRows)
            {
                // Adjust all previous character positions
                foreach (var ch in characters)
                {
                    ch.Row--;
                }
                currentTop = maxRows - 1;
                startTop--;
            }
        }
        else if (c == '\r')
        {
            continue; // Skip carriage returns
        }
        else
        {
            // Check for line wrap
            if (currentLeft >= maxCols)
            {
                currentTop++;
                currentLeft = 0;

                // Handle scrolling
                if (currentTop >= maxRows)
                {
                    foreach (var ch in characters)
                    {
                        ch.Row--;
                    }
                    currentTop = maxRows - 1;
                    startTop--;
                }
            }

            // Create character info and output garbled version
            var charInfo = new CharacterInfo(c, currentTop, currentLeft);
            characters.Add(charInfo);

            // Output mask character or space
            if (charInfo.IsSpace)
            {
                Console.Write(c);
            }
            else
            {
                Console.Write(charInfo.MaskChar);
                // Add typing effect delay
                Thread.Sleep(TYPE_EFFECT_SPEED);
            }

            currentLeft++;
        }
    }

    // Store where we ended for final cursor positioning
    var endTop = Console.CursorTop;
    var endLeft = Console.CursorLeft;

    // Wait for 1 second before starting decryption (auto-decrypt mode)
    // The garbled text is already displayed from initial output
    Thread.Sleep(1000);

    // Phase 2: Jumble effect
    var jumbleLoops = (JUMBLE_SECONDS * 1000) / JUMBLE_LOOP_SPEED;
    for (var i = 0; i < jumbleLoops; i++)
    {
        foreach (var charInfo in characters)
        {
            if (charInfo.OriginalChar == '\n')
                continue;

            Console.SetCursorPosition(charInfo.Col, charInfo.Row);

            if (charInfo.IsSpace)
            {
                Console.Write(charInfo.OriginalChar);
            }
            else
            {
                Console.Write(CharacterInfo.GetRandomMaskChar());
            }
        }

        Thread.Sleep(JUMBLE_LOOP_SPEED);
    }

    // Phase 3: Reveal effect
    var allRevealed = false;
    while (!allRevealed)
    {
        allRevealed = true;

        foreach (var charInfo in characters)
        {
            if (charInfo.OriginalChar == '\n')
                continue;

            Console.SetCursorPosition(charInfo.Col, charInfo.Row);

            if (charInfo.IsSpace)
            {
                Console.Write(charInfo.OriginalChar);
            }
            else if (charInfo.RevealTime > 0)
            {
                // Still masked - occasionally change the mask character
                if (charInfo.RevealTime < 500)
                {
                    if (Random.Shared.Next(3) == 0)
                        charInfo.MaskChar = CharacterInfo.GetRandomMaskChar();
                }
                else
                {
                    if (Random.Shared.Next(10) == 0)
                        charInfo.MaskChar = CharacterInfo.GetRandomMaskChar();
                }

                Console.Write(charInfo.MaskChar);
                charInfo.RevealTime -= REVEAL_LOOP_SPEED;
                allRevealed = false;
            }
            else
            {
                // Reveal the character with blue color and bold
                Console.Write(BOLD + COLOR_BLUE + charInfo.OriginalChar + CLEAR_ATTR);
            }
        }

        if (!allRevealed)
            Thread.Sleep(REVEAL_LOOP_SPEED);
    }

    // Show cursor and restore position
    Console.Write(CURSOR_SHOW);
    Console.SetCursorPosition(endLeft, endTop);
    Console.WriteLine(); // Final newline
}

class CharacterInfo(char original, int row, int col)
{
    // Character table (CP437-inspired)
    static readonly string[] CharTable = {
        "!", "\"", "#", "$", "%", "&", "'", "(", ")", "*", "+", ",", "-", "~",
        ".", "/", ":", ";", "<", "=", ">", "?", "[", "\\", "]", "_", "{", "}",
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
        "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
        "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "Ç", "ü", "é", "â", "ä", "à", "å", "ç", "ê", "ë", "è", "ï",
        "î", "ì", "Ä", "Å", "É", "æ", "Æ", "ô", "ö", "ò", "û", "ù",
        "ÿ", "Ö", "Ü", "¢", "£", "¥", "ƒ", "á", "í", "ó", "ú", "ñ",
        "Ñ", "ª", "º", "¿", "¬", "½", "¼", "¡", "«", "»", "α", "ß",
        "Γ", "π", "Σ", "σ", "µ", "τ", "Φ", "Θ", "Ω", "δ", "φ", "ε",
        "±", "÷", "°", "·", "²", "¶", "⌐", "₧", "░", "▒", "▓", "│",
        "┤", "╡", "╢", "╖", "╕", "╣", "║", "╗", "╝", "╜", "╛", "┐",
        "└", "┴", "┬", "├", "─", "┼", "╞", "╟", "╚", "╔", "╩", "╦",
        "╠", "═", "╬", "╧", "╨", "╤", "╥", "╙", "╘", "╒", "╓", "╫",
        "╪", "┘", "┌", "█", "▄", "▌", "▐", "▀", "∞", "∩", "≡", "≥",
        "≤", "⌠", "⌡", "≈", "∙", "√", "ⁿ", "■"
    };

    public char OriginalChar { get; set; } = original;
    public string MaskChar { get; set; } = GetRandomMaskChar();
    public int RevealTime { get; set; } = Random.Shared.Next(0, 5000);
    public bool IsSpace { get; set; } = char.IsWhiteSpace(original) && original != '\n';
    public int Row { get; set; } = row;
    public int Col { get; set; } = col;

    public static string GetRandomMaskChar()
    {
        return CharTable[Random.Shared.Next(CharTable.Length)];
    }
}