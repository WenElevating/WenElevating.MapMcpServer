# WenElevating.MapMcpServer
本项目是基于高德地图API实现的MCP服务。

## 如何开始？
1. 将高德地图提供的API Key添加到系统环境变量，并将设置名称为“GaoDeMapKey”，工具需要从环境中获取密钥来请求高德地图的API。**如果你不设置key的话，模型无法调用任何工具!**

3. 添加Mcp服务配置文件，使模型能够识别工具。示例如下：
``` json
{
  "mcpServers": {
    "map": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "项目路径",
        "--no-build"
      ]
    }
  }
}
```

## 支持的高德地图api列表
- 定位IP所在的地理位置
- 获取指定城市的天气资料
- 获取指定位置的地理信息
- 步行路径规划
- 公交路线规划
- 驾车路线规划
- 骑行路线规划
- 距离测量
- 关键词搜索

## 下面是一个使用示例：
<img width="715" height="1435" alt="image" src="https://github.com/user-attachments/assets/0b02b56d-0713-41ec-985f-51fb9a6b24b5" />


## 谢谢
**本项目的成功依赖于以下项目：**
[Modelcontextprotocol](https://github.com/modelcontextprotocol)

[Trace](https://www.trae.ai/)
