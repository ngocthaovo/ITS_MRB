var FEA_Its = function () { };

function closetabl(ele) {
    $("#panel_tab_" + ele).remove();
}

function GetCurrentActiveTab() {
    var obj = new Object();

    var a = $("#tabs").find(".active").length;
    if (a > 1) {
        var iframe = $("#tabs").find(".active:eq(1)").find('iframe');
        //alert($(iframe).attr('id'));

        obj.id = $("#tabs").find(".active:eq(1)").attr('id');
        obj.containername = $("#tabs").find(".active:eq(1)").find('iframe').attr('id');
        return obj;
       // alert(JSON.stringify(obj));
    }
    //alert(a);

    return undefined;
}

function createnewtab(tabname, taburl, tabid) {

    var checktab = $("#tab_" + tabid).length;
    if (checktab > 0) {
       // $('a[href="#panel_tab_' + num_tabs + '"]').tab('show');
        $("#tab_" + tabid).find('a:eq(0)').tab('show');
        return;
    }

    var num_tabs = $("div#tabs ul li").length + 1;

    $("#tabs ul").append(
        "<li id='tab_"+tabid+"'><span data-dismiss='alert' class='close tabclose' onclick ='closetabl(" + num_tabs + ")'>×</span><a data-toggle='tab'  href='#panel_tab_" + num_tabs + "'>" + tabname + "</a></li>"
    );
    $("#tab-content").append(
                "<div id='panel_tab_" + num_tabs + "' class='tab-pane'><iframe id='ifmMain" + num_tabs + "' src='" + taburl + "' style='height:900px;width: 100%; border: solid 0px;'></iframe></div>"
            );
    $('a[href="#panel_tab_' + num_tabs + '"]').tab('show');

}

$(function () {

    $(".main-navigation-menu").find("li:eq(0)").addClass("active");

    //var childHeight = $(".main-content").height();
    //$("#ifmMain").css("height", childHeight);


    $('.main-navigation-menu > li a').click(function () {
        var citem = $(this);

        //Active Menu
        if (citem.parent().parent().hasClass('main-navigation-menu'))
        {
            $('.main-navigation-menu li').removeClass('active');
            setTimeout(function () {
                $(citem).parent().addClass('active');
            }, 50);
            
        }


        $('.main-navigation-menu > li').each(function (index, ele) {
            if ($(this) == citem) alert('');
        });


        var url = $(this).attr('href');
        var menuName = $(this).text();
        if (url != undefined && url != null && url.length > 0)
        {
            // alert(url);

            var tabid = $(this).parent().attr('id');
            if (tabid == undefined || tabid.length == 0)
                tabid = 'default';

            //loadContentByLink(url);
            //loadContentTitle(menuName);

            createnewtab(menuName, url, tabid);
        }

        return false;
    });


    $("#btnAccountInfo").click(function () {
        var url = $(this).attr('href');

        createnewtab('Thông tin cá nhân', url, 'default');
        //loadContentByLink(url);

        $('.current-user').click();//close the menu
        return false;
    });

    //$(".todo-actions").click(function () {
    //    var url = $(this).attr('href');



    //    loadContentByLink(url);


    //    $('.dropdown-menu-title:eq(0)').click();//close the menu
    //    return false;
    //});

   
});

function loadContentByLink(url)
{

    
    //$("#ifmMain").attr("src", url);
}

function loadContentTitle(title)
{
    $("#contentTitle").html(title);
}

function reloadContent()
{

    var Currenttab = GetCurrentActiveTab();
   // alert(Currenttab);
    if (Currenttab != undefined) {
        //alert(JSON.stringify(Currenttab));
        document.getElementById(Currenttab.containername).contentWindow.location.reload(true);
    }
   // document.getElementById('ifmMain').contentWindow.location.reload(true);


}

