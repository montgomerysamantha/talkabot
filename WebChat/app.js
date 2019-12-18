setInterval(time, 10000);
setInterval(() => {
    if($(".list li").length > 5){
        $('.list li').first().fadeOut('slow', function () {
            $('.list li').first().remove();
        });
    }
}, 1000);

var socket = new WebSocket("ws://127.0.0.1:22199/");
socket.onopen = function (event) { };
socket.onmessage = function (e) {
    if ($(".list li").length >= 4) {
        $('.list li').first().fadeOut('slow', function () {
            $('.list li').first().remove();
        });
    }
    var msg = e.data.split(",");
    $(".main ul").append(`<li class="element"><div class="user">${msg[0]}</div><h1 class="comment">${msg[1]}</h1></li>`);
};

function time() {
    if ($(".list li").length >= 1) {
        $('.list li').eq(0).fadeOut('slow', function () {
            $('.list li').eq(0).remove();
        });
    }
}