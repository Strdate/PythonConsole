namespace PythonConsole
{
    internal sealed class ScriptEditorFile
    {
        public const string DefaultSource = @"# Welcome to the Python Console mod
# Begin with executing the following line

print(""Hello world!!"")

# Press ALT+A to activate the Clipboard tool. With this tool you can inspect properties of
# in-game objects, such as buildings, props, trees, segments etc. Click on any object and
# label 'var: cb' should appear. From now on 'cb' (short for clipboard) is the name of
# python variable that you can access to manipulate with the object using python console.
# Uncomment the following line and execute the script:

# print(cb)

# Information about the selected object should now be printed in the output.
# By holding CTRL you can select multiple objects, which will then be saved in an array.
#
# Now we'll learn how to create a new object in the game. Activate the Clipboard Tool and
# while holding SHIFT click on any point on the map.
# The point should now live in the 'cb' variable.
# Comment back the previous script line 'print(cb)' and uncomment the following:

# my_tree = game.create_tree(cb, ""Conifer"")

# A conifer tree should appear on the selected point. Moreover, you can now manipulate
# it using the my_tree variable. Comment back the previous line and execute the following:

# my_tree.position += Vector(15, 0, 0)

# Every time you execute this line the tree moves 15 units along the game X axis.
# (X and Z axes represent the horizontal position of the object, while Y axis represents
# its height)
# You can also list all available properties and methods of the object:

# help(my_tree)

# Finally you might want to delete the object:

# my_tree.delete()

# You can find more examples how to use this mod (such as building roads) by navigating to
# the 'examples' subfolder in the textfield at the top of this window and hitting reload.
#
# As well as that you can find documentation in the mod description and on the wiki:
#
# https://github.com/Strdate/PythonConsole/wiki
#
#
# Useful commands:
#
# help() - prints basic set of commands
# help(cb) - prints available commands for the given object
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