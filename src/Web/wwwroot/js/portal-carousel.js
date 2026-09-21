(() => {
    document.querySelectorAll(".rede-card img").forEach(img => {
        const hide = () => { img.hidden = true; };
        img.addEventListener("error", hide);
        if (img.complete && !img.naturalWidth) hide();
    });

    document.querySelectorAll(".banner-carousel, #rede-carousel").forEach(element => {
        const slides = element.querySelectorAll(".carousel-item");
        const alwaysControls = element.hasAttribute("data-always-controls");
        if (slides.length < 2 && !alwaysControls) return;
        const carousel = slides.length > 1 ? new bootstrap.Carousel(element, {
            interval: 8000, wrap: true, pause: false, touch: true
        }) : null;
        const controls = document.createElement("div");
        controls.className = "portal-carousel-controls";
        controls.setAttribute("role", "group");
        controls.setAttribute("aria-label", "Controles de " + element.getAttribute("aria-label"));
        const arrow = (label, symbol, action) => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "portal-carousel-arrow";
            button.setAttribute("aria-label", label);
            button.setAttribute("aria-controls", element.id);
            button.textContent = symbol;
            if (carousel) button.addEventListener("click", action);
            else button.disabled = true;
            return button;
        };
        controls.append(arrow("Slide anterior", "‹", () => carousel.prev()));
        const indicators = document.createElement("div");
        indicators.className = "portal-carousel-indicators";
        const dots = Array.from(slides, (_, index) => {
            const dot = document.createElement("button");
            dot.type = "button";
            dot.setAttribute("aria-label", "Ir para slide " + (index + 1));
            dot.setAttribute("aria-controls", element.id);
            if (carousel) dot.addEventListener("click", () => carousel.to(index));
            indicators.append(dot);
            return dot;
        });
        const select = index => dots.forEach((dot, i) => {
            dot.classList.toggle("active", i === index);
            if (i === index) dot.setAttribute("aria-current", "true");
            else dot.removeAttribute("aria-current");
        });
        select(Array.from(slides).findIndex(slide => slide.classList.contains("active")));
        if (carousel) element.addEventListener("slid.bs.carousel", event => select(event.to));
        controls.append(indicators, arrow("Próximo slide", "›", () => carousel.next()));
        element.after(controls);

        if (!carousel) return;

        const paused = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        const region = element.closest("section") || element;
        let hovering = false;
        let focusing = false;
        function update() {
            if (paused || hovering || focusing) carousel.pause();
            else carousel.cycle();
        }
        region.addEventListener("mouseenter", () => { hovering = true; update(); });
        region.addEventListener("mouseleave", () => { hovering = false; update(); });
        region.addEventListener("focusin", () => { focusing = true; update(); });
        region.addEventListener("focusout", event => {
            focusing = region.contains(event.relatedTarget);
            update();
        });
        update();
    });
})();
