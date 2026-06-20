"use strict";

const configs = (function () {
	const streamerBotSettings = {
		host: "127.0.0.1",
		port: 8080,
		endpoint: "/",
	};

	const commands = [
		"!taskhelp",
		"!add",
		"!edit",
		"!done",
		"!remove",
		"!focus",
		"!clearmydone",
	];

	const twitchSettings = {
		autoUserColor: true
	}

	const kickSettings = {
		autouserColor: true
	}

	return {
		streamerBotSettings,
		commands,
		twitchSettings,
		kickSettings
	};
})();
