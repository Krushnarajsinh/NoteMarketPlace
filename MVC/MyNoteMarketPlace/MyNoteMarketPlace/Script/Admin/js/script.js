/* ==================================
            Toggle Password
==================================== */
$(".toggle-password").click(function () {

    $(this).toggleClass("fa-eye fa-eye-slash");
    var input = $($(this).attr("toggle"));
    if (input.attr("type") == "password") {
        input.attr("type", "text");
    } else {
        input.attr("type", "password");
    }
});

/* ==================================
            Navigation
====================================

/* Show & Hide White Navigation 
$(function () {

    // show/hide nav on page load
    showHideNav();

    $(window).scroll(function () {

        showHideNav();

    });

    function showHideNav() {

        if ($(window).scrollTop() > 50) {

            //Show White nav
            $(".navbar-movable").addClass("white-nav-top");

            // Show dark logo
            $(".navbar-movable .navbar-brand img").attr("src", "images/pre-login/Capture.PNG");

        } else {

            // Hide White nav
            $(".navbar-movable").removeClass("white-nav-top");

            // Show logo
            $(".navbar-movable .navbar-brand img").attr("src", "images/pre-login/top-logo.png");

        }
    }
});
*/
/* ==================================
            Mobile Menu
====================================*/
$(function () {

    // Show Mobile nav
    $("#mobile-nav-open-btn").click(function () {
        $("#mobile-nav").css("height", "100%");
    });

    // Hide Mobile nav
    $("#mobile-nav-close-btn").click(function () {
        $("#mobile-nav").css("height", "0");
    });


});
/* ==================================
            Accodion
====================================*/
$(document).ready(function () {

    for (let i = 1; i <= 7; i++) {
        $(".showdata" + i).click(function () {
            $(".mybody" + i).show();
            $(".myhead" + i).hide();
        });
        $(".hidedata" + i).click(function () {
            $(".mybody" + i).hide();
            $(".myhead" + i).show();
        });
    }
});

/* ========================================================================================================== */
//                      Dashboard
/* ========================================================================================================== */
$(document).ready(function () {

    $("#dashboard-month").change(function () {
        this.form.submit();
    });

});
/* ========================================================================================================== */
//                      Notes Under Review Notes
/* ========================================================================================================== */
$(document).ready(function () {

    $("#notesunderreview-seller").change(function () {
        this.form.submit();
    });

});

/* ========================================================================================================== */
//                      Rejected Notes
/* ========================================================================================================== */
$(document).ready(function () {

    $("#rejectednotes-seller").change(function () {
        this.form.submit();
    });

});

/* ========================================================================================================== */
//                      Published Notes
/* ========================================================================================================== */
$(document).ready(function () {

    $("#publishednotes-seller").change(function () {
        this.form.submit();
    });

});

/* ========================================================================================================== */
//                      Downloaded Notes
/* ========================================================================================================== */
$(document).ready(function () {

    $("#downloadednotes-note").change(function () {
        this.form.submit();
    });
    $("#downloadednotes-seller").change(function () {
        this.form.submit();
    });
    $("#downloadednotes-buyer").change(function () {
        this.form.submit();
    });

});


/* ==================================
            Drom-down Menu
====================================
$(document).ready(function () {
    $('.dropdown').click(function () {
        $('.dropdown-menu').toggleClass('show');
    });
    
});

 */