//document.addEventListener("DOMContentLoaded", function () {
//    loadStep(1); // Arrancamos en el paso 1
//});

////async function loadStep(stepNumber) {
////    const container = document.getElementById('step-container');

////    try {
////        let response;
////        //if (stepNumber === 1) {
////        //    response = await fetch(urls.getStep1);
////        //}
////        // ... lógica para otros pasos ...

////        switch (stepNumber) {
////            case 1:
////                response = await fetch(urls.getStep1);
////                break;
////            case 2:
////                response = await fetch(urls.getStep2);
////                break;
////            case 3:
////                response = await fetch(urls.getStep3);
////                break;
////            case 4:
////                response = await fetch(urls.getStep4);
////                break;
////            case 5:
////                response = await fetch(urls.getStep5);
////                break;
////            default:
////                throw new Error("Paso no reconocido");
////        }

////        if (!response.ok) throw new Error("No se pudo obtener el componente visual.");

////        const html = await response.text();
////        container.innerHTML = html;

////    } catch (error) {
////        container.innerHTML = `
////            <div class="alert alert-danger m-3">
////                <i class="bi bi-exclamation-triangle-fill"></i>
////                Error exacto: ${error.message}
////            </div>`;
////    }
////}


//async function loadStep(stepNumber) {
//    const container = document.getElementById('step-container');

//    if (!container) {
//        console.error("No se encontró el elemento 'step-container'. Revisa tu Index.cshtml");
//        return;
//    }

//    // Actualizar visualmente el stepper superior
//    document.querySelectorAll('.step-item').forEach((item, index) => {
//        const idx = index + 1;
//        item.classList.remove('active', 'completed');
//        if (idx < stepNumber) item.classList.add('completed');
//        if (idx === stepNumber) item.classList.add('active');
//    });

//    try {
//        const response = await fetch(urls[`getStep${stepNumber}`]);
//        if (!response.ok) throw new Error("Error visual.");

//        const html = await response.text();
//        container.innerHTML = html;

//        // Ejecutar init si es el paso 1
//        if (stepNumber === 1 && typeof Step1Handler !== 'undefined') {
//            Step1Handler.handleTypeChange(); // Para que detecte el estado inicial (Titular)
//        }
//    } catch (error) {
//        container.innerHTML = `<div class="alert alert-danger">${error.message}</div>`;
//    }
//}