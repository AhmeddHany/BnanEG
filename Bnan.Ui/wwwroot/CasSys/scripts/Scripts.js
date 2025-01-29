// ******************************************************************sidebar toggle script*************************************************************************************//
// Variables
var bodyElement = document.querySelector("body"),
 sidebar = bodyElement.querySelector(".sidebar"),
 mainPageIcon = document.querySelector(".main-page-icon"),
 newPageIcon = document.querySelector(".new-Page-icon"),

 sidebar2 = bodyElement.querySelector(".sidebar2");
 const sidebar1Col = document.querySelector(".sidebar1-col");
 const sidebar2Col = document.querySelector(".sidebar2-col");

toggles1 = [
  {
    toggle: bodyElement.querySelector(".toggle"),
    sidebarToToggle: sidebar,
    sidebarToClose: sidebar2,
  },
  {
    toggle: bodyElement.querySelector(".toggle4"),
    sidebarToToggle: sidebar,
    sidebarToClose: sidebar2,
  },
];
toggles2 = [
  {
    toggle: bodyElement.querySelector(".toggle2"),
    sidebarToToggle: sidebar2,
    sidebarToClose: sidebar,
  },
];
// Event Listeners
toggles1.forEach(({ toggle, sidebarToToggle, sidebarToClose}) => {
  toggle.addEventListener("click", () => {
    
    const collapsibleElements = sidebar.querySelectorAll(".collapse");
    collapsibleElements.forEach((element) => {
      const collapseInstance = bootstrap.Collapse.getInstance(element);
      if (collapseInstance) {
        collapseInstance.hide();
      }
    });

    setTimeout(() => {
      sidebarToToggle.classList.toggle("close");
      sidebar1Col.classList.toggle("close");
      sidebarToClose.classList.add("close2");
      sidebar2Col.classList.add("close2");
      if (window.innerWidth > 1700) {
        
      if (mainPageIcon) {
        if (sidebarToToggle.classList.contains("close")) {
          mainPageIcon.style.left = ""; 
          mainPageIcon.style.bottom = "";
          newPageIcon.style.bottom = ""; 
          newPageIcon.style.left = ""; 
 
        } else {
          newPageIcon.style.left = "-60px";
          newPageIcon.style.bottom = "60px";
        }
      }
      }
    }, 300);
  });
});

toggles2.forEach(({ toggle, sidebarToToggle, sidebarToClose }) => {
  toggle.addEventListener("click", () => {
    sidebarToToggle.classList.toggle("close2");
    sidebarToToggle.classList.add("open");
    sidebar2Col.classList.toggle("close2");
    sidebar2Col.classList.add("open");
    const collapsibleElements = sidebar.querySelectorAll(".collapse");
    collapsibleElements.forEach((element) => {
      const collapseInstance = bootstrap.Collapse.getInstance(element);
      if (collapseInstance) {
        collapseInstance.hide();
      }
    });
    setTimeout(() => {
      sidebarToClose.classList.add("close");
      sidebar1Col.classList.add("close");

      if (window.innerWidth > 1700) {
        
        if (mainPageIcon) {
          if (sidebarToToggle.classList.contains("close2")) {
            mainPageIcon.style.left = ""; 
            mainPageIcon.style.bottom = ""; 
          } else {
            mainPageIcon.style.left = "-60px";
            mainPageIcon.style.bottom = "60px";
          }
        }
        }
    },300);
  });
});

// *******************************************************************************************************************************************************//

sidebar.addEventListener("mouseenter", function () {
  sidebar.classList.remove("close");
  sidebar1Col.classList.remove("close");

  sidebar2.classList.add("close2");
  sidebar2Col.classList.add("close2");

  if (window.innerWidth > 1700) {
    if (mainPageIcon) {
      if (sidebar.classList.contains("close")) {
        mainPageIcon.style.left = ""; 
        mainPageIcon.style.bottom = ""; 
        newPageIcon.style.bottom = "";
        newPageIcon.style.left = "";
      } else {
        // newPageIcon.style.left = "-60px";
        newPageIcon.style.bottom = "60px";
        mainPageIcon.style.left = "-60px"; 
        mainPageIcon.style.bottom = "";

      }
    }
  }
});

