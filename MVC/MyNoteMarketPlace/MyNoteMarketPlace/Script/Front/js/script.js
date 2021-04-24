/*===============================
             Toggle Password
===================================*/
$(".toggle-password").click(function () {

    $(this).toggleClass("fa-eye fa-eye-slash");
    var input = $($(this).attr("toggle"));
    if (input.attr("type") == "password") {
        input.attr("type", "text");
    } else {
        input.attr("type", "password");
    }
});

function matchPassword() {
    var pw1 = document.getElementById("password-field");
    var pw2 = document.getElementById("password-confirm-field");
    if (pw1 != pw2) {
        alert("Passwords did not match");
        document.getElementById("password-field").setTextValue() = "";
        document.getElementById("password-confirm-field").setTextValue() = "";
        return "@Url.Action('SignUp','SignUp')";
    }
}
/*===============================
             Navigation
===================================*/
// show & hide white navigation
$(function () {

    //on page load
    showHideNav();

    $(window).scroll(function () {

        //on window scroll
        showHideNav();

    });

    function showHideNav() {
        if ($(window).scrollTop() > 50) {
            // Show white nav
            $("nav").addClass("white-nav-top-1");
            //Show dark logo
            $(".navbar-brand-1 img").attr("src", "/Img/Front/pre-login/logo.png");
            // Show back to top button
            $("#back-to-top").fadeIn();
            $(".open-btn").css("color", "#6255a5");
        } else {
            //Hide white nav
            $("nav").removeClass("white-nav-top-1");
            //Show Normal Logo
            $(".navbar-brand-1 img").attr("src", "/Img/Front/pre-login/top-logo.png");
            // Hide back to top button
            $("#back-to-top").fadeOut();
            $(".open-btn").css("color", "#fff");
        }
    }
});


//Smooth Scrolling
$(function () {
    $("a.smooth-scroll").click(function (event) {
        //prevent default action
        event.preventDefault();
        // get section id like #about, #team, #work etc.
        var section_id = $(this).attr("href");

        $("html, body").animate({
            scrollTop: $(section_id).offset().top - 64
        }, 1250 /*animation duration*/, "easeInOutExpo" /*Easing plugin*/);

    });
});

/*===============================
              Mobile Menu
===================================*/
$(function () {
    // Show mobile nav
    $("#mobile-nav-open-btn").click(function () {
        $("#mobile-nav").css("height", "100%");
    });
    // Hide mobile nav
    $("#mobile-nav-close-btn, #mobile-nav a").click(function () {
        $("#mobile-nav").css("height", "0%");
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
/* ==================================
         Add Notes
====================================*/
$(document).ready(function () {
    $("#add-form input[name='IsPaid']").change(function () {
        if ($("#paid").is(":checked")) {
            $("#price").removeAttr("disabled");
            $("#price").focus();
            $("#note-preview").attr("required", "required");
        } else {
            $("#price").val(0);
            $("#price").attr("disabled", "disabled");
            $("#note-preview").removeAttr("required");
        }
    });
});
$(document).ready(function () {
    $("#edit-form input[name='IsPaid']").change(function () {
        if ($("#paid").is(":checked")) {
            $("#price").removeAttr("disabled");
            $("#price").focus();
        } else {
            $("#price").val(0);
            $("#price").attr("disabled", "disabled");
        }
    });
});
/* ==================================
           Search Filtering
====================================*/
$(document).ready(function () {

    $('.filtering #type').change(function () {
        this.form.submit();
    });
    $('.filtering #search').change(function () {
        this.form.submit();
    });
    $('.filtering #category').change(function () {
        this.form.submit();
    });
    $('.filtering #university').change(function () {
        this.form.submit();
    });
    $('.filtering #course').change(function () {
        this.form.submit();
    });
    $('.filtering #country').change(function () {
        this.form.submit();
    });
    $('.filtering #rating').change(function () {
        this.form.submit();
    });

});