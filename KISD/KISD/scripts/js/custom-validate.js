
$(document).ready(function (e) {
    $('.dateonly').on('change keyup paste blur focusout', function (e) {

        var date = $(this).val();
        var m = date.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
        ValidateDateObj(m);
        var element = $(this).closest('td').children('p');
        if (m == null && String(date).trim() != '') {
            alert("Invalid Date Format. Date Must be MM/DD/YYYY");
            /// element.html('');
            // element.html('<p class="field-validation-error">Invalid Date Format. Date Must be MM/DD/YYYY</p>');
            $(this).val('');
        }
        //else {
        //    element.html('');
        //}
    });
    $('.IsNumeric').on("change keyup paste", function (e) {
        $(this).val($(this).val().replace(/[^0-9+.]/g, ''));
    });

    $('.IsNumericwithoutdecimal').on("change keyup paste", function (e) {
        e.preventDefault();
        $(this).val($(this).val().replace(/[^0-9]/g, ''));
    });

    $('.NumberGreaterthanZero').on("change keyup paste", function (e) {
        e.preventDefault();
        if (parseInt($(this).val()) <= 0) {
            $(this).val('');
        }
    });
  
    $('.urladdress').on('change keyup paste blur focusout', function (e) {

        var date = $(this).val();
        var m = date.match(/^(?:(?:https?|ftp):\/\/)(?:\S+(?::\S*)?@)?(?:(?!10(?:\.\d{1,3}){3})(?!127(?:\.\d{1,3}){3})(?!169\.254(?:\.\d{1,3}){2})(?!192\.168(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:\/[^\s]*)?$/i);
        ValidateDateObj(m);
        var element = $(this).closest('td').children('p');
        if (m == null && String(date).trim() != '') {
            alert("Invalid url.");

            /// element.html('');
            // element.html('<p class="field-validation-error">Invalid Date Format. Date Must be MM/DD/YYYY</p>');
            $(this).val('');
        }
        //else {
        //    element.html('');
        //}
    });
    if ($(".StartDate").length > 0) {
        $(".StartDate").datepicker({

            changeMonth: true,
            changeYear: true,
            onClose: function (selectedDate) {
                $(".EndDate").datepicker("option", "minDate", selectedDate);
            }
        });
    }

    if ($(".EndDate").length > 0) {
        $(".EndDate").datepicker({

            changeMonth: true,
            changeYear: true,
            onClose: function (selectedDate) {
                $(".StartDate").datepicker("option", "maxDate", selectedDate);
            }
        });
    }

    if ($(".EndDate").length > 0) {
        $('.EndDate').on("change keyup paste", function (e) {

            DateCheck();
        });
    }

    if ($(".datebeforeToday").length > 0) {
        $(".datebeforeToday").datepicker({
            maxDate: new Date()
        });
    }

    if ($(".dateAfterToday").length > 0) {
        $(".dateAfterToday").datepicker({
            dateFormat: 'mm/dd/yy',
            firstDay: 1,
            showOn: 'focus',
            showAnim: 'blind',
            changeMonth: true,
            changeYear: true,
            yearRange: '-1:+20',
            minDate: '0',
            onSelect: function () { $(this).trigger("onchange", null); }
        });
    }
});



//This Function will validate date 
function ValidateDateObj(selectdate) {
    var bool = true;
    if (selectdate != null && selectdate != undefined && String(selectdate).trim() != '') {
        var dt = String(selectdate).split(',');
        if (!(parseInt(dt[1]) > 0 && parseInt(dt[1]) < 13)) {

            //     alert("Invalid Date Format. Date Must be MM/DD/YYYY");
            bool = false;
            return bool;
        }
        else if (!(parseInt(dt[2]) > 0 && parseInt(dt[2]) < 31)) {
            //   alert("Invalid Date Format. Date Must be MM/DD/YYYY");
            bool = false;
            return bool;
        }
        else if (!(dt[3].length == 4 && parseInt(dt[3]) > 0)) {
            if (parseInt(a[0]) > 12 || parseInt(a[1] > 31)) {
                //     alert("Invalid Date Format. Date Must be MM/DD/YYYY");
                bool = false;
                return bool;
            }
        }
    }
    return bool;
}

//This Function will check for start date and end date
function DateCheck() {

    var StartDate = $('.StartDate').val();
    var EndDate = $('.EndDate').val();
    var eDate = new Date(EndDate);
    var sDate = new Date(StartDate);
    if (StartDate != '' && StartDate != '' && sDate > eDate) {
        alert("Please ensure that the End Date is greater than or equal to the Start Date.");
        $('.EndDate').val('');
        return false;
    }
    else {

    }
}



