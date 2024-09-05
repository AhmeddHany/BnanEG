
let current_fs, next_fs, previous_fs;
const ExpensesCheckbox = document.getElementById('expenses');
const CompensationCheckbox = document.getElementById('compensation-check');
var compensationArray = [];
var ExpensesArray = [];

jQuery(document).ready(function () {

    ExpensesImgUpload();
    compensationImgUpload();
    examinationImgUpload();
});


//====================================================================================================
//========================================Upload Expenses Imgs Lists============================================================
//====================================================================================================
var imgExpenses = [];
function ExpensesImgUpload() {
    var imgWrap = '';
    var ExpensesArray = [];
    $('#Expenses-images').each(function () {

        $(this).on('change', function (e) {
            imgWrap = $(this).closest('.upload__box').find('.upload_img-wrap_inner');
            var maxLength = 4;
            var uploadBtnBox = document.getElementById('Expenses-images');
            var errorMessageDiv = document.getElementById('ExpensesError');
            var files = e.target.files;
            var filesArr = Array.prototype.slice.call(files);

            var btn_expense = document.getElementById('btn_expense');

            if (ExpensesArray.length + filesArr.length >= maxLength) {
                btn_expense.style.display = "none";
            } else {
                btn_expense.style.display = "flex";
            }

            for (var i = 0; i < Math.min(filesArr.length, maxLength - ExpensesArray.length); i++) {
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
                            $('.upload__img-close1').length +
                            "' data-file='" +
                            f.name +
                            "' class='img-bg'><div class='upload__img-close1'><img src='/BranchSys/Pages/img/delete.png'></div></div></div>";
                        imgWrap.append(html);
                        ExpensesArray.push({
                            f: f,
                            url: e.target.result
                        });
                        imgExpenses = ExpensesArray;
                    };
                    reader.readAsDataURL(f);
                })(filesArr[i]);
            }
        });
    });

    $('body').on('click', '.upload__img-close1', function (e) {
        e.stopPropagation();
        var file = $(this).parent().data('file');

        for (var i = 0; i < ExpensesArray.length; i++) {
            if (ExpensesArray[i].f.name === file) {
                ExpensesArray.splice(i, 1);
                break;
            }
        }

        $(this).parent().parent().remove();
        console.log(ExpensesArray);

        var maxLength = 4;
        var uploadBtnBox = document.getElementById('Expenses-images');
        var errorMessageDiv = document.getElementById('ExpensesError');

        if (ExpensesArray.length >= maxLength) {
            btn_expense.style.display = "none";
        } else {
            btn_expense.style.display = "flex";
        }
    });

    $('body').on('click', '.img-bg', function (e) {
        var imageUrl = $(this).css('background-image');
        $('#preview-image').attr('src', imageUrl);
        $('#image-preview').modal('show');
    });
}
//====================================================================================================
//========================================Upload compensation Imgs Lists============================================================
//====================================================================================================
var imgCompensations = [];
function compensationImgUpload() {
    var imgWrap = '';
    var compensationArray = [];
    $('#compensation-images').each(function () {
        $(this).on('change', function (e) {
            imgWrap = $(this).closest('.upload__box').find('.upload_img-wrap_inner');
            var uploadBtnBox = document.getElementById('compensation-images');
            var maxLength = 4;
            var files = e.target.files;
            var filesArr = Array.prototype.slice.call(files);

            var btn_compensation = document.getElementById('btn_compensation');

            if (compensationArray.length + filesArr.length >= maxLength) {
                btn_compensation.style.display = "none";
            } else {
                btn_compensation.style.display = "flex";
            }

            for (var i = 0; i < Math.min(filesArr.length, maxLength - compensationArray.length); i++) {
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
                            $('.upload__img-close2').length +
                            "' data-file='" +
                            f.name +
                            "' class='img-bg'><div class='upload__img-close2'><img src='/BranchSys/Pages/img/delete.png'></div></div></div>";
                        imgWrap.append(html);
                        compensationArray.push({
                            f: f,
                            url: e.target.result
                        });
                        imgCompensations = compensationArray;
                    };
                    reader.readAsDataURL(f);
                })(filesArr[i]);
            }
        });
    });

    $('body').on('click', '.upload__img-close2', function (e) {
        e.stopPropagation();
        var file = $(this).parent().data('file');

        for (var i = 0; i < compensationArray.length; i++) {
            if (compensationArray[i].f.name === file) {
                compensationArray.splice(i, 1);
                break;
            }
        }

        $(this).parent().parent().remove();
        console.log(compensationArray);

        var maxLength = 4;
        var uploadBtnBox = document.getElementById('compensation-images');
        var errorMessageDiv = document.getElementById('compensationError');
        if (compensationArray.length >= maxLength) {
            btn_compensation.style.display = "none";            
        } else {
            btn_compensation.style.display = "flex";
        }
    });

    $('body').on('click', '.img-bg', function (e) {
        var imageUrl = $(this).css('background-image');
        $('#preview-image').attr('src', imageUrl);
        $('#image-preview').modal('show');
    });
}
//====================================================================================================
//========================================Upload examination Imgs Lists============================================================
//====================================================================================================
var imgCheckUpArray=[];
function examinationImgUpload() {
    var imgWrap = '';
    var examinationArray=[]
    var uploadBtnBox = document.getElementById('examination-images');
    var btn_checkup = document.getElementById('btn_checkup');
    $('#examination-images').each(function () {
        $(this).on('change', function (e) {
            imgWrap = $(this).closest('.upload__box').find('.upload_img-wrap_inner');
            var maxLength = 12;
            var files = e.target.files;
            var filesArr = Array.prototype.slice.call(files);

            if (examinationArray.length + filesArr.length >= maxLength) {
                btn_checkup.style.display = "none";
            } else {
                btn_checkup.style.display = "flex";

            }

            for (var i = 0; i < Math.min(filesArr.length, maxLength - examinationArray.length); i++) {
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
                            $('.upload__img-close2').length +
                            "' data-file='" +
                            f.name +
                            "' class='img-bg'><div class='upload__img-close2'><img src='/BranchSys/Pages/img/delete.png'></div></div></div>";
                        imgWrap.append(html);
                        examinationArray.push({
                            f: f,
                            url: e.target.result
                        });
                        imgCheckUpArray = examinationArray;
                    };
                    reader.readAsDataURL(f);
                })(filesArr[i]);
            }
        });
    });

    $('body').on('click', '.upload__img-close2', function (e) {
        e.stopPropagation();
        var file = $(this).parent().data('file');

        for (var i = 0; i < examinationArray.length; i++) {
            if (examinationArray[i].f.name === file) {
                examinationArray.splice(i, 1);
                break;
            }
        }

        $(this).parent().parent().remove();
        console.log(examinationArray);

        var maxLength = 12;


        if (examinationArray.length >= maxLength) {
            btn_checkup.style.display = "none";

        } else {
            btn_checkup.style.display = "flex";

        }
    });

    $('body').on('click', '.img-bg', function (e) {
        var imageUrl = $(this).css('background-image');
        $('#preview-image').attr('src', imageUrl);
        $('#image-preview').modal('show');
    });
}
//====================================================================================================
//====================================================================================================
const image = document.getElementById('hover-image-Settlement');
const dropdown = document.getElementById('dropdown-content-Settlement');

