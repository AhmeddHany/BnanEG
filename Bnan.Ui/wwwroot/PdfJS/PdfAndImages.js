
const loadDynamicImages = async (images) => {
    try {
        const loadedImages = {};
        for (let key in images) {
            loadedImages[key] = await loadImage(images[key]);
        }
        return loadedImages;
    } catch (error) {
        console.error("Error loading images", error);
    }
};

const loadImage = (src) => {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve(img);
        img.onerror = (error) => {
            console.error(`Failed to load image: ${src}`, error);
            reject(error);
        };
        img.src = src;
    });
};
// تحويل الـ Canvas إلى PDF
const createPdf = async (PdfNo, canvas, InputPdf, InputHaveNo) => {
    const doc = new jsPDF("p", "pt", "a4", true);
    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();

    // تحويل canvas إلى blob بشكل متزامن
    const blob = await new Promise((resolve, reject) => {
        canvas.toBlob((blob) => {
            if (blob) {
                resolve(blob);
            } else {
                reject("Error generating blob from canvas.");
            }
        });
    });

    // قراءة blob وتحويله إلى Data URL بشكل متزامن
    const imageDataUrl = await new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject("Error reading blob.");
        reader.readAsDataURL(blob);
    });

    // إضافة الصورة إلى الـ PDF
    doc.addImage(imageDataUrl, "PNG", 0, 0, pageWidth, pageHeight, "", "FAST");

    // إنشاء الـ PDF Blob و Base64
    const pdfBlob = doc.output("blob");
    const pdfBase64 = doc.output("datauristring");

    // تخزين الـ PDF في المدخلات
    document.getElementById(InputPdf).value = pdfBase64;
    document.getElementById(InputHaveNo).value = PdfNo;

    console.log("pdfBase64", pdfBase64);
    console.log("PdfNo", PdfNo);

    // يمكنك إضافة منطق لتحميل الـ PDF إذا كان مطلوبًا
};
// تحويل الـ Canvas إلى PDF
const createMergedPdfs = async (PdfNo, canvas, InputPdf, InputHaveNo, exitingInvoicePdf) => {
    const doc = new jsPDF("p", "pt", "a4", true);
    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();

    try {
        // تحويل canvas إلى blob بشكل متزامن
        const blob = await new Promise((resolve, reject) => {
            canvas.toBlob((blob) => {
                if (blob) {
                    resolve(blob);
                } else {
                    reject("Error generating blob from canvas.");
                }
            });
        });

        // قراءة blob وتحويله إلى Data URL بشكل متزامن
        const imageDataUrl = await new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = () => reject("Error reading blob.");
            reader.readAsDataURL(blob);
        });

        // إضافة الصورة إلى الـ PDF
        doc.addImage(imageDataUrl, "PNG", 0, 0, pageWidth, pageHeight, "", "FAST");
        const pdfBlob = doc.output("blob");

        // التحقق مما إذا كان exitingInvoicePdf صالحًا قبل الدمج
        let mergedPdfBase64;

        if (exitingInvoicePdf && exitingInvoicePdf.length > 0) {
            try {
                // دمج الـ PDF الجديد مع الـ exitingInvoicePdf
                mergedPdfBase64 = await mergePdfs(exitingInvoicePdf, pdfBlob);
            } catch (error) {
                console.error('Error merging PDFs:', error);
                // في حالة وجود خطأ، استخدام الـ PDF الجديد فقط
                mergedPdfBase64 = doc.output('datauristring');
            }
        } else {
            // إذا لم يكن exitingInvoicePdf موجودًا، استخدام الـ PDF الجديد فقط
            mergedPdfBase64 = doc.output('datauristring');
        }

        // تحديث الحقل المخفي بـ Base64 الناتج
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