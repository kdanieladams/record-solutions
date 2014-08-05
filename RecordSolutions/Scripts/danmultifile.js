// danmultifile.js

var newFileInputIndex = 1;

var closeFileBtn = function (index) {
    $("div#newFileGroup" + index).remove();
    newFileInputIndex--;
};

var addFileInputBtn = function () {
    var elm = "div#uploadFileField";
    
    var newFileInput = $(document.createElement('div'));
    
    newFileInput.attr("class", "input-group");
    newFileInput.attr("id", "newFileGroup"+ newFileInputIndex);
    
    newFileInput.html("<input name=\"uploadFile"+ newFileInputIndex +"\" type=\"file\""
      + " class=\"form-control\" id=\"newFile" 
      + newFileInputIndex + "\"/>"
      + "<span class='input-group-addon'>"
      + "<a style='cursor:pointer' class='close' onclick='closeFileBtn(" + newFileInputIndex + ")' data-input-id='newFile"
      + newFileInputIndex + "'>&times</a>"
      + "</span></div>");

    $(elm).append(newFileInput);
    
    newFileInputIndex++;
};