namespace BD.WTTS.Models;

/// <summary>
/// 命令历史记录
/// </summary>
public class ArchiSteamFarmCommandHistory
{
    private List<string> commandHistory = new List<string>();
    private int currentIndex = -1;
    private string currentCommand = string.Empty;

    #region Properties

    /// <summary>
    /// Gets the total number of commands in the history.
    /// </summary>
    public int Count
    {
        get { return commandHistory.Count; }
    }

    /// <summary>
    /// Gets or sets the current command.
    /// </summary>
    /// <value>
    /// The current command.
    /// </value>
    public string CurrentCommand
    {
        get
        { return currentCommand; }

        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                currentCommand = value;
            }
        }
    }

    /// <summary>
    /// Gets the command history list.
    /// </summary>
    public List<string> CommandHistoryList
    {
        get { return commandHistory; }
    }

    /// <summary>
    /// Gets or sets the current command history index.
    /// </summary>
    /// <value>
    /// The current index.
    /// </value>
    public int CurrentIndex
    {
        get { return currentIndex; }
        set { currentIndex = value; }
    }

    #endregion

    #region Command Control

    /// <summary>
    /// Adds to history. Skips empty commands.
    /// </summary>
    /// <param name="command">The command to add.</param>
    public void AddCommandToHistory(string command)
    {
        if (!string.IsNullOrEmpty(command)) { commandHistory.Add(command); }
    }

    /// <summary>
    /// Clears the command history.
    /// </summary>
    public void ClearHistory()
    {
        commandHistory.Clear();
        currentIndex = -1;
    }

    /// <summary>
    /// Gets the next or previous command in the que.
    /// </summary>
    /// <returns></returns>
    private string GetCommand()
    {
        string selected = currentCommand;

        if (commandHistory.Count > 0)
        {
            if (currentIndex >= 0 && currentIndex < commandHistory.Count)
            {
                selected = commandHistory.ElementAt(currentIndex);
            }
        }

        return selected;
    }

    /// <summary>
    /// Gets the command at the selected index number.
    /// </summary>
    /// <param name="indexNumber">The index number.</param>
    /// <returns></returns>
    public string GetCommand(int indexNumber)
    {
        if (indexNumber >= 0 && indexNumber < commandHistory.Count) { return commandHistory.ElementAt(indexNumber); }
        return string.Empty;
    }

    /// <summary>
    /// Gets the previous command.
    /// </summary>
    /// <returns>The previous command.</returns>
    public string GetPrevious()
    {
        currentIndex--;
        if (currentIndex < 0) { currentIndex = commandHistory.Count - 1; }
        return GetCommand();
    }

    /// <summary>
    /// Gets the next command.
    /// </summary>
    /// <returns>The next command.</returns>
    public string GetNext()
    {
        currentIndex++;
        if (currentIndex >= commandHistory.Count) { currentIndex = 0; }
        return GetCommand();
    }

    /// <summary>
    /// Removes a command from the command history.
    /// </summary>
    /// <param name="indexNumber">The index number of the command to remove.</param>
    public void RemoveFromCommandHistory(int indexNumber)
    {
        if (indexNumber >= 0 && indexNumber < commandHistory.Count)
        {
            commandHistory.RemoveAt(indexNumber);
        }
        else
        {
            throw new ArgumentOutOfRangeException("The selected index number was outside of the bounds of the list");
        }
    }

    #endregion
}
