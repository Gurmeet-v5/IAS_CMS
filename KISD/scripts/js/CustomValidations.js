
function validateEmail(sEmail) {
    var filter = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
    if (filter.test(sEmail)) {
        return true;
    }
    else {
        return false;
    }
}

jQuery('#AddSubscriber').click(function () {
    var url = "/Common/SendData";
    var subscriberdata = jQuery('.subscriber-data').val();
    var success = document.getElementById('success');
    if (subscriberdata.trim() == '') {
        success.innerText = "This field is required.";
        jQuery('#success').removeClass("common_hide");
        jQuery('#success').addClass("common_red");
        return false;
    }
    else if (!validateEmail(String(subscriberdata).trim())) {
        success.innerText = "Invalid Email.";
        jQuery('#success').addClass("common_red");
        jQuery('#success').removeClass("common_hide");
        return false;
    }

    jQuery.ajax({
        url: url,
        type: 'POST',
        data: { "Email": subscriberdata },
        cache: false,
        success: function (result) {
            if (result == true) {
                jQuery('.subscriber-data').val('');
                jQuery('#success').removeClass("common_red");
                jQuery('#success').addClass("common_green");
                success.innerText = "Congratulations ! You have subscribed successfully.";
                jQuery('#success').hide().fadeIn('slow').delay(8000).fadeOut('slow');
            }
            else {
                jQuery('#success').removeClass("common_green");
                jQuery('#success').addClass("common_red");
                success.innerText = 'Some error occured.';
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });
});

jQuery("#userName").keypress(function () {
    if (jQuery("#userName").val() != '') {
        jQuery('#SpanLastName').addClass('common_hide');
        jQuery('#SpanLastName').removeClass('common_show');
    }
});

jQuery("#TxtMsg").keypress(function () {
    if (jQuery("#TxtMsg").val() != '') {
        jQuery('#SpanMsg').addClass('common_hide');
        jQuery('#SpanMsg').removeClass('common_show');
    }
});

jQuery('#userName').keyup(function (e) {
    if (e.keyCode == 8 && jQuery("#userName").val() == '') {
        jQuery('#SpanLastName').removeClass('common_hide');
        jQuery('#SpanLastName').addClass('common_show');
    }
    else {
        jQuery('#SpanLastName').addClass('common_hide');
        jQuery('#SpanLastName').removeClass('common_show');
    }
})
jQuery('#TxtMsg').keyup(function (e) {
    if (e.keyCode == 8 && jQuery("#TxtMsg").val() == '') {
        jQuery('#SpanMsg').removeClass('common_hide');
        jQuery('#SpanMsg').addClass('common_show');
    }
    else {
        jQuery('#SpanMsg').addClass('common_hide');
        jQuery('#SpanMsg').removeClass('common_show');
    }
})


jQuery('#btnsubmit').on('click', function () {
    var lname = jQuery("#userName").val().trim();
    var msg = jQuery("#TxtMsg").val();
    var respone = grecaptcha.getResponse();
    var selectedPurpose = document.getElementById('AllPurposes');
    var selectedValue = selectedPurpose.value;
    if (lname == '' && msg == '') { jQuery('#userName').focus(); }
 
    if (String(selectedValue).trim() == '') {
        jQuery('#SpanRegarding').removeClass('common_hide');
        jQuery('#SpanRegarding').addClass('common_show');
    }
    else {
        jQuery('#SpanRegarding').addClass('common_hide');
        jQuery('#SpanRegarding').removeClass('common_show');
    }

    if (String(lname).trim() == '') {
        jQuery('#SpanLastName').removeClass('common_hide');
        jQuery('#SpanLastName').addClass('common_show');
    }
    else {
        jQuery('#SpanLastName').addClass('common_hide');
        jQuery('#SpanLastName').removeClass('common_show');
    }

    if (String(msg).trim() == '') {
        jQuery('#SpanMsg').removeClass('common_hide');
        jQuery('#SpanMsg').addClass('common_show');
    }
    else {
        jQuery('#SpanMsg').addClass('common_hide');
        jQuery('#SpanMsg').removeClass('common_show');
    }

    if (String(respone).trim() == '') {
        jQuery('#rCaptchaMsg').removeClass('common_hide');
        jQuery('#rCaptchaMsg').addClass('common_show');
        return false;
    }
    else {
        jQuery('#rCaptchaMsg').addClass('common_hide');
        jQuery('#rCaptchaMsg').removeClass('common_show');
    }

    if (String(selectedValue).trim() == '' || String(lname) == '' || String(msg) == '' || respone == '') { return false; }
});

jQuery(document).ready(function () {

    jQuery('#AllPurposes').change(function () {
        if (jQuery(this).val() != '') {
            jQuery('#SpanRegarding').addClass('common_hide');
            jQuery('#SpanRegarding').removeClass('common_show');
        }
        else {
            jQuery('#SpanRegarding').removeClass('common_hide');
            jQuery('#SpanRegarding').addClass('common_show');
        }
    });

    jQuery('.only-numbers').keydown(function (e) {
        if (e.shiftKey || e.ctrlKey || e.altKey) {
            e.preventDefault();
        } else {
            var key = e.keyCode;
            if (!((key == 8) || (key == 46) || (key >= 35 && key <= 40) || (key >= 48 && key <= 57) || (key >= 96 && key <= 105))) {
                e.preventDefault();
            }
        }
    });
});

