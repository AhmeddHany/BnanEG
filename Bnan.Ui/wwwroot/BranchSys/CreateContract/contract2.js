jQuery(document).ready(function () {
    ImgUpload();
});

function HideFirstImg() {
    var firstImg = document.getElementById('upload-img1');
    firstImg.style.display = 'none';
}
var imgCheckUpArray = [];
function ImgUpload() {
    var imgWrap = '';

    $('.upload__inputfile').each(function () {
        $(this).on('change', function (e) {
            imgWrap = $(this).closest('.upload__box').find('.upload_img-wrap_inner');
            var maxLength = 12;
            var files = e.target.files;
            var filesArr = Array.prototype.slice.call(files);
            var uploadBtnBox = document.getElementById('checking-img');
            var uploadBtnBox2 = document.querySelector('.upload__btn');
            if (imgCheckUpArray.length + filesArr.length >= maxLength) {
                //uploadBtnBox.disabled = true;
                uploadBtnBox2.style.display = "none";
            } else {
                //uploadBtnBox.disabled = false;
                uploadBtnBox2.style.display = "block";
            }
            for (var i = 0; i < Math.min(filesArr.length, maxLength - imgCheckUpArray.length); i++) {
                (function (f) {
                    if (!f.type.match('image.*')) {
                        return;
                    }

                    var reader = new FileReader();
                    reader.onload = function (e) {
                        var html =
                            "<div class='upload__img-box'><div style='background-image: url(" +
                            e.target.result +
                            ")' data-number='" +
                            $('.upload__img-close').length +
                            "' data-file='" +
                            f.name +
                            "' class='img-bg'><div class='upload__img-close'><img src='/BranchSys/CreateContract/delete.png'></div></div></div>";
                        imgWrap.append(html);
                        imgCheckUpArray.push({
                            f: f,
                            url: e.target.result
                        });
                    };
                    reader.readAsDataURL(f);
                })(filesArr[i]);
            }
        });
    });

    $('body').on('click', '.upload__img-close', function (e) {
        e.stopPropagation(); // Prevent event bubbling to the .img-bg element
        var file = $(this).parent().data('file');

        for (var i = 0; i < imgCheckUpArray.length; i++) {
            if (imgCheckUpArray[i].f.name === file) {
                imgCheckUpArray.splice(i, 1);
                break;
            }
        }
        $(this).parent().parent().remove();
        var maxLength = 12;
        var uploadBtnBox = document.getElementById('checking-img');
        var uploadBtnBox2 = document.querySelector('.upload__btn');
        if (imgCheckUpArray.length == maxLength) {
            uploadBtnBox.disabled = true;
            uploadBtnBox2.style.display = "none";

        } else {
            uploadBtnBox.disabled = false;
            uploadBtnBox2.style.display = "block";
        }
    });

    $('body').on('click', '.img-bg', function (e) {
        var imageUrl = $(this).css('background-image');
        imageUrl = imageUrl.replace(/^url\(['"](.+)['"]\)/, '$1');
        var newTab = window.open();
        newTab.document.body.innerHTML = '<img src="' + imageUrl + '">';

        $(newTab.document.body).css({
            'background-color': 'black',
            display: 'flex',
            'align-items': 'center',
            'justify-content': 'center',
        });
    });

}





