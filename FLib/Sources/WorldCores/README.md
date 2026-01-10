# 为什么造这个轮子? 目标是实现下面的这几点

- 极致高性能的实现方式, unity ecs的做法, 纯c#的实现, 可以方便迁移到linux等任何平台, 每个组件手动分配和管理内存(
  aligned_alloc), 无gc.  
  当然缺点和unity一样必须是unmanaged的struct, 如果需要managed的组件(class/struct)需要额外包裹一层, 会多一次间接寻址.  
  这样能做到极致的性能, 相比目前已有的纯c#实现的ecs库例如[genaray/Arch](https://github.com/genaray/Arch)理论上性能会强的多.
  后续开发完了在测试.
- 完全避免传统ecs在每次修改组件时性能开销很高的问题, 例如以下代码是很多教程的例子.  
  unity新版本新增enable/disable来缓解这个问题, 但依然不算是一个好的解决方案.  
  所以ecs不恰当的使用某些时候可能反而不如以前传统开发的性能高.
  现在完全避免了这种问题, 可以随意动态增删组件.

```
// 这里每次都会改变archetype, 会频繁的copy entity所在的chunk到新的archetype chunk.
if (input) AddComponent<Movement>()
else RemoveComponent<Movement>()
```

- 迎接高性能的同时, 依然能够像传统GameObject/Component那样保持习惯的开发.  
  不需要为了ecs而非要改变传统的编程思维, 甚至可以完全不要System.  
  让ecs只停留在系统底层数据内存管理分配, 而不过多的侵入上层使用.

## 目前正在开发中...

后续可能还有很多修改,包括很多地方也要改为simd操作   
另外如果贵公司也正好需要这方面的开发人员, 非常乐意进入公司全职开发(qcbf@qq.com)(不要太多加班)(期望成都)