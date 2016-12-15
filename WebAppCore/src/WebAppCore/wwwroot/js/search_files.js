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
}

function start_Seacrh() {
    
    $("#img-load").show(500);
    $("#search-list").hide(500);
    $("#message-search").hide(500);
}