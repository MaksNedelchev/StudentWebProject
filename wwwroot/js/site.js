const themeToggle = document.getElementById("themeToggle");

function applyTheme(theme) {
    document.documentElement.setAttribute("data-theme", theme);
    localStorage.setItem("theme", theme);

    if (themeToggle) {
        themeToggle.textContent = theme === "dark" ? "Light mode" : "Dark mode";
        themeToggle.setAttribute("aria-pressed", theme === "dark" ? "true" : "false");
    }
}

applyTheme(localStorage.getItem("theme") || "light");

themeToggle?.addEventListener("click", () => {
    const currentTheme = document.documentElement.getAttribute("data-theme") || "light";
    applyTheme(currentTheme === "dark" ? "light" : "dark");
});
