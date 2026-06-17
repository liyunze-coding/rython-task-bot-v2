(function () {
	const commands = configs.commands;

	const titleEl = document.getElementById("title");
	let currentIndex = 0;

	const DISPLAY_DURATION = 3000; // how long each command stays visible
	const FADE_DURATION = 400; // fade in/out duration in ms

	function applyTransition() {
		titleEl.style.transition = `opacity ${FADE_DURATION}ms ease-in-out`;
	}

	function fadeOut() {
		return new Promise((resolve) => {
			titleEl.style.opacity = "0";
			setTimeout(resolve, FADE_DURATION);
		});
	}

	function fadeIn() {
		return new Promise((resolve) => {
			titleEl.style.opacity = "1";
			setTimeout(resolve, FADE_DURATION);
		});
	}

	async function cycle() {
		await fadeOut();

		currentIndex = (currentIndex + 1) % commands.length;
		titleEl.textContent = commands[currentIndex];

		await fadeIn();

		setTimeout(cycle, DISPLAY_DURATION);
	}

	applyTransition();
	titleEl.textContent = commands[0];
	setTimeout(cycle, DISPLAY_DURATION);
})();
