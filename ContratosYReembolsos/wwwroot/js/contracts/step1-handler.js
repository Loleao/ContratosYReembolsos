//const Step1Handler = {
//    selectedItem: null, // Variable temporal para guardar la selección

//    handleTypeChange: function () {
//        const type = document.getElementById('SolicitorType').value;
//        const btnSearch = document.getElementById('btnOpenSearch'); // ID del botón de la lupa

//        // Limpiamos todos los campos siempre que cambie el tipo
//        this.clearForm();

//        if (type === "Titular") {
//            // Mostrar lupa y bloquear campos para obligar búsqueda oficial
//            btnSearch.classList.remove('d-none');
//            btnSearch.style.display = "block";
//            this.setFieldsReadOnly(true);
//        } else {
//            // Ocultar lupa y permitir ingreso manual para familiares
//            btnSearch.classList.add('d-none');
//            this.setFieldsReadOnly(false);
//            document.getElementById('SolicitorDni').focus();
//        }
//    },

//    // 2. Limpiar todos los inputs del Paso 1
//    clearForm: function () {
//        const fields = ['AffiliateIdfaf', 'SolicitorDni', 'SolicitorCip', 'SolicitorName', 'SolicitorPhone'];
//        fields.forEach(id => {
//            const el = document.getElementById(id);
//            if (el) el.value = "";
//        });
//        this.selectedItem = null;
//    },

//    // 3. Activar o desactivar edición manual
//    setFieldsReadOnly: function (isReadOnly) {
//        document.getElementById('SolicitorDni').readOnly = isReadOnly;
//        document.getElementById('SolicitorName').readOnly = isReadOnly;
//        document.getElementById('SolicitorCip').readOnly = isReadOnly;
//        // El teléfono siempre lo dejamos editable por si cambió
//        document.getElementById('SolicitorPhone').readOnly = false;
//    },

//    executeSearch: async function () {
//        const dni = document.getElementById('filterDni').value;
//        const cip = document.getElementById('filterCip').value;
//        const name = document.getElementById('filterName').value;
//        const tbody = document.getElementById('resultsTableBody');
//        const btnAccept = document.getElementById('btnAcceptSelection');

//        tbody.innerHTML = '<tr><td colspan="4" class="text-center"><div class="spinner-border spinner-border-sm text-primary"></div> Buscando...</td></tr>';
//        btnAccept.disabled = true;

//        try {
//            // Construimos la URL con los 3 parámetros
//            const query = `dni=${dni}&cip=${cip}&name=${encodeURIComponent(name)}`;
//            const response = await fetch(`${urls.searchAffiliate}?${query}`);
//            const data = await response.json();

//            if (!response.ok) throw new Error(data.message || "Error en búsqueda");

//            tbody.innerHTML = '';
//            if (data.length === 0) {
//                tbody.innerHTML = '<tr><td colspan="4" class="text-center text-danger">No hay coincidencias.</td></tr>';
//                return;
//            }

//            data.forEach(item => {
//                const row = document.createElement('tr');
//                row.style.cursor = "pointer";
//                row.innerHTML = `
//                    <td>${item.dni}</td>
//                    <td>${item.cip || '---'}</td>
//                    <td class="small fw-bold">${item.name}</td>
//                    <td class="text-center"><input type="radio" name="affiliateRadio" value="${item.id}"></td>
//                `;
//                // Al hacer clic en cualquier parte de la fila, selecciona el radio
//                row.onclick = () => {
//                    this.selectedItem = item;
//                    row.querySelector('input').checked = true;
//                    btnAccept.disabled = false;
//                };
//                tbody.appendChild(row);
//            });

//        } catch (error) {
//            tbody.innerHTML = `<tr><td colspan="4" class="text-center text-danger">${error.message}</td></tr>`;
//        }
//    },

//    acceptSelection: function () {
//        if (!this.selectedItem) return;

//        // Inyectar datos
//        document.getElementById('AffiliateIdfaf').value = this.selectedItem.id;
//        document.getElementById('SolicitorDni').value = this.selectedItem.dni;
//        document.getElementById('SolicitorName').value = this.selectedItem.name;
//        document.getElementById('SolicitorCip').value = this.selectedItem.cip;

//        // Bloquear campos si es Titular para mantener integridad de datos
//        if (document.getElementById('SolicitorType').value === "Titular") {
//            document.getElementById('SolicitorDni').readOnly = true;
//            document.getElementById('SolicitorName').readOnly = true;
//            document.getElementById('SolicitorCip').readOnly = true;
//        }

//        // Cerrar modal
//        const modal = bootstrap.Modal.getInstance(document.getElementById('modalSearchAffiliate'));
//        modal.hide();

//        console.log("✅ Datos cargados correctamente para el ID: " + this.selectedItem.id);
//    }
//};