﻿using System.Diagnostics;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using MvcPdfDemo.Models;
using MvcPdfDemo.Models.NW;

namespace MvcPdfDemo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly NorthwindContext _northwindContext;

    public HomeController(
        ILogger<HomeController> logger,
        NorthwindContext northwindContext)
    {
        _logger = logger;
        _northwindContext = northwindContext;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Report()
    {
        MemoryStream ms = new MemoryStream();

        PdfWriter writer = new PdfWriter(ms);
        PdfDocument pdfDoc = new PdfDocument(writer);
        Document document = new Document(pdfDoc, PageSize.A4, false);
        writer.SetCloseStream(false);

        Paragraph header = new Paragraph("Northwind Products")
          .SetTextAlignment(TextAlignment.CENTER)
          .SetFontSize(20);

        document.Add(header);

        Paragraph subheader = new Paragraph(DateTime.Now.ToShortDateString())
          .SetTextAlignment(TextAlignment.CENTER)
          .SetFontSize(15);
        document.Add(subheader);

        // empty line
        document.Add(new Paragraph(""));

        // Line separator
        LineSeparator ls = new LineSeparator(new SolidLine());
        document.Add(ls);

        // empty line
        document.Add(new Paragraph(""));

        // Add table containing data
        document.Add(GetPdfTable());

        // Page Numbers
        int n = pdfDoc.GetNumberOfPages();
        for (int i = 1; i <= n; i++)
        {
            document.ShowTextAligned(new Paragraph(String
              .Format("Page " + i + " of " + n)),
              559, 806, i, TextAlignment.RIGHT,
              VerticalAlignment.TOP, 0);
        }

        document.Close();
        byte[] byteInfo = ms.ToArray();
        ms.Write(byteInfo, 0, byteInfo.Length);
        ms.Position = 0;

        FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");

        //Uncomment this to return the file as a download
        fileStreamResult.FileDownloadName = "NorthwindProducts.pdf";

        return fileStreamResult;
    }

    private Table GetPdfTable()
    {
        // Table
        Table table = new Table(4, false);

        // Headings
        Cell cellProductId = new Cell(1, 1)
           .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
           .SetTextAlignment(TextAlignment.CENTER)
           .Add(new Paragraph("Product ID"));

        Cell cellProductName = new Cell(1, 1)
           .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
           .SetTextAlignment(TextAlignment.LEFT)
           .Add(new Paragraph("Product Name"));

        Cell cellQuantity = new Cell(1, 1)
           .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
           .SetTextAlignment(TextAlignment.CENTER)
           .Add(new Paragraph("Quantity"));

        Cell cellUnitPrice = new Cell(1, 1)
           .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
           .SetTextAlignment(TextAlignment.CENTER)
           .Add(new Paragraph("Unit Price"));

        table.AddCell(cellProductId);
        table.AddCell(cellProductName);
        table.AddCell(cellQuantity);
        table.AddCell(cellUnitPrice);

        var qry = _northwindContext.Products
            .Select(_ => new
            {
                _.ProductId,
                _.ProductName,
                _.UnitPrice,
                _.UnitsInStock
            });

        foreach (var item in qry)
        {
            Cell cId = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph(item.ProductId.ToString()));

            Cell cName = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.LEFT)
                .Add(new Paragraph(item.ProductName));

            Cell cQty = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.RIGHT)
                .Add(new Paragraph(item.UnitsInStock.ToString()));

            Cell cPrice = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.RIGHT)
                .Add(new Paragraph(String.Format("{0:C2}", item.UnitPrice)));

            table.AddCell(cId);
            table.AddCell(cName);
            table.AddCell(cQty);
            table.AddCell(cPrice);
        }

        return table;
    }
}
