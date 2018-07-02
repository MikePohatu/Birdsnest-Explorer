/* Set the width of the side navigation to 250px and the left margin of the page content to 250px */
function openNav() {
	document.getElementById("openclosebtn").onclick = function() { 
            closeNav(); 
        };
	document.getElementById("openclosebtn").innerHTML = "&lt;";
	document.getElementById("sidemenu").style.width = "250px";
	document.getElementById("openclosebox").style.left = "250px";
	document.getElementById("main").style.left = "250px";
}

/* Set the width of the side navigation to 0 and the left margin of the page content to 0 */
function closeNav() {
	document.getElementById("openclosebtn").onclick = function() { 
            openNav(); 
        };
	document.getElementById("openclosebtn").innerHTML = "&gt;";
	document.getElementById("sidemenu").style.width = "0";
	document.getElementById("openclosebox").style.left = "0";
	document.getElementById("main").style.left = "0px";
}