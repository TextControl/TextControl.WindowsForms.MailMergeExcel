using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tx_mailmerge_excel.Properties;
using TXTextControl.Windows.Forms.Ribbon;
using static TXTextControl.Windows.Forms.ResourceProvider;

namespace tx_mailmerge_excel
{
    public partial class Form1 : TXTextControl.Windows.Forms.Ribbon.RibbonForm
    {
        Report report;

        public Form1()
        {
            InitializeComponent();
        }

        public class Report
        {
            public string Name { get; set; }
        }

        private void MailMerge_IncludeTextMerging(object sender,
            TXTextControl.DocumentServer.MailMerge.IncludeTextMergingEventArgs e)
        {
            // custom handing of XLSX files
            switch (Path.GetExtension(e.IncludeTextField.Filename))
            {
                case ".xlsx": // Excel file detected

                    if (!File.Exists(e.IncludeTextField.Filename))
                        return;

                    // load document into temp. ServerTextControl
                    using (TXTextControl.ServerTextControl tx =
                        new TXTextControl.ServerTextControl())
                    {
                        tx.Create();

                        // Bookmark name is the sheet name of the Excel document
                        TXTextControl.LoadSettings ls = new TXTextControl.LoadSettings()
                        {
                            DocumentPartName = e.IncludeTextField.Bookmark
                        };

                        try
                        {
                            // load the Excel document
                            tx.Load(
                                e.IncludeTextField.Filename, 
                                TXTextControl.StreamType.SpreadsheetML, 
                                ls);
                        }
                        catch {
                            return;
                        }

                        byte[] data;

                        // save the document using the TX format
                        tx.Save(out data, 
                            TXTextControl.BinaryStreamType.InternalUnicodeFormat);

                        e.IncludeTextDocument = data; // pass back to the field
                    }

                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // new ribbon group
            TXTextControl.Windows.Forms.Ribbon.RibbonGroup rgPreview =
                new TXTextControl.Windows.Forms.Ribbon.RibbonGroup();
            rgPreview.SmallIcon = Resources.Preview16;
            rgPreview.LargeIcon = Resources.Preview32;
            rgPreview.Text = "Preview Merge";
            rgPreview.DialogBoxLauncher.Visible = false;
            rgPreview.HorizontalContentAlignment = TXTextControl.HorizontalAlignment.Center;

            // new ribbon button
            RibbonButton rbPreview = new RibbonButton();
            rbPreview.Text = "Preview";
            rbPreview.LargeIcon = Resources.Preview32;
            rbPreview.SmallIcon = Resources.Preview16;
            rbPreview.Click += RbPreview_Click;

            // add button to new gruop
            rgPreview.RibbonItems.Add(rbPreview);
            
            // add group to reporting tab
            ribbonReportingTab1.RibbonGroups.Insert(0, rgPreview);

            // create a sample data source object
            report = new Report() { Name = "Test Report" };
            ribbonReportingTab1.DataSourceManager.LoadSingleObject(report);
        }

        private void RbPreview_Click(object sender, EventArgs e)
        {
            // create a MailMerge instance
            TXTextControl.DocumentServer.MailMerge mailMerge =
                new TXTextControl.DocumentServer.MailMerge()
                {
                    TextComponent = textControl1
                };

            // attach the IncludeTextMerging event
            mailMerge.IncludeTextMerging += MailMerge_IncludeTextMerging;

            // merge template with dummy data
            mailMerge.MergeObject(report);
        }
    }
}
