
let current_fs, next_fs, previous_fs;
const ExpensesCheckbox = document.getElementById('expenses');
const CompensationCheckbox = document.getElementById('compensation-check');
var imgCompensations = [];
var imgExpenses = [];


jQuery(document).ready(function () {

    ExpensesImgUpload();
    compensationImgUpload();
    examinationImgUpload();
});
//=======================================’New Functions=============================================================

function setFocusToFirstInput(fieldset) {
    var firstFocusable = fieldset.find("input, select , textarea").first();
    if (firstFocusable.length) {
        firstFocusable.focus();
    }
}

function moveToNextInput(event) {

    if (event.key === "Enter") {
        event.preventDefault();
        let formElements = Array.from(
            event.target.form.querySelectorAll("input, select, button , textarea")
        );
        let index = formElements.indexOf(event.target);

        if (index > -1 && index < formElements.length - 1) {
            let nextElement = formElements[index + 1];
            if (nextElement.tagName === "BUTTON" || nextElement.type === "button") {
                nextElement.click();
            } else {
                nextElement.focus();
            }
        }
    }
}

$(document).ready(function () {
    $("input, select, button , textarea").on("keydown", moveToNextInput);

    var firstFieldset = $("fieldset").first();
    setFocusToFirstInput(firstFieldset);
});
//====================================================================================================
//========================================Upload Expenses/compensation Imgs Lists============================================================
//====================================================================================================
var compensationArray = [];
var expensesArray = [];
var compensationTdIndex = 0;
var expensesTdIndex = 0;

function compensationImgUpload() {
    var maxLength = 4;
    var uploadBtnBox = document.getElementById('compensation-upload-box');
    console.log(compensationArray);
    $('#compensation-images').on('change', function (e) {
        handleImageUpload(e, '#compensation-Attatchments-Table', compensationArray, maxLength, uploadBtnBox, 'compensation');
    });
}

function ExpensesImgUpload() {
    var maxLength = 4;
    var uploadBtnBox = document.getElementById('Expenses-upload-box');
    console.log(expensesArray);
    $('#Expenses-images').on('change', function (e) {
        handleImageUpload(e, '#Expenses-Attatchments-Table', expensesArray, maxLength, uploadBtnBox, 'expenses');
    });
}

function handleImageUpload(event, tableSelector, array, maxLength, uploadBtnBox, type) {
    var uploadBox = $(event.target).closest('.upload__box');
    var files = event.target.files;
    var filesArr = Array.prototype.slice.call(files);
    console.log(array);
    for (var i = 0; i < Math.min(filesArr.length, maxLength - array.length); i++) {
        (function (f) {
            if (f.type === 'image/heic' || f.type === 'image/heif' || f.name.endsWith('.heic') || f.name.endsWith('.heif')) {
                heic2any({ blob: f, toType: "image/jpeg" })
                    .then(function (convertedBlob) {
                        processFile(convertedBlob, f.name, tableSelector, array, maxLength, uploadBtnBox, type);
                    })
                    .catch(function (err) {
                        console.error("Error converting HEIC/HEIF image:", err);
                    });
            } else {
                processFile(f, f.name, tableSelector, array, maxLength, uploadBtnBox, type);
            }
        })(filesArr[i]);
    }

    $('body').on('click', '.upload__img-close1', function (e) {
        e.stopPropagation();
        var file = $(this).parent().data('file');

        for (var i = 0; i < array.length; i++) {
            if (array[i].f.name === file) {
                array.splice(i, 1);
                break;
            }
        }

        $(this).closest('.upload__img-box').remove();

        if (array.length < maxLength) {
            uploadBtnBox.style.display = 'flex';
            updateCurrentTdIndex(tableSelector, type, uploadBtnBox);
        }

        if (array.length === 0) {
            if (type === 'compensation') compensationTdIndex = 0;
            else expensesTdIndex = 0;

            uploadBtnBox.style.display = 'flex';
            $(`${tableSelector} td`).eq(type === 'compensation' ? compensationTdIndex : expensesTdIndex).append(uploadBtnBox);
        }
    });
}

