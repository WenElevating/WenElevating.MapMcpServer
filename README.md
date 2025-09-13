# WenElevating.MapMcpServer
This project is an MCP service implemented based on the Gaode Map API.

## How to start?
1. Add the key to the system environment variable
You need to put the Amap key into the system environment variable and set its name to "GaoDeMapKey", because this key is obtained in the tool through "GaoDeMapKey". **If you don't set the key, you won't be able to do anything!**
2. Add the Mcp service configuration file
You need to add an Mcp configuration file to enable the model to recognize the tool. An example is as follows:
```json
{
  "mcpServers": {
    "map": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "project position",
        "--no-build"
      ]
    }
  }
}
```
In fact, the model started the Mcp service through the terminal

## List of supported amap apis
- Locate the geographical location of the IP
- Get the weather information of the designated city
- Obtain the geographic information of the specified location
- Pedestrian path planning
- Bus route planning
- Driving route planning
- Cycling route planning
- Distance measurement
- Keyword search
## The following is an example of use: 
<img width="730" height="1443" alt="image" src="https://github.com/user-attachments/assets/f88766d0-d998-4d00-9477-4d52c3f7620e" />

## Thanks
**The success of this project relies on the following projects:**  
[Modelcontextprotocol](https://github.com/modelcontextprotocol)

[Trace](https://www.trae.ai/)
