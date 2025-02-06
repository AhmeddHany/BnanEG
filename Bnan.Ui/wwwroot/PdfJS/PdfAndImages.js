
const loadDynamicImages = async (images) => {
    const loadedImages = {};

    for (let key in images) {
        if (!images[key]) continue; // تخطي الصور الفارغة أو غير المعرفة
        
        const img = await loadImage(images[key]);
        if (img) {
            loadedImages[key] = img;
        } else {
            console.warn(`Skipping image: ${key}, failed to load.`);
        }
    }

    return loadedImages;
};

const loadImage = (src) => {
    return new Promise((resolve) => {
        if (!src) {
            console.warn("Invalid image source:", src);
            return resolve(null); // إرجاع null بدلاً من رفض الـ Promise
        }

        const img = new Image();
        img.onload = () => resolve(img);
        img.onerror = () => {
            console.warn(`Failed to load image: ${src}`);
            resolve(null); // إرجاع null بدلاً من رفض الـ Promise
        };
        img.src = src;
    });
};
const convertBase64ToImage = (base64String) => {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve(img); // عندما يتم تحميل الصورة بنجاح
        img.onerror = (error) => reject(error); // في حالة حدوث خطأ أثناء تحميل الصورة
        img.src = base64String; // تعيين Base64 للصورة
    });
};

