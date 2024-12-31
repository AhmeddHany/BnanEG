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





//function initializeIdleTimeoutHandler(exitUser) {
//    let timeout = parseInt(exitUser) * 60 * 1000;
//    //let timeout = .1 * 60 * 1000;
//    let countdownDuration = 60; // Countdown duration in seconds
//    let idleTimeout = null;
//    let countdownTimer = null;
//    let countdownStartTime;
//    console.log("timeout", timeout)
//    // Static URLs
//    const updateLastActionUrl = "/Identity/Account/UpdateLastActionDate";
//    const logoutUrl = "/Identity/Account/Logout";

//    // Function to start the countdown
//    function startCountdown() {
//        countdownStartTime = Date.now();
//        countdownTimer = setInterval(updateCountdown, 1000);
//    }

//    // Function to clear the countdown timer
//    function clearCountdown() {
//        if (countdownTimer) {
//            clearInterval(countdownTimer);
//            countdownTimer = null;
//        }
//    }

//    // Function to update the countdown display
//    function updateCountdown() {
//        let countdownElem = $('#countdown');
//        let elapsedTime = Math.floor((Date.now() - countdownStartTime) / 1000);
//        let timeLeft = countdownDuration - elapsedTime;
//        console.log("timeLeft", timeLeft);

//        if (timeLeft > 0) {
//            countdownElem.text(timeLeft);
//        } else {
//            console.log("timeLeft");
//            clearCountdown();
//            logout();
//        }
//    }

//    // Function to log out the user
//    function logout() {
//        $.ajax({
//            type: "GET",
//            url: logoutUrl,
//            success: function (response) {
//                window.location.href = '/Identity/Account/Login';
//            }
//        });
//    }

//    // Function to reset the idle timeout
//    function resetIdleTimeout() {
//        clearCountdown();
//        clearTimeout(idleTimeout);
//        $('#popup').modal("hide");
//        updateLastAction();
//        idleTimeout = setTimeout(showPopup, timeout);
//    }

//    // Function to show the countdown popup
//    function showPopup() {
//        if ($('#popup').is(':visible')) {
//            return; // Popup already displayed
//        }

//        $('#countdown').text(countdownDuration);
//        $('#popup').modal("show");
//        startCountdown();
//    }

//    // Event listener for user activity (click/keydown)
//    $(document).on('click keydown', resetIdleTimeout);

//    // Continue button click handler
//    $('#continueBtn').click(resetIdleTimeout);

//    // Logout button click handler
//    $('#logoutBtn').click(function () {
//        clearTimeout(idleTimeout);
//        logout();
//    });

//    // Function to update last action date in the database
//    function updateLastAction() {
//        $.ajax({
//            type: "POST",
//            url: updateLastActionUrl,
//            success: function (response) {
//                console.log("response", response);
//                if (!response) {
//                    setTimeout(function () {
//                        window.location.href = '/Identity/Account/Login';
//                    }, 500); // Redirect after 1 second
//                }
//            },
//            error: function (xhr, status, error) {
//                console.error("An error occurred:", error);
//            }
//        });
//    }

//    // Handle visibility change (tab focus/blur)
//    document.addEventListener('visibilitychange', function () {
//        if (document.visibilityState === 'visible') {
//            if (!$('#popup').is(':visible')) {
//                resetIdleTimeout();
//            }
//        } else {
//            clearCountdown();
//        }
//    });

//    // Initialize idle timeout
//    idleTimeout = setTimeout(showPopup, timeout);
//}
