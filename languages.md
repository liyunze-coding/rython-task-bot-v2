# Languages

## How to change language

1. Finish setup in [README](./README#setup-instructions)
2. On StreamerBot, go to Actions -> \[RythonDev\] Task Bot -> 0. Tasks Code
3. Under Sub-Actions, double click on `Execute Code (Task Bot 2.0)` to open the editor
4. Find (Ctrl+F) `#region Response Text`
5. Replace the section `#region Response Text` and `#endregion` *inclusive* with your desired language config.

## Languages

- [English](#english) (DEFAULT)
- [Spanish](#spanish)
- [Italian](#italian)
- [Chinese (Simplified)](#chinese-simplified)
- [Chinese (Traditional)](#chinese-traditional)

### English

```cs
#region Response Text
public static class BotResponses
{
    // Customize chat-facing text in this section when translating the bot.
    public const string Help = "📝 Rython Task Bot Commands: !task !edit !remove !done. For mods, you can do !adel @user. More commmands here: https://github.com/liyunze-coding/rython-task-bot-v2#usage";
    public const string EditNotEnoughArguments = "❌ Error: Not enough arguments, try !edit <number> <new task>";
    public const string EditInvalidArgument = "❌  Error: Not a valid argument, try !edit <number> <new task>";
    public const string TaskCannotBeEmpty = "❌ Error: Task cannot be empty";
    public const string TaskCannotBeNumber = "❌ Error: Task cannot be a number";
    public const string ReservedAllKeyword = "❌ Error: 'All' is a reserved keyword";
    public const string TasksNotFound = "❌ Error 404: Tasks not found";
    public const string TasksNotFoundShort = "❌ 404 Tasks not found";
    public const string InvalidTaskNumber = "❌ Error: Invalid task number";
    public const string TaskAlreadyExists = "❌ Error: Task already exists";
    public const string CannotFocusMultipleTasks = "❌ Cannot focus on multiple tasks";
    public const string IndexOutOfRange = "❌ Error: Index out of range.";
    public const string CannotFocusCompletedTask = "❌ Error: Cannot focus on completed task.";
    public const string CannotFocusCompletedTaskShort = "❌ Cannot focus on completed task.";
    public const string NoTasksProvided = "❌ No tasks provided";
    public const string NoTasksProvidedWithError = "❌ Error: No tasks provided";
    public const string NoFocusedTask = "❌ You do not have a focused task.";
    public const string NextNeedsFocusedTask = "❌ Can't use !next command, select a task to complete using !done and/or add another task";
    public const string UserNotFound = "❌ User not found";
    public const string InvalidInput = "❌ Error: Invalid input";
    public const string AdminDeleteUserNotFound = "❌ Error: Unable to find user with that username on the task list";
    public const string AdminDeleteSuccess = "All of the user's tasks have been deleted";
    public const string Unfocused = "📝 Task have been unfocused!";
    public const string ClearAll = "📝 All tasks have been cleared!";
    public const string ClearMyDone = "📝 All of your completed tasks have been cleared!";
    public const string ClearDone = "📝 All completed tasks have been cleared!";
    public const string ClearNotStreamer = "📝 All tasks (excluding the streamer's) have been cleared!";
    public const string InvalidDaysArgument = "❌ Error: Please provide a valid number of days, e.g. !clearold 7";
    public const string AddSuccessShort = "Task List Update ♡ →  Your task(s) have been added! Good luck! 🍀";
    public const string AddFailedShort = "❌ Failed to add your task(s)";
    public const string AddPartialShort = "Task List Update ♡ → some tasks were successful but some failed :p";
    public const string NoTasksToAdd = "No tasks to add";
    public const string LogSuccessShort = "Task List Update ♡ →  Your task(s) have been logged! Good job! 🍀";
    public const string LogFailedShort = "❌ Failed to logged your task(s)";
    public const string LogPartialShort = "Task List Update ♡ → some tasks were logged successfully but some failed :p";
    public const string NoTasksToLog = "No tasks to log";
    public const string RemoveAllSuccess = "Tasklist Update 🚮 → All your tasks have been removed!";
    public const string RemoveMultipleSuccess = "Tasklist Update 🚮 → Tasks removed successfully!";
    public const string CompleteAllSuccess = "Task list updated! 🎉 Good job on completing all your tasks!";
    public const string CompleteMultipleSuccess = "Task list updated! 🎉 Good job on finishing your tasks!";
    public const string UndoneMultipleSuccess = "Task list updated 📝 — Tasks marked as incomplete";
    public const string FocusedMarker = "(ongoing) ";

    public static string TaskAlreadyExistsWithName(string taskName) => $"❌ Error: Task '{taskName}' already exists";
    public static string AddSuccess(string addedTasks) => $"Task List Update ♡ → The task(s): \"{addedTasks}\" have been added! Good luck! 🍀";
    public static string AddFailed(string failedTasks) => $"❌ Failed to add your task(s) {failedTasks}";
    public static string AddPartial(string addedTasks, string failedTasks) => $"Task List Update ♡ → Task(s) added: \"{addedTasks}\" | Failed: {failedTasks}";
    public static string LogSuccess(string loggedTasks) => $"Task List Update ♡ → The task(s) {loggedTasks} have been logged! Good job! 🍀";
    public static string LogFailed(string failedTasks) => $"❌ Failed to logged your task(s) {failedTasks}";
    public static string LogPartial(string loggedTasks, string failedTasks) => $"Task List Update ♡ → Task(s) logged: {loggedTasks} | Failed: {failedTasks}";
    public static string RemoveSingleSuccess(string taskName) => $"Tasklist Update 🚮 → Task '{taskName}' has been deleted successfully!";
    public static string RemoveFailed(string failedTasks) => $"Failed to remove: {failedTasks}";
    public static string CompleteSingleSuccess(string taskName) => $"Task list updated! 🎉 Good job on finishing the task '{taskName}'!";
    public static string CompleteFailed(string failedTasks) => $"❌ Failed to complete: {failedTasks}";
    public static string UndoneSingleSuccess(string taskName) => $"Task list updated 📝 — Task {taskName} marked as incomplete!";
    public static string UndoneFailed(string failedTasks) => $"❌ Failed to mark as incomplete: {failedTasks}";
    public static string FocusedTask(int taskNumber, string taskName) => $"✅ Current focused task: {taskNumber}. {taskName}";
    public static string NextSuccess(string completedTaskName, int nextTaskNumber, string nextTaskName) => $"✅ Good job on completing '{completedTaskName}'! Moving onto '({nextTaskNumber}) {nextTaskName}'";
    public static string EditSuccess(string oldTaskName, string newTaskName) => $"📝 Task '{oldTaskName}' has been edited to '{newTaskName}'";
    public static string PendingTaskItem(int taskNumber, string taskName, bool focused) => $"{taskNumber}. {(focused ? FocusedMarker : "")}{taskName}";
    public static string PendingTasks(int count, string tasks) => $"Task(s) pending ({count}): {tasks}";
    public static string OtherUserPendingTasks(string username, int count, string tasks) => $"{username}'s task(s) ({count}): {tasks}";
    public static string CompletedTaskItem(int taskNumber, string taskName) => $"{taskNumber}. {taskName}";
    public static string CompletedTasks(int count, string tasks) => $"Completed {count} tasks: {tasks}";
    public static string OtherUserCompletedTasks(string username, int count, string tasks) => $"{username}'s tasks ({count}): {tasks}";
    public static string FailedToRemoveTasks(string failedTasks) => $"❌ Failed to remove task(s): {failedTasks}";
    public static string FailedToCompleteTasks(string failedTasks) => $"❌ Failed to complete task(s): {failedTasks}";
    public static string FailedToUndoneTasks(string failedTasks) => $"❌ Failed to un-done task(s): {failedTasks}";
    public static string OtherUserCount(string username, int completedCount) => $"{username} has completed {completedCount} task(s) so far!";
    public static string MyCount(int completedCount) => $"You have completed {completedCount} task(s) so far!";
    public static string ClearOld(int count, int days) => $"📝 Cleared {count} task(s) that were {days} day(s) or older!";
    public static string CountOld(int count, int days) => $"📝 There are {count} task(s) that are {days} day(s) or older.";
}

#endregion
```

### Spanish

```cs
#region Response Text
public static class BotResponses
{
    // Customize chat-facing text in this section when translating the bot.
    public const string Help = "📝 Comandos del Rython Task Bot: !task !edit !remove !done. Los moderadores pueden usar !adel @usuario. Más comandos aquí: https://github.com/liyunze-coding/rython-task-bot-v2#usage";
    public const string EditNotEnoughArguments = "❌ Error: Argumentos insuficientes, prueba !edit <número> <nueva tarea>";
    public const string EditInvalidArgument = "❌ Error: Argumento no válido, prueba !edit <número> <nueva tarea>";
    public const string TaskCannotBeEmpty = "❌ Error: La tarea no puede estar vacía";
    public const string TaskCannotBeNumber = "❌ Error: La tarea no puede ser un número";
    public const string ReservedAllKeyword = "❌ Error: 'All' es una palabra reservada";
    public const string TasksNotFound = "❌ Error 404: Tareas no encontradas";
    public const string TasksNotFoundShort = "❌ 404 Tareas no encontradas";
    public const string InvalidTaskNumber = "❌ Error: Número de tarea no válido";
    public const string TaskAlreadyExists = "❌ Error: La tarea ya existe";
    public const string CannotFocusMultipleTasks = "❌ No se puede enfocar en múltiples tareas";
    public const string IndexOutOfRange = "❌ Error: Índice fuera de rango.";
    public const string CannotFocusCompletedTask = "❌ Error: No se puede enfocar una tarea completada.";
    public const string CannotFocusCompletedTaskShort = "❌ No se puede enfocar una tarea completada.";
    public const string NoTasksProvided = "❌ No se proporcionaron tareas";
    public const string NoTasksProvidedWithError = "❌ Error: No se proporcionaron tareas";
    public const string NoFocusedTask = "❌ No tienes una tarea enfocada.";
    public const string NextNeedsFocusedTask = "❌ No se puede usar el comando !next, selecciona una tarea para completar usando !done y/o añade otra tarea";
    public const string UserNotFound = "❌ Usuario no encontrado";
    public const string InvalidInput = "❌ Error: Entrada no válida";
    public const string AdminDeleteUserNotFound = "❌ Error: No se pudo encontrar al usuario con ese nombre en la lista de tareas";
    public const string AdminDeleteSuccess = "Se han eliminado todas las tareas del usuario";
    public const string Unfocused = "📝 ¡La tarea ha dejado de estar enfocada!";
    public const string ClearAll = "📝 ¡Todas las tareas han sido eliminadas!";
    public const string ClearMyDone = "📝 ¡Todas tus tareas completadas han sido eliminadas!";
    public const string ClearDone = "📝 ¡Todas las tareas completadas han sido eliminadas!";
    public const string ClearNotStreamer = "📝 ¡Todas las tareas (excluyendo las del streamer) han sido eliminadas!";
    public const string InvalidDaysArgument = "❌ Error: Por favor, proporciona un número válido de días, ej: !clearold 7";
    public const string AddSuccessShort = "Actualización de Lista ♡ → ¡Tus tareas han sido añadidas! ¡Buena suerte! 🍀";
    public const string AddFailedShort = "❌ Fallo al añadir tu(s) tarea(s)";
    public const string AddPartialShort = "Actualización de Lista ♡ → algunas tareas se añadieron correctamente pero otras fallaron :p";
    public const string NoTasksToAdd = "No hay tareas para añadir";
    public const string LogSuccessShort = "Actualización de Lista ♡ → ¡Tus tareas han sido registradas! ¡Buen trabajo! 🍀";
    public const string LogFailedShort = "❌ Fallo al registrar tu(s) tarea(s)";
    public const string LogPartialShort = "Actualización de Lista ♡ → algunas tareas se registraron correctamente pero otras fallaron :p";
    public const string NoTasksToLog = "No hay tareas para registrar";
    public const string RemoveAllSuccess = "Actualización de Lista 🚮 → ¡Todas tus tareas han sido eliminadas!";
    public const string RemoveMultipleSuccess = "Actualización de Lista 🚮 → ¡Tareas eliminadas con éxito!";
    public const string CompleteAllSuccess = "¡Lista de tareas actualizada! 🎉 ¡Buen trabajo completando todas tus tareas!";
    public const string CompleteMultipleSuccess = "¡Lista de tareas actualizada! 🎉 ¡Buen trabajo terminando tus tareas!";
    public const string UndoneMultipleSuccess = "Lista de tareas actualizada 📝 — Tareas marcadas como incompletas";
    public const string FocusedMarker = "(en curso) ";

    public static string TaskAlreadyExistsWithName(string taskName) => $"❌ Error: La tarea '{taskName}' ya existe";
    public static string AddSuccess(string addedTasks) => $"Actualización de Lista ♡ → La(s) tarea(s): \"{addedTasks}\" han sido añadidas! ¡Buena suerte! 🍀";
    public static string AddFailed(string failedTasks) => $"❌ Fallo al añadir tu(s) tarea(s) {failedTasks}";
    public static string AddPartial(string addedTasks, string failedTasks) => $"Actualización de Lista ♡ → Tarea(s) añadidas: \"{addedTasks}\" | Fallidas: {failedTasks}";
    public static string LogSuccess(string loggedTasks) => $"Actualización de Lista ♡ → La(s) tarea(s) {loggedTasks} han sido registradas! ¡Buen trabajo! 🍀";
    public static string LogFailed(string failedTasks) => $"❌ Fallo al registrar tu(s) tarea(s) {failedTasks}";
    public static string LogPartial(string loggedTasks, string failedTasks) => $"Actualización de Lista ♡ → Tarea(s) registradas: {loggedTasks} | Fallidas: {failedTasks}";
    public static string RemoveSingleSuccess(string taskName) => $"Actualización de Lista 🚮 → ¡La tarea '{taskName}' ha sido eliminada con éxito!";
    public static string RemoveFailed(string failedTasks) => $"Fallo al eliminar: {failedTasks}";
    public static string CompleteSingleSuccess(string taskName) => $"¡Lista de tareas actualizada! 🎉 ¡Buen trabajo terminando la tarea '{taskName}'!";
    public static string CompleteFailed(string failedTasks) => $"❌ Fallo al completar: {failedTasks}";
    public static string UndoneSingleSuccess(string taskName) => $"Lista de tareas actualizada 📝 — ¡Tarea '{taskName}' marcada como incompleta!";
    public static string UndoneFailed(string failedTasks) => $"❌ Fallo al marcar como incompleta: {failedTasks}";
    public static string FocusedTask(int taskNumber, string taskName) => $"✅ Tarea enfocada actual: {taskNumber}. {taskName}";
    public static string NextSuccess(string completedTaskName, int nextTaskNumber, string nextTaskName) => $"✅ ¡Buen trabajo completando '{completedTaskName}'! Pasando a '({nextTaskNumber}) {nextTaskName}'";
    public static string EditSuccess(string oldTaskName, string newTaskName) => $"📝 La tarea '{oldTaskName}' ha sido editada a '{newTaskName}'";
    public static string PendingTaskItem(int taskNumber, string taskName, bool focused) => $"{taskNumber}. {(focused ? FocusedMarker : "")}{taskName}";
    public static string PendingTasks(int count, string tasks) => $"Tarea(s) pendiente(s) ({count}): {tasks}";
    public static string OtherUserPendingTasks(string username, int count, string tasks) => $"Tarea(s) de {username} ({count}): {tasks}";
    public static string CompletedTaskItem(int taskNumber, string taskName) => $"{taskNumber}. {taskName}";
    public static string CompletedTasks(int count, string tasks) => $"{count} tareas completadas: {tasks}";
    public static string OtherUserCompletedTasks(string username, int count, string tasks) => $"Tareas de {username} ({count}): {tasks}";
    public static string FailedToRemoveTasks(string failedTasks) => $"❌ Fallo al eliminar tarea(s): {failedTasks}";
    public static string FailedToCompleteTasks(string failedTasks) => $"❌ Fallo al completar tarea(s): {failedTasks}";
    public static string FailedToUndoneTasks(string failedTasks) => $"❌ Fallo al marcar como incompleta la(s) tarea(s): {failedTasks}";
    public static string OtherUserCount(string username, int completedCount) => $"¡{username} ha completado {completedCount} tarea(s) hasta ahora!";
    public static string MyCount(int completedCount) => $"¡Has completado {completedCount} tarea(s) hasta ahora!";
    public static string ClearOld(int count, int days) => $"📝 ¡Se eliminaron {count} tarea(s) que tenían {days} día(s) o más!";
    public static string CountOld(int count, int days) => $"📝 Hay {count} tarea(s) que tienen {days} día(s) o más.";
}

#endregion
```

### Italian

```cs
#region Response Text
public static class BotResponses
{
    // Customize chat-facing text in this section when translating the bot.
    public const string Help = "📝 Comandi di Rython Task Bot: !task !edit !remove !done. I moderatori possono usare !adel @utente. Altri comandi qui: https://github.com/liyunze-coding/rython-task-bot-v2#usage";
    public const string EditNotEnoughArguments = "❌ Errore: Argomenti insufficienti, prova !edit <numero> <nuova attività>";
    public const string EditInvalidArgument = "❌ Errore: Argomento non valido, prova !edit <numero> <nuova attività>";
    public const string TaskCannotBeEmpty = "❌ Errore: L'attività non può essere vuota";
    public const string TaskCannotBeNumber = "❌ Errore: L'attività non può essere un numero";
    public const string ReservedAllKeyword = "❌ Errore: 'All' è una parola riservata";
    public const string TasksNotFound = "❌ Errore 404: Attività non trovate";
    public const string TasksNotFoundShort = "❌ 404 Attività non trovate";
    public const string InvalidTaskNumber = "❌ Errore: Numero di attività non valido";
    public const string TaskAlreadyExists = "❌ Errore: L'attività esiste già";
    public const string CannotFocusMultipleTasks = "❌ Impossibile mettere in evidenza più attività";
    public const string IndexOutOfRange = "❌ Errore: Indice fuori intervallo.";
    public const string CannotFocusCompletedTask = "❌ Errore: Impossibile mettere in evidenza un'attività completata.";
    public const string CannotFocusCompletedTaskShort = "❌ Impossibile mettere in evidenza un'attività completata.";
    public const string NoTasksProvided = "❌ Nessuna attività fornita";
    public const string NoTasksProvidedWithError = "❌ Errore: Nessuna attività fornita";
    public const string NoFocusedTask = "❌ Non hai un'attività in evidenza.";
    public const string NextNeedsFocusedTask = "❌ Impossibile usare il comando !next, seleziona un'attività da completare usando !done e/o aggiungi un'altra attività";
    public const string UserNotFound = "❌ Utente non trovato";
    public const string InvalidInput = "❌ Errore: Input non valido";
    public const string AdminDeleteUserNotFound = "❌ Errore: Impossibile trovare l'utente con quel nome nella lista delle attività";
    public const string AdminDeleteSuccess = "Tutte le attività dell'utente sono state eliminate";
    public const string Unfocused = "📝 L'attività non è più in evidenza!";
    public const string ClearAll = "📝 Tutte le attività sono state rimosse!";
    public const string ClearMyDone = "📝 Tutte le tue attività completate sono state rimosse!";
    public const string ClearDone = "📝 Tutte le attività completate sono state rimosse!";
    public const string ClearNotStreamer = "📝 Tutte le attività (escluse quelle dello streamer) sono state rimosse!";
    public const string InvalidDaysArgument = "❌ Errore: Fornisci un numero valido di giorni, es. !clearold 7";
    public const string AddSuccessShort = "Aggiornamento Lista ♡ → Le tue attività sono state aggiunte! Buona fortuna! 🍀";
    public const string AddFailedShort = "❌ Impossibile aggiungere le tue attività";
    public const string AddPartialShort = "Aggiornamento Lista ♡ → alcune attività sono state aggiunte con successo ma altre sono fallite :p";
    public const string NoTasksToAdd = "Nessuna attività da aggiungere";
    public const string LogSuccessShort = "Aggiornamento Lista ♡ → Le tue attività sono state registrate! Ottimo lavoro! 🍀";
    public const string LogFailedShort = "❌ Impossibile registrare le tue attività";
    public const string LogPartialShort = "Aggiornamento Lista ♡ → alcune attività sono state registrate con successo ma altre sono fallite :p";
    public const string NoTasksToLog = "Nessuna attività da registrare";
    public const string RemoveAllSuccess = "Aggiornamento Lista 🚮 → Tutte le tue attività sono state rimosse!";
    public const string RemoveMultipleSuccess = "Aggiornamento Lista 🚮 → Attività rimosse con successo!";
    public const string CompleteAllSuccess = "Lista delle attività aggiornata! 🎉 Ottimo lavoro, hai completato tutte le tue attività!";
    public const string CompleteMultipleSuccess = "Lista delle attività aggiornata! 🎉 Ottimo lavoro, hai terminato le tue attività!";
    public const string UndoneMultipleSuccess = "Lista delle attività aggiornata 📝 — Attività contrassegnate come non completate";
    public const string FocusedMarker = "(in corso) ";

    public static string TaskAlreadyExistsWithName(string taskName) => $"❌ Errore: L'attività '{taskName}' esiste già";
    public static string AddSuccess(string addedTasks) => $"Aggiornamento Lista ♡ → Attività aggiunta/e: \"{addedTasks}\"! Buona fortuna! 🍀";
    public static string AddFailed(string failedTasks) => $"❌ Impossibile aggiungere l'attività/le attività {failedTasks}";
    public static string AddPartial(string addedTasks, string failedTasks) => $"Aggiornamento Lista ♡ → Attività aggiunta/e: \"{addedTasks}\" | Fallite: {failedTasks}";
    public static string LogSuccess(string loggedTasks) => $"Aggiornamento Lista ♡ → Attività registrata/e: {loggedTasks}! Ottimo lavoro! 🍀";
    public static string LogFailed(string failedTasks) => $"❌ Impossibile registrare l'attività/le attività {failedTasks}";
    public static string LogPartial(string loggedTasks, string failedTasks) => $"Aggiornamento Lista ♡ → Attività registrata/e: {loggedTasks} | Fallite: {failedTasks}";
    public static string RemoveSingleSuccess(string taskName) => $"Aggiornamento Lista 🚮 → L'attività '{taskName}' è stata eliminata con successo!";
    public static string RemoveFailed(string failedTasks) => $"Impossibile rimuovere: {failedTasks}";
    public static string CompleteSingleSuccess(string taskName) => $"Lista delle attività aggiornata! 🎉 Ottimo lavoro per aver terminato l'attività '{taskName}'!";
    public static string CompleteFailed(string failedTasks) => $"❌ Impossibile completare: {failedTasks}";
    public static string UndoneSingleSuccess(string taskName) => $"Lista delle attività aggiornata 📝 — Attività '{taskName}' contrassegnata come non completata!";
    public static string UndoneFailed(string failedTasks) => $"❌ Impossibile contrassegnare come non completata: {failedTasks}";
    public static string FocusedTask(int taskNumber, string taskName) => $"✅ Attività attuale in evidenza: {taskNumber}. {taskName}";
    public static string NextSuccess(string completedTaskName, int nextTaskNumber, string nextTaskName) => $"✅ Ottimo lavoro per aver completato '{completedTaskName}'! Passiamo a '({nextTaskNumber}) {nextTaskName}'";
    public static string EditSuccess(string oldTaskName, string newTaskName) => $"📝 L'attività '{oldTaskName}' è stata modificata in '{newTaskName}'";
    public static string PendingTaskItem(int taskNumber, string taskName, bool focused) => $"{taskNumber}. {(focused ? FocusedMarker : "")}{taskName}";
    public static string PendingTasks(int count, string tasks) => $"Attività in sospeso ({count}): {tasks}";
    public static string OtherUserPendingTasks(string username, int count, string tasks) => $"Attività di {username} ({count}): {tasks}";
    public static string CompletedTaskItem(int taskNumber, string taskName) => $"{taskNumber}. {taskName}";
    public static string CompletedTasks(int count, string tasks) => $"{count} attività completate: {tasks}";
    public static string OtherUserCompletedTasks(string username, int count, string tasks) => $"Attività di {username} ({count}): {tasks}";
    public static string FailedToRemoveTasks(string failedTasks) => $"❌ Impossibile rimuovere l'attività/le attività: {failedTasks}";
    public static string FailedToCompleteTasks(string failedTasks) => $"❌ Impossibile completare l'attività/le attività: {failedTasks}";
    public static string FailedToUndoneTasks(string failedTasks) => $"❌ Impossibile contrassegnare come non completata l'attività/le attività: {failedTasks}";
    public static string OtherUserCount(string username, int completedCount) => $"{username} ha completato {completedCount} attività finora!";
    public static string MyCount(int completedCount) => $"Hai completato {completedCount} attività finora!";
    public static string ClearOld(int count, int days) => $"📝 Rimosse {count} attività vecchie di almeno {days} giorno/i!";
    public static string CountOld(int count, int days) => $"📝 Ci sono {count} attività vecchie di almeno {days} giorno/i.";
}

#endregion
```

### Chinese (Simplified)

```cs
#region Response Text
public static class BotResponses
{
    // Customize chat-facing text in this section when translating the bot.
    public const string Help = "📝 Rython Task Bot 指令：!task !edit !remove !done。管理员可以使用 !adel @用户。更多指令请见：https://github.com/liyunze-coding/rython-task-bot-v2#usage";
    public const string EditNotEnoughArguments = "❌ 错误：参数不足，请尝试 !edit <编号> <新任务>";
    public const string EditInvalidArgument = "❌ 错误：无效的参数，请尝试 !edit <编号> <新任务>";
    public const string TaskCannotBeEmpty = "❌ 错误：任务内容不能为空";
    public const string TaskCannotBeNumber = "❌ 错误：任务内容不能为纯数字";
    public const string ReservedAllKeyword = "❌ 错误：'All' 是保留关键字";
    public const string TasksNotFound = "❌ 错误 404：未找到任务";
    public const string TasksNotFoundShort = "❌ 404 未找到任务";
    public const string InvalidTaskNumber = "❌ 错误：无效的任务编号";
    public const string TaskAlreadyExists = "❌ 错误：该任务已存在";
    public const string CannotFocusMultipleTasks = "❌ 无法同时专注多个任务";
    public const string IndexOutOfRange = "❌ 错误：编号超出范围。";
    public const string CannotFocusCompletedTask = "❌ 错误：无法专注已完成的任务。";
    public const string CannotFocusCompletedTaskShort = "❌ 无法专注已完成的任务。";
    public const string NoTasksProvided = "❌ 未提供任务";
    public const string NoTasksProvidedWithError = "❌ 错误：未提供任务";
    public const string NoFocusedTask = "❌ 你当前没有正在专注的任务。";
    public const string NextNeedsFocusedTask = "❌ 无法使用 !next 指令，请使用 !done 选择要完成的任务，或添加新任务";
    public const string UserNotFound = "❌ 未找到该用户";
    public const string InvalidInput = "❌ 错误：输入无效";
    public const string AdminDeleteUserNotFound = "❌ 错误：在任务列表中未找到该用户名的用户";
    public const string AdminDeleteSuccess = "已删除该用户的所有任务";
    public const string Unfocused = "📝 已取消专注任务！";
    public const string ClearAll = "📝 已清空所有任务！";
    public const string ClearMyDone = "📝 已清空你所有已完成的任务！";
    public const string ClearDone = "📝 已清空所有已完成的任务！";
    public const string ClearNotStreamer = "📝 已清空所有任务（主播除外）！";
    public const string InvalidDaysArgument = "❌ 错误：请提供有效的天数，例如 !clearold 7";
    public const string AddSuccessShort = "任务列表更新 ♡ → 你的任务已添加！加油！🍀";
    public const string AddFailedShort = "❌ 添加任务失败";
    public const string AddPartialShort = "任务列表更新 ♡ → 部分任务添加成功，但部分失败了 :p";
    public const string NoTasksToAdd = "没有要添加的任务";
    public const string LogSuccessShort = "任务列表更新 ♡ → 你的任务已记录！干得好！🍀";
    public const string LogFailedShort = "❌ 记录任务失败";
    public const string LogPartialShort = "任务列表更新 ♡ → 部分任务记录成功，但部分失败了 :p";
    public const string NoTasksToLog = "没有要记录的任务";
    public const string RemoveAllSuccess = "任务列表更新 🚮 → 你的所有任务均已移除！";
    public const string RemoveMultipleSuccess = "任务列表更新 🚮 → 任务已成功移除！";
    public const string CompleteAllSuccess = "任务列表已更新！🎉 恭喜你完成了所有任务！";
    public const string CompleteMultipleSuccess = "任务列表已更新！🎉 恭喜你完成了这些任务！";
    public const string UndoneMultipleSuccess = "任务列表已更新 📝 — 任务已标记为未完成";
    public const string FocusedMarker = "(进行中) ";

    public static string TaskAlreadyExistsWithName(string taskName) => $"❌ 错误：任务 '{taskName}' 已存在";
    public static string AddSuccess(string addedTasks) => $"任务列表更新 ♡ → 任务：\"{addedTasks}\" 已添加！加油！🍀";
    public static string AddFailed(string failedTasks) => $"❌ 添加任务失败：{failedTasks}";
    public static string AddPartial(string addedTasks, string failedTasks) => $"任务列表更新 ♡ → 已添加任务：\"{addedTasks}\" | 失败：{failedTasks}";
    public static string LogSuccess(string loggedTasks) => $"任务列表更新 ♡ → 任务 {loggedTasks} 已记录！干得好！🍀";
    public static string LogFailed(string failedTasks) => $"❌ 记录任务失败：{failedTasks}";
    public static string LogPartial(string loggedTasks, string failedTasks) => $"任务列表更新 ♡ → 已记录任务：{loggedTasks} | 失败：{failedTasks}";
    public static string RemoveSingleSuccess(string taskName) => $"任务列表更新 🚮 → 任务 '{taskName}' 已成功删除！";
    public static string RemoveFailed(string failedTasks) => $"移除失败：{failedTasks}";
    public static string CompleteSingleSuccess(string taskName) => $"任务列表已更新！🎉 恭喜完成任务 '{taskName}'！";
    public static string CompleteFailed(string failedTasks) => $"❌ 完成失败：{failedTasks}";
    public static string UndoneSingleSuccess(string taskName) => $"任务列表已更新 📝 — 任务 {taskName} 已标记为未完成！";
    public static string UndoneFailed(string failedTasks) => $"❌ 标记为未完成失败：{failedTasks}";
    public static string FocusedTask(int taskNumber, string taskName) => $"✅ 当前专注任务：{taskNumber}. {taskName}";
    public static string NextSuccess(string completedTaskName, int nextTaskNumber, string nextTaskName) => $"✅ 恭喜完成 '{completedTaskName}'！接下来进行 '({nextTaskNumber}) {nextTaskName}'";
    public static string EditSuccess(string oldTaskName, string newTaskName) => $"📝 任务 '{oldTaskName}' 已修改为 '{newTaskName}'";
    public static string PendingTaskItem(int taskNumber, string taskName, bool focused) => $"{taskNumber}. {(focused ? FocusedMarker : "")}{taskName}";
    public static string PendingTasks(int count, string tasks) => $"待办任务 ({count})：{tasks}";
    public static string OtherUserPendingTasks(string username, int count, string tasks) => $"{username} 的任务 ({count})：{tasks}";
    public static string CompletedTaskItem(int taskNumber, string taskName) => $"{taskNumber}. {taskName}";
    public static string CompletedTasks(int count, string tasks) => $"已完成 {count} 项任务：{tasks}";
    public static string OtherUserCompletedTasks(string username, int count, string tasks) => $"{username} 的任务 ({count})：{tasks}";
    public static string FailedToRemoveTasks(string failedTasks) => $"❌ 移除任务失败：{failedTasks}";
    public static string FailedToCompleteTasks(string failedTasks) => $"❌ 完成任务失败：{failedTasks}";
    public static string FailedToUndoneTasks(string failedTasks) => $"❌ 标记未完成失败：{failedTasks}";
    public static string OtherUserCount(string username, int completedCount) => $"{username} 目前已完成 {completedCount} 项任务！";
    public static string MyCount(int completedCount) => $"你目前已完成 {completedCount} 项任务！";
    public static string ClearOld(int count, int days) => $"📝 已清理 {count} 项 {days} 天或更早之前的任务！";
    public static string CountOld(int count, int days) => $"📝 共有 {count} 项 {days} 天或更早之前的任务。";
}

#endregion
```

### Chinese (Traditional)

```cs
#region Response Text
public static class BotResponses
{
    // Customize chat-facing text in this section when translating the bot.
    public const string Help = "📝 Rython Task Bot 指令：!task !edit !remove !done。管理員可以使用 !adel @使用者。更多指令請見：https://github.com/liyunze-coding/rython-task-bot-v2#usage";
    public const string EditNotEnoughArguments = "❌ 錯誤：參數不足，請嘗試 !edit <編號> <新任務>";
    public const string EditInvalidArgument = "❌ 錯誤：無效的參數，請嘗試 !edit <編號> <新任務>";
    public const string TaskCannotBeEmpty = "❌ 錯誤：任務內容不能為空";
    public const string TaskCannotBeNumber = "❌ 錯誤：任務內容不能為純數字";
    public const string ReservedAllKeyword = "❌ 錯誤：'All' 是保留關鍵字";
    public const string TasksNotFound = "❌ 錯誤 404：找不到任務";
    public const string TasksNotFoundShort = "❌ 404 找不到任務";
    public const string InvalidTaskNumber = "❌ 錯誤：無效的任務編號";
    public const string TaskAlreadyExists = "❌ 錯誤：該任務已存在";
    public const string CannotFocusMultipleTasks = "❌ 無法同時專注多個任務";
    public const string IndexOutOfRange = "❌ 錯誤：編號超出範圍。";
    public const string CannotFocusCompletedTask = "❌ 錯誤：無法專注已完成的任務。";
    public const string CannotFocusCompletedTaskShort = "❌ 無法專注已完成的任務。";
    public const string NoTasksProvided = "❌ 未提供任務";
    public const string NoTasksProvidedWithError = "❌ 錯誤：未提供任務";
    public const string NoFocusedTask = "❌ 你目前沒有正在專注的任務。";
    public const string NextNeedsFocusedTask = "❌ 無法使用 !next 指令，請使用 !done 選擇要完成的任務，或新增其他任務";
    public const string UserNotFound = "❌ 找不到該使用者";
    public const string InvalidInput = "❌ 錯誤：輸入無效";
    public const string AdminDeleteUserNotFound = "❌ 錯誤：在任務清單中找不到該使用者名稱的使用者";
    public const string AdminDeleteSuccess = "已刪除該使用者的所有任務";
    public const string Unfocused = "📝 已取消專注任務！";
    public const string ClearAll = "📝 已清空所有任務！";
    public const string ClearMyDone = "📝 已清空你所有已完成的任務！";
    public const string ClearDone = "📝 已清空所有已完成的任務！";
    public const string ClearNotStreamer = "📝 已清空所有任務（實況主除外）！";
    public const string InvalidDaysArgument = "❌ 錯誤：請提供有效的天數，例如 !clearold 7";
    public const string AddSuccessShort = "任務清單更新 ♡ → 你的任務已新增！加油！🍀";
    public const string AddFailedShort = "❌ 新增任務失敗";
    public const string AddPartialShort = "任務清單更新 ♡ → 部分任務新增成功，但部分失敗了 :p";
    public const string NoTasksToAdd = "沒有要新增的任務";
    public const string LogSuccessShort = "任務清單更新 ♡ → 你的任務已記錄！做得好！🍀";
    public const string LogFailedShort = "❌ 記錄任務失敗";
    public const string LogPartialShort = "任務清單更新 ♡ → 部分任務記錄成功，但部分失敗了 :p";
    public const string NoTasksToLog = "沒有要記錄的任務";
    public const string RemoveAllSuccess = "任務清單更新 🚮 → 你的所有任務均已移除！";
    public const string RemoveMultipleSuccess = "任務清單更新 🚮 → 任務已成功移除！";
    public const string CompleteAllSuccess = "任務清單已更新！🎉 恭喜你完成了所有任務！";
    public const string CompleteMultipleSuccess = "任務清單已更新！🎉 恭喜你完成了這些任務！";
    public const string UndoneMultipleSuccess = "任務清單已更新 📝 — 任務已標記為未完成";
    public const string FocusedMarker = "(進行中) ";

    public static string TaskAlreadyExistsWithName(string taskName) => $"❌ 錯誤：任務 '{taskName}' 已存在";
    public static string AddSuccess(string addedTasks) => $"任務清單更新 ♡ → 任務：\"{addedTasks}\" 已新增！加油！🍀";
    public static string AddFailed(string failedTasks) => $"❌ 新增任務失敗：{failedTasks}";
    public static string AddPartial(string addedTasks, string failedTasks) => $"任務清單更新 ♡ → 已新增任務：\"{addedTasks}\" | 失敗：{failedTasks}";
    public static string LogSuccess(string loggedTasks) => $"任務清單更新 ♡ → 任務 {loggedTasks} 已記錄！做得好！🍀";
    public static string LogFailed(string failedTasks) => $"❌ 記錄任務失敗：{failedTasks}";
    public static string LogPartial(string loggedTasks, string failedTasks) => $"任務清單更新 ♡ → 已記錄任務：{loggedTasks} | 失敗：{failedTasks}";
    public static string RemoveSingleSuccess(string taskName) => $"任務清單更新 🚮 → 任務 '{taskName}' 已成功刪除！";
    public static string RemoveFailed(string failedTasks) => $"移除失敗：{failedTasks}";
    public static string CompleteSingleSuccess(string taskName) => $"任務清單已更新！🎉 恭喜完成任務 '{taskName}'！";
    public static string CompleteFailed(string failedTasks) => $"❌ 完成失敗：{failedTasks}";
    public static string UndoneSingleSuccess(string taskName) => $"任務清單已更新 📝 — 任務 {taskName} 已標記為未完成！";
    public static string UndoneFailed(string failedTasks) => $"❌ 標記為未完成失敗：{failedTasks}";
    public static string FocusedTask(int taskNumber, string taskName) => $"✅ 目前專注任務：{taskNumber}. {taskName}";
    public static string NextSuccess(string completedTaskName, int nextTaskNumber, string nextTaskName) => $"✅ 恭喜完成 '{completedTaskName}'！接下來進行 '({nextTaskNumber}) {nextTaskName}'";
    public static string EditSuccess(string oldTaskName, string newTaskName) => $"📝 任務 '{oldTaskName}' 已修改為 '{newTaskName}'";
    public static string PendingTaskItem(int taskNumber, string taskName, bool focused) => $"{taskNumber}. {(focused ? FocusedMarker : "")}{taskName}";
    public static string PendingTasks(int count, string tasks) => $"待辦任務 ({count})：{tasks}";
    public static string OtherUserPendingTasks(string username, int count, string tasks) => $"{username} 的任務 ({count})：{tasks}";
    public static string CompletedTaskItem(int taskNumber, string taskName) => $"{taskNumber}. {taskName}";
    public static string CompletedTasks(int count, string tasks) => $"已完成 {count} 項任務：{tasks}";
    public static string OtherUserCompletedTasks(string username, int count, string tasks) => $"{username} 的任務 ({count})：{tasks}";
    public static string FailedToRemoveTasks(string failedTasks) => $"❌ 移除任務失敗：{failedTasks}";
    public static string FailedToCompleteTasks(string failedTasks) => $"❌ 完成任務失敗：{failedTasks}";
    public static string FailedToUndoneTasks(string failedTasks) => $"❌ 標記未完成失敗：{failedTasks}";
    public static string OtherUserCount(string username, int completedCount) => $"{username} 目前已完成 {completedCount} 項任務！";
    public static string MyCount(int completedCount) => $"你目前已完成 {completedCount} 項任務！";
    public static string ClearOld(int count, int days) => $"📝 已清理 {count} 項 {days} 天或更早之前的任務！";
    public static string CountOld(int count, int days) => $"📝 共有 {count} 項 {days} 天或更早之前的任務。";
}

#endregion
```