function processFile(file, fileName, tableSelector, array, maxLength, uploadBtnBox, type) {
    var reader = new FileReader();
    reader.onload = function (e) {
        var html = `
      <div class='upload__img-box Attatchments-img-box'>
        <div style='background-image: url(${e.target.result})' data-file='${fileName}' class='img-bg'>
          <div class='upload__img-close1'><i class='fa-regular fa-trash-can'></i></div>
        </div>
      </div>
    `;

        var currentTdIndex = type === 'compensation' ? compensationTdIndex : expensesTdIndex;
        var targetTd = $(`${tableSelector} td`).eq(currentTdIndex);
        targetTd.append(html);

        array.push({ f: file, url: e.target.result });

        updateCurrentTdIndex(tableSelector, type, uploadBtnBox);

        if (array.length >= maxLength) {
            uploadBtnBox.style.display = 'none';
        }
    };
    reader.readAsDataURL(file);
}

function updateCurrentTdIndex(tableSelector, type, uploadBtnBox) {
    var currentTdIndex = type === 'compensation' ? compensationTdIndex : expensesTdIndex;

    $(`${tableSelector} td`).each(function (index) {
        if ($(this).find('.upload__img-box').length === 0) {
            if (type === 'compensation') compensationTdIndex = index;
            else expensesTdIndex = index;

            uploadBtnBox.style.display = 'flex';
            $(this).append(uploadBtnBox);
            return false; // Break the loop
        }
    });
}


//========================================calculate Expenses/compensation  ============================================================

document.addEventListener("DOMContentLoaded", () => {
    const ExpensesTable = document.getElementById("Expenses-Data-Table");
    const TotalExpenses = document.getElementById("TotalExpenses");

    const CompensationTable = document.getElementById("compensation-Data-Table");
    const TotalCompensation = document.getElementById("TotalCompensation");

    const calculateTotal = (table, totalRow) => {
        let sum = 0;

        table.querySelectorAll("tbody tr:not(:last-child)").forEach(row => {
            const valueCell = row.cells[1];
            const inputCell = valueCell.querySelector("input");

            let rawValue = inputCell ? inputCell.value : valueCell.textContent;
            let value = parseFloat(rawValue.replace(/,/g, '') || 0);

            if (!isNaN(value) && value <= 1000000) {
                sum += value;
            } else if (value >= 1000000) {
                sum += 1000000;
            }
        });

        totalRow.textContent = sum.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    };

    const addInputListeners = (table, totalRow) => {
        const inputs = table.querySelectorAll("input.Table-inputs");

        inputs.forEach(input => {
            input.addEventListener("input", () => {
                input.value = input.value.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
                calculateTotal(table, totalRow);
            });
        });
    };

    addInputListeners(ExpensesTable, TotalExpenses);
    calculateTotal(ExpensesTable, TotalExpenses);
    addInputListeners(CompensationTable, TotalCompensation);
    calculateTotal(CompensationTable, TotalCompensation);
});
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
            var maxLength = 10;
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
        dropdown5.style.display = 'none';


    }
});
//====================================================================================================
//====================================================================================================
const image2 = document.getElementById('contract-value-Settlement2');
const dropdown2 = document.getElementById('dropdown-content-Settlement2');

image2.addEventListener('click', function () {
    if (dropdown2.style.display === 'block') {
        dropdown2.style.display = 'none';
        dropdown5.style.display = 'none';

    } else {
        dropdown2.style.display = 'block';
        dropdown.style.display = 'none';
        dropdown5.style.display = 'none';


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
//====================================================================================================
const image5 = document.getElementById('Settlement-type-Data');
const dropdown5 = document.getElementById('dropdown-content-Settlement5');

image5.addEventListener('click', function () {
    if (dropdown5.style.display === 'block') {
        dropdown5.style.display = 'none';
        dropdown2.style.display = 'none';
        dropdown.style.display = 'none';

    } else {
        dropdown5.style.display = 'block';
        dropdown2.style.display = 'none';
        dropdown.style.display = 'none';

    }
});
//====================================================================================================
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



