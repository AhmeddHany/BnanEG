function initializeIdleTimeoutHandler(exitUser) {
    const FULL_DASH_ARRAY = 283;
    const TIME_LIMIT = 60; // Countdown duration in seconds
    let timeLeft = TIME_LIMIT;
    let idleTimeout = null;
    let countdownTimer = null;
    const updateLastActionUrl = "/Identity/Account/UpdateLastActionDate";
    const logoutUrl = "/Identity/Account/Logout";

    function initializeTimerUI() {
        document.getElementById("app").innerHTML = `
            <div class="base-timer">
                <svg class="base-timer__svg" viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
                    <g class="base-timer__circle">
                        <circle class="base-timer__path-elapsed" cx="50" cy="50" r="45"></circle>
                        <path
                            id="base-timer-path-remaining"
                            stroke-dasharray="283"
                            class="base-timer__path-remaining blue"
                            d="
                                M 50, 50
                                m -45, 0
                                a 45,45 0 1,0 90,0
                                a 45,45 0 1,0 -90,0
                            "
                        ></path>
                    </g>
                </svg>
                <span id="base-timer-label" class="base-timer__label">${formatTime(timeLeft)}</span>
            </div>
        `;
    }

    function startCountdown() {
        countdownTimer = setInterval(() => {
            timeLeft -= 1;
            document.getElementById("base-timer-label").innerHTML = formatTime(timeLeft);
            setCircleDasharray();

            if (timeLeft <= 0) {
                clearInterval(countdownTimer);
                logout();
            }
        }, 1000);
    }

    function logout() {
        window.location.href = logoutUrl;
    }


    function updateLastAction() {
        $.ajax({
            type: "POST",
            url: updateLastActionUrl,
            success: function (response) {
                console.log("response", response);
                if (!response) {
                    setTimeout(function () {
                        window.location.href = '/Identity/Account/Login';
                    }, 500); // Redirect after 1 second
                }
            },
            error: function (xhr, status, error) {
                console.error("An error occurred:", error);
            }
        });
    }

    function resetIdleTimeout() {
        clearInterval(countdownTimer);
        clearTimeout(idleTimeout);
        timeLeft = TIME_LIMIT;
        initializeTimerUI();
        $('#ActiveTime').modal("hide");
        idleTimeout = setTimeout(showPopup, exitUser * 60 * 1000);
        updateLastAction();
    }

    function showPopup() {
        initializeTimerUI();
        $('#ActiveTime').modal("show");
        startCountdown();
    }

    function formatTime(time) {
        const minutes = Math.floor(time / 60);
        let seconds = time % 60;
        if (seconds < 10) seconds = `0${seconds}`;
        return `${minutes}:${seconds}`;
    }

    function calculateTimeFraction() {
        return timeLeft / TIME_LIMIT - (1 / TIME_LIMIT) * (1 - timeLeft / TIME_LIMIT);
    }

    function setCircleDasharray() {
        const circleDasharray = `${(calculateTimeFraction() * FULL_DASH_ARRAY).toFixed(0)} 283`;
        document.getElementById("base-timer-path-remaining").setAttribute("stroke-dasharray", circleDasharray);
    }

    document.querySelector("#continueBtn").addEventListener("click", resetIdleTimeout);
    document.querySelector("#logoutBtn").addEventListener("click", logout);
    document.addEventListener("click", resetIdleTimeout);

    idleTimeout = setTimeout(showPopup, exitUser * 60 * 1000);
}
