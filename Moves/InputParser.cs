using System;
using System.Collections.Generic;
using System.Linq;

public class InputParser
{
    private string commandString;
    private string stance;
    private string notes;
    private bool rage;
    private IEnumerable<Command> commands;
    public InputParser(string command)
    {
        this.commandString = command;
    }

    public void Parse()
    {
        ParseRage();
        if (!TryParseDirectionalInput() && !TryParseButtonInput())
            ParseStance();
        ParseCommands();
        //parse "or"
        //parse the rest
        //parse extra (), can be removed
    }

    public string GetStance()
    {
        return stance;
    }

    public bool Rage()
    {
        return rage;
    }

    public string GetNotes()
    {
        return notes;
    }

    public IEnumerable<Command> GetCommands()
    {
        return commands;
    }

    private void ProgressCommandString(string parsed)
    {
        commandString = string.Concat(commandString.Substring(parsed.Length).SkipWhile(c => c == ' ' || c == '+' || c == ','));
    }

    private void ParseRage()
    {
        const string inRage = "in rage";
        if (commandString.StartsWith(inRage))
        {
            rage = true;
            ProgressCommandString(inRage);
        }
    }

    private bool TryParseDirectionalInput()
    {
        var s = string.Concat(commandString.TakeWhile(c => c != '+' && c != ' ' && c != ','));
        return !Enum.TryParse(typeof(DirectionalInput), s.Replace("/", ""), true, out _);
    }

    private bool TryParseButtonInput()
    {
        var s = string.Concat(commandString.TakeWhile(c => c == '+' || char.IsDigit(c)));
        return string.IsNullOrEmpty(s);
    }

    private void ParseStance()
    {
        stance = string.Concat(commandString.TakeWhile(c => c != ' ' && c != '+'));
        ProgressCommandString(stance);
    }

    public void ParseCommands()
    {
        commands = commandString.Replace(" ", "").Split(",")
        .Select(command =>
          {
              var directionalInput = DirectionalInput.n;
              var buttonInput = new ButtonInput();

              var s = string.Concat(command.TakeWhile(c => c != '+' && c != ' ' && c != ','));
              if (Enum.TryParse(typeof(DirectionalInput), s.Replace("/", ""), true, out var d))
              {
                  directionalInput = (DirectionalInput)d;
                  command = command.Substring(Math.Min(s.Length + 1, command.Length));
              }

              for (var i = 0; i < command.Length; ++i)
              {
                  if (command[i] == '+')
                  {
                      continue;
                  }
                  if (command[i] == '~')
                  {
                      buttonInput.Cancel = true;
                  }
                  if (char.IsDigit(command[i]))
                  {
                      buttonInput.SetButton(command[i]);
                  }
              }
              return new Command { DirectionalInput = directionalInput, ButtonInput = buttonInput };
          }).ToList();
    }
}