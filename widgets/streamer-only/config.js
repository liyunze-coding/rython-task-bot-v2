"use strict";

const configs = (function () {
	const streamerBotSettings = {
		host: "127.0.0.1",
		port: 6968,
		endpoint: "/",
	};

	const username = "RythonDev";

	const commands = [
		"!taskhelp",
		"!add",
		"!edit",
		"!done",
		"!remove",
		"!focus",
		"!clearmydone",
	];

	const userColorSettings = {
		autoUserColor: true,
	};

	const emoteSettings = {
		enabled: true,
		providers: ["7tv", "bttv", "ffz"],
		size: "1x",
		cacheHours: 24,
	};

	return {
		username,
		streamerBotSettings,
		commands,
		userColorSettings,
		emoteSettings,
	};
})();
