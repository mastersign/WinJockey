namespace Mastersign.WinJockey;

public class DefaultEditorNotFoundException : Exception
{
    public string EditorExecutable { get; private set; }

    public DefaultEditorNotFoundException(string editorExecutable)
        : base("Could not find the executable file of the default editor")
    {
        EditorExecutable = editorExecutable;
    }
}