image.addEventListener('click', function () {
    if (dropdown.style.display === 'block') {
        dropdown.style.display = 'none';
    } else {
        dropdown.style.display = 'block';
        dropdown2.style.display = 'none';

    }
});
//====================================================================================================
//====================================================================================================
const image2 = document.getElementById('contract-value-Settlement2');
const dropdown2 = document.getElementById('dropdown-content-Settlement2');

image2.addEventListener('click', function () {
    if (dropdown2.style.display === 'block') {
        dropdown2.style.display = 'none';

    } else {
        dropdown2.style.display = 'block';
        dropdown.style.display = 'none';

    }
});
//====================================================================================================
//====================================================================================================
const image3 = document.getElementById('contract-value-Settlement3');
const dropdown3 = document.getElementById('dropdown-content-Settlement3');

image3.addEventListener('click', function () {
    if (dropdown3.style.display === 'block') {
        dropdown3.style.display = 'none';

    } else {
        dropdown3.style.display = 'block';
        dropdown4.style.display = 'none';

    }
});
//====================================================================================================
//====================================================================================================
const image4 = document.getElementById('contract-value-Settlement4');
const dropdown4 = document.getElementById('dropdown-content-Settlement4');

image4.addEventListener('click', function () {
    if (dropdown4.style.display === 'block') {
        dropdown4.style.display = 'none';

    } else {
        dropdown4.style.display = 'block';
        dropdown3.style.display = 'none';

    }
});
//====================================================================================================
//$('#Expenses-images').click(function () {
//    $('.upload__img-box').eq(0).hide();
//    var x = $('.upload__img-box')
//})
$('#Expenses-images').click(function () {
    $('#FirstUpload-img').hide()
})
$('#compensation-images').click(function () {
    $('#FirstUpload-img2').hide()
})
$('#examination-images').click(function () {
    $('#FirstUpload-img3').hide()
})



