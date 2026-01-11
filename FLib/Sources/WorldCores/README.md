# 高性能, 简易好用的ECS实现

- 迎接高性能的同时, 依然能够像传统GameObject/Component那样保持习惯的开发.  
  不需要为了ecs而非要改变传统的编程思维, 甚至可以完全不要业务层的System.  
  让ecs只停留在系统底层数据内存管理分配, 而不过多的侵入上层使用.  
  (思考中...)


- 极致高性能的实现方式, unity ecs的做法, 纯c#的实现, 可以方便运行到任何dotnet平台, 每个组件手动分配和管理内存(
  aligned_alloc), 无gc参与.    
  当然缺点和unity一样必须是unmanaged的struct, 如果需要managed的组件(class/struct)需要额外包裹一层, 会多一次间接寻址.  
  这样能做到极致的性能, 所以相比目前已有的纯c#实现的ecs库例如[genaray/Arch](https://github.com/genaray/Arch)
  理论上性能会更强.  
  (后续开发完了在测试)(开发中...)


- 通过增加较低的删除entity时开销, 保持chunk内存数据始终紧凑, 每次chunk的遍历都是完整无跳过的,
  更极致的利用prefetching和cache. 在每次删除entity时不仅仅是在当前chunk swapback, 而是进行跨chunk的swapback.  
  (开发中...)


- 完全避免传统ecs在每次修改组件时性能开销很高的问题, 例如以下代码是很多教程的例子.  
  unity新版本新增enable/disable来缓解这个问题, 但依然不算是一个好的解决方案.  
  unreal的fragmented struct设计不太好应用到managed.  
  所以ecs不恰当的使用某些时候可能反而不如以前传统开发的性能高.  
  现在完全避免了这种问题, 区别于传统ecs将所有组件都放到chunk, 我通过区分静态内存和动态内存区, 在创建entity时将数据都存放正静态区,
  后续的组件都存放到动态内存区, 达到可以随意动态增删组件, 而不用改变到新的archetype.    
  (开发中...)

```
// 这里每次都会改变archetype, 会频繁的copy entity所在的chunk到新的archetype chunk.
if (input) AddComponent<Movement>()
else RemoveComponent<Movement>()
```

- 另外还有一个 [之前开发的基于稀疏集的 ecs 系统](../World/README.md) 相当于性能更强的 GameObject/Component,
  相似性非常高.  
  也支持Entity和Component不再是一一对应, 而是Entity可以挂载多个相同的Component.  
  相对于基于archetype的标准ecs不需要太多习惯改变, 天然的从传统开发习惯来编写.  
  [目前基于这套系统开发的游戏链接](https://www.taptap.cn/app/747653)(视频大概50秒,上万单位也能保持流畅战斗)

## 目前正在开发中...

目前仅有单线程  
后续可能还有很多修改,包括很多地方也要改为simd操作

