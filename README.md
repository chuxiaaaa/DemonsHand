# 一键通关恶魔手契

## 📝功能

- [x] 一键通关 恶魔手契 小游戏
- [x] 一键领取以往奖励
- [x] 修改 恶魔手契 小游戏内的数值

## 实现原理
1. 一键通关 恶魔手契 小游戏
    - 调用 **LCU API** 读取任务列表，并寻找任务名称开头为 DemonsHand_Auto 的任务，调用 **LCU API** 将任务提交结算
2. 一键领取以往奖励
   - 调用 **LCU API** 读取当前可领取的奖励列表，将可领取的奖励调用 **LCU API** 提交结算
3. 修改 恶魔手契 小游戏内的数值
   - 调用 **LCU API** 修改游戏存档内容 

## 协议
本项目遵循 [MIT](https://github.com/chuxiaaaa/DemonsHand/blob/master/LICENSE.md) 协议
