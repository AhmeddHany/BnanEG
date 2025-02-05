
// رسم العقد على الـ Canvas
const DrawContractPage1 = async (canvas, data) => {
console.log("بدء رسم الصفحة الاولي");
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
            //الفرع//
            { key: "BRANCH_AR", x: canvas.width - 420, y: 986, align: "right", color: "#0D5485", fontSize: 50, fontWeight: "600" },
            { key: "BRANCH_EN", x: canvas.width - 420, y: 1045, align: "right", color: "#0D5485", fontSize: 35, fontWeight: "600" },
            { key: "PHONE_1", x: canvas.width - 526, y: 1135, align: "right" },
            { key: "PHONE_2", x: canvas.width - 1130, y: 1135, align: "right" },
            { key: "EMAIL", x: canvas.width - 1730, y: 1135, align: "right" },
            { key: "ADDRESS_AR", x: canvas.width - 535, y: 1225, align: "right", fontSize: 40 },
            { key: "ADDRESS_EN", x: canvas.width - 535, y: 1265, align: "right" },
            //السيارة//
            { key: "CAR_DETAILS_AR", x: canvas.width - 425, y: 1430, align: "right", color: "#0D5485", fontSize: 50, fontWeight: "600" },
            { key: "CAR_DETAILS_EN", x: canvas.width - 425, y: 1490, align: "right", color: "#0D5485", fontSize: 35, fontWeight: "600" },
            { key: "SERIAL_NUMBER", x: canvas.width - 630, y: 1575, align: "right" },
            { key: "REGISTRATION_AR", x: canvas.width - 1798, y: 1560, align: "right" },
            { key: "REGISTRATION_EN", x: canvas.width - 1798, y: 1600, align: "right" },
            { key: "FUEL_TYPE_AR", x: canvas.width - 570, y: 1667, align: "right" },
            { key: "FUEL_TYPE_EN", x: canvas.width - 570, y: 1705, align: "right" },
            { key: "TRANSMISSION_AR", x: canvas.width - 1855, y: 1667, align: "right" },
            { key: "TRANSMISSION_EN", x: canvas.width - 1855, y: 1705, align: "right" },
            { key: "OIL_TYPE_AR", x: canvas.width - 560, y: 1771, align: "right" },
            { key: "OIL_TYPE_EN", x: canvas.width - 560, y: 1811, align: "right" },
            { key: "CHANGE_OIL_DATE", x: canvas.width - 1832, y: 1785, align: "right" },
            { key: "REGISTRATION_END_DATE", x: canvas.width - 730, y: 1897, align: "right" },
            { key: "INESPECTION_END_DATE", x: canvas.width - 1960, y: 1890, align: "right" },
            { key: "OPERATING_CARD", x: canvas.width - 625, y: 1997, align: "right" },
            { key: "OPERATING_CARD_END", x: canvas.width - 1837, y: 1997, align: "right" },
            { key: "INSURANCE", x: canvas.width - 600, y: 2100, align: "right" },
            { key: "INSURANCE_END", x: canvas.width - 1837, y: 2100, align: "right" },
            { key: "PERODIC_MAINTENANCE", x: canvas.width - 680, y: 2205, align: "right" },
            { key: "TIRE_MAINTENANCE", x: canvas.width - 1855, y: 2205, align: "right" },
            { key: "FRONT_BREAK", x: canvas.width - 715, y: 2310, align: "right" },
            { key: "REAR_BREAK", x: canvas.width - 1930, y: 2310, align: "right" },
            // المالك
            { key: "OWNER_COMPANY_AR", x: canvas.width - 425, y: 2460, align: "right", color: "#0D5485", fontSize: 50, fontWeight: "600" },
            { key: "OWNER_COMPANY_EN", x: canvas.width - 425, y: 2520, align: "right", color: "#0D5485", fontSize: 35, fontWeight: "600" },
            { key: "ID_NUMBER", x: canvas.width - 515, y: 2610, align: "right" },
            { key: "ID_TYPE_AR", x: canvas.width - 1170, y: 2597, align: "right" },
            { key: "ID_TYPE_EN", x: canvas.width - 1170, y: 2634, align: "right" },
            { key: "VERSION_NUM", x: canvas.width - 1810, y: 2610, align: "right" },
            { key: "PHONE_3", x: canvas.width - 526, y: 2715, align: "right" },
            { key: "EMAIL_2", x: canvas.width - 1125, y: 2715, align: "right" },
            { key: "SIGNATURE_AR", x: canvas.width - 280, y: 2830, align: "right", fontSize: 40 },
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
