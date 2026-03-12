// Author: rythondev , https://twitch.tv/rythondev , https://x.com/rythondev, https://ko-fi.com/rython
// Contact: rythondev@gmail.com , or on the above mentioned social media.
//
// This code is licensed under the GNU General Public License Version 3 (GPLv3).
// 
// The GPLv3 is a free software license that ensures end users have the freedom to run,
// study, share, and modify the software. Key provisions include:
// 
// - Copyleft: Modified versions of the code must also be licensed under the GPLv3.
// - Source Code: You must provide access to the source code when distributing the software.
// - Credit: You must credit the original author of the software, by mentioning either contact e-mail or their social media.
// - No Warranty: The software is provided "as-is," without warranty of any kind.
// 
// For more details, see https://www.gnu.org/licenses/gpl-3.0.en.html.
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Model;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Common.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#region Data Models
public class Task
{
    public string Name;
    public bool Completed;
    public bool Focused;
    public DateTime AddedTime;
    public DateTime UpdatedTime;
    public DateTime? CompletedTime;
    public Task(string Name, bool Completed = false, bool Focused = false)
    {
        this.Name = Name;
        this.Completed = Completed;
        this.Focused = Focused;
        this.AddedTime = DateTime.Now;
        this.UpdatedTime = DateTime.Now;
    }
}

public class UserData
{
    public string Username;
    public List<Task> Tasks;
    public int TotalCompletedCount;
    public UserData(List<Task> tasks, string username)
    {
        this.Username = username;
        this.Tasks = tasks;
        this.TotalCompletedCount = 0;
    }
}

public class Response
{
    public bool Success { get; set; }
    public string ErrorMsg { get; set; }

    public Response(bool Success, string ErrorMsg = null)
    {
        this.Success = Success;
        this.ErrorMsg = ErrorMsg;
    }
}

public class Response<T> : Response
{
    public T Data { get; set; }

    public Response(bool success, T data = default(T), string errorMsg = null) : base(success, errorMsg)
    {
        this.Data = data;
    }
}

#endregion
#region Helper Classes
public static class TaskHelpers
{
    public static readonly char[] Separators =
    {
        '|',
        ',',
        ';'
    };
    public const int CharacterLimit = 450;
    public static List<string> SplitTasks(string tasks)
    {
        return tasks.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
    }

    // return true index
    public static int GetTaskIndex(List<Task> tasks, string input)
    {
        if (int.TryParse(input, out int n))
        {
            int index = n - 1;
            return (index >= 0 && index < tasks.Count) ? index : -1;
        }

        return tasks.FindIndex(t => t.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
    }

    public static List<string> ParseTasksInput(string input, Func<int> getFocusedTask)
    {
        input = input.Trim();
        if (input == "all")
        {
            return new()
            {
                "all"
            };
        }

        bool IsSpaceSeparatedInts = Regex.IsMatch(input, @"^\d+(\s+\d+)*$");
        if (IsSpaceSeparatedInts)
        {
            return input.Split(' ').ToList();
        }

        var splittedInput = input.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
        if (splittedInput.Count == 0)
        {
            int focusedTaskIndex = getFocusedTask() + 1;
            return new List<string>
            {
                focusedTaskIndex.ToString()
            };
        }

        return splittedInput;
    }

    public static List<string> ParseUndoneInput(string input, List<Task> userTasks)
    {
        input = input.Trim();
        bool IsSpaceSeparatedInts = Regex.IsMatch(input, @"^\d+(\s+\d+)*$");
        if (IsSpaceSeparatedInts)
        {
            return input.Split(' ').ToList();
        }

        var splittedInput = input.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
        if (splittedInput.Count == 0)
        {
            var completedTasks = userTasks.Where(t => t.Completed);
            if (completedTasks.Count() == 1)
            {
                int soloCompletedTaskIndex = userTasks.FindIndex(t => t.Completed) + 1;
                return new List<string>
                {
                    soloCompletedTaskIndex.ToString()
                };
            }
        }
        else
        {
            return splittedInput;
        }

        return new List<string>();
    }

    public static Response<(int, string)> ParseEditInput(string rawInput, int focusedTaskIndex)
    {
        bool validEditInput = true;
        rawInput = rawInput.Trim();
        bool hasFocusedTask = focusedTaskIndex > -1;
        string[] spaceSeparated = rawInput.Split(new[] { ' ' }, 2);
        if (spaceSeparated.Length < 2)
        {
            validEditInput = false;
            if (!hasFocusedTask)
            {
                return new Response<(int, string)>(false, default, "❌ Error: Not enough arguments, try !edit <number> <new task>");
            }
        }

        string numberString = spaceSeparated[0];
        string newTask = spaceSeparated.Length > 1 ? spaceSeparated[1] : rawInput;
        if (!int.TryParse(numberString, out int index))
        {
            validEditInput = false;
            if (!hasFocusedTask)
            {
                return new Response<(int, string)>(false, default, "❌  Error: Not a valid argument, try !edit <number> <new task>");
            }
        }

        if (validEditInput)
        {
            return new Response<(int, string)>(true, (index - 1, newTask), null);
        }

        return new Response<(int, string)>(true, (focusedTaskIndex, rawInput), null);
    }
}

public static class MessageBuilder
{
    public static string BuildAddResponseMessage(List<string> added, List<(string, string)> failed)
    {
        if (added.Count > 0 && failed.Count == 0)
        {
            string response = $"Task List Update ♡ → The task(s): \"{String.Join("\", \"", added)}\" have been added! Good luck! 🍀";
            return response.Length > TaskHelpers.CharacterLimit ? "Task List Update ♡ →  Your task(s) have been added! Good luck! 🍀" : response;
        }

