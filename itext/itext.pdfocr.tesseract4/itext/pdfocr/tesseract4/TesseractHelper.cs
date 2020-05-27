using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Logging;
using iText.IO.Util;
using iText.Pdfocr;
using iText.StyledXmlParser.Jsoup.Nodes;
using iText.StyledXmlParser.Jsoup.Select;

namespace iText.Pdfocr.Tesseract4 {
    /// <summary>Helper class.</summary>
    public class TesseractHelper {
        /// <summary>The logger.</summary>
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(iText.Pdfocr.Tesseract4.TesseractHelper)
            );

        /// <summary>
        /// Creates a new
        /// <see cref="TesseractHelper"/>
        /// instance.
        /// </summary>
        private TesseractHelper() {
        }

        /// <summary>
        /// Parses each hocr file from the provided list, retrieves text, and
        /// returns data in the format described below.
        /// </summary>
        /// <param name="inputFiles">list of input files</param>
        /// <param name="textPositioning">
        /// 
        /// <see cref="TextPositioning"/>
        /// </param>
        /// <returns>
        /// 
        /// <see cref="System.Collections.IDictionary{K, V}"/>
        /// where key is
        /// <see cref="int?"/>
        /// representing the number of the page and value is
        /// <see cref="System.Collections.IList{E}"/>
        /// of
        /// <see cref="iText.Pdfocr.TextInfo"/>
        /// elements where each
        /// <see cref="iText.Pdfocr.TextInfo"/>
        /// element contains a word or a line and its 4
        /// coordinates(bbox)
        /// </returns>
        public static IDictionary<int, IList<TextInfo>> ParseHocrFile(IList<FileInfo> inputFiles, TextPositioning 
            textPositioning) {
            IDictionary<int, IList<TextInfo>> imageData = new LinkedDictionary<int, IList<TextInfo>>();
            foreach (FileInfo inputFile in inputFiles) {
                if (inputFile != null && File.Exists(System.IO.Path.Combine(inputFile.FullName))) {
                    FileStream fileInputStream = new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read);
                    Document doc = iText.StyledXmlParser.Jsoup.Jsoup.Parse(fileInputStream, System.Text.Encoding.UTF8.Name(), 
                        inputFile.FullName);
                    Elements pages = doc.GetElementsByClass("ocr_page");
                    Regex bboxPattern = iText.IO.Util.StringUtil.RegexCompile(".*bbox(\\s+\\d+){4}.*");
                    Regex bboxCoordinatePattern = iText.IO.Util.StringUtil.RegexCompile(".*\\s+(\\d+)\\s+(\\d+)\\s+(\\d+)\\s+(\\d+).*"
                        );
                    IList<String> searchedClasses = TextPositioning.BY_LINES.Equals(textPositioning) ? JavaUtil.ArraysAsList("ocr_line"
                        , "ocr_caption") : JavaCollectionsUtil.SingletonList<String>("ocrx_word");
                    foreach (iText.StyledXmlParser.Jsoup.Nodes.Element page in pages) {
                        String[] pageNum = iText.IO.Util.StringUtil.Split(page.Id(), "page_");
                        int pageNumber = Convert.ToInt32(pageNum[pageNum.Length - 1]);
                        IList<TextInfo> textData = new List<TextInfo>();
                        if (searchedClasses.Count > 0) {
                            Elements objects = page.GetElementsByClass(searchedClasses[0]);
                            for (int i = 1; i < searchedClasses.Count; i++) {
                                Elements foundElements = page.GetElementsByClass(searchedClasses[i]);
                                for (int j = 0; j < foundElements.Count; j++) {
                                    objects.Add(foundElements[j]);
                                }
                            }
                            foreach (iText.StyledXmlParser.Jsoup.Nodes.Element obj in objects) {
                                String value = obj.Attr("title");
                                Match bboxMatcher = iText.IO.Util.StringUtil.Match(bboxPattern, value);
                                if (bboxMatcher.Success) {
                                    Match bboxCoordinateMatcher = iText.IO.Util.StringUtil.Match(bboxCoordinatePattern, iText.IO.Util.StringUtil.Group
                                        (bboxMatcher));
                                    if (bboxCoordinateMatcher.Success) {
                                        IList<float> coordinates = new List<float>();
                                        for (int i = 0; i < 4; i++) {
                                            String coord = iText.IO.Util.StringUtil.Group(bboxCoordinateMatcher, i + 1);
                                            coordinates.Add(float.Parse(coord, System.Globalization.CultureInfo.InvariantCulture));
                                        }
                                        textData.Add(new TextInfo(obj.Text(), coordinates));
                                    }
                                }
                            }
                        }
                        if (textData.Count > 0) {
                            if (imageData.ContainsKey(pageNumber)) {
                                pageNumber = Enumerable.Max(imageData.Keys) + 1;
                            }
                            imageData.Put(pageNumber, textData);
                        }
                    }
                    fileInputStream.Dispose();
                }
            }
            return imageData;
        }

        /// <summary>Deletes file using provided path.</summary>
        /// <param name="pathToFile">path to the file to be deleted</param>
        internal static void DeleteFile(String pathToFile) {
            try {
                if (pathToFile != null && !String.IsNullOrEmpty(pathToFile) && File.Exists(System.IO.Path.Combine(pathToFile
                    ))) {
                    File.Delete(System.IO.Path.Combine(pathToFile));
                }
            }
            catch (Exception e) {
                LOGGER.Info(MessageFormatUtil.Format(Tesseract4LogMessageConstant.CANNOT_DELETE_FILE, pathToFile, e.Message
                    ));
            }
        }

        /// <summary>Reads from text file to string.</summary>
        /// <param name="txtFile">
        /// input
        /// <see cref="System.IO.FileInfo"/>
        /// to be read
        /// </param>
        /// <returns>
        /// result
        /// <see cref="System.String"/>
        /// from provided text file
        /// </returns>
        internal static String ReadTxtFile(FileInfo txtFile) {
            String content = null;
            try {
                content = iText.IO.Util.JavaUtil.GetStringForBytes(System.IO.File.ReadAllBytes(txtFile.FullName), System.Text.Encoding
                    .UTF8);
            }
            catch (System.IO.IOException e) {
                LOGGER.Error(MessageFormatUtil.Format(Tesseract4LogMessageConstant.CANNOT_READ_FILE, txtFile.FullName, e.Message
                    ));
            }
            return content;
        }

        /// <summary>
        /// Writes provided
        /// <see cref="System.String"/>
        /// to text file using
        /// provided path.
        /// </summary>
        /// <param name="path">
        /// path as
        /// <see cref="System.String"/>
        /// to file to be created
        /// </param>
        /// <param name="data">
        /// text data in required format as
        /// <see cref="System.String"/>
        /// </param>
        internal static void WriteToTextFile(String path, String data) {
            try {
                using (TextWriter writer = new StreamWriter(new FileStream(path, FileMode.Create), System.Text.Encoding.UTF8
                    )) {
                    writer.Write(data);
                }
            }
            catch (System.IO.IOException e) {
                LOGGER.Error(MessageFormatUtil.Format(Tesseract4LogMessageConstant.CANNOT_WRITE_TO_FILE, path, e.Message));
            }
        }

        /// <summary>Runs given command.</summary>
        /// <param name="execPath">path to the executable</param>
        /// <param name="paramsList">
        /// 
        /// <see cref="System.Collections.IList{E}"/>
        /// of command line arguments
        /// </param>
        internal static void RunCommand(String execPath, IList<String> paramsList) {
            try {
                String @params = String.Join(" ", paramsList);
                bool cmdSucceeded = SystemUtil.RunProcessAndWait(execPath, @params);
                if (!cmdSucceeded) {
                    LOGGER.Error(MessageFormatUtil.Format(Tesseract4LogMessageConstant.COMMAND_FAILED, execPath + " " + @params
                        ));
                    throw new Tesseract4OcrException(Tesseract4OcrException.TESSERACT_FAILED);
                }
            }
            catch (Exception e) {
                // NOSONAR
                LOGGER.Error(MessageFormatUtil.Format(Tesseract4LogMessageConstant.COMMAND_FAILED, e.Message));
                throw new Tesseract4OcrException(Tesseract4OcrException.TESSERACT_FAILED);
            }
        }
    }
}
