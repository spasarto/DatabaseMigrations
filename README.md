# Introduction 
Performs SQL based database migrations, structured similar to the Microsoft.Extensions.* libraries, with full support for async/await.

# Getting Started
To add DatabaseMigrations to your project, simply register it in your service collection:

``` csharp
services.AddDatabaseMigrations(o =>
{ 
    o.WithSqlServerConnection(sp => new SqlConnection("...");
    o.WithEmbeddedScripts(Assembly.GetExecutingAssembly());
}));
```

This will get you database migrator that searches the current assembly for *.sql embedded resources and stores its execution history in a table in your SqlServer database. More on those options later.

Then somewhere in your code, request an instance of ```IDatabaseMigrator``` and call ```ApplyMigrationsAsync```.

``` csharp
public class MyMigrator
{
    private readonly IDatabaseMigrator databaseMigrator;

    public MyMigrator(IDatabaseMigrator databaseMigrator)
    {
        this.databaseMigrator = databaseMigrator;
    }

    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
    {
        await databaseMigrator.ApplyMigrationsAsync(cancellationToken);
    }
}
```

If you have logging enabled, you should see something like this in your logs:

```
info: DatabaseMigrations.DatabaseMigrator[0]
      Checking migration journal
info: DatabaseMigrations.Database.TableMigrationJournal[0]
      Journal table does not exist. Creating now.
info: DatabaseMigrations.DatabaseMigrator[0]
      Processing scripts
warn: DatabaseMigrations.DatabaseMigrator[0]
      No migration scripts were executed!
info: DatabaseMigrations.DatabaseMigrator[0]
      Updating migration journal
```

# Why DatabaseMigrations
This project is very similar to the [DbUp](https://github.com/DbUp/DbUp) project. A few key differences:
* Full async/await support.
* Fully customizable. Dependency Injection based means you can overwrite any aspect of the project.
* Familiar code structure. Based on the Microsoft.Extensions.* projects. If you are already leveraging those packages in your project, this package will seem very familiar in it's setup.
* Nearly provider agnostic. You provide a DbConnection of whatever type. If you are using a table journal, the syntax will need to be tweaked for your provider. Otherwise no provider configuration is needed.

# Advanced Configuration
Below are some examples of the various ways DatabaseMigrations can be configured.

## Script Sources
The easiest way to manage your scripts is to mark them as EmbeddedResources in your project.

```csharp
services.AddDatabaseMigrations(o =>
{
    o.WithEmbeddedScripts(Assembly.GetExecutingAssembly());
});
```

By default, this will look for any *.sql file. You can further limit these results by providing a filter:
```csharp
services.AddDatabaseMigrations(o =>
{
    o.WithEmbeddedScripts(Assembly.GetExecutingAssembly(), resourceName => resourceName.Contains("Migration"));
});
```

Another option is you can specify a custom script provider. You can store your scripts in code, network share, etc.
```csharp
services.AddDatabaseMigrations(o =>
{
    o.WithScriptProvider<MyScriptProvider>();
});
```

## Migration Journals
Migration journals track the execution history of your scripts. One easy way to track the history is to have a table to store the executed scripts. Because the table syntax will be specific to your provide, you can use the "With[Provider]Connection" to configure the table syntax:
``` csharp
services.AddDatabaseMigrations(o =>
{ 
    o.WithSqlServerConnection(sp => new SqlConnection("...");
}));
```

Let's say your provider is not supported out of the box (feel free to submit a PR!). You can customize the provider specific details in the configuration:
``` csharp
services.AddDatabaseMigrations(m =>
{ 
    m.WithTableMigrationJournal(o =>
    {
        o.CreateJournalTableSql = "";
        o.DoesJournalTableExistSql = "";
        o.InsertEntrySql = "";
        o.RetrieveEntriesSql = "";

        // Another way to configure for a provider:
        // o.ForSqlServer();
    });
}));
```

Don't need to track a history? Maybe your scripts are idempotent, and therefore, can be executed every time migrations are ran. Disabling journaling can be done two ways.

Implicitly:
``` csharp
    services.AddDatabaseMigrations(o =>
    {
        // Generic connection that does no provider specific configuration. Disables journaling.
        o.WithConnection(new MyConnection("..."));
    }
```

Explicitly:
``` csharp
    services.AddDatabaseMigrations(o =>
    {
        o.WithNoMigrationJournal();
    });
```

## Script Preprocessors
You can specify code to mutate your script prior to execution. A common scenario is to specify values at runtime and substitute them in your script.

```csharp
services.AddDatabaseMigrations(o =>
{
    // $variable$ can be anything here. 
    o.WithVariableSubstution("$variable$", sp => myValue);
    
    // Call can be chained for multiple variables.
    o.WithVariableSubstution("#variable2#", sp => myValue2);
});
```

Of course, you can specify your pre processor too.

```csharp
services.AddDatabaseMigrations(o =>
{
    o.WithPreprocessor<MyScriptPreprocessor>();
});
```

## Script Execution Order
By default, the scripts are executed alphabetically. It doesn't have to be that way though:
```csharp
services.AddDatabaseMigrations(o =>
{
    // Random order does not seem like a good idea to me, but you do you.
    var random = new Random();
    o.WithMigrationOrder(m => m.OrderBy(x => random.Next()));
});
```

# Build and Test
Clone the code and press build in Visual Studio. Or use dotnet build/test to get started.

# Contribute
Please create a pull request with details on what you are improving.