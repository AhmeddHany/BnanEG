function initializeIdleTimeoutHandler(exitUser) {
    let timeout = parseInt(exitUser) * 60 * 1000;
    let countdownDuration = 60; // Countdown duration in seconds
    let idleTimeout = null;
    let countdownTimer = null;
    let countdownStartTime;

    // Static URLs
    const updateLastActionUrl = "/Identity/Account/UpdateLastActionDate";
    const logoutUrl = "/Identity/Account/Logout";

    // Function to start the countdown
    function startCountdown() {
        countdownStartTime = Date.now();
        countdownTimer = setInterval(updateCountdown, 1000);
    }

    // Function to clear the countdown timer
    function clearCountdown() {
        if (countdownTimer) {
            clearInterval(countdownTimer);
            countdownTimer = null;
        }
    }

    // Function to update the countdown display
    function updateCountdown() {
        let countdownElem = $('#countdown');
        let elapsedTime = Math.floor((Date.now() - countdownStartTime) / 1000);
        let timeLeft = countdownDuration - elapsedTime;
        console.log("timeLeft", timeLeft);

        if (timeLeft > 0) {
            countdownElem.text(timeLeft);
        } else {
            console.log("timeLeft");
            clearCountdown();
            logout();
        }
    }

    // Function to log out the user
    function logout() {
        $.ajax({
            type: "GET",
            url: logoutUrl,
            success: function (response) {
                window.location.href = '/Identity/Account/Login';
            }
        });
    }

    // Function to reset the idle timeout
    function resetIdleTimeout() {
        clearCountdown();
        clearTimeout(idleTimeout);
        $('#popup').modal("hide");
        updateLastAction();
        idleTimeout = setTimeout(showPopup, timeout);
    }

    // Function to show the countdown popup
    function showPopup() {
        if ($('#popup').is(':visible')) {
            return; // Popup already displayed
        }

        $('#countdown').text(countdownDuration);
        $('#popup').modal("show");
        startCountdown();
    }

    // Event listener for user activity (click/keydown)
    $(document).on('click keydown', resetIdleTimeout);

    // Continue button click handler
    $('#continueBtn').click(resetIdleTimeout);

    // Logout button click handler
    $('#logoutBtn').click(function () {
        clearTimeout(idleTimeout);
        logout();
    });

    // Function to update last action date in the database
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

    // Handle visibility change (tab focus/blur)
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'visible') {
            if (!$('#popup').is(':visible')) {
                resetIdleTimeout();
            }
        } else {
            clearCountdown();
        }
    });

    // Initialize idle timeout
    idleTimeout = setTimeout(showPopup, timeout);
}
