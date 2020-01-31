 function  uploadcropimage() {

    var validAdminImagesTypes = ["jpg", "jpeg", "png", "gif"];
    var files = document.getElementsByClassName("flpcropImage");
    var file = files[0];
    var errormsg = '';
    var filePath = file.value;
    if (!filePath) {
        return ;
    }

    var data = new FormData();
    data.append("HelpSectionImages", files[0]);

     $.ajax({
         url: "/CropImage/UploadFile",
         type: "POST",   
         contentType: false,
         processData: false,
         dataType: data,
         success: function (result) {
             if (result != '') {

             }
             else {

             }
         },
         error: function (err) {
             alert(err.statusText)
         }
     });






   // $('#form1').submit();

    //var data = new FormData();
    //data.append(files[0].name, files[0]);
     
  //  $('#newform').submit();

    //$.ajax({
    //    url: "/CropImage/UploadFile",
    //    type: "POST",
    //   // data: data,
    //    contentType: false,
    //    processData: false,
    //    dataType: 'json',
    //    success: function (result) {
    //        if (result != '') {

    //        }
    //        else {

    //        }
    //    },
    //    error: function (err) {
    //        alert(err.statusText)
    //    }
    //});


    //$.ajax({
    //    url: "../../common/fileupload.ashx",
    //    type: "POST",
    //    data: data,
    //    contentType: false,
    //    processData: false,

    //    success: function (result) {
    //        if (result != '') {
                
    //        }
    //        else {
                
    //        }
    //    },
    //    error: function (err) {
    //        alert(err.statusText)
    //    }
    //});

}