        if (added.Count == 0 && failed.Count == 1)
        {
            return failed[0].Item2;
        }

        if (added.Count == 0 && failed.Count > 1)
        {
            string response = $"❌ Failed to add your task(s) {String.Join(", ", failed.Select(f => f.Item1))}";
            return response.Length > TaskHelpers.CharacterLimit ? "❌ Failed to add your task(s)" : response;
        }

        if (added.Count > 0 && failed.Count > 0)
        {
            string response = $"Task List Update ♡ → Task(s) added: \"{String.Join("\", \"", added)}\" | Failed: {String.Join(", ", failed.Select(f => f.Item1))}";
            return response.Length > TaskHelpers.CharacterLimit ? "Task List Update ♡ → some tasks were successful but some failed :p" : response;
        }

        return "No tasks to add";
    }

    public static string BuildLogResponseMessage(List<string> logged, List<(string, string)> failed)
    {
        if (logged.Count > 0 && failed.Count == 0)
        {
            string response = $"Task List Update ♡ → The task(s) {String.Join(", ", logged)} have been logged! Good job! 🍀";
            return response.Length > TaskHelpers.CharacterLimit ? "Task List Update ♡ →  Your task(s) have been logged! Good job! 🍀" : response;
        }

        if (logged.Count == 0 && failed.Count == 1)
        {
            return failed[0].Item2;
        }

        if (logged.Count == 0 && failed.Count > 1)
        {
            string response = $"❌ Failed to logged your task(s) {String.Join(", ", failed.Select(f => f.Item1))}";
            return response.Length > TaskHelpers.CharacterLimit ? "❌ Failed to logged your task(s)" : response;
        }

        if (logged.Count > 0 && failed.Count > 0)
        {
            string response = $"Task List Update ♡ → Task(s) logged: {String.Join(" | ", logged)} | Failed: {String.Join(", ", failed.Select(f => f.Item1))}";
            return response.Length > TaskHelpers.CharacterLimit ? "Task List Update ♡ → some tasks were logged successfully but some failed :p" : response;
        }

        return "No tasks to log";
    }

    public static string BuildRemoveMessage(List<string> tasksRemoved, List<string> tasksFailedToRemove, bool allTasks)
    {
        if (allTasks)
        {
            return "Tasklist Update 🚮 → All your tasks have been removed!";
        }

        if (tasksFailedToRemove.Count == 0 && tasksRemoved.Count == 1)
        {
            return $"Tasklist Update 🚮 → Task '{tasksRemoved[0]}' has been deleted successfully!";
        }

        if (tasksFailedToRemove.Count == 0)
        {
            return "Tasklist Update 🚮 → Tasks removed successfully!";
        }

        return $"Failed to remove: {String.Join(", ", tasksFailedToRemove)}";
    }

    public static string BuildCompletedMessage(List<string> tasksCompleted, List<string> tasksFailedToComplete, bool allTasks)
    {
        if (allTasks)
        {
            return "Task list updated! 🎉 Good job on completing all your tasks!";
        }

        if (tasksFailedToComplete.Count == 0 && tasksCompleted.Count == 1)
        {
            return $"Task list updated! 🎉 Good job on finishing the task '{tasksCompleted[0]}'!";
        }

        if (tasksFailedToComplete.Count == 0)
        {
            return "Task list updated! 🎉 Good job on finishing your tasks!";
        }

        return $"❌ Failed to complete: {String.Join(", ", tasksFailedToComplete)}";
    }

    public static string BuildUndoneMessage(List<string> tasksIncomplete, List<string> tasksFailedToComplete)
    {
        if (tasksFailedToComplete.Count == 0 && tasksIncomplete.Count == 1)
        {
            return $"Task list updated 📝 — Task {tasksIncomplete[0]} marked as incomplete!";
        }

        if (tasksFailedToComplete.Count == 0)
        {
            return "Task list updated 📝 — Tasks marked as incomplete";
        }

        return $"❌ Failed to mark as incomplete: {String.Join(", ", tasksFailedToComplete)}";
    }
}

