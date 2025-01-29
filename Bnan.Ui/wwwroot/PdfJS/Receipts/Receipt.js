
// رسم الإيصال على الـ Canvas
const drawReceipt = async (canvas, data) => {
    console.log("Start drawReceipt");
    const ctx = canvas.getContext("2d");

    // ضبط أبعاد الـ canvas
    const backgroundImg = data.images.background;
    canvas.width = backgroundImg.width;
    canvas.height = backgroundImg.height;
    ctx.drawImage(backgroundImg, 0, 0);

    // إعداد النصوص والصور
    const fixedConfig = {
        texts: [
            { key: "DATE_AR", x: canvas.width - 210, y: 605, align: "right" },
            { key: "DATE_EN", x: canvas.width - 232, y: 675, align: "right" },
            { key: "HIJRI_DATE_AR", x: canvas.width - 1520, y: 605, align: "left" },
            { key: "HIJRI_DATE_EN", x: canvas.width - 1500, y: 675, align: "left" },
            { key: "CONTRACT_NUMBER", x: canvas.width - 2400, y: 605, align: "left" },
            { key: "AMOUNT", x: canvas.width - 2350, y: 690, align: "left" },
            { key: "HALALAS", x: canvas.width - 2040, y: 690, align: "left" },
            { key: "TENANT_NAME_AR", x: canvas.width - 200, y: 795, align: "right" },
            { key: "TENANT_NAME_EN", x: canvas.width - 2255, y: 795, align: "left" },
            { key: "AMOUNT_AR", x: canvas.width - 295, y: 900, align: "right" },
            { key: "AMOUNT_EN", x: canvas.width - 2220, y: 970, align: "left" },
            { key: "PAYMENT_METHOD_AR", x: canvas.width - 315, y: 1075, align: "right" },
            { key: "PAYMENT_METHOD_EN", x: canvas.width - 2040, y: 1075, align: "left" },
            { key: "PAYMENT_DESC_AR", x: canvas.width - 305, y: 1180, align: "right" },
            { key: "PAYMENT_DESC_EN", x: canvas.width - 2140, y: 1255, align: "left" },
            { key: "NOTES_AR", x: canvas.width - 250, y: 1335, align: "right" },
            { key: "APPROVER_NAME_AR", x: canvas.width - 220, y: 1560, align: "right" },
            { key: "APPROVER_NAME_EN", x: canvas.width - 220, y: 1605, align: "right" },
        ],
        images: [
            { content: data.images.signature, x: canvas.width - 957, y: 1519, width: 240, height: 105 },
            { content: data.images.qr, x: canvas.width - 2411, y: 1447, width: 200, height: 200 },
            { content: data.images.stamp, x: canvas.width - 1076, y: 1475, width: 204, height: 160 },
        ],
        textStyle: {
            fontWeight: "normal",
            fontSize: 46,
            fontFamily: "Sakkal Majalla Regular",
            textColor: "#000000",
        },
    };

    //await document.fonts.load(`${fixedConfig.textStyle.fontWeight} ${fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`);

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
        ctx.drawImage(content, x, y, width, height);
    });
};


