let collapseTimeout;
sidebar.addEventListener('mouseleave', function () {
  if (collapseTimeout) clearTimeout(collapseTimeout);
  
  collapseTimeout = setTimeout(() => {
    const collapsibleElements = sidebar.querySelectorAll('.collapse');
    collapsibleElements.forEach(element => {
      let collapseInstance = bootstrap.Collapse.getInstance(element);
      if (!collapseInstance) {
        collapseInstance = new bootstrap.Collapse(element, { toggle: false });
      }
      collapseInstance.hide();
    });

    sidebar.classList.add("close");
    sidebar1Col.classList.add("close");
    sidebar.classList.remove("close2");

    if (window.innerWidth > 1700 && mainPageIcon) {
      if (sidebar.classList.contains("close")) {
        mainPageIcon.style.left = ""; 
        mainPageIcon.style.bottom = ""; 
        newPageIcon.style.bottom = "";
        newPageIcon.style.left = "";


      } else {
        mainPageIcon.style.left = "-60px";
        mainPageIcon.style.bottom = "60px";
        newPageIcon.style.bottom = "60px";
        newPageIcon.style.left = "-60px";

      }
    }
  }, 300); 
});


//***************************************************************************main sidebar collapse script****************************************************************************** */

const collapsibleElements = sidebar.querySelectorAll(".collapse");

collapsibleElements.forEach((element) => {
  element.addEventListener("show.bs.collapse", function (event) {
    if (sidebar.classList.contains("close")) {
      event.preventDefault(); // Prevent the collapsible from opening if sidebar is closed
      return;
    }
    
    collapsibleElements.forEach((otherElement) => {
      if (otherElement !== element) {
        const collapseInstance = bootstrap.Collapse.getInstance(otherElement);
        if (collapseInstance) {
          collapseInstance.hide();
        }
      }
    });
  });
});

//**************************************************************************alert sidebar collapse script******************************************************************************* */

const collapse = document.querySelectorAll(".accordion-item");

collapse.forEach((item) => {
  item.querySelector(".accordion-item-header").addEventListener("click", () => {
    item.classList.toggle("open");
    collapse.forEach((otherElement) => {
      if (otherElement !== item) {
        otherElement.classList.remove("open");
      }
    });
  });
});

//*****************************************************************initialize tooltips*************************************************************************************** */

const tooltipTriggerList = document.querySelectorAll(
  '[data-bs-toggle="tooltip"]'
);
const tooltipList = [...tooltipTriggerList].map(
  (tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl)
);

// //***************************************************************** inputs collapses toggle *************************************************************************************** */
const inputsAccordion = document.querySelectorAll(".inputs-accordion-item");

inputsAccordion.forEach((item) => {
  item
    .querySelector(".accordion-item-header-icon")
    .addEventListener("click", () => {
      item.classList.toggle("open");
      inputsAccordion.forEach((otherElement) => {
        if (otherElement !== item) {
          otherElement.classList.remove("open");
        }
      });
    });
});
// //***************************************************************** move foucs between fields*************************************************************************************** */
document.addEventListener("DOMContentLoaded", function () {
  const accordionItems = document.querySelectorAll(".inputs-accordion-item");

  if (accordionItems.length > 0) {
    // Case for forms with accordion items
    console.log("ki")
    accordionItems.forEach((item) => {
      const header = item.querySelector(".accordion-item-header-icon");
      const focusableElements = item.querySelectorAll("input, select, textarea, button");

      // Set focus on the first input when the accordion item is clicked
      header.addEventListener("click", function () {
        setTimeout(() => {
          if (focusableElements.length > 0) {
            focusableElements[0].focus();
          }
        }, 300);
      });

      // Move focus to the next element on pressing Enter
      focusableElements.forEach((element, index) => {
        element.addEventListener("keydown", function (e) {
          if (e.key === "Enter") {
            e.preventDefault();
            const nextElement = focusableElements[index + 1];
            if (nextElement) {
              nextElement.focus();
            }
          }
        });
      });
    });
  }

  // Case for regular forms (not inside accordion)
  const regularFormElements = document.querySelectorAll("input, select, textarea");
  if (regularFormElements.length > 0) {
    regularFormElements[0].focus();
  }
  regularFormElements.forEach((element, index) => {
    element.addEventListener("keydown", function (e) {
      if (e.key === "Enter") {
        e.preventDefault();
        const nextElement = regularFormElements[index + 1];
        if (nextElement) {
          nextElement.focus();
        }
      }
    });
  });
});


