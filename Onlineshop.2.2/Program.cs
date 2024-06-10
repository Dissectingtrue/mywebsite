using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Productec.Models;
using Shopic.Models;
using System.Security.Claims;
using Userss.Models;




var adminRole = new Role("admin");
var userRole = new Role("user");

var people = new List<Person>
{
  
    new Person("admin@gmail.com", "12345", adminRole),
    new Person("user@gmail.com", "55555", userRole),
};



var builder = WebApplication.CreateBuilder();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/accessdenied";
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod() // ��������� ��� ������
               .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors(builder => builder.AllowAnyOrigin());
app.UseAuthentication();
app.UseAuthorization();   // ���������� middleware ����������� 

app.UseDefaultFiles();
app.UseStaticFiles();

ShopRepository shops = new ShopRepository();



#region �����������
// ������� ��� ������� � �������� ������ �������
app.MapGet("/accessdenied", async (HttpContext context) =>
{
    context.Response.StatusCode = 403;
    context.Response.ContentType = "text/html; charset=utf-8";
    try
    {
        var accessDeniedHtml = await File.ReadAllTextAsync("wwwroot/accessdenied.html");
        await context.Response.WriteAsync(accessDeniedHtml);
    }
    catch (FileNotFoundException)
    {
        await context.Response.WriteAsync("<h1>Access Denied</h1><p>File not found.</p>");
    }
});
// ������� ��� ������� � �������� ������
app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    try
    {
        var loginHtml = await File.ReadAllTextAsync("wwwroot/login.html");
        await context.Response.WriteAsync(loginHtml);
    }
    catch (FileNotFoundException)
    {
        await context.Response.WriteAsync("<h1>Login</h1><p>File not found.</p>");
    }
});
app.MapPost("/login", async (string? returnUrl, HttpContext context) =>
{
    // �������� �� ����� email � ������
    var form = context.Request.Form;
    // ���� email �/��� ������ �� �����������, �������� ��������� ��� ������ 400
    if (!form.ContainsKey("email") || !form.ContainsKey("password"))
        return Results.BadRequest("Email �/��� ������ �� �����������");
    string email = form["email"];
    string password = form["password"];

    // ������� ������������ 
    Person? person = people.FirstOrDefault(p => p.Email == email);
    // ���� ������������ �� ������ ��� ������ �������, ���������� ��������� � ������������ ������
    if (person is null || person.Password != password)
        return Results.Ok("������������ ������");
    // ����������, �� ����� �������� ��������� ������������ ����� �����������
    string redirectUrl = person.Role.Name == "admin" ? "/adshop.html" : "/user.html";

    var claims = new List<Claim>
    {
        new Claim(ClaimsIdentity.DefaultNameClaimType, person.Email),
        new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role.Name)
    };
    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await context.SignInAsync(claimsPrincipal);

    // ���������� �������� �� �������� � ������������ � ����� ������������
    return Results.Redirect(redirectUrl);
});

// ������� ��� ������� � ������ ��������������
app.Map("/admin", [Authorize(Roles = "admin")] () => "Admin Panel");

// ������ ������ ��� ����� admin � user
app.Map("/", [Authorize(Roles = "admin, user")] (HttpContext context) =>
{
    // ���������, ���������������� �� ������������
    if (context.User.Identity.IsAuthenticated)
    {
        // �������� ���� ������������
        var role = context.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType)?.Value;
        // ����������, �� ����� �������� ������������� ������������ � ����������� �� ��� ����
        string redirectUrl = role == "admin" ? "/adshop.html" : "/user.html";
        // �������������� ������������
        context.Response.Redirect(redirectUrl);
        return;
    }
    // ���� ������������ �� ����������������, �������������� ��� �� �������� �����
    context.Response.Redirect("/login");
});

// ������� ��� ������ �� �������
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});
#endregion

// ������� ��� ��������� ���� ���������
app.MapGet("/shops", async (HttpContext context) =>
{
    var response = context.Response;
    response.ContentType = "application/json";
    await response.WriteAsJsonAsync(shops.ReadAll());
});

// ������� ��� ��������� ���� ������� � �������� �� ID ��������
app.MapGet("/ushop/{id:guid}", async (Guid id, HttpContext context) =>
{
    var response = context.Response;
    response.ContentType = "application/json";
    var shop = shops.Read(id);
    if (shop != null)
    {
        await response.WriteAsJsonAsync(shop.ReadAllProduct());
    }
    else
    {
        response.StatusCode = StatusCodes.Status404NotFound;
    }
});

// ������� ��� ������� ������
app.MapPost("/buy/{shopid:guid}/{productid:guid}", async (Guid shopid, Guid productid, HttpContext context) =>
{
    var response = context.Response;
    response.ContentType = "application/json"; // ������������� Content-Type ��� ������

    var shop = shops.Read(shopid);
    if (shop != null)
    {
        var product = shop.ReadProduct(productid);
        if (product != null)
        {
            if (product.Count > 0)
            {
                shop.Buy(productid, 1);
                response.StatusCode = StatusCodes.Status200OK;

                // ���������� ����������� ������ ��������� ����� �������
                await response.WriteAsJsonAsync(shop.ReadAllProduct());
            }
            else
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                await response.WriteAsync("����� ����������");
            }
        }
        else
        {
            response.StatusCode = StatusCodes.Status404NotFound;
            await response.WriteAsync("������� �� ������");
        }
    }
    else
    {
        response.StatusCode = StatusCodes.Status404NotFound;
        await response.WriteAsync("������� �� ������");
    }
});

