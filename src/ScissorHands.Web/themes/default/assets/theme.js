(() => {
  const navigation = document.querySelector(".site-header nav");
  if (!navigation) {
    return;
  }

  const toggles = [...navigation.querySelectorAll(".navigation-toggle")];

  function setExpanded(toggle, expanded) {
    const submenu = document.getElementById(toggle.getAttribute("aria-controls"));
    toggle.setAttribute("aria-expanded", String(expanded));
    submenu.hidden = !expanded;

    if (!expanded) {
      submenu.querySelectorAll(".navigation-toggle").forEach(child => {
        child.setAttribute("aria-expanded", "false");
        document.getElementById(child.getAttribute("aria-controls")).hidden = true;
      });
    }
  }

  function closeAll() {
    toggles.forEach(toggle => setExpanded(toggle, false));
  }

  toggles.forEach(toggle => {
    setExpanded(toggle, false);
    toggle.hidden = false;
    toggle.addEventListener("click", () => {
      const expanded = toggle.getAttribute("aria-expanded") !== "true";
      const siblings = toggle.closest("li").parentElement.querySelectorAll(":scope > li > .navigation-link > .navigation-toggle");
      siblings.forEach(sibling => {
        if (sibling !== toggle) {
          setExpanded(sibling, false);
        }
      });
      setExpanded(toggle, expanded);
    });
  });
  navigation.dataset.navigationEnhanced = "true";

  navigation.addEventListener("keydown", event => {
    if (event.key !== "Escape") {
      return;
    }

    const toggle = event.target.closest(".navigation-item")?.querySelector(":scope > .navigation-link > .navigation-toggle[aria-expanded='true']")
      ?? event.target.closest(".navigation-children")?.parentElement.querySelector(":scope > .navigation-link > .navigation-toggle");
    if (toggle) {
      event.preventDefault();
      setExpanded(toggle, false);
      toggle.focus();
    }
  });

  navigation.addEventListener("focusout", event => {
    if (event.relatedTarget && !navigation.contains(event.relatedTarget)) {
      closeAll();
    }
  });

  window.addEventListener("blur", closeAll);

  document.addEventListener("click", event => {
    if (!navigation.contains(event.target)) {
      closeAll();
    }
  });
})();

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
