


//  لرسم الإيصال على الصورة
const DrawContractPage3 = async (canvas, data) => {
  console.log("بدء رسم الإيصال");
  const ctx = canvas.getContext("2d");

  // إعداد حجم الصورة بناءً على صورة الخلفية
  const backgroundImg = data.images.background;
  canvas.width = 2481;
  canvas.height = backgroundImg.height;
  ctx.drawImage(backgroundImg, 0, 0);

  // إعدادات النصوص والصور
  const fixedConfig = {
    texts : [
      { key: "NUMBER", x: canvas.width - 2360, y: 358, align: "left", color: "#0D5485", fontSize: 40, fontWeight: "700"  },
      { key: "CONTRACT_NUMBER", x: canvas.width - 2420, y: 405, align: "left", color: "#0D5485", fontSize: 45, fontWeight: "700" },
      
      // بنود التأجير //
      { key: "DELIVERY_BRANCH_AR", x: canvas.width - 615, y: 505, fontSize: 40 , align: "right"},
      { key: "DELIVERY_BRANCH_EN", x: canvas.width - 615, y: 545, align: "right" },
      { key: "RECEIVING_BRANCH_AR", x: canvas.width - 1230, y: 505, fontSize: 40 , align: "right" },
      { key: "RECEIVING_BRANCH_EN", x: canvas.width - 1230, y: 545 , align: "right" },
      { key: "Geographic_area_AR", x: canvas.width - 1890, y: 505, fontSize: 40 , align: "right" },
      { key: "Geographic_area_EN", x: canvas.width - 1890, y: 545 , align: "right"},
      { key: "DAILY_RENT", x: canvas.width - 605, y: 628 , align: "right"},
      { key: "Additional_driver", x: canvas.width - 1230, y: 628 , align: "right"},
      { key: "Personal_driver", x: canvas.width - 1845, y: 628 , align: "right"},
      { key: "Free_hours", x: canvas.width - 605, y: 728 , align: "right" },
      { key: "Maximum_hours", x: canvas.width - 1317, y: 728 , align: "right" },
      { key: "Free_KM", x: canvas.width - 1870, y: 728 , align: "right"},
      { key: "Extra_KM_Value", x: canvas.width - 670, y: 835 , align: "right"},
      { key: "Extra_hours_Value", x: canvas.width - 1335, y: 835 , align: "right"},
      { key: "full_Fuel_value", x: canvas.width - 1885, y: 835 , align: "right"},
      { key: "Discount_rate", x: canvas.width - 600, y: 940 , align: "right"},
      { key: "Value_added_ratio", x: canvas.width - 1295, y: 940 , align: "right"},
      { key: "Total_expected_contract", x: canvas.width - 1920, y: 940 , align: "right"},
      { key: "accident_Liability", x: canvas.width - 635, y: 1045 , align: "right"},
      { key: "theft_Liability", x: canvas.width - 1210, y: 1045 , align: "right" },
      { key: "fire_Liability", x: canvas.width - 600, y: 1147 , align: "right"},
      { key: "drowning_Liability", x: canvas.width - 1245, y: 1147 , align: "right"},
      
      // سياسة التأجير //
      { key: "RENTAL_POLICY_AR", x: canvas.width - 635, y: 2290, fontSize: 40 , align: "right"},
      { key: "Fuel_Policy", x: canvas.width - 1520, y: 2290, fontSize: 40 , align: "right"},
      { key: "RETURN_POLICY_AR", x: canvas.width - 645, y: 2400, fontSize: 40 , align: "right" },
      { key: "EXTEND_POLICY", x: canvas.width - 730, y: 2495, fontSize: 40 , align: "right"},
      { key: "ACCIDENT_POLICY", x: canvas.width - 640, y: 2605 , align: "right" },
      { key: "EXTENTION_POLICY", x: canvas.width - 1520, y: 2495 , align: "right" },
      { key: "FAULT_POLICY", x: canvas.width - 1520, y: 2605 , align: "right" },

      // التوقيع //
      { key: "SIGNATURE_AR", x: canvas.width - 280, y: 2830, fontSize: 40  , align: "right"},
      { key: "SIGNATURE_EN", x: canvas.width - 280, y: 2870 , align: "right" },
    ],
    images: [
      { content: data.images.EMPLOYEE_SIGN, x: canvas.width - 705, y: 2810, width: 160, height: 50  },
      { content: data.images.TENANT_SIGN,  x: canvas.width - 500, y: 2910, width: 160, height: 50   },
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
  fixedConfig.texts.forEach(({ key, x, y, align , color , fontSize , fontWeight}) => {
    ctx.font =  `${fontWeight || fixedConfig.textStyle.fontWeight} ${fontSize || fixedConfig.textStyle.fontSize}px ${fixedConfig.textStyle.fontFamily}`;
    ctx.fillStyle = color||fixedConfig.textStyle.textColor;
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

  // رسم الجدول (عناصر الخيارات و الاضافاتات و المميزات)
  const startY = 1395; 
  const increment = 105; 
  const startYForNumbers = 1395;
  const drawTextItems = (ctx, items, baseX, startY, increment) => {
    if (items && items.length > 0) {
      items.forEach((item, i) => {
        ctx.font = `${fixedConfig.tableStyle.fontWeight} ${fixedConfig.tableStyle.fontSize}px ${fixedConfig.tableStyle.fontFamily}`;
        ctx.fillStyle = fixedConfig.tableStyle.textColor;
  
        // قياس عرض النص للغة العربية
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
  
        // رسم النص العربي
        ctx.fillText(item.arabic, adjustedXArabic, startY + i * increment);
        // رسم النص الإنجليزي
        ctx.fillText(item.english, adjustedXEnglish, startY + i * increment + 40);
      });
    }
  };
  // Usage
if (data.OptionsItems) {
  drawTextItems(ctx, data.OptionsItems, canvas.width - 190, startY, increment);
}

if (data.AdditionalItems) {
  drawTextItems(ctx, data.AdditionalItems, canvas.width - 975, startY, increment);
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

      ctx.fillText(content, adjustedX, startY + i * increment + 22);
     
    });
  };

  // رسم جميع فئات العناصر
  drawItems(data.Option_Price_Items, canvas.width - 710, startYForNumbers, "right");
  drawItems(data.Addition_Price_Items, canvas.width - 1500, startYForNumbers, "right");
  const imageBlob = await new Promise((resolve) => {
        canvas.toBlob(resolve, 'image/png');
  });
  return imageBlob;
};