#endregion
#region Task Operations
public class TaskOperations
{
    private Dictionary<string, UserData> taskData;
    private readonly Action<object, string> broadcast;
    private readonly Func<string> getKey;
    private readonly Func<string, string> getKeyByUsername;
    private readonly Func<string> getUsername;
    public TaskOperations(Dictionary<string, UserData> taskData, Action<object, string> broadcast, Func<string> getKey, Func<string, string> getKeyByUsername, Func<string> getUsername)
    {
        this.taskData = taskData;
        this.broadcast = broadcast;
        this.getKey = getKey;
        this.getKeyByUsername = getKeyByUsername;
        this.getUsername = getUsername;
    }

    public void SetTaskData(Dictionary<string, UserData> data) => this.taskData = data;
    public Dictionary<string, UserData> GetTaskData() => this.taskData;
    public List<Task> ListUserTasks(string userKey = null)
    {
        var emptyList = new List<Task>();
        if (taskData == null || taskData.Count == 0)
            return emptyList;
        string key = userKey ?? getKey();
        if (!taskData.TryGetValue(key, out var userData))
            return emptyList;
        return userData.Tasks.Count == 0 ? emptyList : taskData[key].Tasks;
    }

    public int GetFocusedTask()
    {
        var userTasks = ListUserTasks();
        var incompleteTasks = userTasks.Where(t => !t.Completed);
        if (incompleteTasks.Count() == 1)
        {
            return userTasks.FindIndex(t => !t.Completed);
        }

        return userTasks.FindIndex(t => t.Focused);
    }

    public Response<(int, string)> AddTask(string taskName, bool completed = false, bool focused = false)
    {
        taskName = taskName.Trim();
        if (string.IsNullOrEmpty(taskName))
            return new Response<(int, string)>(false, default, "❌ Error: Task cannot be empty");
        if (int.TryParse(taskName, out _))
            return new Response<(int, string)>(false, default, $"❌ Error: Task cannot be a number");
        if (taskName.Equals("all", StringComparison.OrdinalIgnoreCase))
            return new Response<(int, string)>(false, default, $"❌ Error: 'All' is a reserved keyword");
        string key = getKey();
        if (!taskData.ContainsKey(key))
        {
            string username = getUsername();
            taskData.Add(key, new UserData(new List<Task>(), username));
        }

        if (taskData[key].Tasks.Any(t => t.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase) && !t.Completed))
            return new Response<(int, string)>(false, default, $"❌ Error: Task '{taskName}' already exists");
        taskData[key].Username = getUsername();
        taskData[key].Tasks.Add(new Task(taskName, completed, focused));
        if (completed)
        {
            taskData[key].TotalCompletedCount++;
        }

