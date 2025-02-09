

// رسم الإيصال على الـ Canvas
const drawWelcomeCard = async (canvas, data) => {
    console.log("Start drawWelcomeCard");
    const ctx = canvas.getContext("2d");
    ctx.direction = "ltr"; 
    // ضبط أبعاد الـ canvas
    const backgroundImg = data.images.background;
    canvas.width = backgroundImg.width;
    canvas.height = backgroundImg.height;
    ctx.drawImage(backgroundImg, 0, 0);

    // إعداد النصوص والصور
    const fixedConfig = {
        texts: [
            { key: "Employee_AR_Name", x: canvas.width - 232, y: 530, align: "right" },
            { key: "Employee_EN_Name", x: canvas.width - 1315, y: 800, align: "left" },
            { key: "Employee_No", x: canvas.width - 670, y: 1275, align: "right" },
            { key: "Password", x: canvas.width - 670, y: 1130, align: "right" },


        ],
        images: [
            { content: data.images.qr, x: canvas.width - 1396, y: 1638, width: 280, height: 280 },
        ],
        textStyle: {
            fontWeight: "normal",
            fontSize: 50,
            fontFamily: "Sakkal Majalla Regular",
            textColor: "#000000",
        },
    };

    await document.fonts.load(`${fixedConfig.textStyle.fontWeight} ${fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`);

    ctx.font = `${fixedConfig.textStyle.fontWeight} ${fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`;
    ctx.fillStyle = fixedConfig.textStyle.textColor;

    // رسم النصوص
    fixedConfig.texts.forEach(({ key, x, y, align }) => {
        const content = data[key] || "";
        const textWidth = ctx.measureText(content).width;
        let adjustedX = x;

        if (align === "right") adjustedX = x - textWidth;
        else if (align === "center") adjustedX = x - textWidth / 2;
        ctx.direction = "ltr"; 
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
