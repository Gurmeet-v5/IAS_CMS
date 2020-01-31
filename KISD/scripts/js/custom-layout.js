
jQuery(document).ready(function () {
    jQuery(window).load(function () {
        //website loader images
			jQuery("#loader").fadeOut();
			jQuery("#pre-div").delay(1000).fadeOut("slow");
	});


 /* Back to top
     -----------------------------------------------*/
    if (jQuery('#back-to-top').length) {
        var scrollTrigger = 400, // px
                backToTop = function () {
                    var scrollTop = jQuery(window).scrollTop();
                    if (scrollTop > scrollTrigger) {
                        jQuery('#back-to-top').addClass('show');
                    } else {
                        jQuery('#back-to-top').removeClass('show');
                    }
                };
        backToTop();
        jQuery(window).on('scroll', function () {
            backToTop();
        });
        jQuery('#back-to-top').on('click', function (e) {
            e.preventDefault();
            jQuery('html,body').animate({
                scrollTop: 0
            }, 700);
        });
    }

    /* used for animation
     -----------------------------------------------*/
    new WOW().init();
  


/* Used for asscibility
     -----------------------------------------------*/
if (jQuery.cookie('highcontrast') === "undefined" || jQuery.cookie('highcontrast') === "yes") {
        jQuery('body').toggleClass("highcontrast");
    } 

    if (jQuery.cookie('font_size') === "undefined" || jQuery.cookie('font_size') === "yes") {
        jQuery('body').toggleClass('font_size');
    } 

    if (jQuery.cookie('grayscale') === "undefined" || jQuery.cookie('grayscale') === "yes") {
        jQuery('body').toggleClass('desaturated');
    } 

// When the element is clicked
	jQuery(".contrast_btn").click(function () {
        if (jQuery.cookie('highcontrast') === "undefined" || jQuery.cookie('highcontrast') === "yes") {
            jQuery.cookie('highcontrast', 'no');
            jQuery('body').toggleClass("highcontrast");
        } else {
            jQuery.cookie('highcontrast', 'yes');
            jQuery('body').toggleClass("highcontrast");
        }
    });

	jQuery('.font_size_btn').click(function () {
	    if (jQuery.cookie('font_size') === "undefined" || jQuery.cookie('font_size') === "yes") {
            jQuery.cookie('font_size', 'no');
            jQuery('body').toggleClass('font_size');
        } else {
            jQuery.cookie('font_size', 'yes');
            jQuery('body').toggleClass('font_size');
        }
    });
	jQuery('.grayscale_btn').click(function () {
        if (jQuery.cookie('grayscale') === "undefined" || jQuery.cookie('grayscale') === "yes") {
            jQuery.cookie('grayscale', 'no');
            jQuery('body').toggleClass('desaturated');
        } else {
            jQuery.cookie('grayscale', 'yes');
            jQuery('body').toggleClass('desaturated');
        }
    });

});



     