let count = 0;
const box = document.getElementById("container");
const container = document.getElementById("box");
const inbox = document.getElementById("inner-container");

function startLoader(totalTimeInMillis) {
    count = 0;
    updateProgress();
    nextLoader(totalTimeInMillis);
    $('.main1').css('display', 'flex');
}

function updateProgress() {
    box.innerHTML = count + "%";
}

function nextLoader(totalTimeInMillis) {
    const incrementTime = totalTimeInMillis / 100; // Calculate the time interval for each percentage increment

    inbox.style.width = count + "%";

    if (count < 100) {
        count++;
        updateProgress();
        setTimeout(() => nextLoader(totalTimeInMillis), incrementTime); // Use the calculated increment time
    } else {
        container.style.opacity = "1";
    }
}