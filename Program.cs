using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/body-data", async context =>
{
    // Testes no postman só funcionaram ao retirar os espaços e quebra de linha, no formato JSON dessa forma: {"danilo": "teste","parceiro": "daniel"}
    context.Response.Headers.Append("Content-Type", "text/html; charset=utf-8"); // application/json
    using (StreamReader stream = new StreamReader(context.Request.Body))
    {
        var body = await stream.ReadLineAsync();
        var data = JsonConvert.DeserializeObject<dynamic>(body);

        await context.Response.WriteAsync("<h1>Parâmetros no http</h1>");
        await context.Response.WriteAsync($"Parametro danilo = {data.danilo}<br>");
        await context.Response.WriteAsync($"Parametro parceiro = {data.parceiro}<br>");
    }
});

app.MapPost("/form-data", async context =>
{
    // Para o teste no Postman, foi preenchido no body os campos do tipo x-www-form-urlencoded
    // Exibir dados de Cookie na tela web
    context.Response.Headers.Append("Content-Type", "text/html; charset=utf-8");
    var dict = context.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
    string teste = dict["danilo"].ToString();
    string teste2 = dict["parceiro"].ToString(); // context.Request.Form
    await context.Response.WriteAsync("<h1>Parâmetros no http</h1>");
    await context.Response.WriteAsync($"Parametro danilo = {teste}<br>");
    await context.Response.WriteAsync($"Parametro parceiro = {teste2}<br>");
    // await context.Response.WriteAsync("Hello World! " + context.Request.Headers["User-Agent"]);
});

app.MapGet("/query-string", async context =>
{
    // Exibir dados de Cookie na tela web
    context.Response.Headers.Append("Content-Type", "text/html; charset=utf-8");
    string teste = context.Request.Query["danilo"].ToString();
    string teste2 = context.Request.Query["parceiro"].ToString();
    await context.Response.WriteAsync("<h1>Parâmetros no http</h1>");
    await context.Response.WriteAsync($"Parametro danilo = {teste}<br>");
    await context.Response.WriteAsync($"Parametro parceiro = {teste2}<br>");
    // await context.Response.WriteAsync("Hello World! " + context.Request.Headers["User-Agent"]);
});

app.MapGet("/html", async context =>
{
    // Adicionado Cookie
    context.Response.Cookies.Append("CookieTeste", "ValorDoCookieTeste");
    var cookieOptions = new CookieOptions
    { 
        Expires = DateTime.Now.AddDays(1), HttpOnly = true, Secure = true,
    };
    context.Response.Cookies.Append("MeuCookieComOpcoes", "ValorDoCookieComOpcoes", cookieOptions);
    // Exibir dados de Cookie na tela web
    context.Response.Headers.Append("Content-Type", "text/html; charset=utf-8");
    await context.Response.WriteAsync($"Valores do cookie localhost<br>");
    var cookies = context.Request.Cookies;
    foreach (var c in cookies) 
    {
        await context.Response.WriteAsync($"<b>Chave</b>: {c.Key}, <b>Valor</b>: {c.Value}<br>");
    }
    // await context.Response.WriteAsync("Hello World! " + context.Request.Headers["User-Agent"]);
});

app.MapGet("/pdf", async context =>
{
    context.Response.Cookies.Append("CookieTeste", "ValorDoCookieTeste");
    context.Response.Cookies.Append("MeuCookieComOpcoes", "ValorDoCookieComOpcoes");
    context.Response.Headers.Append("Content-Type", "application/pdf; charset=utf-8");
    if (GlobalFontSettings.FontResolver is not MyFontResolver)
    {
        GlobalFontSettings.FontResolver = new MyFontResolver();
    }
    PdfDocument document = new();
    document.Info.Title = "Created with PDFsharp";
    PdfPage page = document.AddPage();
    XGraphics gfx = XGraphics.FromPdfPage(page);
    XFont font = new("Calibri", 20, XFontStyleEx.BoldItalic);
    gfx.DrawString("Hello, World!", font, XBrushes.Black, new XRect(10, 10, page.Width, page.Height), XStringFormats.TopLeft);
    const string filename = "HelloWorld.pdf";
    document.Save(filename);
    string doc = File.ReadAllText(filename);
    await context.Response.WriteAsync(doc);
});

app.MapGet("/csv", async context =>
{
    // Adicionado Cookie
    context.Response.Cookies.Append("CookieTeste", "ValorDoCookieTeste");
    var cookieOptions = new CookieOptions
    { 
        Expires = DateTime.Now.AddDays(1), HttpOnly = true, Secure = true,
    };
    context.Response.Cookies.Append("MeuCookieComOpcoes", "ValorDoCookieComOpcoes", cookieOptions);
    // Exibir dados de Cookie em csv
    context.Response.Headers.Append("Content-Type", "text/csv; charset=utf-8");
    await context.Response.WriteAsync($"Chave; Valor\n");
    string? texto = context.Request.Headers["Cookie"];
    string[] chaveValor = texto != null ? texto.Split(';') : [];
    foreach (var item in chaveValor)
    {
        string[] cv = item != null ? item.Split('=') : [];
        await context.Response.WriteAsync($"{cv[0]};{cv[1]}\n");
    }
});

app.Run();

class MyFontResolver : IFontResolver
{
    public byte[] GetFont(String faceNme)
    {
        return System.IO.File.ReadAllBytes("C:\\Windows\\Fonts\\Calibri.ttf");
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) 
    {
        return new FontResolverInfo("Calibri", isBold, isItalic);
    }
}
