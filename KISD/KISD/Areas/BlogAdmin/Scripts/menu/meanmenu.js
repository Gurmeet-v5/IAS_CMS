

var isMobile = false;
var showLeft = document.getElementById('showLeft');
var showRight = document.getElementById('showRight');
if ((navigator.userAgent.match(/iPhone/i)) || (navigator.userAgent.match(/iPod/i)) || (navigator.userAgent.match(/iPad/i)) || (navigator.userAgent.match(/Android/i)) || (navigator.userAgent.match(/Blackberry/i)) || (navigator.userAgent.match(/Windows Phone/i))) {
    isMobile = true;
}

var showMeanMenu = function () {
    jQuery('nav').addClass("cbp-spmenu cbp-spmenu-vertical cbp-spmenu-left")
    showLeft.style.display = 'block';
    jQuery('body').addClass("cbp-spmenu-push");

   // jQuery('nav').removeClass("hidemeanmenu");
};

var meanOriginal = function () {
    showLeft.style.display = 'none';
    jQuery('body').removeClass("cbp-spmenu-push");
    jQuery('nav').removeClass("cbp-spmenu cbp-spmenu-vertical cbp-spmenu-left")

   // jQuery('nav').addClass("hidemeanmenu");
};

if (isMobile) {
    showMeanMenu();
}

jQuery(window).resize(function () {
    var meanScreenWidth = "840";

    // get browser width
    currentWidth = window.innerWidth || document.documentElement.clientWidth;

    if (!isMobile) {
        meanOriginal();
        if (currentWidth <= meanScreenWidth) {
            showMeanMenu();
        }
    } else {
        if (currentWidth <= meanScreenWidth) {
            showMeanMenu();
        } else {
            meanOriginal();
        }
    }
});

jQuery(document).ready(function () {
    // debugger;
    RemoveNavHideClass();
    if (!isMobile) {
        var meanScreenWidth = "840";
        // reset menu on resize above meanScreenWidth
        jQuery(window).resize(function () {
            RemoveNavHideClass();
            currentWidth = window.innerWidth || document.documentElement.clientWidth;
            if (currentWidth <= meanScreenWidth) {
                showMeanMenu();
            } else {
                meanOriginal();
            }
        });
    }

    var currentWidth = window.innerWidth || document.documentElement.clientWidth;
    if ((currentWidth <= meanScreenWidth || meanScreenWidth === undefined) && currentWidth <= 840) {
        showMeanMenu();
    } else {
        meanOriginal();
    }

    //var meanScreenWidth = "840";
    //// get browser width
    //currentWidth = window.innerWidth || document.documentElement.clientWidth;

    //if (!isMobile) {
    //    meanOriginal();
    //    if (currentWidth <= meanScreenWidth) {
    //        showMeanMenu();
    //    }
    //} else {
    //    if (currentWidth <= meanScreenWidth) {
    //        if (meanMenuExist === false) {
    //            showMeanMenu();
    //        }
    //    } else {
    //        meanOriginal();
    //    }
    //}

    showLeft.onclick = function () {
        classie.toggle(this, 'active');
        classie.toggle(menuLeft, 'cbp-spmenu-open');
        disableOther('showLeft');
    };
});

var menuLeft = document.getElementById('cbp-spmenu-s1'),
    menuRight = document.getElementById('cbp-spmenu-s2'),
    menuTop = document.getElementById('cbp-spmenu-s3'),
    menuBottom = document.getElementById('cbp-spmenu-s4'),
    showTop = document.getElementById('showTop'),
    showBottom = document.getElementById('showBottom'),
    showLeftPush = document.getElementById('showLeftPush'),
    showRightPush = document.getElementById('showRightPush'),
    body = document.body;

function disableOther(button) {
    if (button !== 'showLeft') {
        classie.toggle(showLeft, 'disabled');
    }
}
 
function RemoveNavHideClass() {
    jQuery('nav').removeClass("hidemeanmenu");
}