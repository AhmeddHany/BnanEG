// Get references to the necessary elements
const shapeCanvas = document.getElementById('shape-canvas');
const BendInBodyBtn = document.getElementById('bend-in-body-btn');
const veryDeepScratch = document.getElementById('very-deep-scratch-btn');
const deepScratch = document.getElementById('deep-scratch-btn');
const smallScratch = document.getElementById('small-scratch-btn');

const undoBtn = document.getElementById('undo-btn');
const redoBtn = document.getElementById('redo-btn');
const saveBtn = document.getElementById('save-btn');

window.addEventListener('load', function () {
    // Get the canvas context
    const ctx = shapeCanvas.getContext('2d');

    // Draw the car background
    const carBackground = new Image();
    carBackground.src = 'img/car shape.svg';
    carBackground.onload = function () {
        ctx.drawImage(carBackground, 0, 0, shapeCanvas.width, shapeCanvas.height);
    };
});

// Load the shape images
const BendInBody = new Image();
BendInBody.src = 'img/bend-in-body.svg';

const VeryDeepScratch = new Image();
VeryDeepScratch.src = 'img/very-deep-scratch.svg';

const DeepScratch = new Image();
DeepScratch.src = 'img/deep-scratch.svg';

const SmallScratch = new Image();
SmallScratch.src = 'img/small-scratch.svg';

// Keep track of the currently selected shape
let selectedShape = null;
let selectedShapeType = '';

// Keep track of the shapes drawn on the canvas
let drawnShapes = [];
let SketchRepresentation = [];
let currentIndex = -1;

// Add click event listeners to the buttons
BendInBodyBtn.addEventListener('click', () => {
    selectedShape = BendInBody;
    selectedShapeType = 'bend-in-body';
});

veryDeepScratch.addEventListener('click', () => {
    selectedShape = VeryDeepScratch;
    selectedShapeType = 'very-deep-scratch';
});

deepScratch.addEventListener('click', () => {
    selectedShape = DeepScratch;
    selectedShapeType = 'deep-scratch';
});

smallScratch.addEventListener('click', () => {
    selectedShape = SmallScratch;
    selectedShapeType = 'small-scratch';
});

undoBtn.addEventListener('click', () => {
    undo();
});

redoBtn.addEventListener('click', () => {
    redo();
});

// Add click event listener to the canvas
shapeCanvas.addEventListener('click', (event) => {
    if (selectedShape) {
        addShape(event);
    }
});

function addShape(event) {
    // Get the canvas context
    const ctx = shapeCanvas.getContext('2d');

    // Get the click coordinates relative to the canvas
    const x = event.clientX - shapeCanvas.getBoundingClientRect().left;
    const y = event.clientY - shapeCanvas.getBoundingClientRect().top;

    // Draw the selected shape on the canvas at the clicked location
    ctx.drawImage(selectedShape, x - selectedShape.width / 2, y - selectedShape.height / 2, 20, 20);

    // Increment the current index
    currentIndex++;

    // Add the new shape to the drawnShapes array at the current index
    drawnShapes[currentIndex] = {
        shape: selectedShape,
        x: x - selectedShape.width / 2,
        y: y - selectedShape.height / 2,
        type: selectedShapeType
    };

    // Remove any shapes beyond the current index (for redo functionality)
    drawnShapes = drawnShapes.slice(0, currentIndex + 1);

    // Update the SketchRepresentation array
    updateSketchRepresentation();
}

function undo() {
    if (currentIndex >= 0) {
        currentIndex--;
        redrawShapes();
    }
}

function redo() {
    if (currentIndex < drawnShapes.length - 1) {
        currentIndex++;
        redrawShapes();
    }
}

function redrawShapes() {
    // Get the canvas context
    const ctx = shapeCanvas.getContext('2d');

    // Clear the canvas
    ctx.clearRect(0, 0, shapeCanvas.width, shapeCanvas.height);

    // Draw the car background
    const carBackground = new Image();
    carBackground.src = 'img/car shape.svg';
    carBackground.onload = function () {
        ctx.drawImage(carBackground, 0, 0, shapeCanvas.width, shapeCanvas.height);

        // Redraw all the shapes in the drawnShapes array up to the current index
        for (let i = 0; i <= currentIndex; i++) {
            ctx.drawImage(drawnShapes[i].shape, drawnShapes[i].x, drawnShapes[i].y, 20, 20);
        }

        // Update the SketchRepresentation array
        updateSketchRepresentation();
    };
}

function updateSketchRepresentation() {
    SketchRepresentation = drawnShapes.slice(0, currentIndex + 1).map(shape => ({
        type: shape.type,
        x: shape.x,
        y: shape.y
    }));
    console.log(SketchRepresentation);
}

saveBtn.addEventListener('click', () => {
    console.log(SketchRepresentation); // This will log the current shapes' type and coordinates
    $('#TechnicalCheckUp').modal('hide');
    
    // const dataURL = shapeCanvas.toDataURL('image/png');
    // const downloadLink = document.createElement('a');
    // downloadLink.download = 'car-damage-image.png';
    // downloadLink.href = dataURL;
    // downloadLink.click();
});

// Add active class toggle functionality
document.getElementById('TechnicalCheckUp-container').addEventListener('click', function(event) {
    if (event.target.classList.contains('TechnicalCheckUp-Btn')) {
        const buttons = document.querySelectorAll('.TechnicalCheckUp-Btn');
        buttons.forEach(btn => btn.classList.remove('TechnicalCheckUp-Btn-active'));
        event.target.classList.add('TechnicalCheckUp-Btn-active');
    }
});
