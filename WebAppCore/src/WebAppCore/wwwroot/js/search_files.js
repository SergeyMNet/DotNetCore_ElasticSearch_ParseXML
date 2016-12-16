// getElementById
function $id(id) {
    return document.getElementById(id);
}

Init();

//
// initialize
function Init() {
    var button = $id("sub-button");
    button.addEventListener("click", start_Seacrh, false);

    var count = $('#sel_cat').find('option').length;

    if (count === 1) {
        category_set();
    }
}

function start_Seacrh() {
    
    $("#img-load").show(500);
    $("#search-list").hide(500);
    $("#message-search").hide(500);
}


function category_changed() {

    var cat = $('#sel_cat')[0].value;

    $('#sel_field')
        .find('option')
        .remove()
        .end();
    
    $.ajax({
        type: "GET",
        url: "http://localhost:3387/Book/GetFields?cat=" + cat,
        success:
            function (obj) {
                //alert(obj);

                $.each(obj, function (key, value) {
                    $('#sel_field')
                        .append($("<option></option>")
                                   .attr("value", value)
                                   .text(value));
                });

            },
        error:
            function () {
                alert("error category_changed");
            }
    });
}


function category_set() {

    $('#sel_cat')
        .find('option')
        .remove()
        .end();

    $.ajax({
        type: "GET",
        url: "http://localhost:3387/Book/GetCategories",
        success:
            function (obj) {
                //alert(obj);

                $.each(obj, function (key, value) {
                    $('#sel_cat')
                        .append($("<option></option>")
                                   .attr("value", value)
                                   .text(value));
                });

            },
        error:
            function () {
                alert("error category_set");
            }
    });
}


function search() {

    var url = "http://localhost:3387/Book/Search";

    var data = {
        Category: "_all",
        Field: "_all",
        SearchQuery: "ch"
    };


    $.ajax({
            type: "POST",
            dataType: "application/json",
            url: url,
            data: data,
            success:
                function(o) {
                    alert("OK search");
                }
            
        })
        .onsuccess(
                function (o) {
                    alert("OK search");
                }
        );
    
    ;

    
}