using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.IO;
using log4net;

using System.Text.RegularExpressions;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FileHelpers;
using OpenXmlPowerTools;

using TallComponents.PDF;
using TallComponents.PDF.Shapes;
using TallComponents.PDF.TextExtraction;
using TallComponents.PDF.Transforms;

namespace TagConverter
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        [DelimitedRecord(",")]
        public class Record
        {
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string CompetitorTag;
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string DocusignTag;
        }

        public static void convertPDFDoc(string fileDir, string fileName, string fileExtension)
        {
            var Readengine = new FileHelperAsyncEngine<Record>(); //read document goes line by line

            Readengine.BeginReadFile(DSConfig.CsvTags);

            using (Readengine)
            {

                using (FileStream fileIn = new FileStream(fileDir + "\\" + fileName, FileMode.Open, FileAccess.Read))
                {
                    TallComponents.PDF.Document document = new TallComponents.PDF.Document(fileIn);

                    foreach (Record r in Readengine)
                    {
                        //create document


                        TextFindCriteria criteria = new TextFindCriteria(r.CompetitorTag, false, false);
                        TextMatchEnumerator enumerator = document.Find(criteria);

                        overlayText(enumerator, r.DocusignTag);
                    }
                    using (FileStream fileOut = new FileStream(DSConfig.DropoffDir + "\\" + fileName.Substring(0, fileName.Length - fileExtension.Length) + "_converted" + fileExtension, FileMode.Create, FileAccess.Write))
                    {
                        document.Write(fileOut);
                    }

                }
            }
        }

        public static void overlayText(TextMatchEnumerator enumerator, string dsTag)
        {
            foreach (TextMatch match in enumerator)
            {

                GlyphCollection glyphs = match.Glyphs;
                Glyph firstGlyph = glyphs[0];
                Glyph lastGlyph = glyphs[glyphs.Count - 1];

                MultilineTextShape textShape = new MultilineTextShape();
                TranslateTransform translate = new TranslateTransform(firstGlyph.BottomLeft.X, firstGlyph.BottomLeft.Y);
                textShape.Transform = translate;

                textShape.Width = lastGlyph.TopRight.X - firstGlyph.BottomLeft.X;
                textShape.Height = lastGlyph.TopRight.Y - firstGlyph.BottomLeft.Y;
                translate.Y += textShape.Height;
                textShape.HorizontalAlignment = HorizontalAlignment.Center;
                textShape.Opacity = 200;

                Fragment fragment = new Fragment(dsTag, 12);
                fragment.TextColor = TallComponents.PDF.Colors.RgbColor.Blue;
                textShape.Fragments.Add(fragment);

                match.Page.Overlay.Add(textShape);

            }
        }

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            DirectoryInfo pickupDir = new DirectoryInfo(DSConfig.PickupDir);

            //read csv and place it in memory

            foreach (var file in pickupDir.GetFiles())
            {
                if (file.Extension.Equals(".pdf"))
                {
                    Log.Info("Attempt to Convert the following PDF :: " + file.Name);
                    convertPDFDoc(file.DirectoryName, file.Name, file.Extension);
                    Log.Info("The following PDF was Converted Successfully:: " + file.Name);
                }
                else if (file.Extension.Equals(".docx"))
                {
                    //     c.convertWordDoc1("","");
                }
                else
                {
                    //do nothing
                }

            }
            try
            {
                var apiClient = new ApiClient();
                DocuSignProps props = new DocuSignProps(apiClient);

                //read
                var Readengine = new FileHelperAsyncEngine<Record>(); //read document goes line by line

                Readengine.BeginReadFile(DSConfig.DsTags);

                using (Readengine)
                {
                    foreach (Record r in Readengine)
                    {
                        string anchor = r.DocusignTag;

                        anchor = anchor.Substring(1, anchor.Length - 3);

                        switch (anchor)
                        {
                            case "s":
                                props.CreateCustomTabsFromCSV("SignHere", "SDKSigner", "SDKSignerToolTip", "/s{r}/");
                                Log.Info("Document Property of Type - SignHere - Was Created.");
                                break;
                            case "i":
                                props.CreateCustomTabsFromCSV("InitialHere", "SDKInitial", "SDKInitalToolTip", "/i{r}/");
                                Log.Info("Document Property of Type - InitialHere - Was Created.");
                                break;
                            case "oi":
                                props.CreateCustomTabsFromCSV("InitialHereOptional", "SDKInitialOptional", "SDKInitialOptionalToolTip", "/oi{r}/");
                                Log.Info("Document Property of Type - InitialHereOptional - Was Created.");
                                break;
                            case "n":
                                props.CreateCustomTabsFromCSV("FullName", "SDKName", "SDKNameToolTip", "/n{r}/");
                                Log.Info("Document Property of Type - FullName - Was Created.");
                                break;
                            case "co":
                                props.CreateCustomTabsFromCSV("Company", "SDKCompany", "SDKCompanyip", "/co{r}/");
                                Log.Info("Document Property of Type - Company - Was Created.");
                                break;
                            case "t":
                                props.CreateCustomTabsFromCSV("Title", "SDKTime", "SDKSignerTimeTip", "/t{r}/");
                                Log.Info("Document Property of Type - Title - Was Created.");
                                break;
                            case "d":
                                props.CreateCustomTabsFromCSV("Date", "SDKDate", "SDKDateToolTip", "//d{r}//");
                                Log.Info("Document Property of Type - Date - Was Created.");
                                break;
                        
                            default:
                                break;
                        }

                        
                    }
                }
            }
            catch (ApiException e)
            {
                Log.Error("\nDocuSign Exception!");

                // Special handling for consent_required
                String message = e.Message;
                if (!String.IsNullOrWhiteSpace(message) && message.Contains("consent_required"))
                {
                    String consent_url = String.Format("\n    {0}/oauth/auth?response_type=code&scope={1}&client_id={2}&redirect_uri={3}",
                        DSConfig.AuthenticationURL, DSConfig.PermissionScopes, DSConfig.ClientID, DSConfig.OAuthRedirectURI);

                    Log.Error("C O N S E N T   R E Q U I R E D");
                    Log.Error("Ask the user who will be impersonated to run the following url: ");
                    Log.Error(consent_url);
                }
                else
                {

                    Log.Error("Error Reponse: {0}", e.ErrorContent);
                }
            }

            catch (DirectoryNotFoundException dirEx)
            {
                Log.Error("Directory not found: " + dirEx.Message);
            }
            catch (IOException IOEx)
            {
                Log.Error("File not found or could not be Moved / Deleted: " + IOEx.Message);
            }
        }
    }
}
