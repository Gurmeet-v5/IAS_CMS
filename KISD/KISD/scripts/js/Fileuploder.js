var validFilesTypes = ["doc", "docx", "pdf"];

function CheckExtension(file) {
    /*global document: false */
 
    var filePath = file.value;
    var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();
    var isValidFile = false;
    var errormsg = '';
    for (var i = 0; i < validFilesTypes.length; i++) {
        if (ext == validFilesTypes[i]) {
            isValidFile = true;
            break;
        }
    }

    if (!isValidFile) {

        file.value = null;
        file = null;
        errormsg = "Only DOC/DOCX/PDF files are allowed.";
        var element = document.getElementById('input_file_upload_error_doc');
        element.style.display = 'block';
        element.innerHTML = errormsg;
        return isValidFile;
    }

    if (file != null) {

        var f = file.files[0]
        if (f.size > 4194304 || f.fileSize > 4194304) {
            file.value = null;
            errormsg = errormsg + ('Maximum file size is 4 MB.');
            isValidFile = false;
        }
        else {
            var fileUpload = $("#fileupload").get(0);
            var files = fileUpload.files;

            var data = new FormData();
            for (var i = 0; i < files.length; i++) {
                data.append(files[i].name, files[i]);
                $.ajax({
                    url: "./common/fileupload.ashx",
                    type: "POST",
                    data: data,
                    contentType: false,
                    processData: false,
                    success: function (result) {
                    
                        $('#hfresumepath').val(result);
                  
                        
                    },
                    error: function (err) {
                        alert(err.statusText)
                    }
                });

                //evt.preventDefault();
              
            }
            isValidFile = true;
        }

        var element = document.getElementById('input_file_upload_error_doc');
        element.style.display = 'block';
        element.innerHTML = errormsg;
        // only show one err msg at a time.
        document.getElementById('err_ImagePathTxt').style.display = 'none';
        return isValidFile;
    }
}
var validAdminImagesTypes = ["jpg", "jpeg", "png", "gif"];



function uploadimages(file) {
    /*global document: false */
 
    var errormsg = '';
    var filePath = file.value;
    var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();
    var isValidFile = false;

    for (var i = 0; i < validAdminImagesTypes.length; i++) {
        if (ext == validAdminImagesTypes[i]) {
            isValidFile = true;
            break;
        }
    }

    if (!isValidFile) {
        file.value = null;
        file = null;
        errormsg = ("Only JPG/JPEG/GIF/PNG files are allowed.");
    }
    if (file != null) {
        var f = file.files[0]
        if (f.size > 4194304 || f.fileSize > 4194304) {
            file.value = null;
            //  alert('Maximum image size is 4 MB.');
            errormsg = errormsg + ('Maximum image size is 4 MB.');
            isValidFile = false;
        }
        else { isValidFile = true; }

    }
    var element = document.getElementById('input_file_upload_error_img');
    element.style.display = 'block';
    element.innerHTML = errormsg;
    return isValidFile;
}

function uploadimagesmulti(file,errormessageID) {
    /*global document: false */

    var errormsg = '';
    var filePath = file.value;
    var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();
    var isValidFile = false;

    for (var i = 0; i < validAdminImagesTypes.length; i++) {
        if (ext == validAdminImagesTypes[i]) {
            isValidFile = true;
            break;
        }
    }

    if (!isValidFile) {
        file.value = null;
        file = null;
        errormsg = ("Only JPG/JPEG/GIF/PNG files are allowed.");
    }
    if (file != null) {
        var f = file.files[0]
        if (f.size > 4194304 || f.fileSize > 4194304) {
            file.value = null;
            //  alert('Maximum image size is 4 MB.');
            errormsg = errormsg + ('Maximum image size is 4 MB.');
            isValidFile = false;
        }
        else {

            //var img = new Image();
            //img.onload = function () {
            //    alert(this.width + " " + this.height);
            //};
            //var _URL = window.URL || window.webkitURL;
            //img.src = _URL.createObjectURL(f);

            isValidFile = true;
        }

    }
    var element = document.getElementById(errormessageID);
    element.style.display = 'block';
    element.innerHTML = errormsg;
    return isValidFile;
}

 validImagesTypes = ["jpg", "jpeg", "png", "gif", "doc", "docx", "pdf"];

function uploadMultipleimages() {
    var file = document.getElementById('multiplifiles');
    var errormsg = '';
    var isValidFile = false;
    var totalfiles = 0;
    for (var f = 0; f < file.files.length; ++f) {
        totalfiles = totalfiles + 1;
        var filePath = file.files.item(f).name;
        var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();
        isValidFile = false;
        for (var i = 0; i < validImagesTypes.length; i++) {
            if (ext == validImagesTypes[i]) {
                isValidFile = true;
                break;
            }
        }
    }
  

    if (!isValidFile) {
        file.value = null;
        file = null;
        errormsg = ("Only JPG/JPEG/GIF/PNG/DOC/DOCX/PDF files are allowed.");
    }

    if (totalfiles > 10) {
        isValidFile = false;
        file.value = null;
        file = null;
        errormsg = "Maximum 10 files can be uploaded at a time.";
    }
    if (file != null) {
        var totalsize = 20971520;
        var fsize = 0;
        for (var k = 0; k < file.files.length; ++k) {
            fsize = fsize + parseInt(file.files.item(k).size);            
        }
        if (fsize > totalsize ) {
            file.value = null;            
            errormsg = errormsg + ('Maximum total size of all files is 20 MB.');
            isValidFile = false;
        }
        else {
            isValidFile = true;
        }
    }

    var element = document.getElementById('input_file_upload_error_img');
    if (!isValidFile) {
        element.style.display = 'block';
        element.innerHTML = errormsg;
    }
    else {
        element.style.display = 'none';
    }

    return isValidFile;
}

function uploaddocs(file) {
    /*global document: false */

    var errormsg = '';
    var filePath = file.value;
    var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();
    var isValidFile = false;

    for (var i = 0; i < validImagesTypes.length; i++) {
        if (ext == validImagesTypes[i]) {
            isValidFile = true;
            break;
        }
    }

    if (!isValidFile) {
        file.value = null;
        file = null;
        errormsg = ("Only JPG/JPEG/GIF/PNG/DOC/DOCX/PDF files are allowed.");
    }
    if (file != null) {
        var f = file.files[0]
        if (f.size > 10194304 || f.fileSize > 10194304) {
            file.value = null;
            //  alert('Maximum image size is 10 MB.');
            errormsg = errormsg + ('Maximum document size is 10 MB.');
            isValidFile = false;
        }
        else { isValidFile = true; }

    }
    var element = document.getElementById('input_file_upload_error_img');
    element.style.display = 'block';
    element.innerHTML = errormsg;
    return isValidFile;
}





