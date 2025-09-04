using System;                  // lets me write stuff to the screen and read input
using System.Collections.Generic; // lets me make lists of things
using System.IO;               // lets me save and read files
using System.Linq;             // lets me sort and search my lists easily
using System.Text.Json;        // lets me turn my list into text and back (JSON format)

// I’m making a storage system for my tasks, saved in "To-Do-List.json"
var repo = new TodoRepository("To-Do-List.json");
repo.Load(); // this tries to load old tasks from the file if it exists

// This is my main loop. It keeps the program running until I quit. 
while (true)
{
    Console.Clear(); // wipe the screen each time so it looks fresh
    Console.WriteLine("ToDo App (v1.0)"); // title
    Console.WriteLine();

    PrintItems(repo.Items); // show all my tasks
    Console.WriteLine();
    Console.WriteLine("[A]dd  [T]oggle  [D]elete  [Q]uit"); // my menu
    Console.Write("> ");
    var key = Console.ReadKey(); // wait for me to press a key
    Console.WriteLine();

    // depending on the key I press, I do different things
    switch (char.ToLowerInvariant(key.KeyChar)) // makes sure both 'A' and 'a' work
    {
        case 'a': // if I pressed A, I want to add a new task
            AddItem(repo);
            break;
        case 't': // if I pressed T, I want to toggle a task done/not done
            ToggleItem(repo);
            break;
        case 'd': // if I pressed D, I want to delete a task
            DeleteItem(repo);
            break;
        case 'q': // if I pressed Q, I want to quit
            repo.Save(); // save my tasks first
            Console.WriteLine("Saved. Bye!");
            return; // exit the program
        default: // if I pressed something else
            Pause("Unknown option."); // tell me it’s not valid
            break;
    }
}

// Helper functions

// this lets me add a new task
void AddItem(TodoRepository repo)
{
    Console.Write("Enter title: ");
    var title = Console.ReadLine(); // type in the task name

    if (string.IsNullOrWhiteSpace(title)) // make sure I didn’t leave it empty
    {
        Pause("Title cannot be empty.");
        return;
    }

    var item = repo.Add(title.Trim()); // actually add the task
    Pause($"Added #{item.Id}: {item.Title}"); // confirm it worked
}

// this lets me mark a task done or undone
void ToggleItem(TodoRepository repo)
{
    Console.Write("Enter id to toggle: ");
    if (!int.TryParse(Console.ReadLine(), out var id)) // check if I typed a number
    {
        Pause("Please enter a valid number.");
        return;
    }

    if (repo.Toggle(id))
        Pause($"Toggled item #{id}."); // confirm if it worked
    else
        Pause($"No item with id {id}."); // tell me if the id was wrong
}

// this lets me delete a task
void DeleteItem(TodoRepository repo)
{
    Console.Write("Enter id to delete: ");
    if (!int.TryParse(Console.ReadLine(), out var id)) // check if I typed a number
    {
        Pause("Please enter a valid number.");
        return;
    }

    if (repo.Delete(id))
        Pause($"Deleted item #{id}."); // confirm delete
    else
        Pause($"No item with id {id}."); // task not found
}

// this prints all my tasks to the screen
void PrintItems(List<TodoItem> items)
{
    if (items.Count == 0) // if I don’t have any tasks yet
    {
        Console.WriteLine("No items yet. Press 'A' to add one.");
        return;
    }

    // show undone tasks first, then done ones, sorted by ID
    foreach (var i in items
        .OrderBy(i => i.IsDone)
        .ThenBy(i => i.Id))
    {
        var check = i.IsDone ? "[x]" : "[ ]"; // checkbox style
        Console.WriteLine($"{i.Id,3} {check} {i.Title}"); 
    }
}

// this just pauses so I can read messages before continuing
void Pause(string message)
{
    Console.WriteLine(message);
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey(true); // wait for a key
}

// Task class

// this describes what a single task looks like
class TodoItem
{
    public int Id { get; set; }           // each task has an ID number
    public string Title { get; set; } = ""; // the name of my task
    public bool IsDone { get; set; }      // true if finished
    public DateTime CreatedAt { get; set; } = DateTime.Now; // when I made it
    public DateTime? CompletedAt { get; set; } // when I finished it (nullable = can be empty)
}

// Task Repository (task storage system)

// this class handles saving and loading tasks
class TodoRepository
{
    private readonly string _path; // where my file lives

    // the list of tasks I have in memory
    public List<TodoItem> Items { get; private set; } = new();

    // when I create this, I give it a file name
    public TodoRepository(string path) => _path = path;

    // load tasks from the file (if it exists)
    public void Load()
    {
        if (!File.Exists(_path)) return; // if no file, do nothing
        var json = File.ReadAllText(_path); // read the file
        if (string.IsNullOrWhiteSpace(json)) return;

        // turn the text back into a list of tasks
        var items = JsonSerializer.Deserialize<List<TodoItem>>(json, JsonOptions);
        Items = items ?? new(); // if nothing found, just use empty list
    }

    // save all tasks into the file
    public void Save()
    {
        var json = JsonSerializer.Serialize(Items, JsonOptionsPretty);
        File.WriteAllText(_path, json); // overwrite file with current list
    }

    // add a new task
    public TodoItem Add(string title)
    {
        var item = new TodoItem
        {
            Id = NextId(), // give it a new unique number
            Title = title
        };
        Items.Add(item);
        Save(); // save immediately
        return item;
    }

    // flip a task between done and not done
    public bool Toggle(int id)
    {
        var item = Items.FirstOrDefault(i => i.Id == id); // find the task
        if (item is null) return false; // if it doesn’t exist, fail

        item.IsDone = !item.IsDone; // flip done status
        item.CompletedAt = item.IsDone ? DateTime.Now : null; // add/remove completion date
        Save();
        return true;
    }

    // delete a task by its ID
    public bool Delete(int id)
    {
        var removed = Items.RemoveAll(i => i.Id == id) > 0; // remove all that match id
        if (removed) Save();
        return removed;
    }

    // find the next free ID number
    private int NextId() => Items.Count == 0 ? 1 : Items.Max(i => i.Id) + 1;

    // options for reading JSON
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // keys look like: myTask
        ReadCommentHandling = JsonCommentHandling.Skip,    // ignore comments in file
        AllowTrailingCommas = true                        // be forgiving with commas
    };

    // options for saving JSON (makes file look pretty)
    private static readonly JsonSerializerOptions JsonOptionsPretty = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true // adds spaces/lines so I can read it easier
    };
}

// run the program with: dotnet run