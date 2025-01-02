const systemsCheck = document.querySelector('.systems-check');
const IDuploadContainer = document.querySelector(".Doc-upload-container");
const IDmainContainer = document.querySelector(".Doc-main-container");
const UploadPic = document.getElementById("UploadPic");
const IDimageUpload = document.getElementById("DocimageUpload");
const openCameraButton = document.getElementById('openCamera');
let saveIDBtn = null;
var imgeURL;

function updateSystemsCheckBackground() {
    const systemsCheck = document.querySelector('.systems-check');
    const IDuploadContainer = document.querySelector(".Doc-upload-container");

    if (IDuploadContainer && systemsCheck) {
        if (IDuploadContainer.querySelector("img")) {
            systemsCheck.style.backgroundColor = "green";
        } else {
            systemsCheck.style.backgroundColor = ""; 
        }
    } else {
        console.log("IDuploadContainer or systemsCheck not found");
    }
}
document.addEventListener("DOMContentLoaded", function () {
        updateSystemsCheckBackground();   
});

// Image Upload
UploadPic.addEventListener("click", function () {
    IDimageUpload.click();
    saveIDBtn = "UploadPic";
});

IDimageUpload.addEventListener("change", function () {
    const file = IDimageUpload.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const IDimageURL = e.target.result;
            const IDpreviewImage = document.createElement("img");
            IDpreviewImage.classList.add("preview-image");
            IDpreviewImage.src = IDimageURL;
            IDpreviewImage.id = "IDImage";
            imgeURL = IDimageURL;
            IDmainContainer.innerHTML = '<i class="fa-regular fa-circle-xmark xmark-icon"></i>';
            IDuploadContainer.innerHTML = "";
            IDuploadContainer.appendChild(IDpreviewImage);
            IDuploadContainer.classList.add("previewing");
        };
        reader.readAsDataURL(file);
    }
});

// Remove Image
document.getElementById("removeIDImg").addEventListener("click", function (event) {
    event.preventDefault();
    saveIDBtn = null;
    if (IDuploadContainer.firstChild) {
        IDuploadContainer.innerHTML = "";
        IDmainContainer.innerHTML = "";
        IDuploadContainer.classList.remove("previewing");
    }
});

// Camera Capture
openCameraButton.addEventListener('click', async () => {
    saveIDBtn = "CameraID";
    let videoElement = document.getElementById('videoElement');
    let photo = document.getElementById('photo');

    if (!videoElement) {
        IDuploadContainer.innerHTML = `
        <video id="videoElement" autoplay></video>
        <img id="photo" alt="The screen capture will appear in this box." style="display:none;">
        `;
        IDmainContainer.innerHTML = '<i class="fa-regular fa-circle-xmark xmark-icon"></i>';
        videoElement = document.getElementById('videoElement');
        photo = document.getElementById('photo');
         
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

        // Append captured photo to the container
        IDuploadContainer.innerHTML = "";
        IDuploadContainer.appendChild(photo);
    }
});

// Save the uploaded ID photo image
function SaveUplodedIDphoto() {
    const img = document.getElementById("IDImage");
    const canvas = document.createElement("canvas");
    canvas.width = img.width;
    canvas.height = img.height;
    const context = canvas.getContext("2d");
    context.drawImage(img, 0, 0, canvas.width, canvas.height);
    const base64 = canvas.toDataURL("image/png");
    console.log(base64);
    $("#Doc-photo-modal").modal("hide");
}

// Save the camera ID photo image
function SaveCameraIDphoto() {
    const img = document.getElementById("photo");
    const canvas = document.createElement("canvas");
    canvas.width = img.width;
    canvas.height = img.height;
    const context = canvas.getContext("2d");
    context.drawImage(img, 0, 0, canvas.width, canvas.height);
    const base64 = canvas.toDataURL("image/png");
    console.log(base64);
    $("#Doc-photo-modal").modal("hide");
}

document.getElementById("Doc-photo-save").addEventListener("click", function () {
    if (saveIDBtn === "UploadPic") {
        SaveUplodedIDphoto();
    } else if (saveIDBtn === "CameraID") {
        SaveCameraIDphoto();
    }else {
        $("#Doc-photo-modal").modal("hide");

    }
    // Update systems-check background after saving the image
    updateSystemsCheckBackground();
});