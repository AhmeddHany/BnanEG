document.addEventListener("DOMContentLoaded", function () {
  document.body.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
      document.getElementById("contract-extension-form").onsubmit = function (event) {
        event.preventDefault();
        console.log("Search Query:", document.getElementById("search").value);
      };
    }
  });
});
//////////////////////////////////////////////////////////////////////////////////////
document.addEventListener("DOMContentLoaded", function() {
  var table = document.getElementById("extensionTable");
  var rows = table.getElementsByTagName("tr");

  for (var i = 0; i < rows.length; i++) {
    var cells = rows[i].getElementsByTagName("td");

    for (var j = 1; j < cells.length; j++) { 
      cells[j].addEventListener("click", function() {
        window.location.href = "Extension.html"; 
      });
    }
  }
});

/////
var rows = document.getElementById("extensionTable").getElementsByTagName("tr");
for (var i = 0; i < rows.length; i++) {
  rows[i].onclick = function () {
    var cells = this.getElementsByTagName("td");
    var tenantIdParagraph = cells[4].getElementsByClassName("contract-id")[0];
    console.log(tenantIdParagraph.innerText);
  };
}
