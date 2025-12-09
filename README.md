# Kitchen Chaos（Code Monkey 教程复现 & 注释版）

> A Unity 3D cooking game project built by following Code Monkey’s **Kitchen Chaos** course, with my own annotated source code (Chinese comments) and packaged assets included for portfolio review.

---

## 1. 项目简介 · Overview

**Kitchen Chaos** 是一个类似《Overcooked》的 3D 厨房游戏：  
玩家在有限时间内完成顾客订单，需要在厨房中移动、拿取食材、切菜、烹饪、装盘并交付。

本仓库的用途：

- 作为我在 Unity & C# 学习过程中的一个完整实战项目展示
- 向面试官展示我对工程结构、代码风格、注释习惯以及项目交付流程（包含可执行包）的理解
- 严格尊重原作者 **Code Monkey** 的版权，仓库仅用于学习与作品集展示，不作任何商业用途

---

## 2. 仓库内容 · Repository Contents

本仓库主要包含三类内容：

1. `Build/`  
   - 我打包好的可执行版本（例如 Windows 可执行文件），方便直接运行体验游戏逻辑  
   - 具体平台与运行方式见「使用方式」章节

2. `Source/`（或 Unity 工程根目录）  
   - 按照 Code Monkey 教程完成的 Unity 项目源码
   - 在原有脚本基础上增加了**中文注释**，说明：
     - 核心类的职责与设计思路
     - 关键交互逻辑、事件流转、状态机等
     - 一些我在学习过程中对代码结构的理解和补充

3. `Packages/` 或 `Assets/CodeMonkeyPackages/`  
   - 来自 **Code Monkey** 官方提供的游戏素材 / Unity 包（例如模型、纹理、音频、工具脚本等）
   - 这些资源的版权均归 **Code Monkey** 所有，具体条款以原作者说明为准

> 说明：目录名称可能因我本地工程结构略有差异，实际以仓库中文件夹为准。

---

## 3. 主要特性 · Key Features

游戏玩法与系统大体遵循原 Kitchen Chaos 教程实现：

- 角色控制  
  - 基于新输入系统（Input System）的角色移动与动画驱动
- 厨房交互系统  
  - 多种工作台（容器台、切菜台、炉灶台、盘子台、垃圾桶等）
  - 拿取 / 放下食材、切菜、烹饪、装盘等完整交互链路
- 订单与评分系统  
  - 随机生成订单
  - 按交付速度与正确性进行评分
- UI 与游戏流程  
  - 倒计时、订单列表、进度条、暂停菜单、设置菜单等
- 工程实践  
  - 使用 ScriptableObject 配置食谱与食材
  - 使用事件、接口、状态机等方式组织逻辑
  - 在关键脚本中加入了详细的中文注释，体现我对代码结构和设计模式的理解

---



## 4. 我的工作与学习收获 · My Contributions & Takeaways

在本项目中，我主要做了以下工作：

- 全程跟随 Code Monkey 的 Kitchen Chaos 教程，从空项目开始搭建完整游戏
- 在关键脚本中添加了**详细中文注释**，包括但不限于：
  - 输入与角色控制逻辑
  - 厨房工作台与物体交互系统（`Counter`, `KitchenObject` 等）
  - 订单管理、UI 更新、事件广播等模块
- 对工程结构、代码组织方式进行理解与归纳，作为自己之后写 Unity 项目的参考模板
- 完成项目打包流程，将最终可执行版本纳入仓库，模拟「对外交付」的工作场景

本仓库更多用于展示：

- 我在实际项目中阅读、理解并扩展他人代码的能力
- 我在 Unity / C# 开发中的代码风格和注释习惯
- 我对完整开发流程（从教程到成品打包、到版本管理与作品展示）的理解

---

## 5. 版权与使用说明 · Copyright & Usage

本仓库严格区分「原作者内容」与「我个人内容」：

### 5.1 原作者：Code Monkey

- 教程视频、课程设计、游戏玩法整体框架  
- 原始 3D 模型、纹理、音频等美术与音频资源  
- 教程中提供的原始工程文件和 Unity 包等  

上述内容的版权均归 **Code Monkey** 所有：  

- 原课程名称通常为：**“Learn Unity Beginner/Intermediate 2025 (FREE COMPLETE Course)”**（Kitchen Chaos 项目）  
- 课程及项目文件的获取与使用规范，请以 Code Monkey 在其官网、YouTube 频道、课程页面及相关条款中的说明为准。:contentReference[oaicite:0]{index=0}  

此外，根据 Code Monkey 在公开讨论中的说明，学习者可以在自己完整的终端项目中使用课程中的代码和项目文件（包括商业项目），但**不允许**直接原样打包出售课程的源文件或素材。:contentReference[oaicite:1]{index=1}  
本 README 仅为对该立场的转述，**不构成任何法律意见**。

### 5.2 本人工作成果

- 在原项目基础上添加的**中文注释、说明文档与少量工程配置调整**由我本人创作
- 本仓库整体仅用于：
  - 个人学习记录
  - **面试 / 作品集展示**
- 不以此项目单独进行任何形式的商业销售或收费服务

### 5.3 二次使用说明（给浏览本仓库的读者）

如果你希望：

- 使用 Kitchen Chaos 相关素材、美术资源、课程文件  
- 或在自己的项目里大规模复用本项目的内容  

**推荐做法**：

1. 请直接从 Code Monkey 官方渠道获取课程、项目文件和资源；
2. 仔细阅读并遵守 Code Monkey 的使用条款与许可证；
3. 将本仓库视作一个「学习笔记 / 注释版参考实现」，而非再分发源的官方渠道。

如需引用本仓库中的部分注释或说明文字，请保留适当署名并同时注明原项目来源于 Code Monkey 的 Kitchen Chaos 教程。

---

## 6. 致谢 · Acknowledgements

- **Code Monkey**：提供了完整且高质量的 Kitchen Chaos 教程与项目资源，让我有机会系统性实践 Unity 3D 游戏开发与工程化代码编写。
- 所有查看本仓库的面试官 / 同行：欢迎提出任何建议或改进意见，也欢迎讨论 Unity、C#、项目工程结构等相关话题。


