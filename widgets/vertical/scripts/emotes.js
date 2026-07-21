"use strict";

class EmoteManager {
	#emotes = new Map();
	#channelName = "";
	#channelId = "";
	#providers = ["7tv", "bttv", "ffz"];
	#size = "1x";
	#loaded = false;
	#cacheTTL = 24 * 60 * 60 * 1000;
	#CACHE_KEY = "emote-cache-v1";
	#pattern = null;
	#patternSize = 0;

	constructor(config = {}) {
		this.#channelName = config.channelName || "";
		this.#channelId = config.channelId || "";
		if (config.providers) this.#providers = config.providers;
		if (config.size) this.#size = config.size;
	}

	get loaded() {
		return this.#loaded;
	}

	get emoteCount() {
		return this.#emotes.size;
	}

	get providers() {
		return [...this.#providers];
	}

	getEmote(name) {
		return this.#emotes.get(name) || null;
	}

	#buildPattern() {
		if (this.#emotes.size === 0) {
			this.#pattern = null;
			this.#patternSize = 0;
			return;
		}

		if (this.#pattern && this.#emotes.size === this.#patternSize) return;

		const sorted = [...this.#emotes.keys()].sort(
			(a, b) => b.length - a.length,
		);
		const escaped = sorted.map((n) =>
			n.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"),
		);
		this.#pattern = new RegExp(
			`(?<![\\w])(?:${escaped.join("|")})(?![\\w])`,
			"g",
		);
		this.#patternSize = this.#emotes.size;
	}

	parseText(text) {
		if (!text || !this.#loaded || this.#emotes.size === 0) return text;

		this.#buildPattern();
		if (!this.#pattern) return text;

		return text.replace(this.#pattern, (match) => {
			const emote = this.#emotes.get(match);
			if (!emote) return match;
			console.log(emote);
			return `<img src="${emote.url}" alt="${match}" title="${match} (${emote.provider})" class="emote-img" />`;
		});
	}

	async init() {
		const cached = this.#loadCache();
		if (cached) {
			this.#emotes = new Map(Object.entries(cached.emotes));
			this.#loaded = true;
			this.#patternSize = 0;
			return;
		}

		await this.#fetchAll();
		this.#loaded = true;
		this.#patternSize = 0;
		this.#saveCache();
	}

	async forceRefresh() {
		this.#emotes.clear();
		this.#patternSize = 0;
		await this.#fetchAll();
		this.#loaded = true;
		this.#patternSize = 0;
		this.#saveCache();
	}

	async #fetchAll() {
		const fetchers = [];
		if (this.#providers.includes("7tv")) fetchers.push(this.#fetch7TV());
		if (this.#providers.includes("bttv")) fetchers.push(this.#fetchBTTV());
		if (this.#providers.includes("ffz")) fetchers.push(this.#fetchFFZ());
		await Promise.allSettled(fetchers);
	}

	async #fetch7TV() {
		try {
			const globalResp = await fetch(
				"https://7tv.io/v3/emote-sets/global",
			);
			if (!globalResp.ok) return;
			const globalData = await globalResp.json();
			if (globalData.emotes) {
				for (const emote of globalData.emotes) {
					this.#add7TVEmote(emote);
				}
			}

			if (this.#channelId) {
				const userResp = await fetch(
					`https://7tv.io/v3/users/twitch/${this.#channelId}`,
				);
				if (!userResp.ok) return;
				const userData = await userResp.json();
				if (userData.emote_set?.emotes) {
					for (const emote of userData.emote_set.emotes) {
						this.#add7TVEmote(emote);
					}
				}
			}
		} catch (e) {
			console.warn("7TV emote fetch failed:", e.message);
		}
	}

	#add7TVEmote(emote) {
		const entry = emote.data || emote;
		const id = entry.id || emote.id;
		const name = entry.name || emote.name;
		if (!id || !name) return;
		const nameLower = name;
		const host = entry.host.url;
		this.#emotes.set(nameLower, {
			url: `https:${host}/${this.#size}.webp`,
			provider: "7tv",
			animated: entry.animated || false,
		});
	}

	async #fetchBTTV() {
		try {
			const globalResp = await fetch(
				"https://api.betterttv.net/3/cached/emotes/global",
			);
			if (!globalResp.ok) return;
			const globalData = await globalResp.json();
			if (Array.isArray(globalData)) {
				for (const emote of globalData) {
					this.#emotes.set(emote.code, {
						url: `https://cdn.betterttv.net/emote/${emote.id}/${this.#size}`,
						provider: "bttv",
						animated: emote.imageType === "gif",
					});
				}
			}

			if (this.#channelId) {
				const userResp = await fetch(
					`https://api.betterttv.net/3/cached/users/twitch/${this.#channelId}`,
				);
				if (!userResp.ok) return;
				const userData = await userResp.json();
				const allEmotes = [
					...(userData.channelEmotes || []),
					...(userData.sharedEmotes || []),
				];
				for (const emote of allEmotes) {
					this.#emotes.set(emote.code, {
						url: `https://cdn.betterttv.net/emote/${emote.id}/${this.#size}`,
						provider: "bttv",
						animated: emote.imageType === "gif",
					});
				}
			}
		} catch (e) {
			console.warn("BTTV emote fetch failed:", e.message);
		}
	}

	async #fetchFFZ() {
		try {
			const globalResp = await fetch(
				"https://api.frankerfacez.com/v1/set/global",
			);
			if (!globalResp.ok) return;
			const globalData = await globalResp.json();
			if (globalData.sets) {
				for (const setId in globalData.sets) {
					const set = globalData.sets[setId];
					for (const emote of set.emoticons || []) {
						if (emote.modifier) continue;
						this.#emotes.set(emote.name, {
							url: emote.urls[this.#toFFZSize(this.#size)],
							provider: "ffz",
							animated: false,
						});
					}
				}
			}

			const ffzId = this.#channelName || this.#channelId;
			if (ffzId) {
				const roomUrl = this.#channelId
					? `https://api.frankerfacez.com/v1/room/id/${this.#channelId}`
					: `https://api.frankerfacez.com/v1/room/${this.#channelName}`;
				const roomResp = await fetch(roomUrl);
				if (!roomResp.ok) return;
				const roomData = await roomResp.json();
				if (roomData.sets) {
					for (const setId in roomData.sets) {
						const set = roomData.sets[setId];
						for (const emote of set.emoticons || []) {
							if (emote.modifier) continue;
							this.#emotes.set(emote.name, {
								url: emote.urls[this.#toFFZSize(this.#size)],
								provider: "ffz",
								animated: false,
							});
						}
					}
				}
			}
		} catch (e) {
			console.warn("FFZ emote fetch failed:", e.message);
		}
	}

	#toFFZSize(size) {
		switch (size) {
			case "3x":
			case "4x":
				return "4";
			case "2x":
				return "2";
			default:
				return "1";
		}
	}

	#saveCache() {
		try {
			const cache = {
				timestamp: Date.now(),
				channelId: this.#channelId,
				emotes: Object.fromEntries(this.#emotes),
			};
			localStorage.setItem(this.#CACHE_KEY, JSON.stringify(cache));
		} catch (e) {}
	}

	#loadCache() {
		try {
			const raw = localStorage.getItem(this.#CACHE_KEY);
			if (!raw) return null;
			const cache = JSON.parse(raw);
			if (cache.channelId !== this.#channelId) return null;
			if (Date.now() - cache.timestamp > this.#cacheTTL) return null;
			return cache;
		} catch (e) {
			return null;
		}
	}
}
