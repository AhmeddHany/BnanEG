const systemsCheckLicense = document.querySelector('.systems-check-license');
const LicenseUploadContainer = document.querySelector(".License-upload-container");
const LicenseMainContainer = document.querySelector(".License-main-container");
const UploadLicensePic = document.getElementById("UploadLicensePic");
const LicenseImageUpload = document.getElementById("LicenseimageUpload");
const openLicenseCameraButton = document.getElementById('openCameraLicense');
let saveLicenseBtn = null;
var licenseImgURL;

function updateLicenseSystemsCheckBackground() {
    const systemsCheckLicense = document.querySelector('.systems-check-license');
    const LicenseUploadContainer = document.querySelector(".License-upload-container");

    if (LicenseUploadContainer && systemsCheckLicense) {
        if (LicenseUploadContainer.querySelector("img")) {
            systemsCheckLicense.style.backgroundColor = "green";
        } else {
            systemsCheckLicense.style.backgroundColor = ""; 
        }
    } else {
        console.log("LicenseUploadContainer or systemsCheckLicense not found");
    }
}

document.addEventListener("DOMContentLoaded", function () {
    updateLicenseSystemsCheckBackground();   
});

// Image Upload
UploadLicensePic.addEventListener("click", function () {
    LicenseImageUpload.click();
    saveLicenseBtn = "UploadLicensePic";
});

LicenseImageUpload.addEventListener("change", function () {
    const file = LicenseImageUpload.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const licenseImageURL = e.target.result;
            const licensePreviewImage = document.createElement("img");
            licensePreviewImage.classList.add("preview-image");
            licensePreviewImage.src = licenseImageURL;
            licensePreviewImage.id = "LicenseImage";
            licenseImgURL = licenseImageURL;
            LicenseMainContainer.innerHTML = '<i class="fa-regular fa-circle-xmark xmark-icon"></i>';
            LicenseUploadContainer.innerHTML = "";
            LicenseUploadContainer.appendChild(licensePreviewImage);
            LicenseUploadContainer.classList.add("previewing");
        };
        reader.readAsDataURL(file);
    }
});

// Remove Image
document.getElementById("removeLicenseImg").addEventListener("click", function (event) {
    event.preventDefault();
    saveLicenseBtn = null;
    if (LicenseUploadContainer.firstChild) {
        LicenseUploadContainer.innerHTML = "";
        LicenseMainContainer.innerHTML = "";
        LicenseUploadContainer.classList.remove("previewing");
    }
});

// Camera Capture
openLicenseCameraButton.addEventListener('click', async () => {
    saveLicenseBtn = "CameraLicense";
    let videoElement = document.getElementById('videoLicenseElement');
    let photo = document.getElementById('licensePhoto');

    if (!videoElement) {
        LicenseUploadContainer.innerHTML = `
        <video id="videoLicenseElement" autoplay></video>
        <img id="licensePhoto" alt="The screen capture will appear in this box." style="display:none;">
        `;
        LicenseMainContainer.innerHTML = '<i class="fa-regular fa-circle-xmark xmark-icon"></i>';
        videoElement = document.getElementById('videoLicenseElement');
        photo = document.getElementById('licensePhoto');
         
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ video: true });
            videoElement.srcObject = stream;

            await new Promise(resolve => {
                videoElement.onloadedmetadata = () => {
                    resolve();
                };
            });

        } catch (error) {
            console.error('Error accessing the camera:', error);
        }
    } else {
        const canvasElement = document.createElement('canvas');
        canvasElement.width = videoElement.videoWidth;
        canvasElement.height = videoElement.videoHeight;
        const context = canvasElement.getContext('2d');
        context.drawImage(videoElement, 0, 0, canvasElement.width, canvasElement.height);

        const stream = videoElement.srcObject;
        const tracks = stream.getTracks();
        tracks.forEach(track => track.stop());

        const dataUrl = canvasElement.toDataURL('image/png');
        photo.src = dataUrl;
        photo.style.display = 'block';
        videoElement.remove();

        LicenseUploadContainer.innerHTML = "";
        LicenseUploadContainer.appendChild(photo);
    }
});

// Save the uploaded License photo image
function SaveUploadedLicensePhoto() {
    const img = document.getElementById("LicenseImage");
    const canvas = document.createElement("canvas");
    canvas.width = img.width;
    canvas.height = img.height;
    const context = canvas.getContext("2d");
    context.drawImage(img, 0, 0, canvas.width, canvas.height);
    const base64 = canvas.toDataURL("image/png");
    console.log(base64);
    $("#License-photo-modal").modal("hide");
}

// Save the camera License photo image
function SaveCameraLicensePhoto() {
    const img = document.getElementById("licensePhoto");
    const canvas = document.createElement("canvas");
    canvas.width = img.width;
    canvas.height = img.height;
    const context = canvas.getContext("2d");
    context.drawImage(img, 0, 0, canvas.width, canvas.height);
    const base64 = canvas.toDataURL("image/png");
    console.log(base64);
    $("#License-photo-modal").modal("hide");
}

document.getElementById("License-photo-save").addEventListener("click", function () {
    if (saveLicenseBtn === "UploadLicensePic") {
        SaveUploadedLicensePhoto();
    } else if (saveLicenseBtn === "CameraLicense") {
        SaveCameraLicensePhoto();
    } else {
        $("#License-photo-modal").modal("hide");
    }
    updateLicenseSystemsCheckBackground();
});
