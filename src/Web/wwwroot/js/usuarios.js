document.querySelectorAll('form[data-confirmar-exclusao]').forEach((form) => {
    let pending = false;
    form.addEventListener('submit', async (event) => {
        event.preventDefault();
        if (pending) return;
        pending = true;
        try {
            const result = await Swal.fire({
                title: 'Excluir usuário?',
                text: 'Deseja excluir ' + form.dataset.usuario + '? Esta ação não pode ser desfeita.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sim, excluir',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                focusCancel: true,
                reverseButtons: true
            });
            if (result.isConfirmed) {
                form.querySelector('button[type="submit"]').disabled = true;
                HTMLFormElement.prototype.submit.call(form);
            } else {
                pending = false;
            }
        } catch (error) {
            pending = false;
            console.error('Não foi possível abrir a confirmação de exclusão.', error);
        }
    });
});
