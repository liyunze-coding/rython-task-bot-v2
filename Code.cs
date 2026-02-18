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

/*
Todo

delete based on days

{
	platform-userId: {
        username: string
        tasks: [
            name
            completed?
            timestamp added
            timestamp completed
        ],
        totalIncompleteCount: uint
        totalCompletedCount: uint
    }
    task-data
}
*/
class Task
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

class UserData
{
    public string username;
    public List<Task> tasks;
    public uint totalIncompleteCount;
    public uint totalCompletedCount;
    public UserData(List<Task> tasks, string username)
    {
        this.username = username;
        this.tasks = tasks;
        this.totalIncompleteCount = 0;
        this.totalCompletedCount = 0;
    }
}

public class Response
{
    public bool Success { get; set; }
    public string ErrorMsg { get; set; }

    public Response(bool success, string errorMsg = null)
    {
        this.Success = success;
        this.ErrorMsg = errorMsg;
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

public class CPHInline
{
    private int characterLimit = 450;
    private Dictionary<string, UserData> taskData = new Dictionary<string, UserData>();
    public char[] separators =
    {
        '|',
        ',',
        ';'
    };
    public void Init()
    {
        // task list
        string taskDataString = CPH.GetGlobalVar<string>("rython-task-bot", true);
        Dictionary<string, UserData> taskListData = JsonConvert.DeserializeObject<Dictionary<string, UserData>>(taskDataString);
        this.taskData = taskListData;
        Cleanup(false);
    }

    // Send changes to frontend
    private void Broadcast<T>(T t, string? key = null)
    {
        // INDEX MUST BE TRUE INDEX
        string json = JsonConvert.SerializeObject(new { source = "rython-task-bot", id = key ?? GetKey(), body = t, username = GetUsername() });
        CPH.WebsocketBroadcastJson(json);
    }

    private List<string> GetStreamerUsernames()
    {
        TwitchUserInfo twitchInfo = CPH.TwitchGetBroadcaster();
        var youtubeInfo = CPH.YouTubeGetBroadcaster();
        List<string> usernames = new()
        {
            twitchInfo.UserName,
            youtubeInfo.UserName
        };
        return usernames;
    }

    private string GetUsername(string? key = null)
    {
        if (String.IsNullOrEmpty(key))
        {
            CPH.TryGetArg("user", out string username);
            return username;
        }
        else
        {
            return this.taskData[key].username;
        }

        return "";
    }

    private void SaveTasks()
    {
        string taskDataString = JsonConvert.SerializeObject(this.taskData);
        CPH.SetGlobalVar("rython-task-bot", taskDataString, true);
    }

    private void SaveIntoTasks(List<Task> tasks)
    {
        string key = GetKey();
        this.taskData[key].username = GetUsername();
        this.taskData[key].tasks = tasks;
        SaveTasks();
    }

    private void Respond(string message)
    {
        CPH.TryGetArg("userType", out string platform);
        switch (platform)
        {
            case "twitch":
                CPH.TryGetArg("msgId", out string msgId);
                CPH.TwitchReplyToMessage(message, msgId);
                break;
            case "youtube":
                CPH.TryGetArg("user", out string YTUser);
                CPH.SendYouTubeMessage($"@{YTUser} {message}");
                break;
            default:
                // idk i don't stream elsewhere
                break;
        }
    }

    private List<string> SplitTasks(string tasks)
    {
        return tasks.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
    }

    private string GetKey(string? username = null)
    {
        if (!String.IsNullOrEmpty(username))
        {
            if (username[0] == '@')
            {
                username = username.Substring(1);
            }

            // dig through task data, find username, case insensitive
            foreach (var item in this.taskData)
            {
                if (item.Value.username.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Key;
                }
            }

            return "";
        }
        else
        {
            CPH.TryGetArg("userType", out string platform);
            CPH.TryGetArg("userId", out string userId);
            string key = $"{platform}-{userId}";
            return key;
        }
    }

    // ADD COMMAND
    // <(true index, task name)>
    private Response<(int, string)> AddTask(string taskName, bool completed = false, bool focused = false)
    {
        taskName = taskName.Trim();
        // Validation
        if (string.IsNullOrEmpty(taskName))
            return new Response<(int, string)>(false, default, "Task cannot be empty");
        if (int.TryParse(taskName, out _))
            return new Response<(int, string)>(false, default, $"'{taskName}' cannot be a number");
        // Ensure user data exists
        string key = GetKey();
        if (!this.taskData.ContainsKey(key))
        {
            string username = GetUsername();
            this.taskData.Add(key, new UserData(new List<Task>(), username));
        }

        // Check for duplicate incomplete task
        if (this.taskData[key].tasks.Any(t => t.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase) && !t.Completed))
            return new Response<(int, string)>(false, default, $"Error: Task '{taskName}' already exists");
        // Add task
        this.taskData[key].username = GetUsername();
        this.taskData[key].tasks.Add(new Task(taskName, completed, focused));
        int newIndex = this.taskData[key].tasks.Count - 1; // 1-based index for display
        Broadcast(new { mode = "add", task = taskName, completed = completed, focused = focused });
        return new Response<(int, string)>(true, (newIndex, taskName), null);
    }

    public bool AddCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        List<string> taskStrings = SplitTasks(rawInput);
        if (taskStrings.Count == 0)
        {
            Respond("No tasks provided");
            return false;
        }

        List<string> added = new();
        List<(string, string)> failed = new();
        foreach (string taskString in taskStrings)
        {
            var response = AddTask(taskString);
            if (response.Success)
                added.Add($"{response.Data.Item1 + 1}. {taskString.Trim()}");
            else
                failed.Add((taskString, response.ErrorMsg));
        }

        // Save once after all tasks are processed
        if (added.Count > 0)
        {
            SaveTasks();
        }

        // Build response
        Respond(BuildAddResponseMessage(added, failed));
        return true;
    }