// ������� ��� ��������� ���� ������� �� ���� ���������
app.MapGet("/allproducts", async (HttpContext context) =>
{
    var response = context.Response;
    response.ContentType = "application/json";
    var allProducts = shops.ReadAll().SelectMany(shop => shop.ReadAllProduct()).ToList();
    await response.WriteAsJsonAsync(allProducts);
});

#region Admin

// ������� ��� �������� ������ ��������
app.MapPost("/adshop", [Authorize(Roles = "admin")] async (HttpContext context) =>
{
    var request = context.Request;
    var response = context.Response;
    var shopDTO = await request.ReadFromJsonAsync<ShopDTO>();
    if (shopDTO != null)
    {
        // ��������, ��� ������� ������������ ����� ����� �� �������� ��������
        var userRole = context.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType)?.Value;
        if (userRole != "admin")
        {
            response.StatusCode = StatusCodes.Status403Forbidden; // ������ ��������
            return;
        }

        // �������� �������� � ����������� � ��������� ��������
        Shop newShop = new Shop(shopDTO.Name, shopDTO.Address, context.User.Identity.Name); // ����������� �������� ������������ ��� ����������� ���������
        shops.Add(newShop);
        await shops.SaveChangesAsync();
        response.StatusCode = StatusCodes.Status201Created;
    }
    else
    {
        response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

// ������� ��� ��������� ������ ���� ��������� ��������������
app.MapGet("/adshop", [Authorize(Roles = "admin")] async (HttpContext context) =>
{
    var response = context.Response;
    response.ContentType = "application/json";

    // �������� �������� ��������������
    var adminEmail = context.User.Identity.Name;

    // �������� ��� ��������, ������������� ��������������
    var adminShops = shops.ReadAll().Where(shop => shop.Owner == adminEmail).ToList();

    // ���������� ������ ��������� � ������� JSON
    await response.WriteAsJsonAsync(adminShops);
});

// ������� ��� ���������� ������ � �������
app.MapPost("/adshop/{id:guid}", [Authorize(Roles = "admin")] async (Guid id, HttpContext context) =>
{
    var request = context.Request;
    var response = context.Response;
    var productDTO = await request.ReadFromJsonAsync<ProductDTO>();
    if (productDTO != null)
    {
        var shop = shops.Read(id);
        if (shop != null)
        {
            shop.AddProduct(new Product(productDTO.Name, productDTO.Price, productDTO.Count, productDTO.Description));
            response.StatusCode = StatusCodes.Status201Created;
        }
        else
        {
            response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
    else
    {
        response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

// ������� ��� �������� ��������
app.MapDelete("/deleteshop/{idshop:guid}", [Authorize(Roles = "admin")] async (Guid idshop, HttpContext context) =>
{
    var response = context.Response;
    var shop = shops.Read(idshop);
    if (shop != null)
    {
        shops.Delete(idshop);
        response.StatusCode = StatusCodes.Status204NoContent;
    }
    else
    {
        response.StatusCode = StatusCodes.Status404NotFound;
    }
});

// ������� ��� �������� ������ �� ��������
app.MapDelete("/adshop/{idshop:guid}/{idprod:guid}", [Authorize(Roles = "admin")] async (Guid idshop, Guid idprod, HttpContext context) =>
{
    var response = context.Response;
    var shop = shops.Read(idshop);
    if (shop != null)
    {
        shop.DeleteProduct(idprod);
        response.StatusCode = StatusCodes.Status204NoContent;
    }
    else
    {
        response.StatusCode = StatusCodes.Status404NotFound;
    }
});

// ������� ��� ��������� ������ � ������
app.MapPut("/adshop/{idshop:guid}/{idprod:guid}", [Authorize(Roles = "admin")] async (Guid idshop, Guid idprod, HttpContext context) =>
{
    var request = context.Request;
    var response = context.Response;
    var productDTO = await request.ReadFromJsonAsync<ProductDTO>();
    if (productDTO != null)
    {
        var shop = shops.Read(idshop);
        if (shop != null)
        {
            using (var productRepository = new ProductRepository(shop.Name))
            {
                var product = productRepository.Read(idprod);
                if (product != null)
                {
                    productRepository.UpdateName(idprod, productDTO.Name);
                    productRepository.UpdatePrice(idprod, productDTO.Price);
                    productRepository.UpdateCount(idprod, productDTO.Count);
                    productRepository.UpdateDescription(idprod, productDTO.Description);
                    await productRepository.SaveChangesAsync();
                    response.StatusCode = StatusCodes.Status200OK;
                }
                else
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                }
            }
        }
        else
        {
            response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
    else
    {
        response.StatusCode = StatusCodes.Status400BadRequest;
    }
});







#endregion




app.Run();