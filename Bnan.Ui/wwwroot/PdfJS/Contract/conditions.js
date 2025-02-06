

//  لرسم الإيصال على الصورة
const DrawContractPageConditions = async (canvas, data, No) => {
    try {
        console.log(`$بدء رسم الصفحة {No}`);
        const ctx = canvas.getContext("2d");

        // إعداد حجم الصورة بناءً على صورة الخلفية
        const backgroundImg = data.images.background;
        canvas.width = 2481;
        canvas.height = backgroundImg.height;
        ctx.drawImage(backgroundImg, 0, 0);

        // إعدادات النصوص والصور
        const fixedConfig = {
            texts: [
                { key: "NUMBER", x: canvas.width - 2360, y: 358, align: "left", color: "#0D5485", fontSize: 40, fontWeight: "700" },
                { key: "CONTRACT_NUMBER", x: canvas.width - 2420, y: 405, align: "left", color: "#0D5485", fontSize: 45, fontWeight: "700" },

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
            }
        };
        await document.fonts.ready;
        console.log("✅ الخطوط جاهزة للاستخدام");
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
            if (content) { // تحقق مما إذا كانت الصورة محملة
                ctx.drawImage(content, x, y, width, height);
            } else {
                console.warn("الصورة غير موجودة أو لم يتم تحميلها، يتم تخطيها.");
            }
        });
        // تحويل الكانفس إلى صورة
        const imageBlob = await new Promise((resolve) => {
            canvas.toBlob(resolve, 'image/png');
        });

        return imageBlob;
    } catch (error) {
        console.error(`$🚨 خطأ أثناء رسم الصفحة الرابعة: {No}`,"PageNo:", No, error,);
    }

};