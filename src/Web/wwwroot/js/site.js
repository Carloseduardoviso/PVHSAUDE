(() => {
    function ocultarMensagensDeSucesso() {
        document.querySelectorAll(".alert-success").forEach(message => {
            message.setAttribute("role", "status");
            window.setTimeout(() => {
                if (window.bootstrap?.Alert) {
                    window.bootstrap.Alert.getOrCreateInstance(message).close();
                } else {
                    message.remove();
                }
            }, 5000);
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", ocultarMensagensDeSucesso, { once: true });
    } else {
        ocultarMensagensDeSucesso();
    }
})();
