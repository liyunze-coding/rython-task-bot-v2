// STREAMER.BOT SETTINGS

const client = new StreamerbotClient({
	host: configs.streamerBotSettings.host,
	port: configs.streamerBotSettings.port,
	endpoint: configs.streamerBotSettings.endpoint,
	onConnect: onConnect,
	onDisconnect: onDisconnect,
	onError: onError,
});

function getAllLocalstorage() {
	const allLocalStorage = {};

	for (let i = 0; i < localStorage.length; i++) {
		const key = localStorage.key(i);
		allLocalStorage[key] = localStorage.getItem(key);
	}

	return JSON.stringify(allLocalStorage);
}

client.on("General.Custom", (data) => onCustom(data));
if (configs.twitchSettings.autoUserColor) {
	client.on("Twitch.ChatMessage", (data) => onTwitchFirstWord(data));
}

let taskList;
let userColors = {};

function parseHexColor(hex) {
	if (typeof hex !== "string") return null;
	const s = hex.trim();
	const m = /^#?([0-9a-f]{3}|[0-9a-f]{6})$/i.exec(s);
	if (!m) return null;
	let h = m[1].toLowerCase();
	if (h.length === 3) {
		h = h
			.split("")
			.map((c) => c + c)
			.join("");
	}
	const r = parseInt(h.slice(0, 2), 16);
	const g = parseInt(h.slice(2, 4), 16);
	const b = parseInt(h.slice(4, 6), 16);
	return { r, g, b };
}

function relativeLuminance({ r, g, b }) {
	const toLinear = (v) => {
		const s = v / 255;
		return s <= 0.04045 ? s / 12.92 : Math.pow((s + 0.055) / 1.055, 2.4);
	};
	const R = toLinear(r);
	const G = toLinear(g);
	const B = toLinear(b);
	return 0.2126 * R + 0.7152 * G + 0.0722 * B;
}

async function onTwitchFirstWord(data) {
	let userColor = data.data.user.color; // hex colour #FF69B4

	

	if (userColor != undefined || userColor != 'undefined') {
		// update localstorage
		// get id: platform-userID
		let userId = data.data.user.id;
		let key = `twitch-${userId}`;

		localStorage.setItem(`${key}-color`, userColor);
		userColors[`${key}-color`] = userColor;
	}
	
	return;
}

function onDisconnect() {
	showConnectionError("Connection Failed: Unable to connect to Streamer.bot");
}

function onError(err) {
	showConnectionError(
		"Connection Failed: " + (err?.message || "Unknown error"),
	);
}

function showConnectionError(message) {
	// Remove existing popup if any
	const existing = document.getElementById("connection-error");
	if (existing) existing.remove();

	const popup = document.createElement("div");
	popup.id = "connection-error";
	popup.textContent = message;
	Object.assign(popup.style, {
		position: "fixed",
		top: "20px",
		left: "50%",
		transform: "translateX(-50%)",
		background: "#e53935",
		color: "#fff",
		padding: "12px 24px",
		borderRadius: "8px",
		fontFamily: "'Fredoka', sans-serif",
		fontSize: "1.1rem",
		fontWeight: "700",
		zIndex: "9999",
		boxShadow: "0 4px 12px rgba(0,0,0,0.4)",
		textAlign: "center",
	});
	document.body.appendChild(popup);

	// Auto-dismiss after 5 seconds
	setTimeout(() => {
		popup.animate([{ opacity: 1 }, { opacity: 0 }], {
			duration: 300,
			fill: "forwards",
		}).onfinish = () => popup.remove();
	}, 5000);
}

async function refresh() {
	const response = await client.getGlobal("rython-task-bot", true);
	console.log(response);

	if (response.status !== "ok" || !response.variable?.value) return;

	const users = JSON.parse(response.variable.value);
	const sections = transformToSections(users);

	taskList.load(sections);
}

// LOAD TASK LIST
async function onConnect() {
	taskList = new TaskList(".task-panel");
	refresh();
}

function transformToSections(users) {
	return Object.entries(users).map(([userId, userData]) => ({
		id: userId,
		title: userData.Username,
		tasks: userData.Tasks.map((task) => ({
			text: task.Name,
			done: task.Completed,
			focused: task.Focused,
		})),
	}));
}

// Update task list action by action
function onCustom(payload) {
	const data = payload.data;
	if (!data.source && data.source != "rython-task-bot") {
		return;
	}
	if (!taskList) return;
	const body = data.body;
	const id = data.id;
	const username = data.username;

	switch (body.mode) {
		case "add":
			// body.task;
			let taskPayload = {
				text: body.task,
				done: body.completed,
				focused: body.focused,
			};

			let usernameColor =
				userColors[id] ?? localStorage.getItem(`${id}-color`);

			if (configs.twitchSettings.autoUserColor && usernameColor != undefined && usernameColor != null) {
				taskPayload["color"] = usernameColor;
			}

			taskList.addTask(id, taskPayload, username);
			break;
		case "focus":
			taskList.focusTask(id, body.index, usernameColor);
			break;
		case "edit":
			taskList.editTask(id, body.index, body.task, usernameColor);
			break;
		case "remove":
			taskList.removeTask(id, body.index, usernameColor);
			break;
		case "done":
			taskList.doneTask(id, body.index, usernameColor);
			break;
		case "admindelete":
			taskList.removeSection(body.id);
			break;
		case "clearns":
			refresh();
			break;
		case "cleardone":
			taskList.cleardone();
			break;
		case "clearmydone":
			taskList.clearmydone(id);
			break;
		case "clearall":
			refresh();
			break;
		case "undone":
			taskList.undoneTask(id, body.index);
			break;
		case "unfocus":
			taskList.unfocusTask(id);
			break;
		case "admindelete":
			taskList.removeSection(body.id);
			break;
		default:
			break;
	}
}
