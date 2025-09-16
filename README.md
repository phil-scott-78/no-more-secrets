# No More Secrets

A single-file C# implementation that recreates the famous data decryption effect from the 1992 movie Sneakers. 

![Demo of NMS.NET in action](https://github.com/phil-scott-78/no-more-secrets/raw/refs/heads/main/nms.webp)

## Features

- Single-file implementation (`Program.cs`)
- Supports piped input from any command
- UTF-8 text support
- Handles console scrolling properly
- Three-phase animation:
  1. Typing effect with masked characters
  2. Jumble effect (2 seconds)
  3. Gradual reveal with blue colored text
- Auto-decrypt mode (no keypress required)

## Requirements

- .NET 10.0 or later

## Usage

The program works on piped data. Pipe any text output to it:

```powershell
# Windows PowerShell
Get-Content file.txt | dotnet run nms.cs
```

## License

MIT License - See LICENSE file for details