        int newIndex = taskData[key].Tasks.Count - 1;
        broadcast(new { mode = "add", task = taskName, completed = completed, focused = focused }, null);
        return new Response<(int, string)>(true, (newIndex, taskName), null);
    }

    public Response<(string, string)> EditTask(int index, string newTask)
    {
        var userTasks = ListUserTasks();
        if (userTasks.Count == 0)
            return new Response<(string, string)>(false, default, "❌ Error 404: Tasks not found");
        if (index >= userTasks.Count || index < 0)
            return new Response<(string, string)>(false, default, "❌ Error: Invalid task number");
        bool taskAlreadyExists = userTasks.FindIndex(t => t.Name.Equals(newTask, StringComparison.OrdinalIgnoreCase)) > -1;
        if (taskAlreadyExists)
            return new Response<(string, string)>(false, default, "❌ Error: Task already exists");
        string oldName = userTasks[index].Name;
        userTasks[index].Name = newTask;
        SaveIntoTasks(userTasks);
        broadcast(new { mode = "edit", index = index, task = newTask }, null);
        return new Response<(string, string)>(true, (oldName, newTask), null);
    }

    public Response<(int, string)> FocusOnTask(string rawInput)
    {
        bool containsSeparators = rawInput.IndexOfAny(TaskHelpers.Separators) >= 0;
        if (containsSeparators)
            return new Response<(int, string)>(false, default, "❌ Cannot focus on multiple tasks");
        var tasks = ListUserTasks();
        int indexByName = tasks.FindIndex(t => t.Name.Equals(rawInput, StringComparison.OrdinalIgnoreCase));
        if (int.TryParse(rawInput, out int n))
        {
            n = n - 1;
            if (n < 0 || n >= tasks.Count)
                return new Response<(int, string)>(false, default, "❌ Error: Index out of range.");
            if (tasks[n].Completed)
                return new Response<(int, string)>(false, default, "❌ Error: Cannot focus on completed task.");
            UnfocusAll(tasks);
            tasks[n].Focused = true;
            SaveIntoTasks(tasks);
            broadcast(new { mode = "focus", index = n }, null);
            return new Response<(int, string)>(true, (n, tasks[n].Name), null);
        }
        else if (indexByName > -1)
        {
            n = indexByName;
            if (tasks[n].Completed)
                return new Response<(int, string)>(false, default, "❌ Cannot focus on completed task.");
            UnfocusAll(tasks);
            tasks[n].Focused = true;
            SaveIntoTasks(tasks);
            broadcast(new { mode = "focus", index = n }, null);
            return new Response<(int, string)>(true, (n, tasks[n].Name), null);
        }
        else
        {
            var response = AddTask(rawInput, false, true);
            if (response.Success)
            {
                broadcast(new { mode = "focus", index = response.Data.Item1 }, null);
                return new Response<(int, string)>(true, response.Data, null);
            }

            return new Response<(int, string)>(false, default, response.ErrorMsg);
        }
    }

    public void Unfocus()
    {
        var tasks = ListUserTasks();
        UnfocusAll(tasks);
        SaveIntoTasks(tasks);
    }

    private void UnfocusAll(List<Task> tasks)
    {
        for (int i = 0; i < tasks.Count; i++)
            tasks[i].Focused = false;
    }



    public void SaveIntoTasks(List<Task> tasks)
    {
        string key = getKey();
        taskData[key].Username = getUsername();
        taskData[key].Tasks = tasks;
    }

    public void Cleanup(bool userOnly = false)
    {
        if (userOnly)
        {
            var userTasks = ListUserTasks();
            if (userTasks.Count == 0)
            {
                string userKey = getKey();
                taskData.Remove(userKey);
            }
        }
        else
        {
            var keysToRemove = taskData.Where(item => item.Value.Tasks.Count == 0).Select(item => item.Key).ToList();
            foreach (string key in keysToRemove)
                taskData.Remove(key);
        }
    }

    public void RemoveUser(string key)
    {
        taskData.Remove(key);
    }

    public void ClearAllTasks()
    {
        foreach (var item in taskData.Keys.ToList())
            taskData[item].Tasks = new List<Task>();
    }

    public void ClearCompletedTasks()
    {
        foreach (var item in taskData.Keys.ToList())
            taskData[item].Tasks = taskData[item].Tasks.Where(t => !t.Completed).ToList();
    }

    public void ClearUserCompletedTasks(string key)
    {
        taskData[key].Tasks = taskData[key].Tasks.Where(t => !t.Completed).ToList();
    }

    public void FilterToStreamers(List<string> streamerUsernames)
    {
        taskData = taskData.Where(kvp => streamerUsernames.Any(s => string.Equals(s, kvp.Value.Username, StringComparison.OrdinalIgnoreCase))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}

#endregion
#region Main Command Handler
public class CPHInline
{
    private Dictionary<string, UserData> taskData = new Dictionary<string, UserData>();
    private TaskOperations operations;
    public void Init()
    {
        string taskDataString = CPH.GetGlobalVar<string>("rython-task-bot", true) ?? "{}";
        taskData = JsonConvert.DeserializeObject<Dictionary<string, UserData>>(taskDataString);
        operations = new TaskOperations(taskData, Broadcast, () => GetKey(), (username) => GetKey(username), () => GetUsername());
        operations.Cleanup(false);
        SaveTasks();
    }

    #region Platform Helpers
    private void Broadcast(object body, string key)
    {
        string json = JsonConvert.SerializeObject(new { source = "rython-task-bot", id = key ?? GetKey(), body = body, username = GetUsername() });
        CPH.WebsocketBroadcastJson(json);
    }

    private List<string> GetStreamerUsernames()
    {
        TwitchUserInfo twitchInfo = CPH.TwitchGetBroadcaster();
        var youtubeInfo = CPH.YouTubeGetBroadcaster();
        return new List<string>
        {
            twitchInfo.UserName,
            youtubeInfo.UserName
        };
    }

    private string GetUsername(string key = null)
    {
        if (String.IsNullOrEmpty(key))
        {
            CPH.TryGetArg("user", out string username);
            return username;
        }

        return taskData[key].Username;
    }

    private string GetKey(string username = null)
    {
        if (!String.IsNullOrEmpty(username))
        {
            if (username[0] == '@')
                username = username.Substring(1);
            foreach (var item in taskData)
            {
                if (item.Value.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                    return item.Key;
            }

            return "";
        }

        CPH.TryGetArg("userType", out string platform);
        CPH.TryGetArg("userId", out string userId);
        return $"{platform}-{userId}";
    }

    private void IncrementDoneCount(int count = 1)
    {
        string key = GetKey();
        taskData[key].TotalCompletedCount += count;
    }

    private void SaveTasks()
    {
        taskData = operations.GetTaskData();
        string taskDataString = JsonConvert.SerializeObject(taskData);
        CPH.SetGlobalVar("rython-task-bot", taskDataString, true);
    }

    private int GetCount(string? key = null)
    {
        if (String.IsNullOrEmpty(key))
        {
            key = GetKey();
        }

        return taskData[key].TotalCompletedCount;
    }

    private void Respond(string message)
    {
        CPH.TryGetArg("userType", out string platform);
        switch (platform)
        {
            case "twitch":
                CPH.TryGetArg("msgId", out string twitchMsgId);
                CPH.TwitchReplyToMessage(message, twitchMsgId);
                break;
            case "youtube":
                CPH.TryGetArg("user", out string YTUser);
                CPH.SendYouTubeMessage($"@{YTUser} {message}");
                break;
            case "kick":
                CPH.TryGetArg("user", out string kickUser);
                CPH.TryGetArg("msgId", out string kickMsgId);
                CPH.KickReplyToMessage($"@{kickUser} {message}", kickMsgId);
                break;
            default:
                break;
        }
    }

    #endregion
    #region Commands
    public bool HelpCommand()
    {
        Respond("📝 Rython Task Bot Commands: !task !edit !remove !done. For mods, you can do !adel @user. More commmands here: https://github.com/liyunze-coding/rython-task-bot-v2#usage");
        return true;
    }

    public bool AddCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        var taskStrings = TaskHelpers.SplitTasks(rawInput);
        if (taskStrings.Count == 0)
        {
            Respond("❌ No tasks provided");
            return false;
        }

        var added = new List<string>();
        var failed = new List<(string, string)>();
        foreach (string taskString in taskStrings)
        {
            var response = operations.AddTask(taskString);
            if (response.Success)
                added.Add($"{response.Data.Item1 + 1}. {taskString.Trim()}");
            else
                failed.Add((taskString, response.ErrorMsg));
        }

        if (added.Count > 0)
            SaveTasks();
        Respond(MessageBuilder.BuildAddResponseMessage(added, failed));
        return true;
    }

    public bool LogCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        var taskStrings = TaskHelpers.SplitTasks(rawInput);
        if (taskStrings.Count == 0)
        {
            Respond("❌ Error: No tasks provided");
            return false;
        }

        var logged = new List<string>();
        var failed = new List<(string, string)>();
        foreach (string taskString in taskStrings)
        {
            var response = operations.AddTask(taskString, true, false);
            if (response.Success)
                logged.Add($"{response.Data.Item1 + 1}. {taskString.Trim()}");
            else
                failed.Add((taskString, response.ErrorMsg));
        }

        if (logged.Count > 0)
            SaveTasks();
        Respond(MessageBuilder.BuildLogResponseMessage(logged, failed));
        return true;
    }

    public bool FocusCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        var response = operations.FocusOnTask(rawInput);
        if (!response.Success)
            Respond(response.ErrorMsg);
        else
            Respond($"✅ Current focused task: {response.Data.Item1 + 1}. {response.Data.Item2}");
        SaveTasks();
        return true;
    }

    public bool FocusedCommand()
    {
        var userTasks = operations.ListUserTasks();
        int focusedTaskIndex = operations.GetFocusedTask();
        if (focusedTaskIndex == -1)
        {
            Respond("❌ You do not have a focused task.");
            return true;
        }

        Respond($"✅ Current focused task: {focusedTaskIndex + 1}. {userTasks[focusedTaskIndex].Name}");
        return true;
    }

    public bool NextCommand()
    {
        var userTasks = operations.ListUserTasks();
        int focusedTaskIndex = operations.GetFocusedTask();
        if (focusedTaskIndex == -1)
        {
            Respond("❌ Can't use !next command, select a task to complete using !done and/or add another task");
            return false;
        }

        CPH.TryGetArg("rawInput", out string rawInput);
        var focusResponse = operations.FocusOnTask(rawInput);
        if (!focusResponse.Success)
        {
            Respond(focusResponse.ErrorMsg);
            return false;
        }

        string completedTaskName = userTasks[focusedTaskIndex].Name;
        userTasks[focusedTaskIndex].Completed = true;
        userTasks[focusedTaskIndex].Focused = false;
        Broadcast(new { mode = "done", index = focusedTaskIndex }, null);
        operations.SaveIntoTasks(userTasks);
        SaveTasks();
        Respond($"✅ Good job on completing '{completedTaskName}'! Moving onto '({focusResponse.Data.Item1 + 1}) {focusResponse.Data.Item2}'");
        return true;
    }

    public bool EditCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        var editData = TaskHelpers.ParseEditInput(rawInput, operations.GetFocusedTask());
        if (!editData.Success)
        {
            Respond(editData.ErrorMsg);
            return false;
        }

        var result = operations.EditTask(editData.Data.Item1, editData.Data.Item2);
        Respond(result.Success ? $"📝 Task '{result.Data.Item1}' has been edited to '{result.Data.Item2}'" : result.ErrorMsg);
        SaveTasks();
        return true;
    }

    public bool CheckCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        rawInput = rawInput.Trim();
        string key = null;
        bool someoneElse = false;
        if (!String.IsNullOrWhiteSpace(rawInput))
        {
            key = GetKey(rawInput);
            if (key == "")
            {
                Respond("❌ User not found");
                return false;
            }

            someoneElse = true;
        }

        var userTasks = operations.ListUserTasks(key);
        if (userTasks.Count == 0)
        {
            Respond("❌ 404 Tasks not found");
            return false;
        }

        var incompleteTasks = userTasks.Select((t, index) => new { t, index }).Where(x => !x.t.Completed);
        string message = String.Join(" | ", incompleteTasks.Select(x => $"{x.index + 1}. {(x.t.Focused ? "(ongoing) " : "")}{x.t.Name}"));
        message = someoneElse ? $"{GetUsername(key)}'s task(s) ({incompleteTasks.Count()}): {message}" : $"Task(s) pending ({incompleteTasks.Count()}): {message}";
        Respond(message);
        return true;
    }

    public bool CompletedCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        rawInput = rawInput.Trim();
        string key = null;
        bool someoneElse = false;
        if (!String.IsNullOrWhiteSpace(rawInput))
        {
            key = GetKey(rawInput);
            if (key == "")
            {
                Respond("❌ User not found");
                return false;
            }

            someoneElse = true;
        }

        var userTasks = operations.ListUserTasks(key);
        if (userTasks.Count == 0)
        {
            Respond("❌ 404 Tasks not found");
            return false;
        }

        var completedTasks = userTasks.Select((t, index) => new { t, index }).Where(x => x.t.Completed);
        string message = String.Join(" | ", completedTasks.Select(x => $"{x.index + 1}. {x.t.Name}"));
        message = someoneElse ? $"{GetUsername(key)}'s tasks ({completedTasks.Count()}): {message}" : $"Completed {completedTasks.Count()} tasks: {message}";
        Respond(message);
        return true;
    }

    public bool ListCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        rawInput = rawInput.Trim();
        string separator = "; ";
        if (rawInput.Length == 1 && ";,|".Contains(rawInput))
            separator = $"{rawInput} ";
        var userTasks = operations.ListUserTasks();
        string message = String.Join(separator, userTasks.Where(t => !t.Completed).Select(t => t.Name));
        Respond(message);
        return true;
    }

    public bool RemoveCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        var userTasks = operations.ListUserTasks();
        if (userTasks.Count == 0)
        {
            Respond("❌ Error 404: Tasks not found");
            return false;
        }

        var tasksToBeRemoved = TaskHelpers.ParseTasksInput(rawInput, operations.GetFocusedTask);
        if (tasksToBeRemoved.Count == 0)
        {
            Respond("❌ Error: Invalid input");
            return false;
        }

        bool allTasks = tasksToBeRemoved.Count == 1 && tasksToBeRemoved[0] == "all";
        var tasksRemoved = new List<string>();
        var tasksFailedToRemove = new List<string>();
        var taskIndices = new List<int>();
        if (!allTasks)
        {
            foreach (string task in tasksToBeRemoved)
            {
                int index = TaskHelpers.GetTaskIndex(userTasks, task);
                if (index > -1 && !taskIndices.Contains(index))
                {
                    taskIndices.Add(index);
                    tasksRemoved.Add(userTasks[index].Name);
                }
                else
                {
                    tasksFailedToRemove.Add(task);
                }
            }

            if (taskIndices.Count == 0)
            {
                Respond($"❌ Failed to remove task(s): {String.Join(", ", tasksFailedToRemove)}");
                return false;
            }
        }
        else
        {
            taskIndices = Enumerable.Range(0, userTasks.Count).ToList();
        }

        foreach (int i in taskIndices.OrderByDescending(n => n))
        {
            userTasks.RemoveAt(i);
            Broadcast(new { mode = "remove", index = i }, null);
        }

        operations.SaveIntoTasks(userTasks);
        operations.Cleanup(true);
        SaveTasks();
        Respond(MessageBuilder.BuildRemoveMessage(tasksRemoved, tasksFailedToRemove, allTasks));
        return true;
    }

    public bool AdminDelete()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        string[] spaceSeparated = rawInput.Split(new[] { ' ' }, 2);
        if (spaceSeparated.Length == 0)
            return false;
        string user = spaceSeparated[0];
        string key = GetKey(user);
        if (key == "")
        {
            Respond("❌ Error: Unable to find user with that username on the task list");
            return false;
        }

        operations.RemoveUser(key);
        SaveTasks();
        Respond("All of the user's tasks have been deleted");
        Broadcast(new { mode = "admindelete", id = key }, null);
        return true;
    }

    public bool DoneCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        rawInput = rawInput.Trim();
        var userTasks = operations.ListUserTasks();
        if (userTasks.Count == 0)
        {
            Respond("❌ Error 404: Tasks not found");
            return false;
        }

        var tasksToBeCompleted = TaskHelpers.ParseTasksInput(rawInput, operations.GetFocusedTask);
        if (tasksToBeCompleted.Count == 0)
        {
            Respond("❌ Error: Invalid input");
            return false;
        }

        bool allTasks = tasksToBeCompleted.Count == 1 && tasksToBeCompleted[0] == "all";
        var tasksCompleted = new List<string>();
        var tasksFailedToComplete = new List<string>();
        var taskIndices = new List<int>();
        if (!allTasks)
        {
            foreach (string task in tasksToBeCompleted)
            {
                int index = TaskHelpers.GetTaskIndex(userTasks, task);
                if (index > -1 && !taskIndices.Contains(index) && !userTasks[index].Completed)
                {
                    taskIndices.Add(index);
                    tasksCompleted.Add(userTasks[index].Name);
                }
                else
                {
                    tasksFailedToComplete.Add(task);
                }
            }
        }
        else
        {
            taskIndices = Enumerable.Range(0, userTasks.Count).ToList();
        }

        if (taskIndices.Count == 0)
        {
            Respond($"❌ Failed to complete task(s): {String.Join(", ", tasksFailedToComplete)}");
            return false;
        }

        foreach (int i in taskIndices)
        {
            userTasks[i].Completed = true;
            userTasks[i].Focused = false;
            Broadcast(new { mode = "done", index = i }, null);
        }

        IncrementDoneCount(taskIndices.Count);
        operations.SaveIntoTasks(userTasks);
        SaveTasks();
        Respond(MessageBuilder.BuildCompletedMessage(tasksCompleted, tasksFailedToComplete, allTasks));
        return true;
    }

    public bool UnfocusCommand()
    {
        operations.Unfocus();
        SaveTasks();
        Respond("📝 Task have been unfocused!");
        Broadcast(new { mode = "unfocus" }, null);
        return true;
    }

    public bool UndoneCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        var userTasks = operations.ListUserTasks();
        if (userTasks.Count == 0)
        {
            Respond("❌ Error 404: Tasks not found");
            return false;
        }

        var tasksToBeCompleted = TaskHelpers.ParseUndoneInput(rawInput, userTasks);
        if (tasksToBeCompleted.Count == 0)
        {
            Respond("❌ Error: Invalid input");
            return false;
        }

        var tasksCompleted = new List<string>();
        var tasksFailedToComplete = new List<string>();
        var taskIndices = new List<int>();
        foreach (string task in tasksToBeCompleted)
        {
            int index = TaskHelpers.GetTaskIndex(userTasks, task);
            if (index > -1 && !taskIndices.Contains(index) && userTasks[index].Completed)
            {
                taskIndices.Add(index);
                tasksCompleted.Add(userTasks[index].Name);
            }
            else
            {
                tasksFailedToComplete.Add(task);
            }
        }

        if (taskIndices.Count == 0)
        {
            Respond($"❌ Failed to un-done task(s): {String.Join(", ", tasksFailedToComplete)}");
            return false;
        }

        foreach (int i in taskIndices)
        {
            userTasks[i].Completed = false;
            userTasks[i].Focused = false;
            Broadcast(new { mode = "undone", index = i }, null);
        }

        operations.SaveIntoTasks(userTasks);
        SaveTasks();
        Respond(MessageBuilder.BuildUndoneMessage(tasksCompleted, tasksFailedToComplete));
        return true;
    }

    public bool ClearAllCommand()
    {
        operations.ClearAllTasks();
        operations.Cleanup(false);
        SaveTasks();
        Broadcast(new { mode = "clearall" }, null);
        Respond("📝 All tasks have been cleared!");
        return true;
    }

    public bool ClearMyDoneCommand()
    {
        string key = GetKey();
        operations.ClearUserCompletedTasks(key);
        operations.Cleanup(false);
        SaveTasks();
        Broadcast(new { mode = "clearmydone" }, null);
        Respond("📝 All of your completed tasks have been cleared!");
        return true;
    }

    public bool ClearDoneCommand()
    {
        operations.ClearCompletedTasks();
        operations.Cleanup(false);
        SaveTasks();
        Broadcast(new { mode = "cleardone" }, null);
        Respond("📝 All completed tasks have been cleared!");
        return true;
    }

    public bool ClearNotStreamerCommand()
    {
        operations.FilterToStreamers(GetStreamerUsernames());
        operations.Cleanup(false);
        SaveTasks();
        Broadcast(new { mode = "clearns" }, null);
        Respond("📝 All tasks (excluding the streamer's) have been cleared!");
        return true;
    }

    public bool CountCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        rawInput = rawInput.Trim();
        string key = null;
        bool someoneElse = false;
        if (!String.IsNullOrWhiteSpace(rawInput))
        {
            key = GetKey(rawInput);
            if (key == "")
            {
                Respond("❌ User not found");
                return false;
            }

            someoneElse = true;
        }

        int completedCount = GetCount(key);
        if (someoneElse)
        {
            string username = GetUsername(key);
            Respond($"{username} has completed {completedCount} task(s) so far!");
        }
        else
        {
            Respond($"You have completed {completedCount} task(s) so far!");
        }
        return true;
    }
    #endregion
}
#endregion
