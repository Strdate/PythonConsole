namespace PythonConsole
{
    internal sealed class ScriptEditorFile
    {
        public const string DefaultSource = @"# Welcome to the Python Console mod
# Begin with executing the following line

print(""Hello world!!"")

# Press ALT+A to activate the Clipboard tool. With this tool you can inspect properties of
# in-game objects, such as Buildings, Props, Trees, Segments etc. Click on any object and
# label 'var: cb' should appear. From now on 'cb' (short for clipboard) is the name of
# python variable, that you can access to manipulate with the object using python console.
# Uncomment the following line and execute the script:

# print(cb)

# Information about the selected object should now be printed in the output.
# By holding CTRL you can select multiple objects, which will then be saved in an array.
#
# Now we'll learn how to create a new object in the game. Activate the Clipboard Tool and
# while holding SHIFT click on any point on the map.
# The point should now live in the 'cb' variable.
# Comment back the previous script line 'print(cb)' and uncomment the following:

# game.create_tree(cb, ""Conifer"")

# A conifer tree should apper on the selected point.
#
# Navigate to the 'examples' subfolder in the textfield above and hit reload
# to see more examples.
#
# Useful commands:
#
# help() - prints basic set of commands
# help(cb) - prints available commands for the given object. Try selecting any object
#   and executing this command
# help_all() - Dumps all documentation in the output (that's a lot of text)
# list_globals() - Prints all variables available in the global scope
#
# Have fun!
#";

        public ScriptEditorFile(string source, string path)
        {
            Source = source ?? string.Empty;
            Path = path ?? string.Empty;
        }

        public void Rename(string path)
        {
            Path = path;
        }

        public string Source { get; set; }

        public string Path { get; private set; }
    }
}