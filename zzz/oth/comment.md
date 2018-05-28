title: "github pages + gitment实现页面评论"

### 说在前面的话 
  + gitment 使用github的oauth app协助认证授权，只允许github登录用户评论 ,评论支持github flavored markdown；
  + 本文旨在记录踩坑史...为像我这样github新手提供点帮助，老手请自觉忽略本文 -.- 
  
### 传送门
+ [github flavored markdown 语法传送门](https://guides.github.com/features/mastering-markdown?_blank)

 
### 实现过程
 + 准备工作 
   1. 新建自己的仓库，设置github pages: 
   * 新建repository的时候注意，命名尽量和自己的github用户名一致,采用 \[username.github.io\]，比如![img](https://jqhgit.github.com/res/zzz/oth/name.png)
   * 创建github pages,点开刚刚新建的工程的 \[Settings\]：
      ![settings](https://jqhgit.github.com/res/zzz/oth/reposetting.png)
   * 然后选择点击 \[Select theme\] 选择一个你喜欢的风格然后继续点击 \[Select theme\],然后你的主页就创建好了
      ![choosetheme](https://jqhgit.github.com/res/zzz/oth/choosetheme.png)
      ![selecttheme](https://jqhgit.github.com/res/zzz/oth/reposelecttheme.png)
      
   2. 注册oauth app： 
   * 点自己头像然后选择 \[Settings\]:
      ![settings2](https://jqhgit.github.com/res/zzz/oth/gitsettings.png)
   * 选择页面左边靠下的 \[Developer settings\]: 
      ![developer settings](https://jqhgit.github.com/res/zzz/oth/developer settings.png)
   * 选择如图的 \[Register a new application\]:
      ![Register new app](https://jqhgit.github.com/res/zzz/oth/registeroauth.png) 
   * 然后填写注册的内容:
      ![Register](https://jqhgit.github.com/res/zzz/oth/registeroauth2.png)
      Application name:随意发挥；
      Homepage URL:填写你当前的repository的路径，注意仓库名和用户名不同的时候可能路径长一些 -.-
      Authorization callback URL这个很关键，在你自己的页面请求授权的时候，oauth app为了安全性会固定回调指定这个页面，
      这个地方经过n次试验一定要填写**仓库根路径**，配合gitment在你多个页面都需要评论时均能够准确授权成功；
      另外这些填错了后面也可以改，很方便，不用太担心 0.0
      成功后会有如图的结果，重要的信息就是Client ID和Client Secret，这是你在gitment请求授权的时候需要配置的。
      ![register finish](https://jqhgit.github.com/res/zzz/oth/oauthapp.png)
      ok！后面可以开始在实际网页中借助gitment配置你需要评论的页面了。
      
   3. 开始配置gitment:
      
      
   
      
   
   