// //********************************************************************** validation form submit ********************************************************************************** */
(() => {
  'use strict';

  const forms = document.querySelectorAll('.needs-validation');

  // Function to open an accordion item
  const openAccordionItem = (item, inputsAccordion) => {
      item.classList.add("open");
      inputsAccordion.forEach(otherElement => {
          if (otherElement !== item) {
              otherElement.classList.remove("open");
          }
      });
  };

  // Function to validate accordion sections or regular form inputs
  const validateAccordionSections = (form) => {
      const accordionItems = form.querySelectorAll('.inputs-accordion-item');
      let formIsValid = true;
      let firstInvalidInput = null;

      if (accordionItems.length > 0) {
          // Validation for forms with accordion sections
          Array.from(accordionItems).forEach(item => {
              const inputs = item.querySelectorAll('input, select, textarea'); // Scope to the current accordion item
              const checkIcon = item.querySelector('.data-check');
              let sectionIsValid = true;

              // Validate inputs within the accordion section
              Array.from(inputs).forEach(input => {
                  if (!input.checkValidity()) {
                      sectionIsValid = false;
                      if (!firstInvalidInput) {
                          firstInvalidInput = input; // Store the first invalid input

                          // Open the accordion section containing the first invalid input
                          openAccordionItem(item, accordionItems);
                      }
                  }
              });

              // Set checkIcon background color based on validity
              if (checkIcon) {
                  checkIcon.style.backgroundColor = sectionIsValid ? 'green' : '';
              }

              // Update overall form validity
              formIsValid = formIsValid && sectionIsValid;
          });

      } else {
          // Validation for regular forms (without accordion sections)
          const inputs = form.querySelectorAll('input, select, textarea');
          Array.from(inputs).forEach(input => {
              if (!input.checkValidity()) {
                  if (!firstInvalidInput) {
                      firstInvalidInput = input; // Store the first invalid input
                  }
                  formIsValid = false;
              }
          });
      }

      // Focus the first invalid input, if any
      if (firstInvalidInput) {
          firstInvalidInput.focus();
      }

      // Add 'was-validated' class to the form if invalid
      if (!formIsValid) {
          form.classList.add('was-validated');
      }

      return formIsValid;
  };

  // Function to validate and update check icons for each input field
  const validateInputFields = () => {
      const accordionItems = document.querySelectorAll('.inputs-accordion-item');

      accordionItems.forEach(item => {
          const inputs = item.querySelectorAll('input, select, textarea');
          const checkIcon = item.querySelector('.data-check');

          const validateInputs = () => {
              let allValid = true;
              inputs.forEach(input => {
                  if (!input.checkValidity()) {
                      allValid = false;
                  }
              });

              if (checkIcon) {
                  checkIcon.style.backgroundColor = allValid ? 'green' : '';
              }
          };

          inputs.forEach(input => {
              input.addEventListener("input", validateInputs);
              input.addEventListener("blur", validateInputs);
          });
      });
  };

  // Apply validation to all forms on submit
  Array.from(forms).forEach(form => {
      form.addEventListener('submit', event => {
          const isFormValid = validateAccordionSections(form);

          if (!isFormValid) {
              event.preventDefault();  
              event.stopPropagation();
          }
      }, false);
  });

  // Initialize input field validation on page load
  document.addEventListener("DOMContentLoaded", validateInputFields);

})();


// //********************************************************************** stop autocompelete ********************************************************************************** */

document.querySelectorAll('input, select, textarea').forEach(el => {
  el.setAttribute('autocomplete', 'off');
});


// //********************************************************************** number inputs script prevent entering negitive values ********************************************************************************** */
document.querySelectorAll('input[type="number"]').forEach(function(input) {
  input.addEventListener('keydown', function(event) {
    if (event.key === '-' || event.keyCode === 189) {
      event.preventDefault();
    }
  });
});
// //********************************************************************** email inputs script prevent entering arabic values ********************************************************************************** */
document.querySelectorAll('input[type="email"]').forEach(function(input) {
  input.addEventListener('input', function() {
    this.value = this.value.replace(/[\u0600-\u06FF]/g, '');
  });
});

