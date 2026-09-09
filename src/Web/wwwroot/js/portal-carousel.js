(() => {
    const element = document.getElementById("rede-carousel");
    document.querySelectorAll(".rede-card img").forEach(img => {
        const hide = () => { img.hidden = true; };
        img.addEventListener("error", hide);
        if (img.complete && !img.naturalWidth) hide();
    });
    if (!element || element.querySelectorAll(".carousel-item").length < 2) return;
    const carousel = new bootstrap.Carousel(element, { interval: 5000, wrap: true, pause: false, touch: true });
    const button = document.getElementById("rede-pausar");
    let paused = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    function update() {
        button.textContent = paused ? "Continuar" : "Pausar";
        button.setAttribute("aria-pressed", String(paused));
        paused ? carousel.pause() : carousel.cycle();
    }
    button.addEventListener("click", () => { paused = !paused; update(); });
    element.addEventListener("mouseenter", () => carousel.pause());
    element.addEventListener("mouseleave", () => { if (!paused) carousel.cycle(); });
    element.addEventListener("focusin", () => carousel.pause());
    element.addEventListener("focusout", event => { if (!paused && !element.contains(event.relatedTarget)) carousel.cycle(); });
    update();
})();
