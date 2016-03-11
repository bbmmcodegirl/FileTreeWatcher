
namespace FileTreeWatcher
{
    public enum EntryChange
    {
        NoChange,
        Discovered,
        Dismissed,
        Creation,
        Deletion,
        NameChange,
        Grown,
        Shrunk
    }
}