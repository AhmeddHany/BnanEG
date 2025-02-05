
// رسم العقد على الـ Canvas
const DrawContractPage2 = async (canvas, data) => {
    console.log("بدء رسم الصفحة التانية");
    const ctx = canvas.getContext("2d");

    // ضبط أبعاد الـ canvas
    const backgroundImg = data.images.background;
    canvas.width = 2481;
    canvas.height = backgroundImg.height;
    ctx.drawImage(backgroundImg, 0, 0);

    // إعداد النصوص والصور
    const fixedConfig = {
        texts: [
            { key: "NUMBER", x: canvas.width - 2360, y: 358, align: "left", color: "#0D5485", fontSize: 40, fontWeight: "700" },
            { key: "CONTRACT_NUMBER", x: canvas.width - 2420, y: 405, align: "left", color: "#0D5485", fontSize: 45, fontWeight: "700" },
            // المستأجر //
            { key: "TENANT_NAME_AR", x: canvas.width - 420, y: 525, color: "#0D5485", fontSize: 50, fontWeight: "600", align: "right" },
            { key: "TENANT_NAME_EN", x: canvas.width - 420, y: 580, color: "#0D5485", fontSize: 45, fontWeight: "600", align: "right" },
            { key: "TENANT_ID", x: canvas.width - 518, y: 677, align: "right" },
            { key: "TENANT_TAX_NUM", x: canvas.width - 1210, y: 677, align: "right" },
            { key: "TENANT_EMAIL", x: canvas.width - 1730, y: 677, align: "right" },
            { key: "TENANT_ADDRESS_AR", x: canvas.width - 535, y: 762, fontSize: 40, align: "right" },
            { key: "TENANT_ADDRESS_EN", x: canvas.width - 535, y: 805, align: "right" },
            // السائق //
            { key: "DRIVER_NAME_AR", x: canvas.width - 420, y: 935, color: "#0D5485", fontSize: 50, fontWeight: "600", align: "right" },
            { key: "DRIVER_NAME_EN", x: canvas.width - 420, y: 990, color: "#0D5485", fontSize: 35, fontWeight: "600", align: "right" },
            { key: "DRIVER_ID", x: canvas.width - 518, y: 1082, align: "right" },
            { key: "DRIVER_ID_TYPE_AR", x: canvas.width - 1167, y: 1067, fontSize: 40, align: "right" },
            { key: "DRIVER_ID_TYPE_EN", x: canvas.width - 1167, y: 1104, align: "right" },
            { key: "DRIVER_VERSION_NUM", x: canvas.width - 1810, y: 1082, align: "right" },
            { key: "DRIVER_BIRTH_DATE", x: canvas.width - 580, y: 1182, align: "right" },
            { key: "DRIVER_GENDER_AR", x: canvas.width - 1130, y: 1167, fontSize: 40, align: "right" },
            { key: "DRIVER_GENDER_EN", x: canvas.width - 1130, y: 1205, align: "right" },
            { key: "DRIVER_NATIONALITY_AR", x: canvas.width - 1785, y: 1167, fontSize: 40, align: "right" },
            { key: "DRIVER_NATIONALITY_EN", x: canvas.width - 1785, y: 1205, align: "right" },
            { key: "DRIVER_LICENSE", x: canvas.width - 655, y: 1290, align: "right" },
            { key: "DRIVER_VEHICLE_TYPE_AR", x: canvas.width - 1270, y: 1272, fontSize: 40, align: "right" },
            { key: "DRIVER_VEHICLE_TYPE_EN", x: canvas.width - 1270, y: 1314, align: "right" },
            { key: "DRIVER_LICENSE_EXPIRY", x: canvas.width - 1835, y: 1290, align: "right" },
            { key: "DRIVER_POSITION_AR", x: canvas.width - 555, y: 1377, fontSize: 40, align: "right" },
            { key: "DRIVER_POSITION_EN", x: canvas.width - 555, y: 1415, align: "right" },
            { key: "DRIVER_WORKPLACE_AR", x: canvas.width - 1175, y: 1377, fontSize: 40, align: "right" },
            { key: "DRIVER_WORKPLACE_EN", x: canvas.width - 1175, y: 1415, align: "right" },
            { key: "DRIVER_EMAIL", x: canvas.width - 1730, y: 1395, align: "right" },
            { key: "DRIVER_ADDRESS_AR", x: canvas.width - 535, y: 1480, fontSize: 40, align: "right" },
            { key: "DRIVER_ADDRESS_EN", x: canvas.width - 535, y: 1520, align: "right" },
            // السائق الاضافي //
            { key: "ADDITIONAL_DRIVER_NAME_AR", x: canvas.width - 420, y: 1640, color: "#0D5485", fontSize: 50, fontWeight: "600", align: "right" },
            { key: "ADDITIONAL_DRIVER_NAME_EN", x: canvas.width - 420, y: 1700, color: "#0D5485", fontSize: 35, fontWeight: "600", align: "right" },
            { key: "ADDITIONAL_DRIVER_ID", x: canvas.width - 518, y: 1797, align: "right" },
            { key: "ADDITIONAL_DRIVER_ID_TYPE_AR", x: canvas.width - 1167, y: 1785, fontSize: 40, align: "right" },
            { key: "ADDITIONAL_DRIVER_ID_TYPE_EN", x: canvas.width - 1167, y: 1825, align: "right" },
            { key: "ADDITIONAL_DRIVER_VERSION", x: canvas.width - 1810, y: 1800, align: "right" },
            { key: "ADDITIONAL_DRIVER_BIRTH_DATE", x: canvas.width - 575, y: 1900, align: "right" },
            { key: "ADDITIONAL_DRIVER_GENDER_AR", x: canvas.width - 1130, y: 1885, fontSize: 40, align: "right" },
            { key: "ADDITIONAL_DRIVER_GENDER_EN", x: canvas.width - 1130, y: 1920, align: "right" },
            { key: "ADDITIONAL_DRIVER_NATIONALITY_AR", x: canvas.width - 1782, y: 1885, fontSize: 40, align: "right" },
            { key: "ADDITIONAL_DRIVER_NATIONALITY_EN", x: canvas.width - 1782, y: 1920, align: "right" },
            { key: "ADDITIONAL_DRIVER_LICENSE", x: canvas.width - 655, y: 2005, align: "right" },
            { key: "ADDITIONAL_DRIVER_VEHICLE_TYPE_AR", x: canvas.width - 1270, y: 1990, fontSize: 40, align: "right" },
            { key: "ADDITIONAL_DRIVER_VEHICLE_TYPE_EN", x: canvas.width - 1270, y: 2032, align: "right" },
            { key: "ADDITIONAL_DRIVER_LICENSE_EXPIRY", x: canvas.width - 1835, y: 2005, align: "right" },
            { key: "ADDITIONAL_DRIVER_POSITION_AR", x: canvas.width - 560, y: 2090, fontSize: 40, align: "right" },
            { key: "ADDITIONAL_DRIVER_POSITION_EN", x: canvas.width - 560, y: 2130, align: "right" },
            { key: "ADDITIONAL_DRIVER_WORKPLACE_AR", x: canvas.width - 1175, y: 2090, fontSize: 40, align: "right" },
            { key: "ADDITIONAL_DRIVER_WORKPLACE_EN", x: canvas.width - 1175, y: 2130, align: "right" },
            { key: "ADDITIONAL_DRIVER_EMAIL", x: canvas.width - 1730, y: 2110, align: "right" },
            { key: "ADDITIONAL_DRIVER_ADDRESS_AR", x: canvas.width - 535, y: 2199, fontSize: 40, align: "right" },
            { key: "ADDITIONAL_DRIVER_ADDRESS_EN", x: canvas.width - 535, y: 2235, align: "right" },
            // سائق خاص  //
            { key: "PRIVATE_DRIVER_NAME_AR", x: canvas.width - 420, y: 2360, color: "#0D5485", fontSize: 50, fontWeight: "600", align: "right" },
            { key: "PRIVATE_DRIVER_NAME_EN", x: canvas.width - 420, y: 2420, color: "#0D5485", fontSize: 35, fontWeight: "600", align: "right" },
            { key: "PRIVATE_DRIVER_ID", x: canvas.width - 518, y: 2510, align: "right" },
            { key: "PRIVATE_DRIVER_ID_TYPE_AR", x: canvas.width - 1167, y: 2490, fontSize: 40, align: "right" },
            { key: "PRIVATE_DRIVER_ID_TYPE_EN", x: canvas.width - 1167, y: 2530, align: "right" },
            { key: "PRIVATE_DRIVER_NATIONALITY", x: canvas.width - 1785, y: 2490, fontSize: 40, align: "right" },
            { key: "PRIVATE_DRIVER_NATIONALITY_EN", x: canvas.width - 1785, y: 2530, align: "right" },
            { key: "PRIVATE_DRIVER_LICENSE", x: canvas.width - 655, y: 2620, align: "right" },
            { key: "PRIVATE_DRIVER_VEHICLE_TYPE_AR", x: canvas.width - 1270, y: 2605, fontSize: 40, align: "right" },
            { key: "PRIVATE_DRIVER_VEHICLE_TYPE_EN", x: canvas.width - 1270, y: 2645, align: "right" },
            { key: "PRIVATE_DRIVER_LICENSE_EXPIRY", x: canvas.width - 1835, y: 2620, align: "right" },
            // التوقيع //
            { key: "SIGNATURE_AR", x: canvas.width - 280, y: 2830, fontSize: 40, align: "right" },
            { key: "SIGNATURE_EN", x: canvas.width - 280, y: 2870, align: "right" },
        ],
        images: [
            { content: data.images.EMPLOYEE_SIGN, x: canvas.width - 705, y: 2810, width: 160, height: 50 },
            { content: data.images.TENANT_SIGN, x: canvas.width - 500, y: 2910, width: 160, height: 50 },
            { content: data.images.STAMP, x: canvas.width - 776, y: 2800, width: 190, height: 194 },
            { content: data.images.Authentication_STAMP, x: canvas.width - 560, y: 2905, width: 190, height: 194 },
            { content: data.images.QR, x: canvas.width - 2432, y: 2796, width: 190, height: 194 },
        ],
        textStyle: {
            fontWeight: "normal",
            fontSize: 35,
            fontFamily: "Sakkal Majalla Regular",
            textColor: "#000000",
            textAlign: "right",
        },
    };
    // تحميل الخطوط قبل استخدامها
    await document.fonts.ready;
    console.log("✅ الخطوط جاهزة للاستخدام");
    await document.fonts.load(`${fixedConfig.textStyle.fontWeight} ${fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`);
    // رسم النصوص

    const truncateText = (text, maxLength) => {
        if (text.length <= maxLength) return text;

        const isArabic = /[\u0600-\u06FF]/.test(text);
        return isArabic
            ? "..." + text.substring(0, maxLength)
            : text.substring(0, maxLength) + "...";
    };


    const keysToTruncate = ["DRIVER_WORKPLACE_AR", "DRIVER_WORKPLACE_EN", "ADDITIONAL_DRIVER_WORKPLACE_EN", "ADDITIONAL_DRIVER_WORKPLACE_AR"];

    fixedConfig.texts.forEach(({ key, x, y, align, color, fontSize, fontWeight }) => {

        ctx.font = `${fontWeight || fixedConfig.textStyle.fontWeight} ${fontSize || fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily} `;
        ctx.fillStyle = color || fixedConfig.textStyle.textColor;
        var content = data[key] || "";

        if (keysToTruncate.includes(key)) {
            content = truncateText(content, 36);
        }
        const textWidth = ctx.measureText(content).width;
        let adjustedX = x;

        if (align === "right") adjustedX = x - textWidth;
        else if (align === "center") adjustedX = x - textWidth / 2;

        ctx.fillText(content, adjustedX, y);
    });


    // رسم الصور
    fixedConfig.images.forEach(({ content, x, y, width, height }) => {
        // التحقق من ان الصورة موجوده
        if (content) {
            ctx.drawImage(content, x, y, width, height);
        } else {
            console.warn("Image not found or not loaded, skipping.");
        }
    });

    const imageBlob = await new Promise((resolve) => {
        canvas.toBlob(resolve, 'image/png');
    });
    return imageBlob;
};
