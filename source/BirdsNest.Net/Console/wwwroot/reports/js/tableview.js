var maxrecords = 100;
var pagenum = 1;
var resultset = JSON.parse(results.value);
var tHeader = document.getElementById('outputHeader');
var tBody = document.getElementById('outputBody');
var el;
var columnstyleprefix = "col-";
var toggleprefix = "toggle-";
var columnstoggles = document.getElementById("columnstoggles");
var recordcount = resultset.Nodes.length + resultset.Edges.length;
var maxpagenum = Math.ceil(recordcount / maxrecords);

document.getElementById("reportmenutext").innerHTML = "Query returned " + recordcount + " records";
console.log(resultset);
if (recordcount === 0) {
    document.getElementById("columnsli").classList.add("hidden");
    document.getElementById("visualizerli").classList.add("hidden");
    document.getElementById("downloadli").classList.add("hidden");
    document.getElementById("pages").classList.add("hidden");
} else {
    if (recordcount < maxrecords) { document.getElementById("pages").classList.add("hidden"); }

    document.getElementById("status").innerHTML = "Building results table. Please wait...";


    Object.keys(resultset.PropertyFilters).forEach(function (key) {
        addTH(key);
    });

    Object.keys(resultset.PropertyNames).forEach(function (key) {
        if (!(key in resultset.PropertyFilters)) {
            addTH(key);
        }
    });

    setTimeout(buildBody(), 50);
}




function buildBody() {
    var toprecord = pagenum * maxrecords;
    var bottomrecord = (pagenum - 1) * maxrecords;
    var count = 0;

    //console.log("buildBody page: " + pagenum + " top:" + toprecord + " bottom:" + bottomrecord)
    document.getElementById("pagemessage").innerHTML = "Page " + pagenum + " of " + maxpagenum;

    //add the cells
    var BreakException = {};
    try {
        resultset.Nodes.forEach(function (node) {
            if (count === toprecord) { throw BreakException; }
            if (count >= bottomrecord) {
                addRecord(node);
            }
            count++;
        });

        resultset.Edges.forEach(function (rel) {
            if (count === toprecord) { throw BreakException; }
            if (count >= bottomrecord) {
                addRecord(rel);
            }
            count++;
        });
    }
    catch (e) {
        if (e !== BreakException) throw e;
    }


    //update visibility to hide unneeded columns
    if (resultset.PropertyFiltersApplied === true) {

        Object.keys(resultset.PropertyNames).forEach(function (key) {
            var shown = (key in resultset.PropertyFilters);
            SetClassVisible(columnstyleprefix + key, shown);
            var togglebox = document.getElementById(toggleprefix + key);
            togglebox.checked = shown;
        });
    }
    else {
        Object.keys(resultset.PropertyNames).forEach(function (key) {
            SetClassVisible(columnstyleprefix + key, true);
            var togglebox = document.getElementById(toggleprefix + key);
            togglebox.checked = true;
        });
    }
    document.getElementById("status").innerHTML = "";

    function addRecord(node) {

        var contents;
        var row = tBody.insertRow();

        function addTD(key) {
            if (key in node.properties) { contents = node.properties[key]; }
            else { contents = ""; }

            el = document.createElement("TD");
            el.classList.add(columnstyleprefix + key);
            el.innerHTML = contents;
            row.appendChild(el);
        }

        Object.keys(resultset.PropertyFilters).forEach(function (key) {
            addTD(key);
        });

        Object.keys(resultset.PropertyNames).forEach(function (key) {
            if (!(key in resultset.PropertyFilters)) {
                addTD(key);
            }
        });
    }
}