const convertBlobToImage = async (blobUrl) => {
    return new Promise((resolve, reject) => {
        fetch(blobUrl)
            .then((response) => response.blob()) // تحويل الـ Blob
            .then((blob) => {
                const img = new Image();
                img.onload = () => resolve(img); // عندما يتم تحميل الصورة بنجاح
                img.onerror = (error) => reject(error); // في حالة حدوث خطأ أثناء تحميل الصورة
                img.src = URL.createObjectURL(blob); // تحويل الـ Blob إلى URL يمكن تحميله
            })
            .catch((error) => reject(error)); // في حال حدوث أي خطأ أثناء جلب الـ Blob
    });
};
// تحويل الـ Canvas إلى PDF بحجم مضغوط
const createPdf = async (PdfNo, canvas, InputPdf, InputHaveNo) => {
    const doc = new jsPDF("p", "pt", "a4", true);
    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();

    // تحويل canvas إلى blob بجودة أقل لضغط الحجم
    const blob = await new Promise((resolve, reject) => {
        canvas.toBlob((blob) => {
            if (blob) {
                resolve(blob);
            } else {
                reject("Error generating blob from canvas.");
            }
        }, "image/jpeg", 0.2); // 🔹 تحويل إلى JPEG بجودة 0.3 لتقليل الحجم
    });

    // قراءة blob وتحويله إلى Data URL بشكل متزامن
    const imageDataUrl = await new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject("Error reading blob.");
        reader.readAsDataURL(blob);
    });

    // ضبط أبعاد الصورة داخل الصفحة
    const img = new Image();
    img.src = imageDataUrl;

    await new Promise((resolve) => (img.onload = resolve));

    let imgWidth = img.width;
    let imgHeight = img.height;
    let scale = Math.min(pageWidth / imgWidth, pageHeight / imgHeight); // 🔹 تصغير الأبعاد بما يناسب الصفحة

    imgWidth *= scale;
    imgHeight *= scale;

    const imgXPos = (pageWidth - imgWidth) / 2; // توسيط الصورة أفقيًا
    const imgYPos = (pageHeight - imgHeight) / 2; // توسيط الصورة عموديًا

    // إضافة الصورة إلى الـ PDF بعد الضغط
    doc.addImage(imageDataUrl, "JPEG", imgXPos, imgYPos, imgWidth, imgHeight, "", "FAST");

    // إنشاء PDF مضغوط
    const pdfBlob = doc.output("blob");
    const pdfBase64 = doc.output("datauristring", { compress: true }); // 🔹 تفعيل الضغط

    // تخزين الـ PDF في المدخلات
    document.getElementById(InputPdf).value = pdfBase64;
    document.getElementById(InputHaveNo).value = PdfNo;

    console.log("pdfBase64", pdfBase64);
    console.log("PdfNo", PdfNo);
};
// تحويل الـ Canvas إلى PDF
const createMergedPdfs = async (PdfNo, canvas, InputPdf, InputHaveNo, exitingInvoicePdf) => {
    const doc = new jsPDF("p", "pt", "a4", true);
    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();

    try {
        // 🔹 تحويل canvas إلى blob بجودة أقل لضغط الحجم
        const blob = await new Promise((resolve, reject) => {
            canvas.toBlob((blob) => {
                if (blob) {
                    resolve(blob);
                } else {
                    reject("Error generating blob from canvas.");
                }
            }, "image/jpeg", 0.2); // 🔹 تحويل إلى JPEG بجودة 0.3 لتقليل الحجم
        });

        // 🔹 قراءة blob وتحويله إلى Data URL
        const imageDataUrl = await new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = () => reject("Error reading blob.");
            reader.readAsDataURL(blob);
        });

        // 🔹 ضبط أبعاد الصورة داخل الصفحة
        const img = new Image();
        img.src = imageDataUrl;
        await new Promise((resolve) => (img.onload = resolve));

        let imgWidth = img.width;
        let imgHeight = img.height;
        let scale = Math.min(pageWidth / imgWidth, pageHeight / imgHeight); // 🔹 تصغير الأبعاد بما يناسب الصفحة
        imgWidth *= scale;
        imgHeight *= scale;

        const imgXPos = (pageWidth - imgWidth) / 2; // توسيط الصورة أفقيًا
        const imgYPos = (pageHeight - imgHeight) / 2; // توسيط الصورة عموديًا

        // 🔹 إضافة الصورة إلى الـ PDF بعد الضغط
        doc.addImage(imageDataUrl, "JPEG", imgXPos, imgYPos, imgWidth, imgHeight, "", "FAST");
        const pdfBlob = doc.output("blob");

        // 🔹 التحقق مما إذا كان هناك ملف PDF موجود مسبقًا للدمج
        let mergedPdfBase64;
        if (exitingInvoicePdf && exitingInvoicePdf.length > 0) {
            try {
                // 🔹 دمج الـ PDF الجديد مع الـ exitingInvoicePdf
                mergedPdfBase64 = await mergePdfs(exitingInvoicePdf, pdfBlob, true); // تمرير `true` للضغط
            } catch (error) {
                console.error('Error merging PDFs:', error);
                mergedPdfBase64 = doc.output('datauristring', { compress: true }); // 🔹 استخدام الضغط
            }
        } else {
            mergedPdfBase64 = doc.output('datauristring', { compress: true });
        }

        // 🔹 تحديث الحقل المخفي بـ Base64 الناتج
        document.getElementById(InputPdf).value = mergedPdfBase64;
        document.getElementById(InputHaveNo).value = PdfNo;

        console.log("mergedPdfBase64", mergedPdfBase64);
        console.log("PdfNo", PdfNo);

    } catch (error) {
        console.error('Error creating or merging PDFs:', error);
    }
};

const mergePdfs = async (existingPdfPath, newPdfBlob) => {
     try {
         console.log(`Fetching existing PDF from ${existingPdfPath}`);
         const existingPdfResponse = await fetch(existingPdfPath);
         if (!existingPdfResponse.ok) {
             console.error(`Failed to fetch existing PDF: ${existingPdfResponse.statusText}`);
             throw new Error('Failed to fetch existing PDF');
         }

         const existingPdfBlob = await existingPdfResponse.blob();
         const existingPdfBytes = await existingPdfBlob.arrayBuffer();
         const newPdfBytes = await newPdfBlob.arrayBuffer();

         const existingPdfDoc = await PDFLib.PDFDocument.load(existingPdfBytes);
         const newPdfDoc = await PDFLib.PDFDocument.load(newPdfBytes);

         const copiedPages = await existingPdfDoc.copyPages(newPdfDoc, newPdfDoc.getPageIndices());
         copiedPages.forEach((page) => {
             existingPdfDoc.addPage(page);
         });

         const mergedPdfBytes = await existingPdfDoc.save();
         const base64String = arrayBufferToBase64(mergedPdfBytes);
         return base64String;
     } catch (error) {
         console.error('Error in mergePdfs:', error);
         throw error;
     }
 };
