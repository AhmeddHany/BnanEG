const DrawContractPage4 = async (canvas, data) => {
    try {
        console.log("🚀 بدء رسم الصفحة الرابعة");

        const ctx = canvas.getContext("2d");

        // إعداد حجم الصورة بناءً على صورة الخلفية
        const backgroundImg = data.images.background;
        if (!backgroundImg) {
            throw new Error("🚨 صورة الخلفية غير موجودة!");
        }

        canvas.width = 2481;
        canvas.height = backgroundImg.height;
        ctx.drawImage(backgroundImg, 0, 0);

        console.log("✅ تم تحميل الخلفية بنجاح");

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
            },
            tableStyle: {
                fontWeight: "normal",
                fontSize: 35,
                fontFamily: "Sakkal Majalla Regular",
                textColor: "#000000",
                textAlign: "center",
            }
        };

        // تحميل الخطوط قبل استخدامها
        await document.fonts.ready;
        console.log("✅ الخطوط جاهزة للاستخدام");

        ctx.font = `${fixedConfig.textStyle.fontWeight} ${fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`;
        ctx.fillStyle = fixedConfig.textStyle.textColor;

        console.log("📝 رسم النصوص");
        fixedConfig.texts.forEach(({ key, x, y, align, color, fontSize, fontWeight }) => {
            const content = data[key] || "";
            if (typeof content !== "string") {
                console.warn(`⚠️ المفتاح "${key}" يحتوي على قيمة غير نصية:`, content);
                return;
            }

            ctx.font = `${fontWeight || fixedConfig.textStyle.fontWeight} ${fontSize || fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`;
            ctx.fillStyle = color || fixedConfig.textStyle.textColor;

            const textWidth = ctx.measureText(content).width;
            let adjustedX = x;

            if (align === "right") adjustedX = x - textWidth;
            else if (align === "center") adjustedX = x - textWidth / 2;

            ctx.fillText(content, adjustedX, y);
        });

        console.log("🖼️ رسم الصور");
        fixedConfig.images.forEach(({ content, x, y, width, height }) => {
            if (content) {
                ctx.drawImage(content, x, y, width, height);
            } else {
                console.warn("⚠️ الصورة غير موجودة أو لم يتم تحميلها، يتم تخطيها.");
            }
        });

        // رسم الجدول
        console.log("📊 رسم الجدول");

        let startY = 625;
        let increment = 104; 

        const drawTextItems = (ctx, items, baseX, startY, increment) => {
            if (items && items.length > 0) {
                items.forEach((item, i) => {
                    ctx.font = `${fixedConfig.tableStyle.fontWeight} ${fixedConfig.tableStyle.fontSize}px ${fixedConfig.tableStyle.fontFamily}`;
                    ctx.fillStyle = fixedConfig.tableStyle.textColor;

                    const arabicTextWidth = ctx.measureText(item.arabic).width;
                    const englishTextWidth = ctx.measureText(item.english).width;

                    let adjustedXArabic = baseX;
                    let adjustedXEnglish = baseX;

                    if (item.textAlign === "right") {
                        adjustedXArabic = baseX - arabicTextWidth;
                        adjustedXEnglish = baseX - englishTextWidth;
                    } else if (item.textAlign === "center") {
                        adjustedXArabic = baseX - arabicTextWidth / 2;
                        adjustedXEnglish = baseX - englishTextWidth / 2;
                    }

                    ctx.fillText(item.arabic, adjustedXArabic, startY + i * increment);
                    ctx.fillText(item.english, adjustedXEnglish, startY + i * increment + 40);
                });
            }
        };

        if (data.Inspection_Items) {
            drawTextItems(ctx, data.Inspection_Items, canvas.width - 425, startY, increment);
        }
        if (data.Inspection_Status) {
            drawTextItems(ctx, data.Inspection_Status, canvas.width - 900, startY, increment);
        }

        console.log("📝 رسم الملاحظات");

        let startY_Notes = 640;
        let increment_Notes = 104;

        const drawItems = (items, xPosition, startY, align) => {
            if (!items || items.length === 0) return;
            items.forEach((item, i) => {
                ctx.font = `${fixedConfig.tableStyle.fontWeight} ${fixedConfig.tableStyle.fontSize}px ${fixedConfig.tableStyle.fontFamily}`;
                ctx.fillStyle = fixedConfig.tableStyle.textColor;
                const content = item.content || "";
                const textWidth = ctx.measureText(content).width;

                let adjustedX = xPosition;
                if (align === "right") adjustedX = xPosition - textWidth;
                else if (align === "center") adjustedX = xPosition - textWidth / 2;

                ctx.fillText(content, adjustedX, startY + i * increment_Notes);
            });
        };

        drawItems(data.Notes_Items, canvas.width - 1160, startY_Notes, "right");

        console.log("✅ تم رسم كل العناصر بنجاح!");

        // تحويل الكانفس إلى صورة
        const imageBlob = await new Promise((resolve) => {
            canvas.toBlob(resolve, 'image/png');
        });

        return imageBlob;
    } catch (error) {
        console.error("🚨 خطأ أثناء رسم الصفحة الرابعة:", error);
    }
};
