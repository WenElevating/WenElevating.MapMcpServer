using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MapMcpServer;

[McpServerToolType]
public static class CommandTools
{
    [McpServerTool, Description("Execute the command in the command terminal")]
    public static async Task<string> ExecuteCommand([Description("The actual cmd command, such as dir")]string command)
    {
        using Process process = new();
        process.StartInfo.FileName = "cmd.exe";
        // 添加chcp 65001命令来设置cmd为UTF8编码，解决中文乱码问题
        process.StartInfo.Arguments = $"/c chcp 65001 && {command}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
        process.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        return $"{output}{error}";
    }    
}