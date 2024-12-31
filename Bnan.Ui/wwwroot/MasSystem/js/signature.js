// // //////////////////////////////////////////////// رفع صورة التوقيع ////////////////////////////////////////////////////////////////////////

let saveSignatureBtn = null;
var signtureImg;
document
    .getElementById("UploadSigntaurePic")
    .addEventListener("click", function () {
        saveSignatureBtn = "UploadSigntaurePic";
    });

document.getElementById("WriteSignature").addEventListener("click", function () {
    saveSignatureBtn = "WriteSignature";
});
const uploadContainer = document.querySelector(".upload-container");
const mainContainer = document.querySelector(".image-main-container");
const UploadSigntaurePic = document.getElementById("UploadSigntaurePic");
const imageUpload = document.getElementById("imageUpload");
const uploadedImg = null;
//

UploadSigntaurePic.addEventListener("click", function () {
    imageUpload.click();
});

imageUpload.addEventListener("change", function () {
    const file = imageUpload.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const imageURL = e.target.result;
            const previewImage = document.createElement("img");
            previewImage.classList.add("preview-image");
            previewImage.src = imageURL;
            previewImage.id = "signatureImage";
            imgeURL = imageURL;
            mainContainer.innerHTML =
                '<i class="fa-regular fa-circle-xmark xmark-icon"></i>';
            uploadContainer.innerHTML = "";
            uploadContainer.appendChild(previewImage);
            uploadContainer.classList.add("previewing");
        };
        reader.readAsDataURL(file);
    }
});

removeSignatureImg.addEventListener("click", function (event) {
    event.preventDefault();
    if (uploadContainer.firstChild) {
        uploadContainer.innerHTML = "";
        mainContainer.innerHTML = "";
        uploadContainer.classList.remove("previewing");
        uploadContainer.innerHTML =
            '<p>ارفق صورة التوقيع</p>';
    }
});
// // //////////////////////////////////////////////// كتابة التوقيع ////////////////////////////////////////////////////////////////////////
const WriteSignature = document.getElementById("WriteSignature");
WriteSignature.addEventListener("click", function () {
    document.body.classList.add("no-scroll");
    uploadContainer.innerHTML = "";
    mainContainer.innerHTML = "";
    uploadContainer.innerHTML =
        '<canvas id="canvas" width="200" height="200" class="signature-canvas"></canvas>';
    var canvas = document.getElementById("canvas");
    var ctx = canvas.getContext("2d");
    ctx.lineWidth = 4;

    var drawing = false;
    var prevX = 0;
    var prevY = 0;
    var currX = 0;
    var currY = 0;

    function drawLine(x0, y0, x1, y1) {
        ctx.beginPath();
        ctx.moveTo(x0, y0);
        ctx.lineTo(x1, y1);
        ctx.stroke();
        ctx.closePath();
    }

    canvas.addEventListener("mousedown", handleMouseDown, false);
    canvas.addEventListener("mousemove", handleMouseMove, false);
    canvas.addEventListener("mouseup", handleMouseUp, false);

    canvas.addEventListener("touchstart", handleTouchStart, false);
    canvas.addEventListener("touchmove", handleTouchMove, false);
    canvas.addEventListener("touchend", handleTouchEnd, false);

    function handleMouseDown(e) {
        drawing = true;
        prevX = e.clientX - canvas.getBoundingClientRect().left;
        prevY = e.clientY - canvas.getBoundingClientRect().top;
    }

    function handleMouseMove(e) {
        if (!drawing) return;
        currX = e.clientX - canvas.getBoundingClientRect().left;
        currY = e.clientY - canvas.getBoundingClientRect().top;

        drawLine(prevX, prevY, currX, currY);
        prevX = currX;
        prevY = currY;
    }

    function handleMouseUp() {
        drawing = false;
    }

    function handleTouchStart(e) {
        drawing = true;
        prevX = e.touches[0].clientX - canvas.getBoundingClientRect().left;
        prevY = e.touches[0].clientY - canvas.getBoundingClientRect().top;
    }

    function handleTouchMove(e) {
        if (!drawing) return;
        currX = e.touches[0].clientX - canvas.getBoundingClientRect().left;
        currY = e.touches[0].clientY - canvas.getBoundingClientRect().top;

        drawLine(prevX, prevY, currX, currY);
        prevX = currX;
        prevY = currY;
    }

    function handleTouchEnd() {
        drawing = false;
    }
    function clearCanvas() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
    }

    document.getElementById("clear").addEventListener("click", function () {
        clearCanvas();
    });
});
function SaveWrittenSignature() {
    document.body.classList.remove("no-scroll");
    var canvas = document.getElementById("canvas");
    var dataURL = canvas.toDataURL();
    var link = document.createElement("a");
    link.href = dataURL;
    console.log(link.href);
    document.getElementById('previewSignature').src = link.href;
    $("#signature-modal").modal("hide");
    return dataURL;
}
// Save the uploded signature image
function SaveUplodedSignature() {
    const img = document.getElementById("signatureImage");
    const canvas = document.createElement("canvas");
    if (img) {
        canvas.width = img.width;
        canvas.height = img.height;
    } else {
        return
    }
    const context = canvas.getContext("2d");
    context.drawImage(img, 0, 0, canvas.width, canvas.height);

    // Use PNG format to preserve transparency
    const base64 = canvas.toDataURL("image/png");
    console.log(base64);
    document.getElementById('previewSignature').src = base64;
    $("#signature-modal").modal("hide");
    return base64;
}

document.getElementById("save").addEventListener("click", function () {
    var signtureInput = document.getElementById('UserSignatureFile');

    if (saveSignatureBtn === "UploadSigntaurePic") {
        signtureInput.value = SaveUplodedSignature();
    } else if (saveSignatureBtn === "WriteSignature") {
        signtureInput.value = SaveWrittenSignature();
    } else {
        console.log("No button has been clicked yet");
        signtureInput.value = null;
    }
});
