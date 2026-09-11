(() => {
  const root = document.documentElement;
  const toggle = document.querySelector("#theme-toggle");
  const media = window.matchMedia("(prefers-color-scheme: dark)");

  function applyTheme(theme, persist) {
    root.dataset.theme = theme;
    toggle?.setAttribute("aria-label", `Switch to ${theme === "dark" ? "light" : "dark"} theme`);

    if (persist) {
      localStorage.setItem("theme", theme);
    }

    const background = getComputedStyle(document.body).backgroundColor;
    document.querySelector("meta[name='theme-color']")?.setAttribute("content", background);
  }

  toggle?.addEventListener("click", () => {
    applyTheme(root.dataset.theme === "dark" ? "light" : "dark", true);
  });

  media.addEventListener("change", event => {
    if (!localStorage.getItem("theme")) {
      applyTheme(event.matches ? "dark" : "light", false);
    }
  });

  applyTheme(root.dataset.theme ?? (media.matches ? "dark" : "light"), false);
})();
