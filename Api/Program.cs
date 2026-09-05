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

tasks.MapGet("/", GetAllTasks);
tasks.MapGet("/complete", GetCompleteTasks);
tasks.MapGet("/{id}", GetTask);
tasks.MapPost("/", CreateTask);
tasks.MapPut("/{id}", UpdateTask);
tasks.MapPatch("/{id}", PatchTask);
tasks.MapDelete("/{id}", DeleteTask);

app.Run();

static async Task<IResult> GetAllTasks(TaskDb db)
{
    return TypedResults.Ok(await db.Tasks.ToArrayAsync());
}

static async Task<IResult> GetCompleteTasks(TaskDb db)
{
    return TypedResults.Ok(await db.Tasks.Where(t => t.IsComplete).ToListAsync());
}

static async Task<IResult> GetTask(int id, TaskDb db)
{
    return await db.Tasks.FindAsync(id)
        is Task task
            ? TypedResults.Ok(task)
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTask(Task task, TaskDb db)
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/tasks/{task.Id}", task);
}

static async Task<IResult> UpdateTask(int id, Task inputTask, TaskDb db)
{
    var task = await db.Tasks.FindAsync(id);

    if (task is null) return TypedResults.NotFound();

    task.Name = inputTask.Name;
    task.IsComplete = inputTask.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> PatchTask(int id, TaskPatchDto inputTask, TaskDb db)
{
    var task = await db.Tasks.FindAsync(id);

    if (task is null) return TypedResults.NotFound();

    if (inputTask.Name is not null) task.Name = inputTask.Name;
    if (inputTask.IsComplete is not null) task.IsComplete = inputTask.IsComplete.Value;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTask(int id, TaskDb db)
{
    if (await db.Tasks.FindAsync(id) is Task task)
    {
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}