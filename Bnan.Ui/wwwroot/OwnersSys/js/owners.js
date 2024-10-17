!(function (l) {
  // WOW active
  new WOW().init();
  "use strict";

  if (window.innerWidth <= 767)
    {
      l("body").addClass("sidebar-toggled");
      l(".sidebar").addClass("toggled");
    }
    l("#sidebarToggle, #sidebarToggleTop").on("click", function (e) {
      new WOW({
        boxClass: 'wow',
        animateClass: 'animated',
        offset: 0,
        mobile: true,
        live: true,
        callback: function(box) {
          if (l(box).closest('#accordionSidebar').length > 0) {
            new WOW().init();
          }
        }
      }).init();
    l("body").toggleClass("sidebar-toggled");
    l(".sidebar").toggleClass("toggled");
   
  });

  
  l(document).on("scroll", function () {
    100 < l(this).scrollTop()
      ? l(".scroll-to-top").fadeIn()
      : l(".scroll-to-top").fadeOut();
  });

  l(document).on("click", "a.scroll-to-top", function (e) {
    var o = l(this);
    l("html, body")
      .stop()
      .animate(
        { scrollTop: l(o.attr("href")).offset().top },
        1e3,
        "easeInOutExpo"
      );
    e.preventDefault();
  });

  l("#sidebarToggle, #sidebarToggleTop").on("click", function (e) {
    const Layer = document.querySelector('.layer');
    const sidebar = document.querySelector('.sidebar');
    const isSideNavClosed = !sidebar.classList.contains('toggled');

    if (window.innerWidth <= 767 && isSideNavClosed) {
      Layer.classList.add('layerStyle');
    } else {
      Layer.classList.remove('layerStyle');
    }
  });

  const Layer = document.querySelector('.layer');
  const sidebar = document.querySelector('.sidebar');
  const isSideNavClosed = !sidebar.classList.contains('toggled');

  if (window.innerWidth <= 767 && isSideNavClosed) {
    Layer.classList.add('layerStyle');
  } else {
    Layer.classList.remove('layerStyle');
  }

})(jQuery);
  

/////////////////////////language change//////////////////////////////////

//const checkbox = document.getElementById('myCheckbox');
//const label2 = document.getElementById('checkboxLabel');

//checkbox.addEventListener('change', function() {
//  const isChecked = checkbox.checked;
//  setCustomCSS(isChecked);
//  if (isChecked) {
//    label2.innerHTML = '  AR <i class="fa-solid fa-globe lang_globe"></i> ';
//  } else {
//    label2.innerHTML = '  <i class="fa-solid fa-globe lang_globe"></i> EN';
//  }
//});

//function setCustomCSS(isChecked) {
//  if (isChecked) {
//    localStorage.setItem('customCSS', 'true');
//  } else {
//    localStorage.removeItem('customCSS');
//  }

//  window.postMessage({ customCSS: isChecked }, '*');
//}

//const isChecked = localStorage.getItem('customCSS') === 'true';
//checkbox.checked = isChecked;
//setCustomCSS(isChecked);
//if (isChecked) {
//  label2.innerHTML = '  AR <i class="fa-solid fa-globe lang_globe"></i>';
//}
//window.addEventListener('DOMContentLoaded', function() {
//  const customCSS = localStorage.getItem('customCSS') === 'true';
//  const cssFile = document.getElementById('customCSS');

//  if (customCSS) {
//    if (!cssFile) {
//      addCustomCSS();
//    }
//  } else {
//    if (cssFile) {
//      removeCustomCSS();
//    }
//  }
//});

//window.addEventListener('message', function(event) {
//  if (event.data.customCSS) {
//    addCustomCSS();
//  } else {
//    removeCustomCSS();
//  }
//});

//function addCustomCSS() {
//  const cssFile = document.createElement('link');
//  cssFile.rel = 'stylesheet';
//  cssFile.href = 'css/En_Style.css';
//  cssFile.id = 'customCSS';
//  document.head.appendChild(cssFile);
//}

//function removeCustomCSS() {
//  const cssFile = document.getElementById('customCSS');
//  if (cssFile) {
//    cssFile.parentNode.removeChild(cssFile);
//  }
//}