function addTH(key) {
    el = document.createElement("TH");
    el.classList.add(columnstyleprefix + key);
    el.classList.add("visible");
    el.innerHTML = key;
    tHeader.appendChild(el);

    //<li>
    //        <label class="filter" for="toggle-dn">
    //            <input id="toggle-dn" type="checkbox" data-label="dn" />
    //            <text>TEST</text>
    //        </label>
    //</li>
    var li = document.createElement("li");


    var label = document.createElement("label");
    label.attributes.for = toggleprefix + key;
    label.classList.add("toggleitem");

    var textpart = document.createTextNode(key);

    var checkbox = document.createElement("input");
    checkbox.classList.add("toggleitem");
    checkbox.id = toggleprefix + key;
    checkbox.type = "checkbox";
    checkbox.onchange = onToggleClicked;
    checkbox.dataset.label = key;

    li.appendChild(label);
    label.appendChild(checkbox);
    label.appendChild(textpart);
    columnstoggles.appendChild(li);
}

function onPageUpClicked() {
    if (pagenum < maxpagenum) {
        pagenum++;
        tBody.innerHTML = "";
        buildBody();
    }
}

function onPageDownClicked() {
    if (pagenum > 1) {
        pagenum--;
        tBody.innerHTML = "";
        buildBody();
    }
}

function onToggleClicked() {
    var el = event.target;
    var key = columnstyleprefix + el.dataset.label;
    SetClassVisible(key, el.checked);
}

function SetClassVisible(classname, show) {
    //console.log("SetClassVisible: " + classname + ":" + show);
    var classelements = document.getElementsByClassName(classname);

    for (var i = 0; i < classelements.length; i++) {

        if (show === true) {
            classelements[i].classList.add('visible');
            classelements[i].classList.remove('hidden');
        } else {
            classelements[i].classList.add('hidden');
            classelements[i].classList.remove('visible');
        }
    }
}

function onVisualizerClicked() {
    if (typeof (Storage) !== "undefined") {
        sessionStorage.birdsnest_resultset = JSON.stringify(resultset);
        window.location.href = "/visualizer?usestored=true";
        // Code for localStorage/sessionStorage.
    } else {
        console.log("No Web Storage support..");
        // Sorry! No Web Storage support..
    }
}

function onDownloadClicked() {
    //console.log("onDownloadClicked");
    var text = "";
    var visiblecols = tHeader.querySelectorAll(".visible");
    var props = [];

    for (var i = 0; i < visiblecols.length; i++) {

        var celltext = "";
        celltext = visiblecols[i].innerText === null ? "" : visiblecols[i].innerText;
        props.push(celltext);
    }
    text = text + (props.join(",")) + "\n";

    resultset.Nodes.forEach(function (node) {
        var row = createRowArray(node);
        text = text + (row.join(",")) + "\n";
    });

    resultset.Edges.forEach(function (node) {
        var row = createRowArray(node);
        text = text + (row.join(",")) + "\n";
    });

    function createRowArray(record) {
        var recordrow = [];
        //console.log(node);
        props.forEach(function (prop) {
            var celltext = "";
            if (prop in record.properties) {
                celltext = record.properties[prop];
                celltext = celltext.replace("\"", "\"\"");
                celltext = "\"" + celltext + "\"";
            }

            recordrow.push(celltext);
        });
        return recordrow;
    }

    Download(text, "results.csv", 'text/csv;encoding:utf-8');
}

//https://stackoverflow.com/questions/14964035/how-to-export-javascript-array-info-to-csv-on-client-side
// The download function takes a CSV string, the filename and mimeType as parameters
// Scroll/look down at the bottom of this snippet to see how download is called
function Download(content, fileName, mimeType) {
    //console.log("Download:");
    //console.log(content);
    var a = document.createElement('a');
    mimeType = mimeType || 'application/octet-stream';

    if (navigator.msSaveBlob) { // IE10
        navigator.msSaveBlob(new Blob([content], {
            type: mimeType
        }), fileName);
    } else if (URL && 'download' in a) { //html5 A[download]
        //console.log("HTML5");
        a.href = URL.createObjectURL(new Blob([content], {
            type: mimeType
        }));
        //a.href = 'data:application/octet-stream,' + encodeURIComponent(content);
        a.setAttribute('download', fileName);
        document.body.appendChild(a);
        //a.onclick = function () { console.log("clicked"); };
        //a.innerHTML = "Ready";
        a.click();
        document.body.removeChild(a);

    } else {
        location.href = 'data:application/octet-stream,' + encodeURIComponent(content); // only this mime type is supported
    }
}