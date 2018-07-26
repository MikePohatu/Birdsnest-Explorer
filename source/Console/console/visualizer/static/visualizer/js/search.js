//requires draw-d3.js

function search() {

/*Search area DOM ids
node1
relationship
node2*/
	var node1 = document.getElementById("node1").value;
	let relationship = document.getElementById("relationship").value;
	let node2 = document.getElementById("node2").value;

	getNode(node1);
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
	console.log(nodeid);
	$.getJSON("/api/nodes/node/"+nodeid, function(data) {
    	addResultSet(data);
        restartLayout();
    });
}

function getAll() {
    $.getJSON("/api/getall", function(data) {
    	addResultSet(data);
        restartLayout();
    });
}

function addRelated(nodeid) {
	console.log("addRelated"+ nodeid);
	$.getJSON("/api/nodes?nodeid="+nodeid, function(data) {
    	addResultSet(data);
    	let nodeids = getAllNodeIds();
    	getEdgesForNodes(nodeids);    	
    });

}

function getEdgesForNodes(nodeids) {
	console.log("getEdgesForNodes");
	console.log(nodeids);
	$.ajax({
		url: '/api/edges',
		method: "POST",
		data: JSON.stringify(nodeids),
		contentType: "application/json; charset=utf-8",
		headers: {
			'X-CSRFToken': getCookie('csrftoken')
		},
		success: function(data) {
			console.log(data);
	    	addResultSet(data);
	    	restartLayout();
	   		}
	});
}