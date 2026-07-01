// Task list view

class TaskList {
	#data = [];
	#firstRender = true;
	#pendingRemovals = 0;

	#scroll = {
		offset: 0,
		contentW: 0,
		viewportW: 0,
		speed: 50,
		raf: null,
		lastT: 0,
		active: false,
		wantStop: false,
	};

	#els = {};

	#ANIM_DURATION = 400;
	#ANIM_EASING = "cubic-bezier(0.22, 1, 0.36, 1)";

	#parseHexColor(hex) {
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

	#relativeLuminance({ r, g, b }) {
		const toLinear = (v) => {
			const s = v / 255;
			return s <= 0.04045
				? s / 12.92
				: Math.pow((s + 0.055) / 1.055, 2.4);
		};
		const R = toLinear(r);
		const G = toLinear(g);
		const B = toLinear(b);
		return 0.2126 * R + 0.7152 * G + 0.0722 * B;
	}

	constructor(container) {
		const el =
			typeof container === "string"
				? document.querySelector(container)
				: container;

		this.#els = {
			track: el.querySelector(".scroll-track"),
			content: el.querySelector(".scroll-content"),
			viewport: el.querySelector(".scroll-viewport"),
		};
	}

	// ── public API ─────────────────────────────────────

	load(sections) {
		this.#data = structuredClone(sections);
		this.#render();
		return this;
	}

	addSection(section) {
		this.#data.push(structuredClone(section));
		this.#render();
		return this;
	}

	removeSection(id) {
		const i = this.#data.findIndex((s) => s.id === id);
		if (i === -1) return this;
		this.#data.splice(i, 1);
		this.#render();
		return this;
	}

	updateSection(id, updater) {
		const section = this.#data.find((s) => s.id === id);
		if (!section) return this;
		updater(section);
		this.#render();
		return this;
	}

	addTask(sectionId, task, sectionTitle) {
		if (!this.#data.find((s) => s.id === sectionId)) {
			this.addSection({
				id: sectionId,
				title: sectionTitle || sectionId,
				tasks: [],
			});
		}

		return this.updateSection(sectionId, (s) => {
			s.tasks.push(structuredClone(task));
		});
	}

	removeTask(sectionId, taskIndex) {
		this.updateSection(sectionId, (s) => {
			s.tasks.splice(taskIndex, 1);
		});
		this.#data = this.#data.filter((s) => s.tasks.length > 0);
		this.#render();
		return this;
	}

	editTask(sectionId, taskIndex, newText, userColor) {
		return this.updateSection(sectionId, (s) => {
			if (s.tasks[taskIndex]) {
				s.tasks[taskIndex].text = newText;
			}
		});
	}

	doneTask(sectionId, taskIndex) {
		return this.updateSection(sectionId, (s) => {
			if (s.tasks[taskIndex]) {
				s.tasks[taskIndex].done = true;
				s.tasks[taskIndex].focused = false;
			}
		});
	}

	undoneTask(sectionId, taskIndex) {
		return this.updateSection(sectionId, (s) => {
			if (s.tasks[taskIndex]) {
				s.tasks[taskIndex].done = false;
			}
		});
	}

	focusTask(sectionId, taskIndex) {
		// unfocus any previously focused task
		this.unfocusTask(sectionId);

		return this.updateSection(sectionId, (s) => {
			if (s.tasks[taskIndex]) {
				s.tasks[taskIndex].focused = true;
			}
		});
	}

	unfocusTask(sectionId) {
		return this.updateSection(sectionId, (s) => {
			for (let i = 0; i < s.tasks.length; i++) {
				s.tasks[i].focused = false;
			}
		});
	}

	clearmydone(sectionId) {
		this.updateSection(sectionId, (s) => {
			s.tasks = s.tasks.filter((t) => !t.done);
		});
		this.#data = this.#data.filter((s) => s.tasks.length > 0);
		this.#render();
		return this;
	}

	cleardone() {
		this.#data.forEach((s) => {
			s.tasks = s.tasks.filter((t) => !t.done);
		});
		this.#data = this.#data.filter((s) => s.tasks.length > 0);
		this.#render();
		return this;
	}

	getData() {
		return structuredClone(this.#data);
	}

	get sectionCount() {
		return this.#data.length;
	}

	get taskCount() {
		return this.#data.reduce((sum, s) => sum + s.tasks.length, 0);
	}

	get doneCount() {
		return this.#data.reduce(
			(sum, s) => sum + s.tasks.filter((t) => t.done).length,
			0,
		);
	}

	destroy() {
		this.#forceStop();
		this.#els.content.innerHTML = "";
		this.#data = [];
		this.#firstRender = true;
	}

	// ── render ─────────────────────────────────────────

	#render() {
		if (this.#firstRender) {
			this.#data.forEach((s) =>
				this.#els.content.appendChild(this.#createSectionEl(s)),
			);
			this.#firstRender = false;
		} else {
			this.#patchContainer(this.#els.content, this.#data, true);
		}

		if (this.#pendingRemovals === 0) {
			this.#syncScroll();
		}
	}

	// ── animations ─────────────────────────────────────

	#animateIn(el) {
		el.style.overflow = "hidden";

		const w = el.scrollWidth;
		el.style.width = "0px";
		el.style.opacity = "0";

		requestAnimationFrame(() => {
			const anim = el.animate(
				[
					{
						width: "0px",
						opacity: 0,
						transform: "translateX(-8px)",
					},
					{
						width: w + "px",
						opacity: 1,
						transform: "translateX(0)",
					},
				],
				{
					duration: this.#ANIM_DURATION,
					easing: this.#ANIM_EASING,
					fill: "forwards",
				},
			);

			anim.onfinish = () => {
				el.style.width = "";
				el.style.opacity = "";
				el.style.overflow = "";
				el.style.transform = "";
				anim.cancel();
				this.#syncScroll();
			};
		});
	}

	#animateRemove(el) {
		this.#pendingRemovals++;
		const w = el.scrollWidth;

		const anim = el.animate(
			[
				{ width: w + "px", opacity: 1, transform: "translateX(0)" },
				{ width: "0px", opacity: 0, transform: "translateX(-8px)" },
			],
			{
				duration: this.#ANIM_DURATION,
				easing: this.#ANIM_EASING,
				fill: "forwards",
			},
		);

		anim.onfinish = () => {
			el.remove();
			this.#pendingRemovals--;

			if (this.#pendingRemovals === 0) {
				this.#syncScroll();
			}
		};
	}

	#animateStrikethrough(el) {
		el.animate(
			[
				{ opacity: 0.5, transform: "scale(0.98)" },
				{ opacity: 1, transform: "scale(1)" },
			],
			{ duration: 250, easing: "ease-out" },
		);
	}

	// ── dom helpers ────────────────────────────────────

	#parseTaskText(text) {
		if (window.emoteManager && window.emoteManager.loaded) {
			return window.emoteManager.parseText(text);
		}
		return text;
	}

	#createTaskEl(task, index) {
		const div = document.createElement("div");
		div.className =
			"task" +
			(task.done ? " done" : "") +
			(task.focused ? " focused" : "");
		div.dataset.text = task.text;
		const numberSpan = document.createElement("span");
		numberSpan.className = "task-number";
		numberSpan.textContent = `${index + 1}.`;

		const textSpan = document.createElement("span");
		textSpan.className = "task-text";
		textSpan.innerHTML = this.#parseTaskText(task.text);

		div.replaceChildren(numberSpan, textSpan);
		return div;
	}

	#createSectionEl(section) {
		const div = document.createElement("div");
		div.className = "section";
		div.dataset.key = section.id;
		if (section.title) {
			const t = document.createElement("div");
			t.className = "section-title";
			t.textContent = section.title;
			let color =
				section.color ?? localStorage.getItem(`${section.id}-color`);
			if (
				configs.twitchSettings.autoUserColor &&
				color != undefined &&
				color != "undefined" &&
				color != null
			) {
				t.style.setProperty("--user-color", color);
				t.classList.add("has-user-color");
			}

			div.appendChild(t);
		}
		section.tasks.forEach((task, i) =>
			div.appendChild(this.#createTaskEl(task, i)),
		);
		return div;
	}

	// ── patching ───────────────────────────────────────

	#patchTasks(sectionEl, tasks, animate) {
		const existing = [...sectionEl.querySelectorAll(":scope > .task")];

		tasks.forEach((task, i) => {
			if (i < existing.length) {
				const el = existing[i];
				const wantClass =
					"task" +
					(task.done ? " done" : "") +
					(task.focused ? " focused" : "");
				if (el.className !== wantClass) {
					el.className = wantClass;
					if (animate) this.#animateStrikethrough(el);
				}
				if (el.dataset.text !== task.text) {
					el.dataset.text = task.text;
					const textEl = el.querySelector(".task-text") || el.querySelector("span:last-child");
					textEl.innerHTML = this.#parseTaskText(task.text);
				}
				el.querySelector(".task-number").textContent = `${i + 1}.`;

				// Update color if changed
				if (task.color != undefined && task.color != "undefined") {
					el.style.setProperty("--user-color", task.color);
					el.classList.add("has-user-color");
				} else {
					el.style.removeProperty("--user-color");
					el.classList.remove("has-user-color");
				}
			} else {
				const newEl = this.#createTaskEl(task, i);
				sectionEl.appendChild(newEl);
				if (animate) this.#animateIn(newEl);
			}
		});

		for (let i = existing.length - 1; i >= tasks.length; i--) {
			if (animate) {
				this.#animateRemove(existing[i]);
			} else {
				existing[i].remove();
			}
		}
	}

	#patchContainer(container, sections, animate) {
		const oldMap = new Map();
		container.querySelectorAll(":scope > .section").forEach((el) => {
			oldMap.set(el.dataset.key, el);
		});

		let cursor = container.firstElementChild;

		sections.forEach((section) => {
			const key = section.id;
			const existing = oldMap.get(key);

			if (existing) {
				this.#patchTasks(existing, section.tasks, animate);
				if (existing !== cursor) {
					container.insertBefore(existing, cursor);
				} else {
					cursor = cursor.nextElementSibling;
				}
				oldMap.delete(key);
			} else {
				const newEl = this.#createSectionEl(section);
				container.insertBefore(newEl, cursor);
				if (animate) this.#animateIn(newEl);
			}
		});

		oldMap.forEach((el) => {
			if (animate) {
				this.#animateRemove(el);
			} else {
				el.remove();
			}
		});
	}

	// ── scroll ─────────────────────────────────────────

	#tick = (now) => {
		const s = this.#scroll;
		const dt = (now - s.lastT) / 1000;
		s.lastT = now;

		if (dt < 0.2) {
			s.contentW = this.#els.content.scrollWidth;
			s.offset += s.speed * dt;

			if (s.contentW > 0 && s.offset >= s.contentW) {
				s.offset -= s.contentW;

				if (s.wantStop) {
					s.wantStop = false;
					this.#forceStop();
					return;
				}
			}

			this.#els.track.style.transform = `translateX(${-s.offset}px)`;
		}

		s.raf = requestAnimationFrame(this.#tick);
	};

	#startScroll() {
		const s = this.#scroll;
		s.wantStop = false;
		if (s.active) return;
		s.active = true;
		s.lastT = performance.now();
		s.raf = requestAnimationFrame(this.#tick);
	}

	#requestStop() {
		if (!this.#scroll.active) return;
		this.#scroll.wantStop = true;
	}

	#forceStop() {
		const s = this.#scroll;
		s.active = false;
		s.wantStop = false;
		cancelAnimationFrame(s.raf);
		s.raf = null;
		s.offset = 0;
		this.#els.track.style.transform = "";

		const clone = this.#els.track.querySelector(".scroll-clone");
		if (clone) clone.remove();
	}

	#syncScroll() {
		const s = this.#scroll;
		s.contentW = this.#els.content.scrollWidth;
		s.viewportW = this.#els.viewport.clientWidth;
		const needs = s.contentW > s.viewportW;

		let clone = this.#els.track.querySelector(".scroll-clone");

		if (needs) {
			if (!clone) {
				clone = this.#els.content.cloneNode(true);
				clone.id = "";
				clone.classList.add("scroll-clone");
				this.#els.track.appendChild(clone);
			} else {
				this.#patchContainer(clone, this.#data, false);
			}
			if (s.offset >= s.contentW) {
				s.offset %= s.contentW;
			}
			this.#startScroll();
		} else {
			if (s.active) {
				if (clone) this.#patchContainer(clone, this.#data, false);
				this.#requestStop();
			} else {
				if (clone) clone.remove();
			}
		}
	}
}

