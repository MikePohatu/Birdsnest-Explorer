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

function sourceNameKeyUp() {
	timedKeyUp( function(d) {
		var el = document.getElementById("sourceName");
		var searchterm = el.value;
		console.log(searchterm);
		searchNodeNames(el,searchterm,20);
	});
}

function sourcePropKeyUp() {
	timedKeyUp( function(d) {
		var searchterm = document.getElementById("sourceProp").value;
		console.log(searchterm);
	});
}

function sourceLabelKeyUp() {
	timedKeyUp( function(d) {
		var searchterm = document.getElementById("sourceLabel").value;
		console.log(searchterm);
	});
}


function searchNodeNames(element, term, limit) {
	console.log("searchNodeNames");
	if (!isNullOrEmpty(term)) {
		$.getJSON('/api/search/nodenames?term=' + term + '&limit=' + limit, function(data) {
			console.log(data);
			autocomplete(element, data);
		});
	}	
}

function isNullOrEmpty( s ) 
{
    return ( s == null || s === "" );
}

function autocomplete(inp, arr) {
	/*the autocomplete function takes two arguments,
	the text field element and an array of possible autocompleted values:*/
	var currentFocus;
	/*execute a function when someone writes in the text field:*/
	inp.addEventListener("input", function(e) {
		var a, b, i, val = this.value;
		/*close any already open lists of autocompleted values*/
		closeAllLists();
		if (!val) { return false;}
		currentFocus = -1;
		/*create a DIV element that will contain the items (values):*/
		a = document.createElement("DIV");
		a.setAttribute("id", this.id + "autocomplete-list");
		a.setAttribute("class", "autocomplete-items");
		/*append the DIV element as a child of the autocomplete container:*/
		this.parentNode.appendChild(a);
		/*for each item in the array...*/
		for (i = 0; i < arr.length; i++) {
			/*check if the item starts with the same letters as the text field value:*/
			if (arr[i].substr(0, val.length).toUpperCase() == val.toUpperCase()) {
				/*create a DIV element for each matching element:*/
				b = document.createElement("DIV");
				/*make the matching letters bold:*/
				b.innerHTML = "<strong>" + arr[i].substr(0, val.length) + "</strong>";
				b.innerHTML += arr[i].substr(val.length);
				/*insert a input field that will hold the current array item's value:*/
				b.innerHTML += "<input type='hidden' value='" + arr[i] + "'>";
				/*execute a function when someone clicks on the item value (DIV element):*/
				b.addEventListener("click", function(e) {
					/*insert the value for the autocomplete text field:*/
					inp.value = this.getElementsByTagName("input")[0].value;
					/*close the list of autocompleted values,
					(or any other open lists of autocompleted values:*/
					closeAllLists();
					});
				a.appendChild(b);
			}
		}
	});
	/*execute a function presses a key on the keyboard:*/
	inp.addEventListener("keydown", function(e) {
		var x = document.getElementById(this.id + "autocomplete-list");
		if (x) x = x.getElementsByTagName("div");
		if (e.keyCode == 40) {
			/*If the arrow DOWN key is pressed,
			increase the currentFocus variable:*/
			currentFocus++;
			/*and and make the current item more visible:*/
			addActive(x);
		}
		else if (e.keyCode == 38) { //up
			/*If the arrow UP key is pressed,
			decrease the currentFocus variable:*/
			currentFocus--;
			/*and and make the current item more visible:*/
			addActive(x);
		}
		else if (e.keyCode == 13) {
			/*If the ENTER key is pressed, prevent the form from being submitted,*/
			e.preventDefault();
			if (currentFocus > -1) {
				/*and simulate a click on the "active" item:*/
				if (x) x[currentFocus].click();
			}
		}
	});

	function addActive(x) {
		/*a function to classify an item as "active":*/
		if (!x) return false;
		/*start by removing the "active" class on all items:*/
		removeActive(x);
		if (currentFocus >= x.length) currentFocus = 0;
		if (currentFocus < 0) currentFocus = (x.length - 1);
		/*add class "autocomplete-active":*/
		x[currentFocus].classList.add("autocomplete-active");
	}

	function removeActive(x) {
		/*a function to remove the "active" class from all autocomplete items:*/
		for (var i = 0; i < x.length; i++) {
			x[i].classList.remove("autocomplete-active");
		}
	}

	function closeAllLists(elmnt) {
		/*close all autocomplete lists in the document,
		except the one passed as an argument:*/
		var x = document.getElementsByClassName("autocomplete-items");
		for (var i = 0; i < x.length; i++) {
			if (elmnt != x[i] && elmnt != inp) {
				x[i].parentNode.removeChild(x[i]);
			}
		}
	}

	/*execute a function when someone clicks in the document:*/
	document.addEventListener("click", function (e) {
	closeAllLists(e.target);
	});
}