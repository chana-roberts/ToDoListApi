using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi;



var builder = WebApplication.CreateBuilder(args);

// Added as service
builder.Services.AddSingleton<Service>();

var connectionString=builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options=>
options.UseMySql(connectionString,new MySqlServerVersion(new Version(8,0,22))));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options=>
{
    options.SwaggerDoc("v1",new OpenApiInfo{
        Version="v1",
Title="ToDo Api",
     Description = "An ASP.NET Core Web API for managing ToDo items",
     });

});

builder.Services.AddCors(options=>
{
    options.AddPolicy("CorsPolicy",builder=>
    {
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();

    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
        c.RoutePrefix = string.Empty;
    });



Console.WriteLine("connectionString: "+connectionString);


    app.MapGet("/items", async(ToDoDbContext db) => {
    return await db.Items.ToListAsync();
});

app.MapPost("/addTask", async(ToDoDbContext db,Item item)=> {
      db.Items.Add(item);
     await db.SaveChangesAsync();
 return Results.Created($"/addTask/{item.Id}",item);
});

app.MapDelete("/deleteTask/{id}",async(ToDoDbContext db,int id) =>{
    var item=await db.Items.FindAsync(id);
    if(item==null)
    {
        return Results.NotFound($"Item with id {id} not found.");
    }
    db.Items.Remove(item);
await db.SaveChangesAsync();
return Results.Ok($"Item with id {id} was deleted.");
});

app.MapPut("/updateTask/{id}", async (ToDoDbContext db, int id,bool IsComplete) => {
    // חיפוש המשימה לפי ID
    var item = await db.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound($"Item with id {id} not found.");
    }

    // עדכון הערכים של המשימה
    
    item.IsComplete = IsComplete;

    // שמירה במסד הנתונים
    await db.SaveChangesAsync();

    return Results.Ok($"Item with id {id} was updated.");
});
app.Run();
 
 class Service { }
