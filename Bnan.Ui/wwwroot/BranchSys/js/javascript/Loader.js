function loader(){
$('.main-loader-container').css("display","block");
 const FULL_DASH_ARRAY = 283; 
const WARNING_THRESHOLD = 100;
const ALERT_THRESHOLD = 283;

const COLOR_CODES = {
    info: {
        color: "blue"
    },
    warning: {
        color: "blue",
        threshold: WARNING_THRESHOLD
    },
    alert: {
        color: "red",
        threshold: ALERT_THRESHOLD
    }
};

const TIME_LIMIT = 50;
let timePassed = 0;
let timeLeft = TIME_LIMIT;
let timerInterval = null;

startTimer();

function onTimesUp() {
    clearInterval(timerInterval);
}

function startTimer() {
    timerInterval = setInterval(() => {
        timePassed = timePassed += 1;
        timeLeft = TIME_LIMIT - timePassed;
        setCircleDasharray();

        if (timeLeft === 0) {
            onTimesUp();
        }
    }, 1000);
}

function calculateTimeFraction() {
    return (TIME_LIMIT - timeLeft) / TIME_LIMIT;
}

function setCircleDasharray() {
    const circleDasharray = `${(
        calculateTimeFraction() * FULL_DASH_ARRAY
    ).toFixed(0)} 283`;
    document
        .getElementById("base-timer-path-remaining")
        .setAttribute("stroke-dasharray", circleDasharray);
}

const dots = document.querySelectorAll('.Dot');
let currentDot = 0;

function changeDotBackground() {
    dots.forEach(dot => {
        dot.style.backgroundColor = '#D9D9D9';
    });

    dots[currentDot].style.backgroundColor = '#4d4d4d';

    currentDot = (currentDot + 1) % dots.length;
}

setInterval(changeDotBackground, 500);
}
