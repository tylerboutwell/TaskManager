using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TaskDb>(opt => opt.UseInMemoryDatabase("TaskList"));
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var tasks = app.MapGroup("/tasks");

tasks.MapGet("/", async (TaskDb db) =>
    await db.Tasks.ToListAsync());

tasks.MapGet("/complete", async (TaskDb db) =>
    await db.Tasks.Where(t => t.IsComplete).ToListAsync());

tasks.MapGet("/{id}", async (int id, TaskDb db) =>
    await db.Tasks.FindAsync(id)
        is Task task ? Results.Ok(task) : Results.NotFound());

tasks.MapPost("/", async (Task task, TaskDb db) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();

    return Results.Created($"/tasks/{task.Id}", task);
});

tasks.MapPut("/{id}", async (int id, Task inputTask, TaskDb db) =>
{
    var task = await db.Tasks.FindAsync(id);

    if (task is null) return Results.NotFound();

    task.Name = inputTask.Name;
    task.IsComplete = inputTask.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

tasks.MapPatch("/{id}", async (int id, TaskPatchDto inputTask, TaskDb db) =>
{
    var task = await db.Tasks.FindAsync(id);

    if (task is null) return Results.NotFound();

    if (inputTask.Name is not null) task.Name = inputTask.Name;
    if (inputTask.IsComplete is not null) task.IsComplete = inputTask.IsComplete.Value;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

tasks.MapDelete("/{id}", async (int id, TaskDb db) =>
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