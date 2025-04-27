namespace Agent.Models;

public record FileSystemNode(string Name, List<FileSystemNode>? Children=null);

public record DirectoryResponse(string Message, FileSystemNode? Structure=null);
