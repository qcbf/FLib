# World

基于稀疏集的 ecs 系统, 相当于性能更强的 GameObject/Component, 相似性非常高.  
同时Entity和Component不再是一一对应, 而是Entity可以挂载多个相同的Component.  
相对于基于archetype的标准ecs不需要太多习惯改变, 天然的从传统开发习惯来编写.  
[目前基于这套系统开发的游戏链接](https://www.taptap.cn/app/747653) 上万单位也能保持流畅战斗.

# [WorldCores链接](Sources/WorldCores/README.md)

目前正在开发的新的 ecs 系统, 具体详情点击小标题跳转描述.

# Binary

二进制相关的功能, 类似protobuf的序列化, 二进制读写

# Collections

一些c#没有自带的集合, 还有一些池化的集合, 还有一些数据结构如四叉树这些.

# Compress

压缩裤,主要是基于其他的二次封装

# Debuger

日志相关库

# Encoder

编码相关库

# Event

事件系统

# Map

包含四边形,六边形的tile地图

# Net

网络库兼容底层tcp,udp,kcp,http等协议的上层封装网络库

# Numeric

数学相关内容, 如定点数, 向量等等

# PublicLibraries

第三方库

# Utilities

很多工具

### json5

一个对兼容性很高json库,如 a:{a:abc},a:{"a:abc},a:{a:a"xx"bc},a:{"a":abc} 这些都是合法可解析的json

### Config

根据每行读取时懒解析的配置工具,防止一启动把所有配置文件所有行都解析

### ....
