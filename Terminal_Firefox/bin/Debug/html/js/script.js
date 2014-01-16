/* Animate button click */

function mouseUp(img) {
    if (img.src.lastIndexOf("_off.png") > 0) {
        var a = img.src;
        var b = a.lastIndexOf("_off.png");
        var c = a.substring(0, b);
        img.src = c + ".png";
    }
}

function mouseDown(img) {
    if (img.src.lastIndexOf("_off.png") > 0) {
    } else {
        var a = img.src;
        var b = a.lastIndexOf(".");
        var c = a.substring(0, b);
        img.src = c + "_off.png";
    }
}
/* Animate button click */


function goDependent() {
    window.location = "dependent.html";
}

function goEnterNumber() {
    window.location = "enter_number.html";
}

function goPay(){
    window.location = "pay.html"
}

