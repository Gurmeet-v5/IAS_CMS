//window.onresize = function (event) {
//    //refresh web page when usser change orientation forced fully.
//    document.location.reload(true);
//}
jQuery(window).load(function () {
    //website loader images
    jQuery("#loader").fadeOut();
    jQuery("#pre-div").delay(1000).fadeOut("slow");
    var sliderElement = document.getElementsByClassName('flexslider');
    if (sliderElement.length > 0) {
        jQuery('.flexslider').flexslider({
            animation: "fade",
            controlNav: true,
            directionNav: false,
            slideshowSpeed: 10000,
            animationSpeed: 1000
        });
    }

    var slider1Element = document.getElementById('owl-car1');
    if (slider1Element !=null) {
        jQuery('#owl-car1').owlCarousel({
            items: 1,
            nav: $("#owl-car1 > .item").length <= 9 ? false : true,
            dots: false,
            loop: $("#owl-car1 > .item").length <= 9 ? false : true,
            nav: true,
            lazyLoad: true,
            autoplay: true,
            autoplayTimeout: 3000,
            smartSpeed: 2000,
            responsiveClass: true,
            responsiveRefreshRate: true,
            responsive: {
                0: {
                    items: 2,
                    margin: 20
                },
                480: {
                    items: 3, // from 480 to 677 
                    nav: false
                },
                640: {
                    items: 4, // from 480 to 677 
                    nav: false
                },
                768: {
                    items: 5,
                    nav: false
                },

                1024: {
                    items: 6,
                    margin: 20,

                },
                1025: {
                    items: 8,
                    margin: 20
                }
            }
        });
    }

    var slider2Element = document.getElementById('owl-car2');
    if (slider2Element != null) {
        jQuery('#owl-car2').owlCarousel({
            items: 1,
            nav: $("#owl-car2 > .item").length <= 3 ? false : true,
            dots: false,
            loop: $("#owl-car2 > .item").length <= 3 ? false : true,


            margin: 10,
            lazyLoad: true,
            autoplay: true,
            autoplayTimeout: 3000,
            smartSpeed: 2000,
            responsiveClass: true,
            responsiveRefreshRate: true,
            responsive: {
                0: {
                    items: 1,
                    margin: 30,
                    nav: false
                },
                480: {
                    items: 2, // from 480 to 677 
                    nav: false
                },

                768: {
                    items: 3,
                    nav: false
                },

                1024: {
                    items: 3,
                    margin: 30,
                    nav: false

                },
                1025: {
                    items: 3,
                    margin: 20
                }
            }
        });
    }

    var slider3Element = document.getElementById('owl-car3');
    if (slider3Element != null) {
    jQuery('#owl-car3').owlCarousel({
        items: 1,
        nav: $("#owl-car3 > .item").length <= 3 ? false : true,
        dots: false,
        loop: $("#owl-car3 > .item").length <= 3 ? false : true,


        margin: 10,
        lazyLoad: true,
        autoplay: true,
        autoplayTimeout: 3000,
        smartSpeed: 2000,
        responsiveClass: true,
        responsiveRefreshRate: true,
        responsive: {
            0: {
                items: 1,
                margin: 30,
                nav: false
            },
            480: {
                items: 2, // from 480 to 677 
                nav: false
            },

            768: {
                items: 3,
                nav: false
            },

            1024: {
                items: 3,
                margin: 30,
                nav: false

            },
            1025: {
                items: 3,
                margin: 15
            }
        }
    });
}
    /* used for animation  -----------------------------------------------*/
    new WOW().init();
});


/* Used for asscibility -----------------------------------------------*/

