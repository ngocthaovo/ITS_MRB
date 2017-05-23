var ContentFunction = function () { };

ContentFunction.DisplayItem = function (ele, iConEle, isShow) {
    if (isShow != null) {
        if (isShow) {
            $("#" + ele).slideDown('fast');
            $("#" + iConEle).removeClass('icon-circle-arrow-right');
            $("#" + iConEle).addClass('icon-circle-arrow-down');
        }
        else {
            $("#" + ele).slideUp('fast');
            $("#"+ iConEle).removeClass('icon-circle-arrow-down');
            $("#" + iConEle).addClass('icon-circle-arrow-right');
        }
    }
    else {

        isShow = $("#" + ele).css('display');
        if (isShow == 'block') {
            $("#" + ele).slideUp('fast');
            $("#"+ iConEle).removeClass('icon-circle-arrow-down');
            $("#" + iConEle).addClass('icon-circle-arrow-right');
        }
        else {
            $("#" + ele).slideDown('fast');
            $("#" + iConEle).removeClass('icon-circle-arrow-right');
            $("#" + iConEle).addClass('icon-circle-arrow-down');
        }
    }
};


$(function () {

    $("form").submit(function (e) {
        var divload = $('#divLoadCommon').length;
        if (divload ==0) {
            $(this).prepend("<div class='loading' id='divLoadCommon' style='display:block;'></div>");
        }
    });
});

function GetQueryStringValue(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