    public bool LogCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        List<string> taskStrings = SplitTasks(rawInput);
        if (taskStrings.Count == 0)
        {
            Respond("Error: No tasks provided");
            return false;
        }

        List<string> logged = new();
        List<(string, string)> failed = new();
        foreach (string taskString in taskStrings)
        {
            var response = AddTask(taskString, true, false);
            if (response.Success)
                logged.Add($"{response.Data.Item1 + 1}. {taskString.Trim()}");
            else
                failed.Add((taskString, response.ErrorMsg));
        }

        // Save once after all tasks are processed
        if (logged.Count > 0)
        {
            SaveTasks();
        }

        // Build response
        Respond(BuildLogResponseMessage(logged, failed));
        return true;
    }

    private string BuildAddResponseMessage(List<string> added, List<(string, string)> failed)
    {
        string response = "No tasks to add";
        if (added.Count > 0 && failed.Count == 0)
        {
            response = $"Added: {String.Join(" | ", added)}";
            if (response.Length > characterLimit)
            {
                response = "All the tasks have been added!";
            }
        }
        else if (added.Count == 0 && failed.Count == 1)
        {
            response = failed[0].Item2; // Single error, show the reason
        }
        else if (added.Count == 0 && failed.Count > 1)
        {
            response = $"Failed: {String.Join(", ", failed.Select(f => f.Item1))}";
            if (response.Length > characterLimit)
            {
                response = "None of the tasks successfully added";
            }
        }
        else if (added.Count > 0 && failed.Count > 0)
        {
            response = $"Added: {String.Join(" | ", added)} | Failed: {String.Join(", ", failed.Select(f => f.Item1))}";
            if (response.Length > characterLimit)
            {
                response = "Some tasks successful but some failed :p";
            }
        }

        return response;
    }

    private string BuildLogResponseMessage(List<string> added, List<(string, string)> failed)
    {
        string response = "No tasks to add";
        if (added.Count > 0 && failed.Count == 0)
        {
            response = $"Added: {String.Join(" | ", added)}";
            if (response.Length > characterLimit)
            {
                response = "All the tasks have been added!";
            }
        }
        else if (added.Count == 0 && failed.Count == 1)
        {
            response = failed[0].Item2; // Single error, show the reason
        }
        else if (added.Count == 0 && failed.Count > 1)
        {
            response = $"Failed: {String.Join(", ", failed.Select(f => f.Item1))}";
            if (response.Length > characterLimit)
            {
                response = "None of the tasks successfully added";
            }
        }
        else if (added.Count > 0 && failed.Count > 0)
        {
            response = $"Added: {String.Join(" | ", added)} | Failed: {String.Join(", ", failed.Select(f => f.Item1))}";
            if (response.Length > characterLimit)
            {
                response = "Some tasks successful but some failed :p";
            }
        }

        return response;
    }

    // focus on existing task, or add a new task and focus on it
    private Response<(int, string)> FocusOnTask(string rawInput)
    {
        // make sure there's no separators
        bool containsSeparators = rawInput.IndexOfAny(separators) >= 0;
        if (containsSeparators)
        {
            return new(false, default, "Cannot focus on multiple tasks");
        }

        List<Task> tasks = ListUserTasks();
        int IndexByName = tasks.FindIndex(t => t.Name.Equals(rawInput, StringComparison.OrdinalIgnoreCase));
        // existing task by name, new task, or index
        if (int.TryParse(rawInput, out int n))
        {
            n = n - 1; // true index
            // IT'S A NUMBER
            if (n < 0 || n >= tasks.Count)
            {
                return new(false, default, "Index out of range.");
            }
            else if (tasks[n].Completed)
            {
                // task already completed, cannot focus on it
                return new(false, default, "Cannot focus on completed task.");
            }
            else
            {
                // unfocus on every single task
                // then focus on that task
                for (int i = 0; i < tasks.Count; i++)
                {
                    tasks[i].Focused = false;
                }

                tasks[n].Focused = true;
                SaveIntoTasks(tasks);
                Broadcast(new { mode = "focus", index = n });
                return new(true, (n, tasks[n].Name), null);
            }
        }
        else if (IndexByName > -1)
        {
            // it's an existing task
            n = IndexByName;
            if (tasks[n].Completed)
            {
                // task already completed, cannot focus on it
                return new(false, default, "Cannot focus on completed task.");
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].Focused = false;
            }

            tasks[n].Focused = true;
            SaveIntoTasks(tasks);
            Broadcast(new { mode = "focus", index = n });
            return new(true, (n, tasks[n].Name), null);
        }
        else
        {
            // new task
            var response = AddTask(rawInput, false, true);
            if (response.Success)
            {
                Broadcast(new { mode = "focus", index = response.Data.Item1 });
                return new(true, (response.Data.Item1, response.Data.Item2), null);
            }
            else
            {
                return new(false, default, $"{response.ErrorMsg}");
            }
        }
    }

    // FOCUS COMMAND
    // 1: focus on existing task by name (check if already focused)
    // 2: focus on existing task by index (must be valid index that isn't already focused)
    // 3: adding a new task by name
    public bool FocusCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        // make sure there's no separators
        var AddOrSelectResponse = FocusOnTask(rawInput);
        if (!AddOrSelectResponse.Success)
        {
            Respond(AddOrSelectResponse.ErrorMsg);
        }
        else
        {
            Respond($"Current focused task: {AddOrSelectResponse.Data.Item1}. {AddOrSelectResponse.Data.Item2}");
        }

        return true;
    }

    // return true index
    private int GetFocusedTask()
    {
        List<Task> userTasks = ListUserTasks();
        var incompleteTasks = userTasks.Where(t => !t.Completed);
        if (incompleteTasks.Count() == 1)
        {
            return userTasks.FindIndex(t => !t.Completed);
        }

        return userTasks.FindIndex(t => t.Focused);
    }

    // NEXT COMMAND
    // get focused task -> mark it as complete -> focus on existing/new task
    public bool NextCommand()
    {
        List<Task> userTasks = ListUserTasks();
        int focusedTaskIndex = GetFocusedTask();
        if (focusedTaskIndex == -1)
        {
            Respond("Can't use !next command, select a task to complete using !done and/or add another task");
            return false;
        }

        // Focus on the task
        CPH.TryGetArg("rawInput", out string rawInput);
        var FocusOnTaskResponse = FocusOnTask(rawInput);
        if (!FocusOnTaskResponse.Success)
        {
            Respond(FocusOnTaskResponse.ErrorMsg);
            return false;
        }

        int newTaskIndex = FocusOnTaskResponse.Data.Item1;
        string taskString = FocusOnTaskResponse.Data.Item2;
        // mark it as complete
        string completedTaskName = userTasks[focusedTaskIndex].Name;
        userTasks[focusedTaskIndex].Completed = true;
        userTasks[focusedTaskIndex].Focused = false;
        Broadcast(new { mode = "done", index = focusedTaskIndex });
        SaveIntoTasks(userTasks);
        Respond($"Completed '{completedTaskName}'! Moving onto '({newTaskIndex + 1}) {taskString}'");
        return true;
    }

    // EDIT COMMAND
    public bool EditCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        // parse check input
        var EditData = ParseEditInput(rawInput);
        if (!EditData.Success)
        {
            Respond(EditData.ErrorMsg);
        }

        int taskIndex = EditData.Data.Item1;
        string newTask = EditData.Data.Item2;
        var result = EditTask(taskIndex, newTask);
        if (!result.Success)
        {
            Respond(result.ErrorMsg);
        }
        else
        {
            Respond($"Task '{result.Data.Item1}' has been edited to '{result.Data.Item2}'");
        }

        // edit task
        return true;
    }

    private Response<(string, string)> EditTask(int index, string newTask)
    {
        List<Task> userTasks = ListUserTasks();
        // task data doesn't exist
        if (userTasks.Count == 0)
        {
            // Respond("404 Tasks not found");
            return new(false, default, "Error 404: Tasks not found");
        }

        // out of range
        if (index > userTasks.Count || index < 0)
        {
            return new(false, default, "Error: Invalid task number");
        }

        // incomplete task already exists
        bool taskAlreadyExists = userTasks.FindIndex(t => t.Name.Equals(newTask, StringComparison.OrdinalIgnoreCase)) > -1;
        if (taskAlreadyExists)
        {
            return new(false, default, "Error: Task already exists");
        }

        // userTasks
        string oldName = userTasks[index].Name;
        userTasks[index].Name = newTask;
        SaveIntoTasks(userTasks);
        Broadcast(new { mode = "edit", index = index, task = newTask });
        return new(true, (oldName, newTask), null);
    }

    // simple: !edit <number> <task> 
    // Response<(int, string)>
    private Response<(int, string)> ParseEditInput(string rawInput)
    {
        bool validEditInput = true;
        rawInput = rawInput.Trim();
        int focusedTaskIndex = GetFocusedTask();
        bool hasFocusedTask = focusedTaskIndex > -1;
        string[] spaceSeparated = rawInput.Split(new[] { ' ' }, 2);
        if (spaceSeparated.Length < 2)
        {
            validEditInput = false;
            if (!hasFocusedTask)
            {
                return new Response<(int, string)>(false, default, "Error: Not enough arguments, try !edit <number> <new task>");
            }
        }

        string numberString = spaceSeparated[0];
        string newTask = spaceSeparated[1];
        if (!int.TryParse(numberString, out int index))
        {
            validEditInput = false;
            if (!hasFocusedTask)
            {
                return new Response<(int, string)>(false, default, "Error: Not a valid argument, try !edit <number> <new task>");
            }
        }

        if (validEditInput)
        {
            return new Response<(int, string)>(true, (index - 1, newTask), null);
        }
        else
        {
            return new Response<(int, string)>(true, (focusedTaskIndex, rawInput), null);
        }
    }

    // CHECK COMMAND
    private List<Task> ListUserTasks(string? userKey = null)
    {
        List<Task> emptyList = new List<Task>();
        if (this.taskData == null || this.taskData.Count == 0)
        {
            return emptyList; // return empty task list
        }

        // user's data doesn't exist
        string key = userKey ?? GetKey();
        var userData = this.taskData[key];
        if (userData == null)
        {
            return emptyList;
        }

        // 0 tasks
        List<Task> tasks = userData.tasks;
        if (tasks.Count == 0)
        {
            return emptyList;
        }

        return this.taskData[key].tasks;
    }

    public bool CheckCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        rawInput = rawInput.Trim();
        string? key = null;
        bool someoneElse = false;
        if (!String.IsNullOrWhiteSpace(rawInput))
        {
            key = GetKey(rawInput);
            if (key == "")
            {
                Respond("User not found");
                return false;
            }
            else
            {
                someoneElse = true;
            }
        }

        List<Task> userTasks = ListUserTasks(key);
        // task data doesn't exist
        if (userTasks.Count == 0)
        {
            Respond("404 Tasks not found");
            return false;
        }

        // JOIN THE TASKS INTO ONE MESSAGE
        // edit however you like
        string message = String.Join(" | ", userTasks.Select((t, index) => $"{index + 1}. {(t.Focused ? "(ongoing) " : "")}{t.Name}"));
        if (someoneElse)
        {
            string theirUser = GetUsername(key);
            message = $"{theirUser}'s tasks: {message}";
        }
        Respond(message);
        return true;
    }

    public bool ListCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        rawInput = rawInput.Trim();
        string separator = "; ";
        string validSeparators = ";,|";
        if (rawInput.Length == 1 && validSeparators.Contains(rawInput))
        {
            separator = $"{rawInput} ";
        }

        List<Task> userTasks = ListUserTasks();
        string message = String.Join(separator, userTasks.Where((t, index) => !t.Completed).Select(task => task.Name));
        Respond(message);
        return true;
    }

    // Helper method to get task index by number or name
    private int GetTaskIndex(List<Task> tasks, string input)
    {
        if (int.TryParse(input, out int n))
        {
            int index = n - 1; // Convert 1-based to 0-based
            return (index >= 0 && index < tasks.Count) ? index : -1;
        }

        return tasks.FindIndex(t => t.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
    }

    // --- REMOVE TASK ---
    private List<string> ParseTasksInput(string input)
    {
        input = input.Trim();
        var tasksToRemove = new List<string>();
        bool IsSpaceSeparatedInts = Regex.IsMatch(input, @"^\d+(\s+\d+)*$");
        if (IsSpaceSeparatedInts)
        {
            return input.Split(' ').ToList();
        }

        var splittedInput = input.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
        if (splittedInput.Count == 0)
        {
            // get focused task
            int focusedTaskIndex = GetFocusedTask() + 1;
            // return false index
            return new List<string>
            {
                focusedTaskIndex.ToString()
            };
        }
        else
        {
            return splittedInput;
        }
    }

    private string BuildRemoveMessage(List<string> tasksRemoved, List<string> tasksFailedToRemove)
    {
        // Build response
        string removedString = String.Join(", ", tasksRemoved);
        if (tasksFailedToRemove.Count == 0)
        {
            return $"Removed task(s)!";
        }
        else
        {
            string failedString = String.Join(", ", tasksFailedToRemove);
            return $"Failed to remove: {failedString}";
        }
    }

    // remove user(s) if they don't have tasks
    private void Cleanup(bool userOnly = false)
    {
        if (userOnly)
        {
            List<Task> userTasks = ListUserTasks();
            if (userTasks.Count == 0)
            {
                string userKey = GetKey();
                this.taskData.Remove(userKey);
            }
        }
        else
        {
            List<string> KeysToRemove = new List<string>();
            foreach (var item in this.taskData)
            {
                if (item.Value.tasks.Count == 0)
                {
                    KeysToRemove.Add(item.Key);
                }
            }

            foreach (string key in KeysToRemove)
            {
                this.taskData.Remove(key);
            }
        }

        SaveTasks();
    }

    public bool RemoveCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        List<Task> userTasks = ListUserTasks();
        if (userTasks.Count == 0)
        {
            Respond("Error 404: Tasks not found");
            return false;
        }

        List<string> tasksToBeRemoved = ParseTasksInput(rawInput);
        if (tasksToBeRemoved.Count == 0)
        {
            Respond("Error: Invalid input");
            return false;
        }

        List<string> tasksRemoved = new List<string>();
        List<string> tasksFailedToRemove = new List<string>();
        List<int> taskIndices = new List<int>();
        // Gather indices of tasks to be removed
        foreach (string task in tasksToBeRemoved)
        {
            int index = GetTaskIndex(userTasks, task);
            if (index > -1 && !taskIndices.Contains(index))
            {
                taskIndices.Add(index);
                tasksRemoved.Add(userTasks[index].Name); // Track what we're removing
            }
            else
            {
                tasksFailedToRemove.Add(task);
            }
        }

        // Nothing valid to remove
        if (taskIndices.Count == 0)
        {
            string failedString = String.Join(", ", tasksFailedToRemove);
            Respond($"Failed to remove task(s): {failedString}");
            return false;
        }

        // Remove from highest index to lowest to preserve indices
        foreach (int i in taskIndices.OrderByDescending(n => n))
        {
            userTasks.RemoveAt(i);
            Broadcast(new { mode = "remove", index = i });
        }

        // Save data
        SaveIntoTasks(userTasks);
        Cleanup(true);
        Respond(BuildRemoveMessage(tasksRemoved, tasksFailedToRemove));
        return true;
    }

    // admin delete
    // !adel @user <task number>
    public bool AdminDelete()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        string[] spaceSeparated = rawInput.Split(new[] { ' ' }, 2);
        if (spaceSeparated.Count() == 0)
        {
            return false;
        }

        string user = spaceSeparated[0];
        string key = GetKey(user);
        if (key == "")
        {
            Respond("Error: Unable to find user with that username on the task list");
            return false;
        }

        // fuck it, just remove all
        this.taskData.Remove(key);
        SaveTasks();
        Respond("All of the user's tasks have been deleted");
        Broadcast(new { mode = "admindelete", id = key });
        return true;
    }

    // done command
    private string BuildCompletedMessage(List<string> tasksCompleted, List<string> tasksFailedToComplete)
    {
        // Build response
        // string completedString = String.Join(", ", tasksCompleted);
        if (tasksFailedToComplete.Count == 0 && tasksCompleted.Count == 1)
        {
            return "Task completed!";
        }
        else if (tasksFailedToComplete.Count == 0)
        {
            return "Completed all task(s) specified!";
        }
        else
        {
            string failedString = String.Join(", ", tasksFailedToComplete);
            return $"Failed to complete: {failedString}";
        }
    }

    public bool DoneCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        List<Task> userTasks = ListUserTasks();
        if (userTasks.Count == 0)
        {
            Respond("Error 404: Tasks not found");
            return false;
        }

        List<string> tasksToBeCompleted = ParseTasksInput(rawInput);
        if (tasksToBeCompleted.Count == 0)
        {
            Respond("Error: Invalid input");
            return false;
        }

        List<string> tasksCompleted = new List<string>();
        List<string> tasksFailedToComplete = new List<string>();
        List<int> taskIndices = new List<int>();
        // Gather indices of tasks to be removed
        foreach (string task in tasksToBeCompleted)
        {
            int index = GetTaskIndex(userTasks, task);
            if (index > -1 && !taskIndices.Contains(index) && !userTasks[index].Completed)
            {
                taskIndices.Add(index);
                tasksCompleted.Add(userTasks[index].Name); // Track what we're removing
            }
            else
            {
                tasksFailedToComplete.Add(task);
            }
        }

        // Nothing valid to complete
        if (taskIndices.Count == 0)
        {
            string failedString = String.Join(", ", tasksFailedToComplete);
            Respond($"Failed to complete task(s): {failedString}");
            return false;
        }

        // Mark all as complete
        foreach (int i in taskIndices)
        {
            userTasks[i].Completed = true;
            userTasks[i].Focused = false;
            Broadcast(new { mode = "done", index = i });
        }

        // Save data
        SaveIntoTasks(userTasks);
        Respond(BuildCompletedMessage(tasksCompleted, tasksFailedToComplete));
        return true;
    }

    // unfocus
    private void Unfocus()
    {
        List<Task> tasks = ListUserTasks();
        for (int i = 0; i < tasks.Count; i++)
        {
            tasks[i].Focused = false;
        }

        SaveIntoTasks(tasks);
    }

    public bool UnfocusCommand()
    {
        Unfocus();
        Respond("Task have been unfocused!");
        Broadcast(new { mode = "unfocus" });
        return true;
    }

    // undone
    public bool UndoneCommand()
    {
        CPH.TryGetArg("rawInput", out string rawInput);
        List<Task> userTasks = ListUserTasks();
        if (userTasks.Count == 0)
        {
            Respond("Error 404: Tasks not found");
            return false;
        }

        List<string> tasksToBeCompleted = ParseUndoneInput(rawInput);
        if (tasksToBeCompleted.Count == 0)
        {
            Respond("Error: Invalid input");
            return false;
        }

        List<string> tasksCompleted = new List<string>();
        List<string> tasksFailedToComplete = new List<string>();
        List<int> taskIndices = new List<int>();
        // Gather indices of tasks to be removed
        foreach (string task in tasksToBeCompleted)
        {
            int index = GetTaskIndex(userTasks, task);
            if (index > -1 && !taskIndices.Contains(index) && userTasks[index].Completed)
            {
                taskIndices.Add(index);
                tasksCompleted.Add(userTasks[index].Name); // Track what we're removing
            }
            else
            {
                tasksFailedToComplete.Add(task);
            }
        }

        // Nothing valid to complete
        if (taskIndices.Count == 0)
        {
            string failedString = String.Join(", ", tasksFailedToComplete);
            Respond($"Failed to un-done task(s): {failedString}");
            return false;
        }

        // Mark all as complete
        foreach (int i in taskIndices)
        {
            userTasks[i].Completed = false;
            userTasks[i].Focused = false;
            Broadcast(new { mode = "undone", index = i });
        }

        // Save data
        SaveIntoTasks(userTasks);
        Respond(BuildUndoneMessage(tasksCompleted, tasksFailedToComplete));
        return true;
    }

    private List<string> ParseUndoneInput(string input)
    {
        input = input.Trim();
        var tasksToRemove = new List<string>();
        bool IsSpaceSeparatedInts = Regex.IsMatch(input, @"^\d+(\s+\d+)*$");
        if (IsSpaceSeparatedInts)
        {
            return input.Split(' ').ToList();
        }

        var splittedInput = input.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
        if (splittedInput.Count == 0)
        {
            // get the only completed task
            List<Task> userTasks = ListUserTasks();
            int soloCompletedTaskIndex = -1;
            var completedTasks = userTasks.Where(t => t.Completed);
            if (completedTasks.Count() == 1)
            {
                soloCompletedTaskIndex = userTasks.FindIndex(t => t.Completed) + 1;
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

        return new()
        {
        };
    }

    private string BuildUndoneMessage(List<string> tasksCompleted, List<string> tasksFailedToComplete)
    {
        // Build response
        // string completedString = String.Join(", ", tasksCompleted);
        if (tasksFailedToComplete.Count == 0 && tasksCompleted.Count == 1)
        {
            return "Task marked as incomplete!";
        }
        else if (tasksFailedToComplete.Count == 0)
        {
            return "Task(s) marked as incomplete!";
        }
        else
        {
            string failedString = String.Join(", ", tasksFailedToComplete);
            return $"Failed to parse: {failedString}";
        }
    }

    // CLEAR TASKS
    public bool ClearAllCommand()
    {
        // clear all TASKS only
        foreach (var item in this.taskData)
        {
            this.taskData[item.Key].tasks = new()
            {
            };
        }

        Cleanup(false);
        SaveTasks();
        Broadcast(new { mode = "clearall" });
        Respond("All tasks have been cleared!");
        return true;
    }

    public bool ClearMyDoneCommand()
    {
        // clear all of the user's completed tasks only
        string key = GetKey();
        this.taskData[key].tasks = this.taskData[key].tasks.Where(t => !t.Completed).ToList();
        Cleanup(false);
        SaveTasks();
        Broadcast(new { mode = "clearmydone" });
        Respond("All of your completed tasks have been cleared!");
        return true;
    }

    public bool ClearDoneCommand()
    {
        // clear all COMPLETED TASKS only
        foreach (var item in this.taskData)
        {
            this.taskData[item.Key].tasks = this.taskData[item.Key].tasks.Where(t => !t.Completed).ToList();
        }

        Cleanup(false);
        SaveTasks();
        Broadcast(new { mode = "cleardone" });
        Respond("All completed tasks have been cleared!");
        return true;
    }

    public bool ClearNotStreamerCommand()
    {
        var streamersUsernames = GetStreamerUsernames();
        // clear all TASKS only if not 
        this.taskData = this.taskData.Where(kvp => streamersUsernames.Any(s => string.Equals(s, kvp.Value.username, StringComparison.OrdinalIgnoreCase))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        Cleanup(false);
        SaveTasks();
        Broadcast(new { mode = "clearns" });
        Respond("All completed tasks have been cleared!");
        return true;
    }
}