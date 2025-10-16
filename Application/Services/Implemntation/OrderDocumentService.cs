
using Application.Services.interfaces;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using System;

namespace Application.Services.Implemntation
{

    public class OrderDocumentService : IOrderDocumentService
    {
        // Updated dimensions to match Python code (72.1mm width, 297mm height)
        // Converting mm to DXA (twentieths of a point): 1mm = 56.7 DXA
        private const int ReceiptWidth = 4088;  // 72.1mm * 56.7 = ~4088 DXA
        private const int PageHeight = 16840;  // 297mm * 56.7 = ~16840 DXA (A4 height)

        private readonly RestaurantDbContext _context;
        public OrderDocumentService(RestaurantDbContext context)
        {
            _context = context;
        }
        public byte[] GenerateOrderDocument(Order order)
        {
            using (var stream = new MemoryStream())
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
                {
                    // Add main document part
                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Set document properties for single page receipt
                    SetReceiptDocumentProperties(mainPart);

                    // Add content in compact format
                    AddCompactOrderHeader(body, order);
                    AddCompactCustomerInfo(body, order);
                    AddCompactOrderItems(body, order);
                    AddCompactOrderSummary(body, order);
                    AddCompactFooter(body, order);

                    // Ensure single page by setting section properties at the end
                    EnsureSinglePageLayout(body);
                }

                return stream.ToArray();
            }
        }

        private void SetReceiptDocumentProperties(MainDocumentPart mainPart)
        {
            // We'll set section properties at the end to ensure they're applied correctly
        }

        private void EnsureSinglePageLayout(Body body)
        {
            var sectionProps = new SectionProperties();

            var pageSize = new PageSize()
            {
                Width = ReceiptWidth,
                Height = PageHeight,
                Orient = PageOrientationValues.Portrait
            };

            // Updated margins to match Python code (4mm left/right, 10mm top/bottom)
            var pageMargins = new PageMargin()
            {
                Top = 567,     // 10mm * 56.7 = ~567 DXA
                Right = 227,   // 4mm * 56.7 = ~227 DXA
                Bottom = 567,  // 10mm * 56.7 = ~567 DXA
                Left = 227,    // 4mm * 56.7 = ~227 DXA
                Header = 0,
                Footer = 0,
                Gutter = 0
            };

            // Prevent page breaks
            sectionProps.Append(pageSize);
            sectionProps.Append(pageMargins);
            sectionProps.Append(new TitlePage() { Val = false });

            body.Append(sectionProps);
        }

        private void AddCompactOrderHeader(Body body, Order order)
        {
            // Order title with smaller font and spacing
            AddCompactCenteredParagraph(body, $"طلب رقم #{order.OrderNumber}", true, "20", 0, 60);

            // Date with smaller font
            AddCompactCenteredParagraph(body, $"التاريخ: {DateTime.Now:yyyy-MM-dd HH:mm}", false, "16", 0, 40);

            // Separator
            AddCompactCenteredParagraph(body, "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━", false, "12", 0, 40);
        }

        private void AddCompactCustomerInfo(Body body, Order order)
        {
            // Customer info header
            AddCompactRightAlignedParagraph(body, "🧾 بيانات العميل", true, "14", 60, 40);

            var table = CreateCompactTable();

            // ✅ FIXED: Use FirstName + LastName
            // ✅ Manual join instead of navigation property
            var user = _context.Users.FirstOrDefault(u => u.Id == order.UserId.ToString().ToLower());

            string customerName = user != null
                ? $"{user.FirstName} {user.LastName}".Trim()
                : "غير محدد";

            AddTableRow(table, "الاسم:", customerName);

            // Phone number
            AddTableRow(table, "رقم الهاتف:", order.PhoneNumber ?? "غير محدد");

            // Check if DeliveryAddress exists
            if (order.DeliveryAddress != null)
            {
                // Combine Street and City safely
                var addressParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(order.DeliveryAddress.Street))
                    addressParts.Add(order.DeliveryAddress.Street);

                if (!string.IsNullOrWhiteSpace(order.DeliveryAddress.City))
                    addressParts.Add(order.DeliveryAddress.City);

                var address = string.Join("، ", addressParts);

                // Add row if address is not empty
                if (!string.IsNullOrWhiteSpace(address))
                {
                    AddTableRow(table, "العنوان:", address);
                }
            }
            else
            {
                AddTableRow(table, "العنوان:", "غير متوفر");
            }

