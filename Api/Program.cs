using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TaskDb>(opt => opt.UseInMemoryDatabase("TaskList"));
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/tasks", async (TaskDb db) =>
    await db.Tasks.ToListAsync());

app.MapGet("/tasks/complete", async (TaskDb db) =>
    await db.Tasks.Where(t => t.IsComplete).ToListAsync());

app.MapGet("tasks/{id}", async (int id, TaskDb db) =>
    await db.Tasks.FindAsync(id)
        is Task task ? Results.Ok(task) : Results.NotFound());

app.MapPost("tasks/", async (Task task, TaskDb db) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();

    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapPut("tasks/{id}", async (int id, Task inputTask, TaskDb db) =>
{
    var task = await db.Tasks.FindAsync(id);

    if (task is null) return Results.NotFound();

    task.Name = inputTask.Name;
    task.IsComplete = inputTask.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("tasks/{id}", async (int id, TaskDb db) =>
{
    if (await db.Tasks.FindAsync(id) is Task task)
    {
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});


app.Run();