$(document).ready(function () {
    $('#calendar').fullCalendar({
        header:
        {
            left: 'prev,next today',
            center: 'title',
            right: 'month,agendaWeek,agendaDay'
        },
        buttonText: {
            today: 'today',
            month: 'month',
            week: 'week',
            day: 'day'
        },        
        events: function (start, end, timezone, callback) {
            var DepartmentID = document.getElementById("DepartmentID").value;
            $.ajax({
                url: '/ContentOuter/GetCalendarData',
                type: "GET",
                data: { 'DepartmentID': DepartmentID },
                dataType: "JSON",

                success: function (result) {
                    var events = [];
                    $.each(result, function (i, data) {
                        events.push(
                       {
                           title: data.Title,
                           description: data.Desc,
                           start: moment(data.Start_Date).format('YYYY-MM-DD hh:mm'),
                           end: moment(data.End_Date).format('YYYY-MM-DD hh:mm'),
                           backgroundColor: "",
                           borderColor: ""
                       });
                    });

                    callback(events);
                }
            });
        },
        eventClick: function (event, jsEvent, view) {
            $('#modalTitle').html(event.title);
            $('#modalBody').html(event.description);
            $('#fullCalModal').modal();
        },
        eventRender: function (event, element) {
        },

        editable: false
    });
});