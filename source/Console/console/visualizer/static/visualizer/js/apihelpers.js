function getAll() {
    var request = new XMLHttpRequest();
    var url = "/api/getall";

    request.responseType = "json";
    request.open('GET', url, true);
    request.send();
    request.onload = function() {
        console.log(this.status);
        if (this.readyState == 4 && this.status == 200) {
            var json = this.responseXML;
            drawGraph('drawingpane',json);
        }
        else {
            console.log("Error fetching data");
        }
    };
}
