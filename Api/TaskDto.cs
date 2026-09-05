namespace Api
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsComplete { get; set; }

        public TaskDto() { }
        public TaskDto(Task task) =>
        (Id, Name, IsComplete) = (task.Id, task.Name, task.IsComplete);
    }
}
