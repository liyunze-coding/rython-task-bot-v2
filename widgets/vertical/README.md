# Widget - Rython Task Bot v2

Optional widget for [Rython Task Bot v2](https://github.com/liyunze-coding/rython-task-bot-v2)

## Instructions

Websocket server running is required for browser source to work. 

> ![websocket](./images/websocket.png)

1. Streamer.Bot -> Servers/Clients -> Websocket Server
2. Auto Start: `ON`; Click on `Start Server`
3. If port number already taken, change port number
4. Ensure that [config.js](./config.js)'s websocket settings are in-sync with Streamer.Bot's 

## Style Customisation 

Note: `styles/structure.css` is meant to be the bare structure to allow the task widget to be functional.

Customise the styles in `styles/style.css`, whether it's through the CSS variables (in `:root`), or the CSS below it.

### Fonts

You can import Google fonts into your style.css:

1. Open [Google Fonts](https://fonts.google.com/)
2. Select your desired fonts
3. Click on `View selected families`
4. Click on `Get Embed Code`
5. Select `@import` under Web
6. Copy the import `@import url('...');`
7. You can use it in your CSS already! 

```css
... {
	font-family: "Fredoka";
    /* ... */
}
```

---