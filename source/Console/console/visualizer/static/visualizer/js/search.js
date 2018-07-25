//requires draw-d3.js

function search() {

/*Search area DOM ids
node1
relationship
node2*/
	var node1 = document.getElementById("node1").value;
	let relationship = document.getElementById("relationship").value;
	let node2 = document.getElementById("node2").value;

	//getAll("/api/getall");
	//$.getJSON(url, function(data) {
	getNode(node1);
}

function getNode(nodeid) {
	console.log(nodeid);
	$.getJSON("/api/nodes/"+nodeid, function(data) {
    	addResultSet(data);
        restartLayout();
    });
}

function getAll() {
    //var url = "/api/getall";

    $.getJSON("/api/getall", function(data) {
    	addResultSet(data);
        restartLayout();
    });
}

function addRelated(nodeid) {
	console.log("addRelated");
	$.getJSON("/api/nodes?nodeid="+nodeid, function(data) {
    	addResultSet(data);
    	let nodeids = getAllNodeIds();
    	getEdgesForNodes(nodeids);
    });

}

function getEdgesForNodes(nodeids) {
	console.log("getEdgesForNodes");
	console.log(nodeids);
	$.post("/api/edges", nodeids, function(data) {
		console.log(data);
    	addResultSet(data);
    	restartLayout();
    }),"json";
}