jQuery(document).ready(function () {
    //debugger;
    var highcontrastCookie = jQuery.cookie('highcontrast');
    if (highcontrastCookie == "undefined" || highcontrastCookie == "yes") {
        jQuery('.main-wrapper').toggleClass("highcontrast");
    }

    var fontsizeCookie = jQuery.cookie('font_size');
    if (fontsizeCookie == "undefined" || fontsizeCookie == "yes") {
        jQuery('.main-wrapper').toggleClass('font_size');
    }

    var grayscaleCookie = jQuery.cookie('grayscale');
    if (grayscaleCookie == "undefined" || grayscaleCookie == "yes") {
        jQuery('.main-wrapper').toggleClass('desaturated');
    }
    
    var cookieVal = jQuery.cookie('showTopAlertMessage');
    if (typeof cookieVal != 'undefined') {
        cookieVal = jQuery.cookie('showTopAlertMessage').toString();
    }
    if (cookieVal == 'undefined' || cookieVal == 'yes') {
        jQuery.cookie('showTopAlertMessage', 'yes');
        jQuery('.alert').show();
    }
    else {
        jQuery('.alert').hide();
    }

    // When the element is clicked
    jQuery(".contrast_btn").click(function () {
        //debugger;
        if (typeof highcontrastCookie == "undefined" || typeof highcontrastCookie == "yes") {
            jQuery.cookie('highcontrast', 'yes');
            jQuery('.main-wrapper').toggleClass("highcontrast");
        } else {
            jQuery.removeCookie('highcontrast');
            //jQuery.cookie('highcontrast', 'yes');
            jQuery('.main-wrapper').toggleClass("highcontrast");
        }
    });

    jQuery('.font_size_btn').click(function () {
        //debugger;
        if (typeof fontsizeCookie == "undefined" || typeof fontsizeCookie == "yes") {
            jQuery.cookie('font_size', 'yes');
            jQuery('.main-wrapper').toggleClass('font_size');
        } else {
            jQuery.removeCookie('font_size');
            //jQuery.cookie('font_size', 'yes');
            jQuery('.main-wrapper').toggleClass('font_size');
        }
    });

    jQuery('.grayscale_btn').click(function () {
        //debugger;
        if (typeof grayscaleCookie == "undefined" || typeof grayscaleCookie == "yes") {
            jQuery.cookie('grayscale', 'yes');
            jQuery('.main-wrapper').toggleClass('desaturated');
        }
        else {
            jQuery.removeCookie('grayscale');
            //jQuery.cookie('grayscale', null, { path: '/' });
            jQuery('.main-wrapper').toggleClass('desaturated');
        }
    });

    jQuery('.alertclose').click(function () {
        var cookieVal = jQuery.cookie('showTopAlertMessage');
        if (typeof cookieVal == 'undefined' || typeof cookieVal == 'yes') {
            jQuery.cookie('showTopAlertMessage', 'yes');
            jQuery('.alert').show();
        } else {
            jQuery.cookie('showTopAlertMessage', 'no');
            jQuery('.alert').hide();
        }
    });

    (function (jQuery) {
        jQuery.fn.menumaker = function (options) {
            var cssmenu = jQuery(this), settings = jQuery.extend({
                format: "dropdown",
                sticky: false
            }, options);
            return this.each(function () {
                jQuery(this).find(".button").on('click', function () {
                    jQuery(this).toggleClass('menu-opened');
                    var mainmenu = jQuery(this).next('ul');
                    if (mainmenu.hasClass('open')) {
                        mainmenu.slideToggle().removeClass('open');
                    }
                    else {
                        mainmenu.slideToggle().addClass('open');
                        if (settings.format === "dropdown") {
                            mainmenu.find('ul').show();
                        }
                    }
                });
                cssmenu.find('li ul').parent().addClass('has-sub');
                multiTg = function () {
                    cssmenu.find(".has-sub").prepend('<span class="submenu-button"></span>');
                    cssmenu.find('.submenu-button').on('click', function () {
                        jQuery(this).toggleClass('submenu-opened');
                        if (jQuery(this).siblings('ul').hasClass('open')) {
                            jQuery(this).siblings('ul').removeClass('open').slideToggle();
                        }
                        else {
                            jQuery(this).siblings('ul').addClass('open').slideToggle();
                        }
                    });
                };
                if (settings.format === 'multitoggle') multiTg();
                else cssmenu.addClass('dropdown');
                if (settings.sticky === true) cssmenu.css('position', 'fixed');
                //resizeFix = function() {
                //  var mediasize = 800;
                //     if (jQuery( window ).width() > mediasize) {
                //       cssmenu.find('ul').show();
                //     }
                //     if (jQuery(window).width() <= mediasize) {
                //       cssmenu.find('ul').hide().removeClass('open');
                //     }
                //   };
                //   resizeFix();
                //   return jQuery(window).on('resize', resizeFix);
            });
        };
    })(jQuery);
});

jQuery(document).ready(function () {
    jQuery("#cssmenu").menumaker({
        format: "multitoggle"
    });

    equalheight = function (container) {
        var currentTallest = 0,
             currentRowStart = 0,
             rowDivs = new Array(),
             jQueryel,
             topPosition = 0;
        jQuery(container).each(function () {

            jQueryel = jQuery(this);
            jQuery(jQueryel).height('auto');
            topPostion = jQueryel.position().top;

            if (currentRowStart !== topPostion) {
                for (currentDiv = 0 ; currentDiv < rowDivs.length ; currentDiv++) {
                    rowDivs[currentDiv].height(currentTallest);
                }
                rowDivs.length = 0; // empty the array
                currentRowStart = topPostion;
                currentTallest = jQueryel.height();
                rowDivs.push(jQueryel);
            } else {
                rowDivs.push(jQueryel);
                currentTallest = (currentTallest < jQueryel.height()) ? (jQueryel.height()) : (currentTallest);
            }
            for (currentDiv = 0 ; currentDiv < rowDivs.length ; currentDiv++) {
                rowDivs[currentDiv].height(currentTallest);
            }
        });
    };
    jQuery(window).load(function () {
        equalheight('ul.bsc-list li .bsc-box');
    });
    jQuery(window).resize(function () {
        equalheight('ul.bsc-list li .bsc-box');
    });

    //jQuery(function() {
    //      jQuery('table').footable();
    //    });
    //});

    if (jQuery(window).width() < 1025) {
        jQuery('.navbar-container').appendTo(".mobile-header");
    }

    var getImageSrc = $('.banner_img').attr('src');
    // add div background image using the variable above
    $('.main-banner').css('background-image', 'url(' + getImageSrc + ')');
});




