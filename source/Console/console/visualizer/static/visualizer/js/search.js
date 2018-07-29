//requires draw-d3.js
function search() {

console.log('search');
/*Search area DOM ids
node1
relationship
node2*/
	

	var sourcetype = document.getElementById("sourceType").value;
	var sourceprop = document.getElementById("sourceProp").value;
	var sourceval = document.getElementById("sourceVal").value;

	var relationship = document.getElementById("edgeType").value;

	var tartype = document.getElementById("targetType").value;
	var tarprop = document.getElementById("targetProp").value;
	var tarval = document.getElementById("targetVal").value;

	let url = "/api/search/path?" + 
		"sourcetype="+sourcetype +
		"&sourceprop="+sourceprop +
		"&sourceval="+sourceval +

		"&relationship="+relationship +

		"&tartype="+tartype +
		"&tarprop="+tarprop +
		"&tarval="+tarval;
	//let node2 = document.getElementById("node2").value;

	console.log("sourcetype: " + sourcetype);
	console.log("sourceprop: " + sourceprop);
	console.log("sourceval: " + sourceval);

	console.log("relationship: " + relationship);

	console.log("tartype: " + tartype);
	console.log("tarprop: " + tarprop);
	console.log("tarval: " + tarval);

	console.log(url);

	$.getJSON(url, function(data) {
		console.log(data);
    	addResultSet(data);
    	updateEdges();
        restartLayout();
    });
}


//getCookie function from django documentation
function getCookie(name) {
    var cookieValue = null;
    if (document.cookie && document.cookie !== '') {
        var cookies = document.cookie.split(';');
        for (var i = 0; i < cookies.length; i++) {
            var cookie = jQuery.trim(cookies[i]);
            // Does this cookie string begin with the name we want?
            if (cookie.substring(0, name.length + 1) === (name + '=')) {
                cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
                break;
            }
        }
    }
    return cookieValue;
}


function getNode(nodeid) {
	//console.log(nodeid);
	$.getJSON("/api/nodes/node/"+nodeid, function(data) {
		//console.log(data);
    	addResultSet(data);
        restartLayout();
        updateEdges();
    });
}

function getAll() {
    $.getJSON("/api/getall", function(data) {
    	addResultSet(data);
        restartLayout();
    });
}

function addRelated(nodeid) {
	//console.log("addRelated"+ nodeid);
	$.getJSON("/api/nodes?nodeid="+nodeid, function(data) {
    	addResultSet(data);
   		updateEdges();
    });

}

function getEdgesForNodes(nodeids) {
	//console.log("getEdgesForNodes");
	//console.log(nodeids);
	$.ajax({
		url: '/api/edges',
		method: "POST",
		data: JSON.stringify(nodeids),
		contentType: "application/json; charset=utf-8",
		headers: {
			'X-CSRFToken': getCookie('csrftoken')
		},
		success: function(data) {
			//console.log(data);
	    	addResultSet(data);
	    	restartLayout();
	   		}
	});
}

function updateEdges() {
	let nodeids = getAllNodeIds();
    getEdgesForNodes(nodeids); 
}

// Keystroke event handlers
var typetimer = null;

function timedKeyUp(timedfunction) {
    clearTimeout(typetimer);
    typetimer = setTimeout(timedfunction, 700);
}

function sourceValKeyUp() {
	timedKeyUp( function(d) {
		console.log('sourceValKeyUp');
		searchValues('source');
	});
}



/*function searchNodeNames(element, term, limit) {
	console.log("searchNodeNames");
	if (!isNullOrEmpty(term)) {
		$.getJSON('/api/search/nodenames?term=' + term + '&limit=' + limit, function(data) {
			console.log(data);
			autocomplete(element, data);
		});
	}	
}*/

function isNullOrEmpty( s ) 
{
    return ( s == null || s === "" );
}



//populate 
function addOption (selectbox, text, value) {
    var o = document.createElement("OPTION");
    o.text = text;
    o.value = value;
    selectbox.options.add(o);  
}

function addLabelOptions(selectbox, labelList) {
	for (var i = 0; i < labelList.length; ++i) {
		addOption(selectbox, labelList[i], labelList[i])
	}
}

function clearOptions(selectbox)
{
	selectbox.options.length = 0;
}

function updateProps(elementPrefix) {
	var type = document.getElementById(elementPrefix+"Type").value;
	var elprops = document.getElementById(elementPrefix+"Prop");
	
	clearOptions(elprops);
	$.getJSON("/api/nodes/properties?type="+type, function(data) {
	    for (var i = 0; i < data.length; ++i) {
			addOption(elprops, data[i], data[i])
		}
	});
}

bindAutoComplete("source");
bindAutoComplete("target");
function bindAutoComplete(elementPrefix) {
	$("#"+elementPrefix+"Val").autocomplete({
		source: function (request, response) {
			//console.log("autoComplete: "+ request.term);
			var type = document.getElementById(elementPrefix+"Type").value;
			var prop = document.getElementById(elementPrefix+"Prop").value;

			var url = "/api/nodes/values?type="+type+"&property="+prop+"&searchterm="+request.term;
			$.getJSON(url,response);
		}
	});
}