            body.Append(table);
            AddSpacingParagraph(body, 40);
        }

        private void AddCompactOrderItems(Body body, Order order)
        {
            if (order.OrderItems == null || !order.OrderItems.Any())
                return;

            // Order items header
            AddCompactRightAlignedParagraph(body, "📋 تفاصيل الطلب", true, "14", 60, 40);

            var table = CreateCompactTable();

            // Table header with smaller font
            var headerRow = new TableRow();
            headerRow.Append(CreateCompactHeaderCell("الإجمالي", "25%"));
            headerRow.Append(CreateCompactHeaderCell("السعر", "25%"));
            headerRow.Append(CreateCompactHeaderCell("الكمية", "15%"));
            headerRow.Append(CreateCompactHeaderCell("اسم المنتج", "35%"));
            table.Append(headerRow);

            // Items rows with compact formatting
            foreach (var item in order.OrderItems)
            {
                var row = new TableRow();
                row.Append(CreateCompactCell($"{(item.Price * item.Quantity):F2} ج.م", "25%"));
                row.Append(CreateCompactCell($"{item.Price:F2} ج.م", "25%"));
                row.Append(CreateCompactCell(item.Quantity.ToString(), "15%"));
                row.Append(CreateCompactCell(item.ProductName ?? "غير محدد", "35%"));
                table.Append(row);
            }

            body.Append(table);
            AddSpacingParagraph(body, 40);
        }

        private void AddCompactOrderSummary(Body body, Order order)
        {
            var subtotal = order.OrderItems?.Sum(x => x.Price * x.Quantity) ?? 0;
            var deliveryFee = order.DeliveryFee;
            var total = subtotal + deliveryFee;

            // Totals header
            AddCompactRightAlignedParagraph(body, "💰 ملخص الحساب", true, "14", 60, 40);

            var table = CreateCompactSummaryTable();

            // Subtotal
            AddSummaryRow(table, "المجموع الفرعي:", $"{subtotal:F2} ج.م");

            // Delivery fee
            if (deliveryFee > 0)
            {
                AddSummaryRow(table, "رسوم التوصيل:", $"{deliveryFee:F2} ج.م");
            }

            // Payment method - ENHANCED DYNAMIC HANDLING
            var paymentMethodDisplay = GetPaymentMethodDisplayName(order);
            AddSummaryRow(table, "طريقة الدفع:", paymentMethodDisplay);

            // Total row with emphasis
            var totalRow = new TableRow();
            totalRow.Append(CreateCompactCell($"{total:F2} ج.م", "50%", true));
            totalRow.Append(CreateCompactCell("الإجمالي النهائي:", "50%", true));
            table.Append(totalRow);

            body.Append(table);
            AddSpacingParagraph(body, 60);

            // Notes section - ENHANCED
            if (!string.IsNullOrWhiteSpace(order.Notes))
            {
                AddSpacingParagraph(body, 30);
                AddNotesTextBox(body, order.Notes);
            }

            AddSpacingParagraph(body, 60);
        }

        private void AddNotesTextBox(Body body, string notes)
        {
            if (string.IsNullOrWhiteSpace(notes)) return;

            // Notes header with icon
            AddCompactRightAlignedParagraph(body, "📝 ملاحظات الطلب", true, "14", 40, 20);

            // Create a table for the text box effect with better styling
            var notesTable = new Table();
            var tableProperties = new TableProperties();

            // Enhanced border styling
            tableProperties.Append(new TableBorders(
                new TopBorder() { Val = BorderValues.Single, Size = 6, Color = "4A90E2" },
                new BottomBorder() { Val = BorderValues.Single, Size = 6, Color = "4A90E2" },
                new LeftBorder() { Val = BorderValues.Single, Size = 6, Color = "4A90E2" },
                new RightBorder() { Val = BorderValues.Single, Size = 6, Color = "4A90E2" }
            ));

            tableProperties.Append(new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct });
            notesTable.AppendChild(tableProperties);

            // Create table row and cell
            var row = new TableRow();
            var cell = new TableCell();

            // Cell properties with enhanced padding
            var cellProperties = new TableCellProperties();
            cellProperties.Append(new TableCellWidth() { Width = "5000", Type = TableWidthUnitValues.Pct });
            cellProperties.Append(new TableCellMargin(
                new TopMargin() { Width = "150", Type = TableWidthUnitValues.Dxa },
                new BottomMargin() { Width = "150", Type = TableWidthUnitValues.Dxa },
                new LeftMargin() { Width = "150", Type = TableWidthUnitValues.Dxa },
                new RightMargin() { Width = "150", Type = TableWidthUnitValues.Dxa }
            ));

            // Light background for notes
            cellProperties.Append(new Shading() { Fill = "F8F9FA" });
            cell.Append(cellProperties);

            // Add notes text with proper Arabic formatting
            var paragraph = new Paragraph();
            var run = new Run();
            var text = new Text(notes);

            var runProperties = new RunProperties();
            runProperties.Append(new FontSize() { Val = "20" }); // Slightly larger for readability
            runProperties.Append(new FontSizeComplexScript() { Val = "20" });
            runProperties.Append(new RunFonts()
            {
                Ascii = "Arial",
                HighAnsi = "Arial",
                ComplexScript = "Arial",
                EastAsia = "Arial"
            });
            runProperties.Append(new Color() { Val = "2C3E50" });
            runProperties.Append(new Bold());

            run.Append(runProperties);
            run.Append(text);

            // Enhanced paragraph properties for Arabic text
            var paragraphProperties = new ParagraphProperties();
            paragraphProperties.Append(new Justification() { Val = JustificationValues.Right });
            paragraphProperties.Append(new BiDi()); // Enable bidirectional text
            paragraphProperties.Append(new SpacingBetweenLines()
            {
                Line = "360",
                LineRule = LineSpacingRuleValues.Auto
            });

            paragraph.Append(paragraphProperties);
            paragraph.Append(run);
            cell.Append(paragraph);
            row.Append(cell);
            notesTable.Append(row);

            body.Append(notesTable);
            AddSpacingParagraph(body, 40);
        }

        private void AddCompactFooter(Body body, Order order)
        {
            // Status with enhanced styling
            string statusArabic = GetStatusInArabic(order.Status.ToString());
            AddCompactCenteredParagraph(body, $"📊 حالة الطلب: {statusArabic}", false, "12", 40, 40);

            // Thank you message with decorative elements
            AddCompactCenteredParagraph(body, "🙏 شكراً لثقتكم بنا", false, "12", 0, 20);
            AddCompactCenteredParagraph(body, "نتطلع لخدمتكم مرة أخرى", false, "10", 0, 0);
        }

        #region Helper Methods

        private Table CreateCompactTable()
        {
            var table = new Table();

            var tableProps = new TableProperties(
                new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Pct },
                new TableBorders(
                    new TopBorder() { Val = BorderValues.Single, Size = 4, Color = "2C3E50" },
                    new BottomBorder() { Val = BorderValues.Single, Size = 4, Color = "2C3E50" },
                    new LeftBorder() { Val = BorderValues.Single, Size = 4, Color = "2C3E50" },
                    new RightBorder() { Val = BorderValues.Single, Size = 4, Color = "2C3E50" },
                    new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 2, Color = "BDC3C7" },
                    new InsideVerticalBorder() { Val = BorderValues.Single, Size = 2, Color = "BDC3C7" }
                ),
                new TableLayout() { Type = TableLayoutValues.Fixed },
                new TableStyle() { Val = "TableGrid" },
                new TableCellSpacing() { Width = "0" }
            );

            table.AppendChild(tableProps);
            return table;
        }

        private Table CreateCompactSummaryTable()
        {
            var table = new Table();

            var tableProps = new TableProperties(
                new TableWidth() { Width = "70%", Type = TableWidthUnitValues.Pct },
                new TableJustification() { Val = TableRowAlignmentValues.Right },
                new TableBorders(
                    new TopBorder() { Val = BorderValues.Single, Size = 4, Color = "27AE60" },
                    new BottomBorder() { Val = BorderValues.Single, Size = 4, Color = "27AE60" },
                    new LeftBorder() { Val = BorderValues.Single, Size = 4, Color = "27AE60" },
                    new RightBorder() { Val = BorderValues.Single, Size = 4, Color = "27AE60" },
                    new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 2, Color = "A8E6CF" },
                    new InsideVerticalBorder() { Val = BorderValues.Single, Size = 2, Color = "A8E6CF" }
                ),
                new TableLayout() { Type = TableLayoutValues.Fixed },
                new TableCellSpacing() { Width = "0" }
            );

            table.AppendChild(tableProps);
            return table;
        }

        private TableCell CreateCompactCell(string text, string width, bool isBold = false)
        {
            var cell = new TableCell();
            var paragraph = new Paragraph();
            var run = new Run();

            var textElement = new Text(text);
            run.Append(textElement);

            var runProperties = new RunProperties();
            runProperties.Append(new RunFonts()
            {
                Ascii = "Arial",
                HighAnsi = "Arial",
                ComplexScript = "Arial",
                EastAsia = "Arial"
            });
            runProperties.Append(new FontSize() { Val = "18" });
            runProperties.Append(new FontSizeComplexScript() { Val = "18" });

            if (isBold)
            {
                runProperties.Append(new Bold());
                runProperties.Append(new Color() { Val = "2C3E50" });
            }
            else
            {
                runProperties.Append(new Color() { Val = "34495E" });
            }

            run.RunProperties = runProperties;
            paragraph.Append(run);

            // Enhanced RTL support for Arabic text
            var paraProps = new ParagraphProperties();
            paraProps.Append(new Justification() { Val = JustificationValues.Right });
            paraProps.Append(new BiDi()); // Enable bidirectional text
            paraProps.Append(new SpacingBetweenLines()
            {
                Before = "0",
                After = "0",
                Line = "240",
                LineRule = LineSpacingRuleValues.Auto
            });

            paragraph.ParagraphProperties = paraProps;
            cell.Append(paragraph);

            var cellProperties = new TableCellProperties(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = width },
                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center },
                new TableCellMargin(
                    new TopMargin() { Width = "30", Type = TableWidthUnitValues.Dxa },
                    new BottomMargin() { Width = "30", Type = TableWidthUnitValues.Dxa },
                    new LeftMargin() { Width = "60", Type = TableWidthUnitValues.Dxa },
                    new RightMargin() { Width = "60", Type = TableWidthUnitValues.Dxa }
                ),
                new Shading() { Fill = isBold ? "ECF0F1" : "FFFFFF" }
            );

            cell.PrependChild(cellProperties);
            return cell;
        }

        private TableCell CreateCompactHeaderCell(string text, string width)
        {
            return CreateCompactCell(text, width, true);
        }

        private void AddTableRow(Table table, string label, string value)
        {
            var row = new TableRow();
            row.Append(CreateCompactCell(value, "70%"));
            row.Append(CreateCompactCell(label, "30%", true));
            table.Append(row);
        }

        private void AddSummaryRow(Table table, string label, string value)
        {
            var row = new TableRow();
            row.Append(CreateCompactCell(value, "50%"));
            row.Append(CreateCompactCell(label, "50%"));
            table.Append(row);
        }

        private void AddCompactCenteredParagraph(Body body, string text, bool isBold, string fontSize, int spaceBefore, int spaceAfter)
        {
            var para = new Paragraph();
            var run = new Run(new Text(text));

            var runProps = new RunProperties
            {
                Bold = isBold ? new Bold() : null,
                FontSize = new FontSize() { Val = fontSize },
                FontSizeComplexScript = new FontSizeComplexScript() { Val = fontSize },
                RunFonts = new RunFonts()
                {
                    Ascii = "Arial",
                    HighAnsi = "Arial",
                    ComplexScript = "Arial",
                    EastAsia = "Arial"
                },
                Color = new Color() { Val = "2C3E50" }
            };
            run.RunProperties = runProps;
            para.Append(run);

            var paraProps = new ParagraphProperties();
            paraProps.Append(new Justification() { Val = JustificationValues.Center });
            paraProps.Append(new BiDi()); // Enable bidirectional text
            paraProps.Append(new SpacingBetweenLines()
            {
                Before = spaceBefore.ToString(),
                After = spaceAfter.ToString(),
                Line = "240",
                LineRule = LineSpacingRuleValues.Auto
            });

            para.ParagraphProperties = paraProps;
            body.Append(para);
        }

        private void AddCompactRightAlignedParagraph(Body body, string text, bool isBold, string fontSize, int spaceBefore, int spaceAfter)
        {
            var para = new Paragraph();
            var run = new Run(new Text(text));

            var runProps = new RunProperties
            {
                Bold = isBold ? new Bold() : null,
                FontSize = new FontSize() { Val = fontSize },
                FontSizeComplexScript = new FontSizeComplexScript() { Val = fontSize },
                RunFonts = new RunFonts()
                {
                    Ascii = "Arial",
                    HighAnsi = "Arial",
                    ComplexScript = "Arial",
                    EastAsia = "Arial"
                },
                Color = new Color() { Val = "2C3E50" }
            };
            run.RunProperties = runProps;
            para.Append(run);

            var paraProps = new ParagraphProperties();
            paraProps.Append(new Justification() { Val = JustificationValues.Right });
            paraProps.Append(new BiDi()); // Enable bidirectional text
            paraProps.Append(new SpacingBetweenLines()
            {
                Before = spaceBefore.ToString(),
                After = spaceAfter.ToString(),
                Line = "240",
                LineRule = LineSpacingRuleValues.Auto
            });

            para.ParagraphProperties = paraProps;
            body.Append(para);
        }

        private void AddSpacingParagraph(Body body, int spacing)
        {
            var para = new Paragraph();
            var paraProps = new ParagraphProperties(
                new SpacingBetweenLines()
                {
                    Before = "0",
                    After = spacing.ToString(),
                    Line = "240",
                    LineRule = LineSpacingRuleValues.Auto
                }
            );
            para.ParagraphProperties = paraProps;
            body.Append(para);
        }

        private string GetStatusInArabic(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "⏳ في الانتظار",
                "confirmed" => "✅ مؤكد",
                "processing" => "⚙️ قيد التجهيز",
                "preparing" => "👨‍🍳 قيد التحضير",
                "ready" => "📦 جاهز للتسليم",
                "shipped" => "🚚 تم الشحن",
                "delivered" => "✅ تم التسليم",
                "cancelled" => "❌ ملغي",
                "refunded" => "💰 مسترد",
                _ => $"📊 {status}"
            };
        }

        private string GetPaymentMethodDisplayName(Order order)
        {
            if (order == null)
                return "💳 غير محدد";

            try
            {
                var paymentMethod = (Domain.Enums.PaymentMethod)order.PaymentMethodId;
                var arabicName = MapToArabicDisplayName(paymentMethod);
                return $"💳 {arabicName}";
            }
            catch (Exception ex)
            {
                return "💳 غير محدد";
            }
        }

        private string MapToArabicDisplayName(Domain.Enums.PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                Domain.Enums.PaymentMethod.CashOnDelivery => "الدفع عند الاستلام",
                Domain.Enums.PaymentMethod.CreditCard => "بطاقة ائتمان",
                Domain.Enums.PaymentMethod.Wallet => "محفظة إلكترونية",
                _ => "الدفع عند الاستلام"
            };
        }

        #region Try

        private string TranslateToArabic(string englishName)
        {
            return englishName?.ToLower() switch
            {
                "credit card" or "card" => "بطاقة ائتمان",
                "wallet" => "محفظة إلكترونية",
                "cash on delivery" => "الدفع عند الاستلام",
                "paypal" => "باي بال",
                _ => englishName
            };
        }

        private string TranslatePaymentMethodToArabic(string paymentMethod)
        {
            return paymentMethod?.ToLower().Trim() switch
            {
                "cash" or "نقد" or "كاش" => "نقداً عند الاستلام",
                "card" or "credit card" or "debit card" or "بطاقة" => "بطاقة ائتمان/خصم",
                "visa" or "فيزا" => "فيزا",
                "mastercard" or "ماستر كارد" => "ماستر كارد",
                "wallet" or "محفظة" or "محفظة الكترونية" => "محفظة إلكترونية",
                "vodafone cash" or "فودافون كاش" => "فودافون كاش",
                "orange cash" or "اورنج كاش" => "أورنج كاش",
                "etisalat cash" or "اتصالات كاش" => "اتصالات كاش",
                "fawry" or "فوري" => "فوري",
                "paymob" or "بايموب" => "بايموب",
                "manual" or "يدوي" => "دفع يدوي",
                "bank transfer" or "تحويل بنكي" => "تحويل بنكي",
                "instapay" or "انستاباي" => "انستاباي",
                "digital" or "رقمي" => "دفع رقمي",
                "online" or "اونلاين" => "دفع إلكتروني",
                "paypal" or "بايبال" => "بايبال",
                _ => paymentMethod ?? "غير محدد"
            };
        }
        #endregion

        #endregion
    }
}