# Sync Relay Netcode Demo
## 前置准备
- 使用 Unity 2021.3.5f1c1
- 安装 com.unity.netcode.gameobjects 1.3.1 版本
  - 打开 Unity 编辑器菜单 Window -> Package Manager
  - 在 Package Manager 窗口中，点击 Add -> Add package by name
  - 填写 Name: com.unity.netcode.gameobjects, version: 1.3.1，点击 Add 添加。

## 使用步骤
1. 打开场景 Scenes/SampleScene.unity
2. 选中 Hierarchy 页面中的 [NetworkManager]
3. 在 Inspector 页面的 Relay Transport (Netcode) 处，填写 Room Profile UUID / Transport Type
4. 同时执行两个客户端，第一个选择 Create Host，第二个选择 Join Game 即可 （ 目前Client是默认选择房间列表的第一个加入，如果当前有多个房间处于运行状态，可能会导致Client和Host没有加入到同一个房间，可以在UOS官网关闭不需要的房间 ）
