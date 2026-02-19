# Rython Task Bot v2

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
![Streamer.Bot](https://img.shields.io/badge/Streamer.Bot-%234285F4.svg?style=for-the-badge&logoColor=white)
![Twitch](https://img.shields.io/badge/twitch-8957E5?style=for-the-badge&logo=twitch&logoColor=white)
![YouTube](https://img.shields.io/badge/youtube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)

Task Bot for Co-working streamers on Twitch & YouTube.

## Why this Task Bot?

- Browser Source is optional
  - You only need Streamer.Bot running to have it working with Twitch chat
- Compatible with Twitch and YouTube; possibly extending to Kick and Trovo
- Task data is accessible via Streamer.Bot global variable
- Commands' permissions are easily configurable:
  - Mod (and Streamer) only
  - VIP and above
  - Subscribers and above
  - etc.
- Easy customizable with other Streamer.Bot actions

## Widget (Optional)

- [Task Widget Repository](https://github.com/liyunze-coding/task-bot-v2-widget)

## Setup Instructions

1. Install and setup [Streamer.Bot](https://streamer.bot/)
   - [Nutty's Reference Video](https://youtu.be/gfGy1gRH5ik?t=146) 

2. Open Streamer.Bot and click on Import

   > ![Import Button](./images/import-button.png)

3. Import the string from [RythonTaskBot.sb](./RythonTaskBot.sb)

    - Copy paste the string into the `Import` box

        OR

    - Drag the file into the `Import` box

    > ![Imported](./images/imported.png)

4. Click on the `Import` button

5. You may need to go to the Commands tab and enable the commands.

## Usage

(Work in progress)


### Commands

-   `!add [task]` : add the task to your list
-   `!focus (number / task)` : focus on ONE task
-   `!edit (number) [task]` : edit the task at the specified index
-   `!done (number / task)` : mark task as done
-   `!undone (number / task)` : mark task as incomplete
-   `!remove (number / task)` : remove task from list
-   `!unfocus` : unfocuses on your focused task
-   `!clearmydone` : Clears all your completed tasks

### Moderators only

-   `!clearall` - Clear all tasks
-   `!cleardone` - Clear all completed tasks
-   `!adel @user` - Remove all tasks from a user

## Credits

- [RythonDev](https://twitch.tv/RythonDev)
