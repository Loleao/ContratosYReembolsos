// --- SISTEMA DE NOTIFICACIONES GLOBAL ---

function loadNotifications() {
    // Usamos la URL inyectada desde el Layout con Url.Action
    $.get(window.AppUrls.getNotifications, function (data) {
        const $list = $('#notif-items-list');
        const $badge = $('#notif-badge');
        const $label = $('#notif-count-label');

        if (data.count > 0) {
            $badge.text(data.count).removeClass('d-none');
            $label.text(`${data.count} NUEVAS`);
        } else {
            $badge.addClass('d-none');
            $label.text('AL DÍA');
        }

        let html = '';
        if (data.items.length === 0) {
            html = `
                <div class="p-5 text-center text-muted">
                    <i class="fa-solid fa-bell-slash fa-2x mb-3 opacity-25"></i>
                    <p class="small mb-0">No hay notificaciones pendientes</p>
                </div>`;
        } else {
            data.items.forEach(n => {
                html += `
                <li>
                    <a class="dropdown-item p-3 border-bottom d-flex align-items-start gap-3" href="${n.targetUrl || '#'}" onclick="markAsRead(${n.id})">
                        <div class="bg-primary-subtle p-2 rounded-circle text-primary">
                            <i class="fa-solid ${n.iconClass || 'fa-bell'} small"></i>
                        </div>
                        <div style="white-space: normal;">
                            <div class="fw-bold small text-dark">${n.title}</div>
                            <div class="x-small text-muted" style="font-size: 0.7rem;">${n.message}</div>
                            <div class="x-small text-primary mt-1 fw-bold" style="font-size: 0.65rem;">${n.timeAgo}</div>
                        </div>
                    </a>
                </li>`;
            });
        }
        $list.html(html);
    }).fail(function () {
        console.error("Error al sincronizar notificaciones. Verifique el controlador.");
    });
}

function markAsRead(id) {
    // Usamos la URL inyectada desde el Layout
    $.post(window.AppUrls.markNotificationRead, { id: id });
}

// Inicializar al cargar el documento
$(document).ready(function () {
    loadNotifications();

    // Sincronizar cada 2 minutos (120,000 ms)
    setInterval(loadNotifications, 120000);
});


// ==========================================
// CONFIGURACIÓN DE SIGNALR (TIEMPO REAL)
// ==========================================

// 1. Crear la conexión apuntando a la ruta que definimos en Program.cs
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect() // Reintento automático si se pierde el internet
    .build();

// 2. Escuchar el evento "ReceiveNotification" enviado por el NotificationService.cs
connection.on("ReceiveNotification", function () {
    console.log("¡Se recibió una señal de SignalR! Actualizando campana...");

    // Si tienes la función de cargar notificaciones, la ejecutamos
    if (typeof loadNotifications === "function") {
        loadNotifications();
    }
});

// 3. Iniciar la conexión
connection.start()
    .then(function () {
        console.log("SignalR: Conexión establecida con éxito.");

        // Unirse al grupo de la sede actual (BranchId)
        if (window.UserContext && window.UserContext.branchId) {
            const branchIdStr = window.UserContext.branchId.toString();

            connection.invoke("JoinBranchGroup", branchIdStr)
                .then(() => console.log("SignalR: Te has unido al grupo de la Sede: " + branchIdStr))
                .catch(err => console.error("Error al unirse al grupo:", err));
        }
    })
    .catch(err => {
        console.error("SignalR: Error al conectar:", err.toString());
    });