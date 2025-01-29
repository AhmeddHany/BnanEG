
//  لرسم الإيصال على الصورة
const drawInvoice = async (canvas, data) => {
    console.log("بدء رسم الفاتورة");
    const ctx = canvas.getContext("2d");

    // إعداد حجم الصورة بناءً على صورة الخلفية
    const backgroundImg = data.images.background;
    canvas.width = backgroundImg.width;
    canvas.height = backgroundImg.height;
    ctx.drawImage(backgroundImg, 0, 0);

    // إعدادات النصوص والصور
    const fixedConfig = {
        texts: [
            { key: "CONTRACT_NUMBER", x: canvas.width - 510, y: 695, align: "left" },
            { key: "DATE_AR", x: canvas.width - 225, y: 777, align: "right" },
            { key: "DATE_EN", x: canvas.width - 915, y: 777, align: "right" },
            { key: "DATE_EN", x: canvas.width - 1530, y: 777, align: "left" },
            { key: "DATE_EN_LONG", x: canvas.width - 2275, y: 777, align: "left" },
            { key: "HIJRI_DATE_EN", x: canvas.width - 1530, y: 777, align: "left" },
            { key: "CONTRACT_CREATE_AR", x: canvas.width - 225, y: 874, align: "right" },
            { key: "CONTRACT_CREATE_EN", x: canvas.width - 2170, y: 874, align: "left" },
            { key: "TENANT_NAME_AR", x: canvas.width - 270, y: 1165, align: "right" },
            { key: "TENANT_NAME_EN", x: canvas.width - 2225, y: 1165, align: "left" },
            { key: "CAR_DESCRIPTION_AR", x: canvas.width - 250, y: 1250, align: "right" },
            { key: "CAR_DESCRIPTION_EN", x: canvas.width - 2295, y: 1250, align: "left" },
            { key: "EMPLOYEE_AR", x: canvas.width - 250, y: 2950, align: "right" },
            { key: "EMPLOYEE_EN", x: canvas.width - 250, y: 3000, align: "right" },
        ],
        images: [
            { content: data.images.signature, x: canvas.width - 988, y: 2908, width: 240, height: 105 },
            { content: data.images.qr, x: canvas.width - 2395, y: 2880, width: 200, height: 200 },
            { content: data.images.stamp, x: canvas.width - 1128, y: 2885, width: 204, height: 160 },
        ],
        textStyle: {
            fontWeight: "normal",
            fontSize: 46,
            fontFamily: "Sakkal Majalla Regular",
            textColor: "#000000",
        },
        tableStyle: {
            fontWeight: "normal",
            fontSize: 40,
            fontFamily: "Sakkal Majalla Regular",
            textColor: "#000000",
            textAlign: "center",
        }
    };
    //await document.fonts.load(`${fixedConfig.textStyle.fontWeight} ${fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`);
    //await document.fonts.load(`${fixedConfig.tableStyle.fontWeight} ${fixedConfig.tableStyle.fontSize}px ${fixedConfig.tableStyle.fontFamily}`);

    ctx.font = `${fixedConfig.textStyle.fontWeight} ${fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`;
    ctx.fillStyle = fixedConfig.textStyle.textColor;

    // رسم النصوص
    fixedConfig.texts.forEach(({ key, x, y, align }) => {
        const content = data[key] || "";
        const textWidth = ctx.measureText(content).width;
        let adjustedX = x;
        if (align === "right") adjustedX = x - textWidth;
        else if (align === "center") adjustedX = x - textWidth / 2;
        ctx.fillText(content, adjustedX, y);
    });

    // رسم الصور
    fixedConfig.images.forEach(({ content, x, y, width, height }) => {
        if (content) { // تحقق مما إذا كانت الصورة محملة
            ctx.drawImage(content, x, y, width, height);
        } else {
            console.warn("الصورة غير موجودة أو لم يتم تحميلها، يتم تخطيها.");
        }
    });

    // رسم الجدول (عناصر الخدمة)
    const startY = 1528;
    const increment = 124;
    const startYForNumbers = 1548;
    if (data.Serviceitems && data.Serviceitems.length > 0) {
        data.Serviceitems.forEach((item, i) => {
            ctx.font = `${fixedConfig.tableStyle.fontWeight} ${fixedConfig.tableStyle.fontSize}px ${fixedConfig.tableStyle.fontFamily}`;
            ctx.fillStyle = fixedConfig.tableStyle.textColor;

            // قياس عرض النص للغة العربية
            const arabicTextWidth = ctx.measureText(item.arabic).width;
            const englishTextWidth = ctx.measureText(item.english).width;

            let adjustedXArabic = canvas.width - 280; // الموضع الأساسي للنص العربي
            let adjustedXEnglish = canvas.width - 280; // الموضع الأساسي للنص الإنجليزي

            if (item.textAlign === "right") {
                adjustedXArabic = canvas.width - 280 - arabicTextWidth;
                adjustedXEnglish = canvas.width - 280 - englishTextWidth;
            } else if (item.textAlign === "center") {
                adjustedXArabic = canvas.width - 280 - arabicTextWidth / 2;
                adjustedXEnglish = canvas.width - 280 - englishTextWidth / 2;
            }

            // رسم النص العربي
            ctx.fillText(item.arabic, adjustedXArabic, startY + i * increment);
            // رسم النص الإنجليزي
            ctx.font = "35px Sakkal Majalla Regular"; // تغيير حجم الخط للنص الإنجليزي
            ctx.fillText(item.english, adjustedXEnglish, startY + i * increment + 40);
        });
    }

    //  لرسم العناصر في تنسيق جدول
    const drawItems = (items, xPosition, startY, align) => {
        if (!items || items.length === 0) return; // تحقق من العناصر الفارغة
        items.forEach((item, i) => {
            ctx.font = `${fixedConfig.tableStyle.fontWeight} ${fixedConfig.tableStyle.fontSize}px ${fixedConfig.tableStyle.fontFamily}`;
            ctx.fillStyle = fixedConfig.tableStyle.textColor;
            const content = item.content || "";
            const textWidth = ctx.measureText(content).width;

            let adjustedX = xPosition;
            if (align === "right") adjustedX = xPosition - textWidth;
            else if (align === "center") adjustedX = xPosition - textWidth / 2;

            ctx.fillText(content, adjustedX, startY + i * increment);
            if (item.sum) {
                ctx.fillText(item.sum, adjustedX, 2760);
            }
        });
    };

    // رسم جميع فئات العناصر
    drawItems(data.Valueitems, canvas.width - 580, startYForNumbers, "center");
    drawItems(data.Numberitems, canvas.width - 830, startYForNumbers, "center");
    drawItems(data.Amountitems, canvas.width - 1085, startYForNumbers, "center");
    drawItems(data.Discountitems, canvas.width - 1360, startYForNumbers, "center");
    drawItems(data.AfterDiscountitems, canvas.width - 1640, startYForNumbers, "center");
    drawItems(data.VATitems, canvas.width - 1930, startYForNumbers, "center");
    drawItems(data.Totalitems, canvas.width - 2220, startYForNumbers, "center");
};