// ── usage ──────────────────────────────────────────────

// const list2 = new TaskList(".task-panel");

// list2.load([
// 	{
// 		title: "someone",
// 		tasks: [
// 			{ text: "dinner" },
// 			{ text: "watch bad bunny halftime show" },
// 			{ text: "tie tf down", done: true },
// 			{ text: "boy dinner", done: true },
// 			{ text: "3 hour SOOP" },
// 		],
// 	},
// 	{
// 		title: "Cape_Codder",
// 		tasks: [
// 			{ text: "Part 2", done: true },
// 			{ text: "Ch 1-2", done: true },
// 		],
// 	},
// 	{
// 		title: "sunflawer",
// 		tasks: [
// 			{ text: "fix walls from home office" },
// 			{ text: "sand walls zzzsdkjhfsk", done: true },
// 			{ text: "meal prep" },
// 			{ text: "buy wrist brace maybe" },
// 		],
// 	},
// 	{
// 		title: "extra_user1",
// 		tasks: [
// 			{ text: "go grocery shopping" },
// 			{ text: "clean the kitchen", done: true },
// 			{ text: "respond to emails" },
// 		],
// 	},
// ]);

// demo

// setTimeout(() => {
// 	list2.addTask("someone2", { text: "new task just dropped" });
// }, 1000);

// setTimeout(() => {
// 	list2.;
// }, 3000);

// setTimeout(() => {
// 	list.toggleTask("sunflawer", 0);
// }, 10000);

// setTimeout(() => {
// 	list.removeSection("Cape_Codder");
// }, 12000);

// setTimeout(() => {
// 	list.removeTask("extra_user1", 1);
// }, 14000);
