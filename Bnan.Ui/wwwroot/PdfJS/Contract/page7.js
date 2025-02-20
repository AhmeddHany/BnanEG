
const DrawContractPage7 = async (canvas, data) => {
    console.log("بدء رسم الصفحة السابعة");
    const ctx = canvas.getContext("2d");

    // ضبط أبعاد الـ canvas
    const backgroundImg = data.images.background;
    canvas.width = 2481;
    canvas.height = backgroundImg.height;
    ctx.drawImage(backgroundImg, 0, 0);

    // إعداد النصوص والصور
    const fixedConfig = {
        texts: [
            { key: "NUMBER", x: canvas.width - 2360, y: 358, align: "left", color: "#0D5485", fontSize: 40, fontWeight: "700", },
            { key: "CONTRACT_NUMBER", x: canvas.width - 2420, y: 405, align: "left", color: "#0D5485", fontSize: 45, fontWeight: "700", },
            { key: "CONTRACT_DATE_AR", x: canvas.width - 610, y: 510, align: "right", fontSize: 40 },
            { key: "CONTRACT_DATE_EN", x: canvas.width - 610, y: 550, align: "right" },
            { key: "CONTRACT_START_AR", x: canvas.width - 610, y: 615, align: "right", fontSize: 40 },
            { key: "CONTRACT_START_EN", x: canvas.width - 610, y: 655, align: "right" },
            { key: "CONTRACT_END_AR", x: canvas.width - 610, y: 720, align: "right", fontSize: 40 },
            { key: "CONTRACT_END_EN", x: canvas.width - 610, y: 760, align: "right" },
            { key: "DAYSNUMBER", x: canvas.width - 665, y: 840, align: "right" },
            { key: "CONTRACT_STATUE_AR", x: canvas.width - 1835, y: 822, align: "right", fontSize: 40 },
            { key: "CONTRACT_STATUE_EN", x: canvas.width - 1835, y: 860, align: "right" },
            { key: "DATE_1", x: canvas.width - 1885, y: 527, align: "right" },
            { key: "TIME_1", x: canvas.width - 1758, y: 527, align: "right" },
            { key: "DATE_2", x: canvas.width - 1885, y: 626, align: "right" },
            { key: "TIME_2", x: canvas.width - 1758, y: 626, align: "right" },
            { key: "DATE_3", x: canvas.width - 1885, y: 728, align: "right" },
            { key: "TIME_3", x: canvas.width - 1758, y: 728, align: "right" },
            //  بنود التأجبر ////
            { key: "Delivery_Branch_AR", x: canvas.width - 615, y: 965, fontSize: 40, align: "right" },
            { key: "Delivery_Branch_EN", x: canvas.width - 615, y: 1005, align: "right" },
            { key: "Receiving_Branch_AR", x: canvas.width - 1230, y: 965, fontSize: 40, align: "right" },
            { key: "Delay_hours", x: canvas.width - 620, y: 1085, align: "right" },
            { key: "Delay_days", x: canvas.width - 1180, y: 1085, align: "right" },
            { key: "Extra_KM", x: canvas.width - 665, y: 1190, align: "right" },
            { key: "consumed_Fuel", x: canvas.width - 1235, y: 1190, align: "right" },
            { key: "Total_actual_contract", x: canvas.width - 675, y: 1295, align: "right" },

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
    fixedConfig.texts.forEach(({ key, x, y, align, color, fontSize, fontWeight }) => {
        ctx.font = `${fontWeight || fixedConfig.textStyle.fontWeight} ${fontSize || fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`;
        ctx.fillStyle = color || fixedConfig.textStyle.textColor;
        const content = data[key] || "";
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
