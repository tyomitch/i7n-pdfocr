using System;
using System.IO;
using iText.IO.Util;
using iText.Kernel;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Pdfa;
using iText.Pdfocr.Helpers;
using iText.Test;
using iText.Test.Attributes;

namespace iText.Pdfocr {
    public class PdfA3uTest : ExtendedITextTest {
        [NUnit.Framework.Test]
        public virtual void TestPdfA3uWithNullIntent() {
            String testName = "testPdfA3uWithNullIntent";
            String path = PdfHelper.GetDefaultImagePath();
            String pdfPath = PdfHelper.GetTargetDirectory() + testName + ".pdf";
            OcrPdfCreatorProperties properties = new OcrPdfCreatorProperties();
            properties.SetTextColor(DeviceCmyk.BLACK);
            properties.SetScaleMode(ScaleMode.SCALE_TO_FIT);
            PdfHelper.CreatePdfA(pdfPath, new FileInfo(path), properties, null);
            String result = PdfHelper.GetTextFromPdfLayer(pdfPath, "Text Layer");
            NUnit.Framework.Assert.AreEqual(PdfHelper.DEFAULT_EXPECTED_RESULT, result);
            NUnit.Framework.Assert.AreEqual(ScaleMode.SCALE_TO_FIT, properties.GetScaleMode());
        }

        [NUnit.Framework.Test]
        public virtual void TestIncompatibleOutputIntentAndFontColorSpaceException() {
            NUnit.Framework.Assert.That(() =>  {
                String testName = "testIncompatibleOutputIntentAndFontColorSpaceException";
                String path = PdfHelper.GetDefaultImagePath();
                String pdfPath = PdfHelper.GetTargetDirectory() + testName + ".pdf";
                PdfHelper.CreatePdfA(pdfPath, new FileInfo(path), new OcrPdfCreatorProperties().SetTextColor(DeviceCmyk.BLACK
                    ), PdfHelper.GetRGBPdfOutputIntent());
            }
            , NUnit.Framework.Throws.InstanceOf<PdfException>().With.Message.EqualTo(PdfAConformanceException.DEVICECMYK_MAY_BE_USED_ONLY_IF_THE_FILE_HAS_A_CMYK_PDFA_OUTPUT_INTENT_OR_DEFAULTCMYK_IN_USAGE_CONTEXT))
;
        }

        [NUnit.Framework.Test]
        public virtual void TestPdfA3DefaultMetadata() {
            String testName = "testPdfDefaultMetadata";
            String path = PdfHelper.GetDefaultImagePath();
            String pdfPath = PdfHelper.GetTargetDirectory() + testName + ".pdf";
            FileInfo file = new FileInfo(path);
            PdfHelper.CreatePdfA(pdfPath, file, new OcrPdfCreatorProperties().SetTextColor(DeviceRgb.BLACK), PdfHelper
                .GetRGBPdfOutputIntent());
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfPath));
            NUnit.Framework.Assert.AreEqual("en-US", pdfDocument.GetCatalog().GetLang().ToString());
            NUnit.Framework.Assert.AreEqual("", pdfDocument.GetDocumentInfo().GetTitle());
            NUnit.Framework.Assert.AreEqual(PdfAConformanceLevel.PDF_A_3U, pdfDocument.GetReader().GetPdfAConformanceLevel
                ());
            pdfDocument.Close();
        }

        [NUnit.Framework.Test]
        public virtual void TestPdfCustomMetadata() {
            String testName = "testPdfCustomMetadata";
            String path = PdfHelper.GetDefaultImagePath();
            String pdfPath = PdfHelper.GetTargetDirectory() + testName + ".pdf";
            FileInfo file = new FileInfo(path);
            OcrPdfCreatorProperties properties = new OcrPdfCreatorProperties();
            String locale = "nl-BE";
            properties.SetPdfLang(locale);
            String title = "Title";
            properties.SetTitle(title);
            PdfHelper.CreatePdfA(pdfPath, file, new OcrPdfCreatorProperties(properties), PdfHelper.GetCMYKPdfOutputIntent
                ());
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfPath));
            NUnit.Framework.Assert.AreEqual(locale, pdfDocument.GetCatalog().GetLang().ToString());
            NUnit.Framework.Assert.AreEqual(title, pdfDocument.GetDocumentInfo().GetTitle());
            NUnit.Framework.Assert.AreEqual(PdfAConformanceLevel.PDF_A_3U, pdfDocument.GetReader().GetPdfAConformanceLevel
                ());
            pdfDocument.Close();
        }

        [LogMessage(PdfOcrLogMessageConstant.PROVIDED_FONT_CONTAINS_NOTDEF_GLYPHS, Count = 1)]
        [NUnit.Framework.Test]
        public virtual void TestNonCompliantThaiPdfA() {
            NUnit.Framework.Assert.That(() =>  {
                String testName = "testNonCompliantThaiPdfA";
                String path = PdfHelper.GetThaiImagePath();
                String pdfPath = PdfHelper.GetTargetDirectory() + testName + ".pdf";
                PdfHelper.CreatePdfA(pdfPath, new FileInfo(path), new OcrPdfCreatorProperties().SetTextColor(DeviceRgb.BLACK
                    ), PdfHelper.GetRGBPdfOutputIntent());
            }
            , NUnit.Framework.Throws.InstanceOf<OcrException>().With.Message.EqualTo(MessageFormatUtil.Format(OcrException.CANNOT_CREATE_PDF_DOCUMENT, PdfOcrLogMessageConstant.PROVIDED_FONT_CONTAINS_NOTDEF_GLYPHS)))
;
        }

        [NUnit.Framework.Test]
        public virtual void TestCompliantThaiPdfA() {
            String testName = "testCompliantThaiPdfA";
            String path = PdfHelper.GetThaiImagePath();
            String pdfPath = PdfHelper.GetTargetDirectory() + testName + ".pdf";
            OcrPdfCreatorProperties ocrPdfCreatorProperties = new OcrPdfCreatorProperties();
            ocrPdfCreatorProperties.SetTextColor(DeviceRgb.BLACK);
            ocrPdfCreatorProperties.SetFontPath(PdfHelper.GetKanitFontPath());
            PdfHelper.CreatePdfA(pdfPath, new FileInfo(path), ocrPdfCreatorProperties, PdfHelper.GetRGBPdfOutputIntent
                ());
        }
    }
}