const arrayBufferToBase64 = (arrayBuffer) => {
     const uint8Array = new Uint8Array(arrayBuffer);
     let binaryString = '';
     uint8Array.forEach(byte => {
         binaryString += String.fromCharCode(byte);
     });
     return btoa(binaryString);
};
// دالة لدمج كل الصفحات وتحويلها إلى PDF
const generateContractPdf = async (canvasArray, InputPdf) => {
    const imageBlobs = [];
    for (const canvas of canvasArray) {
        // تحويل كل Canvas إلى صورة (Blob)
        const imageBlob = await new Promise((resolve) => {
            canvas.toBlob(resolve, 'image/png');
        });
        imageBlobs.push(imageBlob);
    }
    // بعد جمع كل الصور، نرسلها إلى دالة إنشاء الـ PDF
    await createPdfWithMultiPhoto(imageBlobs, InputPdf);
};
const createPdfWithMultiPhoto = async (imageBlobs, InputPdf) => {
    const doc = new jsPDF('p', 'pt', 'a4', true);

    for (let imageIndex = 0; imageIndex < imageBlobs.length; imageIndex++) {
        if (imageIndex > 0) {
            doc.addPage();
        }
        doc.setPage(imageIndex + 1);

        const pageWidth = doc.internal.pageSize.getWidth();
        const pageHeight = doc.internal.pageSize.getHeight();

        // ضغط الصورة مع تقليل الحجم أكثر
        const blob = imageBlobs[imageIndex];
        const imgCompressed = await compressImage(blob, 0.2);
        const img = await createImageFromBlob(imgCompressed);

        // 🔹 جعل الصورة تغطي الصفحة بالكامل دون فراغ زائد
        let imgWidth = pageWidth; // تغيير إلى let بدلاً من const
        let imgHeight = (imgWidth * img.height) / img.width; // تغيير إلى let بدلاً من const

        // إذا كانت الصورة أطول من الصفحة، سنتأكد من أنها تغطي العرض الكامل
        if (imgHeight < pageHeight) {
            const scale = pageHeight / imgHeight;
            imgHeight *= scale;
            imgWidth *= scale;
        }

        const imgXPos = 0; // تأكد أن الصورة تبدأ من أقصى اليسار
        const imgYPos = (pageHeight - imgHeight) / 2; // توسيط الصورة عموديًا

        doc.addImage(img, 'JPEG', imgXPos, imgYPos, imgWidth, imgHeight, '', 'FAST');
    }

    // 🔹 استخدام الضغط لتقليل حجم PDF أكثر
    const pdfBase64 = doc.output('datauristring', { compress: true });
    document.getElementById(InputPdf).value = pdfBase64;
    doc.save("Contracts.pdf")
};
const compressImage = (blob, quality) => {
    return new Promise((resolve) => {
        const img = new Image();
        img.src = URL.createObjectURL(blob);
        img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = img.width;
            canvas.height = img.height;
            const ctx = canvas.getContext('2d');

            // 🔹 تعيين الخلفية بيضاء لمنع أي حواف سوداء عند الضغط
            ctx.fillStyle = "white";
            ctx.fillRect(0, 0, canvas.width, canvas.height);
            ctx.drawImage(img, 0, 0, img.width, img.height);
            canvas.toBlob((compressedBlob) => resolve(compressedBlob), 'image/jpeg', quality);
        };
    });
};
// Helper function to create an image element from a blob
const createImageFromBlob = (blob) => {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve(img);
        img.onerror = reject;
        img.src = URL.createObjectURL(blob);
    });
};