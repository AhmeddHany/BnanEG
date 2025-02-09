
// رسم الإيصال على الـ Canvas
const drawCard = async (canvas, data) => {
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
            { key: "CARD_TITLE_AR", x: canvas.width - 520, y: 90, align: "center", fontSize: 50, fontWeight: "500" },
            { key: "CARD_TITLE_EN", x: canvas.width - 520, y: 135, align: "center", fontWeight: "500" },

            { key: "RENTAR_NAME_AR", x: canvas.width - 42, y: 195, align: "right" },
            { key: "RENTAR_NAME_EN", x: canvas.width - 1010, y: 276, align: "left" },

            { key: "CAR_NAME_AR", x: canvas.width - 42, y: 232, align: "right" },
            { key: "CAR_NAME_EN", x: canvas.width - 1010, y: 315, align: "left" },

            { key: "CONTRACT_DATE_AR", x: canvas.width - 150, y: 355, align: "right" },
            { key: "CONTRACT_DATE_EN", x: canvas.width - 895, y: 355, align: "left" },

            { key: "CONTRACT_END_DATE_AR", x: canvas.width - 150, y: 400, align: "right" },
            { key: "CONTRACT_END_DATE_EN", x: canvas.width - 895, y: 400, align: "left" },

            { key: "ADDRESS", x: canvas.width - 42, y: 550, align: "right" },

            { key: "CONTRACT_NUMBER", x: canvas.width - 1010, y: 550, align: "left" },



        ],
        images: [
            { content: data.images.QR, x: canvas.width - 1017, y: 405, width: 120, height: 120 },
        ],
        textStyle: {
            fontWeight: "500",
            fontSize: 30,
            fontFamily: "Sakkal Majalla Regular",
            textColor: "#000000",
        },
    };

    await document.fonts.load(`${fixedConfig.textStyle.fontWeight} ${fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`);

    ctx.font = `${fixedConfig.textStyle.fontWeight} ${fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`;
    ctx.fillStyle = fixedConfig.textStyle.textColor;

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

};
