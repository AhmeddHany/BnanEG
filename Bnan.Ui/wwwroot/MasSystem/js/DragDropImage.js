document.querySelectorAll(".drop-zone__input").forEach((inputElement) => {
    const dropZoneElement = inputElement.closest(".drop-zone");
    const dropZoneCard = inputElement.closest(".dropzone-Card");
    const layerElement = dropZoneElement.querySelector(".layer");
    const removeImageButton = dropZoneElement.querySelector(".removeImage");
    var fileNameP = dropZoneCard.querySelector(".fileNameP");
    if (fileNameP) fileNameP.dataset.originalText = fileNameP.innerHTML;

    layerElement.addEventListener("mouseenter", (e) => {
        e.stopPropagation();
        if (!removeImageButton.hasAttribute("data-listener")) {
            removeImageButton.setAttribute("data-listener", "true");
            removeImageButton.addEventListener("click", (e) => {
                e.stopPropagation();
                removeImage(dropZoneElement, dropZoneCard, inputElement);
                console.log("removed")
            });
        }
    });

    dropZoneElement.addEventListener("click", (e) => {
        if (!layerElement.contains(e.target)) {
            inputElement.click();
        }
    });

    inputElement.addEventListener("change", (e) => {
        if (inputElement.files.length) {
            updateThumbnail(dropZoneElement, dropZoneCard, inputElement.files[0]);
        }
    });

    dropZoneElement.addEventListener("dragover", (e) => {
        e.preventDefault();
        dropZoneElement.classList.add("drop-zone--over");
    });

    ["dragleave", "dragend"].forEach((type) => {
        dropZoneElement.addEventListener(type, (e) => {
            dropZoneElement.classList.remove("drop-zone--over");
        });
    });

    dropZoneElement.addEventListener("drop", (e) => {
        e.preventDefault();

        if (e.dataTransfer.files.length) {
            inputElement.files = e.dataTransfer.files;
            updateThumbnail(dropZoneElement, dropZoneCard, e.dataTransfer.files[0]);
        }

        dropZoneElement.classList.remove("drop-zone--over");
    });

    dropZoneElement.addEventListener("mouseenter", () => {
        if (inputElement.files.length) {
            layerElement.style.display = "flex";
        }
    });

    dropZoneElement.addEventListener("mouseleave", () => {
        layerElement.style.display = "none";
    });

});

function removeImage(dropZoneElement, dropZoneCard, inputElement) {
    inputElement.value = "";
    const layerElement = dropZoneElement.querySelector(".layer");
    layerElement.style.display = "none";
    const thumbnailElement = dropZoneElement.querySelector(".drop-zone__thumb");
    if (thumbnailElement) {
        thumbnailElement.remove();
    }
    const promptElement = dropZoneElement.querySelector(".drop-zone__prompt");
    if (!promptElement) {
        const newPromptElement = document.createElement("span");
        newPromptElement.classList.add("drop-zone__prompt");
        newPromptElement.innerHTML = `<img src="/MasSystem/images/upload icon.svg" class="mb-3"> <br>قم بسحب وإسقاط صورة أو اختر صورة`;
        dropZoneElement.appendChild(newPromptElement);
    }

    var fileNameP = dropZoneCard.querySelector(".fileNameP");
    if (fileNameP) fileNameP.innerHTML = fileNameP.dataset.originalText;

    dropZoneElement.style.border = "";
    var fileNameDiv = dropZoneCard.querySelector(".fileNameDiv");
    if (fileNameDiv) fileNameDiv.style.background = "";
}

function updateThumbnail(dropZoneElement, dropZoneCard, file) {

    let thumbnailElement = dropZoneElement.querySelector(".drop-zone__thumb");

    const promptElement = dropZoneElement.querySelector(".drop-zone__prompt");
    if (promptElement) {
        promptElement.remove();
    }

    if (!thumbnailElement) {
        thumbnailElement = document.createElement("div");
        thumbnailElement.classList.add("drop-zone__thumb");
        dropZoneElement.appendChild(thumbnailElement);
        dropZoneElement.style.border = "none";
    }

    const fileNameP = dropZoneCard.querySelector(".fileNameP");
    const fileNameDiv = dropZoneCard.querySelector(".fileNameDiv");



    if (fileNameP) {
        fileNameP.innerHTML = file.name;
    }
    if (fileNameDiv) {
        fileNameDiv.style.background = "#39629C";
    }

    if (file.type.startsWith("image/")) {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => {
            thumbnailElement.style.backgroundImage = `url('${reader.result}')`;
        };
    } else {
        thumbnailElement.style.backgroundImage = null;
    }
}

