(() => {
    const pendingForms = new WeakSet();

    document.addEventListener('submit', async (event) => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || !form.matches('[data-swal-confirm]')) return;

        event.preventDefault();
        if (pendingForms.has(form)) return;
        pendingForms.add(form);

        const result = await Swal.fire({
            title: form.dataset.swalTitle || 'Confirmar ação?',
            text: form.dataset.swalText || '',
            icon: form.dataset.swalIcon || 'warning',
            showCancelButton: true,
            confirmButtonText: form.dataset.swalConfirmText || 'Confirmar',
            cancelButtonText: form.dataset.swalCancelText || 'Cancelar',
            confirmButtonColor: form.dataset.swalConfirmColor || '#dc3545',
            cancelButtonColor: '#6c757d',
            focusCancel: true,
            reverseButtons: true
        });

        if (!result.isConfirmed) {
            pendingForms.delete(form);
            return;
        }

        form.querySelector('button[type="submit"]')?.setAttribute('disabled', 'disabled');
        HTMLFormElement.prototype.submit.call(form);
    });
})();
