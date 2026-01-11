- 基于稀疏集的 ecs 系统, 性能低于unity ecs这些基于archetype的实现方式, 高于传统oop的做法.
- 也支持Entity和Component不再是一一对应. 而是Entity可以挂载多个相同的Component. 就相当于性能更强的 GameObject/Component,
  相似性非常高.
- 相对于基于archetype的标准ecs不需要太多习惯改变, 天然的从传统开发习惯来编写.
- 支持网络同步(状态同步和帧同步), 能够快速生成当前整个world状态数据二进制的序列化.
- [目前基于这套系统开发的游戏链接](https://www.taptap.cn/app/747653)(视频大概50秒,上万单位也能保持流畅战斗)
